using HPTK.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HPTK.Components
{
    public class Respawner : MonoBehaviour
    {
        static List<Rigidbody> respawning = new List<Rigidbody>();

        public Transform onlyChildrenOf;

        private void OnCollisionEnter(Collision collision)
        {
            if (onlyChildrenOf && collision.rigidbody.transform.IsChildOf(onlyChildrenOf))
            {
                if (!respawning.Contains(collision.rigidbody))
                {
                    RigidbodyGroup respawnable = collision.rigidbody.GetComponent<RigidbodyGroup>();
                    if (respawnable)
                    {
                        respawning.Add(collision.rigidbody);

                        Respawn(collision.rigidbody, respawnable);
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (onlyChildrenOf && other.transform.IsChildOf(onlyChildrenOf))
            {
                if (!other.isTrigger && other.attachedRigidbody && !respawning.Contains(other.attachedRigidbody))
                {
                    RigidbodyGroup respawnable = other.attachedRigidbody.GetComponent<RigidbodyGroup>();
                    if (respawnable)
                    {
                        Respawn(other.attachedRigidbody, respawnable);
                    }
                }
            }
        }

        public void Respawn(Rigidbody rb, RigidbodyGroup rbGorup)
        {
            if (rbGorup == null)
                return;

            respawning.Add(rb);

            rb.detectCollisions = false;

            Transform[] parents = new Transform[rbGorup.rigidbodies.Length];
            for (int i = 0; i < rbGorup.rigidbodies.Length; i++)
            {
                rbGorup.rigidbodies[i].detectCollisions = false;
                parents[i] = rbGorup.rigidbodies[i].transform.parent;
                rbGorup.rigidbodies[i].transform.parent = rbGorup.transform;
            }

            rb.transform.position = rbGorup.initialPosition;
            rb.transform.rotation = rbGorup.initialRotation;

            StartCoroutine(PhysHelpers.DoAfterFixedUpdate(() =>
            {
                for (int i = 0; i < rbGorup.rigidbodies.Length; i++)
                {
                    rbGorup.rigidbodies[i].transform.parent = parents[i];
                    rbGorup.rigidbodies[i].detectCollisions = true;
                    rbGorup.rigidbodies[i].velocity = Vector3.zero;
                    rbGorup.rigidbodies[i].angularVelocity = Vector3.zero;
                }

                int index = respawning.IndexOf(rb);
                respawning[index].detectCollisions = true;
                respawning[index].velocity = Vector3.zero;
                respawning[index].angularVelocity = Vector3.zero;

                respawning.RemoveAt(index);
            }));
        }
    }
}
