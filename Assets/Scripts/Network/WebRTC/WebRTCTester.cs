using UnityEngine;
using Network.WebRTC;
using Network.WebRTC.Models;
using Network.WebSocket;

/// <summary>
/// Test script for WebRTC connection and data channels
/// </summary>
public class WebRTCTester : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string robotUsername = "robot"; // Robot username to connect

    [Header("Auto")]
    [SerializeField] private bool autoConnectWebRTC = false; // Auto-connect to robot on join

    [Header("Test Commands")]
    [SerializeField] private float testThrottle = 0.5f; // Throttle for test command
    [SerializeField] private float testSteering = 0.0f; // Steering for test command

    private WebRTCManager webRTCManager;
    private WebSocketManager wsManager;

    void Start()
    {
        Debug.Log("========================================");
        Debug.Log("[WEBRTC TESTER] WebRTC Test Started");
        Debug.Log("========================================");

        webRTCManager = WebRTCManager.Instance;
        wsManager = WebSocketManager.Instance;

        // Subscribe to events
        webRTCManager.OnWebRTCConnected += OnWebRTCConnected;
        webRTCManager.OnWebRTCDisconnected += OnWebRTCDisconnected;
        webRTCManager.OnDataChannelReady += OnDataChannelReady;
        webRTCManager.OnVideoReady += OnVideoReady;
        webRTCManager.OnTelemetryReceived += OnTelemetryReceived;

        wsManager.Signaling.OnRobotConnected += OnRobotConnected;

        Debug.Log("[WEBRTC TESTER] Keyboard Controls:");
        Debug.Log("  R - Connect to Robot");
        Debug.Log("  T - Send Test Command");
        Debug.Log("  E - Emergency Stop");
        Debug.Log("  S - Start Stats Monitoring");
        Debug.Log("  D - Disconnect");
        Debug.Log("  Arrow Keys - Control Vehicle");
        Debug.Log("========================================\n");
    }

    void Update()
    {
        // Manual controls
        if (Input.GetKeyDown(KeyCode.R)) ConnectToRobot();
        if (Input.GetKeyDown(KeyCode.T)) SendTestCommand();
        if (Input.GetKeyDown(KeyCode.E)) SendEmergencyStop();
        if (Input.GetKeyDown(KeyCode.S)) StartStatsMonitoring();
        if (Input.GetKeyDown(KeyCode.D)) Disconnect();

        // Vehicle controls (if data channel ready)
        if (webRTCManager.IsDataChannelReady)
        {
            HandleVehicleControls();
        }
    }

    // Handle arrow keys for vehicle control
    void HandleVehicleControls()
    {
        float throttle = 0f;
        float steering = 0f;

        // Throttle
        if (Input.GetKey(KeyCode.UpArrow))
            throttle = 1f;
        else if (Input.GetKey(KeyCode.DownArrow))
            throttle = -1f;

        // Steering
        if (Input.GetKey(KeyCode.LeftArrow))
            steering = -1f;
        else if (Input.GetKey(KeyCode.RightArrow))
            steering = 1f;

        // Send command if any input
        if (throttle != 0f || steering != 0f)
        {
            webRTCManager.SendMovementCommand(throttle, steering);
        }
    }

    // Connect to robot
    void ConnectToRobot()
    {
        Debug.Log($"[WEBRTC TESTER] Connecting to robot: {robotUsername}");
        webRTCManager.ConnectToPeer(robotUsername);
    }

    // Send a simple test movement command
    void SendTestCommand()
    {
        if (!webRTCManager.IsDataChannelReady)
        {
            Debug.LogWarning("[WEBRTC TESTER] Data channel not ready");
            return;
        }

        Debug.Log($"[WEBRTC TESTER] Sending test command: throttle={testThrottle}, steering={testSteering}");
        webRTCManager.SendMovementCommand(testThrottle, testSteering);
    }

    // Send emergency stop command
    void SendEmergencyStop()
    {
        Debug.Log("[WEBRTC TESTER] Sending emergency stop");
        webRTCManager.SendEmergencyStop("User test");
    }

    // Start monitoring WebRTC stats
    void StartStatsMonitoring()
    {
        Debug.Log("[WEBRTC TESTER] Starting stats monitoring");
        webRTCManager.StartStatsMonitoring(2f);
        webRTCManager.Stats.OnStatsUpdated += OnStatsUpdated;
    }

    // Disconnect WebRTC
    void Disconnect()
    {
        Debug.Log("[WEBRTC TESTER] Disconnecting");
        webRTCManager.Disconnect();
    }

    #region Event Handlers

    // WebRTC connected
    void OnWebRTCConnected()
    {
        Debug.Log("========================================");
        Debug.Log("[WEBRTC TESTER] WebRTC CONNECTED");
        Debug.Log("========================================");
    }

    // WebRTC disconnected
    void OnWebRTCDisconnected()
    {
        Debug.Log("========================================");
        Debug.Log("[WEBRTC TESTER] WebRTC DISCONNECTED");
        Debug.Log("========================================");
    }

    // Data channels ready
    void OnDataChannelReady()
    {
        Debug.Log("========================================");
        Debug.Log("[WEBRTC TESTER] DATA CHANNEL READY");
        Debug.Log("[WEBRTC TESTER] You can now send commands!");
        Debug.Log("========================================");
    }

    // Video track ready
    void OnVideoReady()
    {
        Debug.Log("========================================");
        Debug.Log("[WEBRTC TESTER] VIDEO TRACK READY");
        Debug.Log("========================================");
    }

    // Telemetry received from robot
    void OnTelemetryReceived(TelemetryData telemetry)
    {
        Debug.Log($"[WEBRTC TESTER] Telemetry received at {telemetry.timestamp}");

        if (telemetry.battery != null)
        {
            Debug.Log($"  Battery: {telemetry.battery.percentage}% ({telemetry.battery.voltage}V)");
        }
        if (telemetry.gps != null)
        {
            Debug.Log($"  GPS: {telemetry.gps.latitude}, {telemetry.gps.longitude}");
        }
        if (telemetry.system != null)
        {
            Debug.Log($"  CPU: {telemetry.system.cpuUsage}%, Temp: {telemetry.system.cpuTemperature}°C");
        }
    }

    // Robot connected to WebSocket
    void OnRobotConnected(string username)
    {
        Debug.Log($"[WEBRTC TESTER] Robot connected: {username}");

        if (autoConnectWebRTC)
        {
            Debug.Log("[WEBRTC TESTER] Auto-connecting to robot...");
            Invoke(nameof(ConnectToRobot), 1f);
        }
    }

    // Stats updated
    void OnStatsUpdated(Unity.WebRTC.RTCStatsReport stats)
    {
        Debug.Log($"[WEBRTC TESTER] Stats updated: {stats.Stats.Count} items");
    }

    #endregion

    void OnDestroy()
    {
        // Unsubscribe from events
        if (webRTCManager != null)
        {
            webRTCManager.OnWebRTCConnected -= OnWebRTCConnected;
            webRTCManager.OnWebRTCDisconnected -= OnWebRTCDisconnected;
            webRTCManager.OnDataChannelReady -= OnDataChannelReady;
            webRTCManager.OnVideoReady -= OnVideoReady;
            webRTCManager.OnTelemetryReceived -= OnTelemetryReceived;
        }

        if (wsManager != null && wsManager.Signaling != null)
        {
            wsManager.Signaling.OnRobotConnected -= OnRobotConnected;
        }
    }
}
