using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class TalkativeBall : MonoBehaviour
{
    TextMeshPro _tmpro;
    public TextMeshPro tmpro
    {
        get
        {
            if (!_tmpro) _tmpro = GetComponent<TextMeshPro>();
            return _tmpro;
        }
    }

    public List<string> enterMessage = new List<string>();
    public List<string> touchMessage = new List<string>();
    public List<string> grabMessage = new List<string>();

    public void SayEntered()
    {
        tmpro.text = enterMessage[Random.Range(0, enterMessage.Count - 1)];
    }

    public void SayTouched()
    {
        tmpro.text = touchMessage[Random.Range(0, touchMessage.Count - 1)];
    }

    public void SayGrabbed()
    {
        tmpro.text = grabMessage[Random.Range(0, grabMessage.Count - 1)];
    }

    public void Clear()
    {
        tmpro.text = "";
    }
}
