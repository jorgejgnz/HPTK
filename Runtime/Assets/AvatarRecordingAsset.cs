using HandPhysicsToolkit.Input;
using HandPhysicsToolkit.Modules.Avatar;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HandPhysicsToolkit.Assets
{
    [CreateAssetMenu(fileName = "AvatarRecording", menuName = "HPTK/AvatarRecording")]
    public class AvatarRecordingAsset : ScriptableObject
    {
        public float fps;
        public List<AvatarSample> frames = new List<AvatarSample>();
    }

    [Serializable]
    public struct IDPSample
    {
        public AbstractTsfStruct wrist;
        public AbstractTsfStruct forearm;
        public AbstractTsfStruct thumb0;
        public AbstractTsfStruct thumb1;
        public AbstractTsfStruct thumb2;
        public AbstractTsfStruct thumb3;
        public AbstractTsfStruct index1;
        public AbstractTsfStruct index2;
        public AbstractTsfStruct index3;
        public AbstractTsfStruct middle1;
        public AbstractTsfStruct middle2;
        public AbstractTsfStruct middle3;
        public AbstractTsfStruct ring1;
        public AbstractTsfStruct ring2;
        public AbstractTsfStruct ring3;
        public AbstractTsfStruct pinky0;
        public AbstractTsfStruct pinky1;
        public AbstractTsfStruct pinky2;
        public AbstractTsfStruct pinky3;

        public IDPSample(HandView hand)
        {
            this.wrist = new AbstractTsfStruct(hand.wrist.transformRef, Space.World, "wrist");

            this.forearm = new AbstractTsfStruct(Space.Self, "forearm");

            if (hand.thumb.threeUnderLast) this.thumb0 = new AbstractTsfStruct(hand.thumb.threeUnderLast.transformRef, Space.Self, "thumb0");
            else this.thumb0 = new AbstractTsfStruct(Space.Self, "thumb0 (missing)");

            this.thumb1 = new AbstractTsfStruct(hand.thumb.twoUnderLast.transformRef, Space.Self, "thumb1");
            this.thumb2 = new AbstractTsfStruct(hand.thumb.oneUnderLast.transformRef, Space.Self, "thumb2");
            this.thumb3 = new AbstractTsfStruct(hand.thumb.last.transformRef, Space.Self, "thumb3");

            this.index1 = new AbstractTsfStruct(hand.index.twoUnderLast.transformRef, Space.Self, "index1");
            this.index2 = new AbstractTsfStruct(hand.index.oneUnderLast.transformRef, Space.Self, "index2");
            this.index3 = new AbstractTsfStruct(hand.index.last.transformRef, Space.Self, "index3");

            this.middle1 = new AbstractTsfStruct(hand.middle.twoUnderLast.transformRef, Space.Self, "middle1");
            this.middle2 = new AbstractTsfStruct(hand.middle.oneUnderLast.transformRef, Space.Self, "middle2");
            this.middle3 = new AbstractTsfStruct(hand.middle.last.transformRef, Space.Self, "middle3");

            this.ring1 = new AbstractTsfStruct(hand.ring.twoUnderLast.transformRef, Space.Self, "ring1");
            this.ring2 = new AbstractTsfStruct(hand.ring.oneUnderLast.transformRef, Space.Self, "ring2");
            this.ring3 = new AbstractTsfStruct(hand.ring.last.transformRef, Space.Self, "ring3");

            if (hand.pinky.threeUnderLast) this.pinky0 = new AbstractTsfStruct(hand.pinky.threeUnderLast.transformRef, Space.Self, "pinky0");
            else this.pinky0 = new AbstractTsfStruct(Space.Self, "pinky0 (missing)");

            this.pinky1 = new AbstractTsfStruct(hand.pinky.twoUnderLast.transformRef, Space.Self, "pinky1");
            this.pinky2 = new AbstractTsfStruct(hand.pinky.oneUnderLast.transformRef, Space.Self, "pinky2");
            this.pinky3 = new AbstractTsfStruct(hand.pinky.last.transformRef, Space.Self, "pinky3");
        }

        public IDPSample(InputDataProvider idp)
        {
            this.wrist = new AbstractTsfStruct(idp.bones[0]);
            this.forearm = new AbstractTsfStruct(idp.bones[1]);
            this.thumb0 = new AbstractTsfStruct(idp.bones[2]);
            this.thumb1 = new AbstractTsfStruct(idp.bones[3]);
            this.thumb2 = new AbstractTsfStruct(idp.bones[4]);
            this.thumb3 = new AbstractTsfStruct(idp.bones[5]);
            this.index1 = new AbstractTsfStruct(idp.bones[6]);
            this.index2 = new AbstractTsfStruct(idp.bones[7]);
            this.index3 = new AbstractTsfStruct(idp.bones[8]);
            this.middle1 = new AbstractTsfStruct(idp.bones[9]);
            this.middle2 = new AbstractTsfStruct(idp.bones[10]);
            this.middle3 = new AbstractTsfStruct(idp.bones[11]);
            this.ring1 = new AbstractTsfStruct(idp.bones[12]);
            this.ring2 = new AbstractTsfStruct(idp.bones[13]);
            this.ring3 = new AbstractTsfStruct(idp.bones[14]);
            this.pinky0 = new AbstractTsfStruct(idp.bones[15]);
            this.pinky1 = new AbstractTsfStruct(idp.bones[16]);
            this.pinky2 = new AbstractTsfStruct(idp.bones[17]);
            this.pinky3 = new AbstractTsfStruct(idp.bones[18]);
        }

        public void ApplyToHand(Transform origin, Transform moveThisAsWrist, HandView hand, string reprKey, bool applyFingerBonePos)
        {
            if (!moveThisAsWrist) moveThisAsWrist = hand.wrist.point.reprs[reprKey].transformRef;

            wrist.ApplyToTransformRelativeToOther(moveThisAsWrist, origin, true);

            thumb0.ApplyToBone(hand.thumb.threeUnderLast, reprKey, applyFingerBonePos);
            thumb1.ApplyToBone(hand.thumb.twoUnderLast, reprKey, applyFingerBonePos);
            thumb2.ApplyToBone(hand.thumb.oneUnderLast, reprKey, applyFingerBonePos);
            thumb3.ApplyToBone(hand.thumb.last, reprKey, applyFingerBonePos);
            index1.ApplyToBone(hand.index.twoUnderLast, reprKey, applyFingerBonePos);
            index2.ApplyToBone(hand.index.oneUnderLast, reprKey, applyFingerBonePos);
            index3.ApplyToBone(hand.index.last, reprKey, applyFingerBonePos);
            middle1.ApplyToBone(hand.middle.twoUnderLast, reprKey, applyFingerBonePos);
            middle2.ApplyToBone(hand.middle.oneUnderLast, reprKey, applyFingerBonePos);
            middle3.ApplyToBone(hand.middle.last, reprKey, applyFingerBonePos);
            ring1.ApplyToBone(hand.ring.twoUnderLast, reprKey, applyFingerBonePos);
            ring2.ApplyToBone(hand.ring.oneUnderLast, reprKey, applyFingerBonePos);
            ring3.ApplyToBone(hand.ring.last, reprKey, applyFingerBonePos);
            pinky0.ApplyToBone(hand.pinky.threeUnderLast, reprKey, applyFingerBonePos);
            pinky1.ApplyToBone(hand.pinky.twoUnderLast, reprKey, applyFingerBonePos);
            pinky2.ApplyToBone(hand.pinky.oneUnderLast, reprKey, applyFingerBonePos);
            pinky3.ApplyToBone(hand.pinky.last, reprKey, applyFingerBonePos);
        }
    }

    [Serializable]
    public struct AvatarSample
    {
        public AbstractTsfStruct head;
        public IDPSample leftHand;
        public IDPSample rightHand;

        public AvatarSample(Transform referenceTsf, Transform head, HandView leftHand, HandView rightHand)
        {
            Vector3 relHeadPos = referenceTsf.InverseTransformPoint(head.position);
            Quaternion relHeadRot = Quaternion.Inverse(referenceTsf.rotation) * head.rotation;

            this.head = new AbstractTsfStruct(Space.Self, "head");
            this.head.position = relHeadPos;
            this.head.rotation = relHeadRot;
            this.head.localScale = head.localScale;

            this.leftHand = new IDPSample(leftHand);
            this.rightHand = new IDPSample(rightHand);
        }
    }
}
