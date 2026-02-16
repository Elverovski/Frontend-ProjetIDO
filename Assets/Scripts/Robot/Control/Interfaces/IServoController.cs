using UnityEngine;

namespace Robot.Control.Interfaces
{
    /// <summary>
    /// Interface for controlling robot servos (pan and tilt)
    /// </summary>
    public interface IServoController
    {
        /// <summary>
        /// Set both pan and tilt angles at once
        /// </summary>
        void SetPosition(float pan, float tilt);
        
        /// <summary>
        /// Set only pan (horizontal) angle
        /// </summary>
        void SetPan(float angle);
        
        /// <summary>
        /// Set only tilt (vertical) angle
        /// </summary>
        void SetTilt(float angle);
        
        /// <summary>
        /// Current servo position (x=pan, y=tilt)
        /// </summary>
        Vector2 CurrentPosition { get; }
        
        /// <summary>
        /// Minimum allowed angles (x=pan, y=tilt)
        /// </summary>
        Vector2 MinAngles { get; set; }
        
        /// <summary>
        /// Maximum allowed angles (x=pan, y=tilt)
        /// </summary>
        Vector2 MaxAngles { get; set; }
        
        /// <summary>
        /// Update method - call every frame
        /// </summary>
        void Update();
    }
}