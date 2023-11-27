using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.UI;
using HandPhysicsToolkit.Utils;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Hand.Interactor
{
    [RequireComponent(typeof(PointInteractorModel))]
    public class PointInteractorController : HPTKController
    {
        [ReadOnly]
        public PointInteractorModel model;

        private List<PointTrack> prevHovered = new List<PointTrack>();
        private List<PointTrack> prevPressed = new List<PointTrack>();
        private List<PointTrack> hovered = new List<PointTrack>();
        private List<PointTrack> pressed = new List<PointTrack>();

        public override void Awake()
        {
            base.Awake();
            model = GetComponent<PointInteractorModel>();
            SetGeneric(model.view, model);
        }

        public void OnEnable()
        {
            model.hand.registry.Add(this);
            PointInteractorView.registry.Add(model.view);
        }

        public void OnDisable()
        {
            model.hand.registry.Remove(this);
            PointInteractorView.registry.Remove(model.view);
        }

        private void OnDestroy()
        {
            PointInteractorView.registry.Remove(model.view);
        }

        public override void ControllerStart()
        {
            base.ControllerStart();

            if (!model.point)
            {
                if (model.hand && model.hand.index.tip.reprs.ContainsKey(model.repr))
                {
                    Transform indexTip = model.hand.index.tip.reprs[model.repr].transformRef;
                    model.point = BasicHelpers.InstantiateEmptyChild(indexTip.gameObject).transform;

                    Transform indexDistal = model.hand.index.last.reprs[model.repr].transformRef;
                    Vector3 fwdDir = (indexTip.position - indexDistal.position).normalized;

                    // Fix point fwd direction
                    model.point.localRotation = Quaternion.FromToRotation(model.point.forward, fwdDir);

                    if (model.hand.side == Side.Left)
                    {
                        model.point.Rotate(Vector3.forward, 180.0f, Space.Self);
                    }
                }
            }
        }

        public override void ControllerUpdate()
        {
            base.ControllerUpdate();

            hovered = model.tracks.Where(pt => pt.state == PointTrackState.Hovering).ToList();
            pressed = model.tracks.Where(pt => pt.state == PointTrackState.Pressing).ToList();

            // First hover
            if (prevHovered.Count == 0 && hovered.Count > 0)
            {
                model.view.onFirstHover.Invoke(hovered[0]);
            }

            // Hover
            for (int i = 0; i < hovered.Count; i++)
            {
                if (!prevHovered.Contains(hovered[i])) model.view.onHover.Invoke(hovered[i]);
            }

            // First press
            if (prevPressed.Count == 0 && pressed.Count > 0)
            {
                model.view.onFirstPress.Invoke(pressed[0]);
            }

            // Press
            for (int i = 0; i < pressed.Count; i++)
            {
                if (!prevPressed.Contains(pressed[i])) model.view.onPress.Invoke(pressed[i]);
            }

            // Release
            for (int i = 0; i < prevPressed.Count; i++)
            {
                if (!pressed.Contains(prevPressed[i])) model.view.onRelease.Invoke(prevPressed[i]);
            }

            // Last release
            if (prevPressed.Count > 0 && pressed.Count == 0)
            {
                model.view.onLastRelease.Invoke(prevPressed[0]);
            }

            // Unhover
            for (int i = 0; i < prevHovered.Count; i++)
            {
                if (!hovered.Contains(prevHovered[i])) model.view.onUnhover.Invoke(prevHovered[i]);
            }

            // Last unhover
            if (prevHovered.Count > 0 && hovered.Count == 0)
            {
                model.view.onLastUnhover.Invoke(prevHovered[0]);
            }

            prevHovered.Clear();
            prevHovered.AddRange(hovered);
            prevPressed.Clear();
            prevPressed.AddRange(pressed);
        }
    }
}
