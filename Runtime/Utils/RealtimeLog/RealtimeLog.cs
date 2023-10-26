using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class RealtimeLog : MonoBehaviour
{
    private static RealtimeLog _singleton;
    public static RealtimeLog singleton
    {
        get { if (!_singleton) _singleton = FindObjectOfType<RealtimeLog>(); return _singleton; }
    }

    public TextMeshProUGUI tmpro;

    string frameLog = "<mspace=30.0>";

    private void LateUpdate()
    {
        frameLog += "</mspace>";
        tmpro.text = frameLog;
        frameLog = "<mspace=30.0>";
    }

    public void Write(string text)
    {
        frameLog += text + '\n';
    }
}
