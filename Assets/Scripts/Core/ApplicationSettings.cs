using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "ApplicationSettings", menuName = "VRDrive/Settings")]
    public class ApplicationSettings : ScriptableObject
    {
        [Header("Network - Backend")]
        public string backendURL = "http://192.168.1.100:5000";
        public string backendUsername = "admin";
        public string backendPassword = "admin123";
        
        [Header("Network - Raspberry Pi")]
        public string raspberryPiIP = "192.168.1.101";
        public int webRTCSignalingPort = 8080;
        
        [Header("WebRTC - ICE Servers")]
        public string[] stunServers = new string[]
        {
            "stun:stun.l.google.com:19302",
            "stun:stun1.l.google.com:19302"
        };
        
        [Header("Camera")]
        public float cameraYawMin = -90f;
        public float cameraYawMax = 90f;
        public float cameraPitchMin = -45f;
        public float cameraPitchMax = 45f;
        public float cameraSmoothSpeed = 0.2f;
        
        [Header("Safety")]
        public float timeoutThreshold = 2.0f;
        public int batteryWarningLevel = 20;
        public int batteryCriticalLevel = 10;
        
        [Header("Video")]
        public int videoBufferSize = 5;
        public int targetFrameRate = 30;
        
        [Header("Debug")]
        public LogLevel logLevel = LogLevel.Info;
        public bool showDebugUI = true;
    }
}