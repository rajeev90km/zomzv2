using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour {

    [SerializeField]
    private List<Cage> _cages;

    [SerializeField]
    private int _maxZombiesInScene = 10;

    [SerializeField]
    private float _timeBetweenSpawns = 5f;

    public int _numZombiesInScene = 0;

    public GameObject _regularZombie;
    public GameObject _fastZombie;
    public GameObject _strongZombie;
    public GameObject _exploderZombie;
    public GameObject _vomitZombie;
    public GameObject _screamerZombie;

    Coroutine _cageOpenCoroutine;

    private Dictionary<ZombieTypes, GameObject> _zombieMapping = new Dictionary<ZombieTypes, GameObject>();

	private void Start()
	{
        _numZombiesInScene = 0;

        _zombieMapping.Add(ZombieTypes.REGULAR, _regularZombie);
        _zombieMapping.Add(ZombieTypes.FAST, _fastZombie);
        _zombieMapping.Add(ZombieTypes.STRONG, _strongZombie);
        _zombieMapping.Add(ZombieTypes.EXPLODER, _exploderZombie);
        _zombieMapping.Add(ZombieTypes.VOMIT, _vomitZombie);
        _zombieMapping.Add(ZombieTypes.SCREAMER, _screamerZombie);

        StartCoroutine(InitSpawnZombies());


	}

    IEnumerator InitSpawnZombies()
    {
        //For each cage
        for (int i = 0; i < _cages.Count;i++)
        {
            //check all configs
            for (int j = 0; j < _cages[i].ZombieConfig.Count;j++)
            {
                //for all minmax zombies
                if (_cages[i].ZombieConfig[j].MaxZombies > 0)
                {
                    if (_numZombiesInScene < _maxZombiesInScene)
                    {
                        GameObject zom = Instantiate(_zombieMapping[_cages[i].ZombieConfig[j].ZombieType],_cages[i].transform);
                        zom.transform.localPosition = Vector3.zero;
                        zom.transform.parent = null;

                        ZombieBase zombieBase = zom.GetComponent<ZombieBase>();
                        zombieBase.WayPointsString = _cages[i].ZombieConfig[j].WayPointString;
                        zombieBase.zombieType = _cages[i].ZombieConfig[j].ZombieType;

                        _cages[i].CageZombies.Add(zombieBase);
                        _numZombiesInScene++;
                    }
                }
            }
        }

        //for (int i = 0; i < _cages.Count; i++)
            //_cages[i].StartCoroutine(_cages[i].OpenCloseDoor());
        
        //yield return new WaitForSeconds(_timeBetweenSpawns);

        //for each cage, for each non restrictive zombie type, spawn 1 each till max zombies per scene are reached
        while(_numZombiesInScene < _maxZombiesInScene)
        {
            //for each cage
            for (int i = 0; i < _cages.Count; i++)
            {
                //check all configs
                for (int j = 0; j < _cages[i].ZombieConfig.Count; j++)
                {
                    //non-restrictive zombie types
                    if (_cages[i].ZombieConfig[j].MaxZombies < 0 && _numZombiesInScene < _maxZombiesInScene)
                    { 
                        GameObject zom = Instantiate(_zombieMapping[_cages[i].ZombieConfig[j].ZombieType]);
                        zom.transform.parent = _cages[i].transform;
                        zom.transform.localPosition = Vector3.zero;
                        zom.transform.parent = null;

                        ZombieBase zombieBase = zom.GetComponent<ZombieBase>();
                        zombieBase.WayPointsString = _cages[i].ZombieConfig[j].WayPointString;
                        zombieBase.zombieType = _cages[i].ZombieConfig[j].ZombieType;

                        _cages[i].CageZombies.Add(zombieBase);
                        _numZombiesInScene++;
                    }
                }
            } 
        }


        for (int i = 0; i < _cages.Count;i++)
            _cages[i].StartCoroutine(_cages[i].OpenCloseDoor());

        yield return null;
    }

    public void OnZombieDie()
    {
        Debug.Log("DEAD ZOMBIE");
        _numZombiesInScene--;
    }


    private void Update()
	{
        //For each cage
        //Check for each zombie type that has cage max limit
        //Check each cage list to see if that many are there
        //if not spawn that many and add to cage
        for (int i = 0; i < _cages.Count; i++)
        {
            for (int j = 0; j < _cages[i].ZombieConfig.Count; j++)
            { 
                if (_cages[i].ZombieConfig[j].MaxZombies > 0)
                {
                    int zCount = 0;

                    for (int k = 0; k < _cages[i].CageZombies.Count;k++)
                    {
                        if (_cages[i].ZombieConfig[j].ZombieType == _cages[i].CageZombies[k].zombieType)
                            zCount++;    
                    }

                    //Debug.Log(zCount);

                    if(zCount < _cages[i].ZombieConfig[j].MaxZombies && _numZombiesInScene < _maxZombiesInScene)
                    {
                        GameObject zom = Instantiate(_zombieMapping[_cages[i].ZombieConfig[j].ZombieType], _cages[i].transform);
                        zom.transform.localPosition = Vector3.zero;
                        zom.transform.parent = null;

                        ZombieBase zombieBase = zom.GetComponent<ZombieBase>();
                        zombieBase.WayPointsString = _cages[i].ZombieConfig[j].WayPointString;
                        zombieBase.zombieType = _cages[i].ZombieConfig[j].ZombieType;

                        _cages[i].CageZombies.Add(zombieBase);
                        _numZombiesInScene++;

                        _cages[i].StartCoroutine(_cages[i].OpenCloseDoor());
                    }
                        
                }
            }
        }
	}

}
