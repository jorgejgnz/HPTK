using HandPhysicsToolkit.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Helpers
{
    public static class InputHelpers
    {
        public static void RecordBone(AbstractTsf[][] boneRecords, AbstractTsf raw, int bone)
        {
            AbstractTsf[] updatedRecords = new AbstractTsf[boneRecords[bone].Length];
            Array.Copy(boneRecords[bone], 1, updatedRecords, 0, updatedRecords.Length - 1);
            updatedRecords[updatedRecords.Length - 1] = new AbstractTsf(raw);

            boneRecords[bone] = updatedRecords;
        }

        public static AbstractTsf SimpleMovingAverage(AbstractTsf[] window, bool averagePosition, bool averageRotation)
        {
            // Result is a copy of the last element to preserve its name and space
            AbstractTsf result = new AbstractTsf(window[window.Length - 1]);

            float weight = 1.0f / window.Length;

            if (averagePosition)
            {
                result.position = Vector3.zero;
                for (int i = 0; i < window.Length; i++)
                {
                    result.position += window[i].position * weight;
                }
            }

            if (averageRotation)
            {
                Vector3 averageForward = Vector3.zero;
                Vector3 averageUpwards = Vector3.zero;

                for (int i = 0; i < window.Length; i++)
                {
                    averageForward += (window[i].rotation * Vector3.forward) * weight;
                    averageUpwards += (window[i].rotation * Vector3.up) * weight;
                }

                result.rotation = Quaternion.LookRotation(averageForward, averageUpwards);
            }

            return result;
        }

        public static AbstractTsf WeightedMovingAverage(AbstractTsf[] window, float[] weights, bool averagePosition, bool averageRotation)
        {
            if (window.Length != weights.Length)
            {
                Debug.LogError("Window and weight arrays are required to have the same length!");
                return window[window.Length - 1];
            }

            // Result is a copy of the last element to preserve its name and space
            AbstractTsf result = new AbstractTsf(window[window.Length - 1]);

            if (averagePosition)
            {
                result.position = Vector3.zero;
                for (int i = 0; i < window.Length; i++)
                {
                    result.position += window[i].position * weights[i];
                }
            }

            if (averageRotation)
            {
                Vector3 averageForward = Vector3.zero;
                Vector3 averageUpwards = Vector3.zero;

                for (int i = 0; i < window.Length; i++)
                {
                    averageForward += (window[i].rotation * Vector3.forward) * weights[i];
                    averageUpwards += (window[i].rotation * Vector3.up) * weights[i];
                }

                result.rotation = Quaternion.LookRotation(averageForward, averageUpwards);
            }

            return result;
        }

        public static float[] GetLinearWeights(int windowLength)
        {
            float[] result = new float[windowLength];

            result[0] = 1.0f;
            float sum = 1.0f;
            for (int i = 1; i < windowLength; i++)
            {
                result[i] = 1 + result[i - 1];  // ... -> result = {1, 2, 3, 4, 5}
                sum += result[i];               // 1 -> 1+2=3 -> 3+3=6 -> 6+4=10 -> 10+5=15
            }

            for (int i = 0; i < windowLength; i++)
            {
                result[i] = result[i] / sum;
            }

            return result;
        }
    }
}
