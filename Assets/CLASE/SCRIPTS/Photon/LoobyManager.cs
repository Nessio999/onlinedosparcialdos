using System.Xml;
using Fusion;
using UnityEngine;

public class LoobyManager : MonoBehaviour
{

    [SerializeField] private Transform viewPortContent;
    [SerializeField] private GameObject lobbyPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject warningMassage;
    void Start()
    {
        PhotonManager._PhotonManager.onSessionListUpdated += DestroyCanvasContent;
        PhotonManager._PhotonManager.onSessionListUpdated += UpddateSessionCanvas;

    }

    public void UpddateSessionCanvas()
    {
        Debug.Log("Creando sesiones:" + PhotonManager._PhotonManager.availabelSesion.Count);
        foreach (SessionInfo session in PhotonManager._PhotonManager.availabelSesion)
        {
            GameObject sessionInstance = Instantiate(lobbyPrefab, viewPortContent);
            sessionInstance.GetComponent<SessionEntry>().SetInfo(session);
        }
        
    }

    // Aqui deben destruir el contenido de el viewportContent
    //Obligatorio: Usar for, foreach no
    void DestroyCanvasContent()
    {
        Debug.Log("Destroy canvas"); 
        // Waringn Message 
        warningMassage.SetActive(PhotonManager._PhotonManager.availabelSesion.Count <= 0); // Waringn Message 
        
      

        for (int i = 0; i < viewPortContent.childCount; i++)
        {
          Destroy(viewPortContent.GetChild(i).gameObject);
        }

        
    }
}
