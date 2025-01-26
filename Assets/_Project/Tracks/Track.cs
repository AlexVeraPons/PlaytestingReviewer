using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PlaytestingReviewer.Tracks
{
    [System.Serializable]
    public class Track
    {
        public TrackType type;
        public string iconName;
        public string name;
        [SerializeField]
        public List<SerializableDictionary> instances = new List<SerializableDictionary>();
    }

    public class TrackConverter
    {
        public static TrackCollection JsonToTracks(string pathToJson)
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

        public static void TracksToJson(Track[] tracks, string pathToJson)
        {
            TrackCollection trackCollection = new TrackCollection { tracks = tracks };

            string json = JsonUtility.ToJson(trackCollection, true);

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

        public static void TracksToJson(TrackCollection tracks, string pathToJson)
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

    [System.Serializable]
    public class TrackCollection
    {
        public Track[] tracks;
    }

    public enum TrackType
    {
        Metric
    }
}