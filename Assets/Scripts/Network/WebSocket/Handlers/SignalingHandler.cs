using System;
using UnityEngine;
using Network.WebSocket.Interfaces;
using Network.WebSocket.Core;
using Network.WebSocket.Models;

namespace Network.WebSocket.Handlers
{
    /// <summary>
    /// Handles WebRTC signaling over WebSocket.
    /// Exchanges SDP offers/answers and ICE candidates between peers.
    /// Used to establish WebRTC peer-to-peer connections.
    /// </summary>
    public class SignalingHandler : ISignalingHandler
    {
        private readonly ISocketClient socketClient;

        public event Action<WebRTCOffer> OnOfferReceived;
        public event Action<WebRTCAnswer> OnAnswerReceived;
        public event Action<ICECandidate> OnICECandidateReceived;
        public event Action<string> OnRobotConnected;
        public event Action<string> OnFrontendConnected;

        public SignalingHandler(ISocketClient client)
        {
            socketClient = client;
            RegisterEvents();
        }

        /// <summary>
        /// Subscribes to socket messages for WebRTC signaling.
        /// </summary>
        private void RegisterEvents()
        {
            socketClient.OnMessage += HandleMessage;
        }

        /// <summary>
        /// Sends WebRTC offer to a peer.
        /// Offer contains SDP describing our media capabilities.
        /// </summary>
        public void SendOffer(string sdp, string target = null)
        {
            var offer = new WebRTCOffer { sdp = sdp, target = target };
            socketClient.SendMessage(SocketEvents.WEBRTC_OFFER, JsonUtility.ToJson(offer));
            Debug.Log($"[SIGNALING] Offer sent to {target ?? "broadcast"}");
        }

        /// <summary>
        /// Sends WebRTC answer to a peer.
        /// Answer responds to received offer with our SDP.
        /// </summary>
        public void SendAnswer(string sdp, string target = null)
        {
            var answer = new WebRTCAnswer { sdp = sdp, target = target };
            socketClient.SendMessage(SocketEvents.WEBRTC_ANSWER, JsonUtility.ToJson(answer));
            Debug.Log($"[SIGNALING] Answer sent to {target ?? "broadcast"}");
        }

        /// <summary>
        /// Sends ICE candidate to a peer.
        /// ICE candidates help establish connection through NAT/firewalls.
        /// </summary>
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

        /// <summary>
        /// Routes incoming signaling messages to appropriate handlers.
        /// </summary>
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
            }
        }

        /// <summary>
        /// Processes received WebRTC offer from peer.
        /// Validates SDP and fires event for WebRTC handler.
        /// </summary>
        private void HandleOfferReceived(string jsonData)
        {
            try
            {
                var offer = JsonUtility.FromJson<WebRTCOffer>(jsonData);

                if (offer == null || string.IsNullOrEmpty(offer.sdp))
                {
                    Debug.LogError("[SIGNALING] Invalid offer payload");
                    return;
                }

                Debug.Log($"[SIGNALING] Offer received from {offer.from}");
                OnOfferReceived?.Invoke(offer);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SIGNALING] Offer parse error: {ex.Message}");
            }
        }

        /// <summary>
        /// Processes received WebRTC answer from peer.
        /// Validates SDP and fires event for WebRTC handler.
        /// </summary>
        private void HandleAnswerReceived(string jsonData)
        {
            try
            {
                var answer = JsonUtility.FromJson<WebRTCAnswer>(jsonData);

                if (answer == null || string.IsNullOrEmpty(answer.sdp))
                {
                    Debug.LogError("[SIGNALING] Invalid answer payload");
                    return;
                }

                Debug.Log($"[SIGNALING] Answer received from {answer.from}");
                OnAnswerReceived?.Invoke(answer);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SIGNALING] Answer parse error: {ex.Message}");
            }
        }

        /// <summary>
        /// Processes received ICE candidate from peer.
        /// Validates candidate and fires event for WebRTC handler.
        /// </summary>
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
            }
        }

        /// <summary>
        /// Handles notification that a robot has connected to the server.
        /// </summary>
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

        /// <summary>
        /// Handles notification that a frontend has connected to the server.
        /// </summary>
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

        /// <summary>
        /// Unsubscribes from socket events.
        /// </summary>
        public void Dispose()
        {
            socketClient.OnMessage -= HandleMessage;
        }
    }
}