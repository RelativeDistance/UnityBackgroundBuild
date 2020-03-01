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

	string[] windowsBrowsers = new string[] { "chrome ", "firefox ", "microsoft-edge:", "iexplore ","Safari Not Available on Windows"};  
	string[] macBrowsers = new string[] { 
													"/Applications/Google Chrome.app/Contents/MacOS/Google Chrome", 
													"/Applications/Firefox.app/Contents/MacOS/firefox", 
													"/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge",
													"Internet Explore Not Available on Mac",
													"open" //safari works a little differently when opening from command line
												};  
													
	string logFilename = "Background-Build-Log.txt";
	
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
		pathToScript = pathToScript.Substring(0, pathToScript.LastIndexOf('/')) + "/EditorResources/bbuildwindowicon.png";
			
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
		currentlyCopying = false;
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
			
		if (settings.logBuild) 
		{
			Directory.CreateDirectory(settings.logFolderPath);
			writeToLog("---------------------------------------",settings.logFolderPath);
			writeToLog("Copy Started",settings.logFolderPath);
		}
		
		FileUtil.DeleteFileOrDirectory( settings.temporaryFolderPath );
		Directory.CreateDirectory(settings.temporaryFolderPath);
		
		FileUtil.ReplaceDirectory(System.IO.Directory.GetCurrentDirectory()+"/Assets", settings.temporaryFolderPath+"/Assets" );
		FileUtil.ReplaceDirectory(System.IO.Directory.GetCurrentDirectory()+"/Library", settings.temporaryFolderPath+"/Library" );
		FileUtil.ReplaceDirectory(System.IO.Directory.GetCurrentDirectory()+"/Packages", settings.temporaryFolderPath+"/Packages" );
		FileUtil.ReplaceDirectory(System.IO.Directory.GetCurrentDirectory()+"/ProjectSettings", settings.temporaryFolderPath+"/ProjectSettings" );
		currentlyCopying = false;
		
		string cmdLineParams = "-projectPath ";
		
		if (settings.silentBuild)
		{
			cmdLineParams = "-quit -batchmode " + cmdLineParams;
		}
	
		string unityPath = EditorApplication.applicationPath;
		
		#if UNITY_EDITOR_OSX
			unityPath += "/Contents/MacOS/Unity";
		#endif
		
		doProcess(unityPath, cmdLineParams + "\"" +settings.temporaryFolderPath + "\" -executeMethod BackgroundBuildScript.PerformBuild");  
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
		
		string productName = Application.productName;
		
		#if UNITY_EDITOR_WIN
			if ((bbs.settings.buildTargetSelected == BuildTarget.StandaloneWindows) || (bbs.settings.buildTargetSelected == BuildTarget.StandaloneWindows64))
			productName = productName + ".exe";
		#endif
		
		BuildReport report = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, bbs.settings.buildFolderPath+"/"+productName , bbs.settings.buildTargetSelected, buildOptions);
		
		if (bbs.settings.launchBuild && bbs.settings.customServer &&  bbs.settings.buildTargetSelected==BuildTarget.WebGL)
		{
			bbs.launchBrowserForWebGLBuild();
		}
		
		if ((bbs.settings.launchBuild) && !(bbs.settings.buildTargetSelected==BuildTarget.WebGL))
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
		
		
		
		if (bbs.settings.showNotifications)
		{
			string message = "";
			if (report.summary.result == BuildResult.Succeeded)
			{
				message = "BUILD SUCCEEDED - Total build time: " + report.summary.totalTime;
			}
			else if (report.summary.result == BuildResult.Failed)
			{
				message ="BUILD FAILED - Total build time: " + report.summary.totalTime;	
			}	
			
			bbs.showNotification("Unity Build", message ); 
			if (bbs.settings.logBuild) bbs.writeToLog(message,bbs.settings.logFolderPath);
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
			notificationProgram = Application.dataPath.Replace("/Assets","") + "/" +pathToScript.Substring(0, pathToScript.LastIndexOf('/')) + "/snoretoast.exe";
			string iconPath = Application.dataPath.Replace("/Assets","") + "/" +pathToScript.Substring(0, pathToScript.LastIndexOf('/'))+"/ToastIcon.png";
			notification = string.Format ("-t \"{0}\" -m \"{1}\" -silent -p \"{2}\" -appID Snore.DesktopToasts.0.7.0" , notificationMessage ,windowTitle, iconPath );
		#endif
		doProcess(notificationProgram, notification);
	}
	 
	public void launchBrowserForWebGLBuild()
	{
		string browserLocation;
		string url = settings.webGLURL;
		#if UNITY_EDITOR_OSX
		browserLocation = macBrowsers[(int)settings.browser];
			
		if (settings.browser==BackgroundBuildSettings.Browsers.Safari)
		{
			url = "-a Safari "+url;
		}
			
		doProcess(browserLocation, settings.webGLURL);
			
		#else
			browserLocation = windowsBrowsers[(int)settings.browser];
			doProcess("cmd.exe", "/C start "+browserLocation + settings.webGLURL);
		#endif

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
		StreamWriter writer = new StreamWriter(path+"/"+logFilename, true);
		writer.WriteLine(DateTime.Now +" " +line);
		writer.Close();
	}
	
	public void displayLog(string path)
	{
		#if UNITY_EDITOR_OSX
			doProcess("open", "\""+path+"/"+logFilename+"\"");
		#else
			doProcess("explorer.exe", path+"/"+logFilename);
		#endif
	}
	
	
}
