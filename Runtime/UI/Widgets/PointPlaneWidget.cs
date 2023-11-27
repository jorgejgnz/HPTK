using HandPhysicsToolkit;
using HandPhysicsToolkit.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class PointPlaneWidget : Widget
    {
        [Header("Plane")]
        public Transform normal;
        public bool useInvertedNormal = true;
        public bool onlyOppositeInteractors = true;

        public Vector3 normalDir { get { if (useInvertedNormal) return normal.forward * -1.0f; else return normal.forward; } }

        RectTransform rectTsf;

        Vector3[] worldRectCorners = new Vector3[4];
        Vector3 _debugX, _debugY, _debugZ;
        Plane plane = new Plane();

        Vector3 _scaledRelPos;
        Vector3 _closestSpherePointToPlane;

        private void Awake()
        {
            rectTsf = GetComponent<RectTransform>();
            if (!normal) normal = transform;
        }

        protected sealed override void UpdateWidget()
        {
            base.UpdateWidget();

            rectTsf.GetWorldCorners(worldRectCorners);
            plane.Set3Points(worldRectCorners[0], worldRectCorners[1], worldRectCorners[2]);
        }

        protected sealed override void UpdatePointTrack(PointTrack track)
        {
            base.UpdatePointTrack(track);

            track.prevWorldPlanePos = track.worldPlanePos;
            track.worldPlanePos = transform.position + Vector3.ProjectOnPlane(track.worldPos - transform.position, plane.normal);

            track.prevUnclampedNormPlanePos = track.unclampedNormPlanePos;
            _scaledRelPos = transform.InverseTransformPoint(track.worldPos);
            track.unclampedNormPlanePos.x = (_scaledRelPos.x - rectTsf.rect.x) / rectTsf.rect.width;
            track.unclampedNormPlanePos.y = (_scaledRelPos.y - rectTsf.rect.y) / rectTsf.rect.height;

            // Depth
            track.prevDepth = track.depth;
            _closestSpherePointToPlane = track.interactor.position - normalDir * track.interactor.radius;
            track.depth = plane.GetDistanceToPoint(_closestSpherePointToPlane);

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
                        // If it's acandidate
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
                Debug.Log($"[{gameObject.name}] Track for {track.interactor.hand.name} ::::::::::::::");
                Debug.Log($"- Tracks count? {track.interactor.tracks.Count}");
                Debug.Log($"- uncNormPlanePos is norm? {track.unclampedNormPlanePos.IsNorm()} ({track.unclampedNormPlanePos})");
                Debug.Log($"- uncNormDepth is norm? {track.unclampedNormDepth.IsNorm()} ({track.unclampedNormDepth})");
                Debug.Log($"- Can offer direction? {track.interactor.hasDirection}");
                Debug.Log($"- Is direction opposite? {Vector3.Dot(track.interactor.forward, normalDir) <= 0.0f}");
            }
        }

        bool IsHovering(PointTrack track)
        {
            // If it's inside bounds and, if point has direction, direction is ok, then it's a candidate
            return (track.unclampedNormPlanePos.IsNorm() &&
                track.unclampedNormDepth.IsNorm() &&
                (!track.interactor.hasDirection || !onlyOppositeInteractors || (track.interactor.hasDirection && Vector3.Dot(track.interactor.forward, normalDir) <= 0.0f)));
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;

            if (!normal) normal = transform;
            if (!rectTsf) rectTsf = GetComponent<RectTransform>();
            
            rectTsf.GetWorldCorners(worldRectCorners);
            DrawRectangle(worldRectCorners[0], worldRectCorners[1], worldRectCorners[2], worldRectCorners[3], normalDir, hoverDistance, Color.blue);
            DrawRectangle(worldRectCorners[0], worldRectCorners[1], worldRectCorners[2], worldRectCorners[3], normalDir, enterDistance, Color.green);

            for (int p = 0; p < tracks.Count; p++)
            {
                if (tracks[p].state == PointTrackState.Pressing)
                {
                    _debugX = Vector3.Project(tracks[p].worldPos - transform.position, transform.right);
                    _debugY = Vector3.Project(tracks[p].worldPos - transform.position, transform.up);
                    _debugZ = Vector3.Project(tracks[p].worldPos - transform.position, transform.forward);

                    Debug.DrawLine(transform.position, transform.position + _debugX, Color.red);
                    Debug.DrawLine(transform.position, transform.position + _debugY, Color.green);
                    Debug.DrawLine(transform.position, transform.position + _debugZ, Color.blue);

                    Gizmos.color = Color.white;
                    Gizmos.DrawSphere(tracks[p].worldPlanePos, 0.02f);
                }
                else if (tracks[p].state == PointTrackState.Hovering)
                {
                    Debug.DrawLine(transform.position, tracks[p].interactor.position, Color.blue);
                }
            }
        }

        void DrawRectangle(Vector3 bottomLeft, Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 planeNormal, float offset, Color color)
        {
            Debug.DrawLine(bottomLeft + planeNormal * offset, topLeft + planeNormal * offset, color);
            Debug.DrawLine(topLeft + planeNormal * offset, topRight + planeNormal * offset, color);
            Debug.DrawLine(topRight + planeNormal * offset, bottomRight + planeNormal * offset, color);
            Debug.DrawLine(bottomRight + planeNormal * offset, bottomLeft + planeNormal * offset, color);
        }
    }
}
