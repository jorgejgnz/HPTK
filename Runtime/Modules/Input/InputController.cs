using HPTK.Input;
using HPTK.Models.Avatar;
using HPTK.Views.Handlers.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Controllers.Input
{
    public class InputController : InputHandler
    {
        public InputModel model;

        private void Awake()
        {
            viewModel = new InputViewModel(model);
        }

        private void Start()
        {
            model.proxyHand.relatedHandlers.Add(this);

            model.inputDataProvider.InitData(model.proxyHand.master);

            // Input data for finger tips can be ignored as it will depend on the master hand model
            // This array MUST match model.inputDataProvider.bones[]
            model.bonesToUpdate = new MasterBoneModel[19]
            {
                model.wrist,
                model.forearm,
                model.thumb0,
                model.thumb1,
                model.thumb2,
                model.thumb3,
                model.index1,
                model.index2,
                model.index3,
                model.middle1,
                model.middle2,
                model.middle3,
                model.ring1,
                model.ring2,
                model.ring3,
                model.pinky0,
                model.pinky1,
                model.pinky2,
                model.pinky3
            };
        }

        void Update()
        {
            model.inputDataProvider.UpdateData();

            // Update pos and rot for wrist and forearm

            if (model.bonesToUpdate[0] != null)
            {
                UpdateMasterBonePos(model.wrist, model.inputDataProvider.bones[0]);
                UpdateMasterBoneRot(model.wrist, model.inputDataProvider.bones[0]);
            }

            if (model.bonesToUpdate[1] != null)
            {
                UpdateMasterBonePos(model.forearm, model.inputDataProvider.bones[1]);
                UpdateMasterBoneRot(model.forearm, model.inputDataProvider.bones[1]);
            }

            // Update only rot for bones as input data may assume master bone lengths that don't match master on custom rigged hands

            for (int i = 2; i < model.bonesToUpdate.Length; i++)
            {
                if (model.bonesToUpdate[i] != null)
                    UpdateMasterBoneRot(model.bonesToUpdate[i], model.inputDataProvider.bones[i]);
            }
        }

        void UpdateMasterBonePos(MasterBoneModel masterBone, AbstractTsf inputData)
        {
            if (inputData.space == Space.World)
                masterBone.transformRef.position = inputData.position;
            else
                masterBone.transformRef.localPosition = inputData.position;
        }

        void UpdateMasterBoneRot(MasterBoneModel masterBone, AbstractTsf inputData)
        {
            if (inputData.space == Space.World)
                masterBone.transformRef.rotation = inputData.rotation;
            else
                masterBone.transformRef.localRotation = inputData.rotation;
        }

    }
}