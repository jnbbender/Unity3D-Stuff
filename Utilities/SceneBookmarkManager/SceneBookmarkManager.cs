using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NastyDiaper
{
    public class SceneBookmarkManager : EditorWindow
    {
        private string bookmarkName = "New Bookmark";
        private GameObject targetObject;
        private bool keepRotation;
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
            const string path = "Assets/Editor/SceneBookmarkDatabase.asset";
            database = AssetDatabase.LoadAssetAtPath<SceneBookmarkDatabase>(path);

            if (database == null)
            {
                database = CreateInstance<SceneBookmarkDatabase>();
                System.IO.Directory.CreateDirectory("Assets/Editor");
                AssetDatabase.CreateAsset(database, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            foldouts = new List<bool>(new bool[database.bookmarks.Count]);
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Add New Bookmark", EditorStyles.boldLabel);
            bookmarkName = EditorGUILayout.TextField("Name", bookmarkName);
            targetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", targetObject, typeof(GameObject), true);
            keepRotation = EditorGUILayout.Toggle("Keep Rotation", keepRotation);

            GUI.backgroundColor = new Color(0.6f, 0.8f, 1f);
            if (GUILayout.Button("Add"))
            {
                AddBookmark();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Bookmarks", EditorStyles.boldLabel);

            for (int i = 0; i < database.bookmarks.Count; i++)
            {
                if (i >= foldouts.Count)
                    foldouts.Add(false);

                SceneBookmark bookmark = database.bookmarks[i];
                GameObject target = bookmark.cachedTarget ?? FindTargetByUUID(bookmark.uuid);
                if (target == null)
                {
                    // fallback: try path lookup
                    target = GameObject.Find(bookmark.targetPath);
                }
                bookmark.cachedTarget = target;

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                foldouts[i] = EditorGUILayout.Foldout(foldouts[i], bookmark.name, true);

                GUI.backgroundColor = new Color(0.3f, 1f, 0.5f);
                if (GUILayout.Button("Go", GUILayout.Width(50)))
                {
                    GoToBookmark(bookmark, target);
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                if (foldouts[i])
                {
                    if (renameIndex == i)
                    {
                        EditorGUI.BeginChangeCheck();
                        bookmark.name = EditorGUILayout.TextField("Name", bookmark.name);
                        if (EditorGUI.EndChangeCheck())
                        {
                            MarkDatabaseDirty();
                        }

                        GUI.backgroundColor = new Color(1f, 1f, 0.4f);
                        if (GUILayout.Button("Save"))
                        {
                            renameIndex = -1;
                            MarkDatabaseDirty();
                        }
                        GUI.backgroundColor = Color.white;
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Name", bookmark.name);
                        EditorGUILayout.BeginHorizontal();

                        GUI.backgroundColor = new Color(1f, 0.8f, 0.4f);
                        if (GUILayout.Button("Rename"))
                        {
                            renameIndex = i;
                        }

                        GUI.backgroundColor = new Color(0.6f, 0.9f, 1.0f);
                        if (GUILayout.Button("Reset Position"))
                        {
                            ResetBookmarkPosition(bookmark, target);
                            MarkDatabaseDirty();
                        }

                        GUI.backgroundColor = Color.white;
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUI.BeginChangeCheck();
                    GameObject newTarget = (GameObject)EditorGUILayout.ObjectField("Target Object", target, typeof(GameObject), true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        string newUUID = null;
                        string newPath = null;

                        if (newTarget)
                        {
                            BookmarkTarget tag = newTarget.GetComponent<BookmarkTarget>();
                            if (tag == null)
                            {
                                tag = newTarget.AddComponent<BookmarkTarget>();
                                EditorUtility.SetDirty(tag);
                            }
                            newUUID = tag.uuid;
                            newPath = GetHierarchyPath(newTarget);
                        }

                        bookmark.uuid = newUUID;
                        bookmark.targetPath = newPath;
                        MarkDatabaseDirty();
                    }

                    GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                    if (GUILayout.Button("Remove"))
                    {
                        Undo.RecordObject(database, "Remove Bookmark");
                        database.bookmarks.RemoveAt(i);
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

            EditorGUILayout.EndScrollView();
        }

        private void AddBookmark()
        {
            if (string.IsNullOrWhiteSpace(bookmarkName)) return;

            SceneView view = SceneView.lastActiveSceneView;
            if (view == null) return;

            Quaternion rotation = view.camera.transform.rotation;
            if (keepRotation && targetObject)
                rotation = targetObject.transform.rotation;

            string uuid = null;
            string path = null;
            if (targetObject)
            {
                BookmarkTarget tag = targetObject.GetComponent<BookmarkTarget>();
                if (tag == null)
                {
                    tag = targetObject.AddComponent<BookmarkTarget>();
                    EditorUtility.SetDirty(tag);
                }
                uuid = tag.uuid;
                path = GetHierarchyPath(targetObject);
            }

            var bookmark = new SceneBookmark
            {
                name = bookmarkName,
                position = view.camera.transform.position,
                rotation = rotation,
                uuid = uuid,
                targetPath = path
            };

            Undo.RecordObject(database, "Add Bookmark");
            database.bookmarks.Add(bookmark);
            foldouts.Add(true);
            MarkDatabaseDirty();
        }

        private void GoToBookmark(SceneBookmark bookmark, GameObject target)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null) return;

            if (target != null)
            {
                Undo.RecordObject(target.transform, "Move Target to Bookmark");
                target.transform.position = bookmark.position;
                target.transform.rotation = bookmark.rotation;

                Renderer renderer = target.GetComponentInChildren<Renderer>();
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

        private void ResetBookmarkPosition(SceneBookmark bookmark, GameObject target)
        {
            if (target != null)
            {
                bookmark.position = target.transform.position;
                bookmark.rotation = target.transform.rotation;
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

        private string GetHierarchyPath(GameObject obj)
        {
            if (obj == null) return null;

            string path = obj.name;
            Transform current = obj.transform;
            while (current.parent != null)
            {
                current = current.parent;
                path = current.name + "/" + path;
            }
            return path;
        }

        private GameObject FindTargetByUUID(string uuid)
        {
            if (string.IsNullOrEmpty(uuid))
                return null;

            var allTargets = GameObject.FindObjectsOfType<BookmarkTarget>(true);
            foreach (var target in allTargets)
            {
                if (target.uuid == uuid)
                    return target.gameObject;
            }

            return null;
        }

        private void MarkDatabaseDirty()
        {
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
        }
    }
}
