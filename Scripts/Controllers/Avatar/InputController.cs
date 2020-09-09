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

        bool initialized = false;

        private void Awake()
        {
            viewModel = new InputViewModel(model);
        }

        private void Start()
        {
            model.proxyHand.relatedHandlers.Add(this);

            if (model.proxyHand.master.bones.Length == 0)
                model.proxyHand.handler.onInitialized.AddListener(Initialize);
            else
                Initialize();
        }

        public void Initialize()
        {
            model.inputDataProvider.InitData(model.proxyHand.master);

            if (model.inputDataProvider.bones.Length != model.proxyHand.master.allTransforms.Length)
            {
                Debug.LogError("Number of master (" + model.proxyHand.master.name + ":" + model.proxyHand.master.allTransforms.Length + ") and replicated (" + model.inputDataProvider.name + ":" + model.inputDataProvider.bones.Length + ") transforms has to be the same!");
                return;
            }

            initialized = true;
        }

        void Update()
        {
            if (!initialized)
                return;

            model.inputDataProvider.UpdateData();

            for (int b = 0; b < model.proxyHand.master.allTransforms.Length; b++)
            {
                if (model.inputDataProvider.bones[b].space == Space.World)
                {
                    model.proxyHand.master.allTransforms[b].position = model.inputDataProvider.bones[b].position;
                    model.proxyHand.master.allTransforms[b].rotation = model.inputDataProvider.bones[b].rotation;
                }
                else
                {
                    model.proxyHand.master.allTransforms[b].localPosition = model.inputDataProvider.bones[b].position;
                    model.proxyHand.master.allTransforms[b].localRotation = model.inputDataProvider.bones[b].rotation;
                }
            }
        }
    }
}