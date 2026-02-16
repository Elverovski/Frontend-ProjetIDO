using System;
using UnityEngine;
using Network.WebSocket.Interfaces;
using Network.WebSocket.Core;
using Network.WebSocket.Models;

namespace Network.WebSocket.Handlers
{
    /// <summary>
    /// Handles user authentication via WebSocket.
    /// Manages login, logout, session tokens, and authentication state.
    /// </summary>
    public class AuthHandler : IAuthHandler
    {
        private readonly ISocketClient socketClient;
        
        public event Action<LoginResponse> OnLoginSuccess;
        public event Action<string> OnLoginError;
        public event Action OnLogout;

        private bool isAuthenticated;
        private string currentToken;
        private string currentUsername;

        public bool IsAuthenticated => isAuthenticated;
        public string Token => currentToken;
        public string Username => currentUsername;

        public AuthHandler(ISocketClient client)
        {
            socketClient = client;
            RegisterEvents();
        }

        /// <summary>
        /// Subscribes to socket events for authentication messages.
        /// </summary>
        private void RegisterEvents()
        {
            socketClient.OnMessage += HandleMessage;
            socketClient.OnDisconnected += HandleDisconnect;
        }

        /// <summary>
        /// Sends login credentials to the server.
        /// Uses device ID for session tracking.
        /// </summary>
        public void Login(string username, string password, string deviceId = null)
        {
            deviceId ??= SystemInfo.deviceUniqueIdentifier;

            var loginRequest = new LoginRequest
            {
                username = username,
                password = password,
                deviceId = deviceId
            };

            string json = JsonUtility.ToJson(loginRequest);
            socketClient.SendMessage(SocketEvents.AUTHENTICATE, json);
            
            Debug.Log($"[AUTH] Login attempt: {username}");
        }

        /// <summary>
        /// Logs out and clears authentication state.
        /// </summary>
        public void Logout()
        {
            socketClient.SendMessage(SocketEvents.DISCONNECT, "{}");
            ClearAuth();
            Debug.Log("[AUTH] Logout");
        }

        /// <summary>
        /// Routes incoming messages to appropriate handlers.
        /// </summary>
        private void HandleMessage(string eventName, string jsonData)
        {
            switch (eventName)
            {
                case SocketEvents.AUTH_SUCCESS:
                    HandleAuthSuccess(jsonData);
                    break;
                    
                case SocketEvents.AUTH_ERROR:
                    HandleAuthError(jsonData);
                    break;
            }
        }

        /// <summary>
        /// Processes successful authentication response.
        /// Stores token and user data.
        /// </summary>
        private void HandleAuthSuccess(string jsonData)
        {
            Debug.Log($"[AUTH] Raw JSON received: {jsonData}");
    
            try
            {
                var response = JsonUtility.FromJson<LoginResponse>(jsonData);
        
                Debug.Log($"[AUTH] Parsed - status: {response.status}, token: {response.token != null}, userData: {response.userData != null}");
        
                if (response.status)  
                {
                    isAuthenticated = true;
                    currentToken = response.token;
                    currentUsername = response.userData?.username;
            
                    Debug.Log($"[AUTH] Authenticated: {currentUsername}");
                    Debug.Log($"[AUTH] Token: {currentToken?.Substring(0, Math.Min(10, currentToken?.Length ?? 0))}...");
            
                    OnLoginSuccess?.Invoke(response);
                }
                else
                {
                    Debug.LogWarning($"[AUTH] Login failed: {response.message}");
                    OnLoginError?.Invoke(response.message);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AUTH] Parse error: {ex.Message}");
                Debug.LogError($"[AUTH] Stack trace: {ex.StackTrace}");
                OnLoginError?.Invoke("Parse error");
            }
        }

        /// <summary>
        /// Processes authentication error response.
        /// </summary>
        private void HandleAuthError(string jsonData)
        {
            try
            {
                var errorResponse = JsonUtility.FromJson<ErrorResponse>(jsonData);
                Debug.LogError($"[AUTH] Error: {errorResponse.message}");
                OnLoginError?.Invoke(errorResponse.message);
            }
            catch
            {
                OnLoginError?.Invoke("Authentication error");
            }
        }

        /// <summary>
        /// Handles disconnection event.
        /// Clears authentication if user was logged in.
        /// </summary>
        private void HandleDisconnect()
        {
            if (isAuthenticated)
                ClearAuth();
        }

        /// <summary>
        /// Resets authentication state and credentials.
        /// </summary>
        private void ClearAuth()
        {
            isAuthenticated = false;
            currentToken = null;
            currentUsername = null;
            OnLogout?.Invoke();
            Debug.Log("[AUTH] Session ended");
        }

        /// <summary>
        /// Unsubscribes from events.
        /// </summary>
        public void Dispose()
        {
            socketClient.OnMessage -= HandleMessage;
            socketClient.OnDisconnected -= HandleDisconnect;
        }
    }
}