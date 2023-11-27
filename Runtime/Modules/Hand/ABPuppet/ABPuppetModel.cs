using HandPhysicsToolkit;
using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Hand.ABPuppet;
using HandPhysicsToolkit.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Hand.ABPuppet
{
    public class ABPuppetModel : HPTKModel
    {
        public static string key = "puppet.ab";

        public HandModel hand;
        public ABHierarchyWizard hierarchyWizard;
        public ArticulationBodiesConfiguration configuration;
        public string followThis = "";

        CustomController _controller;
        public CustomController controller
        {
            get
            {
                if (!_controller)
                {
                    _controller = GetComponent<CustomController>();
                    if (!_controller) _controller = gameObject.AddComponent<CustomController>();
                }

                return _controller;
            }
        }

        ABPuppetView _view;
        public ABPuppetView view
        {
            get
            {
                if (!_view)
                {
                    _view = GetComponent<ABPuppetView>();
                    if (!_view) _view = gameObject.AddComponent<ABPuppetView>();
                }

                return _view;
            }
        }

        public override void Awake()
        {
            base.Awake();

            if (followThis == null || followThis.Length == 0) followThis = AvatarModel.key;
        }
    }
}