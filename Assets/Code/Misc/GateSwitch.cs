using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateSwitch : MonoBehaviour {

    [SerializeField]
    private GameObject _lever;

    [SerializeField]
    private GameObject _connectedDoor;

    [SerializeField]
    private float _leverSwitchTime;

    [SerializeField]
    private float _doorOpenTime;

    [SerializeField]
    private bool _isLastSwitch = false;
    public bool IsLastSwitch
    {
        get { return _isLastSwitch; }
    }

    private bool _isActivated = false;
    public bool IsActivated
    {
        get { return _isActivated; }
    }

    private const float LEVER_Z_ORIENTATION = 60f;

    [SerializeField]
    private float _doorOpenOrientation = 60f;

	public void Activate()
	{
        if(!_isActivated)   
        {
            _isActivated = true;
            StartCoroutine(MoveLeverAndOpenGate());
        }
	}

    IEnumerator MoveLeverAndOpenGate()
    {
        yield return new WaitForSeconds(0.5f);

        float t = 0;

        AkSoundEngine.PostEvent("Gate_Lever", gameObject);

        while (t<1)
        {
            _lever.transform.localEulerAngles = new Vector3 (0, 0, Mathf.Lerp(0, LEVER_Z_ORIENTATION, t));
            t += Time.deltaTime / _leverSwitchTime;
            yield return null;    
        }

        t = 0;

        AkSoundEngine.PostEvent("Gate_Opening", gameObject);

        Vector3 initEulerAngles = _connectedDoor.transform.localEulerAngles;
        Vector3 endEulerAngles = _connectedDoor.transform.localEulerAngles - new Vector3(0, _connectedDoor.transform.localEulerAngles.y + _doorOpenOrientation , 0 );

        while (t < 1)
        {
            _connectedDoor.transform.localEulerAngles = Vector3.Lerp(initEulerAngles, endEulerAngles, t);
            t += Time.deltaTime / _doorOpenTime;
            yield return null;
        }
    }

	void Update () 
    {
		
	}
}
