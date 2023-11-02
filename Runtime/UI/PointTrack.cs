using HandPhysicsToolkit.Modules.Hand.Interactor;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit.UI
{
    public enum PointTrackState
    {
        None,       // UnclampedNormDepth < 0
        Hovering,   // UnclampedNormDepth > 0 && UnclampedNormDepth < 1 (&& inBounds for PointPlaneWidgets)
        Pressing    // UnclampedNormDepth >= 1 (passed through bounds for PointPlaneWidgets)
    }

    [Serializable]
    public class PointTrackEvent : UnityEvent<PointTrack> { }

    [Serializable]
    public class PointTrack
    {
        public float depth;
        public float unclampedNormDepth;

        [HideInInspector]
        public float prevDepth;
        [HideInInspector]
        public float prevUnclampedNormDepth;

        public Vector3 worldPos;
        [HideInInspector]
        public Vector3 prevWorldPos;

        public Vector3 relPos;
        [HideInInspector]
        public Vector3 prevRelPos;

        public Vector3 worldPlanePos;
        [HideInInspector]
        public Vector3 prevWorldPlanePos;

        public Vector2 unclampedNormPlanePos;
        [HideInInspector]
        public Vector2 prevUnclampedNormPlanePos;

        [HideInInspector]
        public Widget widget;
        [HideInInspector]
        public PointInteractorView interactor;

        public PointTrackState state = PointTrackState.None;

        public PointTrack(PointInteractorView interactor, Widget widget)
        {
            this.interactor = interactor;
            this.widget = widget;
        }
    }
}
