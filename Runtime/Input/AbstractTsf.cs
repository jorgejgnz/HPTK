using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Input
{
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

        public static void ApplyTransform(AbstractTsf bonePose, Transform receiverTsf)
        {
            if (bonePose.space == Space.World)
            {
                receiverTsf.position = bonePose.position;
                receiverTsf.rotation = bonePose.rotation;
            }
            else
            {
                receiverTsf.localPosition = bonePose.position;
                receiverTsf.localRotation = bonePose.rotation;
            }

            receiverTsf.localScale = bonePose.localScale;
        }
    }
}
