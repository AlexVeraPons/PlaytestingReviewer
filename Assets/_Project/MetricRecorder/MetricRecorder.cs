using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace PlaytestingReviewer.Tracks
{
    public class MetricRecorder : MonoBehaviour
    {
        public string metricName = "Metric";

        [SerializeField] private MonoBehaviour _eventSource; // The MonoBehaviour holding the event
        [SerializeField] private string _eventName; // The name of the event to track
        [SerializeField] private List<PropertyToTrack> _propertiesToTrack; // Properties to track

        private Track _track;

        private EventInfo _dotNetEvent; // For C# events
        private FieldInfo _unityEventField; // For UnityEvents
        private Delegate _eventDelegate; // Stored delegate for unsubscribing

        private void Start()
        {
            SubscribeToEvent();
            InitializeTrack();
        }

        private void InitializeTrack()
        {
            _track = new Track
            {
                type = TrackType.Metric,
                name = metricName,
                iconName = "DefaultIcon",
                instances = new List<SerializableDictionary>()
            };
        }

        private void SubscribeToEvent()
        {
            if (_eventSource == null || string.IsNullOrEmpty(_eventName))
                return;

            Type sourceType = _eventSource.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

            // Check for C# event
            _dotNetEvent = sourceType.GetEvent(_eventName, flags);
            if (_dotNetEvent != null)
            {
                Type eventHandlerType = _dotNetEvent.EventHandlerType;
                MethodInfo methodInfo = GetType().GetMethod(nameof(OnEventTriggered),
                    BindingFlags.NonPublic | BindingFlags.Instance);
                _eventDelegate = Delegate.CreateDelegate(eventHandlerType, this, methodInfo);

                _dotNetEvent.AddEventHandler(_eventSource, _eventDelegate);
                Debug.Log($"Subscribed to C# event: {_eventName}");
                return;
            }

            // Check for UnityEvent field
            _unityEventField = sourceType.GetField(_eventName, flags);
            if (_unityEventField != null && typeof(UnityEventBase).IsAssignableFrom(_unityEventField.FieldType))
            {
                UnityEventBase unityEvent = _unityEventField.GetValue(_eventSource) as UnityEventBase;
                if (unityEvent != null)
                {
                    MethodInfo addListenerMethod =
                        unityEvent.GetType().GetMethod("AddListener", new Type[] { typeof(UnityAction) });
                    if (addListenerMethod != null)
                    {
                        UnityAction action = OnUnityEventTriggered;
                        addListenerMethod.Invoke(unityEvent, new object[] { action });
                        Debug.Log($"Subscribed to UnityEvent: {_eventName}");
                    }
                }
            }
        }

        private void OnEventTriggered(object sender, EventArgs e)
        {
            LogEvent();
        }

        private void OnUnityEventTriggered()
        {
            LogEvent();
        }

        private void LogEvent()
        {
            foreach (var property in _propertiesToTrack)
            {
                var instance = new SerializableDictionary();
                instance.Add(property.name, property.value);
                Debug.Log("name: " + property.name + " value: " + property.value);
                _track.instances.Add(instance);
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvent();
            AddTrackToCollector();
        }

        private void AddTrackToCollector()
        {
            TrackCollector collector = FindFirstObjectByType<TrackCollector>();

            if (collector == null)
            {
                Debug.LogError("There is no TrackCollector in the scene.");
                return;
            }

            collector.AddTrack(_track);
        }

        private void UnsubscribeFromEvent()
        {
            if (_dotNetEvent != null && _eventDelegate != null)
            {
                _dotNetEvent.RemoveEventHandler(_eventSource, _eventDelegate);
                Debug.Log($"Unsubscribed from C# event: {_eventName}");
            }

            if (_unityEventField != null)
            {
                UnityEventBase unityEvent = _unityEventField.GetValue(_eventSource) as UnityEventBase;
                if (unityEvent != null)
                {
                    MethodInfo removeListenerMethod = unityEvent.GetType()
                        .GetMethod("RemoveListener", new Type[] { typeof(UnityAction) });
                    if (removeListenerMethod != null)
                    {
                        UnityAction action = OnUnityEventTriggered;
                        removeListenerMethod.Invoke(unityEvent, new object[] { action });
                        Debug.Log($"Unsubscribed from UnityEvent: {_eventName}");
                    }
                }
            }
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(MetricRecorder))]
    public class MetricRecorderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Reference to the actual script
            MetricRecorder recorder = (MetricRecorder)target;

            // Synchronize serialized object with actual fields
            serializedObject.Update();

            // Draw the MonoBehaviour where the event resides
            SerializedProperty eventSourceProp = serializedObject.FindProperty("_eventSource");
            EditorGUILayout.PropertyField(eventSourceProp, new GUIContent("Event Source"));

            // Dropdown for the event name
            SerializedProperty eventNameProp = serializedObject.FindProperty("eventName");

            if (eventSourceProp.objectReferenceValue != null)
            {
                MonoBehaviour eventSource = eventSourceProp.objectReferenceValue as MonoBehaviour;
                if (eventSource != null)
                {
                    List<string> availableEvents = GetAvailableEvents(eventSource) ?? new List<string>();
                    int selectedIndex = Mathf.Max(0, availableEvents.IndexOf(eventNameProp.stringValue));

                    selectedIndex = EditorGUILayout.Popup("Event", selectedIndex, availableEvents.ToArray());
                    if (selectedIndex >= 0 && selectedIndex < availableEvents.Count)
                    {
                        eventNameProp.stringValue = availableEvents[selectedIndex];
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tracked Properties", EditorStyles.boldLabel);

            SerializedProperty propertiesList = serializedObject.FindProperty("_propertiesToTrack");

            for (int i = propertiesList.arraySize - 1; i >= 0; i--) // Iterate backwards for safe removal
            {
                SerializedProperty propertyElement = propertiesList.GetArrayElementAtIndex(i);
                if (propertyElement == null)
                {
                    Debug.LogError($"Property element at index {i} is null!");
                    continue;
                }

                SerializedProperty targetObjectProp = propertyElement.FindPropertyRelative("targetObject");
                SerializedProperty propertyNameProp = propertyElement.FindPropertyRelative("propertyName");

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.PropertyField(targetObjectProp, new GUIContent("Target Object"));

                if (targetObjectProp.objectReferenceValue == null)
                {
                    Debug.LogWarning("Target Object is null! Assign an object in the Inspector.");
                }
                else
                {
                    MonoBehaviour targetMono = targetObjectProp.objectReferenceValue as MonoBehaviour;
                    if (targetMono != null)
                    {
                        List<string> availableProps = GetAvailableProperties(targetMono) ?? new List<string>();
                        int propIndex = Mathf.Max(0, availableProps.IndexOf(propertyNameProp.stringValue));
                        propIndex = EditorGUILayout.Popup("Property", propIndex, availableProps.ToArray());

                        if (propIndex >= 0 && propIndex < availableProps.Count)
                        {
                            propertyNameProp.stringValue = availableProps[propIndex];
                        }
                    }
                }

                if (GUILayout.Button("Remove"))
                {
                    propertiesList.DeleteArrayElementAtIndex(i);
                }

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Property"))
            {
                propertiesList.InsertArrayElementAtIndex(propertiesList.arraySize);
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Gets the list of public events (.NET events) and public UnityEvent fields
        /// from the target MonoBehaviour.
        /// </summary>
        private List<string> GetAvailableEvents(MonoBehaviour target)
        {
            List<string> eventNames = new List<string>();
            if (target == null) return eventNames;

            Type type = target.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

            // 1) Grab all public C# events
            var dotNetEvents = type.GetEvents(flags).Select(e => e.Name);
            eventNames.AddRange(dotNetEvents);

            // 2) Grab all public fields that are UnityEvents (or derived from UnityEventBase)
            var unityEventFields = type.GetFields(flags)
                .Where(field => typeof(UnityEventBase).IsAssignableFrom(field.FieldType))
                .Select(field => field.Name);
            eventNames.AddRange(unityEventFields);

            return eventNames;
        }

        /// <summary>
        /// Gets a list of public properties & fields from the given MonoBehaviour.
        /// </summary>
        private List<string> GetAvailableProperties(MonoBehaviour target)
        {
            List<string> propNames = new List<string>();
            if (target == null) return propNames;

            Type type = target.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

            // Public properties with a getter
            propNames.AddRange(type.GetProperties(flags)
                .Where(p => p.CanRead)
                .Select(p => p.Name));

            // Public fields
            propNames.AddRange(type.GetFields(flags)
                .Select(f => f.Name));

            return propNames;
        }
    }
}
#endif