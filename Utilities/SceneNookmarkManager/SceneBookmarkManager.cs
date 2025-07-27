using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NastyDiaper
{
    [System.Serializable]
    public class SceneBookmark
    {
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        public GameObject target;
    }

    public class SceneBookmarkDatabase : ScriptableObject
    {
        public List<SceneBookmark> bookmarks = new List<SceneBookmark>();
    }

    public class SceneBookmarkManager : EditorWindow
    {
        private string bookmarkName = "New Bookmark";
        private GameObject targetObject;
        private SceneBookmarkDatabase database;
        private int renameIndex = -1;
        private List<bool> foldouts = new List<bool>();
        private Vector2 scrollPosition;

        [MenuItem("Tools/Nasty Diaper/Scene Bookmark Manager")]
        public static void ShowWindow()
        {
            GetWindow<SceneBookmarkManager>("Scene Bookmarks");
        }

        private void OnEnable()
        {
            string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            string directory = System.IO.Path.GetDirectoryName(scriptPath);
            string path = System.IO.Path.Combine(directory, "SceneBookmarkDatabase.asset").Replace("\\", "/");

            database = AssetDatabase.LoadAssetAtPath<SceneBookmarkDatabase>(path);

            if (database == null)
            {
                database = CreateInstance<SceneBookmarkDatabase>();
                AssetDatabase.CreateAsset(database, path);
                AssetDatabase.SaveAssets();
            }

            if (foldouts.Count != database.bookmarks.Count)
            {
                foldouts = new List<bool>(new bool[database.bookmarks.Count]);
            }
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Add New Bookmark", EditorStyles.boldLabel);
            bookmarkName = EditorGUILayout.TextField("Name", bookmarkName);
            targetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", targetObject, typeof(GameObject), true);

            GUI.backgroundColor = new Color(0.6f, 0.8f, 1f); // Light blue
            if (GUILayout.Button("Add"))
            {
                AddBookmark();
                AssetDatabase.SaveAssets();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Bookmarks", EditorStyles.boldLabel);

            for (int i = 0; i < database.bookmarks.Count; i++)
            {
                if (i >= foldouts.Count)
                    foldouts.Add(false);

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                foldouts[i] = EditorGUILayout.Foldout(foldouts[i], database.bookmarks[i].name, true);

                GUI.backgroundColor = new Color(0.3f, 1f, 0.5f); // Green
                if (GUILayout.Button("Go", GUILayout.Width(50)))
                {
                    GoToBookmark(database.bookmarks[i]);
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                if (foldouts[i])
                {
                    var bookmark = database.bookmarks[i];

                    if (renameIndex == i)
                    {
                        bookmark.name = EditorGUILayout.TextField("Name", bookmark.name);

                        GUI.backgroundColor = new Color(1f, 1f, 0.4f); // Yellow
                        if (GUILayout.Button("Save"))
                        {
                            renameIndex = -1;
                            EditorUtility.SetDirty(database);
                            AssetDatabase.SaveAssets();
                        }
                        GUI.backgroundColor = Color.white;
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Name", bookmark.name);
                        EditorGUILayout.BeginHorizontal();

                        GUI.backgroundColor = new Color(1f, 0.8f, 0.4f); // Orange
                        if (GUILayout.Button("Rename"))
                        {
                            renameIndex = i;
                        }

                        GUI.backgroundColor = new Color(0.6f, 0.9f, 1.0f); // Light blue
                        if (GUILayout.Button("Reset Position"))
                        {
                            ResetBookmarkPosition(bookmark);
                            EditorUtility.SetDirty(database);
                            AssetDatabase.SaveAssets();
                        }

                        GUI.backgroundColor = Color.white;
                        EditorGUILayout.EndHorizontal();
                    }

                    bookmark.target = (GameObject)EditorGUILayout.ObjectField("Target Object", bookmark.target, typeof(GameObject), true);

                    GUI.backgroundColor = new Color(1f, 0.4f, 0.4f); // Light red
                    if (GUILayout.Button("Remove"))
                    {
                        database.bookmarks.RemoveAt(i);
                        foldouts.RemoveAt(i);
                        renameIndex = -1;
                        EditorUtility.SetDirty(database);
                        AssetDatabase.SaveAssets();
                        GUI.backgroundColor = Color.white;
                        break;
                    }
                    GUI.backgroundColor = Color.white;
                }

                EditorGUILayout.EndVertical();
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(database);
            }

            EditorGUILayout.EndScrollView();
        }

        private void AddBookmark()
        {
            if (string.IsNullOrWhiteSpace(bookmarkName)) return;

            SceneView view = SceneView.lastActiveSceneView;
            if (view == null) return;

            var bookmark = new SceneBookmark
            {
                name = bookmarkName,
                position = view.camera.transform.position,
                rotation = view.camera.transform.rotation,
                target = targetObject
            };

            database.bookmarks.Add(bookmark);
            foldouts.Add(true);
            EditorUtility.SetDirty(database);
        }

        private void GoToBookmark(SceneBookmark bookmark)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null) return;

            if (bookmark.target != null)
            {
                Undo.RecordObject(bookmark.target.transform, "Move Target to Bookmark");
                bookmark.target.transform.position = bookmark.position;
                bookmark.target.transform.rotation = bookmark.rotation;

                Renderer renderer = bookmark.target.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    sceneView.Frame(renderer.bounds, instant: true);
                }
                else
                {
                    sceneView.pivot = bookmark.position;
                    sceneView.rotation = bookmark.rotation;
                    sceneView.size = 10f;
                    sceneView.Repaint();
                }
            }
            else
            {
                sceneView.pivot = bookmark.position;
                sceneView.rotation = bookmark.rotation;
                sceneView.size = 10f;
                sceneView.Repaint();
            }
        }

        private void ResetBookmarkPosition(SceneBookmark bookmark)
        {
            if (bookmark.target != null)
            {
                bookmark.position = bookmark.target.transform.position;
                bookmark.rotation = bookmark.target.transform.rotation;
            }
            else
            {
                SceneView view = SceneView.lastActiveSceneView;
                if (view != null)
                {
                    bookmark.position = view.camera.transform.position;
                    bookmark.rotation = view.camera.transform.rotation;
                }
            }
        }
    }
}
