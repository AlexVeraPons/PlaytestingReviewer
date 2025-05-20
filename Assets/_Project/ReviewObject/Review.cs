using UnityEngine;
using UnityEditor;
using PlaytestingReviewer.Tracks;

namespace PlaytestingReviewer.Editors
{
    /// <summary>
    /// Represents a review object that contains the paths to associated video and track collection data.
    /// </summary>
    [CreateAssetMenu(menuName = "PlaytestReviwer/Review")]
    public class Review : ScriptableObject
    {
        public string videoPath;
        public string tracksPath;
        public TrackCollection trackCollection { get; set; }

        public TrackCollection GetTrackCollecion()
        {
            return new TrackCollection(tracksPath);
        }
    }
}