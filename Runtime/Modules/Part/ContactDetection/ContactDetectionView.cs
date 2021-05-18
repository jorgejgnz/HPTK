using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Part.Puppet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit.Modules.Part.ContactDetection
{
    [RequireComponent(typeof(ContactDetectionModel))]
    public sealed class ContactDetectionView : HPTKView
    {
        ContactDetectionModel model;

        public PuppetView puppet { get { return model.puppet.view; } }

        public PartView part { get { return model.part.view; } }

        List<ContactView> _contacts = new List<ContactView>();
        public List<ContactView> contacts { get { model.contacts.ConvertAll(c => c.view, _contacts); return _contacts; } }

        // Events
        public UnityEvent onFirstEnter = new UnityEvent();
        public ContactEvent onEnter = new ContactEvent();
        public UnityEvent onFirstTouch = new UnityEvent();
        public ContactEvent onTouch = new ContactEvent();
        public UnityEvent onFirstGrasp = new UnityEvent();
        public ContactEvent onGrasp = new ContactEvent();
        public ContactEvent onUngrasp = new ContactEvent();
        public UnityEvent onLastUngrasp = new UnityEvent();
        public ContactEvent onUntouch = new ContactEvent();
        public UnityEvent onLastUntouch = new UnityEvent();
        public ContactEvent onExit = new ContactEvent();
        public UnityEvent onLastExit = new UnityEvent();

        public override sealed void Awake()
        {
            base.Awake();
            model = GetComponent<ContactDetectionModel>();
        }
    }
}
