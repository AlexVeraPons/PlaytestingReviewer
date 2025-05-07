using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using PlaytestingReviewer.Video;
using UnityEditor;

namespace PlaytestingReviewer.Tracks
{
    public class MetricRecorder : MonoBehaviour
    {
        [Header("Metric Settings")] public string metricName = "Metric";
        public Color metricColor = Color.red;

        [Header("Events to Track (multiple)")] [SerializeField]
        private List<EventToTrack> _eventsToTrack = new List<EventToTrack>();

        [Header("Properties to Track")] [SerializeField]
        private List<PropertyToTrack> _propertiesToTrack = new List<PropertyToTrack>();

        private Track _track;
        private float _selfCurrentTime = 0f;
        private float _currentTime => _videoCapture == null ? _selfCurrentTime : _videoCapture.CurrentVideoTime;
        private VideoCapture _videoCapture;

        private void Awake()
        {
            _videoCapture = FindAnyObjectByType<VideoCapture>(FindObjectsInactive.Include);
        }

        private void Start()
        {
            InitializeTrack();
            SubscribeToEvents();
        }

        private void Update()
        {
            // If there's no VideoCapture, we simulate "time" ourselves.
            if (_videoCapture == null)
            {
                _selfCurrentTime += Time.deltaTime;
            }
        }

        private void OnApplicationQuit()
        {
            UnsubscribeFromEvents();
            AddTrackToCollector();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
            AddTrackToCollector();
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

        private void SubscribeToEvents()
        {
            foreach (var evt in _eventsToTrack)
            {
                if (evt.eventSource == null || string.IsNullOrEmpty(evt.eventName))
                    continue;

                var sourceType = evt.eventSource.GetType();
                BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

                // 1) .NET event (C# event)
                evt.dotNetEvent = sourceType.GetEvent(evt.eventName, flags);
                if (evt.dotNetEvent != null)
                {
                    Type eventHandlerType = evt.dotNetEvent.EventHandlerType;
                    evt.eventDelegate = CreateDelegateForEvent(eventHandlerType);

                    bool isStatic = evt.dotNetEvent.GetAddMethod().IsStatic;
                    object target = isStatic ? null : evt.eventSource;
                    evt.dotNetEvent.AddEventHandler(target, evt.eventDelegate);
                    continue;
                }

                // 2) UnityEvent field
                evt.unityEventField = sourceType.GetField(evt.eventName, flags);
                if (evt.unityEventField != null &&
                    typeof(UnityEventBase).IsAssignableFrom(evt.unityEventField.FieldType))
                {
                    var unityEvent = evt.unityEventField.GetValue(evt.eventSource) as UnityEventBase;
                    if (unityEvent != null)
                    {
                        // We add a listener: use reflection to call AddListener(UnityAction)
                        MethodInfo addListenerMethod = unityEvent.GetType()
                            .GetMethod("AddListener", new Type[] { typeof(UnityAction) });
                        if (addListenerMethod != null)
                        {
                            UnityAction action = OnUnityEventFired;
                            addListenerMethod.Invoke(unityEvent, new object[] { action });
                        }
                    }

                    continue;
                }

                // 3) Delegate field (e.g. System.Action)
                evt.systemActionField = sourceType.GetField(evt.eventName, flags);
                if (evt.systemActionField != null &&
                    typeof(Delegate).IsAssignableFrom(evt.systemActionField.FieldType))
                {
                    evt.systemActionDelegate = CreateDelegateForEvent(evt.systemActionField.FieldType);
                    Delegate existing = evt.systemActionField.GetValue(evt.eventSource) as Delegate;
                    Delegate combined = Delegate.Combine(existing, evt.systemActionDelegate);
                    evt.systemActionField.SetValue(evt.eventSource, combined);
                }
            }
        }

        private void UnsubscribeFromEvents()
        {
            foreach (var evt in _eventsToTrack)
            {
                // 1) .NET event
                if (evt.dotNetEvent != null && evt.eventDelegate != null)
                {
                    bool isStatic = evt.dotNetEvent.GetRemoveMethod().IsStatic;
                    object target = isStatic ? null : evt.eventSource;
                    evt.dotNetEvent.RemoveEventHandler(target, evt.eventDelegate);
                }

                // 2) UnityEvent
                if (evt.unityEventField != null)
                {
                    var unityEvent = evt.unityEventField.GetValue(evt.eventSource) as UnityEventBase;
                    if (unityEvent != null)
                    {
                        // remove the same action: OnUnityEventFired
                        MethodInfo removeListenerMethod = unityEvent.GetType()
                            .GetMethod("RemoveListener", new Type[] { typeof(UnityAction) });
                        if (removeListenerMethod != null)
                        {
                            UnityAction action = OnUnityEventFired;
                            removeListenerMethod.Invoke(unityEvent, new object[] { action });
                        }
                    }
                }

                // 3) Delegate field
                if (evt.systemActionField != null && evt.systemActionDelegate != null)
                {
                    Delegate existing = evt.systemActionField.GetValue(evt.eventSource) as Delegate;
                    Delegate removed = Delegate.Remove(existing, evt.systemActionDelegate);
                    evt.systemActionField.SetValue(evt.eventSource, removed);
                }
            }
        }

        /// <summary>
        /// Creates a delegate that ignores parameters and calls LogEvent().
        /// </summary>
        private Delegate CreateDelegateForEvent(Type delegateType)
        {
            MethodInfo invokeMethod = delegateType.GetMethod("Invoke");
            ParameterInfo[] parameters = invokeMethod.GetParameters();

            // create param expressions
            var paramExpressions = parameters
                .Select(p => Expression.Parameter(p.ParameterType, p.Name))
                .ToArray();

            MethodInfo logEventMethod = typeof(MetricRecorder)
                .GetMethod(nameof(LogEvent), BindingFlags.NonPublic | BindingFlags.Instance);

            Expression callLogEvent = Expression.Call(
                Expression.Constant(this),
                logEventMethod
            );

            var lambda = Expression.Lambda(delegateType, callLogEvent, paramExpressions);
            return lambda.Compile();
        }

        /// <summary>
        /// Called when any UnityEvent from any tracked source is fired.
        /// </summary>
        private void OnUnityEventFired()
        {
            LogEvent();
        }

        /// <summary>
        /// The core logging function: grabs property data and appends a record to the track.
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
                    var targetType = property.targetObject.GetType();

                    // Try field first
                    var fieldInfo = targetType.GetField(
                        property.propertyName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                    if (fieldInfo != null)
                    {
                        value = fieldInfo.GetValue(property.targetObject);
                    }
                    else
                    {
                        // Then try property
                        var propInfo = targetType.GetProperty(
                            property.propertyName,
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                        );
                        if (propInfo != null)
                        {
                            value = propInfo.GetValue(property.targetObject);
                        }
                        else
                        {
                            // fallback to custom Value
                            value = property.Value;
                            Debug.LogWarning($"'{property.propertyName}' not found on '{property.targetObject}'.");
                        }
                    }
                }
                else
                {
                    // fallback if no object or no property specified
                    value = property.Value;
                }

                instance.Add(property.propertyName, value);
            }

            _track.instances.Add(instance);
        }

        private void AddTrackToCollector()
        {
            TrackCollector.Instance.AddTrack(_track);
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(MetricRecorder))]
    public class MetricRecorderEditor : Editor
    {
        private bool showEvents = true;
        private bool showTrackedProperties = true;

        public override void OnInspectorGUI()
        {
            // Reference to the actual script
            var recorder = (MetricRecorder)target;
            serializedObject.Update();

            // -- METRIC SETTINGS --
            EditorGUILayout.LabelField("Metric Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("helpbox");
            {
                SerializedProperty metricNameProp = serializedObject.FindProperty("metricName");
                EditorGUILayout.PropertyField(metricNameProp, new GUIContent("Metric Name"));

                SerializedProperty metricColorProp = serializedObject.FindProperty("metricColor");
                EditorGUILayout.PropertyField(metricColorProp, new GUIContent("Metric Color"));
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // -- EVENTS TO TRACK --
            showEvents = EditorGUILayout.Foldout(showEvents, "Events to Track", true);
            if (showEvents)
            {
                EditorGUILayout.BeginVertical("helpbox");
                {
                    SerializedProperty eventsList = serializedObject.FindProperty("_eventsToTrack");
                    for (int i = 0; i < eventsList.arraySize; i++)
                    {
                        SerializedProperty evtElement = eventsList.GetArrayElementAtIndex(i);
                        EditorGUILayout.BeginVertical("box");
                        {
                            SerializedProperty eventSourceProp = evtElement.FindPropertyRelative("eventSource");
                            EditorGUILayout.PropertyField(eventSourceProp, new GUIContent("Event Source"));

                            SerializedProperty eventNameProp = evtElement.FindPropertyRelative("eventName");
                            if (eventSourceProp.objectReferenceValue != null)
                            {
                                MonoBehaviour srcMono = eventSourceProp.objectReferenceValue as MonoBehaviour;
                                if (srcMono)
                                {
                                    List<string> availableEvents = GetAvailableEvents(srcMono);
                                    int index = Math.Max(0, availableEvents.IndexOf(eventNameProp.stringValue));
                                    index = EditorGUILayout.Popup("Event Name", index, availableEvents.ToArray());
                                    if (index >= 0 && index < availableEvents.Count)
                                    {
                                        eventNameProp.stringValue = availableEvents[index];
                                    }
                                }
                            }

                            // Remove button
                            if (GUILayout.Button("Remove Event", GUILayout.Width(120)))
                            {
                                eventsList.DeleteArrayElementAtIndex(i);
                                // Important: break or continue after removing to avoid out-of-range
                                break;
                            }
                        }
                        EditorGUILayout.EndVertical();
                    }

                    if (GUILayout.Button("Add New Event"))
                    {
                        eventsList.InsertArrayElementAtIndex(eventsList.arraySize);
                    }
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(10);

            // -- TRACKED PROPERTIES --
            showTrackedProperties = EditorGUILayout.Foldout(showTrackedProperties, "Tracked Properties", true);
            if (showTrackedProperties)
            {
                EditorGUILayout.BeginVertical("helpbox");
                {
                    SerializedProperty propertiesList = serializedObject.FindProperty("_propertiesToTrack");
                    for (int i = 0; i < propertiesList.arraySize; i++)
                    {
                        SerializedProperty propertyElement = propertiesList.GetArrayElementAtIndex(i);
                        EditorGUILayout.BeginVertical("box");
                        {
                            SerializedProperty targetObjectProp = propertyElement.FindPropertyRelative("targetObject");
                            EditorGUILayout.PropertyField(targetObjectProp, new GUIContent("Target Object"));

                            if (targetObjectProp.objectReferenceValue != null)
                            {
                                SerializedProperty propertyNameProp = propertyElement.FindPropertyRelative("propertyName");
                                MonoBehaviour targetMono = targetObjectProp.objectReferenceValue as MonoBehaviour;
                                if (targetMono != null)
                                {
                                    List<string> availableProps = GetAvailableProperties(targetMono);
                                    int propIndex = Math.Max(0, availableProps.IndexOf(propertyNameProp.stringValue));
                                    propIndex = EditorGUILayout.Popup("Property", propIndex, availableProps.ToArray());
                                    if (propIndex >= 0 && propIndex < availableProps.Count)
                                    {
                                        propertyNameProp.stringValue = availableProps[propIndex];
                                    }
                                }
                            }

                            // Remove button
                            if (GUILayout.Button("Remove Property", GUILayout.Width(120)))
                            {
                                propertiesList.DeleteArrayElementAtIndex(i);
                                break;
                            }
                        }
                        EditorGUILayout.EndVertical();
                    }

                    if (GUILayout.Button("Add Property"))
                    {
                        propertiesList.InsertArrayElementAtIndex(propertiesList.arraySize);
                    }
                }
                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }

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
                .Where(f => typeof(UnityEventBase).IsAssignableFrom(f.FieldType))
                .Select(f => f.Name);
            eventNames.AddRange(unityEventFields);

            // 3) Delegate fields (System.Action, etc.)
            var delegateFields = type.GetFields(flags)
                .Where(f => typeof(Delegate).IsAssignableFrom(f.FieldType)
                            && f.FieldType.BaseType == typeof(MulticastDelegate))
                .Select(f => f.Name);
            eventNames.AddRange(delegateFields);

            return eventNames;
        }

        private List<string> GetAvailableProperties(MonoBehaviour target)
        {
            List<string> propNames = new List<string>();
            if (target == null) return propNames;

            Type type = target.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

            // Public or non-public properties with a getter
            propNames.AddRange(
                type.GetProperties(flags).Where(p => p.CanRead).Select(p => p.Name)
            );

            // Fields
            propNames.AddRange(type.GetFields(flags).Select(f => f.Name));

            return propNames;
        }
    }
#endif
}


