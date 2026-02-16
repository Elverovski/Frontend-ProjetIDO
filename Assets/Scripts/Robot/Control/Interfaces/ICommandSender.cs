namespace Robot.Control.Interfaces
{
    /// <summary>
    /// Interface for sending commands to the robot
    /// Abstracts away the network layer (WebRTC, WebSocket, Serial, etc.)
    /// </summary>
    public interface ICommandSender
    {
        /// <summary>
        /// Send a JSON command to the robot
        /// </summary>
        void SendCommand(string jsonCommand);
        
        /// <summary>
        /// Check if command sender is ready to send
        /// </summary>
        bool IsReady { get; }
    }
}