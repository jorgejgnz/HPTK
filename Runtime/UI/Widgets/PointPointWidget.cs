using HandPhysicsToolkit;
using HandPhysicsToolkit.Helpers;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit.UI
{
    public class PointPointWidget : Widget
    {
        Vector3 _closestSpherePointToSphere;
        Vector3 _dir;

        protected sealed override void UpdateWidget()
        {
            base.UpdateWidget();

            if (enterDistance == 0.0f)
            {
                enterDistance = 0.01f;
                Debug.LogWarning("Enter distance cannot be 0 for PointPointWidgets. Enter distance was set to 0.01f");
            }
        }

        protected sealed override void UpdatePointTrack(PointTrack track)
        {
            base.UpdatePointTrack(track);

            track.prevWorldPlanePos = track.worldPlanePos;
            track.worldPlanePos = transform.position;

            track.prevUnclampedNormPlanePos = track.unclampedNormPlanePos;
            track.unclampedNormPlanePos = Vector3.zero;

            // Depth
            track.prevDepth = track.depth;
            _closestSpherePointToSphere = track.interactor.position + (transform.position - track.interactor.position).normalized * track.interactor.radius;
            track.depth = Vector3.Distance(transform.position, _closestSpherePointToSphere);

            // Norm depth
            track.prevUnclampedNormDepth = track.unclampedNormDepth;
            track.unclampedNormDepth = MathHelpers.UnclampedInverseLerp(hoverDistance, enterDistance, track.depth);

            // State
            if (!track.interactor.gameObject.activeInHierarchy || !track.interactor.isAvailable)
            {
                track.state = PointTrackState.None;
            }
            else
            {
                switch (track.state)
                {
                    case PointTrackState.None:
                        // If it's candidate
                        if (IsHovering(track)) track.state = PointTrackState.Hovering;
                        break;
                    case PointTrackState.Hovering:
                        // If it's depth goes beyond max, then it has entered
                        if (track.unclampedNormDepth >= 1.0f) track.state = PointTrackState.Pressing;
                        // Else if it's not a candidate
                        else if (!IsHovering(track)) track.state = PointTrackState.None;
                        break;
                    case PointTrackState.Pressing:
                        if (track.unclampedNormDepth < 1.0f)
                        {
                            // If it's a candidate
                            if (IsHovering(track)) track.state = PointTrackState.Hovering;
                            // Else
                            else track.state = PointTrackState.None;
                        }
                        break;
                }
            }

            if (debugLog)
            {
                _dir = (track.interactor.position - transform.position).normalized;

                Debug.Log($"[{gameObject.name}] Track for {track.interactor.hand.name} ::::::::::::::");
                Debug.Log($"- Tracks count? {track.interactor.tracks.Count}");
                Debug.Log($"- uncNormDepth is norm? {track.unclampedNormDepth.IsNorm()} ({track.unclampedNormDepth})");
                Debug.Log($"- Can offer direction? {track.interactor.hasDirection}");
                Debug.Log($"- Is direction opposite? {Vector3.Dot(track.interactor.forward, _dir) <= 0.0f}");
            }
        }

        bool IsHovering(PointTrack track)
        {
            _dir = (track.interactor.position - transform.position).normalized;
            // If it's inside bounds and, if point has direction, direction is ok, then it's a candidate
            return (track.unclampedNormDepth.IsNorm() &&
                (!track.interactor.hasDirection || (track.interactor.hasDirection && Vector3.Dot(track.interactor.forward, _dir) <= 0.0f)));
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, hoverDistance);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, enterDistance);

            for (int p = 0; p < tracks.Count; p++)
            {
                if (tracks[p].state == PointTrackState.Pressing)
                {
                    Debug.DrawLine(transform.position, tracks[p].interactor.position, Color.green);
                }
                else if (tracks[p].state == PointTrackState.Hovering)
                {
                    Debug.DrawLine(transform.position, tracks[p].interactor.position, Color.blue);
                }
            }
        }
    }
}
