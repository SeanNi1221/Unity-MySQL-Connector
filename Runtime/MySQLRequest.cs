using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
namespace Sean21.MySQLConnector
{
[Serializable]
public class MySQLRequest
{
    [Header("Settings")]
    public bool enableDebugLog = true;
    public bool enableTimer;
    [Header("Terminal")]
    [TextArea(0,100)]
    public string sql;
    [Header("Response")]
    [TextArea(0,100)]
    public string json;
    public float responseTime;
    public MySQLResult result;
    // public MySQLLane lane{ get; internal set;}
    public Action OnFinished;
    public bool succeeded{get; private set;}
    [HideInInspector]
    public UnityWebRequest web;
    public UnityWebRequestAsyncOperation operation;
    public MySQLRequest(){}
    public MySQLRequest(string sql)
    {
        this.sql = sql;
    }
    public MySQLRequest(bool enableDebugLog, bool enableTimer = false)
    {
        this.enableDebugLog = enableDebugLog;
        this.enableTimer = enableTimer;
    }
    public void SendImmediate() {
        // if (lane) lane.SendRequest();
        // else MySQLBridge.SendRequest();
    }
    public IEnumerator Send() {
        yield return Send(sql);
    }
    public IEnumerator Send(string SQL) {
        succeeded = false;
        if (string.IsNullOrEmpty(SQL)) {
            if (enableDebugLog) Debug.LogWarning("Cannot send empty string as SQL, aborted!");
            yield break;
        }
        sql = SQL;
        WWWForm form = new WWWForm();
        MySQLBridge.Authorize(form);
        form.AddField("sql", sql);
        using (web = UnityWebRequest.Post(MySQLBridge.uri, form)) {
            if (enableDebugLog) Debug.Log("Connecting: " + web.uri);
            web.timeout = MySQLBridge.RequestTimeLimit;
            operation = web.SendWebRequest();
            if (enableTimer) {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.update += MySQLBridge.ConstantLoopUpdate;
#endif
                responseTime = 0f;
                while(!web.isDone) {
                    responseTime += Time.deltaTime;
                    //Incase Enable Timer is turned off during timing process.
                    if (!enableTimer) {

#if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= MySQLBridge.ConstantLoopUpdate;
#endif                        
                        break;
                    }
                    yield return null;
                }
            }            
            yield return operation;
#if UNITY_EDITOR
        if (enableTimer)
            UnityEditor.EditorApplication.update -= MySQLBridge.ConstantLoopUpdate;
#endif
#if UNITY_2020_1_OR_NEWER
            if (web.result == UnityWebRequest.Result.ConnectionError || web.result == UnityWebRequest.Result.ProtocolError)
#else 
            if (web.isNetworkError || web.isHttpError)
#endif
            {
                Debug.LogError("Failed sending Request: " + SQL + " with error: " + web.error + "  uri: " + MySQLBridge.uri);
                succeeded = false;
                yield break;
            }
            if (enableDebugLog) Debug.Log("Request succeeded" + (enableTimer? " in " + responseTime + " s" : "") + ":\n" + SQL);
            json = web.downloadHandler.text;
            // result = MySQLBridge.Parse(json);
            
            if (OnFinished != null) OnFinished();
            succeeded = true;
            yield break;
        }
    }
    public void Clear()
    {
        // result.Clear();

        json = null;
        sql = null;
        succeeded = false;
        responseTime = 0f;
    }
}
}