using HandPhysicsToolkit.Modules.Avatar;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Input
{
    [Serializable]
    public struct AbstractTsfStruct
    {
        public string name;

        public Vector3 position;
        public Quaternion rotation;
        public Space space;

        public Vector3 localScale;

        public AbstractTsfStruct(Space space, string name)
        {
            this.name = name;
            this.space = space;
            position = Vector3.zero;
            rotation = Quaternion.identity;
            localScale = Vector3.one;
        }

        public AbstractTsfStruct(Transform tsf, Space space, string name)
        {
            this.name = name;

            this.space = space;

            if (space == Space.Self)
            {
                this.position = tsf.localPosition;
                this.rotation = tsf.localRotation;
            }
            else
            {
                this.position = tsf.position;
                this.rotation = tsf.rotation;

            }

            this.localScale = tsf.localScale;
            
        }

        public AbstractTsfStruct(AbstractTsf abstractTsf)
        {
            this.name = abstractTsf.name;
            this.position = abstractTsf.position;
            this.rotation = abstractTsf.rotation;
            this.space = abstractTsf.space;
            this.localScale = abstractTsf.localScale;
        }

        public void ApplyToTransformRelativeToOther(Transform moveThis, Transform origin, bool applyPos)
        {
            if (applyPos) moveThis.position = origin.TransformPoint(position);
            moveThis.rotation = origin.rotation * rotation;
            
            moveThis.localScale = localScale;
        }

        public void ApplyToTransform(Transform receiverTsf, bool applyPos)
        {
            if (space == Space.World)
            {
                if (applyPos) receiverTsf.position = position;
                receiverTsf.rotation = rotation;
            }
            else
            {
                if (applyPos) receiverTsf.localPosition = position;
                receiverTsf.localRotation = rotation;
            }

            receiverTsf.localScale = localScale;
        }

        public void ApplyToBone(BoneView bone, string reprKey, bool applyPos)
        {
            if (!bone) return;

            ApplyToPoint(bone.point, reprKey, applyPos);
        }

        public void ApplyToPoint(PointView point, string reprKey, bool applyPos)
        {
            if (!point) return;

            if (point.reprs.ContainsKey(reprKey) && point.reprs[reprKey].transformRef)
            {
                if (point.bone.part.body.replicatedTsf) ApplyToTransformRelativeToOther(point.reprs[reprKey].transformRef, point.bone.part.body.replicatedTsf, applyPos);
                else ApplyToTransform(point.reprs[reprKey].transformRef, applyPos);
            }
        }
    }

    [Serializable]
    public class AbstractTsf
    {
        public string name;

        public Vector3 position;
        public Quaternion rotation;
        public Space space;

        public Vector3 localScale;

        public AbstractTsf(Vector3 position, Quaternion rotation, Space space, Vector3 localScale, string name)
        {
            this.position = position;
            this.rotation = rotation;
            this.space = space;

            this.localScale = localScale;
            this.name = name;
        }

        public AbstractTsf(string name, Space space)
        {
            this.space = space;
            this.name = name;
            this.position = Vector3.zero;
            this.rotation = Quaternion.identity;
            this.localScale = Vector3.one;
        }

        public AbstractTsf(Transform tsf, Space space)
        {
            this.name = tsf.name;
            this.space = space;

            if (space == Space.World)
            {
                this.position = tsf.position;
                this.rotation = tsf.rotation;
            }
            else
            {
                this.position = tsf.localPosition;
                this.rotation = tsf.localRotation;
            }

            this.localScale = tsf.localScale;
        }

        public AbstractTsf(AbstractTsf abstractTsf)
        {
            this.name = abstractTsf.name;
            this.space = abstractTsf.space;
            this.position = abstractTsf.position;
            this.rotation = abstractTsf.rotation;
        }

        public static void Copy(AbstractTsf from, AbstractTsf to)
        {
            to.name = from.name;
            to.space = from.space;
            to.position = from.position;
            to.rotation = from.rotation;
            to.localScale = from.localScale;
        }

        public void ApplyToTransform(Transform receiverTsf)
        {
            if (space == Space.World)
            {
                receiverTsf.position = position;
                receiverTsf.rotation = rotation;
            }
            else
            {
                receiverTsf.localPosition = position;
                receiverTsf.localRotation = rotation;
            }

            receiverTsf.localScale = localScale;
        }
    }
}
