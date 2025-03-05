using System;
using APG.Common;
using UnityEngine;
using Random = UnityEngine.Random;

public class Test : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("Hi");

        int a = Random.Range(1, 11);
        int b = Random.Range(1, 11);
        Debug.Log($"{a} + {b} = {Packet.Sum(a,b)}");
        Debug.Log($"{a} - {b} = {Packet.Minus(a, b)}");
        Debug.Log($"{a} * {b} = {Packet.Times(a, b)}");
        Debug.Log($"{a} + {b} = {Packet.Divide(a, b)}");
        Packet.Hello()
    }
}
