using UnityEditor;
using System.Collections;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEditor.Compilation;
using System.IO;

public class BackgroundBuildScript : EditorWindow
{
	
	BackgroundBuildSettings settings;
	
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
		pathToScript = pathToScript.Substring(0, pathToScript.LastIndexOf('/')) + "/Resources/bbuildwindowicon.png";
			
		settings = Resources.Load<BackgroundBuildSettings>("BackgroundBuildSettings");
	}
	
	
	void SaveData()
	{
		EditorUtility.SetDirty(settings);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
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
		settings.buildTargetSelected = (BuildTarget)EditorGUILayout.EnumPopup("Build Target", settings.buildTargetSelected);
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.TextField("Temporary Folder",settings.temporaryFolderPath,GUILayout.ExpandWidth(true));
		if(GUILayout.Button("Browse",GUILayout.ExpandWidth(false)))
			settings.temporaryFolderPath = EditorUtility.SaveFolderPanel("Temporary Folder Path",settings.temporaryFolderPath,null) + "/temp";
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.TextField("Build Folder",settings.buildFolderPath,GUILayout.ExpandWidth(true));
		if(GUILayout.Button("Browse",GUILayout.ExpandWidth(false)))
			settings.buildFolderPath = EditorUtility.SaveFolderPanel("Build Folder Path",settings.buildFolderPath,null) + "/build";
		EditorGUILayout.EndHorizontal();
		
		settings.showNotifications = EditorGUILayout.Toggle("Show Notifications",settings.showNotifications);
	}
	
	void launchSettingsGUI()
	{
		if ((settings.buildTargetSelected==BuildTarget.WebGL) || 
		(settings.buildTargetSelected==BuildTarget.StandaloneOSX) || 
		(settings.buildTargetSelected==BuildTarget.StandaloneWindows) || 
		(settings.buildTargetSelected==BuildTarget.StandaloneWindows64) || 
		(settings.buildTargetSelected==BuildTarget.StandaloneLinux64))
		{
			
			GUILayout.Space(10);
			GUILayout.Label( "Launch Settings" , EditorStyles.boldLabel );
			
			settings.launchBuild = EditorGUILayout.Toggle("Launch Build", settings.launchBuild);
			
			if (settings.buildTargetSelected==BuildTarget.WebGL)
			{
				EditorGUI.BeginDisabledGroup(settings.launchBuild  == false);
				
				settings.customServer = EditorGUILayout.Toggle("Custom Server", settings.customServer);
				
					EditorGUI.BeginDisabledGroup(settings.customServer == false);
					settings.browser = (BackgroundBuildSettings.Browsers)EditorGUILayout.EnumPopup("Browser", settings.browser);
					settings.webGLURL = EditorGUILayout.TextField("WebGL URL",settings.webGLURL,GUILayout.ExpandWidth(true));
					EditorGUI.EndDisabledGroup();	
				EditorGUI.EndDisabledGroup();	
			}
		}
	}
	
	void logSettingsGUI()
	{
		GUILayout.Space(10);
		GUILayout.Label( "Log Settings" , EditorStyles.boldLabel );
		settings.logBuild = EditorGUILayout.Toggle("Log Build", settings.logBuild);
		EditorGUI.BeginDisabledGroup(settings.logBuild == false);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.TextField("Log Folder",settings.logFolderPath,GUILayout.ExpandWidth(true));
		if(GUILayout.Button("Browse",GUILayout.ExpandWidth(false)))
			settings.logFolderPath = EditorUtility.SaveFolderPanel("Log Folder Path",settings.logFolderPath,null) + "/log";
		EditorGUILayout.EndHorizontal();
			
		settings.showLog = EditorGUILayout.Toggle("Show Log",settings.showLog);
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
		settings.reset();
		SaveData();
		LoadData();
	}
	
	void showHelp()
	{
		Application.OpenURL("https://github.com/RelativeDistance/UnityBackgroundBuild");	
	}
	
	
	// BUILD ===========================================================
	
	void PerformCopyAndInitSilentUnity()
	{	
		
		FileUtil.DeleteFileOrDirectory( settings.temporaryFolderPath );
		Directory.CreateDirectory(settings.temporaryFolderPath);
		FileUtil.ReplaceDirectory(System.IO.Directory.GetCurrentDirectory(), settings.temporaryFolderPath );
		doProcess(EditorApplication.applicationPath + "/Contents/MacOS/Unity", "-quit -batchmode -projectPath " + settings.temporaryFolderPath + " -executeMethod BackgroundBuildScript.PerformBuild"); //
	}
		
	static void PerformBuild()
	{
		BackgroundBuildScript bbs = BackgroundBuildScript.CreateInstance("BackgroundBuildScript") as BackgroundBuildScript;
		
		bbs.LoadData();
		bbs.OnEnable(); // Load the data
		
		if (bbs.settings.showNotifications)
			bbs.showNotification("Unity Build", "Build Started");
		
		BuildOptions buildOptions = BuildOptions.None;
		
		
		
		if ((bbs.settings.launchBuild && (!(bbs.settings.buildTargetSelected==BuildTarget.WebGL))) 
			|| (!bbs.settings.customServer &&  bbs.settings.buildTargetSelected==BuildTarget.WebGL))
		{
			buildOptions = BuildOptions.AutoRunPlayer;
			
			if (bbs.settings.buildTargetSelected==BuildTarget.StandaloneOSX)
			{
				bbs.doProcess(bbs.settings.buildFolderPath+"/"+Application.productName+".app", null);
			}
			else if (bbs.settings.buildTargetSelected==BuildTarget.StandaloneWindows)
			{
				bbs.doProcess(bbs.settings.buildFolderPath+"/"+Application.productName+".exe", null);
			}
		}
		
		
		BuildReport report = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, bbs.settings.buildFolderPath+"/"+Application.productName , bbs.settings.buildTargetSelected, buildOptions);
		
		if (bbs.settings.launchBuild && bbs.settings.customServer &&  bbs.settings.buildTargetSelected==BuildTarget.WebGL)
		{
			bbs.launchBrowserForWebGLBuild();
		}
		
		if (bbs.settings.showNotifications)
		{
			if (report.summary.result == BuildResult.Succeeded)
			{
				bbs.showNotification("Unity Build", "BUILD SUCCEEDED - Total build time: " + report.summary.totalTime); 
			}

			if (report.summary.result == BuildResult.Failed)
			{
				bbs.showNotification("Unity Build", "BUILD FAILED - Total build time: " + report.summary.totalTime); 
			}	
		}
	}

	public void showNotification(string windowTitle, string notificationMessage)
	{
		string notificationProgram,notification;
		
		#if UNITY_EDITOR_OSX
		notificationProgram = "osascript";
		notification = string.Format ("-e 'display notification \"{0}\" with title \"{1}\"'" , notificationMessage ,windowTitle);
		#else
		notificationProgram = "snoretoast";
		notification = string.Format ("-t {0} -m {1}" , notificationMessage ,windowTitle );
		#endif
		
		doProcess(notificationProgram, notification);
	}
	 
	public void launchBrowserForWebGLBuild()
	{
		string browserLocation;
		
		#if UNITY_EDITOR_OSX
		browserLocation = macBrowserLocations[(int)settings.browser];
		
		//if safari 
		//open -a Safari URL
		
		#else
		browserLocation = windwsBrowserLocations[(int)settings.browser];
		#endif
		
		doProcess(browserLocation, settings.webGLURL);
	}
	
	void doProcess(string fileName, string arguments)
	{
		System.Diagnostics.Process p = new System.Diagnostics.Process();
		p.StartInfo.FileName = fileName;
		p.StartInfo.Arguments = arguments; 
		p.Start();
	}
	
	
	//static void NoErrorsValidator() 
	//{
	//	//if (Application.isBatchMode)
	//	CompilationPipeline.assemblyCompilationFinished += ProcessBatchModeCompileFinish;
	//}
     
	//private static void ProcessBatchModeCompileFinish(string s, CompilerMessage[] compilerMessages)
	//{
	//	CompilationPipeline.assemblyCompilationFinished += ProcessBatchModeCompileFinish;
	//	//Debug.Log(s);
	//	//exit on error
	//	//EditorApplication.Exit(-1);
	//}
	
}
