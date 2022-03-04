using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using GameHaus.marijnz.EditorCoroutines;

public class GameHausSDKManager : EditorWindow
{
    private const string downloadDir = "Assets/GameHaus";

    private EditorCoroutines.EditorCoroutine coroutine;
    private UnityWebRequest downloader;
    private string activity;

    private GUIStyle labelStyle;
    private GUIStyle labelStyleArea;
    private GUIStyle labelStyleLink;
    private GUIStyle headerStyle;
    private readonly GUILayoutOption fieldWidth = GUILayout.Width(60);

    private struct SdkInfo
    {
        public string Name;
        public string Key;
        public string Url;
        public string LatestVersion;
        public string CurrentVersion;
        public string Filename;
        public string Instructions;


    }

        public static void ShowSDKManager(bool stage)
    {
        var win = GetWindow<GameHausSDKManager>(true);
        win.titleContent = new GUIContent(stage ? "GameHaus SDK Manager (Staging)" : "GameHaus SDK Manager");
        win.Focus();
    }

    void Awake()
    {
        labelStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold
        };
        labelStyleArea = new GUIStyle(EditorStyles.label)
        {
            wordWrap = true
        };
        labelStyleLink = new GUIStyle(EditorStyles.label)
        {
            normal = { textColor = Color.blue },
            active = { textColor = Color.white },
        };
        headerStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            fixedHeight = 18
        };
        //CancelOperation();
    }

    void OnGUI()
    {
        GUILayout.Space(5);
        EditorGUILayout.LabelField("GameHaus SDKs", labelStyle, GUILayout.Height(20));

        SdkHeaders();

        SdkInfo aa = new SdkInfo();
        aa.Name = "TASDK";
        aa.Filename = "TASDK";
        aa.Url = "https://github.com/Avid-ly/Avidly-Unity-TraceAnalysisSDK/blob/master/TASDK-Unity-4.1.0.5.unitypackage?raw=true";
        SdkRow(aa);

        SdkInfo aa1 = new SdkInfo();
        aa1.Name = "AASDK";
        aa1.Filename = "AASDK";
        aa1.Url = "https://github.com/Avid-ly/AASDK-UnityPackage/raw/master/AASDK_Unity_1.1.0.5.unitypackage";
        SdkRow(aa1);

        SdkInfo aa2 = new SdkInfo();
        aa2.Name = "MSSDK";
        aa2.Filename = "MSSDK";
        aa2.Url = "https://github.com/Avid-ly/Avidly-iOS-MSSDK-UnityPackage/raw/master/MSSDK_Unity_1.1.0.4.unitypackage";
        SdkRow(aa2);

        SdkInfo aa3 = new SdkInfo();
        aa3.Name = "CSSDK";
        aa3.Filename = "CSSDK";
        aa3.Url = "https://github.com/Avid-ly/CSSDK_UnityPlugin/raw/master/CSSDK-Unity-2.1.0.1.unitypackage";
        SdkRow(aa3);

        SdkInfo aa4 = new SdkInfo();
        aa4.Name = "PSSDK";
        aa4.Filename = "PSSDK";
        aa4.Url = "https://github.com/Avid-ly/PSSDK_UnityPlugin/raw/master/PSSDK_Unity_2.0.0.2.unitypackage";
        SdkRow(aa4);
    }

    private void SdkHeaders()
    {
        using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(false)))
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Package", headerStyle);
            GUILayout.Button("Version", headerStyle);
            GUILayout.Space(3);
            GUILayout.Button("Action", headerStyle, fieldWidth);
            GUILayout.Button(" ", headerStyle, GUILayout.Width(1));
            GUILayout.Space(5);
        }
        GUILayout.Space(4);
    }

    private void SdkRow(SdkInfo info, Func<bool, bool> customButton = null)
    {
        var lat = info.LatestVersion;
        var cur = info.CurrentVersion;
        var isInst = !string.IsNullOrEmpty(cur);
        var canInst = !string.IsNullOrEmpty(lat) && (!isInst);
        // Is any async job in progress?

        var tooltip = string.Empty;
        //if (info.NetworkVersions != null)
        //{
            string version = "1.1.0";
            //if (info.NetworkVersions.TryGetValue(PackageConfig.Platform.ANDROID, out version))
                tooltip += "\n  Android SDK:  " + version;
            //if (info.NetworkVersions.TryGetValue(PackageConfig.Platform.IOS, out version))
                tooltip += "\n  iOS SDK:  " + version;
        //}
        tooltip += "\n  Installed:  " + (cur ?? "n/a");
        tooltip = info.Name + "\n  Package:  " + (lat ?? "n/a") + tooltip;

        GUILayout.Space(4);
        using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(false)))
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField(new GUIContent { text = info.Name, tooltip = tooltip });
            GUILayout.Button(new GUIContent
            {
                text = lat ?? "--",
                tooltip = tooltip
            }, canInst ? EditorStyles.boldLabel : EditorStyles.label);
            GUILayout.Space(3);
            if (customButton == null || !customButton(canInst))
            {
                //GUI.enabled = !stillWorking && (canInst || testing);
                if (GUILayout.Button(new GUIContent
                {
                    text = isInst ? "Upgrade" : "Install",
                    tooltip = tooltip
                }, fieldWidth))
                    this.StartCoroutine(DownloadSDK(info));
                GUI.enabled = true;
            }

            if (!string.IsNullOrEmpty(info.Instructions))
            {
                if (GUILayout.Button("?", GUILayout.ExpandWidth(false)))
                    Application.OpenURL(info.Instructions);
            }
            else
                // Need to fill space so that the Install/Upgrade buttons all line up nicely.
                GUILayout.Button(" ", EditorStyles.label, GUILayout.Width(17));
            GUILayout.Space(5);
        }
        GUILayout.Space(4);
    }

    private IEnumerator DownloadSDK(SdkInfo info)
    {
        Debug.Log(downloadDir + "    " + info.Filename);
        var path = Path.Combine(downloadDir, info.Filename);
        Debug.Log("path: " + path);

        activity = string.Format("Downloading {0}...", info.Filename);
        Debug.Log(activity);

        // Start the async download job.
        downloader = new UnityWebRequest(info.Url)
        {
            downloadHandler = new DownloadHandlerFile(path),
            timeout = 120, // seconds
        };
        downloader.SendWebRequest();

        // Pause until download done/cancelled/fails, keeping progress bar up to date.
        while (!downloader.isDone)
        {
            yield return null;
            var progress = Mathf.FloorToInt(downloader.downloadProgress * 100);
            if (EditorUtility.DisplayCancelableProgressBar("MoPub SDK Manager", activity, progress))
                downloader.Abort();
        }
        EditorUtility.ClearProgressBar();

        if (string.IsNullOrEmpty(downloader.error))
            AssetDatabase.ImportPackage(path, true);  // OK, got the file, so let the user import it if they want.
        else
        {
            var error = downloader.error;
            if (downloader.isNetworkError)
            {
                if (error.EndsWith("destination host"))
                    error += ": " + info.Url;
            }
            else if (downloader.isHttpError)
            {
                switch (downloader.responseCode)
                {
                    case 404:
                        var file = Path.GetFileName(new Uri(info.Url).LocalPath);
                        error = string.Format("File {0} not found on server.", file);
                        break;
                    default:
                        error = downloader.responseCode + "\n" + error;
                        break;
                }
            }

            Debug.LogError(error);
        }

        // Reset async state so the GUI is operational again.
        downloader.Dispose();
        downloader = null;
        coroutine = null;
    }
}
