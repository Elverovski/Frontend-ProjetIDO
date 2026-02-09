using UnityEngine;

namespace Core
{
    /// <summary>
    /// Holds all configurable settings for network, vehicle, camera, video, safety, and debug in VRDrive.
    /// </summary>
    [CreateAssetMenu(fileName = "ApplicationSettings", menuName = "VRDrive/Settings")]
    public class ApplicationSettings : ScriptableObject
    {
        [Header("Network - Backend")]
        // URL and login info for the backend server
        public string backendURL = "http://192.168.1.100:5000";
        public string backendUsername = "admin";
        public string backendPassword = "admin123";
        
        [Header("Network - Raspberry Pi")]
        // Raspberry Pi IP and WebRTC signaling port
        public string raspberryPiIP = "192.168.1.101";
        public int webRTCSignalingPort = 8080;
        
        [Header("WebRTC - ICE Servers")]
        // List of STUN servers for WebRTC peer connection
        public string[] stunServers = new string[]
        {
            "stun:stun.l.google.com:19302",
            "stun:stun1.l.google.com:19302"
        };
        
        [Header("Vehicle Control")]
        // Vehicle update rate (Hz), max throttle and steering values
        public float vehicleUpdateRate = 30f; 
        public float maxThrottle = 1.0f;
        public float maxSteering = 1.0f;
        
        [Header("Camera")]
        // Camera movement limits and smoothing speed
        public float cameraYawMin = -90f;
        public float cameraYawMax = 90f;
        public float cameraPitchMin = -45f;
        public float cameraPitchMax = 45f;
        public float cameraSmoothSpeed = 0.2f;
        
        [Header("Safety")]
        // Timeout for connection, battery warning and critical levels
        public float timeoutThreshold = 2.0f; 
        public int batteryWarningLevel = 20;
        public int batteryCriticalLevel = 10; 
        
        [Header("Video")]
        // Video buffer size and target frame rate
        public int videoBufferSize = 5; 
        public int targetFrameRate = 30; 
        
        [Header("Debug")]
        // Logging level and option to show debug UI
        public LogLevel logLevel = LogLevel.Info;
        public bool showDebugUI = true;
    }
}
