using HPTK.Models.Avatar;
using HPTK.Views.Events;
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

        [HideInInspector]
        public Handler handler;

        public float readonlyValue = 0.0f;
        public float modifiableValue = 0.0f;

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
            public float readonlyValue { get { return model.readonlyValue; } }
            public float modifiableValue {
                get { return model.modifiableValue; }
                set
                {
                    model.modifiableValue = value;
                    model.handler.onValueChanged.Invoke(value);
                }
            }
            public ProxyHandHandler proxyHand { get { return model.proxyHand.handler; } }
            public ViewModel(Model model)
            {
                this.model = model;
            }
        }
        public ViewModel viewModel;

        public FloatEvent onValueChanged;
        public UnityEvent onOtherEvent;
    }

    public class Controller : Handler
    {
        public Model model;

        private void Awake()
        {
            viewModel = new ViewModel(model);
            model.handler = this;
        }

        private void Start()
        {
            model.proxyHand.relatedHandlers.Add(this);
        }
    }
}
