using System;
using UnityEngine;
using Network.WebRTC.Interfaces;
using Network.WebRTC.Core;
using Network.WebRTC.Handlers;
using Network.WebRTC.Models;
using Network.WebSocket.Interfaces;
using Network.WebSocket;

namespace Network.WebRTC
{
    public class WebRTCManager : MonoBehaviour, IWebRTCManager
    {
        private static WebRTCManager _instance;
        public static WebRTCManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("WebRTCManager");
                    _instance = go.AddComponent<WebRTCManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Configuration")]
        [SerializeField] private RTCConfig rtcConfig;

        private IRtcClient rtcClient;
        private IWebSocketManager wsManager;

        private IPeerConnectionHandler peerConnectionHandler;
        private IDataChannelHandler dataChannelHandler;
        private IVideoStreamHandler videoStreamHandler;
        private IStatsHandler statsHandler;

        public IDataChannelHandler DataChannel => dataChannelHandler;
        public IVideoStreamHandler VideoStream => videoStreamHandler;
        public IPeerConnectionHandler PeerConnection => peerConnectionHandler;
        public IStatsHandler Stats => statsHandler;

        public bool IsConnected => rtcClient?.IsConnected ?? false;
        public bool IsDataChannelReady => dataChannelHandler?.IsReady ?? false;

        public event Action OnWebRTCConnected;
        public event Action OnWebRTCDisconnected;
        public event Action OnDataChannelReady;
        public event Action OnVideoReady;
        public event Action<TelemetryData> OnTelemetryReceived;

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

        private void Start()
        {
            InitializeWebRTC();
        }

        private void InitializeWebRTC()
        {
            Debug.Log("[WEBRTC MANAGER] Initializing...");

            Unity.WebRTC.WebRTC.Initialize();
            Debug.Log("[WEBRTC MANAGER] WebRTC initialized");

            rtcConfig ??= new RTCConfig();

            wsManager = WebSocketManager.Instance;
            rtcClient = RTCClient.Instance;
            rtcClient.Initialize(rtcConfig);

            peerConnectionHandler = new PeerConnectionHandler(rtcClient, wsManager, this);
            dataChannelHandler = new DataChannelHandler(rtcClient);
            videoStreamHandler = new VideoStreamHandler(rtcClient);
            statsHandler = new StatsHandler(rtcClient, this);

            RegisterEvents();

            Debug.Log("[WEBRTC MANAGER] Initialized");
        }

        private void RegisterEvents()
        {
            rtcClient.OnConnected += HandleRTCConnected;
            rtcClient.OnDisconnected += HandleRTCDisconnected;

            peerConnectionHandler.OnNegotiationComplete += HandleNegotiationComplete;
            peerConnectionHandler.OnNegotiationError += HandleNegotiationError;

            dataChannelHandler.OnDataChannelReady += HandleDataChannelReady;
            dataChannelHandler.OnTelemetryReceived += HandleTelemetryReceived;

            videoStreamHandler.OnVideoTrackAdded += HandleVideoTrackAdded;

            wsManager.Signaling.OnRobotConnected += HandleRobotConnected;
        }

        public void ConnectToPeer(string robotUsername)
        {
            Debug.Log($"[WEBRTC MANAGER] Connecting to peer: {robotUsername}");
            peerConnectionHandler.InitiateConnection(robotUsername);
        }

        public void StartStatsMonitoring(float interval = 1f)
        {
            statsHandler?.StartMonitoring(interval);
        }

        public void StopStatsMonitoring()
        {
            statsHandler?.StopMonitoring();
        }

        public void Disconnect()
        {
            Debug.Log("[WEBRTC MANAGER] Disconnecting...");
            statsHandler?.StopMonitoring();
            rtcClient?.ClosePeerConnection();
        }

        private void HandleRTCConnected()
        {
            Debug.Log("[WEBRTC MANAGER] WebRTC CONNECTED");
            OnWebRTCConnected?.Invoke();
        }

        private void HandleRTCDisconnected()
        {
            Debug.Log("[WEBRTC MANAGER] WebRTC DISCONNECTED");
            OnWebRTCDisconnected?.Invoke();
        }

        private void HandleNegotiationComplete()
        {
            Debug.Log("[WEBRTC MANAGER] Negotiation complete");
        }

        private void HandleNegotiationError(string error)
        {
            Debug.LogError($"[WEBRTC MANAGER] Negotiation error: {error}");
        }

        private void HandleDataChannelReady()
        {
            Debug.Log("[WEBRTC MANAGER] Data channel ready");
            OnDataChannelReady?.Invoke();
        }

        private void HandleTelemetryReceived(TelemetryData telemetry)
        {
            OnTelemetryReceived?.Invoke(telemetry);
        }

        private void HandleVideoTrackAdded()
        {
            Debug.Log("[WEBRTC MANAGER] Video track ready");
            OnVideoReady?.Invoke();
        }

        private void HandleRobotConnected(string robotUsername)
        {
            Debug.Log($"[WEBRTC MANAGER] Robot connected: {robotUsername}");
        }

        private void Update()
        {
            videoStreamHandler?.Update();
        }

        private void OnDestroy()
        {
            Debug.Log("[WEBRTC MANAGER] Cleanup...");

            if (rtcClient != null)
            {
                rtcClient.OnConnected -= HandleRTCConnected;
                rtcClient.OnDisconnected -= HandleRTCDisconnected;
            }

            if (wsManager?.Signaling != null)
            {
                wsManager.Signaling.OnRobotConnected -= HandleRobotConnected;
            }

            peerConnectionHandler?.Dispose();
            dataChannelHandler?.Dispose();
            videoStreamHandler?.Dispose();
            statsHandler?.Dispose();

            Unity.WebRTC.WebRTC.Dispose();
            Debug.Log("[WEBRTC MANAGER] WebRTC disposed");

            Debug.Log("[WEBRTC MANAGER] Cleanup complete");

            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnApplicationQuit()
        {
            Disconnect();
        }
    }
}