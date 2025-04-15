using System;
using System.Diagnostics;
using UnityEngine.UIElements;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PlaytestingReviewer.Editors
{
    /// <summary>
    /// Handles zoom interactions using the mouse wheel while holding Ctrl.
    /// </summary>
    public class ZoomUpdater
    {
        // Action
        public Action<float> OnZoomed;
        
        // class values
        private readonly float _initialZoomAmount = 0f;
        private float _currentZoomAmount = 0f;
        private float _maxZoomAmount = 70f;
        private float _minZoomAmount = -10f;
        private float _zoomModifier = 1f;

        public ZoomUpdater()
        {
            _currentZoomAmount = _initialZoomAmount;
        }

        public void SubscribeToZoom(VisualElement element)
        {
            element.RegisterCallback<WheelEvent>(OnMouseZoom);
        }

        private void OnMouseZoom(WheelEvent evt)
        {
            if (evt.ctrlKey == false) // only zoom when holding ctrl
            {
                return;
            }

            _currentZoomAmount += evt.delta.y * _zoomModifier;

            if (_currentZoomAmount > _maxZoomAmount)
            {
                _currentZoomAmount = _maxZoomAmount;
            }
            else if (_currentZoomAmount < _minZoomAmount)
            {
                _currentZoomAmount = _minZoomAmount;
            }
            else
            {
                OnZoomed?.Invoke(evt.delta.y);
            }
        }
        
        public void SetZoomLimits(float minZoomAmount, float maxZoomAmount)
        {
            _minZoomAmount = minZoomAmount;
            _maxZoomAmount = maxZoomAmount;
        }

        public void SetZoomModifier(float modifier)
        {
            _zoomModifier = modifier;
        }
    }
}