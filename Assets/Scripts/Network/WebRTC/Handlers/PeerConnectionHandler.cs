using System;
using UnityEngine;
using Unity.WebRTC;
using Network.WebRTC.Core;
using Network.WebRTC.Models;
using Network.WebSocket;
using Network.WebSocket.Models;

namespace Network.WebRTC.Handlers
{
    /// <summary>
    /// Handles WebRTC negotiation (SDP/ICE) with backend
    /// </summary>
    public class PeerConnectionHandler
    {
        private RTCClient rtcClient;
        private WebSocketManager wsManager;
        private WebRTCManager webRTCManager;

        public event Action OnNegotiationComplete;
        public event Action<string> OnNegotiationError;

        private bool isOfferer; // Is Unity initializing the connection?
        private string targetPeer; // Pi user

        public PeerConnectionHandler(RTCClient client, WebSocketManager ws, WebRTCManager manager)
        {
            rtcClient = client;
            wsManager = ws;
            webRTCManager = manager;

            RegisterEvents();
        }

        // Register WebSocket and RTC events
        private void RegisterEvents()
        {
            wsManager.Signaling.OnOfferReceived += HandleOfferReceived;
            wsManager.Signaling.OnAnswerReceived += HandleAnswerReceived;
            wsManager.Signaling.OnICECandidateReceived += HandleICECandidateReceived;

            rtcClient.OnIceCandidate += HandleLocalIceCandidate;
        }

        // Start connection (Unity is offerer)
        public void InitiateConnection(string targetPeerUsername)
        {
            isOfferer = true;
            targetPeer = targetPeerUsername;

            Debug.Log($"[PEER CONNECTION] Initiating connection to {targetPeer}");

            // Create peer connection first
            rtcClient.CreatePeerConnection();

            // Create data channels if available
            if (webRTCManager != null && webRTCManager.DataChannel != null)
            {
                webRTCManager.DataChannel.CreateDataChannels();
            }

            // Create and send offer
            rtcClient.CreateOffer(
                onSuccess: (desc) => 
                {
                    Debug.Log("[PEER CONNECTION] Offer created, sending via WebSocket");
                    wsManager.Signaling.SendOffer(desc.sdp, targetPeer);
                },
                onError: (error) => 
                {
                    Debug.LogError($"[PEER CONNECTION] Failed to create offer: {error}");
                    OnNegotiationError?.Invoke(error);
                }
            );
        }

        // Handle incoming offer (Pi is offerer)
        private void HandleOfferReceived(WebRTCOffer offer)
        {
            isOfferer = false;
            targetPeer = offer.from;

            Debug.Log($"[PEER CONNECTION] Offer received from {targetPeer}");

            rtcClient.CreatePeerConnection();

            var desc = new RTCSessionDescription
            {
                type = RTCSdpType.Offer,
                sdp = offer.sdp
            };

            rtcClient.SetRemoteDescription(desc,
                onSuccess: () => 
                {
                    Debug.Log("[PEER CONNECTION] Remote description set, creating answer");
                    
                    rtcClient.CreateAnswer(
                        onSuccess: (answerDesc) => 
                        {
                            Debug.Log("[PEER CONNECTION] Answer created, sending via WebSocket");
                            wsManager.Signaling.SendAnswer(answerDesc.sdp, targetPeer);
                        },
                        onError: (error) => 
                        {
                            Debug.LogError($"[PEER CONNECTION] Failed to create answer: {error}");
                            OnNegotiationError?.Invoke(error);
                        }
                    );
                },
                onError: (error) => 
                {
                    Debug.LogError($"[PEER CONNECTION] Failed to set remote description: {error}");
                    OnNegotiationError?.Invoke(error);
                }
            );
        }

        // Handle incoming answer (Unity is offerer)
        private void HandleAnswerReceived(WebRTCAnswer answer)
        {
            if (!isOfferer)
            {
                Debug.LogWarning("[PEER CONNECTION] Received answer but we are not the offerer");
                return;
            }

            Debug.Log($"[PEER CONNECTION] Answer received from {answer.from}");

            var desc = new RTCSessionDescription
            {
                type = RTCSdpType.Answer,
                sdp = answer.sdp
            };

            rtcClient.SetRemoteDescription(desc,
                onSuccess: () => 
                {
                    Debug.Log("[PEER CONNECTION] Negotiation complete");
                    OnNegotiationComplete?.Invoke();
                },
                onError: (error) => 
                {
                    Debug.LogError($"[PEER CONNECTION] Failed to set remote description: {error}");
                    OnNegotiationError?.Invoke(error);
                }
            );
        }

        // Send ICE candidate to remote peer
        private void HandleLocalIceCandidate(RTCIceCandidate candidate)
        {
            if (string.IsNullOrEmpty(targetPeer))
            {
                Debug.LogWarning("[PEER CONNECTION] Cannot send ICE candidate, no target peer");
                return;
            }

            Debug.Log($"[PEER CONNECTION] Sending ICE candidate to {targetPeer}");

            var iceCandidatePayload = new IceCandidatePayload
            {
                candidate = candidate.Candidate,
                sdpMid = candidate.SdpMid,
                sdpMLineIndex = candidate.SdpMLineIndex.GetValueOrDefault(0)
            };

            wsManager.Signaling.SendICECandidate(
                iceCandidatePayload.candidate,
                iceCandidatePayload.sdpMid,
                iceCandidatePayload.sdpMLineIndex,
                targetPeer
            );
        }

        // Handle ICE candidate received from remote peer
        private void HandleICECandidateReceived(ICECandidate iceData)
        {
            Debug.Log($"[PEER CONNECTION] ICE candidate received from {iceData.from}");

            var candidate = new RTCIceCandidate(new RTCIceCandidateInit
            {
                candidate = iceData.candidate,
                sdpMid = iceData.sdpMid,
                sdpMLineIndex = iceData.sdpMLineIndex
            });

            rtcClient.AddIceCandidate(candidate);
        }

        // Unregister events
        public void Dispose()
        {
            wsManager.Signaling.OnOfferReceived -= HandleOfferReceived;
            wsManager.Signaling.OnAnswerReceived -= HandleAnswerReceived;
            wsManager.Signaling.OnICECandidateReceived -= HandleICECandidateReceived;

            rtcClient.OnIceCandidate -= HandleLocalIceCandidate;
        }
    }
}
