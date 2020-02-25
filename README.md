![](Screenshot.png)

# Unity Background Build
Fire off a background build of your Unity project and continue working on the same project. Most useful for platforms that take a long time to build such as WebGL.

# How It Works
After applying your settings and pressing build, this plugin duplicates your entire current project, and then runs a batch mode build using the current settings and scene list.  Optionaly it launches the build when complete and logs everything.  

# Installation
Import the UnityBackgroundBuild.unitypackage.  Then go Window->Background Build to open the editor window.  The following options are prensented.

## Build Settings

### Build Target
Select the platform to build to.  If you are building to a different platform than the one you are working on, highly recommended to be using v2 Asset Pipeline so assets won't have to be reimported.

### Temporary Folder
The folder where your project is duplicated to.  Highly recommeneded that you have a fast ssd/nvme drive so this step is as short as possible.

### Build Folder
Folder where the project is built to.

### Show Notifications
Shows OS level notifications for steps during the build.  Native on Mac, uses snoretoast on Windows.

### Silent Build
Build without launching the Unity editor. Should be on most of the time.  Helpful to turn to debug issues.

## Launch Settings

### Launch Build
Just like it says will launch the build after completing.  

### Custom Server (WebGL)
Turn on if you are running a custom server, if off the standard simple server that Unity uses will launch.

### Browser (WebGL)
Pick your browser.  If your browsers are installed in non standard places, edit the main script.  Safari not available on Windows.

### WebGL URL (WebGL)
URL to launch for custom server option.

## Log Settings

### Log build
Output build steps to a text file.

### Log Folder
Location of log text file.

### Show Log
Show the log after build complete.

# TODO
- Show detailed errors in the log.
- Test other platforms beside WebGL, Windows, and Mac.




