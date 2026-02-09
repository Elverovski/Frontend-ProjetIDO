using System;

namespace Network.WebRTC.Models
{
    /// <summary>
    /// Telemetry data from the vehicle or device
    /// </summary>
    [Serializable]
    public class TelemetryData
    {
        public BatteryData battery;   // Battery information
        public GPSData gps;           // GPS information
        public SensorData sensors;    // Sensor readings
        public SystemData system;     // System performance data
        public long timestamp;        // Unix timestamp of the reading
    }

    // Battery information
    [Serializable]
    public class BatteryData
    {
        public float voltage;        // Battery voltage in volts
        public float current;        // Current in amperes
        public float percentage;     // Charge level (0-100%)
        public float temperature;    // Battery temperature in Celsius
        public string status;        // Status, e.g., "charging", "discharging"
    }

    // GPS information
    [Serializable]
    public class GPSData
    {
        public double latitude;      // Latitude in degrees
        public double longitude;     // Longitude in degrees
        public float altitude;       // Altitude in meters
        public float speed;          // Speed in m/s
        public float heading;        // Heading in degrees
        public int satellites;       // Number of satellites connected
        public float accuracy;       // Accuracy in meters
    }

    // Sensor readings
    [Serializable]
    public class SensorData
    {
        public float[] accelerometer;    // [x, y, z] acceleration in m/s2
        public float[] gyroscope;        // [x, y, z] rotation rate in rad/s
        public float[] magnetometer;     // [x, y, z] magnetic field in μT
        public float temperature;        // Ambient temperature in Celsius
        public float humidity;           // Relative humidity in %
    }

    // System performance and network data
    [Serializable]
    public class SystemData
    {
        public float cpuUsage;           // CPU usage in %
        public float memoryUsage;        // Memory usage in %
        public float cpuTemperature;     // CPU temperature in Celsius
        public int signalStrength;       // Network signal strength
        public string networkType;       // Network type, e.g., "WiFi", "LTE"
    }
}
