using System;
using UnityEditor;
using UnityEngine;

namespace NovaDot.LoreForger.Settings
{
    [Serializable]
    public class LFSettings : ScriptableObject
    {
        public string GraphsFolderPath = "Assets/Editor/LoreForger/Graphs/";
        public string DialoguesFolderPath = "Assets/Dialogues/";

        public static LFSettings Load()
        {
            CreateFolders("Assets/Resources/NovaDot");

            string path = "Assets/Resources/NovaDot/LoreForgerSettings.asset";
            LFSettings settings = AssetDatabase.LoadAssetAtPath<LFSettings>(path);

            if (settings == null)
            {
                settings = CreateInstance<LFSettings>();

                AssetDatabase.CreateAsset(settings, path); 
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return settings;
        }

        public void Save(LFSettings settings)
        {
            AssetDatabase.CreateAsset(settings, "Assets/Resources/NovaDot/LoreForgerSettings.asset");
            AssetDatabase.SaveAssets();
        }

        private static void CreateFolders(string fullPath)
        {
            if (AssetDatabase.IsValidFolder(fullPath))
                return;

            string[] parts = fullPath.Split('/');
            string currentPath = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string nextPath = currentPath + "/" + parts[i];

                if (!AssetDatabase.IsValidFolder(nextPath))
                    AssetDatabase.CreateFolder(currentPath, parts[i]);

                currentPath = nextPath;
            }
        }

        #region Utility Methods

        public static void GetFolderAndPath(string fullPath, out string path, out string folder)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                path = string.Empty;
                folder = string.Empty;
                return;
            }

            // Remove trailing slash if it exists
            if (fullPath.EndsWith("/"))
                fullPath = fullPath.Substring(0, fullPath.Length - 1);

            int lastSlashIndex = fullPath.LastIndexOf('/');

            if (lastSlashIndex < 0)
            {
                path = string.Empty;
                folder = fullPath;
                return;
            }

            path = fullPath.Substring(0, lastSlashIndex);
            folder = fullPath.Substring(lastSlashIndex + 1);
        }

        #endregion
    }
}