using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Physics;
using HandPhysicsToolkit.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Hand.ABPuppet
{
    public class ABPuppetReprModel : ReprModel
    {
        [Serializable]
        public class ArticulationNode
        {
            public ArticulationBody ab;
            public char axis = '-';

            public ArticulationNode(ArticulationBody ab)
            {
                this.ab = ab;
            }
        }

        public Pheasy pheasy;

        [Header("AB Puppet")]
        public ArticulationBodyFollower abFollower;
        public Collider colliderRef;

        [Header("Nodes")]
        public ArticulationBody root;
        public ArticulationBody rotation;
        public ArticulationBody rotX, rotY, rotZ;
        public ArticulationBody posX, posY, posZ;
        public List<ArticulationNode> articulationNodes = new List<ArticulationNode>();

        [Header("Characteristics")]
        [ReadOnly]
        public bool isStrong = false;
        [ReadOnly]
        public bool zOnly = false;

        public ABPuppetReprView specificView { get { return view as ABPuppetReprView; } }

        protected sealed override ReprView GetView()
        {
            ReprView view = GetComponent<ABPuppetReprView>();
            if (!view) view = gameObject.AddComponent<ABPuppetReprView>();

            return view;
        }

        protected sealed override string FindKey()
        {
            return ABPuppetModel.key;
        }
    }
}
