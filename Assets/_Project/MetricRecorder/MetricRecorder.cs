using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using PlaytestingReviewer.Video;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace PlaytestingReviewer.Tracks
{
    /// <summary>
    /// The MetricRecorder is a Unity component that subscribes to events (including C# events, UnityEvents, and delegate fields) using reflection.
    /// And when one of these events is triggered it logs into a track to access in the future.
    /// While also recording metrics that are logged using reflection.
    /// </summary>
    public class MetricRecorder : MonoBehaviour
    {
        public string metricName = "Metric";
        public Color metricColor = Color.red;

        [SerializeField] private MonoBehaviour _eventSource; // The MonoBehaviour holding the event
        [SerializeField] private string _eventName; // The name of the event/delegate/UnityEvent to track
        [SerializeField] private List<PropertyToTrack> _propertiesToTrack; // Properties to track

        private float _selfCurrentTime = 0f;
        private float _currentTime => _videoCapture == null ? _selfCurrentTime : _videoCapture.CurrentVideoTime;
        private Track _track;

        // For standard .NET events:
        private EventInfo _dotNetEvent;
        private Delegate _eventDelegate;

        // For UnityEvents:
        private FieldInfo _unityEventField;

        // For delegate field support (System.Action, etc.):
        private FieldInfo _systemActionField;
        private Delegate _systemActionDelegate;


        private VideoCapture _videoCapture;

        private void Start()
        {
            SubscribeToEvent();
            InitializeTrack();
            _videoCapture = FindAnyObjectByType<VideoCapture>();
        }

        private void Update()
        {
            if (_videoCapture == null)
            {
                _selfCurrentTime += Time.deltaTime;
            }
        }

        private void InitializeTrack()
        {
            _track = new Track
            {
                type = TrackType.Metric,
                name = metricName,
                color = metricColor,
                iconName = "DefaultIcon",
                instances = new List<SerializableDictionary>()
            };
        }

        /// <summary>
        /// Subscribe to the event/delegate/UnityEvent defined in _eventSource with name _eventName.
        /// Supports .NET events, UnityEvents, and now System.Action (delegate fields).
        /// </summary>
        private void SubscribeToEvent()
        {
            if (_eventSource == null || string.IsNullOrEmpty(_eventName))
                return;

            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

            // Try to find a C# event first.
            _dotNetEvent = _eventSource.GetType().GetEvent(_eventName, flags);
            if (_dotNetEvent != null)
            {
                Type eventHandlerType = _dotNetEvent.EventHandlerType;
                _eventDelegate = CreateDelegateForEvent(eventHandlerType);

                bool isStatic = _dotNetEvent.GetAddMethod().IsStatic;
                object target = isStatic ? null : _eventSource;
                _dotNetEvent.AddEventHandler(target, _eventDelegate);
                return;
            }

            // If not a C# event, check for a UnityEvent field.
            _unityEventField = _eventSource.GetType().GetField(_eventName, flags);
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
                    }
                }

                return;
            }

            // if not a standard .NET event or UnityEvent, see if it's a delegate field (e.g. System.Action)
            _systemActionField = _eventSource.GetType().GetField(_eventName, flags);
            if (_systemActionField != null && typeof(Delegate).IsAssignableFrom(_systemActionField.FieldType))
            {
                // Create a delegate that calls LogEvent() ignoring parameters (if generic).
                _systemActionDelegate = CreateDelegateForEvent(_systemActionField.FieldType);

                // Combine it with any existing delegate in that field.
                Delegate existing = _systemActionField.GetValue(_eventSource) as Delegate;
                Delegate combined = Delegate.Combine(existing, _systemActionDelegate);
                _systemActionField.SetValue(_eventSource, combined);
            }
        }

        /// <summary>
        /// Creates a delegate that matches any delegate signature by ignoring its parameters
        /// and simply calling LogEvent().
        /// </summary>
        /// <param name="delegateType">The type of the delegate expected by the event or field.</param>
        /// <returns>A delegate of the given type that calls LogEvent().</returns>
        private Delegate CreateDelegateForEvent(Type delegateType)
        {
            // Retrieve the method signature of the delegate.
            MethodInfo invokeMethod = delegateType.GetMethod("Invoke");
            if (invokeMethod == null) return null;

            ParameterInfo[] parameters = invokeMethod.GetParameters();

            // Create parameter expressions matching the delegate parameters.
            ParameterExpression[] paramExpressions =
                parameters.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();

            MethodInfo logEventMethod = typeof(MetricRecorder)
                .GetMethod(nameof(LogEvent), BindingFlags.NonPublic | BindingFlags.Instance);

            // Expression to call LogEvent().
            if (logEventMethod == null) return null;
            Expression callLogEvent = Expression.Call(Expression.Constant(this), logEventMethod);

            var lambda = Expression.Lambda(delegateType, callLogEvent, paramExpressions);
            return lambda.Compile();
        }

        private void OnUnityEventTriggered()
        {
            LogEvent();
        }

        private void LogEvent()
        {
            var instance = new SerializableDictionary();
            instance.Add("Time: ", _currentTime);

            foreach (var property in _propertiesToTrack)
            {
                object value = GetPropertyValue(property);
                instance.Add(property.propertyName, value);
            }

            _track.instances.Add(instance);
        }

        private object GetPropertyValue(PropertyToTrack property)
        {
            if (property.targetObject == null || string.IsNullOrEmpty(property.propertyName))
            {
                return property.value;
            }

            var type = property.targetObject.GetType();
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var fieldInfo = type.GetField(property.propertyName, flags);
            if (fieldInfo != null)
            {
                return fieldInfo.GetValue(property.targetObject);
            }

            // Attempt to get the value from a property
            var propInfo = type.GetProperty(property.propertyName, flags);
            if (propInfo != null)
            {
                return propInfo.GetValue(property.targetObject);
            }

            Debug.LogWarning($"{property.propertyName} was not found in {property.targetObject}.");
            return property.value;
        }


        private void OnApplicationQuit()
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

        /// <summary>
        /// Unsubscribes from whichever event/delegate/UnityEvent subscribed to.
        /// </summary>
        private void UnsubscribeFromEvent()
        {
            // .NET event unsubscribe
            if (_dotNetEvent != null && _eventDelegate != null)
            {
                bool isStatic = _dotNetEvent.GetRemoveMethod().IsStatic;
                object target = isStatic ? null : _eventSource;
                _dotNetEvent.RemoveEventHandler(target, _eventDelegate);
            }

            // UnityEvent unsubscribe
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
                    }
                }
            }

            // Delegate field (System.Action) unsubscribe
            if (_systemActionField != null && _systemActionDelegate != null)
            {
                Delegate existing = _systemActionField.GetValue(_eventSource) as Delegate;
                Delegate removed = Delegate.Remove(existing, _systemActionDelegate);
                _systemActionField.SetValue(_eventSource, removed);
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MetricRecorder))]
    public class MetricRecorderEditor : Editor
    {
        private bool _showTrackedProperties = true;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawMetricSettings();
            EditorGUILayout.Space(10);
            DrawTrackedProperties();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawMetricSettings()
        {
            EditorGUILayout.LabelField("Metric Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("helpbox");
            {
                // Draw basic metric fields
                DrawPropertyField("metricName", "Metric Name");
                DrawPropertyField("metricColor", "Metric Color");

                EditorGUILayout.Space(5);

                // Draw event source and event name dropdown
                SerializedProperty eventSourceProp = serializedObject.FindProperty("_eventSource");
                EditorGUILayout.PropertyField(eventSourceProp, new GUIContent("Event Source"));

                SerializedProperty eventNameProp = serializedObject.FindProperty("_eventName");
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
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawTrackedProperties()
        {
            _showTrackedProperties = EditorGUILayout.Foldout(_showTrackedProperties, "Tracked Properties", true,
                EditorStyles.foldoutHeader);
            if (!_showTrackedProperties)
            {
                return;
            }

            EditorGUILayout.BeginVertical("helpbox");
            {
                SerializedProperty propertiesList = serializedObject.FindProperty("_propertiesToTrack");

                for (int i = propertiesList.arraySize - 1; i >= 0; i--)
                {
                    SerializedProperty propertyElement = propertiesList.GetArrayElementAtIndex(i);
                    if (propertyElement == null)
                    {
                        Debug.LogError($"Property element at index {i} is null!");
                        continue;
                    }

                    DrawTrackedPropertyElement(propertyElement, propertiesList, i);
                }

                EditorGUILayout.Space();
                DrawAddPropertyButton(propertiesList);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawTrackedPropertyElement(SerializedProperty propertyElement, SerializedProperty propertiesList,
            int index)
        {
            EditorGUILayout.BeginVertical("box");
            {
                SerializedProperty targetObjectProp = propertyElement.FindPropertyRelative("targetObject");
                EditorGUILayout.PropertyField(targetObjectProp, new GUIContent("Target Object"));

                if (targetObjectProp.objectReferenceValue != null)
                {
                    SerializedProperty propertyNameProp = propertyElement.FindPropertyRelative("propertyName");
                    MonoBehaviour referenceValue = targetObjectProp.objectReferenceValue as MonoBehaviour;
                    if (referenceValue != null)
                    {
                        List<string> availableProps = GetAvailableProperties(referenceValue) ?? new List<string>();
                        int selectedIndex = Mathf.Max(0, availableProps.IndexOf(propertyNameProp.stringValue));
                        selectedIndex = EditorGUILayout.Popup("Property", selectedIndex, availableProps.ToArray());
                        if (selectedIndex >= 0 && selectedIndex < availableProps.Count)
                        {
                            propertyNameProp.stringValue = availableProps[selectedIndex];
                        }
                    }
                }

                // Button to remove the property element.
                if (GUILayout.Button("Remove", GUILayout.MaxWidth(75)))
                {
                    propertiesList.DeleteArrayElementAtIndex(index);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawAddPropertyButton(SerializedProperty propertiesList)
        {
            GUIStyle addButtonStyle = new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold };
            if (GUILayout.Button("Add Property", addButtonStyle))
            {
                propertiesList.InsertArrayElementAtIndex(propertiesList.arraySize);
            }
        }

        private void DrawPropertyField(string propertyName, string label)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            EditorGUILayout.PropertyField(property, new GUIContent(label));
        }

        /// <summary>
        /// Gets the list of public .NET events, public UnityEvent fields, and delegate fields (System.Action, etc.)
        /// from the target MonoBehaviour. This is what populates the "Event" dropdown.
        /// </summary>
        private List<string> GetAvailableEvents(MonoBehaviour target)
        {
            List<string> eventNames = new List<string>();
            if (target == null) return eventNames;

            Type type = target.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

            // 1) .NET events
            var dotNetEvents = type.GetEvents(flags).Select(e => e.Name);
            eventNames.AddRange(dotNetEvents);

            // 2) UnityEvent fields
            var unityEventFields = type.GetFields(flags)
                .Where(field => typeof(UnityEventBase).IsAssignableFrom(field.FieldType))
                .Select(field => field.Name);
            eventNames.AddRange(unityEventFields);

            // 3) Delegate fields (e.g. System.Action, or any other delegate).
            var delegateFields = type.GetFields(flags)
                .Where(field =>
                    typeof(Delegate).IsAssignableFrom(field.FieldType)
                    && field.FieldType.BaseType == typeof(MulticastDelegate))
                .Select(field => field.Name);
            eventNames.AddRange(delegateFields);

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
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

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
#endif
}