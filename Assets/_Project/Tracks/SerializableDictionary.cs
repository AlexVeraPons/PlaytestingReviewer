using System.Collections.Generic;

namespace PlaytestingReviewer.Tracks
{
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