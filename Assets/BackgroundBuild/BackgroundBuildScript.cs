using UnityEditor;
using System.Collections;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEditor.Compilation;

public class BackgroundBuildScript : EditorWindow
{
	enum Browsers { Chrome, Firefox, Edge, InternetExplorer, Safari }
	
	private BuildTarget buildTargetSelected;
	private bool showNotifications,launchBuild,logBuild,showLog;
	private string temporaryFolderPath,buildFolderPath,logFolderPath,webGLURL;
	private Browsers browser;
	
	static string pathToScript; 
	
	string[] windowsBrowserLocations = new string[] { "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome", 
													  "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome", 
													  "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
													  "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
													  "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome"};  
	
	string[] macBrowserLocations = new string[] { "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome", 
												  "/Applications/Firefox.app/Contents/MacOS/firefox", 
												  "/Applications/Microsoft Edge Beta.app/Contents/MacOS/Microsoft Edge Beta",
												  "",
												  "/Applications/Safari.app/Contents/MacOS/Safari"};  
	
	 
	[MenuItem("Window/Background Build")]
	static void Init()
	{
		BackgroundBuildScript window = (BackgroundBuildScript)EditorWindow.GetWindow(typeof(BackgroundBuildScript));
		GUIContent titleContent = new GUIContent ("BackgroundBuild", AssetDatabase.LoadAssetAtPath<Texture> (pathToScript));
		window.titleContent = titleContent;
		window.Show();
	}
	
	void OnEnable()
	{
		LoadData();
	}
	
	void OnDisable()
	{
		SaveData();
	}
	
	// Data =================================================
	
	void LoadData()
	{
		string dekstopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
		string[] s = Application.dataPath.Split('/');
		string projectName = s[s.Length - 2];
	
		pathToScript = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
		pathToScript = pathToScript.Substring(0, pathToScript.LastIndexOf('/')) + "/EditorResources/bbuildwindowicon.png";
			
		webGLURL = EditorPrefs.GetString("BBS.webGLURL", "http://127.0.0.1");
		buildFolderPath = EditorPrefs.GetString("BBS.buildFolderPath", dekstopPath + "/" + projectName + "/build/");
		temporaryFolderPath = EditorPrefs.GetString("BBS.temporaryFolderPath", dekstopPath + "/" + projectName + "/temp/");
		showNotifications = EditorPrefs.GetBool("BBS.showNotifications", true);
		launchBuild = EditorPrefs.GetBool("BBS.launchBuild", true);
		buildTargetSelected = (BuildTarget)EditorPrefs.GetInt("BBS.buildTargetSelected", (int)BuildTarget.WebGL);
		
		logFolderPath = EditorPrefs.GetString("BBS.logFolderPath", dekstopPath + "/" + projectName + "/log/");
		showLog = EditorPrefs.GetBool("BBS.showLog", true);
	}
	
	
	void SaveData()
	{
		EditorPrefs.SetString("BBS.webGLURL", webGLURL);
		EditorPrefs.SetString("BBS.buildFolderPath", buildFolderPath);
		EditorPrefs.SetString("BBS.temporaryFolderPath", temporaryFolderPath);
		EditorPrefs.SetBool("BBS.showNotifications", showNotifications);
		EditorPrefs.SetBool("BBS.launchBuild", launchBuild);
		EditorPrefs.SetInt("BBS.buildTargetSelected", (int)buildTargetSelected);	
		EditorPrefs.SetBool("BBS.logBuild", logBuild);
		EditorPrefs.SetString("BBS.logFolderPath", logFolderPath);
		EditorPrefs.SetBool("BBS.showLog", showLog);
	}
	
	// UI ================================================================
	
	void OnGUI()
	{
		toolBarGUI();
		
		GUIStyle guiStyle = new GUIStyle();
		guiStyle.padding = new RectOffset( 10, 10, 10, 10 );
		
		EditorGUILayout.BeginVertical(guiStyle);
			buildSettingsGUI();
			launchSettingsGUI();
			logSettingsGUI();
			buildButtonGUI();
		EditorGUILayout.EndVertical();
	}
	
	void toolBarGUI()
	{
		GUILayout.BeginHorizontal(EditorStyles.toolbar);
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Settings...", EditorStyles.toolbarButton)) {
			GenericMenu toolsMenu = new GenericMenu();
			toolsMenu.AddItem(new GUIContent("Reset Settings"), false, doReset);
			toolsMenu.AddSeparator("");
			toolsMenu.AddItem(new GUIContent("Help..."), false, showHelp);
			toolsMenu.DropDown(new Rect(Screen.width - 130 - 40, 0, 0, 16));
			EditorGUIUtility.ExitGUI();
		}
		GUILayout.EndHorizontal();
	}
	
	void buildSettingsGUI()
	{
		GUILayout.Label( "Build Settings" , EditorStyles.boldLabel );
		EditorGUI.indentLevel++;
		buildTargetSelected = (BuildTarget)EditorGUILayout.EnumPopup("Build Target", buildTargetSelected);
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.TextField("Temporary Folder",temporaryFolderPath,GUILayout.ExpandWidth(true));
		if(GUILayout.Button("Browse",GUILayout.ExpandWidth(false)))
			temporaryFolderPath = EditorUtility.SaveFolderPanel("Temporary Folder Path",temporaryFolderPath,null) + "/temp/";
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.TextField("Build Folder",buildFolderPath,GUILayout.ExpandWidth(true));
		if(GUILayout.Button("Browse",GUILayout.ExpandWidth(false)))
			buildFolderPath = EditorUtility.SaveFolderPanel("Build Folder Path",buildFolderPath,null) + "/build/";
		EditorGUILayout.EndHorizontal();
		
		showNotifications = EditorGUILayout.Toggle("Show Notifications",showNotifications);
	}
	
	void launchSettingsGUI()
	{
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
	}
	
	void logSettingsGUI()
	{
		GUILayout.Space(10);
		GUILayout.Label( "Log Settings" , EditorStyles.boldLabel );
		logBuild = EditorGUILayout.Toggle("Log Build", logBuild);
		EditorGUI.BeginDisabledGroup(logBuild == false);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.TextField("Log Folder",logFolderPath,GUILayout.ExpandWidth(true));
		if(GUILayout.Button("Browse",GUILayout.ExpandWidth(false)))
			logFolderPath = EditorUtility.SaveFolderPanel("Log Folder Path",logFolderPath,null) + "/log/";
		EditorGUILayout.EndHorizontal();
			
		showLog = EditorGUILayout.Toggle("Show Log",showLog);
		EditorGUI.EndDisabledGroup();	
	}
	
	void buildButtonGUI()
	{
		GUILayout.Space(10);	
		if (GUILayout.Button("BUILD!", GUILayout.Height(40)))
		{
			SaveData();
			PerformCopyAndInitSilentUnity();
		}
	}
	
	
	// Settings Menu ==================================================
	
	void doReset()
	{
		EditorPrefs.DeleteKey("BBS.webGLURL");
		EditorPrefs.DeleteKey("BBS.buildFolderPath");
		EditorPrefs.DeleteKey("BBS.temporaryFolderPath");
		EditorPrefs.DeleteKey("BBS.showNotifications");
		EditorPrefs.DeleteKey("BBS.launchBuild");
		EditorPrefs.DeleteKey("BBS.buildTargetSelected");	
		EditorPrefs.DeleteKey("BBS.logBuild");
		EditorPrefs.DeleteKey("BBS.logFolderPath");
		EditorPrefs.DeleteKey("BBS.showLog");
		OnEnable();
	}
	
	void showHelp()
	{
		Application.OpenURL("https://github.com/RelativeDistance/UnityBackgroundBuild");	
	}
	
	
	// BUILD ===========================================================
	
	void PerformCopyAndInitSilentUnity()
	{	
		FileUtil.DeleteFileOrDirectory( temporaryFolderPath + "/BackgroundBuildTemp");
		FileUtil.CopyFileOrDirectory("/Data/Apps/VidSquid", temporaryFolderPath + "/BackgroundBuildTemp");
		doProcess(EditorApplication.applicationPath + "/Contents/MacOS/Unity", "-quit -batchmode -projectPath " + temporaryFolderPath + " -executeMethod BackgroundBuildScript.PerformBuild");
	}
		
	static void PerformBuild()
	{
		NoErrorsValidator();
		BackgroundBuildScript bbs = new BackgroundBuildScript();
		bbs.OnEnable(); // Load the data
		
		bbs.showNotification("Unity Build", "Build Started");
		BuildOptions buildOptions = BuildOptions.None;
		if (bbs.launchBuild) buildOptions = BuildOptions.AutoRunPlayer;
		
		BuildReport report = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, bbs.buildFolderPath , bbs.buildTargetSelected, buildOptions);
		
		if (report.summary.result == BuildResult.Succeeded)
		{
			bbs.showNotification("Unity Build", "Status: BUILD SUCCEEDED ");
		}

		if (report.summary.result == BuildResult.Failed)
		{
			bbs.showNotification("Unity Build", "Status: BUILD FAILED "); 
		}
		
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
		string browserLocation;
		
		#if UNITY_EDITOR_OSX
			browserLocation = macBrowserLocations[(int)browser];
		#else
			browserLocation = windwsBrowserLocations[(int)browser];
		#endif
		
		doProcess(browserLocation, webGLURL);
	}
	
	void doProcess(string fileName, string arguments)
	{
		System.Diagnostics.Process p = new System.Diagnostics.Process();
		p.StartInfo.FileName = fileName;
		p.StartInfo.Arguments = arguments; 
		p.Start();
	}
	
	
	static void NoErrorsValidator() 
	{
		//if (Application.isBatchMode)
		CompilationPipeline.assemblyCompilationFinished += ProcessBatchModeCompileFinish;
	}
     
	private static void ProcessBatchModeCompileFinish(string s, CompilerMessage[] compilerMessages)
	{
		//exit on error
		//EditorApplication.Exit(-1);
	}
	
}
