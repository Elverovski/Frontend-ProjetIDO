using System;
using UnityEngine;
using NativeWebSocket;

namespace Network.WebSocket.Core
{
    /// <summary>
    /// Client WebSocket de base - Gère la connexion au serveur
    /// </summary>
    public class SocketClient : MonoBehaviour
    {
        private static SocketClient _instance;
        public static SocketClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("SocketClient");
                    _instance = go.AddComponent<SocketClient>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        private NativeWebSocket.WebSocket websocket;
        private bool isConnecting = false;
        private int reconnectAttempts = 0;
        private float reconnectTimer = 0f;
        
        private string serverUrl;
        private float reconnectDelay;
        private int maxReconnectAttempts;
        
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnError;
        public event Action<string, string> OnMessage;
        
        public bool IsConnected => websocket?.State == WebSocketState.Open;
        public WebSocketState State => websocket?.State ?? WebSocketState.Closed;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Initialize(string url, float reconnectDelay, int maxAttempts)
        {
            this.serverUrl = url;
            this.reconnectDelay = reconnectDelay;
            this.maxReconnectAttempts = maxAttempts;
            
            Debug.Log($"[CLIENT] Initialized: {serverUrl}");
        }

        public async void Connect()
        {
            if (isConnecting || IsConnected)
            {
                Debug.LogWarning("[CLIENT] Already connected or connecting");
                return;
            }

            isConnecting = true;

            try
            {
                websocket = new NativeWebSocket.WebSocket(serverUrl);

                websocket.OnOpen += () =>
                {
                    Debug.Log($"[CLIENT] Connected to {serverUrl}");
                    isConnecting = false;
                    reconnectAttempts = 0;
                    OnConnected?.Invoke();
                };

                websocket.OnError += (e) =>
                {
                    Debug.LogError($"[CLIENT] Error: {e}");
                    isConnecting = false;
                    OnError?.Invoke(e);
                };

                websocket.OnClose += (e) =>
                {
                    Debug.Log($"[CLIENT] Disconnected: {e}");
                    isConnecting = false;
                    OnDisconnected?.Invoke();
                    
                    if (reconnectAttempts < maxReconnectAttempts)
                    {
                        reconnectTimer = reconnectDelay;
                    }
                };

                websocket.OnMessage += (bytes) =>
                {
                    string message = System.Text.Encoding.UTF8.GetString(bytes);
                    HandleMessage(message);
                };

                await websocket.Connect();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CLIENT] Connection error: {ex.Message}");
                isConnecting = false;
                OnError?.Invoke(ex.Message);
            }
        }

        public void Disconnect()
        {
            reconnectAttempts = maxReconnectAttempts; 
            websocket?.Close();
            Debug.Log("[CLIENT] Disconnect requested");
        }

        public async void SendMessage(string eventName, string jsonData)
        {
            if (!IsConnected)
            {
                Debug.LogWarning($"[CLIENT] Not connected, cannot send: {eventName}");
                return;
            }

            try
            {
                var message = new SocketMessage
                {
                    eventName = eventName,
                    data = jsonData,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                string json = JsonUtility.ToJson(message);
                await websocket.SendText(json);
                
                Debug.Log($"[CLIENT] Sent: {eventName}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CLIENT] Send error: {ex.Message}");
                OnError?.Invoke(ex.Message);
            }
        }

        private void HandleMessage(string message)
        {
            try
            {
                var socketMessage = JsonUtility.FromJson<SocketMessage>(message);
                Debug.Log($"[CLIENT] Received: {socketMessage.eventName}");
                OnMessage?.Invoke(socketMessage.eventName, socketMessage.data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CLIENT] Parse error: {ex.Message}");
            }
        }

        private void Update()
        {
            #if !UNITY_WEBGL || UNITY_EDITOR
            websocket?.DispatchMessageQueue();
            #endif
            
            if (reconnectTimer > 0)
            {
                reconnectTimer -= Time.deltaTime;
                if (reconnectTimer <= 0 && !IsConnected && reconnectAttempts < maxReconnectAttempts)
                {
                    reconnectAttempts++;
                    Debug.Log($"[CLIENT] Reconnection attempt {reconnectAttempts}/{maxReconnectAttempts}");
                    Connect();
                }
            }
        }

        private async void OnApplicationQuit()
        {
            if (websocket != null)
            {
                await websocket.Close();
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                websocket?.Close();
                _instance = null;
            }
        }

        [Serializable]
        private class SocketMessage
        {
            public string eventName;
            public string data;
            public long timestamp;
        }
    }
}