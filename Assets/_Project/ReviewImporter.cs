using System;
using System.IO;
using PlaytestingReviewer.Editors;
using UnityEditor;
using UnityEngine;

# if UNITY_EDITOR
namespace PlaytestingReviewer
{
    public static class ReviewImporter
    {
        [MenuItem("Tools/Playtesting/Import Review…")]
        [MenuItem("Tools/Playtesting/Import Review…")]
        private static void ImportReview()
        {
            string trackJsonPath = EditorUtility.OpenFilePanel(
                "Select Track JSON File",
                Application.dataPath,
                "json"
            );
            if (string.IsNullOrEmpty(trackJsonPath))
                return;

            string videoFilePath = EditorUtility.OpenFilePanel(
                "Select Video File",
                Application.dataPath,
                "mp4"
            );
            if (string.IsNullOrEmpty(videoFilePath))
                return;

            string reviewFolderName = Path.GetFileNameWithoutExtension(videoFilePath);
            string assetReviewFolder = Path.Combine(PathManager.ReviewOutputPath, reviewFolderName);

            EnsureFolderExists(assetReviewFolder);

            string newTrack = CopyIntoFolder(trackJsonPath, assetReviewFolder);
            string newVideo = CopyIntoFolder(videoFilePath, assetReviewFolder);

            Debug.Log($"Imported track → {newTrack}");
            Debug.Log($"Imported video → {newVideo}");

            var reviewAsset = ScriptableObject.CreateInstance<Review>();
            reviewAsset.tracksPath = newTrack;
            reviewAsset.videoPath = newVideo;

            string assetFileName = reviewFolderName + ".asset";
            string reviewAssetPath = Path.Combine(assetReviewFolder, assetFileName);

            AssetDatabase.CreateAsset(reviewAsset, reviewAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Import Review",
                $"Imported track & video into “{assetReviewFolder}”",
                "OK"
            );
        }

      
        private static string CopyIntoFolder(string sourcePath, string destAssetFolder)
        {
            string fileName = Path.GetFileName(sourcePath);

            string projectRoot = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
            string destinationRoot = Path.Combine(projectRoot, destAssetFolder);

            if (!Directory.Exists(destinationRoot))
                Directory.CreateDirectory(destinationRoot);

            string destinationFullPath = Path.Combine(destinationRoot, fileName);
            File.Copy(sourcePath, destinationFullPath, overwrite: true);

            string assetPath = Path.Combine(destAssetFolder, fileName);
            AssetDatabase.ImportAsset(assetPath);

            return assetPath.Replace("\\", "/");
        }

        private static void EnsureFolderExists(string assetsPath)
        {
            if (AssetDatabase.IsValidFolder(assetsPath)) return;

            string[] parts = assetsPath.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif