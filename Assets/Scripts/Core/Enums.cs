
namespace Core
{
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Authenticating,
        Authenticated,
        Error
    }
    
    public enum VehicleMode
    {
        Stopped,
        Manual,
        Emergency
    }
    
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }
    
    public enum EmergencyStopReason
    {
        UserButton,
        Timeout,
        LowBattery,
        ConnectionLost,
        SystemError
    }
}