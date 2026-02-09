using System;
using UnityEngine;
using Network.WebRTC.Core;
using Network.WebRTC.Handlers;
using Network.WebRTC.Models;
using Network.WebSocket;

namespace Network.WebRTC
{
    /// <summary>
    /// Main WebRTC manager – orchestrates peer connection, video, and data channels
    /// </summary>
    public class WebRTCManager : MonoBehaviour
    {
        private static WebRTCManager _instance;
        public static WebRTCManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("WebRTCManager");
                    _instance = go.AddComponent<WebRTCManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Configuration")]
        [SerializeField] private RTCConfig rtcConfig;

        // Core components
        private RTCClient rtcClient;
        private WebSocketManager wsManager;

        // Handlers
        private PeerConnectionHandler peerConnectionHandler;
        private DataChannelHandler dataChannelHandler;
        private VideoStreamHandler videoStreamHandler;
        private StatsHandler statsHandler;

        // Public accessors
        public DataChannelHandler DataChannel => dataChannelHandler;
        public VideoStreamHandler VideoStream => videoStreamHandler;
        public PeerConnectionHandler PeerConnection => peerConnectionHandler;
        public StatsHandler Stats => statsHandler;

        // State
        public bool IsConnected => rtcClient?.IsConnected ?? false;
        public bool IsDataChannelReady => dataChannelHandler?.IsReady ?? false;

        // Events
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

            InitializeWebRTC();
        }

        // Initialize WebRTC components and handlers
        private void InitializeWebRTC()
        {
            Debug.Log("[WEBRTC MANAGER] Initializing...");

            // Use default config if none provided
            if (rtcConfig == null)
            {
                rtcConfig = new RTCConfig();
            }

            // Get WebSocket manager
            wsManager = WebSocketManager.Instance;

            // Initialize RTC client
            rtcClient = RTCClient.Instance;
            rtcClient.Initialize(rtcConfig);

            // Initialize handlers
            peerConnectionHandler = new PeerConnectionHandler(rtcClient, wsManager, this);
            dataChannelHandler = new DataChannelHandler(rtcClient);
            videoStreamHandler = new VideoStreamHandler(rtcClient);
            statsHandler = new StatsHandler(rtcClient, this);

            // Subscribe to RTC events
            rtcClient.OnConnected += HandleRTCConnected;
            rtcClient.OnDisconnected += HandleRTCDisconnected;

            peerConnectionHandler.OnNegotiationComplete += HandleNegotiationComplete;
            peerConnectionHandler.OnNegotiationError += HandleNegotiationError;

            dataChannelHandler.OnDataChannelReady += HandleDataChannelReady;
            dataChannelHandler.OnTelemetryReceived += HandleTelemetryReceived;

            videoStreamHandler.OnVideoTrackAdded += HandleVideoTrackAdded;

            // Subscribe to WebSocket robot connected event
            wsManager.Signaling.OnRobotConnected += HandleRobotConnected;

            Debug.Log("[WEBRTC MANAGER] Initialized");
        }

        // Connect to a robot peer
        public void ConnectToPeer(string robotUsername)
        {
            Debug.Log($"[WEBRTC MANAGER] Connecting to peer: {robotUsername}");
            peerConnectionHandler.InitiateConnection(robotUsername);
        }

        // Send vehicle movement command
        public void SendMovementCommand(float throttle, float steering, float brake = 0f)
        {
            if (!IsDataChannelReady)
            {
                Debug.LogWarning("[WEBRTC MANAGER] Data channel not ready");
                return;
            }
            dataChannelHandler.SendMovementCommand(throttle, steering, brake);
        }

        // Send camera command
        public void SendCameraCommand(float pan, float tilt)
        {
            if (!IsDataChannelReady)
            {
                Debug.LogWarning("[WEBRTC MANAGER] Data channel not ready");
                return;
            }
            dataChannelHandler.SendCameraCommand(pan, tilt);
        }

        // Send custom vehicle command
        public void SendVehicleCommand(VehicleCommand command)
        {
            if (!IsDataChannelReady)
            {
                Debug.LogWarning("[WEBRTC MANAGER] Data channel not ready");
                return;
            }
            dataChannelHandler.SendVehicleCommand(command);
        }

        // Send emergency stop command
        public void SendEmergencyStop(string reason = "User initiated")
        {
            Debug.Log($"[WEBRTC MANAGER] Emergency stop: {reason}");
            dataChannelHandler?.SendEmergencyStop(reason);
        }

        // Start stats monitoring
        public void StartStatsMonitoring(float interval = 1f)
        {
            statsHandler?.StartMonitoring(interval);
        }

        // Stop stats monitoring
        public void StopStatsMonitoring()
        {
            statsHandler?.StopMonitoring();
        }

        // Disconnect from peer
        public void Disconnect()
        {
            Debug.Log("[WEBRTC MANAGER] Disconnecting...");
            statsHandler?.StopMonitoring();
            rtcClient?.ClosePeerConnection();
        }

        #region Event Handlers

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
            Debug.Log("[WEBRTC MANAGER] Auto-connecting to robot...");
            ConnectToPeer(robotUsername);
        }

        #endregion

        private void OnDestroy()
        {
            Debug.Log("[WEBRTC MANAGER] Cleanup...");

            if (rtcClient != null)
            {
                rtcClient.OnConnected -= HandleRTCConnected;
                rtcClient.OnDisconnected -= HandleRTCDisconnected;
            }

            if (wsManager != null && wsManager.Signaling != null)
            {
                wsManager.Signaling.OnRobotConnected -= HandleRobotConnected;
            }

            peerConnectionHandler?.Dispose();
            dataChannelHandler?.Dispose();
            videoStreamHandler?.Dispose();
            statsHandler?.Dispose();

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
