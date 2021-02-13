using HPTK.Components;
using HPTK.Models.Avatar;
using HPTK.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HPTK.Helpers
{
    public static class AsyncHelpers
    {
        public static IEnumerator DoAfterFixedUpdate(Action toDo)
        {
            yield return new WaitForFixedUpdate();

            toDo.Invoke();
        }

        public static IEnumerator DoAfterUpdate(Action toDo)
        {
            yield return new WaitForEndOfFrame();

            toDo.Invoke();
        }

        public static IEnumerator DoAfter(float seconds, Action toDo)
        {
            yield return new WaitForSeconds(seconds);

            toDo.Invoke();
        }
    }
}
