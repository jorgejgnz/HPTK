using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HPTK.Components
{
    public class CollisionIgnorer : HPTKElement
    {
        public Collider[] ignoreBetween;

        private void Start()
        {
            for (int i = 0; i < ignoreBetween.Length; i++)
            {
                for (int j = 0; j < ignoreBetween.Length; j++)
                {
                    Physics.IgnoreCollision(ignoreBetween[i], ignoreBetween[j], true);
                }
            }
        }
    }
}
