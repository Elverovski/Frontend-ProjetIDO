using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;

namespace Network.WebSocket.Core
{
    public class SocketClient : MonoBehaviour
    {
        private NativeWebSocket.WebSocket websocket;
        private string serverUrl;
        private bool isConnected = false;
        private bool isConnecting = false;

        // Events
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string, string> OnMessage;

        // Queue for thread-safe message handling
        private Queue<Action> messageQueue = new Queue<Action>();

        // Reconnection settings
        private float reconnectDelay = 3f;
        private int maxReconnectAttempts = 5;
        private int reconnectAttempts = 0;
        private bool shouldReconnect = true;

        public bool IsConnected => isConnected;

        // Models for messages
        [Serializable]
        private class SocketMessage
        {
            public string eventName;
            public string data;
            public long timestamp;
        }

        [Serializable]
        private class BackendMessage
        {
            public string @event;
            public string data;
            public string eventValue => @event;
        }

        public async void Connect(string url)
        {
            if (isConnecting || isConnected) return;

            serverUrl = url;
            isConnecting = true;

            websocket = new NativeWebSocket.WebSocket(serverUrl);

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

            websocket.OnError += (e) =>
            {
                messageQueue.Enqueue(() =>
                {
                    Debug.LogError($"Error: {e}");
                });
            };

            websocket.OnClose += (e) =>
            {
                messageQueue.Enqueue(() =>
                {
                    isConnected = false;
                    isConnecting = false;
                    OnDisconnected?.Invoke();

                    if (shouldReconnect && reconnectAttempts < maxReconnectAttempts)
                        StartCoroutine(ReconnectCoroutine());
                });
            };

            websocket.OnMessage += (bytes) =>
            {
                var message = System.Text.Encoding.UTF8.GetString(bytes);
                messageQueue.Enqueue(() => HandleMessage(message));
            };

            try
            {
                await websocket.Connect();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Connection failed: {ex.Message}");
                isConnecting = false;

                if (shouldReconnect && reconnectAttempts < maxReconnectAttempts)
                    StartCoroutine(ReconnectCoroutine());
            }
        }

        private IEnumerator ReconnectCoroutine()
        {
            reconnectAttempts++;
            yield return new WaitForSeconds(reconnectDelay);
            Connect(serverUrl);
        }

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

        private void HandleMessage(string message)
        {
            try
            {
                var backendMsg = JsonUtility.FromJson<BackendMessage>(message);

                if (!string.IsNullOrEmpty(backendMsg.eventValue))
                {
                    OnMessage?.Invoke(backendMsg.eventValue, backendMsg.data ?? "{}");
                    return;
                }

                var socketMessage = JsonUtility.FromJson<SocketMessage>(message);

                if (!string.IsNullOrEmpty(socketMessage.eventName))
                {
                    OnMessage?.Invoke(socketMessage.eventName, socketMessage.data ?? "{}");
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Parse error: {ex.Message}");
            }
        }

        private void Update()
        {
            #if !UNITY_WEBGL || UNITY_EDITOR
            websocket?.DispatchMessageQueue();
            #endif

            while (messageQueue.Count > 0)
                messageQueue.Dequeue()?.Invoke();
        }

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
