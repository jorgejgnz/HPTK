using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Hand.GestureDetection;
using HandPhysicsToolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit.Modules.Hand.Interactor
{
    [RequireComponent(typeof(PointInteractorModel))]
    public class PointInteractorView : HPTKView
    {
        public static List<PointInteractorView> registry = new List<PointInteractorView>();

        PointInteractorModel model;

        public HandView hand { get { return model.hand.specificView; } }

        public List<PointTrack> tracks { get { return model.tracks; } }
        public Vector3 position { get { return model.point.position; } }
        public Quaternion rotation { get { return model.point.rotation; } }
        public Vector3 forward { get { return model.point.forward; } }

        public bool isAvailable { get { return model.isAvailable; } }
        public bool hasDirection { get { return model.hasDirection; } }
        public float radius { get { return model.radius; } }
        public Gesture selectionGesture {  get { return model.selectionGesture; } }


        public UnityEvent<PointTrack> onFirstHover = new UnityEvent<PointTrack>();
        public UnityEvent<PointTrack> onHover = new UnityEvent<PointTrack>();
        public UnityEvent<PointTrack> onFirstPress = new UnityEvent<PointTrack>();
        public UnityEvent<PointTrack> onPress = new UnityEvent<PointTrack>();
        public UnityEvent<PointTrack> onRelease = new UnityEvent<PointTrack>();
        public UnityEvent<PointTrack> onLastRelease = new UnityEvent<PointTrack>();
        public UnityEvent<PointTrack> onUnhover = new UnityEvent<PointTrack>();
        public UnityEvent<PointTrack> onLastUnhover = new UnityEvent<PointTrack>();

        public override sealed void Awake()
        {
            base.Awake();
            model = GetComponent<PointInteractorModel>();
        }

        public void AddPointTrack(PointTrack pt)
        {
            model.tracks.Add(pt);
        }

        public void RemovePointTrack(PointTrack pt)
        {
            model.tracks.Remove(pt);
        }
    }
}
