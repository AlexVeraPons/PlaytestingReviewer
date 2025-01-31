using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;

namespace PlaytestingReviewer.Tracks
{
    [System.Serializable]
    public class PropertyToTrack
    {
        public MonoBehaviour targetObject; // The MonoBehaviour to track
        [FormerlySerializedAs("name")] public string propertyName; // The property to extract
        public object value => targetObject.GetType();

        public object GetValue()
        {
            if (targetObject == null || string.IsNullOrEmpty(propertyName))
                return null;

            Type type = targetObject.GetType();
            PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);    
            FieldInfo field = type.GetField(propertyName, BindingFlags.Public | BindingFlags.Instance);

            if (property != null)
                return property.GetValue(targetObject);
            if (field != null)
                return field.GetValue(targetObject);

            return null;
        }
    }
}