using UnityEngine;
using Robot.Control.Interfaces;
using Network.WebRTC;

namespace Robot.Control.Controllers
{
    /// <summary>
    /// Main robot controller - manages all robot subsystems
    /// Singleton pattern for global access
    /// Implements IRobotController interface
    /// </summary>
    public class RobotController : MonoBehaviour, IRobotController
    {
        private static RobotController _instance;
        public static RobotController Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("RobotController");
                    _instance = go.AddComponent<RobotController>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        private ICommandSender commandSender;
        private ServoController servoController;
        private MotorController motorController;
        
        // Public accessors
        public IServoController Servo => servoController;
        public IMotorController Motor => motorController;
        public ICommandSender CommandSender => commandSender;
        
        public bool IsConnected => WebRTCManager.Instance?.IsConnected ?? false;
        
        // Settings
        [Header("Servo Settings")]
        [SerializeField] private Vector2 servoMinAngles = new Vector2(0, 45);
        [SerializeField] private Vector2 servoMaxAngles = new Vector2(180, 135);
        [SerializeField] private float servoSmoothSpeed = 5f;
        [SerializeField] private float servoSendRate = 20f;
        
        [Header("Motor Settings")]
        [SerializeField] private float motorMaxSpeed = 100f;
        [SerializeField] private float motorSendRate = 10f;
        
        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        void Start()
        {
            Initialize();
        }
        
        public void Initialize()
        {
            Debug.Log("[RobotController] Initializing...");
            
            // Create command sender
            commandSender = new CommandSender(WebRTCManager.Instance);
            
            // Create servo controller
            servoController = new ServoController(commandSender)
            {
                MinAngles = servoMinAngles,
                MaxAngles = servoMaxAngles,
                SmoothSpeed = servoSmoothSpeed,
                SendRate = servoSendRate
            };
            
            // Create motor controller
            motorController = new MotorController(commandSender)
            {
                MaxSpeed = motorMaxSpeed,
                SendRate = motorSendRate
            };
            
            Debug.Log("[RobotController] ✓ Initialized successfully");
        }
        
        void Update()
        {
            if (!IsConnected) return;
            servoController?.Update();
            motorController?.Update();
        }
        
        public void Shutdown()
        {
            motorController?.Stop();
            Debug.Log("[RobotController] Shutdown complete");
        }
        
        void OnDestroy()
        {
            Shutdown();
            
            if (_instance == this)
            {
                _instance = null;
            }
        }
        
        void OnApplicationQuit()
        {
            Shutdown();
        }
    }
}