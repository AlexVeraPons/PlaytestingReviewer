using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlaytestingReviewer.Editor
{
    public abstract class Track
    {
        private Image _trackIcon;

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

        private VisualElement _imageContainer;

        protected string _title = "Description";

        /// <summary>
        /// Constructor for the track class
        /// </summary>
        /// <param name="description">Where you want the label and what the track does</param>
        /// <param name="information">Where you want the information to be visualized</param>
        public Track(VisualElement description, VisualElement information, ITimePositionTranslator timeRelations)
        {
            Initialization(description, information, timeRelations);
        }

        protected virtual void Initialization(VisualElement description, VisualElement information, ITimePositionTranslator timeRelations)
        {
            _timeRelations = timeRelations;

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

        protected virtual void InitializeInformation(VisualElement information)
        {
            _informationContainer = new VisualElement
            {
                style =
                {
                    // Background color
                    backgroundColor = new StyleColor(new Color(0.20f, 0.20f, 0.20f)),

                    // Borders
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopColor = new StyleColor(Color.black),
                    borderBottomColor = new StyleColor(Color.black),
                    borderLeftColor = new StyleColor(Color.black),
                    borderRightColor = new StyleColor(Color.black),

                    // Rounding
                    borderTopLeftRadius = 6,
                    borderTopRightRadius = 6,
                    borderBottomLeftRadius = 6,
                    borderBottomRightRadius = 6,

                    // Layout
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    height = _trackHeight,
                    marginBottom = 5,
                    marginTop = 5,
                    paddingLeft = 10,
                    paddingRight = 10
                }
            };

            information.Add(_informationContainer);
        }

        protected virtual void InitializeDescription(VisualElement description)
        {
            // Main container for the description
            _descriptionContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    height = _trackHeight,
                    marginBottom = 5,
                    marginTop = 5,

                    // This background will be mostly overshadowed by child elements, 
                    // but let's keep it slightly darker to match the overall style
                    backgroundColor = new StyleColor(new Color(0.35f, 0.35f, 0.35f)),

                    // Borders (unified for a consistent look)
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
                    paddingRight = 5,
                }
            };

            // Pastel bar on the left side
            var pastelBar = new VisualElement
            {
                style =
                {
                    width = 8,
                    height = new Length(100, LengthUnit.Percent),
                    backgroundColor = new StyleColor(new Color(0.90f, 0.90f, 1f)),
                    marginLeft = -5f,
                    borderBottomLeftRadius = 6,
                    borderTopLeftRadius = 6
                }
            };
            _descriptionContainer.Add(pastelBar);

            // We can keep a small spacer if we like
            var spacerElement = new VisualElement
            {
                style =
                {
                    width = 5,
                    height = 20
                }
            };
            _descriptionContainer.Add(spacerElement);

            // Square or “icon” element
            _imageContainer = new VisualElement
            {
                style =
                {
                    width = 20,
                    height = 20,
                    marginLeft = 10,
                    marginRight = 10
                }
            };
            _descriptionContainer.Add(_imageContainer);

            // Label container
            var labelContainer = new VisualElement
            {
                style =
                {
                    backgroundColor = new StyleColor(Color.grey),
                    paddingTop = 5,
                    paddingBottom = 5,
                    paddingLeft = 10,
                    paddingRight = 10,

                    // Smoother rounding
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4,

                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopColor = new StyleColor(Color.black),
                    borderBottomColor = new StyleColor(Color.black),
                    borderLeftColor = new StyleColor(Color.black),
                    borderRightColor = new StyleColor(Color.black)
                }
            };

            var label = new Label(_title)
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleCenter,
                    fontSize = 14
                }
            };

            labelContainer.Add(label);
            _descriptionContainer.Add(labelContainer);

            // Button for context menu
            var button = new Button() { text = ":" };
            button.clicked += OnButtonClicked;
            button.style.marginLeft = 10;
            _descriptionContainer.Add(button);

            // Finally, add the container to the parent
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

            if (_informationContainer != null && _elementToAdaptToWidth != null)
            {
                _informationContainer.style.width = _elementToAdaptToWidth.resolvedStyle.width;
            }
        }

        public void SetTrackIcon(Texture2D image)
        {
            _imageContainer.style.backgroundImage = image;
        }
    }
}
