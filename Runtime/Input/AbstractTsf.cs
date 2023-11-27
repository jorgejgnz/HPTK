using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Modules.Avatar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
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

        public Vector3 worldPosition
        {
            get
            {
                if (space == Space.World) return position;
                else { Debug.LogError($"AbstractTsf {name} is not in world space!"); return Vector3.zero; }
            }
        }

        public Quaternion worldRotation
        {
            get
            {
                if (space == Space.World) return rotation;
                else { Debug.LogError($"AbstractTsf {name} is not in world space!"); return Quaternion.identity; }
            }
        }

        public Vector3 localPosition
        {
            get
            {
                if (space == Space.Self) return position;
                else { Debug.LogError($"AbstractTsf {name} is not in local space!"); return Vector3.zero; }
            }
        }

        public Quaternion localRotation
        {
            get
            {
                if (space == Space.Self) return rotation;
                else { Debug.LogError($"AbstractTsf {name} is not in local space!"); return Quaternion.identity; }
            }
        }

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

        public void Copy(Transform from, Space space)
        {
            this.space = space;
            this.localScale = from.localScale;
            this.name = from.name;

            if (space == Space.World)
            {
                position = from.position;
                rotation = from.rotation;
            }
            else
            {
                position = from.localPosition;
                rotation = from.localRotation;
            }
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

        public Vector3 TransformPoint(Vector3 localPoint)
        {
            if (space == Space.World)
            {
                return MathHelpers.TransformPoint(position, rotation, localScale, localPoint);
            }
            else
            {
                Debug.LogError("TransformPoint can only be applied to AbstractTsf in world space");
                return Vector3.zero;
            }
        }

        public Vector3 InverseTransformPoint(Vector3 worldPoint)
        {
            if (space == Space.World)
            {
                return MathHelpers.InverseTransformPoint(position, rotation, localScale, worldPoint);
            }
            else
            {
                Debug.LogError("InverseTransformPoint can only be applied to AbstractTsf in world space");
                return Vector3.zero;
            }
        }

        public void Reset()
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            localScale = Vector3.one;
        }
    }
}
