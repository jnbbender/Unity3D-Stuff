using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NastyDiaper
{
    public class SceneBookmarkManager : EditorWindow
    {
        private string bookmarkName = "";
        private GameObject targetObject;
        private bool keepRotation = true;
        private SceneBookmarkDatabase database;
        private int renameIndex = -1;
        private List<bool> foldouts = new List<bool>();
        private Vector2 scrollPosition;

        // New multi-scene support variables
        private bool showAllScenes = false;
        private string currentSceneName = "";
        private string currentSceneGuid = "";
        private bool showSceneSelector = false;
        private Vector2 sceneSelectorScrollPosition;

        [MenuItem("Tools/Nasty Diaper/Scene Bookmark Manager")]
        public static void ShowWindow()
        {
            GetWindow<SceneBookmarkManager>("Scene Bookmarks");
        }

        private void OnEnable()
        {
            const string path = "Assets/NastyDiaper/SceneBookmarks/Editor/SceneBookmarkDatabase.asset";
            database = AssetDatabase.LoadAssetAtPath<SceneBookmarkDatabase>(path);

            if (database == null)
            {
                database = CreateInstance<SceneBookmarkDatabase>();
                System.IO.Directory.CreateDirectory("Assets/NastyDiaper/SceneBookmarks/Editor");
                AssetDatabase.CreateAsset(database, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            // Migrate old bookmarks without scene info
            database.MigrateOldBookmarks();

            UpdateCurrentSceneInfo();
            RefreshFoldouts();

            // Subscribe to scene change events
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged;
        }

        private void OnDisable()
        {
            // Unsubscribe from scene change events
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.activeSceneChangedInEditMode -= OnActiveSceneChanged;
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            UpdateCurrentSceneInfo();
            RefreshFoldouts();
            Repaint();
        }

        private void OnActiveSceneChanged(Scene previousScene, Scene newScene)
        {
            UpdateCurrentSceneInfo();
            RefreshFoldouts();
            Repaint();
        }

        private void UpdateCurrentSceneInfo()
        {
            var activeScene = SceneManager.GetActiveScene();
            currentSceneName = activeScene.name;
            currentSceneGuid = AssetDatabase.AssetPathToGUID(activeScene.path);
        }

        private void RefreshFoldouts()
        {
            var currentBookmarks = GetCurrentBookmarks();
            foldouts = new List<bool>(new bool[currentBookmarks.Count]);
        }

        private List<SceneBookmark> GetCurrentBookmarks()
        {
            if (showAllScenes)
            {
                return database.bookmarks;
            }
            else
            {
                return database.GetBookmarksForScene(currentSceneName, currentSceneGuid);
            }
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Scene selector header
            DrawSceneSelector();

            EditorGUILayout.Space();

            // Add new bookmark section
            GUILayout.Label("Add New Bookmark", EditorStyles.boldLabel);
            bookmarkName = EditorGUILayout.TextField(new GUIContent("Name", "Enter a descriptive name for this bookmark"), bookmarkName);
            targetObject = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Target Object", "Optional: GameObject to move to bookmark position. Leave empty to create camera-only bookmark"), targetObject, typeof(GameObject), true);
            keepRotation = EditorGUILayout.Toggle(new GUIContent("Keep Rotation", "When enabled with a Target Object: Uses the target's current rotation instead of the camera's rotation. When disabled: Uses the camera's viewing angle. This affects both the stored rotation and how the camera is positioned when navigating to the bookmark."), keepRotation);

            GUI.backgroundColor = new Color(0.6f, 0.8f, 1f);
            if (GUILayout.Button(new GUIContent("Add", "Create a new bookmark at the current Scene View position")))
            {
                AddBookmark();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space();

            // Bookmarks section
            DrawBookmarksList();

            EditorGUILayout.EndScrollView();
        }

        private void DrawSceneSelector()
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Current Scene: {currentSceneName}", EditorStyles.boldLabel);

            if (GUILayout.Button(new GUIContent(showSceneSelector ? "Hide Scenes" : "Show All Scenes", showSceneSelector ? "Hide the scene selector panel" : "Show scene selector panel with all scenes that have bookmarks"), GUILayout.Width(120)))
            {
                showSceneSelector = !showSceneSelector;
            }
            EditorGUILayout.EndHorizontal();

            // Show scene filter toggle
            EditorGUILayout.BeginHorizontal();
            bool newShowAllScenes = EditorGUILayout.Toggle(new GUIContent("Show All Scenes", "When enabled: Shows bookmarks from all scenes with scene labels. When disabled: Shows only bookmarks from the current scene"), showAllScenes);
            if (newShowAllScenes != showAllScenes)
            {
                showAllScenes = newShowAllScenes;
                RefreshFoldouts();
            }

            var currentBookmarks = GetCurrentBookmarks();
            string countText = showAllScenes ?
                $"Total: {database.bookmarks.Count} bookmarks" :
                $"Current Scene: {currentBookmarks.Count} bookmarks";
            GUILayout.Label(countText, EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            // Scene selector dropdown
            if (showSceneSelector)
            {
                EditorGUILayout.Space(5);
                GUILayout.Label("Scenes with Bookmarks:", EditorStyles.miniLabel);

                sceneSelectorScrollPosition = EditorGUILayout.BeginScrollView(sceneSelectorScrollPosition, GUILayout.MaxHeight(150));

                foreach (string sceneName in database.GetScenesWithBookmarks())
                {
                    int bookmarkCount = database.GetBookmarkCountForScene(sceneName);

                    // Check if this scene is currently loaded - improved detection
                    bool sceneIsLoaded = false;
                    string scenePath = "";

                    // First try to find the scene by name directly
                    Scene sceneByName = SceneManager.GetSceneByName(sceneName);
                    if (sceneByName.IsValid() && sceneByName.isLoaded)
                    {
                        sceneIsLoaded = true;
                        scenePath = sceneByName.path;
                    }
                    else
                    {
                        // Fallback: find scene by asset database and check by path
                        string[] sceneGuids = AssetDatabase.FindAssets($"t:Scene {sceneName}");
                        if (sceneGuids.Length > 0)
                        {
                            scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[0]);
                            Scene sceneByPath = SceneManager.GetSceneByPath(scenePath);
                            sceneIsLoaded = sceneByPath.IsValid() && sceneByPath.isLoaded;
                        }
                    }

                    EditorGUILayout.BeginHorizontal();

                    bool isCurrentScene = sceneName == currentSceneName;
                    GUI.backgroundColor = isCurrentScene ? new Color(0.7f, 1f, 0.7f) : Color.white;

                    // Dynamic button text and tooltip based on scene state
                    string buttonText = sceneIsLoaded ? $"Close {sceneName} ({bookmarkCount})" : $"Open {sceneName} ({bookmarkCount})";
                    string buttonTooltip = sceneIsLoaded ?
                        $"Unload scene '{sceneName}' (contains {bookmarkCount} bookmark{(bookmarkCount != 1 ? "s" : "")})" :
                        $"Load scene '{sceneName}' additively (contains {bookmarkCount} bookmark{(bookmarkCount != 1 ? "s" : "")})";

                    // Debug info in tooltip during development
                    buttonTooltip += $"\n[Debug: Scene loaded = {sceneIsLoaded}, Path = {scenePath}]";

                    if (GUILayout.Button(new GUIContent(buttonText, buttonTooltip), EditorStyles.miniButton))
                    {
                        if (sceneIsLoaded)
                        {
                            // Close/Unload the scene
                            Scene sceneToClose = !string.IsNullOrEmpty(scenePath) ?
                                SceneManager.GetSceneByPath(scenePath) :
                                SceneManager.GetSceneByName(sceneName);

                            if (sceneToClose.IsValid() && sceneToClose.isLoaded)
                            {
                                // Don't close if it's the only scene or the active scene
                                if (SceneManager.sceneCount > 1 && !isCurrentScene)
                                {
                                    // Save if modified before closing
                                    if (sceneToClose.isDirty)
                                    {
                                        if (EditorUtility.DisplayDialog("Save Scene",
                                            $"Scene '{sceneName}' has unsaved changes. Save before unloading?",
                                            "Save", "Don't Save"))
                                        {
                                            EditorSceneManager.SaveScene(sceneToClose);
                                        }
                                    }
                                    EditorSceneManager.CloseScene(sceneToClose, false);
                                }
                                else if (isCurrentScene)
                                {
                                    EditorUtility.DisplayDialog("Cannot Close Scene",
                                        $"Cannot unload '{sceneName}' because it's the active scene. Switch to another scene first.",
                                        "OK");
                                }
                                else
                                {
                                    EditorUtility.DisplayDialog("Cannot Close Scene",
                                        $"Cannot unload '{sceneName}' because it's the only scene open. At least one scene must remain loaded.",
                                        "OK");
                                }
                            }
                        }
                        else
                        {
                            // Open/Load the scene
                            if (string.IsNullOrEmpty(scenePath))
                            {
                                // If we don't have the path, try to find it again
                                string[] sceneGuids = AssetDatabase.FindAssets($"t:Scene {sceneName}");
                                if (sceneGuids.Length > 0)
                                {
                                    scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[0]);
                                }
                            }

                            if (!string.IsNullOrEmpty(scenePath))
                            {
                                // Open scene additively without closing others
                                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

                                // Set the newly opened scene as active
                                Scene newScene = SceneManager.GetSceneByPath(scenePath);
                                if (newScene.IsValid())
                                {
                                    SceneManager.SetActiveScene(newScene);
                                }
                            }
                            else
                            {
                                EditorUtility.DisplayDialog("Scene Not Found",
                                    $"Could not find scene file for '{sceneName}'. The scene may have been moved or deleted.",
                                    "OK");
                            }
                        }
                    }

                    GUI.backgroundColor = new Color(1f, 0.7f, 0.7f);
                    if (GUILayout.Button(new GUIContent("Clear", $"Delete all {bookmarkCount} bookmarks from scene '{sceneName}'"), EditorStyles.miniButton, GUILayout.Width(50)))
                    {
                        if (EditorUtility.DisplayDialog("Clear Scene Bookmarks",
                            $"Are you sure you want to remove all {bookmarkCount} bookmarks from scene '{sceneName}'?",
                            "Yes", "Cancel"))
                        {
                            Undo.RecordObject(database, $"Clear bookmarks for {sceneName}");
                            database.RemoveBookmarksForScene(sceneName);
                            RefreshFoldouts();
                            MarkDatabaseDirty();
                        }
                    }

                    GUI.backgroundColor = Color.white;
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawBookmarksList()
        {
            var currentBookmarks = GetCurrentBookmarks();

            if (showAllScenes)
            {
                EditorGUILayout.LabelField("All Bookmarks", EditorStyles.boldLabel);
            }
            else
            {
                EditorGUILayout.LabelField($"Bookmarks for '{currentSceneName}'", EditorStyles.boldLabel);
            }

            for (int i = 0; i < currentBookmarks.Count; i++)
            {
                if (i >= foldouts.Count)
                    foldouts.Add(false);

                SceneBookmark bookmark = currentBookmarks[i];
                int actualIndex = database.bookmarks.IndexOf(bookmark);

                GameObject target = bookmark.FindTargetObject();
                bookmark.cachedTarget = target;

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();

                // Show scene name if showing all scenes
                string displayName = bookmark.name;
                if (showAllScenes && !string.IsNullOrEmpty(bookmark.sceneName))
                {
                    displayName = $"[{bookmark.sceneName}] {bookmark.name}";
                }

                foldouts[i] = EditorGUILayout.Foldout(foldouts[i], displayName, true);

                // Disable "Go" button if bookmark is from different scene
                bool canGo = bookmark.BelongsToScene(currentSceneName, currentSceneGuid);
                GUI.enabled = canGo;
                GUI.backgroundColor = canGo ? new Color(0.3f, 1f, 0.5f) : new Color(0.7f, 0.7f, 0.7f);

                string goTooltip = canGo ?
                    "Navigate to this bookmark position" :
                    $"Cannot navigate - bookmark is from scene '{bookmark.sceneName}' but current scene is '{currentSceneName}'";

                if (GUILayout.Button(new GUIContent("Go", goTooltip), GUILayout.Width(50)))
                {
                    GoToBookmark(bookmark, target);
                }
                GUI.backgroundColor = Color.white;
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                if (foldouts[i])
                {
                    if (renameIndex == actualIndex)
                    {
                        bookmark.name = EditorGUILayout.TextField("Name", bookmark.name);

                        GUI.backgroundColor = new Color(1f, 1f, 0.4f);
                        if (GUILayout.Button(new GUIContent("Save", "Save the new bookmark name")))
                        {
                            renameIndex = -1;
                            MarkDatabaseDirty();
                        }
                        GUI.backgroundColor = Color.white;
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Name", bookmark.name);

                        // Show scene info
                        if (!string.IsNullOrEmpty(bookmark.sceneName))
                        {
                            EditorGUILayout.LabelField("Scene", bookmark.sceneName);
                        }

                        EditorGUILayout.BeginHorizontal();

                        GUI.backgroundColor = new Color(1f, 0.8f, 0.4f);
                        if (GUILayout.Button(new GUIContent("Rename", "Change the bookmark name")))
                        {
                            renameIndex = actualIndex;
                        }

                        GUI.backgroundColor = new Color(0.6f, 0.9f, 1.0f);
                        if (GUILayout.Button(new GUIContent("Reset Position", "Update bookmark to current Scene View position or target object position")))
                        {
                            ResetBookmarkPosition(bookmark, target);
                            MarkDatabaseDirty();
                        }

                        GUI.backgroundColor = Color.white;
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUI.BeginChangeCheck();
                    GameObject newTarget = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Target Object", "GameObject that will be moved to bookmark position when navigating. Leave empty for camera-only bookmarks"), target, typeof(GameObject), true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        bookmark.SetTargetObject(newTarget);
                        MarkDatabaseDirty();
                    }

                    GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                    if (GUILayout.Button(new GUIContent("Remove", "Delete this bookmark permanently")))
                    {
                        Undo.RecordObject(database, "Remove Bookmark");
                        database.bookmarks.RemoveAt(actualIndex);
                        foldouts.RemoveAt(i);
                        renameIndex = -1;
                        MarkDatabaseDirty();
                        GUI.backgroundColor = Color.white;
                        break;
                    }
                    GUI.backgroundColor = Color.white;
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void AddBookmark()
        {
            if (string.IsNullOrWhiteSpace(bookmarkName)) return;

            SceneView view = SceneView.lastActiveSceneView;
            if (view == null) return;

            // Get the Scene View camera and pivot information
            Vector3 cameraPosition = view.camera.transform.position;
            Quaternion cameraRotation = view.camera.transform.rotation;
            Vector3 pivotPoint = view.pivot;

            // Calculate the distance from pivot to camera
            float distance = Vector3.Distance(pivotPoint, cameraPosition);

            // If we have a target object and keepRotation is enabled, use target's rotation
            if (keepRotation && targetObject)
                cameraRotation = targetObject.transform.rotation;

            var bookmark = new SceneBookmark
            {
                name = bookmarkName,
                position = cameraPosition,      // Scene View camera position
                rotation = cameraRotation,      // Scene View camera rotation
                pivot = pivotPoint,             // Scene View pivot point
                cameraDistance = distance       // Distance from pivot to camera
            };

            // Set scene info and target object properly
            bookmark.UpdateSceneInfo();
            bookmark.SetTargetObject(targetObject);

            Undo.RecordObject(database, "Add Bookmark");
            database.bookmarks.Add(bookmark);
            RefreshFoldouts();
            MarkDatabaseDirty();

            // Move the Target Object to the bookmark position ONLY if it exists
            if (targetObject != null)
            {
                Undo.RecordObject(targetObject.transform, "Move Target to Bookmark Position");
                targetObject.transform.position = cameraPosition;
                targetObject.transform.rotation = cameraRotation;
            }

            // Clear the name and keep the target selected
            bookmarkName = "";
        }

        private void GoToBookmark(SceneBookmark bookmark, GameObject target)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null) return;

            if (target != null)
            {
                // Move the target object to the bookmark position
                Undo.RecordObject(target.transform, "Move Target to Bookmark");
                target.transform.position = bookmark.position;
                target.transform.rotation = bookmark.rotation;

                // Frame the target object in the Scene View
                Renderer renderer = target.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    sceneView.Frame(renderer.bounds, instant: true);
                }
                else
                {
                    // If no renderer, restore the exact Scene View state
                    sceneView.pivot = bookmark.pivot;
                    sceneView.rotation = bookmark.rotation;
                    //sceneView.size = bookmark.cameraDistance;
                    sceneView.Repaint();
                }
            }
            else
            {
                // No target object - restore the exact Scene View state
                // Use the stored pivot and distance to recreate the exact camera position
                sceneView.pivot = bookmark.pivot;
                sceneView.rotation = bookmark.rotation;
                //sceneView.size = bookmark.cameraDistance;

                // Force the Scene View to update immediately
                sceneView.Repaint();

                // Alternative approach: Set the camera transform directly
                // This ensures the exact camera position is restored
                if (sceneView.camera != null)
                {
                    sceneView.camera.transform.position = bookmark.position;
                    sceneView.camera.transform.rotation = bookmark.rotation;
                }
            }
        }

        private void ResetBookmarkPosition(SceneBookmark bookmark, GameObject target)
        {
            if (target != null)
            {
                bookmark.position = target.transform.position;
                bookmark.rotation = target.transform.rotation;
                bookmark.pivot = target.transform.position;
                bookmark.cameraDistance = 5f; // Default distance when using target
            }
            else
            {
                SceneView view = SceneView.lastActiveSceneView;
                if (view != null)
                {
                    bookmark.position = view.camera.transform.position;
                    bookmark.rotation = view.camera.transform.rotation;
                    bookmark.pivot = view.pivot;
                    bookmark.cameraDistance = Vector3.Distance(view.pivot, view.camera.transform.position);
                }
            }
        }

        private void MarkDatabaseDirty()
        {
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
        }
    }
}
