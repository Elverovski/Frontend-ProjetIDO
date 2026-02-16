using UnityEngine;
using Network.WebSocket;
using Network.WebSocket.Interfaces;
using Network.WebSocket.Models;

namespace Network.WebSocket.Test
{
    /// <summary>
    /// Test script for WebSocket connection functionality.
    /// Tests connection, authentication, and signaling events.
    /// Used for debugging and verifying WebSocket setup.
    /// </summary>
    public class WebSocketTester : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private string username = "frontend";
        [SerializeField] private string password = "1234";

        [Header("Auto")]
        [SerializeField] private bool autoConnect = true;
        [SerializeField] private bool autoLogin = true;

        private IWebSocketManager wsManager;

        void Start()
        {
            Debug.Log("========================================");
            Debug.Log("[TESTER] VRDrive - WebSocket Test");
            Debug.Log("========================================");

            wsManager = WebSocketManager.Instance;

            RegisterEvents();
            PrintControls();

            // Auto connect if enabled
            if (autoConnect)
            {
                wsManager.Connect();
            }
        }

        /// <summary>
        /// Subscribes to WebSocket events for testing.
        /// </summary>
        private void RegisterEvents()
        {
            wsManager.OnConnected += OnConnected;
            wsManager.OnAuthenticated += OnAuthenticated;
            wsManager.OnDisconnected += OnDisconnected;
            wsManager.OnError += OnError;

            // Subscribe to signaling events if available
            if (wsManager.Signaling != null)
            {
                wsManager.Signaling.OnOfferReceived += OnOfferReceived;
                wsManager.Signaling.OnAnswerReceived += OnAnswerReceived;
                wsManager.Signaling.OnICECandidateReceived += OnICEReceived;
                wsManager.Signaling.OnRobotConnected += OnRobotConnected;
            }
        }

        /// <summary>
        /// Displays available keyboard controls in console.
        /// </summary>
        private void PrintControls()
        {
            Debug.Log("[TESTER] Keyboard Controls:");
            Debug.Log("  C - Connect");
            Debug.Log("  L - Login");
            Debug.Log("  D - Disconnect");
            Debug.Log("========================================\n");
        }

        void Update()
        {
            // Keyboard controls for testing
            if (Input.GetKeyDown(KeyCode.C)) wsManager.Connect();
            if (Input.GetKeyDown(KeyCode.L)) wsManager.Login(username, password);
            if (Input.GetKeyDown(KeyCode.D)) wsManager.Disconnect();
        }

        #region Event Handlers

        /// <summary>
        /// Called when WebSocket connection is established.
        /// Auto-logins if enabled.
        /// </summary>
        void OnConnected()
        {
            Debug.Log("========================================");
            Debug.Log("[TESTER] CONNECTED TO SERVER");
            Debug.Log("========================================");

            // Auto login after 1 second
            if (autoLogin) Invoke(nameof(AutoLogin), 1f);
        }

        /// <summary>
        /// Performs automatic login if user is not authenticated.
        /// </summary>
        void AutoLogin()
        {
            if (!wsManager.IsAuthenticated)
            {
                wsManager.Login(username, password);
            }
        }

        /// <summary>
        /// Called when authentication is successful.
        /// </summary>
        void OnAuthenticated()
        {
            Debug.Log("========================================");
            Debug.Log("[TESTER] AUTHENTICATED");
            Debug.Log("[TESTER] Ready for WebRTC signaling");
            Debug.Log("========================================");
        }

        /// <summary>
        /// Called when WebSocket connection is lost.
        /// </summary>
        void OnDisconnected()
        {
            Debug.Log("========================================");
            Debug.Log("[TESTER] DISCONNECTED");
            Debug.Log("========================================");
        }

        /// <summary>
        /// Called when an error occurs during authentication or connection.
        /// </summary>
        void OnError(string error)
        {
            Debug.LogError("========================================");
            Debug.LogError($"[TESTER] ERROR: {error}");
            Debug.LogError("========================================");
        }

        /// <summary>
        /// Called when a WebRTC offer is received via signaling.
        /// </summary>
        void OnOfferReceived(WebRTCOffer offer)
        {
            Debug.Log($"[TESTER] OFFER received from {offer.from}");
        }

        /// <summary>
        /// Called when a WebRTC answer is received via signaling.
        /// </summary>
        void OnAnswerReceived(WebRTCAnswer answer)
        {
            Debug.Log($"[TESTER] ANSWER received from {answer.from}");
        }

        /// <summary>
        /// Called when an ICE candidate is received via signaling.
        /// </summary>
        void OnICEReceived(ICECandidate candidate)
        {
            Debug.Log($"[TESTER] ICE candidate received from {candidate.from}");
        }

        /// <summary>
        /// Called when a robot connects to the WebSocket server.
        /// Indicates robot is available for WebRTC connection.
        /// </summary>
        void OnRobotConnected(string robotName)
        {
            Debug.Log("========================================");
            Debug.Log($"[TESTER] ROBOT AVAILABLE: {robotName}");
            Debug.Log("[TESTER] Can now initiate WebRTC connection");
            Debug.Log("========================================");
        }

        #endregion

        /// <summary>
        /// Cleanup: unsubscribes from all events.
        /// </summary>
        void OnDestroy()
        {
            if (wsManager != null)
            {
                wsManager.OnConnected -= OnConnected;
                wsManager.OnAuthenticated -= OnAuthenticated;
                wsManager.OnDisconnected -= OnDisconnected;
                wsManager.OnError -= OnError;

                if (wsManager.Signaling != null)
                {
                    wsManager.Signaling.OnOfferReceived -= OnOfferReceived;
                    wsManager.Signaling.OnAnswerReceived -= OnAnswerReceived;
                    wsManager.Signaling.OnICECandidateReceived -= OnICEReceived;
                    wsManager.Signaling.OnRobotConnected -= OnRobotConnected;
                }
            }
        }
    }
}