using System.Collections.Generic;
using UnityEngine;

namespace PlaytestingReviewer.Tracks
{
    [System.Serializable]
    public class Track
    {
        public TrackType type;
        public string iconName;
        public string name;
        public Color color;
        [SerializeField] public List<SerializableDictionary> instances = new List<SerializableDictionary>();
    }

    public enum TrackType
    {
        Metric
    }

 
}