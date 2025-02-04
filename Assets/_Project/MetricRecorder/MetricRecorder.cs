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
    public class MetricRecorder : MonoBehaviour
    {
        public string metricName = "Metric";

        [SerializeField] private MonoBehaviour _eventSource; // The MonoBehaviour holding the event
        [SerializeField] private string _eventName; // The name of the event/delegate/UnityEvent to track
        [SerializeField] private List<PropertyToTrack> _propertiesToTrack; // Properties to track

        private Track _track;

        // For standard .NET events:
        private EventInfo _dotNetEvent;
        private Delegate _eventDelegate; // Stored delegate for unsubscribing

        // For UnityEvents:
        private FieldInfo _unityEventField;

        // For delegate field support (System.Action, etc.):
        private FieldInfo _systemActionField;
        private Delegate _systemActionDelegate; // The compiled delegate attached to the field

        private float _selfCurrentTime = 0f;
        private float _currentTime => _videoCapture == null ? _selfCurrentTime : _videoCapture.currentVideoTime;

        private VideoCapture _videoCapture;

        private void Start()
        {
            SubscribeToEvent();
            InitializeTrack();
            _videoCapture = FindAnyObjectByType<VideoCapture>();
        }

        private void Update()
        {
            // If there's no video capture in the scene, increment local time.
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

            // Include both instance and static members.
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

            // 1) Try to find a C# event first.
            _dotNetEvent = _eventSource.GetType().GetEvent(_eventName, flags);
            if (_dotNetEvent != null)
            {
                Type eventHandlerType = _dotNetEvent.EventHandlerType;
                // Create a delegate that calls LogEvent() (ignores parameters).
                _eventDelegate = CreateDelegateForEvent(eventHandlerType);

                // Check if the event is static. For static events, pass null as the target.
                bool isStatic = _dotNetEvent.GetAddMethod().IsStatic;
                object target = isStatic ? null : _eventSource;
                _dotNetEvent.AddEventHandler(target, _eventDelegate);
                return;
            }

            // 2) If not a C# event, check for a UnityEvent field.
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

            // 3) Finally, if not a standard .NET event or UnityEvent, see if it's a delegate field (e.g. System.Action).
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
            ParameterInfo[] parameters = invokeMethod.GetParameters();

            // Create parameter expressions matching the delegate's parameters.
            ParameterExpression[] paramExpressions =
                parameters.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();

            // Get the LogEvent() method (private instance method).
            MethodInfo logEventMethod = typeof(MetricRecorder)
                .GetMethod(nameof(LogEvent), BindingFlags.NonPublic | BindingFlags.Instance);

            // Expression to call LogEvent().
            Expression callLogEvent = Expression.Call(Expression.Constant(this), logEventMethod);

            // Build a lambda expression with the appropriate signature that calls LogEvent.
            var lambda = Expression.Lambda(delegateType, callLogEvent, paramExpressions);
            return lambda.Compile();
        }

        /// <summary>
        /// Callback for UnityEvent-based events.
        /// </summary>
        private void OnUnityEventTriggered()
        {
            LogEvent();
        }

        /// <summary>
        /// Logs the event occurrence.
        /// </summary>
        private void LogEvent()
        {
            var instance = new SerializableDictionary();
            instance.Add("time", _currentTime);

            foreach (var property in _propertiesToTrack)
            {
                object value = null;

                if (property.targetObject != null && !string.IsNullOrEmpty(property.propertyName))
                {
                    var type = property.targetObject.GetType();

                    // Try getting as a field first (public or non-public)
                    var fieldInfo = type.GetField(
                        property.propertyName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );

                    if (fieldInfo != null)
                    {
                        value = fieldInfo.GetValue(property.targetObject);
                    }
                    else
                    {
                        // Then try as a property (public or non-public)
                        var propInfo = type.GetProperty(
                            property.propertyName,
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                        );

                        if (propInfo != null)
                        {
                            value = propInfo.GetValue(property.targetObject);
                        }
                        else
                        {
                            value = property.value;
                            Debug.LogWarning(property.propertyName + " was not found in " + property.targetObject +
                                             ".");
                        }
                    }
                }
                else
                {
                    value = property.value;
                }

                instance.Add(property.propertyName, value);
            }

            _track.instances.Add(instance);
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
        /// Unsubscribes from whichever event/delegate/UnityEvent we subscribed to.
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

            // (2) Delegate field (System.Action) unsubscribe
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
        // For an optional foldout to hide/show Tracked Properties
        private bool showTrackedProperties = true;

        public override void OnInspectorGUI()
        {
            // Reference to the actual script
            MetricRecorder recorder = (MetricRecorder)target;

            // Synchronize serialized object with actual fields
            serializedObject.Update();

            // -- METRIC SETTINGS --
            EditorGUILayout.LabelField("Metric Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("helpbox");
            {
                // Metric Name
                SerializedProperty metricNameProp = serializedObject.FindProperty("metricName");
                EditorGUILayout.PropertyField(metricNameProp, new GUIContent("Metric Name"));

                EditorGUILayout.Space(5);

                // Event Source + Event Name
                SerializedProperty eventSourceProp = serializedObject.FindProperty("_eventSource");
                EditorGUILayout.PropertyField(eventSourceProp, new GUIContent("Event Source"));

                // Dropdown for the event name
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

            EditorGUILayout.Space(10);

            // -- TRACKED PROPERTIES --
            showTrackedProperties = EditorGUILayout.Foldout(showTrackedProperties, "Tracked Properties", true,
                EditorStyles.foldoutHeader);
            if (showTrackedProperties)
            {
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

                        EditorGUILayout.BeginVertical("box");
                        {
                            SerializedProperty targetObjectProp =
                                propertyElement.FindPropertyRelative("targetObject");
                            EditorGUILayout.PropertyField(targetObjectProp, new GUIContent("Target Object"));

                            if (targetObjectProp.objectReferenceValue != null)
                            {
                                SerializedProperty propertyNameProp =
                                    propertyElement.FindPropertyRelative("propertyName");
                                MonoBehaviour targetMono = targetObjectProp.objectReferenceValue as MonoBehaviour;
                                if (targetMono != null)
                                {
                                    List<string> availableProps =
                                        GetAvailableProperties(targetMono) ?? new List<string>();
                                    int propIndex = Mathf.Max(0,
                                        availableProps.IndexOf(propertyNameProp.stringValue));
                                    propIndex = EditorGUILayout.Popup("Property", propIndex,
                                        availableProps.ToArray());
                                    if (propIndex >= 0 && propIndex < availableProps.Count)
                                    {
                                        propertyNameProp.stringValue = availableProps[propIndex];
                                    }
                                }
                            }

                            // Remove button
                            if (GUILayout.Button("Remove", GUILayout.MaxWidth(75)))
                            {
                                propertiesList.DeleteArrayElementAtIndex(i);
                            }
                        }
                        EditorGUILayout.EndVertical();
                    }

                    // Add property button
                    EditorGUILayout.Space();
                    GUIStyle addButtonStyle = new GUIStyle(GUI.skin.button);
                    addButtonStyle.fontStyle = FontStyle.Bold;
                    if (GUILayout.Button("Add Property", addButtonStyle))
                    {
                        propertiesList.InsertArrayElementAtIndex(propertiesList.arraySize);
                    }
                }
                EditorGUILayout.EndVertical();
            }

            // Apply modifications
            serializedObject.ApplyModifiedProperties();
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