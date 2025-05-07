using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlaytestingReviewer.Tracks
{
    /// <summary>
    /// Designated to store the tracks from all metric recorders and then is used to create the review object.
    /// Now implemented as a singleton.
    /// </summary>
    public class TrackCollector : MonoBehaviour
    {
        public static TrackCollector Instance { get; private set; }

        private readonly List<Track> _tracks = new List<Track>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void AddTrack(Track track)
        {
            Track existing = null;
            foreach (var t  in _tracks)
            {
                if (t.name == track.name)
                {
                    existing = t ;
                    break;
                }
            }

            if (existing != null)
            {
                var toAdd = new List<SerializableDictionary>(track.instances);
                foreach (var inst in toAdd)
                    existing.instances.Add(inst);
            }
            else
            {
                _tracks.Add(track);
            }
        }


        public TrackCollection GetTracks()
        {
            if (_tracks == null || _tracks.Count == 0)
                return null;

            return new TrackCollection
            {
                tracks = _tracks.ToArray()
            };
        }
    }
}