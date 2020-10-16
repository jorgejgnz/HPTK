using HPTK.Views.Handlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Models.Avatar
{
    public class AvatarModel : HPTKModel
    {
        public ProxyHandModel leftHand;
        public ProxyHandModel rightHand;

        [HideInInspector]
        public ProxyHandModel[] hands;

        [Header("Module registry")]
        public List<HPTKHandler> relatedHandlers = new List<HPTKHandler>();

        [Header("Head")]
        public Transform headSight;
        public Transform headCenter;
        public GameObject headModel;
        public Transform neck;

        [Header("Body")]
        public Transform torso;
        public Transform shoulderLeft;
        public Transform shoulderCenter;
        public Transform shoulderRight;
        public Transform hips;
        public Transform feet;
        
        [Header("Directions")]
        public Transform forwardDir;
        public Transform lookDir;

        [Header("Control")]
        public bool followsCamera = true;

        private void Awake()
        {
            hands = new ProxyHandModel[2] { leftHand, rightHand };

            for (int i = 0; i < hands.Length; i++)
            {
                hands[i].avatar = this;
            }
        }
    }
}
