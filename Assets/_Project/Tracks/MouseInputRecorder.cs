using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PlaytestingReviewer.Tracks
{
    public class MouseInputRecorder : MonoBehaviour
    {
        private Track MouseTrack;

        private void Start()
        {
            MouseTrack = new Track
            {
                type = TrackType.Metric,
                iconName = "Mouse.png",
                name = "Mouse Input Track",
                instances = new List<SerializableDictionary>()
            };
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var instance = new SerializableDictionary();
                instance.Add("time", Time.realtimeSinceStartup);
                instance.Add("button", "Left");
                instance.Add("position", $"x:{Input.mousePosition.x}, y:{Input.mousePosition.y}, z:{Input.mousePosition.z}");

                MouseTrack.instances.Add(instance);
            }

            if (Input.GetMouseButtonDown(1))
            {
                var instance = new SerializableDictionary();
                instance.Add("time", Time.realtimeSinceStartup);
                instance.Add("button", "Right");
                instance.Add("position", $"x:{Input.mousePosition.x}, y:{Input.mousePosition.y}, z:{Input.mousePosition.z}");

                MouseTrack.instances.Add(instance);
            }
        }

        private void OnDestroy()
        {
            string path = PathManager.VideoOutputPath + "/Mouse.json";
            TrackConverter.TracksToJson(new Track[] { MouseTrack }, path);
        }
    }

    [System.Serializable]
    public class SerializableDictionary
    {
        public List<SerializableKeyValuePair> keyValuePairs = new List<SerializableKeyValuePair>();

        public void Add(string key, object value)
        {
            keyValuePairs.Add(new SerializableKeyValuePair
            {
                key = key,
                value = value.ToString()
            });
        }

        public Dictionary<string, object> ToDictionary()
        {
            var dictionary = new Dictionary<string, object>();
            foreach (var kvp in keyValuePairs)
            {
                dictionary[kvp.key] = kvp.value;
            }
            return dictionary;
        }
    }

    [System.Serializable]
    public class SerializableKeyValuePair
    {
        public string key;
        public string value;
    }
}
