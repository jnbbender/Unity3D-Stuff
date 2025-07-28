using UnityEngine;

namespace NastyDiaper
{
    [System.Serializable]
    public class SceneBookmark
    {
        public string name;
        public Vector3 position;
        public Quaternion rotation;

        [HideInInspector]
        public string uuid;

        [HideInInspector]
        public string targetPath;

        [System.NonSerialized]
        public GameObject cachedTarget;
    }
}
