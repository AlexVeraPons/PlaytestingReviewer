using UnityEngine;
using System;

namespace PlaytestingReviewer.Tracks
{
    [Serializable]
    public class EventToTrack
    {
        public MonoBehaviour eventSource;
        public string eventName;

        [NonSerialized] public Delegate eventDelegate;
        [NonSerialized] public System.Reflection.FieldInfo unityEventField;
        [NonSerialized] public System.Reflection.FieldInfo systemActionField;
        [NonSerialized] public Delegate systemActionDelegate;
    }
}