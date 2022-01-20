using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
namespace Sean21.MySQLConnector
{
[ExecuteInEditMode]
public partial class MySQLBridge : MonoBehaviour
{
    public static MySQLBridge i{get; private set;}
    private static string UriForEditor {
        get => i.uriForEditor + i.PHPFolder + "Connector.php";
    }
    [Header("Server")]
    [SerializeField] [Tooltip("The Server IP to be used if running in Unity Editor.")] private string uriForEditor = "127.0.0.1";
    public static string UriForBuild {
        get => i.uriForBuild + i.PHPFolder + "Connector.php"; 
    }
    [SerializeField] [Tooltip("The Server IP to be used if running in Built App.")] private string uriForBuild = "127.0.0.1";
    [SerializeField] private string PHPFolder = "/UnityBackend_PHP/";
    public static string uri => GetUri();
    public static string checkingUri => GetCheckingUri();
    private static Func<string> GetUri = ()=> UriForEditor;
    private static Func<string> GetCheckingUri = ()=> i.uriForEditor + i.PHPFolder + "ConnectionChecker.php";
    public static string ServerName {
        get => i.serverName; 
        set => i.serverName = value; 
    }

    [SerializeField] [Tooltip("The server name of MySQL.")] private string serverName = "localhost";
    public static string UserName {
        get => i.userName; 
        set => i.userName = value; 
    }
    [Header("Authorization")]
    [SerializeField] private string userName = "root";
    public static string Password {
        get => i.password; 
        set => i.password = value; 
    }
    [SerializeField] private string password;
    public static int RequestTimeLimit {
        get => i.requestTimeLimit; 
        set => i.requestTimeLimit = value;
    }
    public static Action<WWWForm> Authorize = form => {
        form.AddField("serverName", MySQLBridge.ServerName);
        form.AddField("userName", MySQLBridge.UserName);
        form.AddField("password", MySQLBridge.Password);
    };
    [Header("Global Settings")]
    [SerializeField] private int requestTimeLimit = 15;
    public static bool DetailedDebugLog {
        get => i.detailedDebugLog; 
        set => i.detailedDebugLog = value; 
    }
    [SerializeField] private bool detailedDebugLog = false;
    public static MySQLResult Result{ get => i.request.result; }
    public static MySQLRequest Request{ get => i.request; }
    [SerializeField] private MySQLRequest request = new MySQLRequest("SHOW DATABASES");
    public static Action OnInstantiated;
    public static Action OnInitialized;

    void Awake()
    {
        Initialize();
    }    
    void OnEnable()
    {
        SetInstance();
    }
    void Start() {
        // StartCoroutine(request.Get());
    }
    public static void SendRequest() {
        i.StartCoroutine(Request.Send());
    }
    public static void Start(IEnumerator coroutine) {
        i.StartCoroutine(coroutine);
    }

    public void Initialize()
    {
        SetInstance();
        if (!Application.isEditor) GetUri = ()=> UriForBuild;
        if (!Application.isEditor) GetCheckingUri = ()=> uriForBuild + PHPFolder + "ConnectionChecker.php";
        StartCoroutine(CheckingConnection());
    }
    private void SetInstance()
    {
        if (i == null) {
            i = this;
        }
        else if (i != this){
            Debug.LogWarning ("Multiple is of MySQLBridge is running, this may cause unexpected behaviours!. The newer i is ignored!");
            i = this;
        }
        if (OnInstantiated != null) OnInstantiated();
    }
    private IEnumerator CheckingConnection() {
        WWWForm form = new WWWForm();
        Authorize(form);
        
        using (request.web = UnityWebRequest.Post(checkingUri, form)) {
            yield return request.web.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
            if (request.web.result == UnityWebRequest.Result.ConnectionError || request.web.result == UnityWebRequest.Result.ProtocolError)
#else 
            if (request.web.isNetworkError || request.web.isHttpError)
#endif
            {
                Debug.LogError("Connection to server failed" + " with error: " + request.web.error + ".  uri: " + checkingUri);
                yield break;
            }
            Debug.Log("Connection to server succeeded.");
            string phpResult = request.web.downloadHandler.text;
            if (phpResult != "1") {
                Debug.LogError("Connection to MySQL failed. " + phpResult);
                yield break;
            }
            Debug.Log("Connection to MySQL succeeded.");
        }
    }
    //Edit mode coroutine supporter
#if UNITY_EDITOR
    public static void ConstantLoopUpdate()
    {
        if (!Application.isPlaying) {
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
        }
    }
#endif    
}
}
