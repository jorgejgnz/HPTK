using HPTK.Models.Avatar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneLineDrawer : MonoBehaviour
{
    public HandModel hand;
    public Color color;

    private void OnDrawGizmos()
    {
        if (hand != null)
        {
            Gizmos.color = color;
            for (int f = 0; f < hand.fingers.Length; f++)
            {
                for (int b = 0; b < hand.fingers[f].bones.Length; b++)
                {
                    if (b == 0)
                    {
                        Gizmos.DrawLine(hand.wrist.transformRef.position, hand.fingers[f].bones[b].transformRef.position);
                    }
                    else if (b == hand.fingers[f].bones.Length - 1)
                    {
                        Gizmos.DrawLine(hand.fingers[f].bones[b].transformRef.position, hand.fingers[f].fingerTip.position);
                    }
                    else
                    {
                        Gizmos.DrawLine(hand.fingers[f].bones[b-1].transformRef.position, hand.fingers[f].bones[b].transformRef.position);
                        Gizmos.DrawLine(hand.fingers[f].bones[b].transformRef.position, hand.fingers[f].bones[b+1].transformRef.position);
                    }
                }
            }
        }
    }
}
