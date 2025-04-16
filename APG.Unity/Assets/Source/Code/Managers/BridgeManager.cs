using System;
using System.Collections;
using UnityEngine;

public class BridgeManager : MonoBehaviour
{
    [SerializeField] private GameObject bridge;
    [SerializeField] private float loweringSpeed;

    private bool _isLowered;

    private void Start()
    {
        _isLowered = false;
    }

    public void LowerBridge()
    {
        if(_isLowered)
            return;

        _isLowered = true;

        StopAllCoroutines();
        StartCoroutine(LowerBridgeAnimation());
    }

    private IEnumerator LowerBridgeAnimation()
    {
        do
        {
            yield return new WaitForEndOfFrame();
            bridge.transform.localEulerAngles = new Vector3(0, 0,
                bridge.transform.localEulerAngles.z - loweringSpeed * Time.deltaTime);
        } while (bridge.transform.localEulerAngles.z > 270f);
    }
}
