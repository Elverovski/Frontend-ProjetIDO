using UnityEngine;
using Robot.Control.Interfaces;
using Robot.Control.Commands;

namespace Robot.Control.Controllers
{
    /// <summary>
    /// Controls robot motors
    /// Implements IMotorController interface
    /// </summary>
    public class MotorController : IMotorController
    {
        private readonly ICommandSender commandSender;
        
        private Vector2 currentSpeed = Vector2.zero;
        
        public Vector2 CurrentSpeed => currentSpeed;
        
        public float MaxSpeed { get; set; } = 100f;
        public float SendRate { get; set; } = 10f; // Hz
        
        private float lastSendTime;
        private Vector2 lastSentSpeed;
        
        public MotorController(ICommandSender sender)
        {
            commandSender = sender;
            Debug.Log("[MotorController] Initialized");
        }
        
        public void SetSpeed(float left, float right)
        {
            left = Mathf.Clamp(left, -MaxSpeed, MaxSpeed);
            right = Mathf.Clamp(right, -MaxSpeed, MaxSpeed);
            
            currentSpeed = new Vector2(left, right);
        }
        
        public void Stop()
        {
            SetSpeed(0, 0);
            SendImmediately(); 
        }
        
        public void Update()
        {
            // Send at fixed rate
            if (Time.time - lastSendTime >= 1f / SendRate)
            {
                SendCurrentSpeed();
                lastSendTime = Time.time;
            }
        }
        
        private void SendCurrentSpeed()
        {
            if (!commandSender.IsReady) return;
            
            // Only send if speed changed (optimization)
            if (Vector2.Distance(currentSpeed, lastSentSpeed) < 0.1f)
            {
                return;
            }
            
            var command = new MotorCommand(currentSpeed.x, currentSpeed.y);
            commandSender.SendCommand(command.ToJson());
            
            lastSentSpeed = currentSpeed;
        }
        
        private void SendImmediately()
        {
            if (!commandSender.IsReady) return;
            
            var command = new MotorCommand(currentSpeed.x, currentSpeed.y);
            commandSender.SendCommand(command.ToJson());
            
            lastSentSpeed = currentSpeed;
            lastSendTime = Time.time;
        }
    }
}