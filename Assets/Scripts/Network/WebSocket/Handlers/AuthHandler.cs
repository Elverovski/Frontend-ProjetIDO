using System;
using UnityEngine;
using Network.WebSocket.Core;
using Network.WebSocket.Models;

namespace Network.WebSocket.Handlers
{
    public class AuthHandler
    {
        private SocketClient socketClient;
        
        // Events for login/logout
        public event Action<LoginResponse> OnLoginSuccess;
        public event Action<string> OnLoginError;
        public event Action OnLogout;

        // Authentication state
        private bool isAuthenticated = false;
        private string currentToken;
        private string currentUsername;

        public bool IsAuthenticated => isAuthenticated;
        public string Token => currentToken;
        public string Username => currentUsername;

        // Constructor
        public AuthHandler(SocketClient client)
        {
            socketClient = client;
            RegisterEvents();
        }

        // Register WebSocket events
        private void RegisterEvents()
        {
            socketClient.OnMessage += HandleMessage;
            socketClient.OnDisconnected += HandleDisconnect;
        }

        // Send login request
        public void Login(string username, string password, string deviceId = null)
        {
            if (string.IsNullOrEmpty(deviceId))
                deviceId = SystemInfo.deviceUniqueIdentifier;

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

        // Send logout request
        public void Logout()
        {
            socketClient.SendMessage(SocketEvents.DISCONNECT, "{}");
            ClearAuth();
            Debug.Log("[AUTH] Logout");
        }

        // Handle incoming messages
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

        // Process successful authentication
        private void HandleAuthSuccess(string jsonData)
        {
            try
            {
                var response = JsonUtility.FromJson<LoginResponse>(jsonData);
                
                if (response.success)
                {
                    isAuthenticated = true;
                    currentToken = response.token;
                    currentUsername = response.userData?.username;
                    
                    Debug.Log($"[AUTH] Authenticated: {currentUsername}");
                    Debug.Log($"[AUTH] Token: {currentToken?.Substring(0, 10)}...");
                    
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
                OnLoginError?.Invoke("Parse error");
            }
        }

        // Process authentication errors
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

        // Handle disconnect event
        private void HandleDisconnect()
        {
            if (isAuthenticated)
                ClearAuth();
        }

        // Clear authentication state
        private void ClearAuth()
        {
            isAuthenticated = false;
            currentToken = null;
            currentUsername = null;
            OnLogout?.Invoke();
            Debug.Log("[AUTH] Session ended");
        }

        // Unregister events
        public void Dispose()
        {
            socketClient.OnMessage -= HandleMessage;
            socketClient.OnDisconnected -= HandleDisconnect;
        }
    }
}
