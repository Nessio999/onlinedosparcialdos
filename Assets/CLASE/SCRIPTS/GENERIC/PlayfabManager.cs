//using System;
//using System.Threading.Tasks;
//using UnityEngine;
//using PlayFab;
//using PlayFab.ClientModels;
//using TMPro;
//using System.Collections.Generic;

//public class PlayfabManager : MonoBehaviour
//{
//    [SerializeField] private TMP_InputField usernameInput;
//    [SerializeField] private TMP_InputField passwordInput;

//    [Header("Photon")]
//    [SerializeField] private PhotonManager photonManager;

//    [Header("Player Data")]
//    public string playerName;
//    public int playerLevel;
//    public float playerHealth;

//    public Dictionary <string, UserDataRecord> userData;
//     public static PlayfabManager _PlayfabManager;

//     private void Awake()
//    {
//        if (_PlayfabManager == null)
//        {
//            _PlayfabManager = this;

//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }
//    void Start()
//    {
//        PlayFabSettings.TitleId = "1BC0C5";
//    }

//    // BOTON REGISTER
//    public async void RegisterUser()
//    {
//        Debug.Log("Boton Register Presionado");

//        try
//        {
//            var result = await RegisterPlayFabAccount();
//            Debug.Log("Usuario registrado correctamente");
//        }
//        catch (Exception error)
//        {
//            Debug.LogError(error.Message);
//        }
//    }

//    public async Task<RegisterPlayFabUserResult> RegisterPlayFabAccount()
//    {
//        var taskSource = new TaskCompletionSource<RegisterPlayFabUserResult>();

//        RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest()
//        {
//            Username = usernameInput.text.ToLower(),
//            DisplayName = usernameInput.text,
//            Email = usernameInput.text + "@test.com",
//            Password = passwordInput.text
//        };

//        PlayFabClientAPI.RegisterPlayFabUser(
//            request,
//            result => taskSource.SetResult(result),
//            error => taskSource.SetException(new Exception(error.GenerateErrorReport()))
//        );

//        return await taskSource.Task;
//    }

//    // BOTON LOGIN
//    public async void PlayfabLogin()
//    {
//        try
//        {
//            var result = await LoginWithPlayfab();

//            Debug.Log("Login Successful");

//            // AQUI INICIAMOS EL JUEGO
//            photonManager.StartGameAsHost();
//        }
//        catch (Exception error)
//        {
//            Debug.LogError(error.Message);
//        }
//    }

//    public async Task<LoginResult> LoginWithPlayfab()
//    {
//        var taskSource = new TaskCompletionSource<LoginResult>();

//        var request = new LoginWithPlayFabRequest()
//        {
//            Username = usernameInput.text.ToLower(),
//            Password = passwordInput.text
//        };

//        PlayFabClientAPI.LoginWithPlayFab(
//            request,
//            result => taskSource.SetResult(result),
//            error => taskSource.SetException(new Exception(error.GenerateErrorReport()))
//        );

//        return await taskSource.Task;
//    }

//    // SUBIR DATOS
//    public void UploadPlayerData()
//    {
//        var request = new UpdateUserDataRequest()
//        {
//            Data = new System.Collections.Generic.Dictionary<string, string>()
//            {
//                {"PlayerName", playerName},
//                {"PlayerLevel", playerLevel.ToString()},
//                {"PlayerHealth", playerHealth.ToString()}
//            }
//        };

//        PlayFabClientAPI.UpdateUserData(
//            request,
//            result => Debug.Log("Datos subidos a PlayFab"),
//            error => Debug.LogError(error.GenerateErrorReport())
//        );
//    }

//    // OBTENER DATOS
//    public void GetPlayerData()
//    {
//        PlayFabClientAPI.GetUserData(
//            new GetUserDataRequest(),
//            result =>
//            {
//                Debug.Log("Datos recibidos de PlayFab:");

//                if (result.Data != null)
//                {
//                    if (result.Data.ContainsKey("PlayerName"))
//                        Debug.Log("PlayerName: " + result.Data["PlayerName"].Value);

//                    if (result.Data.ContainsKey("PlayerLevel"))
//                        Debug.Log("PlayerLevel: " + result.Data["PlayerLevel"].Value);

//                    if (result.Data.ContainsKey("PlayerHealth"))
//                        Debug.Log("PlayerHealth: " + result.Data["PlayerHealth"].Value);
//                }
//            },
//            error => Debug.LogError(error.GenerateErrorReport())
//        );
//    }
//}
using System;
using System.Threading.Tasks;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayfabManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_Text messageText;
   // public bool IsLoggedIn { get; private set; }

    [Header("Photon")]
    [SerializeField] private PhotonManager photonManager;

    [Header("Player Data")]
    public string playerName;
    public int playerLevel;
    public float playerHealth;

    public bool IsLoggedIn { get; private set; } // ✅ ADD THIS

    public Dictionary<string, UserDataRecord> userData;

    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject mainMenuPanel;

    public static PlayfabManager _PlayfabManager;

    private void Awake()
    {
        if (_PlayfabManager == null)
        {
            _PlayfabManager = this;
            DontDestroyOnLoad(gameObject); // ✅ FIXED
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        PlayFabSettings.TitleId = "1BC0C5";
        loginPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    // =========================
    // REGISTER BUTTON
    // =========================
    //public async void RegisterUser()
    //{
    //    if (string.IsNullOrEmpty(usernameInput.text) || string.IsNullOrEmpty(passwordInput.text))
    //    {
    //        messageText.text = "Enter Username & Password";
    //        return;
    //    }

    //    try
    //    {
    //        var result = await RegisterPlayFabAccount();

    //        messageText.text = "Account Created!";

    //        // Default player data
    //        playerName = usernameInput.text;
    //        playerLevel = 1;
    //        playerHealth = 100;

    //        UploadPlayerData(); // ✅ Save initial data
    //        Invoke(nameof(Call), 1.5f);
    //    }
    //    catch (Exception error)
    //    {
    //        Debug.LogError(error.Message);
    //        messageText.text = error.Message;
    //    }
    //}
    public async void RegisterUser()
    {
        if (string.IsNullOrEmpty(usernameInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            messageText.text = "Enter Username & Password";
            return;
        }

        try
        {
            var result = await RegisterPlayFabAccount();

            // 🔐 Login after register
            await LoginWithPlayfab();

            messageText.text = "Account Created!";

            // Default player data
            playerName = usernameInput.text;
            playerLevel = 1;
            playerHealth = 100;
            // ✅ Send name to Photon
            photonManager.SetPlayerName(playerName);
            UploadPlayerData(); // ✅ Save initial data
            IsLoggedIn = true; // ✅ IMPORTANT
            Invoke(nameof(Call), 1.5f);
        }
        catch (Exception error)
        {
            Debug.LogError(error.Message);
            messageText.text = error.Message;
        }
    }
    void Call()
    {
        loginPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public async Task<RegisterPlayFabUserResult> RegisterPlayFabAccount()
    {
        var taskSource = new TaskCompletionSource<RegisterPlayFabUserResult>();

        RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest()
        {
            Username = usernameInput.text.ToLower(),
            DisplayName = usernameInput.text,
            Email = usernameInput.text + "@test.com",
            Password = passwordInput.text
        };

        PlayFabClientAPI.RegisterPlayFabUser(
            request,
            result => taskSource.SetResult(result),
            error => taskSource.SetException(new Exception(error.GenerateErrorReport()))
        );

        return await taskSource.Task;
    }

    // =========================
    // LOGIN BUTTON
    // =========================
    public async void PlayfabLogin()
    {
        if (string.IsNullOrEmpty(usernameInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            messageText.text = "Enter Username & Password";
            return;
        }

        try
        {
            var result = await LoginWithPlayfab();

            playerName = usernameInput.text;
            IsLoggedIn = true; // ✅ IMPORTANT
            messageText.text = "Login Successful!";

            GetPlayerData(); // ✅ Load saved data
            photonManager.SetPlayerName(playerName);
            // ✅ SWITCH PANELS
            loginPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
        }
        catch (Exception error)
        {
            Debug.LogError(error.Message);
            messageText.text = error.Message;
        }
    }

    public async Task<LoginResult> LoginWithPlayfab()
    {
        var taskSource = new TaskCompletionSource<LoginResult>();

        var request = new LoginWithPlayFabRequest()
        {
            Username = usernameInput.text.ToLower(),
            Password = passwordInput.text
        };

        PlayFabClientAPI.LoginWithPlayFab(
            request,
            result => taskSource.SetResult(result),
            error => taskSource.SetException(new Exception(error.GenerateErrorReport()))
        );

        return await taskSource.Task;
    }

    // =========================
    // UPLOAD PLAYER DATA
    // =========================
    public void UploadPlayerData()
    {
        var request = new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>()
            {
                {"PlayerName", playerName},
                {"PlayerLevel", playerLevel.ToString()},
                {"PlayerHealth", playerHealth.ToString()}
            }
        };

        PlayFabClientAPI.UpdateUserData(
            request,
            result => Debug.Log("Data uploaded to PlayFab"),
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

    // =========================
    // GET PLAYER DATA
    // =========================
    public void GetPlayerData()
    {
        PlayFabClientAPI.GetUserData(
            new GetUserDataRequest(),
            result =>
            {
                Debug.Log("Data received from PlayFab");

                if (result.Data != null)
                {
                    userData = result.Data;

                    if (result.Data.ContainsKey("PlayerName"))
                        playerName = result.Data["PlayerName"].Value;

                    if (result.Data.ContainsKey("PlayerLevel"))
                        playerLevel = int.Parse(result.Data["PlayerLevel"].Value);

                    if (result.Data.ContainsKey("PlayerHealth"))
                        playerHealth = float.Parse(result.Data["PlayerHealth"].Value);
                }
            },
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }
}



//if (PlayfabManager._PlayfabManager == null ||
//!PlayfabManager._PlayfabManager.IsLoggedIn)
//{
//    Debug.LogError("You must login first!");
//    return;
//}