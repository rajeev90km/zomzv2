using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ZombieTypes
{
    REGULAR = 0,
    FAST = 1,
    STRONG = 2,
    EXPLODER = 3,
    VOMIT = 4,
    SCREAMER = 5
}

[System.Serializable]
public class ZombieConfig
{
    public ZombieTypes ZombieType;
    public string WayPointString;
    public float MaxZombies = Mathf.Infinity;
}

public class Cage : MonoBehaviour {

    [SerializeField]
    private GameObject _door;
    public GameObject Door{
        get { return _door; }
    }

    [SerializeField]
    private Transform _zombieSpawnPoint;
    public Transform ZombieSpawnPoint
    {
        get { return _zombieSpawnPoint; }
    }

    [SerializeField]
    private float _doorOpenCloseTime;
    public float DoorOpenCloseTime{
        get { return _doorOpenCloseTime; }
    }

    public List<ZombieConfig> ZombieConfig;

    public List<ZombieBase> CageZombies;

    [SerializeField]
    private float _waitToCloseDoor = 3f;

    [SerializeField]
    private float _doorAngle = -150f;

    private bool _isOpening = false;

	private void Update()
	{
        for (int i = CageZombies.Count - 1; i >= 0; i--)
        {
            if (!CageZombies[i].IsAlive)
                CageZombies.RemoveAt(i);
        }
	}

	public IEnumerator OpenCloseDoor()
    {
        //if (!_isOpening)
        //{
            _isOpening = true;

            float t = 0;

            while (t < 1)
            {
                _door.transform.localEulerAngles = new Vector3(0, Mathf.Lerp(0, _doorAngle, Mathf.SmoothStep(0f, 1f, t)), 0);
                t += Time.deltaTime / _doorOpenCloseTime;
                yield return null;
            }

            yield return new WaitForSeconds(_waitToCloseDoor);

            t = 0;

            while (t < 1)
            {
                _door.transform.localEulerAngles = new Vector3(0, Mathf.Lerp(_doorAngle, 0, Mathf.SmoothStep(0f, 1f, t)), 0);
                t += Time.deltaTime / _doorOpenCloseTime;
                yield return null;
            }

            _isOpening = false;
        //}
        yield return null;
    }

}
