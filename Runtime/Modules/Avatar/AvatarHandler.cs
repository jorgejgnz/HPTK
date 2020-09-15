using HPTK.Models.Avatar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HPTK.Views.Handlers {
    public class AvatarHandler : HPTKHandler
    {
        public sealed class AvatarViewModel
        {
            AvatarModel model;

            public ProxyHandHandler rightHand { get { return model.rightHand.handler; } }
            public ProxyHandHandler leftHand { get { return model.leftHand.handler; } }

            public AvatarViewModel(AvatarModel model)
            {
                this.model = model;
            }
        }
        public AvatarViewModel viewModel;

        // public UnityEvent onEvent;
    }
}
