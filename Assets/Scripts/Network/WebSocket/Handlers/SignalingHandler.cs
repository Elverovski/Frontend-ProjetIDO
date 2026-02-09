using System;
using UnityEngine;
using Network.WebSocket.Core;
using Network.WebSocket.Models;

namespace Network.WebSocket.Handlers
{
    public class SignalingHandler
    {
        private SocketClient socketClient;

        // Events for WebRTC signaling and peer connection
        public event Action<WebRTCOffer> OnOfferReceived;
        public event Action<WebRTCAnswer> OnAnswerReceived;
        public event Action<ICECandidate> OnICECandidateReceived;
        public event Action<string> OnRobotConnected;
        public event Action<string> OnFrontendConnected;

        // Constructor
        public SignalingHandler(SocketClient client)
        {
            socketClient = client;
            RegisterEvents();
        }

        // Register WebSocket events
        private void RegisterEvents()
        {
            socketClient.OnMessage += HandleMessage;
        }

        /* ===================== SEND ===================== */

        // Send WebRTC offer
        public void SendOffer(string sdp, string target = null)
        {
            var offer = new WebRTCOffer { sdp = sdp, target = target };
            socketClient.SendMessage(SocketEvents.WEBRTC_OFFER, JsonUtility.ToJson(offer));
            Debug.Log($"[SIGNALING] Offer sent to {target ?? "broadcast"}");
        }

        // Send WebRTC answer
        public void SendAnswer(string sdp, string target = null)
        {
            var answer = new WebRTCAnswer { sdp = sdp, target = target };
            socketClient.SendMessage(SocketEvents.WEBRTC_ANSWER, JsonUtility.ToJson(answer));
            Debug.Log($"[SIGNALING] Answer sent to {target ?? "broadcast"}");
        }

        // Send ICE candidate
        public void SendICECandidate(string candidate, string sdpMid, int sdpMLineIndex, string target = null)
        {
            var ice = new ICECandidate
            {
                candidate = candidate,
                sdpMid = sdpMid,
                sdpMLineIndex = sdpMLineIndex,
                target = target
            };

            socketClient.SendMessage(SocketEvents.WEBRTC_ICE_CANDIDATE, JsonUtility.ToJson(ice));
            Debug.Log($"[SIGNALING] ICE candidate sent to {target ?? "broadcast"}");
        }

        /* ===================== RECEIVE ===================== */

        // Handle incoming messages
        private void HandleMessage(string eventName, string jsonData)
        {
            if (string.IsNullOrEmpty(jsonData))
            {
                Debug.LogWarning($"[SIGNALING] Empty payload for event {eventName}");
                return;
            }

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

                default:
                    Debug.LogWarning($"[SIGNALING] Unknown event: {eventName}");
                    break;
            }
        }

        /* ===================== HANDLERS ===================== */

        // Process received offer
        private void HandleOfferReceived(string jsonData)
        {
            try
            {
                var offer = JsonUtility.FromJson<WebRTCOffer>(jsonData);

                if (offer == null || string.IsNullOrEmpty(offer.sdp))
                {
                    Debug.LogError("[SIGNALING] Invalid offer payload");
                    Debug.LogError($"[SIGNALING] Raw data: {jsonData}");
                    return;
                }

                Debug.Log($"[SIGNALING] Offer received from {offer.from}");
                Debug.Log($"[SIGNALING] Offer SDP length: {offer.sdp.Length}");

                OnOfferReceived?.Invoke(offer);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SIGNALING] Offer parse error: {ex.Message}");
                Debug.LogError($"[SIGNALING] Raw data: {jsonData}");
            }
        }

        // Process received answer
        private void HandleAnswerReceived(string jsonData)
        {
            try
            {
                var answer = JsonUtility.FromJson<WebRTCAnswer>(jsonData);

                if (answer == null || string.IsNullOrEmpty(answer.sdp))
                {
                    Debug.LogError("[SIGNALING] Answer received but SDP is empty");
                    Debug.LogError($"[SIGNALING] Raw data: {jsonData}");
                    return;
                }

                Debug.Log($"[SIGNALING] Answer received from {answer.from}");
                Debug.Log($"[SIGNALING] Answer SDP length: {answer.sdp.Length}");

                OnAnswerReceived?.Invoke(answer);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SIGNALING] Answer parse error: {ex.Message}");
                Debug.LogError($"[SIGNALING] Raw data: {jsonData}");
            }
        }

        // Process received ICE candidate
        private void HandleICECandidateReceived(string jsonData)
        {
            try
            {
                var candidate = JsonUtility.FromJson<ICECandidate>(jsonData);

                if (candidate == null || string.IsNullOrEmpty(candidate.candidate))
                {
                    Debug.LogWarning("[SIGNALING] Empty ICE candidate received");
                    return;
                }

                Debug.Log($"[SIGNALING] ICE candidate received from {candidate.from}");
                OnICECandidateReceived?.Invoke(candidate);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SIGNALING] ICE parse error: {ex.Message}");
                Debug.LogError($"[SIGNALING] Raw data: {jsonData}");
            }
        }

        // Process robot connected notification
        private void HandleRobotConnected(string jsonData)
        {
            try
            {
                var notif = JsonUtility.FromJson<PeerConnectionNotification>(jsonData);
                Debug.Log($"[SIGNALING] Robot connected: {notif.username}");
                OnRobotConnected?.Invoke(notif.username);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SIGNALING] Robot notification error: {ex.Message}");
            }
        }

        // Process frontend connected notification
        private void HandleFrontendConnected(string jsonData)
        {
            try
            {
                var notif = JsonUtility.FromJson<PeerConnectionNotification>(jsonData);
                Debug.Log($"[SIGNALING] Frontend connected: {notif.username}");
                OnFrontendConnected?.Invoke(notif.username);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SIGNALING] Frontend notification error: {ex.Message}");
            }
        }

        // Unregister events
        public void Dispose()
        {
            socketClient.OnMessage -= HandleMessage;
        }
    }
}
