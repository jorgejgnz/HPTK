using HPTK.Models.Avatar;
using HPTK.Views.Handlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Models
{
    public class CoreController : CoreHandler
    {
        public CoreModel model;

        private void Awake()
        {
            model.handler = this;
            viewModel = new CoreViewModel(model);
        }

        private void Start()
        {

        }
    }
}
