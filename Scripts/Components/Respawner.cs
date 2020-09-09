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
                    Respawnable respawnable = collision.rigidbody.GetComponent<Respawnable>();
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
                    Respawnable respawnable = other.attachedRigidbody.GetComponent<Respawnable>();
                    if (respawnable)
                    {
                        Respawn(other.attachedRigidbody, respawnable);
                    }
                }
            }
        }

        public void Respawn(Rigidbody rb, Respawnable respawnable)
        {
            if (respawnable == null)
                return;

            respawning.Add(rb);

            rb.detectCollisions = false;

            Transform[] parents = new Transform[respawnable.attachedRbs.Length];
            for (int i = 0; i < respawnable.attachedRbs.Length; i++)
            {
                respawnable.attachedRbs[i].detectCollisions = false;
                parents[i] = respawnable.attachedRbs[i].transform.parent;
                respawnable.attachedRbs[i].transform.parent = respawnable.transform;
            }

            rb.transform.position = respawnable.initialPosition;
            rb.transform.rotation = respawnable.initialRotation;

            StartCoroutine(PhysHelpers.DoAfterFixedUpdate(() =>
            {
                for (int i = 0; i < respawnable.attachedRbs.Length; i++)
                {
                    respawnable.attachedRbs[i].transform.parent = parents[i];
                    respawnable.attachedRbs[i].detectCollisions = true;
                    respawnable.attachedRbs[i].velocity = Vector3.zero;
                    respawnable.attachedRbs[i].angularVelocity = Vector3.zero;
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
