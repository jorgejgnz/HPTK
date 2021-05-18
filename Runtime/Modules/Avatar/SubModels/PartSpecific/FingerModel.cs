using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Avatar
{
    public class FingerModel : PartModel
    {
        [Header("Debug")]
        public bool drawLength = false;

        [Header("Read Only")]
        [ReadOnly]
        public HumanFinger finger;
        [ReadOnly]
        public PointModel knuckle;
        [ReadOnly]
        public PointModel tip;
        [ReadOnly]
        public HandModel hand;
        [ReadOnly]
        public List<BoneModel> bonesFromRootToTip;

        float _length = 0.0f;
        public float length
        {
            get
            {
                if (_length <= 0.0f) _length = controller.GetFingerLength(this);
                return _length;
            }
            set
            {
                _length = value;
            }
        }

        public BoneModel threeUnderLast {
            get
            {
                if (bonesFromRootToTip.Count >= 4) return bonesFromRootToTip[bonesFromRootToTip.Count - 4];
                else return null;
            }
        }

        public BoneModel twoUnderLast {
            get
            {
                if (bonesFromRootToTip.Count >= 3) return bonesFromRootToTip[bonesFromRootToTip.Count - 3];
                else return null;
            }
        }

        public BoneModel oneUnderLast
        {
            get
            {
                if (bonesFromRootToTip.Count >= 2) return bonesFromRootToTip[bonesFromRootToTip.Count - 2];
                else return null;
            }
        }

        public BoneModel last
        {
            get
            {
                if (bonesFromRootToTip.Count >= 1) return bonesFromRootToTip[bonesFromRootToTip.Count - 1];
                else return null;
            }
        }

        public FingerView specificView { get { return view as FingerView; } }

        protected override void PartAwake()
        {
            base.PartAwake();

            if (parent is HandModel) hand = parent as HandModel;
            else Debug.LogWarning("[" + transform.name + "] Finger parent is not a hand");
        }

        protected sealed override PartView GetView()
        {
            PartView view = GetComponent<FingerView>();
            if (!view) view = gameObject.AddComponent<FingerView>();

            return view;
        }

        private void Update()
        {
            if (drawLength)
            {
                Vector3 dir = (bonesFromRootToTip[1].transformRef.position - bonesFromRootToTip[0].transformRef.position).normalized;
                Debug.DrawLine(root.transformRef.position, root.transformRef.position + dir * length, Color.yellow);
            }
        }
    }
}
