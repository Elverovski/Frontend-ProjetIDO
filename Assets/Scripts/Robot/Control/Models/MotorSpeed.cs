using System;
using UnityEngine;

namespace Robot.Control.Models
{
    /// <summary>
    /// Represents motor speeds for left and right motors
    /// </summary>
    [Serializable]
    public struct MotorSpeed
    {
        public float left;
        public float right;
        
        public MotorSpeed(float leftSpeed, float rightSpeed)
        {
            left = leftSpeed;
            right = rightSpeed;
        }
        
        public Vector2 ToVector2() => new Vector2(left, right);
        
        public static MotorSpeed FromVector2(Vector2 v) => new MotorSpeed(v.x, v.y);
        
        public override string ToString() => $"Left: {left:F1}, Right: {right:F1}";
    }
}