#if UNITY_EDITOR && !UNITY_WEBPLAYER && UNITY_IOS
using UnityEngine;

using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using UnityEditor.XCodeEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor.iOS.Xcode;
using UPTrace;

public static class TASDKPostBuild
{
	[PostProcessBuildAttribute(805)]
	public static void OnPostProcessBuild(BuildTarget target2, string path)
	{
#if UNITY_5 || UNITY_5_3_OR_NEWER
		if (target2 == BuildTarget.iOS)
#else
if (target2 == BuildTarget.iPhone)
#endif
		{
			UnityEditor.XCodeEditor.XCProject proj = new UnityEditor.XCodeEditor.XCProject(path);

			string projmodsPath = System.IO.Path.Combine(Application.dataPath, "SDKPackage/TASDK/Plugins/IOS/PostProcessBuild");
			string[] projmods = System.IO.Directory.GetFiles(projmodsPath, "TASDK.projmods", System.IO.SearchOption.AllDirectories);

			if (projmods.Length == 0)
			{
				Debug.LogWarning("[TASDKPostBuild] TASDK.projmods not found!");
			}
			foreach (string p in projmods)
			{
				proj.ApplyMod(p);
			}

			proj.AddOtherLinkerFlags ("-ObjC");
			//proj.AddOtherLinkerFlags ("-fobjc-arc");
			proj.Save();

			// add info.plist
			string infoPlistPath = Path.Combine(Path.GetFullPath(path), "info.plist");
			var plist = new PlistDocument();
			plist.ReadFromString(File.ReadAllText(infoPlistPath));
			PlistElementDict root = plist.root;

			// NSAppTransportSecurity
			root.CreateDict("NSAppTransportSecurity").SetBoolean("NSAllowsArbitraryLoads", true);

			// Version
			PlistElementDict versionDict = (PlistElementDict)root["PackageSDKVersion"];
			if (versionDict == null)
			{
				versionDict = root.CreateDict("PackageSDKVersion");
			}

			PlistElementDict sdkVersionDict = versionDict.CreateDict("TASDK");

			string iOS_SDK_Version = UPTraceApi.iOS_SDK_Version;
			string Android_SDK_Version = UPTraceApi.Android_SDK_Version;
			string Unity_Package_Version = UPTraceApi.Unity_Package_Version;


			sdkVersionDict.SetString("TASDK_iOS_SDK_Version", iOS_SDK_Version);
			sdkVersionDict.SetString("TASDK_Android_SDK_Version", Android_SDK_Version);
			sdkVersionDict.SetString("TASDK_Unity_Package_Version", Unity_Package_Version);

			File.WriteAllText(infoPlistPath, plist.WriteToString());
		}
	}
}
#endif