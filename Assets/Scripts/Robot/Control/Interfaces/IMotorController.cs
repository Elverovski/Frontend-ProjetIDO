using UnityEngine;

namespace Robot.Control.Interfaces
{
    /// <summary>
    /// Interface for controlling robot motors (left and right)
    /// </summary>
    public interface IMotorController
    {
        /// <summary>
        /// Set motor speeds for left and right motors
        /// </summary>
        void SetSpeed(float left, float right);
        
        /// <summary>
        /// Stop all motors
        /// </summary>
        void Stop();
        
        /// <summary>
        /// Current motor speeds (x=left, y=right)
        /// </summary>
        Vector2 CurrentSpeed { get; }
        
        /// <summary>
        /// Update method - call every frame
        /// </summary>
        void Update();
    }
}