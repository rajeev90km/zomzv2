using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flammable : MonoBehaviour {

    public Transform FireSpawnTransform;

    public GameObject FireVFXPrefab;

    GameObject fireFx;

    public bool _isLit = false;

    private float _litTime = 100f;

    public void OnCombustion()
    {
        if(FireVFXPrefab!=null)
        {
            if(FireSpawnTransform!=null)
            {
                _isLit = true;

                if (fireFx != null)
                    Destroy(fireFx);

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
