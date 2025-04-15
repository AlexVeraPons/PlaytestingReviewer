using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlaytestingReviewer.Tracks
{
    /// <summary>
    /// Designated to store the tracks from all metric recorders and then is used to create the review object 
    /// </summary>
    public class TrackCollector : MonoBehaviour
    {
        private List<Track> _tracks = new List<Track>();

        public void AddTrack(Track track)
        {
            _tracks.Add(track);
        }

        public TrackCollection GetTracks()
        {
            if(_tracks == null) return null;
            TrackCollection tracks = new TrackCollection
            {
                tracks = _tracks.ToArray()
            };
            return tracks;
        }

    }
}

