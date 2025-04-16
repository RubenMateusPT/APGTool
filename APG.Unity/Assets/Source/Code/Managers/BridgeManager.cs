using System;
using System.Collections;
using System.Threading.Tasks;
using APG.Unity.Managers;
using UnityEngine;
using Random = UnityEngine.Random;

public class BridgeManager : MonoBehaviour
{
    [SerializeField] private GameObject bridge;
    [SerializeField] private float loweringSpeed;

    private bool _isLowered;
    private float _currentRotation;

    private int _bridgeCode;

    private void Start()
    {
        _isLowered = false;
        _currentRotation = 90;
    }

    public void RequestBridgeCode()
    {
        _bridgeCode = Random.Range(100, 200);
        //APG Request
    }

    public void LowerBridgeWithCode(int code)
    {
        if (_bridgeCode == code)
            LowerBridge();
    }

    public void StopBridge()
    {
        StopAllCoroutines();
    }

    public void RaiseBridge()
    {
        if (!_isLowered)
            return;

        _isLowered = false;

        StopAllCoroutines();
        StartCoroutine(RaiseBridgeAnimation());
    }

    private IEnumerator RaiseBridgeAnimation()
    {
        do
        {
            yield return new WaitForEndOfFrame();
            _currentRotation += loweringSpeed * Time.deltaTime;
            bridge.transform.localEulerAngles = new Vector3(0, 0,
                bridge.transform.localEulerAngles.z + loweringSpeed * Time.deltaTime);
        } while (_currentRotation < 90);
        _currentRotation = 90;
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
            _currentRotation -= loweringSpeed * Time.deltaTime;
            bridge.transform.localEulerAngles = new Vector3(0, 0,
                bridge.transform.localEulerAngles.z - loweringSpeed * Time.deltaTime);
        } while (_currentRotation > 0);
        _currentRotation = 0;
    }
}
