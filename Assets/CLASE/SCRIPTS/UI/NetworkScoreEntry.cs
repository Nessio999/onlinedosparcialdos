using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;

public class NetworkScoreEntry : NetworkBehaviour
{
    public static Dictionary<PlayerRef, NetworkScoreEntry> AllScores = new Dictionary<PlayerRef, NetworkScoreEntry>();
    [Header("Stats")]

    [SerializeField] private GameObject statsPrefab;
    // <--- Add this line
    private static Dictionary<PlayerRef, string> pendingNames = new Dictionary<PlayerRef, string>();

    private GameObject _localUIInstance;
    private TextMeshProUGUI _scoreText;

    [Networked, OnChangedRender(nameof(OnDataChanged))]
    public int Score {get; set;}
    [Networked, OnChangedRender(nameof(OnDataChanged))]
    public NetworkString<_16> PlayerName {get; set;}

    [Networked] public PlayerRef OwnerPlayer {get; set;}

    //public override void Spawned()
    //{
    //    if (OwnerPlayer == PlayerRef.None) return;

    //    if (AllScores.ContainsKey(OwnerPlayer))
    //    {
    //        Debug.LogWarning($"Player {OwnerPlayer} ya tiene una entrada de puntuación registrada.");
    //    }
    //    else
    //    {
    //        AllScores.Add(OwnerPlayer, this);
    //    }
    //    InitializeUI();
    //}
    public override void Spawned()
    {
        if (Object.HasInputAuthority) // THIS PLAYER OWNS THIS OBJECT
        {
            string myName = PlayfabManager._PlayfabManager?.playerName
                            ?? $"Player{Runner.LocalPlayer.PlayerId}";

            PlayerName = myName;

            Debug.Log("Set my own name: " + myName);
        }

        if (OwnerPlayer != PlayerRef.None && !AllScores.ContainsKey(OwnerPlayer))
        {
            AllScores.Add(OwnerPlayer, this);
        }

        InitializeUI();
    }
    public static void SetPlayerName(PlayerRef player, string name)
    {
        // If entry exists, update directly
        if (AllScores.TryGetValue(player, out NetworkScoreEntry entry))
        {
            entry.PlayerName = name;
        }
        else
        {
            // Store pending name for when Spawned() is called
            pendingNames[player] = name;
        }
    }
    public void SetOwner(PlayerRef player)
    {
        OwnerPlayer = player;
        if (!AllScores.ContainsKey(player))
        {
            AllScores.Add(player, this);
            Debug.Log($"[NetworkScoreEntry] Registered Score Entry for {player}");
        }
        InitializeUI();
    }

    private void InitializeUI()
    {
        if (_localUIInstance != null) return;

        GameObject layoutContainer = GameObject.FindGameObjectWithTag("StatContainer");
        if (layoutContainer != null)
        {
            _localUIInstance = Instantiate(statsPrefab, layoutContainer.transform);
            _scoreText = _localUIInstance.GetComponent<TextMeshProUGUI>();
            UpdateVisuals();
        }
    }

    public void AddCustomPlayer(PlayerRef player)
    {
        AllScores.Add(player, this);
        Debug.Log("Custom player added to AllScores: " + player);
        Debug.Log("Total players in AllScores: " + AllScores.Count);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (AllScores.ContainsKey(OwnerPlayer))
        {
            AllScores.Remove(OwnerPlayer);
        }

        if (_localUIInstance != null)
        {
            Destroy(_localUIInstance);
        }
    }

    void UpdateVisuals()
    {
        if (_scoreText != null)
        {
            _scoreText.text = $"{PlayerName}: {Score} pts";
        }
    }

    void OnDataChanged()
    {
        UpdateVisuals();
    }
}
