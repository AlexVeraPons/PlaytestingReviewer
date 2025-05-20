using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;

namespace PlaytestingReviewer.Tracks
{
    [System.Serializable]
    public class PropertyToTrack
    {
        public MonoBehaviour targetObject;
        [FormerlySerializedAs("name")] public string propertyName;
        public object Value => targetObject.GetType();

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