using UnityEditor;
using UnityEngine;

public class GameHuasMenu
{
    [MenuItem("GameHuas/About SDK", false, 0)]
    public static void About()
    {
        Application.OpenURL("http://doc.gamehaus.com/");
    }

    [MenuItem("GameHuas/Documentation...", false, 1)]
    public static void Documentation()
    {
        Application.OpenURL("http://doc.gamehaus.com/");
    }

    [MenuItem("GameHuas/Report Issue...", false, 2)]
    public static void ReportIssue()
    {
        Application.OpenURL("http://doc.gamehaus.com/");
    }

    [MenuItem("GameHuas/Manage SDKs...", false, 4)]
    public static void SdkManagerProd()
    {
        GameHausSDKManager.ShowSDKManager(stage: false);
    }
}
