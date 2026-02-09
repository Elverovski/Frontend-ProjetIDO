using System;
using UnityEngine;
using Network.WebSocket.Core;
using Network.WebSocket.Handlers;
using Network.WebSocket.Models;

namespace Network.WebSocket
{
    public class WebSocketManager : MonoBehaviour
    {
        private static WebSocketManager _instance;
        public static WebSocketManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("WebSocketManager");
                    _instance = go.AddComponent<WebSocketManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Configuration")]
        [SerializeField] private string serverUrl = "ws://localhost:3000";

        private SocketClient socketClient;
        private AuthHandler authHandler;
        private SignalingHandler signalingHandler;

        public AuthHandler Auth => authHandler;
        public SignalingHandler Signaling => signalingHandler;

        public bool IsConnected => socketClient != null && socketClient.IsConnected;
        public bool IsAuthenticated => authHandler != null && authHandler.IsAuthenticated;
        public string Username => authHandler?.Username;

        // Events for connection and authentication
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

        // Setup WebSocket client and handlers
        private void InitializeWebSocket()
        {
            Debug.Log("[WS MANAGER] Initializing...");

            // Create SocketClient object
            GameObject socketGO = new GameObject("SocketClient");
            socketGO.transform.SetParent(transform);
            socketClient = socketGO.AddComponent<SocketClient>();

            // Setup auth and signaling handlers
            authHandler = new AuthHandler(socketClient);
            signalingHandler = new SignalingHandler(socketClient);

            // Register events
            socketClient.OnConnected += HandleConnected;
            socketClient.OnDisconnected += HandleDisconnected;

            authHandler.OnLoginSuccess += HandleLoginSuccess;
            authHandler.OnLoginError += HandleLoginError;
            authHandler.OnLogout += HandleLogout;

            signalingHandler.OnRobotConnected += HandleRobotConnected;
            signalingHandler.OnFrontendConnected += HandleFrontendConnected;

            Debug.Log("[WS MANAGER] Initialized");
        }

        // Connect to server
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

        // Disconnect from server
        public void Disconnect()
        {
            Debug.Log("[WS MANAGER] Disconnecting...");
            if (IsAuthenticated) authHandler.Logout();
            socketClient.Disconnect();
        }

        // Send login request
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

        // Handle connection event
        private void HandleConnected()
        {
            Debug.Log("[WS MANAGER] CONNECTED");
            OnConnected?.Invoke();
        }

        // Handle disconnection event
        private void HandleDisconnected()
        {
            Debug.Log("[WS MANAGER] DISCONNECTED");
            OnDisconnected?.Invoke();
        }

        // Handle successful login
        private void HandleLoginSuccess(LoginResponse response)
        {
            Debug.Log($"[WS MANAGER] AUTHENTICATED: {response.userData.username}");
            Debug.Log($"[WS MANAGER] Token: {response.token.Substring(0, 10)}...");
            OnAuthenticated?.Invoke();
        }

        // Handle login failure
        private void HandleLoginError(string error)
        {
            Debug.LogError($"[WS MANAGER] AUTH FAILED: {error}");
            OnError?.Invoke(error);
        }

        // Handle logout
        private void HandleLogout()
        {
            Debug.Log("[WS MANAGER] Session ended");
        }

        // Handle robot available notification
        private void HandleRobotConnected(string username)
        {
            Debug.Log($"[WS MANAGER] Robot available: {username}");
        }

        // Handle frontend connected notification
        private void HandleFrontendConnected(string username)
        {
            Debug.Log($"[WS MANAGER] Frontend connected: {username}");
        }

        private void OnApplicationQuit()
        {
            Disconnect();
        }
    }
}
