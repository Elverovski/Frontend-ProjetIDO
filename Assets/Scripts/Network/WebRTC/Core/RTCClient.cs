using System;
using System.Collections;
using UnityEngine;
using Unity.WebRTC;

namespace Network.WebRTC.Core
{
    public class RTCClient : MonoBehaviour
    {
        private static RTCClient _instance;
        public static RTCClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("RTCClient");
                    _instance = go.AddComponent<RTCClient>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private RTCPeerConnection peerConnection;
        private RTCConfig rtcConfig;

        // Connection state
        public bool IsConnected { get; private set; }

        // Events
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<RTCIceCandidate> OnIceCandidate;
        public event Action<RTCTrackEvent> OnTrack;
        public event Action<RTCDataChannel> OnDataChannel;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            Debug.Log("[RTC CLIENT] Initialized");
        }

        // Set RTC configuration
        public void Initialize(RTCConfig config)
        {
            rtcConfig = config;
            Debug.Log("[RTC CLIENT] Configuration set");
        }

        // Create a new peer connection
        public void CreatePeerConnection()
        {
            if (peerConnection != null)
            {
                Debug.LogWarning("[RTC CLIENT] Peer connection already exists");
                return;
            }

            var configuration = rtcConfig.GetConfiguration();
            peerConnection = new RTCPeerConnection(ref configuration);

            // ICE candidate event
            peerConnection.OnIceCandidate = candidate => 
            {
                Debug.Log("[RTC CLIENT] ICE candidate generated");
                OnIceCandidate?.Invoke(candidate);
            };

            // Track received event
            peerConnection.OnTrack = e => 
            {
                Debug.Log($"[RTC CLIENT] Track received: {e.Track.Kind}");
                OnTrack?.Invoke(e);
            };

            // Data channel received event
            peerConnection.OnDataChannel = channel => 
            {
                Debug.Log($"[RTC CLIENT] Data channel received: {channel.Label}");
                OnDataChannel?.Invoke(channel);
            };

            // Connection state changes
            peerConnection.OnConnectionStateChange = state =>
            {
                Debug.Log($"[RTC CLIENT] Connection state: {state}");
                
                if (state == RTCPeerConnectionState.Connected)
                {
                    IsConnected = true;
                    OnConnected?.Invoke();
                }
                else if (state == RTCPeerConnectionState.Disconnected || 
                         state == RTCPeerConnectionState.Failed || 
                         state == RTCPeerConnectionState.Closed)
                {
                    IsConnected = false;
                    OnDisconnected?.Invoke();
                }
            };

            Debug.Log("[RTC CLIENT] Peer connection created");
        }

        // Close and dispose peer connection
        public void ClosePeerConnection()
        {
            if (peerConnection == null) return;

            peerConnection.Close();
            peerConnection.Dispose();
            peerConnection = null;

            IsConnected = false;
            Debug.Log("[RTC CLIENT] Peer connection closed");
        }

        // Create WebRTC offer
        public void CreateOffer(Action<RTCSessionDescription> onSuccess, Action<string> onError)
        {
            StartCoroutine(CreateOfferCoroutine(onSuccess, onError));
        }

        private IEnumerator CreateOfferCoroutine(Action<RTCSessionDescription> onSuccess, Action<string> onError)
        {
            if (peerConnection == null)
            {
                onError?.Invoke("Peer connection not created");
                yield break;
            }

            var op = peerConnection.CreateOffer();
            yield return op;

            if (op.IsError)
            {
                Debug.LogError($"[RTC CLIENT] Create offer failed: {op.Error.message}");
                onError?.Invoke(op.Error.message);
                yield break;
            }

            var desc = op.Desc;
            var setLocalOp = peerConnection.SetLocalDescription(ref desc);
            yield return setLocalOp;

            if (setLocalOp.IsError)
            {
                Debug.LogError($"[RTC CLIENT] Set local description failed: {setLocalOp.Error.message}");
                onError?.Invoke(setLocalOp.Error.message);
                yield break;
            }

            Debug.Log("[RTC CLIENT] Offer created successfully");
            onSuccess?.Invoke(desc);
        }

        // Create WebRTC answer
        public void CreateAnswer(Action<RTCSessionDescription> onSuccess, Action<string> onError)
        {
            StartCoroutine(CreateAnswerCoroutine(onSuccess, onError));
        }

        private IEnumerator CreateAnswerCoroutine(Action<RTCSessionDescription> onSuccess, Action<string> onError)
        {
            if (peerConnection == null)
            {
                onError?.Invoke("Peer connection not created");
                yield break;
            }

            var op = peerConnection.CreateAnswer();
            yield return op;

            if (op.IsError)
            {
                Debug.LogError($"[RTC CLIENT] Create answer failed: {op.Error.message}");
                onError?.Invoke(op.Error.message);
                yield break;
            }

            var desc = op.Desc;
            var setLocalOp = peerConnection.SetLocalDescription(ref desc);
            yield return setLocalOp;

            if (setLocalOp.IsError)
            {
                Debug.LogError($"[RTC CLIENT] Set local description failed: {setLocalOp.Error.message}");
                onError?.Invoke(setLocalOp.Error.message);
                yield break;
            }

            Debug.Log("[RTC CLIENT] Answer created successfully");
            onSuccess?.Invoke(desc);
        }

        // Set remote description
        public void SetRemoteDescription(RTCSessionDescription desc, Action onSuccess, Action<string> onError)
        {
            StartCoroutine(SetRemoteDescriptionCoroutine(desc, onSuccess, onError));
        }

        private IEnumerator SetRemoteDescriptionCoroutine(RTCSessionDescription desc, Action onSuccess, Action<string> onError)
        {
            if (peerConnection == null)
            {
                onError?.Invoke("Peer connection not created");
                yield break;
            }

            var op = peerConnection.SetRemoteDescription(ref desc);
            yield return op;

            if (op.IsError)
            {
                Debug.LogError($"[RTC CLIENT] Set remote description failed: {op.Error.message}");
                onError?.Invoke(op.Error.message);
                yield break;
            }

            Debug.Log("[RTC CLIENT] Remote description set successfully");
            onSuccess?.Invoke();
        }

        // Add ICE candidate
        public void AddIceCandidate(RTCIceCandidate candidate)
        {
            if (peerConnection == null)
            {
                Debug.LogWarning("[RTC CLIENT] Cannot add ICE candidate, peer connection is null");
                return;
            }

            peerConnection.AddIceCandidate(candidate);
            Debug.Log("[RTC CLIENT] ICE candidate added");
        }

        // Create data channel
        public RTCDataChannel CreateDataChannel(string label)
        {
            if (peerConnection == null)
            {
                Debug.LogError("[RTC CLIENT] Cannot create data channel, peer connection is null");
                return null;
            }

            var dataChannel = peerConnection.CreateDataChannel(label);
            Debug.Log($"[RTC CLIENT] Data channel created: {label}");
            return dataChannel;
        }

        // Get peer connection instance
        public RTCPeerConnection GetPeerConnection()
        {
            return peerConnection;
        }

        // Cleanup on destroy
        private void OnDestroy()
        {
            ClosePeerConnection();

            if (_instance == this)
            {
                _instance = null;
            }

            Debug.Log("[RTC CLIENT] Destroyed");
        }

        private void OnApplicationQuit()
        {
            ClosePeerConnection();
        }
    }
}
