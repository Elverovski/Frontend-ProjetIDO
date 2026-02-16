using System;
using System.Collections;
using UnityEngine;
using Unity.WebRTC;
using Network.WebRTC.Interfaces;

namespace Network.WebRTC.Core
{
    public class RTCClient : MonoBehaviour, IRtcClient
    {
        private static RTCClient _instance;
        public static RTCClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("RTCClient");
                    _instance = go.AddComponent<RTCClient>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private RTCPeerConnection peerConnection;
        private RTCConfiguration rtcConfiguration;

        public bool IsConnected { get; private set; }

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

            rtcConfiguration = GetDefaultConfiguration();

            Debug.Log("[RTC CLIENT] Initialized");
        }

        private void Start()
        {
            StartCoroutine(Unity.WebRTC.WebRTC.Update());
            Debug.Log("[RTC CLIENT] Ready");
        }

        public void Initialize(RTCConfig config)
        {
            if (config == null)
            {
                Debug.LogWarning("[RTC CLIENT] Config is null, using default");
                rtcConfiguration = GetDefaultConfiguration();
            }
            else
            {
                rtcConfiguration = config.GetConfiguration();
            }
            Debug.Log("[RTC CLIENT] Configuration set");
        }

        private RTCConfiguration GetDefaultConfiguration()
        {
            return new RTCConfiguration
            {
                iceServers = new RTCIceServer[]
                {
                    new RTCIceServer { urls = new string[] { "stun:stun.l.google.com:19302" } }
                }
            };
        }

        public void CreatePeerConnection()
        {
            if (peerConnection != null)
            {
                Debug.LogWarning("[RTC CLIENT] Peer connection already exists");
                return;
            }

            if (rtcConfiguration.iceServers == null || rtcConfiguration.iceServers.Length == 0)
            {
                Debug.LogWarning("[RTC CLIENT] rtcConfiguration was invalid, creating default");
                rtcConfiguration = GetDefaultConfiguration();
            }

            var configuration = rtcConfiguration;
            peerConnection = new RTCPeerConnection(ref configuration);

            var transceiverInit = new RTCRtpTransceiverInit
            {
                direction = RTCRtpTransceiverDirection.RecvOnly
            };

            peerConnection.AddTransceiver(TrackKind.Video, transceiverInit);
            Debug.Log("[RTC CLIENT] Video transceiver added (RecvOnly)");

            RegisterPeerConnectionCallbacks();

            Debug.Log("[RTC CLIENT] Peer connection created");
        }

        private void RegisterPeerConnectionCallbacks()
        {
            peerConnection.OnIceCandidate = candidate =>
            {
                Debug.Log("[RTC CLIENT] ICE candidate generated");
                OnIceCandidate?.Invoke(candidate);
            };

            peerConnection.OnTrack = e =>
            {
                Debug.Log($"[RTC CLIENT] Track received: {e.Track.Kind}");
                OnTrack?.Invoke(e);
            };

            peerConnection.OnDataChannel = channel =>
            {
                Debug.Log($"[RTC CLIENT] Data channel received: {channel.Label}");
                OnDataChannel?.Invoke(channel);
            };

            peerConnection.OnConnectionStateChange = HandleConnectionStateChange;
        }

        private void HandleConnectionStateChange(RTCPeerConnectionState state)
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
        }

        public void ClosePeerConnection()
        {
            if (peerConnection == null) return;

            peerConnection.Close();
            peerConnection.Dispose();
            peerConnection = null;

            IsConnected = false;
            Debug.Log("[RTC CLIENT] Peer connection closed");
        }

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

        public RTCPeerConnection GetPeerConnection()
        {
            return peerConnection;
        }

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