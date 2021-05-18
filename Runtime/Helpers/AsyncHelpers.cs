using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Helpers
{
    public static class AsyncHelpers
    {
        public static void DoAfterFixedUpdate(MonoBehaviour host, Action toDo)
        {
            host.StartCoroutine(AfterFixedUpdate(toDo));
        }

        public static void DoAfterUpdate(MonoBehaviour host, Action toDo)
        {
            host.StartCoroutine(AfterUpdate(toDo));
        }

        public static void DoAfterFrames(MonoBehaviour host, int frames, Action toDo)
        {
            host.StartCoroutine(AfterFrames(frames, toDo));
        }

        public static void DoAfterTime(MonoBehaviour host, float seconds, Action toDo)
        {
            host.StartCoroutine(After(seconds, toDo));
        }

        static IEnumerator AfterFixedUpdate(Action toDo)
        {
            yield return new WaitForFixedUpdate();

            toDo.Invoke();
        }

        static IEnumerator AfterUpdate(Action toDo)
        {
            yield return new WaitForEndOfFrame();

            toDo.Invoke();
        }

        static IEnumerator AfterFrames(int frames, Action toDo)
        {
            for (int f = 0; f < frames; f++)
            {
                yield return new WaitForEndOfFrame();
            }

            toDo.Invoke();
        }

        static IEnumerator After(float seconds, Action toDo)
        {
            yield return new WaitForSeconds(seconds);

            toDo.Invoke();
        }
    }
}
