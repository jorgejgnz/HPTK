using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HandPhysicsToolkit.Physics
{
    [RequireComponent(typeof(Pheasy))]
    public class Respawnable : MonoBehaviour
    {
        Pheasy pheasy;

        Vector3 initialWorldPos;
        Quaternion initialWorldRot;

        [Header("Control")]
        public float respawnUnderHeigth = 0.1f;

        private void Start()
        {
            pheasy = GetComponent<Pheasy>();

            initialWorldPos = transform.position;
            initialWorldRot = transform.rotation;
        }

        private void Update()
        {
            if (transform.position.y <= respawnUnderHeigth)
            {
                transform.position = initialWorldPos;
                transform.rotation = initialWorldRot;

                pheasy.rb.velocity = Vector3.zero;
            }
        }
    }
}
