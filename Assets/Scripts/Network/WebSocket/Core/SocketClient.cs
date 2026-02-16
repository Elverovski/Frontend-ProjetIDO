using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;
using Network.WebSocket.Interfaces;

namespace Network.WebSocket.Core
{
    /// <summary>
    /// Low-level WebSocket client for real-time bidirectional communication.
    /// Handles connection, reconnection, message serialization, and thread-safe message dispatching.
    /// Uses NativeWebSocket library for cross-platform support.
    /// </summary>
    public class SocketClient : MonoBehaviour, ISocketClient
    {
        private NativeWebSocket.WebSocket websocket;
        private string serverUrl;
        private bool isConnected;
        private bool isConnecting;

        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string, string> OnMessage;

        // Thread-safe message queue for Unity main thread
        private readonly Queue<Action> messageQueue = new Queue<Action>();

        // Reconnection settings
        private const float ReconnectDelay = 3f;
        private const int MaxReconnectAttempts = 5;
        private int reconnectAttempts;
        private bool shouldReconnect = true;

        public bool IsConnected => isConnected;

        /// <summary>
        /// Message format sent to server.
        /// </summary>
        [Serializable]
        private class SocketMessage
        {
            public string eventName;
            public string data;
            public long timestamp;
        }

        /// <summary>
        /// Message format received from backend server.
        /// </summary>
        [Serializable]
        private class BackendMessage
        {
            public string @event;
            public string data;
            public string eventValue => @event;
        }

        /// <summary>
        /// Connects to the WebSocket server.
        /// Automatically retries on failure.
        /// </summary>
        public async void Connect(string url)
        {
            if (isConnecting || isConnected) return;

            serverUrl = url;
            isConnecting = true;

            websocket = new NativeWebSocket.WebSocket(serverUrl);

            RegisterWebSocketCallbacks();

            try
            {
                await websocket.Connect();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SOCKET] Connection failed: {ex.Message}");
                isConnecting = false;

                if (shouldReconnect && reconnectAttempts < MaxReconnectAttempts)
                    StartCoroutine(ReconnectCoroutine());
            }
        }

        /// <summary>
        /// Subscribes to WebSocket lifecycle events.
        /// Messages are queued for thread-safe processing on Unity main thread.
        /// </summary>
        private void RegisterWebSocketCallbacks()
        {
            // Connection opened successfully
            websocket.OnOpen += () =>
            {
                messageQueue.Enqueue(() =>
                {
                    isConnected = true;
                    isConnecting = false;
                    reconnectAttempts = 0;
                    OnConnected?.Invoke();
                });
            };

            // Error occurred
            websocket.OnError += (e) =>
            {
                messageQueue.Enqueue(() =>
                {
                    Debug.LogError($"[SOCKET] Error: {e}");
                });
            };

            // Connection closed
            websocket.OnClose += (e) =>
            {
                messageQueue.Enqueue(() =>
                {
                    isConnected = false;
                    isConnecting = false;
                    OnDisconnected?.Invoke();

                    if (shouldReconnect && reconnectAttempts < MaxReconnectAttempts)
                        StartCoroutine(ReconnectCoroutine());
                });
            };

            // Message received
            websocket.OnMessage += (bytes) =>
            {
                var message = System.Text.Encoding.UTF8.GetString(bytes);
                messageQueue.Enqueue(() => HandleMessage(message));
            };
        }

        /// <summary>
        /// Waits and attempts reconnection after failure.
        /// </summary>
        private IEnumerator ReconnectCoroutine()
        {
            reconnectAttempts++;
            yield return new WaitForSeconds(ReconnectDelay);
            Connect(serverUrl);
        }

        /// <summary>
        /// Sends a message to the server.
        /// Automatically adds timestamp to outgoing messages.
        /// </summary>
        public void SendMessage(string eventName, string data)
        {
            if (!isConnected) return;

            var message = new SocketMessage
            {
                eventName = eventName,
                data = data,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            string json = JsonUtility.ToJson(message);
            websocket.SendText(json);
        }

        /// <summary>
        /// Parses incoming JSON messages.
        /// Supports both backend format (@event) and standard format (eventName).
        /// </summary>
        private void HandleMessage(string message)
        {
            try
            {
                // Try backend format first
                var backendMsg = JsonUtility.FromJson<BackendMessage>(message);

                if (!string.IsNullOrEmpty(backendMsg.eventValue))
                {
                    OnMessage?.Invoke(backendMsg.eventValue, backendMsg.data ?? "{}");
                    return;
                }

                // Fall back to standard format
                var socketMessage = JsonUtility.FromJson<SocketMessage>(message);

                if (!string.IsNullOrEmpty(socketMessage.eventName))
                {
                    OnMessage?.Invoke(socketMessage.eventName, socketMessage.data ?? "{}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SOCKET] Parse error: {ex.Message}");
            }
        }

        /// <summary>
        /// Processes WebSocket messages and queued events on Unity main thread.
        /// Called every frame.
        /// </summary>
        private void Update()
        {
            // Dispatch WebSocket library's internal message queue
            #if !UNITY_WEBGL || UNITY_EDITOR
            websocket?.DispatchMessageQueue();
            #endif

            // Process our thread-safe message queue
            while (messageQueue.Count > 0)
                messageQueue.Dequeue()?.Invoke();
        }

        /// <summary>
        /// Disconnects from the server and stops reconnection attempts.
        /// </summary>
        public void Disconnect()
        {
            shouldReconnect = false;

            if (websocket != null && websocket.State == WebSocketState.Open)
                websocket.Close();
        }

        private void OnDestroy() => Disconnect();
        private void OnApplicationQuit() => Disconnect();
    }
}