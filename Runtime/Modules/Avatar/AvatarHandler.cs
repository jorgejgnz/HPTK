using HPTK.Models.Avatar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static HPTK.Views.Handlers.ProxyHandHandler;

namespace HPTK.Views.Handlers {
    public class AvatarHandler : HPTKHandler
    {
        public sealed class AvatarViewModel
        {
            AvatarModel model;

            // Hands
            public ProxyHandHandler rightHand { get { return model.rightHand.handler; } }
            public ProxyHandHandler leftHand { get { return model.leftHand.handler; } }
            ProxyHandHandler[] _hands;
            public ProxyHandHandler[] hands
            {
                get
                {
                    if (_hands == null && model.hands != null) _hands = GetProxyHandHandlersArray();
                    return _hands;
                }
            }

            // Related modules
            public HPTKHandler[] relatedHandlers { get { return model.relatedHandlers.ToArray(); } }

            // Transforms
            public Transform headSight { get { return model.headSight; } }
            public Transform headCenter { get { return model.headCenter; } }
            public GameObject headModel { get { return model.headModel; } }
            public Transform neck { get { return model.neck; } }

            public Transform torso { get { return model.torso; } }
            public Transform shoulderLeft { get { return model.shoulderLeft; } }
            public Transform shoulderRight { get { return model.shoulderRight; } }
            public Transform hips { get { return model.hips; } }
            public Transform feet { get { return model.feet; } }

            public Transform torsoDirection { get { return model.forwardDir; } }
            public Transform headDirection { get { return model.lookDir; } }

            // Extra
            public bool followsCamera { get { return model.followsCamera; } }

            public AvatarViewModel(AvatarModel model)
            {
                this.model = model;
            }
            ProxyHandHandler[] GetProxyHandHandlersArray()
            {
                List<ProxyHandHandler> proxyHandHandlersList = new List<ProxyHandHandler>();

                for (int h = 0; h < model.hands.Length; h++)
                {
                    if (model.hands[h] != null)
                    {
                        if (model.hands[h] == model.leftHand)
                            proxyHandHandlersList.Add(leftHand);

                        else if (model.hands[h] == model.rightHand)
                            proxyHandHandlersList.Add(rightHand);

                        else
                            proxyHandHandlersList.Add(model.hands[h].handler);
                    }
                }

                return proxyHandHandlersList.ToArray();
            }
        }
        public AvatarViewModel viewModel;
    }
}
