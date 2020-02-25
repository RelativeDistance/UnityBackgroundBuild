using UnityEditor;
using System.Collections;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEditor.Compilation;
using System.IO;
using System;

public class BackgroundBuildScript : EditorWindow
{
	
	BackgroundBuildSettings settings;
	bool currentlyCopying = false;
	static string pathToScript; 
	
	string[] windowsBrowserLocations = new string[] { "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome", 
		"/Applications/Google Chrome.app/Contents/MacOS/Google Chrome", 
		"/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
		"/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
		"/Applications/Google Chrome.app/Contents/MacOS/Google Chrome"};  
	
	string[] macBrowserLocations = new string[] { "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome", 
		"/Applications/Firefox.app/Contents/MacOS/firefox", 
		"/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge",
		"",
		"open"};  

	[MenuItem("Window/Background Build")]
	static void Init()
	{
		BackgroundBuildScript window = (BackgroundBuildScript)EditorWindow.GetWindow(typeof(BackgroundBuildScript));
		GUIContent titleContent = new GUIContent ("BackgroundBuild", AssetDatabase.LoadAssetAtPath<Texture> (pathToScript));
		window.titleContent = titleContent;
		window.Show();
		window.currentlyCopying = false;
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
		settings.silentBuild = EditorGUILayout.Toggle("Silent Build",settings.silentBuild);
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
		EditorGUI.BeginDisabledGroup(currentlyCopying == true);
		if (GUILayout.Button("BUILD!", GUILayout.Height(40)))
		{
			currentlyCopying = true;
			SaveData();
			PerformCopyAndInitSilentUnity();
		}
		EditorGUI.EndDisabledGroup();	
		
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

		if (settings.showNotifications)
			showNotification("Unity Build", "Copy Started");
			
		
		Directory.CreateDirectory(settings.logFolderPath);
		if (settings.logBuild) 
		{
			writeToLog("---------------------------------------",settings.logFolderPath);
			writeToLog("Copy Started",settings.logFolderPath);
		}
		
		
		FileUtil.DeleteFileOrDirectory( settings.temporaryFolderPath );
		Directory.CreateDirectory(settings.temporaryFolderPath);
		
		
		
		FileUtil.ReplaceDirectory(System.IO.Directory.GetCurrentDirectory(), settings.temporaryFolderPath );
		currentlyCopying = false;
		
		string cmdLineParams = "-projectPath ";
		
		if (settings.silentBuild)
		{
			cmdLineParams = "-quit -batchmode " + cmdLineParams;
		}
		
		doProcess(EditorApplication.applicationPath + "/Contents/MacOS/Unity", cmdLineParams + settings.temporaryFolderPath + " -executeMethod BackgroundBuildScript.PerformBuild");  
	}
		
		
	static void PerformBuild()
	{
		BackgroundBuildScript bbs = BackgroundBuildScript.CreateInstance("BackgroundBuildScript") as BackgroundBuildScript;
		
		bbs.LoadData();
		bbs.OnEnable(); 
		
		if (bbs.settings.showNotifications) bbs.showNotification("Unity Build", "Build Started");
		
		if (bbs.settings.logBuild) bbs.writeToLog("Build Started",bbs.settings.logFolderPath);
			
		BuildOptions buildOptions = BuildOptions.None;
		
		if ((bbs.settings.launchBuild && (!(bbs.settings.buildTargetSelected==BuildTarget.WebGL))) 
			|| (!bbs.settings.customServer &&  bbs.settings.buildTargetSelected==BuildTarget.WebGL))
		{
			buildOptions = BuildOptions.AutoRunPlayer;
		}
		
		BuildReport report = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, bbs.settings.buildFolderPath+"/"+Application.productName , bbs.settings.buildTargetSelected, buildOptions);
		
		if (bbs.settings.launchBuild && bbs.settings.customServer &&  bbs.settings.buildTargetSelected==BuildTarget.WebGL)
		{
			bbs.launchBrowserForWebGLBuild();
		}
		
		if (bbs.settings.launchBuild)
		{
			if (bbs.settings.buildTargetSelected==BuildTarget.StandaloneOSX)
			{
				bbs.doProcess(bbs.settings.buildFolderPath+"/"+Application.productName+".app", null);
			}
			else if (bbs.settings.buildTargetSelected==BuildTarget.StandaloneWindows)
			{
				bbs.doProcess(bbs.settings.buildFolderPath+"/"+Application.productName+".exe", null);
			}
		}
		
		
		string message;
		
		if (bbs.settings.showNotifications)
		{
			if (report.summary.result == BuildResult.Succeeded)
			{
				message = "BUILD SUCCEEDED - Total build time: " + report.summary.totalTime;
				bbs.showNotification("Unity Build", message); 
				if (bbs.settings.logBuild) bbs.writeToLog(message,bbs.settings.logFolderPath);
			}

			if (report.summary.result == BuildResult.Failed)
			{
				message ="BUILD FAILED - Total build time: " + report.summary.totalTime;
				bbs.showNotification("Unity Build", message ); 
				if (bbs.settings.logBuild) bbs.writeToLog(message,bbs.settings.logFolderPath);
			}	
		}
		
		if (bbs.settings.showLog)
		{
				bbs.displayLog(bbs.settings.logFolderPath);
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
		string url = settings.webGLURL;
		#if UNITY_EDITOR_OSX
			browserLocation = macBrowserLocations[(int)settings.browser];
			
			if (settings.browser==BackgroundBuildSettings.Browsers.Safari)
			{
				url = "-a Safari "+url;
			}
			
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
	
	 void writeToLog(string line, string path)
	{
		StreamWriter writer = new StreamWriter(path+"/BackgroundBuildLog.txt", true);
		writer.WriteLine(DateTime.Now +" " +line);
		writer.Close();
	}
	
	public void displayLog(string path)
	{
		#if UNITY_EDITOR_OSX
			doProcess("open", path+"/BackgroundBuildLog.txt");
		#else
		doProcess("explorer", path+"/BackgroundBuildLog.txt");
		#endif
	}
	
	
}
