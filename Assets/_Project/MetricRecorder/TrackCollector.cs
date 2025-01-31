using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlaytestingReviewer.Tracks
{
    public class TrackCollector : MonoBehaviour
    {
        private List<Track> _tracks;

        public void AddTrack(Track track)
        {
            _tracks.Add(track);
        }

        public TrackCollection GetTracks()
        {
            TrackCollection tracks = new TrackCollection
            {
                tracks = _tracks.ToArray()
            };
            return tracks;
        }

    }
}

