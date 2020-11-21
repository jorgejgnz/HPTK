using HPTK.Helpers;
using HPTK.Models.Interaction;
using HPTK.Settings;
using HPTK.Views.Handlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Models.Avatar
{
    public class ProxyHandModel : HPTKModel
    {
        [HideInInspector]
        public ProxyHandHandler handler;

        [HideInInspector]
        public AvatarModel avatar;

        [Header("Models")]
        public MasterHandModel master;
        public SlaveHandModel slave;
        public HandModel ghost;

        [HideInInspector]
        public HandModel[] hands;

        [Header("Configuration")]
        public CoreConfiguration configuration;

        [Header("Module registry")]
        public List<HPTKHandler> relatedHandlers = new List<HPTKHandler>();

        [HideInInspector]
        public Transform shoulderTip;

        public Side side = Side.Left;

        public float scale = 1.0f;

        [Header("Hand gesture theresolds")]
        public float minLerpToFist = 0.5f;
        public float minLerpToGrasp = 0.5f;

        [Header("Hands to update")]
        public bool updateValuesForMaster = true;
        public bool updateValuesForSlave = true;
        public bool updateValuesForOther = true;

        [Header("Values of each hand to update")]
        [Tooltip("Palm line lerp for each finger and fist lerp for the hand representation.")]
        public bool fist = true;
        [Tooltip("Base rotation for each finger and grasp lerp for the hand representation.")]
        public bool grasp = true;
        [Tooltip("Pinch for each finger.")]
        public bool pinch = true;
        [Tooltip("Strength values for each finger.")]
        public bool strength = true;
        [Tooltip("Flex values for each finger.")]
        public bool flex = true;
        [Tooltip("Update ray direction for the hand representation.")]
        public bool rays = true;

        [Header("Updated by Controller")]
        public float error;
        public float errorLerp;

        private void Awake()
        {
            List<HandModel> handList = new List<HandModel>();

            // Master hand is mandatory
            master.proxyHand = this;
            handList.Add(master);

            if (slave)
            {
                slave.proxyHand = this;
                handList.Add(slave);
            }

            if (ghost)
            {
                ghost.proxyHand = this;
                handList.Add(ghost);
            }

            hands = handList.ToArray();
        }
    }
}