using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject createRoomPanel;
    public GameObject inRoomPanel;

    [Header("Create Room UI")]
    public TMP_InputField roomNameInput;
    public TMP_InputField maxPlayersInput;

    [Header("Room List")]
    public Transform roomListContent;
    public GameObject roomButtonPrefab;

    [Header("In Room UI")]
    public TextMeshProUGUI playerCountText;

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected");
        PhotonNetwork.JoinLobby();
    }

    
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(room.Name))
                    cachedRoomList.Remove(room.Name);
            }
            else
            {
                cachedRoomList[room.Name] = room;
            }
        }

        UpdateRoomListUI();
    }
    void UpdateRoomListUI()
    {
        foreach (Transform child in roomListContent)
            Destroy(child.gameObject);

        foreach (RoomInfo room in cachedRoomList.Values)
        {
            GameObject button = Instantiate(roomButtonPrefab, roomListContent);

            button.GetComponentInChildren<TextMeshProUGUI>().text =
                room.Name + " (" + room.PlayerCount + "/" + room.MaxPlayers + ")";

            string roomName = room.Name;

            button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                JoinRoom(roomName);
            });
        }
    }

    // ================= CREATE ROOM =================

    public void OpenCreateRoomPanel()
    {
        mainMenuPanel.SetActive(false);
        createRoomPanel.SetActive(true);
    }

    public void CloseCreateRoomPanel()
    {
        createRoomPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
    public void CreateRoom()
    {
        string roomName = roomNameInput.text.Trim();

        if (string.IsNullOrEmpty(roomName))
            roomName = "Room" + Random.Range(0, 1000);

        byte maxPlayers = 4;

        if (!string.IsNullOrEmpty(maxPlayersInput.text))
        {
            if (!byte.TryParse(maxPlayersInput.text, out maxPlayers))
                maxPlayers = 4;

            if (maxPlayers > 10)
            {
                StartCoroutine(ShowWarning("Maximum players allowed is 10!"));
                return; // Stop room creation
            }

            if (maxPlayers < 1)
                maxPlayers = 4;
        }

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = maxPlayers;

        PhotonNetwork.CreateRoom(roomName, options);
    }
    //public void CreateRoom()
    //{
    //    string roomName = roomNameInput.text;

    //    if (string.IsNullOrEmpty(roomName))
    //        roomName = "Room" + Random.Range(0, 1000);

    //    byte maxPlayers = 4;

    //    if (!string.IsNullOrEmpty(maxPlayersInput.text))
    //    {
    //        byte.TryParse(maxPlayersInput.text, out maxPlayers);
    //        if (maxPlayers > 10)
    //            maxPlayers = 10;
    //        if (maxPlayers < 1)
    //            maxPlayers = 4;
    //    }

    //    RoomOptions options = new RoomOptions();
    //    options.MaxPlayers = maxPlayers;

    //    PhotonNetwork.CreateRoom(roomName, options);
    //}
    [Header("Warnings")]
    public TextMeshProUGUI warningText;
    IEnumerator ShowWarning(string message)
    {
        warningText.text = message;
        warningText.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        warningText.gameObject.SetActive(false);
    }
    // ================= JOIN ROOM =================

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        mainMenuPanel.SetActive(false);
        createRoomPanel.SetActive(false);
        inRoomPanel.SetActive(true);

        UpdatePlayerCount();
        SpawnPlayer();
    }

    // ================= PLAYER COUNT =================

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerCount();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerCount();
    }

    void UpdatePlayerCount()
    {
        playerCountText.text =
            "Players: " +
            PhotonNetwork.CurrentRoom.PlayerCount +
            " / " +
            PhotonNetwork.CurrentRoom.MaxPlayers;
    }

    // ================= LEAVE ROOM =================

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        inRoomPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // ================= SPAWN =================

    void SpawnPlayer()
    {
        PhotonNetwork.Instantiate("Player",
            new Vector3(Random.Range(-4, 4), 1, Random.Range(-4, 4)),
            Quaternion.identity);
    }
    public void RefreshRooms()
    {
        PhotonNetwork.LeaveLobby();
        PhotonNetwork.JoinLobby();
    }
  


}
