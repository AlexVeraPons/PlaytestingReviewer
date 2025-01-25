using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlaytestingReviewer.Editor
{
    public abstract class Track
    {
        protected Action OnResize;

        protected VisualElement _infoRoot;
        protected VisualElement _descriptionRoot;

        protected VisualElement _descriptionContainer;
        protected VisualElement _informationContainer;

        protected ITimePositionTranslator _timeRelations;

        protected float _trackHeight = 40;

        private float _resizeDebounceTimer;
        protected float _resizeDebounceDuration = 1f;
        private bool _shouldStartResizeTimer = false;
        protected VisualElement _elementToAdaptToWidth;


        /// <summary>
        /// Constructor for the track class
        /// </summary>
        /// <param name="description">Where you want the label and what the track does</param>
        /// <param name="information">Where you want the information to be visualized</param>
        public Track(VisualElement description, VisualElement information, ITimePositionTranslator timeRelations)
        {
            _timeRelations = timeRelations;
            PreInitialization();

            _descriptionRoot = description;
            _infoRoot = information;

            InitializeDescription(description);
            InitializeInformation(information);

            EditorApplication.update += TrackUpdate;
        }

        protected virtual void TrackUpdate()
        {
            if (_shouldStartResizeTimer == true)
            {
                _resizeDebounceTimer -= Time.deltaTime;
                if (_resizeDebounceTimer <= 0)
                {
                    _resizeDebounceTimer = 0;
                    _shouldStartResizeTimer = false;
                    OnResize?.Invoke();
                    Debug.Log("Action called");
                }
            }
        }

        protected abstract void PreInitialization();

        protected virtual void InitializeInformation(VisualElement information)
        {
            _informationContainer = new VisualElement();

            // Background color, border, and rounding
            _informationContainer.style.backgroundColor = new StyleColor(new Color(0.25f, 0.25f, 0.25f));
            _informationContainer.style.borderTopWidth = 1;
            _informationContainer.style.borderBottomWidth = 1;
            _informationContainer.style.borderLeftWidth = 1;
            _informationContainer.style.borderRightWidth = 1;
            _informationContainer.style.borderTopColor = new StyleColor(Color.black);
            _informationContainer.style.borderBottomColor = new StyleColor(Color.black);
            _informationContainer.style.borderLeftColor = new StyleColor(Color.black);
            _informationContainer.style.borderRightColor = new StyleColor(Color.black);
            _informationContainer.style.borderTopLeftRadius = 3;
            _informationContainer.style.borderTopRightRadius = 6;
            _informationContainer.style.borderBottomLeftRadius = 3;
            _informationContainer.style.borderBottomRightRadius = 6;

            _informationContainer.style.flexDirection = FlexDirection.Row;
            _informationContainer.style.alignItems = Align.Center;
            _informationContainer.style.height = _trackHeight;
            _informationContainer.style.marginBottom = 5;
            _informationContainer.style.marginTop = 5;
            _informationContainer.style.paddingLeft = 10;
            _informationContainer.style.paddingRight = 10;

            information.Add(_informationContainer);
        }

        protected virtual void InitializeDescription(VisualElement description)
        {
            _descriptionContainer = new VisualElement
            {
                style =
                {
                    maxWidth = 200,
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    height = _trackHeight,
                    marginBottom = 5,
                    marginTop = 5,
                    backgroundColor = new StyleColor(new Color(0.35f, 0.35f, 0.35f)),

                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopColor = new StyleColor(Color.black),
                    borderBottomColor = new StyleColor(Color.black),
                    borderLeftColor = new StyleColor(Color.black),
                    borderRightColor = new StyleColor(Color.black),
                    borderTopLeftRadius = 6,
                    borderTopRightRadius = 6,
                    borderBottomLeftRadius = 6,
                    borderBottomRightRadius = 6,
                    
                    // Some horizontal padding
                    paddingLeft = 5,
                    paddingRight = 5
                }
            };

            var spacerElement = new VisualElement
            {
                style =
                {
                    width = 10,
                    height = 20
                }
            };
            _descriptionContainer.Add(spacerElement);

            var squareElement = new VisualElement
            {
                style =
                {
                    width = 20,
                    height = 20,
                    backgroundColor = new StyleColor(Color.black),
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopColor = new StyleColor(Color.white),
                    borderBottomColor = new StyleColor(Color.white),
                    borderLeftColor = new StyleColor(Color.white),
                    borderRightColor = new StyleColor(Color.white),
                    borderTopLeftRadius = 2,
                    borderTopRightRadius = 2,
                    borderBottomLeftRadius = 2,
                    borderBottomRightRadius = 2,
                    marginLeft = 5,
                    marginRight = 5
                }
            };
            _descriptionContainer.Add(squareElement);

            // Create a label container with a bit of internal padding
            var labelContainer = new VisualElement
            {
                style =
                {
                    backgroundColor = new StyleColor(Color.grey),
                    paddingTop = 5,
                    paddingBottom = 5,
                    paddingLeft = 10,
                    paddingRight = 10,
                    
                    // Adding a slight rounding to match outer container
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4
                }
            };

            var label = new Label("Description")
            {
                // Center the label text vertically
                style =
                {
                    unityTextAlign = TextAnchor.MiddleCenter,
                    fontSize = 14
                }
            };

            labelContainer.Add(label);
            _descriptionContainer.Add(labelContainer);

            // Add a button for the context menu
            var button = new Button() { text = ":" };
            button.clicked += OnButtonClicked;
            button.style.marginLeft = 10;
            _descriptionContainer.Add(button);

            // Add our container to the parent
            description.Add(_descriptionContainer);
        }

        protected virtual void OnButtonClicked()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Delete Track"), false, DeleteTrack);
            menu.AddItem(new GUIContent("Toggle Visibility"), false, ToggleVisibility);
            AddMenuItems(menu);
            menu.ShowAsContext();
        }

        protected virtual void AddMenuItems(GenericMenu menu) { } // Default implementation

        protected void ToggleVisibility()
        {
            if (_informationContainer == null)
            {
                InitializeInformation(_infoRoot);
            }
            else
            {
                _informationContainer.RemoveFromHierarchy();
                _informationContainer.Clear();
                _informationContainer = null;
            }
        }

        protected void DeleteTrack()
        {
            _descriptionContainer?.RemoveFromHierarchy();
            _informationContainer?.RemoveFromHierarchy();

            _descriptionContainer = null;
            _informationContainer = null;
        }

        public void AdaptToWidth(VisualElement element)
        {
            _elementToAdaptToWidth = element;
            element.RegisterCallback<GeometryChangedEvent>(evt => ElementResized());
        }

        private void ElementResized()
        {
            Debug.Log("Element resized");
            _shouldStartResizeTimer = true;
            _resizeDebounceTimer = _resizeDebounceDuration;

            _informationContainer.style.width = _elementToAdaptToWidth.resolvedStyle.width;
        }
    }
}
