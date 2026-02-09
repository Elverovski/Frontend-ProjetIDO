using UnityEngine;
using Network.WebSocket;
using Network.WebSocket.Models;

public class WebSocketTester : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string serverUrl = "ws://localhost:3000";
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
        wsManager.SetServerUrl(serverUrl);
        
        wsManager.OnConnected += OnConnected;
        wsManager.OnAuthenticated += OnAuthenticated;
        wsManager.OnDisconnected += OnDisconnected;
        wsManager.OnError += OnError;
        
        wsManager.Signaling.OnOfferReceived += OnOfferReceived;
        wsManager.Signaling.OnAnswerReceived += OnAnswerReceived;
        wsManager.Signaling.OnICECandidateReceived += OnICEReceived;
        wsManager.Signaling.OnRobotConnected += OnRobotConnected;

        Debug.Log("[TESTER] Keyboard Controls:");
        Debug.Log("  C - Connect");
        Debug.Log("  L - Login");
        Debug.Log("  D - Disconnect");
        Debug.Log("========================================\n");

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

    void OnConnected()
    {
        Debug.Log("========================================");
        Debug.Log("[TESTER] CONNECTED TO SERVER");
        Debug.Log("========================================");
        
        if (autoLogin) Invoke(nameof(AutoLogin), 1f);
    }

    void AutoLogin()
    {
        if (!wsManager.IsAuthenticated)
        {
            wsManager.Login(username, password);
        }
    }

    void OnAuthenticated()
    {
        Debug.Log("========================================");
        Debug.Log("[TESTER] AUTHENTICATED");
        Debug.Log("[TESTER] Ready for WebRTC signaling");
        Debug.Log("========================================");
    }

    void OnDisconnected()
    {
        Debug.Log("========================================");
        Debug.Log("[TESTER] DISCONNECTED");
        Debug.Log("========================================");
    }

    void OnError(string error)
    {
        Debug.LogError("========================================");
        Debug.LogError($"[TESTER] ERROR: {error}");
        Debug.LogError("========================================");
    }

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

    void OnDestroy()
    {
        if (wsManager != null)
        {
            wsManager.OnConnected -= OnConnected;
            wsManager.OnAuthenticated -= OnAuthenticated;
            wsManager.OnDisconnected -= OnDisconnected;
            wsManager.OnError -= OnError;
        }
    }
}