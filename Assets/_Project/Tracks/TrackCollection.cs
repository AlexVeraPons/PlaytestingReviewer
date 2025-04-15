using System;

namespace PlaytestingReviewer.Tracks
{
    [System.Serializable]
    public class TrackCollection
    {
        public Track[] tracks; 

        public TrackCollection() { }
        public TrackCollection(Track[] tracks) { this.tracks = tracks; }
        public TrackCollection(string pathToJson)
        {
            TrackCollection trackCollection = TrackConverter.JsonPathToTracks(pathToJson);
            tracks = trackCollection != null ? trackCollection.tracks : Array.Empty<Track>();
        }
    }
}