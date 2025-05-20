using System;
using System.IO;
using PlaytestingReviewer.Editors;
using PlaytestingReviewer.Video;
using PlaytestingReviewer.Tracks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PlaytestingReviewer.Tracks
{
    public class ReviewCreator : MonoBehaviour
    {
        [SerializeField] private TrackCollector _trackCollector;
        [SerializeField] private VideoCapture _videoCapture;

        private string _folderName;
        private string _folderPath;
        private bool _directoryCreated;

        private void Awake()
        {
            if (_trackCollector == null)
                _trackCollector = FindFirstObjectByType<TrackCollector>();
            if (_videoCapture == null)
                _videoCapture = FindFirstObjectByType<VideoCapture>();

            _folderName = "Review" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            CreateFutureDirectory();

            _videoCapture.outputPath = _folderPath;
            _videoCapture.outputFileName = _folderName + ".mp4";
        }

        private void CreateFutureDirectory()
        {
#if UNITY_EDITOR
            const string rootAssetsPath = "Assets/_Project/ReviewOutput";

            if (!AssetDatabase.IsValidFolder(rootAssetsPath))
                AssetDatabase.CreateFolder("Assets/_Project", "ReviewOutput");

            AssetDatabase.CreateFolder(rootAssetsPath, _folderName);

            _folderPath = Path.Combine(rootAssetsPath, _folderName).Replace("\\", "/");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            _directoryCreated = true;
#else
            string buildFolder = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string root = Path.Combine(buildFolder, "ReviewOutput");
            Directory.CreateDirectory(root);

            _folderPath = Path.Combine(root, _folderName);
            Directory.CreateDirectory(_folderPath);
            _directoryCreated = true;
#endif
        }

        private void OnDestroy()
        {
            if (!_directoryCreated) return;
            CreateReviewOutput();
        }

        private void CreateReviewOutput()
        {
            var tracks = _trackCollector?.GetTracks();
            string trackJsonPath = Path.Combine(
                _folderPath,
                _folderName + ".review.json"
            );
            TrackConverter.OutputTracksToJson(tracks, trackJsonPath);

#if UNITY_EDITOR
            var reviewAsset = ScriptableObject.CreateInstance<Review>();
            reviewAsset.tracksPath = trackJsonPath;
            reviewAsset.videoPath = Path.Combine(_folderPath, "Review" + ".mp4");

            AssetDatabase.CreateAsset(
                reviewAsset,
                Path.Combine(_folderPath, "Review.asset")
            );
            AssetDatabase.SaveAssets();
#endif
        }
    }
}