namespace Robot.Control.Interfaces
{
    /// <summary>
    /// Main interface for robot control system
    /// Provides access to all robot subsystems
    /// </summary>
    public interface IRobotController
    {
        IServoController Servo { get; }
        IMotorController Motor { get; }
        ICommandSender CommandSender { get; }
        
        bool IsConnected { get; }
        
        void Initialize();
        void Shutdown();
    }
}