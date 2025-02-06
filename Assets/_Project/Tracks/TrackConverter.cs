using System.IO;
using UnityEngine;

namespace PlaytestingReviewer.Tracks
{
    public static class TrackConverter
    {
        public static TrackCollection JsonPathToTracks(string pathToJson)
        {
            if (!File.Exists(pathToJson))
            {
                Debug.LogError($"JSON file not found at path: {pathToJson}");
                return null;
            }

            string jsonContent;
            try
            {
                jsonContent = File.ReadAllText(pathToJson);
            }
            catch (IOException e)
            {
                Debug.LogError($"Error reading JSON file: {e.Message}");
                return null;
            }

            TrackCollection trackCollection;
            try
            {
                trackCollection = JsonUtility.FromJson<TrackCollection>(jsonContent);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error parsing JSON file: {e.Message}");
                return null;
            }

            return trackCollection;
        }

        public static void OutputTracksToJson(Track[] tracks, string pathToJson)
        {
            TrackCollection trackCollection = new TrackCollection { tracks = tracks };
            OutputTracksToJson(trackCollection, pathToJson);
        }

        public static void OutputTracksToJson(TrackCollection tracks, string pathToJson)
        {
            string json = JsonUtility.ToJson(tracks, true);

            try
            {
                File.WriteAllText(pathToJson, json);
                Debug.Log($"Tracks saved to: {pathToJson}");
            }
            catch (IOException e)
            {
                Debug.LogError($"Error writing JSON file: {e.Message}");
            }
        }
    }
}