using HPTK.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK
{
    public class HPTKElement : MonoBehaviour
    {
        public HPTKCore core { get { return HPTKCore.core; } }
    }

    public class HPTKCore : MonoBehaviour
    {
        public static HPTKCore core;

        public CoreModel model;

        private void Awake()
        {
            if (!core)
                core = this;
        }

        void Start() { }
    }
}
