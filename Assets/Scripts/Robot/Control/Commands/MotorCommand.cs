using System;
using UnityEngine;

namespace Robot.Control.Commands
{
    /// <summary>
    /// Command to control motor speeds
    /// JSON format: {"type":"motor","left":50.0,"right":50.0}
    /// </summary>
    [Serializable]
    public class MotorCommand : BaseCommand
    {
        [SerializeField] public float left;
        [SerializeField] public float right;
        
        public MotorCommand(float leftSpeed, float rightSpeed)
        {
            type = "motor";
            left = leftSpeed;
            right = rightSpeed;
        }
        
        public override string ToString() => $"MotorCommand: Left={left:F1}, Right={right:F1}";
    }
}