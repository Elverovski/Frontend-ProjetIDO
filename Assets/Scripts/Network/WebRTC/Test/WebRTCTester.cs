using UnityEngine;
using Network.WebRTC;
using Network.WebRTC.Interfaces;
using Network.WebRTC.Models;
using Network.WebSocket;
using Network.WebSocket.Interfaces;

namespace Network.WebRTC.Test
{
    /// <summary>
    /// Test script for WebRTC connection functionality.
    /// Tests peer connection, data channels, video streams, and statistics.
    /// Used for debugging and verifying WebRTC setup.
    /// </summary>
    public class WebRTCTester : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private string robotUsername = "pi";

        [Header("Auto")]
        [SerializeField] private bool autoConnectWebRTC = false;

        private IWebRTCManager webRTCManager;
        private IWebSocketManager wsManager;

        void Start()
        {
            Debug.Log("========================================");
            Debug.Log("[WEBRTC TESTER] WebRTC Test Started");
            Debug.Log("========================================");

            webRTCManager = WebRTCManager.Instance;
            wsManager = WebSocketManager.Instance;

            RegisterEvents();
            PrintControls();
        }

        /// <summary>
        /// Subscribes to WebRTC and WebSocket events for testing.
        /// </summary>
        private void RegisterEvents()
        {
            webRTCManager.OnWebRTCConnected += OnWebRTCConnected;
            webRTCManager.OnWebRTCDisconnected += OnWebRTCDisconnected;
            webRTCManager.OnDataChannelReady += OnDataChannelReady;
            webRTCManager.OnVideoReady += OnVideoReady;
            webRTCManager.OnTelemetryReceived += OnTelemetryReceived;

            wsManager.Signaling.OnRobotConnected += OnRobotConnected;
        }

        /// <summary>
        /// Displays available keyboard controls in console.
        /// </summary>
        private void PrintControls()
        {
            Debug.Log("[WEBRTC TESTER] Keyboard Controls:");
            Debug.Log("  R - Connect to Robot");
            Debug.Log("  S - Start Stats Monitoring");
            Debug.Log("  D - Disconnect");
            Debug.Log("========================================\n");
        }

        void Update()
        {
            // Keyboard controls for testing
            if (Input.GetKeyDown(KeyCode.R)) ConnectToRobot();
            if (Input.GetKeyDown(KeyCode.S)) StartStatsMonitoring();
            if (Input.GetKeyDown(KeyCode.D)) Disconnect();
        }

        /// <summary>
        /// Initiates WebRTC connection to the robot.
        /// </summary>
        void ConnectToRobot()
        {
            Debug.Log($"[WEBRTC TESTER] Connecting to robot: {robotUsername}");
            webRTCManager.ConnectToPeer(robotUsername);
        }

        /// <summary>
        /// Starts monitoring WebRTC connection statistics.
        /// </summary>
        void StartStatsMonitoring()
        {
            Debug.Log("[WEBRTC TESTER] Starting stats monitoring");
            webRTCManager.StartStatsMonitoring(2f);
            webRTCManager.Stats.OnStatsUpdated += OnStatsUpdated;
        }

        /// <summary>
        /// Disconnects from the WebRTC peer.
        /// </summary>
        void Disconnect()
        {
            Debug.Log("[WEBRTC TESTER] Disconnecting");
            webRTCManager.Disconnect();
        }

        #region Event Handlers

        /// <summary>
        /// Called when WebRTC peer connection is established.
        /// </summary>
        void OnWebRTCConnected()
        {
            Debug.Log("========================================");
            Debug.Log("[WEBRTC TESTER] WebRTC CONNECTED");
            Debug.Log("========================================");
        }

        /// <summary>
        /// Called when WebRTC peer connection is lost.
        /// </summary>
        void OnWebRTCDisconnected()
        {
            Debug.Log("========================================");
            Debug.Log("[WEBRTC TESTER] WebRTC DISCONNECTED");
            Debug.Log("========================================");
        }

        /// <summary>
        /// Called when data channels are open and ready.
        /// </summary>
        void OnDataChannelReady()
        {
            Debug.Log("========================================");
            Debug.Log("[WEBRTC TESTER] DATA CHANNEL READY");
            Debug.Log("========================================");
        }

        /// <summary>
        /// Called when video track is received and ready.
        /// </summary>
        void OnVideoReady()
        {
            Debug.Log("========================================");
            Debug.Log("[WEBRTC TESTER] VIDEO TRACK READY");
            Debug.Log("========================================");
        }

        /// <summary>
        /// Called when telemetry data is received from robot.
        /// Logs battery, GPS, and system information.
        /// </summary>
        void OnTelemetryReceived(TelemetryData telemetry)
        {
            Debug.Log($"[WEBRTC TESTER] Telemetry received at {telemetry.timestamp}");

            // Log battery info if available
            if (telemetry.battery != null)
            {
                Debug.Log($"  Battery: {telemetry.battery.percentage}% ({telemetry.battery.voltage}V)");
            }

            // Log GPS info if available
            if (telemetry.gps != null)
            {
                Debug.Log($"  GPS: {telemetry.gps.latitude}, {telemetry.gps.longitude}");
            }

            // Log system info if available
            if (telemetry.system != null)
            {
                Debug.Log($"  CPU: {telemetry.system.cpuUsage}%, Temp: {telemetry.system.cpuTemperature}°C");
            }
        }

        /// <summary>
        /// Called when a robot connects to the WebSocket server.
        /// Auto-connects to WebRTC if enabled.
        /// </summary>
        void OnRobotConnected(string username)
        {
            Debug.Log($"[WEBRTC TESTER] Robot connected: {username}");

            if (autoConnectWebRTC)
            {
                Debug.Log("[WEBRTC TESTER] Auto-connecting to robot...");
                Invoke(nameof(ConnectToRobot), 1f);
            }
        }

        /// <summary>
        /// Called when WebRTC stats are updated.
        /// </summary>
        void OnStatsUpdated(Unity.WebRTC.RTCStatsReport stats)
        {
            Debug.Log($"[WEBRTC TESTER] Stats updated: {stats.Stats.Count} items");
        }

        #endregion

        /// <summary>
        /// Cleanup: unsubscribes from all events.
        /// </summary>
        void OnDestroy()
        {
            if (webRTCManager != null)
            {
                webRTCManager.OnWebRTCConnected -= OnWebRTCConnected;
                webRTCManager.OnWebRTCDisconnected -= OnWebRTCDisconnected;
                webRTCManager.OnDataChannelReady -= OnDataChannelReady;
                webRTCManager.OnVideoReady -= OnVideoReady;
                webRTCManager.OnTelemetryReceived -= OnTelemetryReceived;
            }

            if (wsManager?.Signaling != null)
            {
                wsManager.Signaling.OnRobotConnected -= OnRobotConnected;
            }
        }
    }
}