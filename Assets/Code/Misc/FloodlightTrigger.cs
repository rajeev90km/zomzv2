using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloodlightTrigger : MonoBehaviour {

    [SerializeField]
    private Light _floodLight;

    private bool _isLightOn = false;

	public IEnumerator PowerFloodLight()
    {
        if (!_isLightOn)
        {
            _isLightOn = true;
            _floodLight.enabled = true;
        }
        yield return null;
    }
}
