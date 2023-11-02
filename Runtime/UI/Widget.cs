using HandPhysicsToolkit;
using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Modules.Hand.Interactor;
using HandPhysicsToolkit.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit.UI
{
    public abstract class Widget : MonoBehaviour
    {
        public float enterDistance = 0.0f; // Min
        public float hoverDistance = 0.05f; // Max
        public float maxDistance = 1000.0f;

        public List<PointTrack> tracks = new List<PointTrack>();

        public List<PointTrack> hovered = new List<PointTrack>();
        public List<PointTrack> pressed = new List<PointTrack>();

        [Header("Events")]
        public PointTrackEvent onFirstHover = new PointTrackEvent();
        public PointTrackEvent onHover = new PointTrackEvent();
        public PointTrackEvent onFirstPress = new PointTrackEvent();
        public PointTrackEvent onPress = new PointTrackEvent();
        public PointTrackEvent onRelease = new PointTrackEvent();
        public PointTrackEvent onLastRelease = new PointTrackEvent();
        public PointTrackEvent onUnhover = new PointTrackEvent();
        public PointTrackEvent onLastUnhover = new PointTrackEvent();

        [Header("Debug")]
        public float currentMaxDepth;
        public bool debugLog = false;
        public bool drawGizmos = true;
        public Transform center;

        PointTrack _pt;

        private List<PointTrack> prevHovered = new List<PointTrack>();
        private List<PointTrack> prevPressed = new List<PointTrack>();

        private void Start()
        {
            if (!center) center = transform;
        }

        private void Update()
        {
            UpdateWidget();

            // Update tracks
            for (int p = 0; p < PointInteractorView.registry.Count; p++)
            {
                _pt = null;

                for (int i = 0; i < tracks.Count; i++)
                {
                    if (tracks[i].interactor == PointInteractorView.registry[p])
                    {
                        _pt = tracks[i];
                        break;
                    }
                }

                float dist = Vector3.Distance(center.position, PointInteractorView.registry[p].position);

                // Add tracks for point interactors that are not tracked but are close
                if (_pt == null)
                {
                    if (dist < maxDistance)
                    {
                        _pt = new PointTrack(PointInteractorView.registry[p], this);
                        tracks.Add(_pt);
                        PointInteractorView.registry[p].AddPointTrack(_pt);
                    }
                }

                // Remove tracks that has null interactor or are too far
                if (_pt != null)
                {
                    if (dist > maxDistance)
                    {
                        tracks.Remove(_pt);
                        PointInteractorView.registry[p].RemovePointTrack(_pt);
                        _pt = null;
                    }
                }

                if (_pt != null)
                {
                    // Update common attributes
                    _pt.prevWorldPos = _pt.worldPos;
                    _pt.worldPos = _pt.interactor.position;

                    _pt.prevRelPos = _pt.relPos;
                    _pt.relPos = transform.GetRelativePosForScaleOne(_pt.worldPos);

                    // Update state and specific attributes
                    UpdatePointTrack(_pt);
                } 
            }

            // Update lists
            hovered = tracks.Where(pt => pt.state == PointTrackState.Hovering).ToList();
            pressed = tracks.Where(pt => pt.state == PointTrackState.Pressing).ToList();

            // First hover
            if (prevHovered.Count == 0 && hovered.Count > 0)
            {
                onFirstHover.Invoke(hovered[0]);
            }

            // Hover
            for (int i = 0; i < hovered.Count; i++)
            {
                if (!prevHovered.Contains(hovered[i])) onHover.Invoke(hovered[i]);
            }

            // First press
            if (prevPressed.Count == 0 && pressed.Count > 0)
            {
                onFirstPress.Invoke(pressed[0]);
            }

            // Press
            for (int i = 0; i < pressed.Count; i++)
            {
                if (!prevPressed.Contains(pressed[i])) onPress.Invoke(pressed[i]);
            }

            // Release
            for (int i = 0; i < prevPressed.Count; i++)
            {
                if (!pressed.Contains(prevPressed[i])) onRelease.Invoke(prevPressed[i]);
            }

            // Last release
            if (prevPressed.Count > 0 && pressed.Count == 0)
            {
                onLastRelease.Invoke(prevPressed[0]);
            }

            // Unhover
            for (int i = 0; i < prevHovered.Count; i++)
            {
                if (!hovered.Contains(prevHovered[i])) onUnhover.Invoke(prevHovered[i]);
            }

            // Last unhover
            if (prevHovered.Count > 0 && hovered.Count == 0)
            {
                onLastUnhover.Invoke(prevHovered[0]);
            }

            prevHovered.Clear();
            prevHovered.AddRange(hovered);
            prevPressed.Clear();
            prevPressed.AddRange(pressed);

            // Update current max depth
            PointTrack deepest;
            if (pressed.Count > 0) deepest = pressed[0];
            else if (hovered.Count > 0) deepest = hovered.OrderBy(x => x.unclampedNormDepth).LastOrDefault();
            else deepest = null;

            if (deepest != null) currentMaxDepth = deepest.unclampedNormDepth;
            else currentMaxDepth = 0;
        }

        protected virtual void UpdateWidget() { }

        protected virtual void UpdatePointTrack(PointTrack pt) { }
    }
}
