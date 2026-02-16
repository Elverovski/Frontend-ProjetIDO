using UnityEngine;
using Robot.Control.Controllers;

namespace Robot.Input
{
    /// <summary>
    /// Tracks VR headset rotation and maps it to robot servo motors
    /// Controls a pan-tilt camera system based on where the player looks
    /// </summary>
    public class VRCameraInput : MonoBehaviour
    {
        // ===== VR CAMERA SETTINGS =====
        [Header("VR Camera")]
        [Tooltip("Your VR camera (Main Camera by default)")]
        [SerializeField] private Transform vrCamera;
        
        // ===== SERVO CENTER POSITIONS =====
        [Header("Servo Mapping")]
        [Tooltip("Center position for horizontal servo (0-180°)")]
        [SerializeField] private float servoPanCenter = 90f;
        
        [Tooltip("Center position for vertical servo (0-180°)")]
        [SerializeField] private float servoTiltCenter = 90f;
        
        [Tooltip("Rotation sensitivity (1.0 = normal, higher = more sensitive)")]
        [SerializeField] private float sensitivity = 1.0f;
        
        // ===== SMOOTHING & UPDATE RATE =====
        [Header("Smoothing & Rate")]
        [Tooltip("Smoothing speed (LOWER = smoother, less jitter) [Recommended: 2-3]")]
        [SerializeField] private float smoothSpeed = 2f;
        
        [Tooltip("Send frequency in Hz (LOWER = less jitter) [Recommended: 10-15]")]
        [SerializeField] private float sendRate = 10f;
        
        // ===== HEAD ROTATION LIMITS =====
        [Header("VR Head Rotation Limits")]
        [Tooltip("Maximum left/right head rotation in degrees")]
        [SerializeField] private float maxYaw = 90f;
        
        [Tooltip("Maximum up/down head rotation in degrees")]
        [SerializeField] private float maxPitch = 45f;
        
        // ===== DEBUG DISPLAY =====
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        
        // ===== INTERNAL VARIABLES =====
        private RobotController robotController;      // Reference to robot control system
        private Vector2 currentServoAngles;           // Current smoothed servo positions
        private Vector2 targetServoAngles;            // Target servo positions (where we want to go)
        private float lastSendTime;                   // Timestamp of last command sent
        
        /// <summary>
        /// Initialize the VR camera tracking system
        /// </summary>
        void Start()
        {
            // Try to find VR camera automatically if not assigned
            if (vrCamera == null)
            {
                vrCamera = Camera.main?.transform;
                
                if (vrCamera == null)
                {
                    Debug.LogError("[VRCameraInput] ✗ No VR camera found! Assign one in Inspector.");
                    enabled = false;
                    return;
                }
                
                Debug.Log("[VRCameraInput] Auto-detected VR camera: " + vrCamera.name);
            }
            
            // Get reference to the robot controller
            robotController = RobotController.Instance;
            
            if (robotController == null)
            {
                Debug.LogError("[VRCameraInput] ✗ RobotController not found!");
                enabled = false;
                return;
            }
            
            // Start with servos at center position
            currentServoAngles = new Vector2(servoPanCenter, servoTiltCenter);
            targetServoAngles = currentServoAngles;
            
            Debug.Log("[VRCameraInput] ✓ Initialized - VR head tracking active");
            Debug.Log($"[VRCameraInput] Smoothing: {smoothSpeed}, Send rate: {sendRate}Hz");
        }
        
        /// <summary>
        /// Update servo positions based on VR headset rotation
        /// Called every frame
        /// </summary>
        void Update()
        {
            // Only run if we're connected to the robot
            if (robotController == null || !robotController.IsConnected)
            {
                return;
            }
            
            // STEP 1: Read VR headset rotation
            Vector3 headRotation = vrCamera.localEulerAngles;
            
            // STEP 2: Extract Yaw (left/right) and Pitch (up/down) angles
            float yaw = NormalizeAngle(headRotation.y);   // Horizontal rotation
            float pitch = NormalizeAngle(headRotation.x); // Vertical rotation
            
            // STEP 3: Limit head rotation to specified maximums
            // This prevents the servos from moving beyond safe ranges
            yaw = Mathf.Clamp(yaw, -maxYaw, maxYaw);
            pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);
            
            // STEP 4: Convert head rotation to servo angles
            // Pan servo (horizontal): add yaw rotation to center position
            float panAngle = servoPanCenter + (yaw * sensitivity);
            
            // Tilt servo (vertical): add pitch rotation to center position
            // Positive pitch (head up) increases tilt angle
            float tiltAngle = servoTiltCenter + (pitch * sensitivity);
            
            // STEP 5: Save as target position (where we want to move to)
            targetServoAngles = new Vector2(panAngle, tiltAngle);
            
            // STEP 6: Smooth the movement using linear interpolation
            // This prevents jerky motion and reduces jitter
            currentServoAngles = Vector2.Lerp(currentServoAngles, targetServoAngles, Time.deltaTime * smoothSpeed);
            
            // STEP 7: Send commands to robot with rate limiting
            // Only send updates at the specified frequency (not every frame)
            // This reduces network traffic and servo jitter
            if (Time.time - lastSendTime >= 1f / sendRate)
            {
                robotController.Servo.SetPosition(currentServoAngles.x, currentServoAngles.y);
                lastSendTime = Time.time;
            }
        }
        
        /// <summary>
        /// Converts Unity's 0-360° angle format to -180° to +180° format
        /// Makes it easier to work with left/right and up/down rotations
        /// </summary>
        /// <param name="angle">Angle in 0-360° format</param>
        /// <returns>Angle in -180° to +180° format</returns>
        private float NormalizeAngle(float angle)
        {
            // If angle is greater than 180, convert to negative angle
            // Example: 270° becomes -90°
            if (angle > 180f)
            {
                angle -= 360f;
            }
            return angle;
        }
        
        /// <summary>
        /// Display debug information on screen
        /// Shows current head rotation and servo positions
        /// </summary>
        void OnGUI()
        {
            // Only show if debug is enabled and component is active
            if (!showDebugInfo || !enabled) return;
            
            // Setup text style
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 14;
            style.normal.textColor = Color.white;
            style.fontStyle = FontStyle.Bold;
            
            // Draw semi-transparent background box
            GUI.Box(new Rect(10, 200, 320, 170), "");
            
            int yPos = 210;
            
            // Title
            GUI.Label(new Rect(20, yPos, 300, 25), "=== VR CAMERA INPUT ===", style);
            yPos += 30;
            
            // Show tracking info if connected
            if (robotController != null && robotController.IsConnected)
            {
                // Get current head rotation
                var headRot = vrCamera.localEulerAngles;
                float yaw = NormalizeAngle(headRot.y);
                float pitch = NormalizeAngle(headRot.x);
                
                // Display head yaw (left/right rotation)
                GUI.Label(new Rect(20, yPos, 300, 20), 
                    $"Head Yaw: {yaw:F1}° (±{maxYaw}°)", style);
                yPos += 25;
                
                // Display head pitch (up/down rotation)
                GUI.Label(new Rect(20, yPos, 300, 20), 
                    $"Head Pitch: {pitch:F1}° (±{maxPitch}°)", style);
                yPos += 25;
                
                // Display current pan servo angle
                GUI.Label(new Rect(20, yPos, 300, 20), 
                    $"Servo Pan: {currentServoAngles.x:F1}°", style);
                yPos += 25;
                
                // Display current tilt servo angle
                GUI.Label(new Rect(20, yPos, 300, 20), 
                    $"Servo Tilt: {currentServoAngles.y:F1}°", style);
                yPos += 25;
                
                // Display smoothing and rate settings
                style.fontSize = 11;
                GUI.Label(new Rect(20, yPos, 300, 20), 
                    $"Smooth: {smoothSpeed} | Rate: {sendRate}Hz", style);
            }
            else
            {
                // Show error if not connected
                style.normal.textColor = Color.red;
                GUI.Label(new Rect(20, yPos, 300, 20), 
                    "Robot not connected!", style);
            }
        }
    }
}