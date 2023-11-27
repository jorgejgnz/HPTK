using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Hand.GestureDetection;
using HandPhysicsToolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Hand.Interactor
{
    public class PointInteractorModel : HPTKModel
    {
        public HandModel hand;
        public string repr = "";
        [Space]
        public bool isAvailable = true;
        public bool hasDirection = true;
        public float radius = 0.005f;
        public Gesture selectionGesture;
        [Space]
        public Transform point;
        public List<PointTrack> tracks = new List<PointTrack>();

        PointInteractorController _controller;
        public PointInteractorController controller
        {
            get
            {
                if (!_controller)
                {
                    _controller = GetComponent<PointInteractorController>();
                    if (!_controller) _controller = gameObject.AddComponent<PointInteractorController>();
                }

                return _controller;
            }
        }

        PointInteractorView _view;
        public PointInteractorView view
        {
            get
            {
                if (!_view)
                {
                    _view = GetComponent<PointInteractorView>();
                    if (!_view) _view = gameObject.AddComponent<PointInteractorView>();
                }

                return _view;
            }
        }

        public override void Awake()
        {
            base.Awake();

            if (repr == null || repr.Length == 0)
            {
                repr = AvatarModel.key;
            }
        }

        private void OnDrawGizmos()
        {
            if (point)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(point.position, radius);
                Gizmos.DrawLine(point.position, point.position + point.forward * radius * 2.0f);
            }
        }
    }
}
