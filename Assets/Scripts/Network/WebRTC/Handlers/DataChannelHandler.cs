using System;
using System.Text;
using UnityEngine;
using Unity.WebRTC;
using Network.WebRTC.Core;
using Network.WebRTC.Models;

namespace Network.WebRTC.Handlers
{
    /// <summary>
    /// Handles data channels for commands and telemetry
    /// </summary>
    public class DataChannelHandler
    {
        private RTCClient rtcClient;
        private RTCDataChannel commandChannel;
        private RTCDataChannel telemetryChannel;

        // Events
        public event Action<TelemetryData> OnTelemetryReceived;
        public event Action OnDataChannelReady;

        private bool isReady = false;
        public bool IsReady => isReady;

        public DataChannelHandler(RTCClient client)
        {
            rtcClient = client;
            rtcClient.OnDataChannel += HandleIncomingDataChannel;
        }

        // Create data channels (Unity as offerer)
        public void CreateDataChannels()
        {
            Debug.Log("[DATA CHANNEL] Creating data channels");

            // Channel for sending commands
            commandChannel = rtcClient.CreateDataChannel("commands");
            SetupDataChannel(commandChannel, "COMMAND");

            // Channel for receiving telemetry
            telemetryChannel = rtcClient.CreateDataChannel("telemetry");
            SetupDataChannel(telemetryChannel, "TELEMETRY");
        }

        // Handle incoming data channels (Pi as offerer)
        private void HandleIncomingDataChannel(RTCDataChannel channel)
        {
            Debug.Log($"[DATA CHANNEL] Incoming channel: {channel.Label}");

            if (channel.Label == "commands")
            {
                commandChannel = channel;
                SetupDataChannel(commandChannel, "COMMAND");
            }
            else if (channel.Label == "telemetry")
            {
                telemetryChannel = channel;
                SetupDataChannel(telemetryChannel, "TELEMETRY");
            }
        }

        // Setup channel events
        private void SetupDataChannel(RTCDataChannel channel, string channelType)
        {
            channel.OnOpen = () => 
            {
                Debug.Log($"[DATA CHANNEL] {channelType} channel opened");
                CheckIfReady();
            };

            channel.OnClose = () => 
            {
                Debug.Log($"[DATA CHANNEL] {channelType} channel closed");
                isReady = false;
            };

            channel.OnMessage = (bytes) => 
            {
                if (channelType == "TELEMETRY")
                {
                    HandleTelemetryMessage(bytes);
                }
            };
        }

        // Check if both channels are ready
        private void CheckIfReady()
        {
            if (commandChannel?.ReadyState == RTCDataChannelState.Open &&
                telemetryChannel?.ReadyState == RTCDataChannelState.Open)
            {
                isReady = true;
                Debug.Log("[DATA CHANNEL] All channels ready");
                OnDataChannelReady?.Invoke();
            }
        }

        // Send command to vehicle
        public void SendVehicleCommand(VehicleCommand command)
        {
            if (commandChannel?.ReadyState != RTCDataChannelState.Open)
            {
                Debug.LogWarning("[DATA CHANNEL] Command channel not ready");
                return;
            }

            command.timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string json = JsonUtility.ToJson(command);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            commandChannel.Send(bytes);
        }

        // Helper: Send movement command
        public void SendMovementCommand(float throttle, float steering, float brake = 0f)
        {
            var command = new VehicleCommand
            {
                type = "movement",
                movement = new MovementData
                {
                    throttle = throttle,
                    steering = steering,
                    brake = brake
                }
            };

            SendVehicleCommand(command);
        }

        // Helper: Send camera command
        public void SendCameraCommand(float pan, float tilt)
        {
            var command = new VehicleCommand
            {
                type = "camera",
                camera = new CameraData
                {
                    pan = pan,
                    tilt = tilt
                }
            };

            SendVehicleCommand(command);
        }

        // Send emergency stop
        public void SendEmergencyStop(string reason)
        {
            if (commandChannel?.ReadyState != RTCDataChannelState.Open)
            {
                Debug.LogWarning("[DATA CHANNEL] Command channel not ready for emergency stop");
                return;
            }

            var stopCommand = new EmergencyStopCommand
            {
                activate = true,
                reason = reason,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            string json = JsonUtility.ToJson(stopCommand);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            commandChannel.Send(bytes);
            Debug.Log($"[DATA CHANNEL] Emergency stop sent: {reason}");
        }

        // Handle telemetry messages
        private void HandleTelemetryMessage(byte[] bytes)
        {
            try
            {
                string json = Encoding.UTF8.GetString(bytes);
                TelemetryData telemetry = JsonUtility.FromJson<TelemetryData>(json);
                
                OnTelemetryReceived?.Invoke(telemetry);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DATA CHANNEL] Failed to parse telemetry: {ex.Message}");
            }
        }

        // Dispose channels
        public void Dispose()
        {
            commandChannel?.Close();
            telemetryChannel?.Close();
            
            rtcClient.OnDataChannel -= HandleIncomingDataChannel;
        }
    }
}
