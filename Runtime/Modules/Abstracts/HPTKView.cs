using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit
{
    [Serializable]
    public class HPTKViewEvent : UnityEvent<HPTKView> { }


    public abstract class HPTKView : HPTKElement
    {
        public virtual void Awake() { }
    }
}
