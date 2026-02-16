using UnityEngine;
using Robot.Control.Interfaces;
using Network.WebRTC;

namespace Robot.Control.Controllers
{
    /// <summary>
    /// Sends commands to robot via WebRTC Data Channel
    /// Implements ICommandSender interface
    /// </summary>
    public class CommandSender : ICommandSender
    {
        private readonly WebRTCManager webRTCManager;
        
        public bool IsReady => webRTCManager?.IsDataChannelReady ?? false;
        
        public CommandSender(WebRTCManager manager)
        {
            webRTCManager = manager;
            Debug.Log("[CommandSender] Initialized");
        }
        
        public void SendCommand(string jsonCommand)
        {
            if (!IsReady)
            {
                Debug.LogWarning("[CommandSender] Cannot send - data channel not ready");
                return;
            }
            
            webRTCManager.DataChannel.SendCommand(jsonCommand);
        }
    }
}