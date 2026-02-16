using System;
using UnityEngine;

namespace Robot.Control.Commands
{
    /// <summary>
    /// Command to control servo positions
    /// JSON format: {"type":"servo","pan":90.0,"tilt":90.0}
    /// </summary>
    [Serializable]
    public class ServoCommand : BaseCommand
    {
        [SerializeField] public float pan;
        [SerializeField] public float tilt;
        
        public ServoCommand(float panAngle, float tiltAngle)
        {
            type = "servo";
            pan = panAngle;
            tilt = tiltAngle;
        }
        
        public override string ToString() => $"ServoCommand: Pan={pan:F1}°, Tilt={tilt:F1}°";
    }
}