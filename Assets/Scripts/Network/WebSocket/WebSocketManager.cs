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
        [SerializeField] private float reconnectDelay = 3f;
        [SerializeField] private int maxReconnectAttempts = 5;

        private SocketClient socketClient;
        private AuthHandler authHandler;
        private Handlers.SignalingHandler signalingHandler;

        public AuthHandler Auth => authHandler;
        public Handlers.SignalingHandler Signaling => signalingHandler;  

        public bool IsConnected => socketClient?.IsConnected ?? false;
        public bool IsAuthenticated => authHandler?.IsAuthenticated ?? false;
        public string Username => authHandler?.Username;

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

        private void InitializeWebSocket()
        {
            Debug.Log("[WS MANAGER] Initializing...");

            socketClient = SocketClient.Instance;
            socketClient.Initialize(serverUrl, reconnectDelay, maxReconnectAttempts);

            authHandler = new AuthHandler(socketClient);
            signalingHandler = new Handlers.SignalingHandler(socketClient);

            socketClient.OnConnected += HandleConnected;
            socketClient.OnDisconnected += HandleDisconnected;
            socketClient.OnError += HandleError;

            authHandler.OnLoginSuccess += HandleLoginSuccess;
            authHandler.OnLoginError += HandleLoginError;
            authHandler.OnLogout += HandleLogout;

            signalingHandler.OnRobotConnected += HandleRobotConnected;
            signalingHandler.OnFrontendConnected += HandleFrontendConnected;

            Debug.Log("[WS MANAGER] Initialized");
        }

        public void Connect()
        {
            if (IsConnected)
            {
                Debug.LogWarning("[WS MANAGER] Already connected");
                return;
            }
            Debug.Log($"[WS MANAGER] Connecting to {serverUrl}...");
            socketClient.Connect();
        }

        public void Disconnect()
        {
            Debug.Log("[WS MANAGER] Disconnecting...");
            if (IsAuthenticated) authHandler.Logout();
            socketClient.Disconnect();
        }

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

        public void SetServerUrl(string url)
        {
            if (IsConnected)
            {
                Debug.LogWarning("[WS MANAGER] Disconnect first");
                return;
            }
            serverUrl = url;
            socketClient.Initialize(serverUrl, reconnectDelay, maxReconnectAttempts);
            Debug.Log($"[WS MANAGER] URL configured: {serverUrl}");
        }

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

        private void HandleError(string error)
        {
            Debug.LogError($"[WS MANAGER] ERROR: {error}");
            OnError?.Invoke(error);
        }

        private void HandleLoginSuccess(LoginResponse response)
        {
            Debug.Log($"[WS MANAGER] AUTHENTICATED: {response.userData.username}");
            Debug.Log($"[WS MANAGER] Token: {response.token.Substring(0, 10)}...");
            OnAuthenticated?.Invoke();
        }

        private void HandleLoginError(string error)
        {
            Debug.LogError($"[WS MANAGER] AUTH FAILED: {error}");
            OnError?.Invoke($"Login failed: {error}");
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

        private void OnDestroy()
        {
            if (socketClient != null)
            {
                socketClient.OnConnected -= HandleConnected;
                socketClient.OnDisconnected -= HandleDisconnected;
                socketClient.OnError -= HandleError;
            }

            authHandler?.Dispose();
            signalingHandler?.Dispose();

            if (_instance == this) _instance = null;
        }

        private void OnApplicationQuit()
        {
            Disconnect();
        }
    }
}