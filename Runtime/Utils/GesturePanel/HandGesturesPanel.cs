using HandPhysicsToolkit;
using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Hand.GestureDetection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Utils
{
    public class HandGesturesPanel : MonoBehaviour
    {
        public Side side;

        public ScaleLerper grasp;
        public ScaleLerper fist;
        public ScaleLerper gesture;

        public FingerGesturesPanel thumb;
        public FingerGesturesPanel index;
        public FingerGesturesPanel middle;
        public FingerGesturesPanel ring;
        public FingerGesturesPanel pinky;

        GestureDetectionView detector;
        Gesture extra;

        int avatarToSearch = 0;

        private void Start()
        {
            if (HPTK.core.avatars.Count > 0) FindAvatar();
            else HPTK.core.onAvatarEntered.AddListener(avatar => Init(avatar));
        }

        void FindAvatar()
        {
            if (avatarToSearch < 0 || avatarToSearch >= HPTK.core.avatars.Count)
            {
                Debug.LogWarning("Any avatar was found");
                return;
            }

            AvatarView avatar = HPTK.core.avatars[avatarToSearch];

            if (avatar == null)
            {
                Debug.LogWarning("Any avatar was found");
                return;
            }

            if (avatar.started) Init(avatar);
            else avatar.onStarted.AddListener(() => Init(avatar));
        }

        void Init(AvatarView avatar)
        {
            if (detector != null)
                return;

            if (side == Side.Left)
            {
                detector = BasicHelpers.FindFirst<HPTKView, GestureDetectionView>(avatar.body.leftHand.registry);
            }
            else
            {
                side = Side.Right;
                detector = BasicHelpers.FindFirst<HPTKView, GestureDetectionView>(avatar.body.rightHand.registry);
            }

            if (detector == null)
            {
                Debug.LogWarning(side.ToString() + " hand of avatar " + avatar.name + " does not have a GestureDetectionController registered. Searching in the next avatar");
                avatarToSearch++;
                FindAvatar();
                return;
            }

            thumb.finger = detector.thumb;
            index.finger = detector.index;
            middle.finger = detector.middle;
            ring.finger = detector.ring;
            pinky.finger = detector.pinky;

            if (detector.extra.Count > 0) extra = detector.extra[0];

            thumb.SearchExtraGesture();
            index.SearchExtraGesture();
            middle.SearchExtraGesture();
            ring.SearchExtraGesture();
            pinky.SearchExtraGesture();
        }

        private void Update()
        {
            if (detector == null) return;
            if (detector.grasp) grasp.UpdateSize(detector.grasp.lerp); else grasp.UpdateSize(0.0f);
            if (detector.fist) fist.UpdateSize(detector.fist.lerp); else fist.UpdateSize(0.0f);
            if (extra) gesture.UpdateSize(extra.lerp); else gesture.UpdateSize(0.0f);
        }
    }
}
