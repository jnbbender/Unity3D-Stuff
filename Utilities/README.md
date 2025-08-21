This folder contains a collection of Utility scripts collected over the years to help with common in-game functions or Inspector enhancements.

* `BorderWarning.cs` is a script which is intended to be attached to a collider which defines your environment boundaries. Marked as a trigger, when the Player enters the boundary the scene's PostProcessingLayer (requires ColorGrading) will turn Grey & a countdown 'till death will be displayed.
* `ConsoleGUI.cs` is simply a script which will direct all the Console logs to the screen.
* `EditorCollapseAll.cs` Hate it when your Project window has tons of open folders? Maybe showing hundreds of files?. Use this to collapse ALL folders. Just right click on anything in the Project window and "Collapse All"
* `FPSDisplay.cs` adds an FPS monitor to the screen
* `GroupWizard.cs` takes all the selected game objects and creates a single parent for them. Nice to organize your hierarchy
* `HelpAttribute.cs` allows you to place "Help" text using a [Help] tag above a field in your scripts
* `MeshCombineWizard.cs` does just that. Combines the selected meshes.
* `ResourceChecker.cs` is most valuable as it shows what is taking up the most resources in your scene
* `ScriptedDoorAnimator.cs` is an effective, generic script to open and close doors.
* `Singleton.cs` allows to to make any class a Singleton by inheriting from `Singleton<classname>`
* `ScreenShot.cs` is a simple script to take screen shots using a dedicated camera or from the main camera
