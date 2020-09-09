using HPTK.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HPTK.Views.Handlers
{
    public class CoreHandler : HPTKHandler
    {
        public sealed class CoreViewModel
        {
            CoreModel model;

            public CoreViewModel(CoreModel model)
            {
                this.model = model;
            }
        }
        public CoreViewModel viewModel;

        public UnityEvent onInit;
    } 
}
