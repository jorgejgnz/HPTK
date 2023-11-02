using HandPhysicsToolkit;
using HandPhysicsToolkit.Modules.Avatar;
using System.Collections;
using System.Collections.Generic;
using HandPhysicsToolkit.Utils;
using UnityEngine;
using System.Data.SqlTypes;

namespace HandPhysicsToolkit.Modules.Part.Constraint
{
    [RequireComponent(typeof(ConstraintModel))]
    public class ConstraintController : HPTKController
    {
        [ReadOnly]
        public ConstraintModel model;

        public override void Awake()
        {
            base.Awake();
            model = GetComponent<ConstraintModel>();
            SetGeneric(model.view, model);
        }

        public void OnEnable()
        {
            model.part.registry.Add(this);
        }

        public void OnDisable()
        {
            model.part.registry.Remove(this);
        }

        public override void ControllerStart()
        {
            base.ControllerStart();
        }

        public override void ControllerUpdate()
        {
            base.ControllerUpdate();

            if (!gameObject.activeSelf)
                return;

            // Copy master
            MimicPart(model.part);

            // Move constrained wrist to fix UI penetration
            foreach (Constraint constraint in model.constraints)
            {
                if (constraint.gameObject.activeInHierarchy)
                {
                    constraint.OnMimicCompleted(model.part);
                }
            }

            model.view.onConstraintsApplied.Invoke();
        }

        void MimicPart(PartModel part)
        {
            foreach (BoneModel b in part.bones)
            {
                MimicBone(b);
            }

            foreach (PartModel p in part.parts)
            {
                MimicPart(p);
            }
        }

        void MimicBone(BoneModel bone)
        {
            foreach (PointModel p in bone.points)
            {
                MimicPoint(p);
            }
        }

        void MimicPoint(PointModel point)
        {
            bool useWorld = point.bone.part.root == point.bone && model.mimicRootInWorldSpace;

            if (useWorld)
            {
                point.reprs[ConstraintModel.key].transformRef.position = point.reprs[AvatarModel.key].transformRef.position;
                point.reprs[ConstraintModel.key].transformRef.rotation = point.reprs[AvatarModel.key].transformRef.rotation;
            }
            else
            {
                point.reprs[ConstraintModel.key].transformRef.localPosition = point.reprs[AvatarModel.key].transformRef.localPosition;
                point.reprs[ConstraintModel.key].transformRef.localRotation = point.reprs[AvatarModel.key].transformRef.localRotation;
            }

            if (model.mimicScale) point.reprs[ConstraintModel.key].transformRef.localScale = point.reprs[AvatarModel.key].transformRef.localScale;
        }
    }
}
