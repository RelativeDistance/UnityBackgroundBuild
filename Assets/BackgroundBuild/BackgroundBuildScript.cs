using UnityEditor;
using System.Collections;
using System.Diagnostics;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BackgroundBuildScript : EditorWindow
{
	private BuildTarget buildTargetSelected;
	private bool launchBuild;
	private bool showNotifications;
	private string temporaryFolderPath;
	private string buildFolderPath;
	private string webGLURL;
	
	enum Browsers
	{
		Chrome,
		Firefox,
		Edge,
		InternetExplorer,
		Safari
	}

	private Browsers browser;

	string[] windowsBrowserLocations = new string[] { "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome", 
													  "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome", 
													  "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
													  "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
													  "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome"};  
	
	string[] macBrowserLocations = new string[] { "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome", 
												  "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome", 
												  "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
												  "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
												  "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome"};  
	
	[MenuItem("Window/Background Build")]
	static void Init()
	{
		BackgroundBuildScript window = (BackgroundBuildScript)EditorWindow.GetWindow(typeof(BackgroundBuildScript));
		window.Show();
	}
	
	void OnEnable()
	{
		webGLURL = EditorPrefs.GetString("webGLURL", "http://127.0.0.1");
		buildFolderPath = EditorPrefs.GetString("buildFolderPath", "");
		temporaryFolderPath = EditorPrefs.GetString("temporaryFolderPath", "");
		showNotifications = EditorPrefs.GetBool("showNotifications", true);
		launchBuild = EditorPrefs.GetBool("launchBuild", true);
		buildTargetSelected = (BuildTarget)EditorPrefs.GetInt("buildTargetSelected", (int)BuildTarget.WebGL);
	}
	
	void OnGUI()
	{
		GUIStyle guiStyle = new GUIStyle();
		guiStyle.padding = new RectOffset( 10, 10, 10, 10 );
		
		EditorGUILayout.BeginVertical(guiStyle);
		GUILayout.Label( "Build Settings" , EditorStyles.boldLabel );
		EditorGUI.indentLevel++;
		buildTargetSelected = (BuildTarget)EditorGUILayout.EnumPopup("Build Target", buildTargetSelected);
		
		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.TextField("Temporary Folder",temporaryFolderPath,GUILayout.ExpandWidth(true));
			if(GUILayout.Button("Browse",GUILayout.ExpandWidth(false)))
				temporaryFolderPath = EditorUtility.SaveFolderPanel("Temporary Folder Path",temporaryFolderPath,null);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.TextField("Build Folder",buildFolderPath,GUILayout.ExpandWidth(true));
			if(GUILayout.Button("Browse",GUILayout.ExpandWidth(false)))
				buildFolderPath = EditorUtility.SaveFolderPanel("Build Folder Path",buildFolderPath,null);
		EditorGUILayout.EndHorizontal();
		
		showNotifications = EditorGUILayout.Toggle("Show Notifications",showNotifications);
		
		
		if ((buildTargetSelected==BuildTarget.WebGL) || 
			(buildTargetSelected==BuildTarget.StandaloneOSX) || 
			(buildTargetSelected==BuildTarget.StandaloneWindows) || 
			(buildTargetSelected==BuildTarget.StandaloneWindows64) || 
			(buildTargetSelected==BuildTarget.StandaloneLinux64))
		{
			
			GUILayout.Space(10);
			GUILayout.Label( "Launch Settings" , EditorStyles.boldLabel );
			
			launchBuild = EditorGUILayout.Toggle("Launch Build", launchBuild);
			if (buildTargetSelected==BuildTarget.WebGL)
			{
				
				EditorGUI.BeginDisabledGroup(launchBuild == false);
				browser = (Browsers)EditorGUILayout.EnumPopup("Browser", browser);
				webGLURL = EditorGUILayout.TextField("WebGL URL",webGLURL,GUILayout.ExpandWidth(true));
				EditorGUI.EndDisabledGroup();	
			}
		}
		
		GUILayout.Space(10);
		
		if (GUILayout.Button("BUILD!", GUILayout.Height(40)))
		{
			SaveData();
			PerformCopyAndInitSilentUnity();
		}
		
		EditorGUILayout.EndVertical();
		
	}
	
	void SaveData()
	{
		EditorPrefs.SetString("webGLURL", webGLURL);
		EditorPrefs.SetString("buildFolderPath", buildFolderPath);
		EditorPrefs.SetString("temporaryFolderPath", temporaryFolderPath);
		EditorPrefs.SetBool("showNotifications", showNotifications);
		EditorPrefs.SetBool("launchBuild", launchBuild);
		EditorPrefs.SetInt("buildTargetSelected", (int)buildTargetSelected);	
	}
	
	void OnDisable()
	{
		SaveData();
	}
	
	void PerformCopyAndInitSilentUnity()
	{	
		FileUtil.DeleteFileOrDirectory( temporaryFolderPath + "/BackgroundBuildTemp");
		FileUtil.CopyFileOrDirectory("/Data/Apps/VidSquid", temporaryFolderPath + "/BackgroundBuildTemp");
		doProcess(EditorApplication.applicationPath + "/Contents/MacOS/Unity", "-quit -batchmode -projectPath " + temporaryFolderPath + " -executeMethod BackgroundBuildScript.PerformBuild");
	}
		
	static void PerformBuild()
	{
		BackgroundBuildScript bbs = new BackgroundBuildScript();
		bbs.OnEnable(); // Load the data
		
		bbs.showNotification("Unity Build", "Build Started");
		BuildOptions buildOptions = BuildOptions.None;
		if (bbs.launchBuild) buildOptions = BuildOptions.AutoRunPlayer;
		BuildReport report = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, bbs.buildFolderPath , bbs.buildTargetSelected, buildOptions);
		if (bbs.launchBuild) bbs.launchBrowserForWebGLBuild();
		bbs.showNotification("Unity Build", "Total build time: " + report.summary.totalTime); 
    }

	public void showNotification(string windowTitle, string notificationMessage)
	{
		string notificationProgram,notification;
		
		#if UNITY_EDITOR_OSX
			notificationProgram = "osascript";
			notification = string.Format ("-e 'display notification \"{0}\" with title \"{1}\"'" , notificationMessage ,windowTitle);
		#else
			notificationProgram = "snoretoast";
			notification = string.Format ("-t {0} -m {1}" , notificationMessage ,windowTitle )
		#endif
		
		doProcess(notificationProgram, notification);
	}
	 
	public void launchBrowserForWebGLBuild()
	{
		string browserLocation = "";
		
		
		#if UNITY_EDITOR_OSX
			browserLocation = macBrowserLocations[(int)browser];
		#else
			browserLocation = windwsBrowserLocations[(int)browser];
		#endif
		
		doProcess(browserLocation, webGLURL);
	}
	
	void doProcess(string fileName, string arguments)
	{
		Process p = new Process();
		p.StartInfo.FileName = fileName;
		p.StartInfo.Arguments = arguments; 
		p.Start();
	}
	
}
