using System;
using UnityEngine;
using Network.WebSocket.Interfaces;
using Network.WebSocket.Core;
using Network.WebSocket.Handlers;
using Network.WebSocket.Models;

namespace Network.WebSocket
{
    /// <summary>
    /// Main WebSocket manager - orchestrates all WebSocket operations.
    /// Manages authentication, signaling, and connection lifecycle.
    /// Singleton pattern for global access throughout the application.
    /// </summary>
    public class WebSocketManager : MonoBehaviour, IWebSocketManager
    {
        private static WebSocketManager _instance;
        public static WebSocketManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("WebSocketManager");
                    _instance = go.AddComponent<WebSocketManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Configuration")]
        [SerializeField] private string serverUrl = "ws://localhost:3000";

        private ISocketClient socketClient;
        private IAuthHandler authHandler;
        private ISignalingHandler signalingHandler;

        // Public accessors for handlers
        public IAuthHandler Auth => authHandler;
        public ISignalingHandler Signaling => signalingHandler;

        // Connection state
        public bool IsConnected => socketClient?.IsConnected ?? false;
        public bool IsAuthenticated => authHandler?.IsAuthenticated ?? false;
        public string Username => authHandler?.Username;

        // Events for external subscribers
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action OnAuthenticated;
        public event Action<string> OnError;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeWebSocket();
        }

        /// <summary>
        /// Sets up WebSocket client and handlers.
        /// </summary>
        private void InitializeWebSocket()
        {
            Debug.Log("[WS MANAGER] Initializing...");

            // Create socket client as child GameObject
            var socketGO = new GameObject("SocketClient");
            socketGO.transform.SetParent(transform);
            socketClient = socketGO.AddComponent<SocketClient>();

            // Create handlers
            authHandler = new AuthHandler(socketClient);
            signalingHandler = new SignalingHandler(socketClient);

            RegisterEvents();

            Debug.Log("[WS MANAGER] Initialized");
        }

        /// <summary>
        /// Subscribes to events from socket client and handlers.
        /// </summary>
        private void RegisterEvents()
        {
            // Socket connection events
            socketClient.OnConnected += HandleConnected;
            socketClient.OnDisconnected += HandleDisconnected;

            // Authentication events
            authHandler.OnLoginSuccess += HandleLoginSuccess;
            authHandler.OnLoginError += HandleLoginError;
            authHandler.OnLogout += HandleLogout;

            // Signaling notification events
            signalingHandler.OnRobotConnected += HandleRobotConnected;
            signalingHandler.OnFrontendConnected += HandleFrontendConnected;
        }

        /// <summary>
        /// Connects to the WebSocket server.
        /// </summary>
        public void Connect()
        {
            if (IsConnected)
            {
                Debug.LogWarning("[WS MANAGER] Already connected");
                return;
            }

            Debug.Log($"[WS MANAGER] Connecting to {serverUrl}...");
            socketClient.Connect(serverUrl);
        }

        /// <summary>
        /// Disconnects from the WebSocket server.
        /// Logs out if authenticated.
        /// </summary>
        public void Disconnect()
        {
            Debug.Log("[WS MANAGER] Disconnecting...");
            if (IsAuthenticated) authHandler.Logout();
            socketClient.Disconnect();
        }

        /// <summary>
        /// Attempts to login with credentials.
        /// Requires active WebSocket connection.
        /// </summary>
        public void Login(string username, string password)
        {
            if (!IsConnected)
            {
                Debug.LogError("[WS MANAGER] Not connected");
                return;
            }

            Debug.Log($"[WS MANAGER] Login: {username}");
            authHandler.Login(username, password);
        }

        #region Event Handlers

        private void HandleConnected()
        {
            Debug.Log("[WS MANAGER] CONNECTED");
            OnConnected?.Invoke();
        }

        private void HandleDisconnected()
        {
            Debug.Log("[WS MANAGER] DISCONNECTED");
            OnDisconnected?.Invoke();
        }

        private void HandleLoginSuccess(LoginResponse response)
        {
            Debug.Log($"[WS MANAGER] AUTHENTICATED: {response.userData.username}");
            OnAuthenticated?.Invoke();
        }

        private void HandleLoginError(string error)
        {
            Debug.LogError($"[WS MANAGER] AUTH FAILED: {error}");
            OnError?.Invoke(error);
        }

        private void HandleLogout()
        {
            Debug.Log("[WS MANAGER] Session ended");
        }

        private void HandleRobotConnected(string username)
        {
            Debug.Log($"[WS MANAGER] Robot available: {username}");
        }

        private void HandleFrontendConnected(string username)
        {
            Debug.Log($"[WS MANAGER] Frontend connected: {username}");
        }

        #endregion

        private void OnApplicationQuit()
        {
            Disconnect();
        }
    }
}