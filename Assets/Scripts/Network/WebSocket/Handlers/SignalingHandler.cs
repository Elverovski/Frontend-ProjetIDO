using System;
using UnityEngine;
using Network.WebSocket.Core;
using Network.WebSocket.Models;

namespace Network.WebSocket.Handlers
{
    public class SignalingHandler
    {
        private SocketClient socketClient;

        public event Action<WebRTCOffer> OnOfferReceived;
        public event Action<WebRTCAnswer> OnAnswerReceived;
        public event Action<ICECandidate> OnICECandidateReceived;
        public event Action<string> OnRobotConnected;
        public event Action<string> OnFrontendConnected;

        public SignalingHandler(SocketClient client)
        {
            socketClient = client;
            RegisterEvents();
        }

        private void RegisterEvents()
        {
            socketClient.OnMessage += HandleMessage;
        }

        public void SendOffer(string sdp, string target = null)
        {
            var offer = new WebRTCOffer { sdp = sdp, target = target };
            string json = JsonUtility.ToJson(offer);
            socketClient.SendMessage(SocketEvents.WEBRTC_OFFER, json);
            Debug.Log($"[SIGNALING] Offer sent to {target ?? "broadcast"}");
        }

        public void SendAnswer(string sdp, string target = null)
        {
            var answer = new WebRTCAnswer { sdp = sdp, target = target };
            string json = JsonUtility.ToJson(answer);
            socketClient.SendMessage(SocketEvents.WEBRTC_ANSWER, json);
            Debug.Log($"[SIGNALING] Answer sent to {target ?? "broadcast"}");
        }

        public void SendICECandidate(string candidate, string sdpMid, int sdpMLineIndex, string target = null)
        {
            var iceCandidate = new ICECandidate
            {
                candidate = candidate,
                sdpMid = sdpMid,
                sdpMLineIndex = sdpMLineIndex,
                target = target
            };
            string json = JsonUtility.ToJson(iceCandidate);
            socketClient.SendMessage(SocketEvents.WEBRTC_ICE_CANDIDATE, json);
            Debug.Log($"[SIGNALING] ICE candidate sent to {target ?? "broadcast"}");
        }

        private void HandleMessage(string eventName, string jsonData)
        {
            switch (eventName)
            {
                case SocketEvents.WEBRTC_OFFER:
                    HandleOfferReceived(jsonData);
                    break;
                case SocketEvents.WEBRTC_ANSWER:
                    HandleAnswerReceived(jsonData);
                    break;
                case SocketEvents.WEBRTC_ICE_CANDIDATE:
                    HandleICECandidateReceived(jsonData);
                    break;
                case SocketEvents.ROBOT_CONNECTED:
                    HandleRobotConnected(jsonData);
                    break;
                case SocketEvents.FRONTEND_CONNECTED:
                    HandleFrontendConnected(jsonData);
                    break;
            }
        }

        private void HandleOfferReceived(string jsonData)
        {
            try
            {
                var offer = JsonUtility.FromJson<WebRTCOffer>(jsonData);
                Debug.Log($"[SIGNALING] Offer received from {offer.from}");
                OnOfferReceived?.Invoke(offer);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SIGNALING] Offer parse error: {ex.Message}");
            }
        }

        private void HandleAnswerReceived(string jsonData)
        {
            try
            {
                var answer = JsonUtility.FromJson<WebRTCAnswer>(jsonData);
                Debug.Log($"[SIGNALING] Answer received from {answer.from}");
                OnAnswerReceived?.Invoke(answer);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SIGNALING] Answer parse error: {ex.Message}");
            }
        }

        private void HandleICECandidateReceived(string jsonData)
        {
            try
            {
                var candidate = JsonUtility.FromJson<ICECandidate>(jsonData);
                Debug.Log($"[SIGNALING] ICE candidate received from {candidate.from}");
                OnICECandidateReceived?.Invoke(candidate);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SIGNALING] ICE parse error: {ex.Message}");
            }
        }

        private void HandleRobotConnected(string jsonData)
        {
            try
            {
                var notification = JsonUtility.FromJson<PeerConnectionNotification>(jsonData);
                Debug.Log($"[SIGNALING] Robot connected: {notification.username}");
                OnRobotConnected?.Invoke(notification.username);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SIGNALING] Error: {ex.Message}");
            }
        }

        private void HandleFrontendConnected(string jsonData)
        {
            try
            {
                var notification = JsonUtility.FromJson<PeerConnectionNotification>(jsonData);
                Debug.Log($"[SIGNALING] Frontend connected: {notification.username}");
                OnFrontendConnected?.Invoke(notification.username);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SIGNALING] Error: {ex.Message}");
            }
        }

        public void Dispose()
        {
            socketClient.OnMessage -= HandleMessage;
        }
    }
}