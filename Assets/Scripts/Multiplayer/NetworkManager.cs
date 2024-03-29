using RiptideNetworking;
using RiptideNetworking.Utils;
using System;
using System.Globalization;
using UnityEngine;

public enum ServerToClientId : ushort {
    // game state syncs
    gamestateWaiting = 1,
    gamestateExplore,
    gamestateEscape,
    gamestateEnded,
    syncClock,
    playerConnected,
    playerDisconnected,
    playerActions,

    // game actions
    roomPlaced,
    characterSpawned,
    characterMoved,
    gateRemoved,
    tileChanged,
    timerFlipped,
    pinged,
}

public enum ClientToServerId : ushort {
    // game state syncs
    name = 1,
    startGame,

    // game actions
    moveCharacter,
    explore,
    teleport,
    vent,
    flipTimer,
    ping,
}

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _singleton;
    public static NetworkManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    public Client Client { get; private set; }

    [SerializeField] private string ip;
    [SerializeField] private ushort port;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Client = new Client();
        Client.Connected += DidConnect;
        Client.ConnectionFailed += FailedToConnect;
        Client.ClientDisconnected += PlayerLeft;
        Client.Disconnected += DidDisconnect;
    }

    private void FixedUpdate()
    {
        Client.Tick();
    }

    private void OnApplicationQuit()
    {
        Client.Disconnect();
    }
    
    public void setIp(string ip, string port) {
        this.ip = ip;
        if (ushort.TryParse(port, out ushort outPort)) this.port = outPort;
    }

    public void Connect()
    {
        Client.Connect($"{ip}:{port}");
    }

    private void DidConnect(object sender, EventArgs e)
    {
        UIController.SendName();
    }

    private void FailedToConnect(object sender, EventArgs e)
    {
        UIController.showConnectUI();
    }
    
    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        UIController.disconnectPlayer(e.Id);
    }

    private void DidDisconnect(object sender, EventArgs e)
    {
        UIController.disconnectAllPlayers();
        UIController.showConnectUI();
        GameController.clearGameBoard();
    }
}
