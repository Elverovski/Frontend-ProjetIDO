using System;
using UnityEngine;

namespace Robot.Control.Models
{
    /// <summary>
    /// Represents a servo position with pan and tilt angles
    /// </summary>
    [Serializable]
    public struct ServoPosition
    {
        public float pan;
        public float tilt;
        
        public ServoPosition(float panAngle, float tiltAngle)
        {
            pan = panAngle;
            tilt = tiltAngle;
        }
        
        public Vector2 ToVector2() => new Vector2(pan, tilt);
        
        public static ServoPosition FromVector2(Vector2 v) => new ServoPosition(v.x, v.y);
        
        public override string ToString() => $"Pan: {pan:F1}°, Tilt: {tilt:F1}°";
    }
}