using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEditor;
using TMPro;
using ExitGames.Client.Photon.StructWrapping;
using System;
using Random = UnityEngine.Random;
using Photon.Pun;
using System.Collections;

public class PhotonManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Photon Settings")]

    private Dictionary<PlayerRef, List<NetworkObject>> playerObjects = new Dictionary<PlayerRef, List<NetworkObject>>();
    [SerializeField] private NetworkRunner runner; // Se encarga de la comunicación en red
    [SerializeField] private NetworkSceneManagerDefault sceneManager; // Maneja el cambio de escenas
    [SerializeField] private NetworkObject playerPrefab; // Prefab del jugador con NetworkObject
    [SerializeField] private Transform[] spawnPoint;
    [SerializeField] Dictionary<PlayerRef, NetworkObject> players = new Dictionary<PlayerRef, NetworkObject>();

   public List<SessionInfo> availabelSesion = new List<SessionInfo>();// Sessiones disponinñles 
   //Si esta lista tiene sesiones dentro, Significa que hay sessiones disponibles, si esta vacias pues no hay sessiones disponibles
    public event Action onSessionListUpdated;
   
    public static PhotonManager _PhotonManager;
    public bool isHost;


    [Header("Create Lobby UI")]
    [SerializeField] private GameObject createLobbyPanel;
    [SerializeField] private TMP_InputField lobbyNameInput;
    [SerializeField] private TMP_InputField maxPlayersInput;


    [Header("Lobby & Waiting UI")]
    [SerializeField] private GameObject waitingPanel;   // Panel to show waiting
    [SerializeField] private TMP_Text waitingText;       // Text showing lobby info
    [SerializeField] private int minPlayersToStart = 2;  // Minimum required players
    [SerializeField] private int maxPlayers = 4;         // Max players per session

    private List<PlayerRef> currentPlayers = new List<PlayerRef>();
    public GameObject cameraObj;
    private void Awake()
    {
        if (_PhotonManager == null)
        {
            _PhotonManager = this;
          
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        runner.AddCallbacks(this);
       
       
    }

    public async void JoinLobby()
    {
        Debug.Log("Trying to join lobby...");

        var result = await runner.JoinSessionLobby(SessionLobby.ClientServer);

        Debug.Log("JoinLobby success: " + result.Ok);
    }
    public string localPlayerName;

    public void SetPlayerName(string name)
    {
        localPlayerName = name;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            JoinLobby();
        }
        CheckGameEnd(); // 👈 ADD THIS
    }
    [Header("UI")]
    [SerializeField] private Canvas mainCanvas; // Canvas que se apagará al entrar en la partida
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private GameObject mainMenuPanel;   // Panel-StartButtons
    [SerializeField] private GameObject lobbyPanel;      // Lobbies Panel


    [Header("Events")]
    [SerializeField] UnityEvent onPlayerJoinedToGame;// Los UnityEvent son llamadas que se hacen al invocar evento

    [Header("Score UI")]
    [SerializeField] private GameObject scorePrefab;
    [SerializeField] GameObject rival;

    bool rivalSpawned = false;
   
    public GameObject noLobbiesText;
    public void OnSesionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log("Sesion nueva");
        availabelSesion = sessionList; //Aqui Guardo la lista de sesion mas reciente
        onSessionListUpdated?.Invoke(); 
        
      
    }
    
    private async void StartGame(GameMode mode)
    {
       
        
        runner.ProvideInput = true; // Esto nos dice que el runner recibira y mandara inputs

        var scene = SceneRef.FromIndex(0);// Guardame una referencia a la escena 0

        var scenInfo = new NetworkSceneInfo();// Creo una variable que me va a guardar las escenas que voy a usar

        if (scene.IsValid)
        {
            scenInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }

        await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = RandomSessionName(6), // Este nombre es el interno que yo como desarrollador necesio entrar
            Scene = scene,
            SceneManager = sceneManager,
            IsVisible = true,
            
        });
    }

    public void CancelCreateLobby()
    {
        createLobbyPanel.SetActive(false);
        mainMenuPanel.SetActive(true);   // Apaga menú
    }
    [Header("Enemy Spawner Prefab")]
    [SerializeField] private EnemySpawner enemySpawnerPrefab;

    public async void ConfirmCreateLobby()
    {
        string lobbyName = lobbyNameInput.text;

        if (string.IsNullOrWhiteSpace(lobbyName))
            lobbyName = "Room_" + UnityEngine.Random.Range(1000, 9999);

        int maxPlayers = 4;

        if (int.TryParse(maxPlayersInput.text, out int parsed))
            maxPlayers = Mathf.Clamp(parsed, 2, 10);

        runner.ProvideInput = true;

        await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = lobbyName,
            PlayerCount = maxPlayers,
            IsVisible = true,
            IsOpen = true,
            SceneManager = sceneManager
        });
        Debug.Log("HOST STARTED SESSION: " + lobbyName);
        Debug.Log("IsServer: " + runner.IsServer);
        Debug.Log("Session Info Visible: " + runner.SessionInfo.IsVisible);
        // Spawn EnemySpawner as a networked object
        NetworkObject spawnerObj = runner.Spawn(enemySpawnerPrefab.GetComponent<NetworkObject>(), Vector3.zero, Quaternion.identity);
        //  spawnerObj.GetComponent<EnemySpawner>().Spawned();
        var spawner = spawnerObj.GetComponent<EnemySpawner>();
        if (spawner != null)
            spawner.Spawned();
        createLobbyPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        cameraObj.SetActive(false);
    }
    
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (!runner.IsServer) return;

        // Find the EnemySpawner in the loaded scene
        EnemySpawner spawner = GameObject.FindObjectOfType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.Spawned(); // initialize spawn points if needed
        }
    }




    private void CheckStartGame()
    {
        if (!runner.IsServer) return;

        if (currentPlayers.Count == minPlayersToStart) // EXACTLY 2 players
        {
            Debug.Log("2 players joined. Starting game...");

            waitingPanel.SetActive(false);
            cameraObj.SetActive(false);

            StartGameplay(); // ✅ Correct
        }
    }
    private void StartGameplay()
    {
        Debug.Log("Gameplay Started!");

        gameStarted = true; // ✅ IMPORTANT

       cameraObj.SetActive(false);

        foreach (var kvp in players)
        {
            var controller = kvp.Value.GetComponent<MovementController>();
            if (controller != null)
                controller.enabled = true;
        }

        if (runner.IsServer)
        {
            EnemySpawner spawner = FindObjectOfType<EnemySpawner>();

            if (spawner == null && enemySpawnerPrefab != null)
            {
                runner.Spawn(
                    enemySpawnerPrefab, // ✅ FIXED
                    Vector3.zero,
                    Quaternion.identity
                );
            }
        }
    }

    public void NotifyEnemiesSpawned()
    {
        Debug.Log("Enemies started spawning!");
        enemiesSpawned = true;
    }
 
    public bool enemiesSpawned = false;
    private bool gameStarted = false;
    private bool gameEnded = false;
    public void SearchLobbies()
    {
        Debug.Log("Boton presionado");
     mainMenuPanel.SetActive(false);
     lobbyPanel.SetActive(true);
        JoinLobby();
     
    }
    
    public void BackToMainMenu()
    {
     lobbyPanel.SetActive(false);
     mainMenuPanel.SetActive(true);
    }
    public async void JoinSession(SessionInfo session)
    {
        if (session == null)
        {
            Debug.LogError("Session is null! Cannot join.");
            return;
        }

        Debug.Log("Joining session: " + session.Name);
        mainMenuPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        cameraObj.SetActive(false);
        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = session.Name,
            SceneManager = sceneManager
        });

        if (result.Ok)
        {
            Debug.Log("Joined successfully!");
            
        }
           
        else
            Debug.LogWarning("Failed to join session: " + result.ShutdownReason);
    }
  
    public void JoinLobbyAsClient()
    {
      runner.AddCallbacks(this);
      runner.JoinSessionLobby(SessionLobby.ClientServer);
    }
    public void JoinLobbyAsHost()
    {
        
    }
    public void StartGameAsClient()
    {
        Debug.Log("Iniciando como CLIENTE...");
        StartGame(GameMode.Client);
    }
    
 
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player, NetworkObject playerObject)
    {
        // Despawn all registered objects
        if (playerObjects.TryGetValue(player, out List<NetworkObject> objects))
        {
            foreach (var obj in objects)
            {
                if (obj != null && obj.IsValid)
                    runner.Despawn(obj);
            }

            playerObjects.Remove(player);
        }

        // Remove from main players dictionary if used
        if (players.ContainsKey(player))
            players.Remove(player);

        Debug.Log($"Cleaned up all objects for player {player.PlayerId}");
    }
  
    public void OnInput(NetworkRunner runner, NetworkInput input)
   {
     if (InputManager.Instance == null || Camera.main == null)
        return;

     NetworkInputData data = new NetworkInputData()
     {
        move = InputManager.Instance.GetMoveInput(),
        look = InputManager.Instance.GetMouseDelta(),
        isRunning = InputManager.Instance.WasRunInputPressed(),
        yRotation = Camera.main.transform.eulerAngles.y,
        xRotation = (Camera.main.transform.localEulerAngles.x > 180)
            ? Camera.main.transform.localEulerAngles.x - 360
            : Camera.main.transform.localEulerAngles.x,
        shoot = InputManager.Instance.ShootInputPresed()
     };

      input.Set(data);
   }

    #region Photon Callbacks

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    //El parametro sessioniList es una lista de sesiones, esta se acctualiza cada vez que se crea i se ekimina una sesion.
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log("OnSessionListUpdated CALLED. Sessions: " + sessionList.Count);

        availabelSesion = sessionList;
        onSessionListUpdated?.Invoke();
    }
    //public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken token) { }
   // public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    #endregion

    #region Host Lobby & Start Game

    // Called when host presses "Start Game" button
    public void StartGameAsHost()
    {
        // Ensure player is logged in (PlayFab)
        if (PlayfabManager._PlayfabManager == null || !PlayfabManager._PlayfabManager.IsLoggedIn)
        {
            Debug.LogError("You must login first!");
            return;
        }

        // Generate random 6-character session name
        string sessionName = RandomSessionName(6);

        // Show waiting panel
        waitingPanel.SetActive(true);
        UpdateWaitingText();

        // Hide main menu and lobby UI
        mainMenuPanel.SetActive(false);
        lobbyPanel.SetActive(false);

        // Start hosting session
        runner.ProvideInput = true;
        runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = sessionName,
            PlayerCount = maxPlayers,
            IsVisible = true,
            IsOpen = true,
            SceneManager = sceneManager
        });

        Debug.Log($"Hosting session {sessionName}. Waiting for players...");

        // Reset and add host to player list
        //currentPlayers.Clear();
        //if (!currentPlayers.Contains(runner.LocalPlayer))
        //    currentPlayers.Add(runner.LocalPlayer);

        UpdateWaitingText();
    }


    private Dictionary<PlayerRef, string> playerNames = new Dictionary<PlayerRef, string>();

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!currentPlayers.Contains(player))
            currentPlayers.Add(player);

        UpdateWaitingText();

        if (runner.IsServer)
            SpawnPlayer(player);

        // ✅ FIX: Delay sending name (very important)
        if (player == runner.LocalPlayer)
        {
            StartCoroutine(SendNameDelayed());
        }

        CheckStartGame();
    }
    private IEnumerator SendNameDelayed()
    {
        yield return new WaitForSeconds(1f); // wait for spawn + registration

        SendMyPlayfabName();
    }
    // Spawning player object + score object
    private void SpawnPlayer(PlayerRef player)
    {
        int randomSpawn = Random.Range(0, spawnPoint.Length);

        // Spawn player object
        NetworkObject networkPlayer = runner.Spawn(playerPrefab, spawnPoint[randomSpawn].position, spawnPoint[randomSpawn].rotation, player);
        players[player] = networkPlayer;

        // Spawn score object
        NetworkObject scoreObject = runner.Spawn(scorePrefab, Vector3.zero, Quaternion.identity, player);
        NetworkScoreEntry scoreEntry = scoreObject.GetComponent<NetworkScoreEntry>();
        scoreEntry.SetOwner(player);

        // TEMPORARY fallback name
        scoreEntry.PlayerName = $"Player{player.PlayerId}";
        scoreEntry.Score = 0;

        // ✅ If this is the local player, immediately send name
        if (runner.LocalPlayer == player)
        {
            string myName = PlayfabManager._PlayfabManager?.playerName ?? $"Player{player.PlayerId}";
            RpcSendPlayerName(player, myName); // This sets the NetworkScoreEntry for host/client
        }
    }

    // Called by client to send PlayFab name
    public void SendMyPlayfabName()
    {
        if (PlayfabManager._PlayfabManager == null) return;

        string myName = PlayfabManager._PlayfabManager.playerName ?? $"Player{runner.LocalPlayer.PlayerId}";
        PhotonManager._PhotonManager.RpcSendPlayerName(runner.LocalPlayer, myName);
    }

    // Host receives client name
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcSendPlayerName(PlayerRef sender, string playerName)
    {
        if (!runner.IsServer) return;

        playerNames[sender] = playerName;

        NetworkScoreEntry.SetPlayerName(sender, playerName);

        Debug.Log($"[Host] Registered player {sender.PlayerId} name: {playerName}");
    }
    // Update waiting UI
    private void UpdateWaitingText()
    {
        if (waitingText != null)
            waitingText.text = $"Waiting for players...\n({currentPlayers.Count}/{minPlayersToStart}) joined";
    }

    // Check if host can start game scene

    // Called when a player leaves
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (currentPlayers.Contains(player))
            currentPlayers.Remove(player);

        UpdateWaitingText();

        // Despawn player objects
        if (players.TryGetValue(player, out NetworkObject obj))
        {
            runner.Despawn(obj);
            players.Remove(player);
        }

        if (runner.IsServer && currentPlayers.Count < minPlayersToStart)
            Debug.Log("Not enough players. Waiting for more...");
    }

    // Generates a random 6-character session name
    private string RandomSessionName(int sessionNameLength)
    {
        string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string sessionName = "";

        for (int i = 0; i < sessionNameLength; i++)
            sessionName += characters[Random.Range(0, characters.Length)];

        return sessionName;
    }

    #endregion

    #region GameEnd
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private TMP_Text resultText;

    private void CheckGameEnd()
    {
        if (!runner.IsServer || gameEnded || !gameStarted) return;

        // ⛔ IMPORTANT: don't check until enemies actually spawned
        if (!enemiesSpawned) return;

        if (Health.allObjectives.Count == 0)
        {
            Debug.Log("All enemies are dead!");
            gameEnded = true;

            DeclareWinner();
        }
    }
    private void DeclareWinner()
    {
        PlayerRef winner = default;
        int highestScore = int.MinValue;

        foreach (var kvp in NetworkScoreEntry.AllScores)
        {
            if (kvp.Value.Score > highestScore)
            {
                highestScore = kvp.Value.Score;
                winner = kvp.Key;
            }
        }

        Debug.Log($"Winner is Player {winner.PlayerId} with score {highestScore}");

        Rpc_ShowEndGame(winner, highestScore);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_ShowEndGame(PlayerRef winner, int score)
    {
        Debug.Log($"GAME OVER - Winner: {winner.PlayerId}");

        if (endGamePanel != null)
            endGamePanel.SetActive(true);

        if (resultText != null)
        {
            if (winner == runner.LocalPlayer)
                resultText.text = $"YOU WIN! Score: {score}";
            else
                resultText.text = $"YOU LOSE! Winner: Player {winner.PlayerId}";
        }
        CursorManager.Instance.CursorUnlock();
    }
    public void RestartGame()
    {
        if (!runner.IsServer) return;

        gameEnded = false;

        runner.LoadScene(SceneRef.FromIndex(0));
    }
    public void ExitGame()
    {
        runner.Shutdown();
    }
    #endregion
}