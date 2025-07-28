Ever need to save many positions in your large scene so you can jump around easily?  
Well there are many tools for that.  There is TaskAtlas, CameraBookmarks by borgsystem and plenty of others. But the problem is they just manipulate the scene camera which isn't very helpful when you need to move your character to those positions.  
Task Atlas has some advanced features but it is not compatible with Unity 6 and it can't do that one simple thing.  Which for me was a HUGE problem. 
So here is a script I wrote (with the help of ChatGPT) that will allow you to save positions based on your current scene view *BUT*, unliike the other tools, you have can have an optional Target Object.
For me I use my character so it will be placed at that position.  Now when you "Go" to that position, not only does your Scene View camera jump there, your Target Object is moved there also.

Place the scripts under any directory:
Asset/Scripts/Tools
   * BookmarkTarget.cs
   * SceneBookmark.cs
   * SceneBookmarkDatabase.cs
   * SceneBookmarkManager.cs
    
_Name_  - This will be the name of your Bookmark

_Target Object_  - This is the Object you want moved. If you only want the scene view camera moved this should be empty

_Keep Rotation_  - When you **Add** a bookmark, the rotation of the scene view camera is saved. This rotation will be applied to the Target Object as well which could result in odd rotations. If this is checked, the current Target Objects' rotation will be used.


* **Add**
   - This will add a new bookmark with the given name at the current scene view. If a Target Object is defined, that object will be move there as well.
* **Go**
   - This will mode you to the defined bookmark
* **Rename**
   - This allows you to rename your bookmark
* **Reset Position**
   - Sometimes your camera or Target Object won't be positioned quite as you like it. Often the Target Object may be off the floor. If this is the case, move Target Object where you need it to be an hit **Reset Position**
* **Remove**
   - This is exactly what you think, just removes the bookmark


You can place this script anywhere but a SerializedObject will be placed in Assets/Editor/. All bookmarks are stored in this SerializedObject (SceneBookarkManager)
  
You will find it under   **Tools** &#8594; **Nasty Diaper** &#8594; **Scene Bookmark Manager**

![Screenshot](./Capture.PNG "Screenshot")
