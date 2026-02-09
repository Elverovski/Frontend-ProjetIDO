using System;

namespace Network.WebRTC.Models
{
    /// <summary>
    /// Command sent to the vehicle
    /// </summary>
    [Serializable]
    public class VehicleCommand
    {
        public string type;           // Command type, e.g., "movement", "camera"
        public MovementData movement; // Movement details (optional)
        public CameraData camera;     // Camera details (optional)
        public long timestamp;        // Unix timestamp of the command
    }

    // Movement control data
    [Serializable]
    public class MovementData
    {
        public float throttle;        // Throttle value (0-1)
        public float steering;        // Steering value (-1 to 1)
        public float brake;           // Brake value (0-1)
    }

    // Camera control data
    [Serializable]
    public class CameraData
    {
        public float pan;             // Pan angle
        public float tilt;            // Tilt angle
    }

    // Emergency stop command
    [Serializable]
    public class EmergencyStopCommand
    {
        public bool activate;         // True to activate emergency stop
        public string reason;         // Reason for emergency stop
        public long timestamp;        // Unix timestamp of the command
    }
}