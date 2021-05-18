using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Part.Puppet;
using HandPhysicsToolkit.Physics;
using HandPhysicsToolkit.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit.Modules.Part.ContactDetection
{
    [RequireComponent(typeof(Pheasy))]
    public class ContactableView : HPTKView
    {
        Pheasy _pheasy;
        public Pheasy pheasy
        {
            get
            {
                if (_pheasy == null) _pheasy = GetComponent<Pheasy>();
                return _pheasy;
            }
        }

        public List<ContactView> contacts { get; } = new List<ContactView>();

        [Header("Read Only")]

        [ReadOnly]
        [SerializeField]
        bool _isEntered;
        [ReadOnly]
        [SerializeField]
        bool _isTouched;
        [ReadOnly]
        [SerializeField]
        bool _isGrasped;

        public bool isEntered { get { return _isEntered; } }
        public bool isTouched { get { return _isTouched; } }
        public bool isGrasped { get { return _isGrasped; } }

        [ReadOnly]
        public List<string> contactsDisplay = new List<string>();

        [Header("Events")]

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

        private void Update()
        {
            bool previousEntered = this.isEntered;
            bool previousTouched = this.isTouched;
            bool previousGrasped = this.isGrasped;

            _isEntered = _isTouched = _isGrasped = false;

            for (int c = 0; c < contacts.Count; c++)
            {
                if (contacts[c].type >= ContactType.Entered && !_isEntered) _isEntered = true;
                if (contacts[c].type >= ContactType.Touched && !_isTouched) _isTouched = true;
                if (contacts[c].type >= ContactType.Grasped && !_isGrasped) _isGrasped = true;
            }

            if (previousEntered && !isEntered)
                onLastExit.Invoke();
            if (previousTouched && !isTouched)
                onLastUntouch.Invoke();
            if (previousGrasped && !isGrasped)
                onLastUngrasp.Invoke();
            if (!previousGrasped && isGrasped)
                onFirstGrasp.Invoke();
            if (!previousTouched && isTouched)
                onFirstTouch.Invoke();
            if (!previousEntered && isEntered)
                onFirstEnter.Invoke();

            // Debug
            contactsDisplay.Clear();

            string s;
            for (int c = 0; c < contacts.Count; c++)
            {
                s = contacts[c].detector.part.name + " (" + contacts[c].type.ToString() + ")";
                contactsDisplay.Add(s);
            }
        }

        public void AddContact(Contact contact)
        {
            if (!contacts.Contains(contact.view))
                contacts.Add(contact.view);
        }

        public void RemoveContact(Contact contact)
        {
            if (contacts.Contains(contact.view))
                contacts.Remove(contact.view);
        }

        public override sealed void Awake()
        {
            base.Awake();
        }
    }
}
