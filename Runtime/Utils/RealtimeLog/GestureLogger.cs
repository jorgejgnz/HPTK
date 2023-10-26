using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Modules.Hand.GestureDetection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureLogger: MonoBehaviour
{
    public string prefix;
    Gesture g;

    Side side = Side.None;
    string sideStr;

    private void Start()
    {
        if (g == null) g = GetComponent<Gesture>();
        if (prefix == "") prefix = g.name;

        GestureDetectionView gestureDetector = GetComponentInParent<GestureDetectionView>();
        if (gestureDetector) side = gestureDetector.hand.side;

        if (side == Side.Left) sideStr = "L";
        else if (side == Side.Right) sideStr = "R";
        else sideStr = "-";
    }

    private void Update()
    {
        if (RealtimeLog.singleton) RealtimeLog.singleton.Write($"{sideStr} [{prefix.PadRight(15)}]\t{Math.Round(g.lerp,2)}");
    }
}
