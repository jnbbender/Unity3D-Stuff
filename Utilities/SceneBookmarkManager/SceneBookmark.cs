using UnityEngine;

namespace NastyDiaper
{
    [System.Serializable]
    public class SceneBookmark
    {
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 pivot;               // Store Scene View pivot point
        public float cameraDistance = 10f;  // Store actual distance from pivot to camera
        public string sceneName;            // Store the scene name
        public string sceneGuid;            // Store the scene GUID for more reliable identification

        [HideInInspector]
        public string globalObjectId;

        // Additional fallback methods to find target objects
        [HideInInspector]
        public string targetObjectName;
        [HideInInspector]
        public string targetObjectPath;  // Hierarchy path like "Parent/Child/Target"
        [HideInInspector]
        public int targetInstanceId;     // For current session only

        [System.NonSerialized]
        public GameObject cachedTarget;

        // Method to update scene information (called from manager, not constructor)
        public void UpdateSceneInfo()
        {
#if UNITY_EDITOR
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            sceneName = activeScene.name;
            sceneGuid = UnityEditor.AssetDatabase.AssetPathToGUID(activeScene.path);
#endif
        }

        // Method to store target object information for better persistence
        public void SetTargetObject(GameObject target)
        {
            cachedTarget = target;

            if (target != null)
            {
#if UNITY_EDITOR
                var id = UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(target);
                globalObjectId = id.ToString();
#endif
                targetObjectName = target.name;
                targetInstanceId = target.GetInstanceID();
                targetObjectPath = GetGameObjectPath(target);
            }
            else
            {
                globalObjectId = null;
                targetObjectName = null;
                targetInstanceId = 0;
                targetObjectPath = null;
            }
        }

        // Get the full hierarchy path of a GameObject
        private string GetGameObjectPath(GameObject obj)
        {
            if (obj == null) return null;

            string path = obj.name;
            Transform parent = obj.transform.parent;

            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }

        // Enhanced method to find target object using multiple fallback strategies
        public GameObject FindTargetObject()
        {
            // Strategy 1: Use cached reference if it's still valid and in the right scene
            if (cachedTarget != null && IsTargetInCorrectScene(cachedTarget))
            {
                return cachedTarget;
            }

            // Strategy 2: Try GlobalObjectId (works best for prefabs and assets)
            if (!string.IsNullOrEmpty(globalObjectId))
            {
#if UNITY_EDITOR
                if (UnityEditor.GlobalObjectId.TryParse(globalObjectId, out var id))
                {
                    var obj = UnityEditor.GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as GameObject;
                    if (obj != null && IsTargetInCorrectScene(obj))
                    {
                        cachedTarget = obj;
                        return obj;
                    }
                }
#endif
            }

            // Strategy 3: Try to find by hierarchy path (most reliable for scene objects)
            if (!string.IsNullOrEmpty(targetObjectPath))
            {
                var obj = FindByPath(targetObjectPath);
                if (obj != null && IsTargetInCorrectScene(obj))
                {
                    cachedTarget = obj;
                    return obj;
                }
            }

            // Strategy 4: Find by name as last resort
            if (!string.IsNullOrEmpty(targetObjectName))
            {
                var obj = FindByName(targetObjectName);
                if (obj != null && IsTargetInCorrectScene(obj))
                {
                    cachedTarget = obj;
                    return obj;
                }
            }

            // Clear cached reference if nothing was found
            cachedTarget = null;
            return null;
        }

        private bool IsTargetInCorrectScene(GameObject obj)
        {
            if (obj == null) return false;

            var objScene = obj.scene;
            if (!objScene.IsValid()) return false;

            // Check if it's in the expected scene
            return objScene.name == sceneName;
        }

        private GameObject FindByPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (activeScene.name != sceneName) return null;

            // Try to find the object by traversing the hierarchy path
            string[] pathParts = path.Split('/');
            GameObject[] rootObjects = activeScene.GetRootGameObjects();

            foreach (var rootObj in rootObjects)
            {
                if (rootObj.name == pathParts[0])
                {
                    GameObject current = rootObj;

                    for (int i = 1; i < pathParts.Length; i++)
                    {
                        Transform child = current.transform.Find(pathParts[i]);
                        if (child == null) break;
                        current = child.gameObject;
                    }

                    // Check if we found the complete path
                    if (current.name == pathParts[pathParts.Length - 1])
                    {
                        return current;
                    }
                }
            }

            return null;
        }

        private GameObject FindByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (activeScene.name != sceneName) return null;

            GameObject[] rootObjects = activeScene.GetRootGameObjects();

            foreach (var rootObj in rootObjects)
            {
                var found = FindInChildren(rootObj.transform, name);
                if (found != null) return found.gameObject;
            }

            return null;
        }

        private Transform FindInChildren(Transform parent, string name)
        {
            if (parent.name == name) return parent;

            for (int i = 0; i < parent.childCount; i++)
            {
                var found = FindInChildren(parent.GetChild(i), name);
                if (found != null) return found;
            }

            return null;
        }

        // Check if this bookmark belongs to the specified scene
        public bool BelongsToScene(string checkSceneName, string checkSceneGuid = null)
        {
            // First try GUID comparison (more reliable)
            if (!string.IsNullOrEmpty(sceneGuid) && !string.IsNullOrEmpty(checkSceneGuid))
            {
                return sceneGuid == checkSceneGuid;
            }

            // Fallback to name comparison
            return sceneName == checkSceneName;
        }
    }
}
