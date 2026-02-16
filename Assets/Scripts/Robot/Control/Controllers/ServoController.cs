using UnityEngine;
using Robot.Control.Interfaces;
using Robot.Control.Commands;

namespace Robot.Control.Controllers
{
    /// <summary>
    /// Controls robot servos with smoothing and limits
    /// Implements IServoController interface
    /// </summary>
    public class ServoController : IServoController
    {
        private readonly ICommandSender commandSender;
        
        private Vector2 currentPosition = new Vector2(90, 90);
        private Vector2 targetPosition = new Vector2(90, 90);
        
        public Vector2 CurrentPosition => currentPosition;
        public Vector2 MinAngles { get; set; } = new Vector2(0, 0);
        public Vector2 MaxAngles { get; set; } = new Vector2(180, 180);
        
        public float SmoothSpeed { get; set; } = 5f;
        public float SendRate { get; set; } = 20f; // Hz
        
        private float lastSendTime;
        private Vector2 lastSentPosition;
        
        public ServoController(ICommandSender sender)
        {
            commandSender = sender;
            Debug.Log("[ServoController] Initialized");
        }
        
        public void SetPosition(float pan, float tilt)
        {
            pan = Mathf.Clamp(pan, MinAngles.x, MaxAngles.x);
            tilt = Mathf.Clamp(tilt, MinAngles.y, MaxAngles.y);
            
            targetPosition = new Vector2(pan, tilt);
        }
        
        public void SetPan(float angle)
        {
            SetPosition(angle, targetPosition.y);
        }
        
        public void SetTilt(float angle)
        {
            SetPosition(targetPosition.x, angle);
        }
        
        public void Update()
        {
            // Smooth movement towards target
            currentPosition = Vector2.Lerp(currentPosition, targetPosition, Time.deltaTime * SmoothSpeed);
            
            // Send at fixed rate
            if (Time.time - lastSendTime >= 1f / SendRate)
            {
                SendCurrentPosition();
                lastSendTime = Time.time;
            }
        }
        
        private void SendCurrentPosition()
        {
            if (!commandSender.IsReady) return;
            
            if (Vector2.Distance(currentPosition, lastSentPosition) < 0.5f)
            {
                return;
            }
            
            var command = new ServoCommand(currentPosition.x, currentPosition.y);
            commandSender.SendCommand(command.ToJson());
            
            lastSentPosition = currentPosition;
        }
    }
}