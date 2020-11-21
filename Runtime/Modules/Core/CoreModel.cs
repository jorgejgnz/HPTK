using HPTK.Models.Interaction;
using HPTK.Models.Avatar;
using HPTK.Settings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HPTK.Views.Handlers;

namespace HPTK.Models
{
    public class CoreModel : HPTKModel
    {
        [HideInInspector]
        public CoreHandler handler;

        public CoreConfiguration configuration;

        [Header("Default configuration assets")]
        public ScriptableObject[] defaultConfAssets;

        public List<AvatarModel> avatars;

        [Header("Singletons")]
        public Transform trackedCamera;
    }
}
