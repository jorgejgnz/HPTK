using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Part.Puppet;
using System;
using System.Linq;
using HandPhysicsToolkit.Physics;
using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Utils;

namespace HandPhysicsToolkit.Modules.Part.ContactDetection
{
    [RequireComponent(typeof(ContactDetectionModel))]
    public class ContactDetectionController : HPTKController
    {
        [ReadOnly]
        public ContactDetectionModel model;

        List<Rigidbody> foundRbs = new List<Rigidbody>();

        List<Contact> previousContacts = new List<Contact>();
        List<Rigidbody> previouslyEntered = new List<Rigidbody>();
        List<Rigidbody> enteredRbs = new List<Rigidbody>();
        List<Rigidbody> exitedRbs = new List<Rigidbody>();

        Contact stayedContact;
        ContactableView stayedContactable;

        public override void Awake()
        {
            base.Awake();
            model = GetComponent<ContactDetectionModel>();
            SetGeneric(model.view, model);
        }

        private void OnEnable()
        {
            model.part.registry.Add(this);
        }

        private void OnDisable()
        {
            model.part.registry.Remove(this);
        }

        public override sealed void ControllerStart()
        {
            base.ControllerStart();

            if (model.part.body.avatar.ready) OnAvatarReady();
            else model.part.body.avatar.view.onStarted.AddListener(OnAvatarReady);
        }

        void OnAvatarReady()
        {
            if (!model.part.root.reprs.ContainsKey(PuppetModel.key) || !(model.part.root.reprs[PuppetModel.key] is PuppetReprModel))
            {
                Debug.LogError("Root of part " + model.part.name + ", " + model.part.root.name + ", does not have a valid " + PuppetModel.key + " representation. ContactDetection cannot start for this part");
                return;
            }

            PuppetReprModel rootSlave = model.part.root.reprs[PuppetModel.key] as PuppetReprModel;

            if (rootSlave.specificView.ready) OnPhysicsReady();
            else rootSlave.specificView.onPhysicsReady.AddListener(OnPhysicsReady);
        }

        void OnPhysicsReady()
        {
            if (!model.detectOnlyThese)
            {
                model.bonesToDetect.Clear();
                PartStart(model.part, new List<PartModel>());
            }

            model.bonesToDetect.ForEach(b => StartListening(b));
        }

        void PartStart(PartModel part, List<PartModel> processedParts)
        {
            if (processedParts.Contains(part))
                return;

            processedParts.Add(part);

            part.parts.ForEach(p =>
            {
                if (!p.registry.Find(c => c is ContactDetectionController)) PartStart(p, processedParts);
            });

            part.bones.ForEach(b => { if (!model.bonesToDetect.Contains(b)) model.bonesToDetect.Add(b); });
        }

        void StartListening(BoneModel bone)
        {
            PuppetReprModel slave = bone.reprs[PuppetModel.key] as PuppetReprModel;

            // Listen to trigger and collision notifiers for each bone.pheasy (when possible)
            if (slave.pheasy)
            {
                if (slave.pheasy.collisionNotifier)
                {
                    slave.pheasy.collisionNotifier.invokeStayEvents = true;
                    slave.pheasy.collisionNotifier.onRbEnter.AddListener(col => OnRbTouchStart(col, bone));
                    slave.pheasy.collisionNotifier.onRbStay.AddListener(col => OnRbTouchStay(col, bone));
                    slave.pheasy.collisionNotifier.onRbExit.AddListener(col => OnRbTouchEnd(col, bone));
                }

                if (model.hoverDetectionSystem == HoverDetectionSystem.Triggers)
                {
                    for (int tn = 0; tn < slave.pheasy.triggerNotifiers.Length; tn++)
                    {
                        slave.pheasy.triggerNotifiers[tn].onRbEnter.AddListener(col => OnRbEnter(col, bone));
                        slave.pheasy.triggerNotifiers[tn].onRbExit.AddListener(col => OnRbExit(col, bone));
                    }
                }
            }
        }

        public override sealed void ControllerUpdate()
        {
            base.ControllerUpdate();

            if (!gameObject.activeSelf)
                return;

            // Clean destroyed contactables
            model.contacts.RemoveAll(c => c.contactable == null);

            if (model.hoverDetectionSystem == HoverDetectionSystem.OverlapSphereFromRoot)
            {
                OverlapSphere(model.part.root, model.sphereCastRadius, foundRbs);

                model.contacts.FindAll(c => c.bonesEntered.Contains(model.part.root), previousContacts);
                previousContacts.ConvertAll(c => c.contactable.pheasy.rb, previouslyEntered);
                foundRbs.Except(previouslyEntered, enteredRbs);
                previouslyEntered.Except(foundRbs, exitedRbs);

                for (int i = 0; i < enteredRbs.Count(); i++)
                {
                    OnRbEnter(enteredRbs.ElementAt(i), model.part.root);
                }

                for (int i = 0; i < exitedRbs.Count(); i++)
                {
                    OnRbExit(exitedRbs.ElementAt(i), model.part.root);
                }
            } 

            // For each contact
            for (int c = 0; c < model.contacts.Count; c++)
            {
                if (model.contacts[c] != null) UpdateContact(model.contacts[c]);
            }

            // Update totalX asn isX (from list as list might have changed after update and its elements won't be modified again this frame)
            int totalEntered = 0;
            int totalTouched = 0;
            int totalGrasped = 0;
            for (int c = 0; c < model.contacts.Count; c++)
            {
                switch (model.contacts[c].type)
                {
                    case ContactType.Entered:
                        totalEntered++;
                        break;
                    case ContactType.Touched:
                        totalEntered++;
                        totalTouched++;
                        break;
                    case ContactType.Grasped:
                        totalEntered++;
                        totalTouched++;
                        totalGrasped++;
                        break;
                }
            }

            bool previousEntered = model.isEntered;
            bool previousTouched = model.isTouched;
            bool previousGrasped = model.isGrasped;

            model.isEntered = model.isTouched = model.isGrasped = false;

            for (int c = 0; c < model.contacts.Count; c++)
            {
                if (model.contacts[c].type >= ContactType.Entered && !model.isEntered) model.isEntered = true;
                if (model.contacts[c].type >= ContactType.Touched && !model.isTouched) model.isTouched = true;
                if (model.contacts[c].type >= ContactType.Grasped && !model.isGrasped) model.isGrasped = true;
            }

            // Invoke first/last action events
            InvokeDetectorEvents(previousEntered, previousTouched, previousGrasped);

            // Remove obsolete contacts in this frame
            model.contacts.RemoveAll(c => c.type == ContactType.None);
        }

        void UpdateContact(Contact contact)
        {
            // Update time-related variables
            contact.time += Time.deltaTime;

            switch (contact.type)
            {
                case ContactType.Entered:
                    contact.enteredTime += Time.deltaTime;
                    contact.touchedTime = 0.0f;
                    contact.graspedTime = 0.0f;
                    break;
                case ContactType.Touched:
                    contact.enteredTime += Time.deltaTime;
                    contact.touchedTime += Time.deltaTime;
                    contact.graspedTime = 0.0f;
                    break;
                case ContactType.Grasped:
                    contact.enteredTime += Time.deltaTime;
                    contact.touchedTime += Time.deltaTime;
                    contact.graspedTime += Time.deltaTime;
                    break;
                case ContactType.None:
                    break;
            }

            // Update count of special bones touching
            int specialPartsTouchingCount = 0;

            BoneModel bone;
            BoneCollisionModel boneCollision;
            for (int b = 0; b < contact.bonesTouching.Count; b++)
            {
                boneCollision = contact.bonesTouching[b];
                bone = boneCollision.bone;

                if (bone != null && bone.part != null && isSpeciallyInvolvedInGrasping(bone.part))
                    specialPartsTouchingCount++;

                // Debug
                if (model.drawContacts)
                {
                    for (int p = 0; p < boneCollision.points.Count; p++)
                    {
                        Debug.DrawLine(bone.reprs[PuppetModel.key].transformRef.position, boneCollision.points[p].point, Color.yellow);
                    }
                }
            }

            contact.specialPartsTouchingCount = specialPartsTouchingCount;

            // Update type
            ContactType previousType = contact.type;

            if (isGrasping(contact))
                contact.type = ContactType.Grasped;
            else if (contact.bonesTouching.Count > 0)
                contact.type = ContactType.Touched;
            else if (contact.bonesEntered.Count > 0)
                contact.type = ContactType.Entered;
            else
                contact.type = ContactType.None;

            // Invoke action events
            InvokeContactEvents(contact, previousType);
        }

        bool isSpeciallyInvolvedInGrasping(PartModel part)
        {
            HandModel hand;
            FingerModel finger;

            if (part is FingerModel)
            {
                finger = part as FingerModel;

                if (finger.parent is HandModel)
                {
                    hand = finger.parent as HandModel;

                    // Thumb or index
                    if (finger == hand.thumb || finger == hand.index)
                        return true;
                }
            }
            else if (part is HandModel)
            {
                // Wrist
                return true;
            }

            return false;
        }

        bool isGrasping(Contact contact)
        {
            AvatarController avatarController = model.part.body.avatar.controller;

            int totalPartsTouching = contact.bonesTouching.ConvertAll(bc => bc.bone.part).Distinct().Count();

            if (totalPartsTouching >= 2 &&
                contact.specialPartsTouchingCount > 1 &&
                contact.specialPartsTouchingCount != totalPartsTouching)
                return true;
            else
                return false;
        }

        void OnRbEnter(Rigidbody enteredRb, BoneModel bone)
        {
            ContactableView contactable = enteredRb.GetComponent<ContactableView>();

            if (contactable)
            {
                Contact contact = model.contacts.Find(c => c.contactable == contactable);

                if (contact != null)
                {
                    // Existing contact
                    if (!contact.bonesEntered.Contains(bone)) contact.bonesEntered.Add(bone);
                }
                else
                {
                    // New valid contact
                    contact = AddContact(contactable);
                    contact.type = ContactType.None;
                    contact.bonesEntered.Add(bone);
                }
            }
        }

        void OnRbExit(Rigidbody exitedRb, BoneModel bone)
        {
            ContactableView contactable = exitedRb.GetComponent<ContactableView>();

            if (contactable)
            {
                Contact contact = model.contacts.Find(c => c.contactable == contactable);

                if (contact != null)
                {
                    // Existing contact
                    if (contact.bonesEntered.Contains(bone))
                    {
                        bool stillEntered = false;

                        /*
                        PuppetReprModel slave = bone.reprs.FirstOrDefault(r => r.Value is PuppetReprModel).Value as PuppetReprModel;
                        PuppetReprModel slave = BasicHelpers.Get<ReprModel, PuppetReprModel>(bone.reprs);
                        */

                        PuppetReprModel slave = bone.reprs[PuppetModel.key] as PuppetReprModel;

                        // Remove bone only if exitedRb is not in any bone.pheasy.triggerNotifiers
                        for (int tn = 0; tn < slave.pheasy.triggerNotifiers.Length; tn++)
                        {
                            if (slave.pheasy.triggerNotifiers[tn].enteredRbs.Contains(exitedRb))
                            {
                                stillEntered = true;
                                break;
                            }
                        }

                        if (!stillEntered) contact.bonesEntered.Remove(bone);
                    }
                }
            }
        }

        void OnRbTouchStart(Collision collision, BoneModel bone)
        {
            ContactableView contactable = collision.rigidbody.GetComponent<ContactableView>();

            if (contactable)
            {
                Contact contact = model.contacts.Find(c => c.contactable == contactable);

                BoneCollisionModel boneCollision;

                if (contact != null)
                {
                    // Existing contact

                    boneCollision = contact.bonesTouching.Find(bc => bc.bone == bone);

                    if (boneCollision == null)
                    {
                        boneCollision = new BoneCollisionModel(bone);
                        collision.GetContacts(boneCollision.points);
                        contact.bonesTouching.Add(boneCollision);
                    }
                }
                else
                {
                    // New valid contact
                    contact = AddContact(contactable);
                    contact.type = ContactType.None;

                    boneCollision = new BoneCollisionModel(bone);
                    collision.GetContacts(boneCollision.points);
                    contact.bonesTouching.Add(boneCollision);
                }
            }
        }

        void OnRbTouchStay(Collision collision, BoneModel bone)
        {
            stayedContact = model.contacts.Find(c => c.contactable != null && c.contactable.pheasy.rb == collision.rigidbody);

            if (stayedContact == null || stayedContact.contactable == null)
                return;

            stayedContactable = stayedContact.contactable;

            if (stayedContactable)
            {
                Contact contact = model.contacts.Find(c => c.contactable == stayedContactable);

                if (contact != null)
                {
                    BoneCollisionModel boneCollision = contact.bonesTouching.Find(bc => bc.bone == bone);

                    if (boneCollision != null)
                    {
                        collision.GetContacts(boneCollision.points);
                    }
                }
            }
        }

        void OnRbTouchEnd(Collision collision, BoneModel bone)
        {
            ContactableView contactable = collision.rigidbody.GetComponent<ContactableView>();

            if (contactable)
            {
                Contact contact = model.contacts.Find(c => c.contactable == contactable);

                if (contact != null)
                {
                    BoneCollisionModel boneCollision = contact.bonesTouching.Find(bc => bc.bone == bone);

                    if (boneCollision != null)
                    {
                        contact.bonesTouching.Remove(boneCollision);
                    }
                }
            }
        }

        void InvokeContactEvents(Contact contact, ContactType previousType)
        {
            if (previousType == contact.type)
                return;

            if (previousType >= ContactType.Grasped && contact.type < ContactType.Grasped)
            {
                contact.detector.view.onUngrasp.Invoke(contact.view);
                contact.contactable.onUngrasp.Invoke(contact.view);
            }

            if (previousType >= ContactType.Touched && contact.type < ContactType.Touched)
            {
                contact.detector.view.onUntouch.Invoke(contact.view);
                contact.contactable.onUntouch.Invoke(contact.view);
            }

            if (previousType >= ContactType.Entered && contact.type < ContactType.Entered)
            {
                contact.detector.view.onExit.Invoke(contact.view);
                contact.contactable.onExit.Invoke(contact.view);
            }

            if (previousType < ContactType.Entered && contact.type >= ContactType.Entered)
            {
                contact.detector.view.onEnter.Invoke(contact.view);
                contact.contactable.onEnter.Invoke(contact.view);
            }

            if (previousType < ContactType.Touched && contact.type >= ContactType.Touched)
            {
                contact.detector.view.onTouch.Invoke(contact.view);
                contact.contactable.onTouch.Invoke(contact.view);
            }

            if (previousType < ContactType.Grasped && contact.type >= ContactType.Grasped)
            {
                contact.detector.view.onGrasp.Invoke(contact.view);
                contact.contactable.onGrasp.Invoke(contact.view);
            }
        }

        void InvokeDetectorEvents(bool previousEntered, bool previousTouched, bool previousGrasped)
        {
            // Assuming updated contacts and isX values

            if (previousGrasped && !model.isGrasped)
                model.view.onLastUngrasp.Invoke();
            if (previousTouched && !model.isTouched)
                model.view.onLastUntouch.Invoke();
            if (previousEntered && !model.isEntered)
                model.view.onLastExit.Invoke();
            if (!previousEntered && model.isEntered)
                model.view.onFirstEnter.Invoke();
            if (!previousTouched && model.isTouched)
                model.view.onFirstTouch.Invoke();
            if (!previousGrasped && model.isGrasped)
                model.view.onFirstGrasp.Invoke();
        }

        Contact AddContact(ContactableView contactable)
        {
            Contact contact = new Contact();

            contact.detector = model;
            contact.contactable = contactable;

            contact.name = contact.contactable.name;

            model.contacts.Add(contact);
            contactable.AddContact(contact);

            return contact;
        }

        void OverlapSphere(BoneModel bone, float radius, List<Rigidbody> foundRbs)
        {
            foundRbs.Clear();

            if (!bone.reprs.ContainsKey(PuppetModel.key)) return;

            PuppetReprModel slave = bone.reprs[PuppetModel.key] as PuppetReprModel;
            Collider[] colliders = UnityEngine.Physics.OverlapSphere(slave.transformRef.position, radius);

            for (int c = 0; c < colliders.Length; c++)
            {
                if (!colliders[c].attachedRigidbody)
                    continue;

                foundRbs.Add(colliders[c].attachedRigidbody);
            }
        }
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (model.drawCollidedBones)
            {
                for (int c = 0; c < model.contacts.Count; c++)
                {
                    for (int bc = 0; bc < model.contacts[c].bonesTouching.Count; bc++)
                    {
                        BoneCollisionModel boneCollision = model.contacts[c].bonesTouching[bc];

                        Gizmos.color = Color.yellow;
                        Gizmos.DrawSphere(boneCollision.bone.reprs[PuppetModel.key].transformRef.position, 0.01f);
                    }
                }
            }
        }
#endif
    }
}
