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
        }

        private void OnDestroy()
        {
            CreateReviewObject();
        }

        private void CreateReviewObject()
        {
            //first create folder for the review
            string folderName = "Review" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            AssetDatabase.CreateFolder(PathManager.ReviewOutputPath,folderName);
            
            string path = PathManager.ReviewOutputPath + "/" + folderName;
            
            TrackCollection tracks = _trackCollector.GetTracks();
            string reviewObjectTracksPath = path + "/" + folderName + ".review";
            TrackConverter.TracksToJson(tracks,reviewObjectTracksPath);
            
            Review reviewObject = ScriptableObject.CreateInstance<Review>();
            reviewObject.tracksPath =reviewObjectTracksPath;
            reviewObject.videoPath =path + "/" + folderName + ".mp4";            

            //save the asset into the folder
            AssetDatabase.CreateAsset(reviewObject, path);
        }
    }
}