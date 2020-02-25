using UnityEditor;
using System.Collections;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEditor.Compilation;
using System.IO;

[CreateAssetMenu(fileName = "BackgroundBuildSettings", menuName = "BackgroundBuild/BackgroundBuildSettings")]
public class BackgroundBuildSettings : ScriptableObject
{
	public enum Browsers { Chrome, Firefox, Edge, InternetExplorer, Safari }
	
	public BuildTarget buildTargetSelected = BuildTarget.WebGL;
	public bool showNotifications = true;
	public bool	launchBuild = true;
	public bool	customServer = false;
	public bool	logBuild = false;
	public bool showLog = false;
	public string temporaryFolderPath,buildFolderPath,logFolderPath;
	public string webGLURL = "http://127.0.0.1/";
	public Browsers browser = Browsers.Chrome;
	
	void OnEnable()
	{
		init();
	}
	
	void init()
	{
		string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
		string[] s = Application.dataPath.Split('/');
		string projectName = s[s.Length - 2];
		if (temporaryFolderPath==null){ temporaryFolderPath = desktopPath + "/" + projectName + "/temp"; }
		if (buildFolderPath==null){ buildFolderPath = desktopPath + "/" + projectName + "/build"; }
		if (logFolderPath==null){ logFolderPath = desktopPath + "/" + projectName + "/log";}
	}
	
	public void reset()
	{
		
		buildTargetSelected = BuildTarget.WebGL;
		showNotifications = true;
		launchBuild = true;
		customServer = false;
		logBuild = false;
		showLog = false;
		temporaryFolderPath = null;
		buildFolderPath=null;
		logFolderPath = null;
		webGLURL = "http://127.0.0.1/";
		browser = Browsers.Chrome;
		init();
	}
}
