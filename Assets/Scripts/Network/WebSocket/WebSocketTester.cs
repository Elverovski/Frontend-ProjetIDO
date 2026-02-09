using UnityEngine;
using Network.WebSocket;
using Network.WebSocket.Models;

public class WebSocketTester : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string username = "frontend";
    [SerializeField] private string password = "1234";

    [Header("Auto")]
    [SerializeField] private bool autoConnect = true;
    [SerializeField] private bool autoLogin = true;

    private WebSocketManager wsManager;

    void Start()
    {
        Debug.Log("========================================");
        Debug.Log("[TESTER] VR Drive Car - WebSocket Test");
        Debug.Log("========================================");

        wsManager = WebSocketManager.Instance;

        // Register WebSocket events
        wsManager.OnConnected += OnConnected;
        wsManager.OnAuthenticated += OnAuthenticated;
        wsManager.OnDisconnected += OnDisconnected;
        wsManager.OnError += OnError;

        // Register signaling events if ready
        if (wsManager.Signaling != null)
        {
            wsManager.Signaling.OnOfferReceived += OnOfferReceived;
            wsManager.Signaling.OnAnswerReceived += OnAnswerReceived;
            wsManager.Signaling.OnICECandidateReceived += OnICEReceived;
            wsManager.Signaling.OnRobotConnected += OnRobotConnected;
        }

        Debug.Log("[TESTER] Keyboard Controls:");
        Debug.Log("  C - Connect");
        Debug.Log("  L - Login");
        Debug.Log("  D - Disconnect");
        Debug.Log("========================================\n");

        // Auto connect
        if (autoConnect)
        {
            wsManager.Connect();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)) wsManager.Connect();
        if (Input.GetKeyDown(KeyCode.L)) wsManager.Login(username, password);
        if (Input.GetKeyDown(KeyCode.D)) wsManager.Disconnect();
    }

    // Called when connected
    void OnConnected()
    {
        Debug.Log("========================================");
        Debug.Log("[TESTER] CONNECTED TO SERVER");
        Debug.Log("========================================");

        // Auto login after 1 second
        if (autoLogin) Invoke(nameof(AutoLogin), 1f);
    }

    void AutoLogin()
    {
        if (!wsManager.IsAuthenticated)
        {
            wsManager.Login(username, password);
        }
    }

    // Called when authenticated
    void OnAuthenticated()
    {
        Debug.Log("========================================");
        Debug.Log("[TESTER] AUTHENTICATED");
        Debug.Log("[TESTER] Ready for WebRTC signaling");
        Debug.Log("========================================");
    }

    // Called when disconnected
    void OnDisconnected()
    {
        Debug.Log("========================================");
        Debug.Log("[TESTER] DISCONNECTED");
        Debug.Log("========================================");
    }

    // Called on error
    void OnError(string error)
    {
        Debug.LogError("========================================");
        Debug.LogError($"[TESTER] ERROR: {error}");
        Debug.LogError("========================================");
    }

    // Signaling event handlers
    void OnOfferReceived(WebRTCOffer offer)
    {
        Debug.Log($"[TESTER] OFFER received from {offer.from}");
    }

    void OnAnswerReceived(WebRTCAnswer answer)
    {
        Debug.Log($"[TESTER] ANSWER received from {answer.from}");
    }

    void OnICEReceived(ICECandidate candidate)
    {
        Debug.Log($"[TESTER] ICE candidate received from {candidate.from}");
    }

    void OnRobotConnected(string robotName)
    {
        Debug.Log("========================================");
        Debug.Log($"[TESTER] ROBOT AVAILABLE: {robotName}");
        Debug.Log("[TESTER] Can now initiate WebRTC connection");
        Debug.Log("========================================");
    }

    // Unregister events on destroy
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
