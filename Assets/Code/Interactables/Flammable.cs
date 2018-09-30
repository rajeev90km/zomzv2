using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Flammable : MonoBehaviour {

    public Transform FireSpawnTransform;

    public GameObject FireVFXPrefab;

    GameObject fireFx;

    public bool _isLit = false;

    [SerializeField]
    private float _litTime = 100f;

    [SerializeField]
    private UnityEvent _onCombustionEvent;

    public void OnCombustion()
    {
        if(FireVFXPrefab!=null)
        {
            if(FireSpawnTransform!=null)
            {
                _isLit = true;

                if (fireFx != null)
                    Destroy(fireFx);

                _onCombustionEvent.Invoke();

                fireFx = Instantiate(FireVFXPrefab);
                fireFx.transform.position = FireSpawnTransform.position;
                fireFx.transform.parent = transform;

                StartCoroutine(StartDieDown());
            }    
        }
    }

    IEnumerator StartDieDown()
    {   
        yield return new WaitForSeconds(_litTime);

        if (fireFx != null)
            Destroy(fireFx);
        
        _isLit = false;
    }
}
