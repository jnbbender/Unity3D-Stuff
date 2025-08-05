using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NastyDiaper
{
    public class SceneBookmarkDatabase : ScriptableObject
    {
        public List<SceneBookmark> bookmarks = new List<SceneBookmark>();

        // Get all bookmarks for a specific scene
        public List<SceneBookmark> GetBookmarksForScene(string sceneName, string sceneGuid = null)
        {
            return bookmarks.Where(b => b.BelongsToScene(sceneName, sceneGuid)).ToList();
        }

        // Get all unique scene names that have bookmarks
        public List<string> GetScenesWithBookmarks()
        {
            return bookmarks.Where(b => !string.IsNullOrEmpty(b.sceneName))
                           .Select(b => b.sceneName)
                           .Distinct()
                           .OrderBy(name => name)
                           .ToList();
        }

        // Remove all bookmarks for a specific scene
        public void RemoveBookmarksForScene(string sceneName, string sceneGuid = null)
        {
            bookmarks.RemoveAll(b => b.BelongsToScene(sceneName, sceneGuid));
        }

        // Get bookmark count for a specific scene
        public int GetBookmarkCountForScene(string sceneName, string sceneGuid = null)
        {
            return bookmarks.Count(b => b.BelongsToScene(sceneName, sceneGuid));
        }

        // Migrate old bookmarks without scene info to current scene
        public void MigrateOldBookmarks()
        {
#if UNITY_EDITOR
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            string currentSceneName = activeScene.name;
            string currentSceneGuid = UnityEditor.AssetDatabase.AssetPathToGUID(activeScene.path);

            foreach (var bookmark in bookmarks)
            {
                if (string.IsNullOrEmpty(bookmark.sceneName))
                {
                    bookmark.sceneName = currentSceneName;
                    bookmark.sceneGuid = currentSceneGuid;
                }
            }
#endif
        }
    }
}
