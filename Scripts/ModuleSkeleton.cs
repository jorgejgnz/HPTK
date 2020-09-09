using HPTK.Models.Avatar;
using HPTK.Views.Handlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HPTK.Abstracts
{
    public abstract class ChildModel : HPTKModel
    {
        [HideInInspector]
        public Model parent;
    }

    public abstract class Model : HPTKModel
    {
		public ProxyHandModel proxyHand;
        public ChildModel child;
        public float value = 0.0f;

        private void Awake()
        {
            child.parent = this;
        }
    }

    public abstract class Handler : HPTKHandler
    {
        public sealed class ViewModel
        {
            Model model;
            public float value { get { return model.value; } }
			public ProxyHandHandler proxyHand { get { return model.proxyHand.handler; } }
            public ViewModel(Model model)
            {
                this.model = model;
            }
        }
        public ViewModel viewModel;

        public UnityEvent onEvent;
    }

    public class Controller : Handler
    {
        public Model model;

        private void Awake()
        {
            viewModel = new ViewModel(model);
        }

        private void Start()
        {
            model.proxyHand.relatedHandlers.Add(this);
        }
    }
}
