using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NastyDiaper
{
    [DisallowMultipleComponent]
    public class BookmarkTarget : MonoBehaviour
    {
        [HideInInspector]
        public string uuid = System.Guid.NewGuid().ToString();

#if UNITY_EDITOR
        private void Reset()
        {
            if (string.IsNullOrEmpty(uuid))
            {
                uuid = System.Guid.NewGuid().ToString();
                EditorUtility.SetDirty(this);
            }
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(uuid))
            {
                uuid = System.Guid.NewGuid().ToString();
                EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}
