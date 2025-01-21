using System;
using System.Diagnostics;
using UnityEngine.UIElements;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PlaytestingReviewer.Editor
{
    public class ZoomUpdater
    {
        public Action<float> OnZoomed;
        private float _initialZoomAmmount = 0f;
        public float InitialZoomAmmount => _initialZoomAmmount;
        private float _currentZoomAmmount = 0f;
        public float CurrentZoomAmmount => _currentZoomAmmount;

        private float _maxZoomAmmount = 70f;
        private float _minZoomAmmount = -10f;

        private float _ZoomModifier = 1f;

        public ZoomUpdater() {_currentZoomAmmount = _initialZoomAmmount;} 

        public void SubscribeToZoom(VisualElement element)
        {
            element.RegisterCallback<WheelEvent>(OnMouseZoom);
        }

        public void SetZoomLimits(float minZoomAmmount, float maxZoomAmmount)
        {
            _minZoomAmmount = minZoomAmmount;
            _maxZoomAmmount = maxZoomAmmount;
        }

        public void SetZoomModifier(float modifier)
        {
            _ZoomModifier = modifier;
        }

        private void OnMouseZoom(WheelEvent evt)
        {
            if (evt.ctrlKey == false) { return; }

            _currentZoomAmmount += evt.delta.y * _ZoomModifier;

            if (_currentZoomAmmount > _maxZoomAmmount)
            {
                _currentZoomAmmount = _maxZoomAmmount;
            }
            else if (_currentZoomAmmount < _minZoomAmmount)
            {
                _currentZoomAmmount = _minZoomAmmount;
            }
            else
            {
                OnZoomed?.Invoke(evt.delta.y);
            }
        }
    }
}