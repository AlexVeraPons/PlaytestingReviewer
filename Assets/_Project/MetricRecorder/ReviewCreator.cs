using System;
using System.IO;
using PlaytestingReviewer.Editors;
using UnityEngine;
using PlaytestingReviewer.Video;
using UnityEditor;


namespace PlaytestingReviewer.Tracks
{
    ///<summary>
    /// Initializes the review output folder and configures the video capture and track collection settings.
    /// When the object is destroyed, it converts the collected tracks to JSON and creates a Review asset 
    /// that links the track data and video file for later analysis.
    /// </summary>
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

            string relativePath = "Assets/_Project/ReviewOutput";

            if (!AssetDatabase.IsValidFolder(relativePath))
            {
                AssetDatabase.CreateFolder("Assets/_Project/", "ReviewOutput");
            }

            AssetDatabase.CreateFolder(relativePath, _folderName);
            _folderPath = relativePath + "/" + _folderName;

            AssetDatabase.SaveAssets();
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
            TrackConverter.OutputTracksToJson(tracks,reviewObjectTracksPath);
            
            Review reviewObject = ScriptableObject.CreateInstance<Review>();
            reviewObject.tracksPath =reviewObjectTracksPath;
            reviewObject.videoPath =_folderPath + "/" + _folderName + ".mp4";            

            //save the asset into the folder
            AssetDatabase.CreateAsset(reviewObject, _folderPath + "/"+"Review.asset");
            AssetDatabase.SaveAssets();
        }
    }
}