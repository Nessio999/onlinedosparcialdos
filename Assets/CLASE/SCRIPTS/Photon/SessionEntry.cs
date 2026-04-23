
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;

public class SessionEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text sessionName;
    [SerializeField] private TMP_Text playerCount;
    [SerializeField] private Button joinButton;

    private SessionInfo sessionInfo;

    public void SetInfo(SessionInfo sessionInfo)
    {
        this.sessionInfo = sessionInfo;

        sessionName.text = sessionInfo.Name;
        playerCount.text = sessionInfo.PlayerCount.ToString()
                    + "/" + sessionInfo.MaxPlayers.ToString();

        joinButton.interactable = sessionInfo.PlayerCount < sessionInfo.MaxPlayers;

        // Assign the button click here
        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(OnClickJoin);
    }
    public void OnClickJoin()
    {
        if (sessionInfo != null)
        {
            Debug.Log("Joining session: " + sessionInfo.Name);
            PhotonManager._PhotonManager.JoinSession(sessionInfo); // Pass the full SessionInfo
        }
    }
}
