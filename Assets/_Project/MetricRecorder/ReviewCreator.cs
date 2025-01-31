using System;
using System.IO;
using PlaytestingReviewer.Editors;
using UnityEngine;
using PlaytestingReviewer.Video;
using UnityEditor;


namespace PlaytestingReviewer.Tracks
{
    public class ReviewCreator : MonoBehaviour
    {
        [SerializeField] private TrackCollector _trackCollector;

        [SerializeField] private VideoCapture _videoCapture;

        private string _folderName;
        private string _folderPath;

        private void Start()
        {
            if (_trackCollector == null)
            {
                _trackCollector = FindFirstObjectByType<TrackCollector>();
                if(_trackCollector == null) Debug.LogWarning("There is no TrackCollector on this scene.");
            }

            if (_videoCapture == null)
            {
                _videoCapture = FindFirstObjectByType<VideoCapture>();
            }

            CreateFutureDirectory();
            _videoCapture.outputPath = _folderPath;
            _videoCapture.outputFileName = _folderName + ".mp4";
        }

        private void CreateFutureDirectory()
        {
            _folderName = "Review" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");

            string relativePath = "Assets/StreamingAssets/Output/Review";

            if (!AssetDatabase.IsValidFolder(relativePath))
            {
                AssetDatabase.CreateFolder("Assets/StreamingAssets/Output", "Review");
            }

            // Create a new folder inside Review
            AssetDatabase.CreateFolder(relativePath, _folderName);
            _folderPath = relativePath + "/" + _folderName;

            // Refresh AssetDatabase to reflect changes in Unity
            AssetDatabase.Refresh();
        }

        private void OnDestroy()
        {
            CreateReviewObject();
        }

        private void CreateReviewObject()
        {
            TrackCollection tracks = _trackCollector.GetTracks();
            string reviewObjectTracksPath = _folderPath + "/" + _folderName + ".review";
            TrackConverter.TracksToJson(tracks,reviewObjectTracksPath);
            
            Review reviewObject = ScriptableObject.CreateInstance<Review>();
            reviewObject.tracksPath =reviewObjectTracksPath;
            reviewObject.videoPath =_folderPath + "/" + _folderName + ".mp4";            

            //save the asset into the folder
            AssetDatabase.CreateAsset(reviewObject, _folderPath + "/"+"Review.asset");
        }
    }
}