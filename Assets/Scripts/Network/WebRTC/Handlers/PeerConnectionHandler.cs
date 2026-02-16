using System;
using UnityEngine;
using Unity.WebRTC;
using Network.WebRTC.Interfaces;
using Network.WebRTC.Models;
using Network.WebSocket.Interfaces;
using Network.WebSocket.Models;

namespace Network.WebRTC.Handlers
{
    public class PeerConnectionHandler : IPeerConnectionHandler
    {
        private readonly IRtcClient rtcClient;
        private readonly IWebSocketManager wsManager;
        private readonly IWebRTCManager webRTCManager;

        public event Action OnNegotiationComplete;
        public event Action<string> OnNegotiationError;

        private bool isOfferer;
        private string targetPeer;

        public PeerConnectionHandler(IRtcClient client, IWebSocketManager ws, IWebRTCManager manager)
        {
            rtcClient = client;
            wsManager = ws;
            webRTCManager = manager;

            RegisterEvents();
        }

        private void RegisterEvents()
        {
            wsManager.Signaling.OnOfferReceived += HandleOfferReceived;
            wsManager.Signaling.OnAnswerReceived += HandleAnswerReceived;
            wsManager.Signaling.OnICECandidateReceived += HandleICECandidateReceived;

            rtcClient.OnIceCandidate += HandleLocalIceCandidate;
        }

        public void InitiateConnection(string targetPeerUsername)
        {
            isOfferer = true;
            targetPeer = targetPeerUsername;

            Debug.Log($"[PEER CONNECTION] Initiating connection to {targetPeer}");

            rtcClient.CreatePeerConnection();

            if (webRTCManager?.DataChannel != null)
            {
                webRTCManager.DataChannel.CreateDataChannels();
            }

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

        private void HandleLocalIceCandidate(RTCIceCandidate candidate)
        {
            if (string.IsNullOrEmpty(targetPeer))
            {
                Debug.LogWarning("[PEER CONNECTION] Cannot send ICE candidate, no target peer");
                return;
            }

            Debug.Log($"[PEER CONNECTION] Sending ICE candidate to {targetPeer}");

            wsManager.Signaling.SendICECandidate(
                candidate.Candidate,
                candidate.SdpMid,
                candidate.SdpMLineIndex ?? 0,
                targetPeer
            );
        }

        private void HandleICECandidateReceived(ICECandidate iceData)
        {
            Debug.Log($"[PEER CONNECTION] ICE candidate received from {iceData.from}");

            var candidateInit = new RTCIceCandidateInit
            {
                candidate = iceData.candidate,
                sdpMid = iceData.sdpMid,
                sdpMLineIndex = iceData.sdpMLineIndex
            };

            var candidate = new RTCIceCandidate(candidateInit);

            rtcClient.AddIceCandidate(candidate);
        }

        public void Dispose()
        {
            wsManager.Signaling.OnOfferReceived -= HandleOfferReceived;
            wsManager.Signaling.OnAnswerReceived -= HandleAnswerReceived;
            wsManager.Signaling.OnICECandidateReceived -= HandleICECandidateReceived;

            rtcClient.OnIceCandidate -= HandleLocalIceCandidate;
        }
    }
} 