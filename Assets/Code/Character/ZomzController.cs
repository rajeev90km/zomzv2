using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public enum ZomzManaConsumeType
{
    TIME_BOUND = 0,
    ACTION_BASED = 1
}

public class ZomzController : MonoBehaviour {

    public ZomzData ZomzMode;

    public GameData gameData;

    private int _enemyLayerMask;
    private CharacterControls _characterControls;
    private Animator _animator;

    private const float ZOMZ_COOLDOWN_TIME = 10f;

    private bool _canUseZomzMode = true;
    public bool CanUseZomzMode
    {
        get { return _canUseZomzMode; }
        set { _canUseZomzMode = value; }
    }

    [SerializeField]
    private GameFloatAttribute _zomzManaAttribute;

    [Header("Miscellaneous")]
    [SerializeField]
    private float ZomzUseTime = 10f;

    [SerializeField]
    private GameObject _arrowPrefab;

    [Header("Events")]
    [SerializeField]
    private GameEvent _zomzStartEvent;

    [SerializeField]
    private GameEvent _zomzEndEvent;

    [SerializeField]
    private GameEvent _zomzRegisterEvent;

    [SerializeField]
    private GameEvent _zomzUnregisterEvent;

    private List<ZombieBase> _zombiesUnderControl;
    private GameObject _pointerArrowObj;
    private Coroutine _zomzManaUseCoroutine;

    LevelControllerBase currentLevelController;

    PostProcessVolume _postProcessVolume;

    ColorGrading colorGrading;
    ChromaticAberration chromaticAberration;

    [SerializeField]
    GameObject ZomzCameraObj;

    [SerializeField]
    GameObject _zomzManaObj;

    [SerializeField]
    GameObject _step1Obj;

    [SerializeField]
    GameObject _pressEscObj;

    Camera ZomzCamera;

    float normalSaturation = 20;
    float zomzModeSaturation = -60;
    float normalChromaticAberration = 0;
    float zomzModeChromaticAberration = 0.7f;

	void Awake () 
    {
        //Cache Properties
        _animator = GetComponent<Animator>();
        _characterControls = GetComponent<CharacterControls>();

        _pointerArrowObj = Instantiate(_arrowPrefab) as GameObject;
        _pointerArrowObj.SetActive(false);

        _zombiesUnderControl = new List<ZombieBase>();
        _enemyLayerMask = (1 << LayerMask.NameToLayer("Enemy"));

        currentLevelController = GameObject.FindWithTag("LevelController").GetComponent<LevelControllerBase>();

        GameObject postProcessObj = GameObject.FindWithTag("PostProcessing");
        if (postProcessObj)
        {
            _postProcessVolume = postProcessObj.GetComponent<PostProcessVolume>();
            _postProcessVolume.profile.TryGetSettings<ColorGrading>(out colorGrading);
            _postProcessVolume.profile.TryGetSettings<ChromaticAberration>(out chromaticAberration);
        }

        ZomzCamera = ZomzCameraObj.GetComponent<Camera>();
	}

    void ZomzModePostProcess()
    {
        ZomzCameraObj.SetActive(true);
        colorGrading.saturation.value = zomzModeSaturation;
        chromaticAberration.intensity.value = zomzModeChromaticAberration;

        _pressEscObj.SetActive(true);
    }

    void ResetZomzModePostProcess()
    {
        ZomzCameraObj.SetActive(false);
        colorGrading.saturation.value = normalSaturation;
        chromaticAberration.intensity.value = normalChromaticAberration;

        if (_step1Obj.activeSelf)
            _step1Obj.SetActive(false);

        _pressEscObj.SetActive(false);
    }

    public void InitZomzMode()
    {
        gameData.CurrentLevelData.CanUseZomzMode = true;
        gameData.CurrentLevelData.CanScreenGlitch = false;
        gameData.CurrentLevelData.IsInjured = false;

        _zomzManaObj.SetActive(true);
        _step1Obj.SetActive(true);

        ProcessZomzMode();
    }

	public void ProcessZomzMode()
    {
        if (!ZomzMode.CurrentValue)
        {
            _zombiesUnderControl.Clear();
            ZomzMode.CurrentValue = true;

            if (ZomzMode.CurrentValue)
            {
                ZomzModePostProcess();

                _zomzStartEvent.Raise();
                _zomzManaAttribute.ResetAttribute();

                _animator.SetFloat("speedPercent", 0.0f);

                Collider[] _zombiesHit = Physics.OverlapSphere(transform.position, _characterControls.CharacterStats.ZomzRange, _enemyLayerMask);

                for (int i = 0; i < _zombiesHit.Length; i++)
                {
                    ZombieBase zombie = _zombiesHit[i].GetComponent<ZombieBase>();
                    _zombiesUnderControl.Add(zombie);

                    if (zombie != null && zombie.IsAlive)
                    {
                        zombie.OnZomzModeAffected();
                    }
                }
            }
        }
        else
        {
            Debug.Log("Disable");
            ZomzCameraObj.SetActive(false);
        }
    }

    public void RegisterZomzMode()
    {
        if (ZomzMode.CurrentSelectedZombie)
        {
            ZomzMode.IsRegistered = true;
            ZomzMode.CurrentSelectedZombie.OnZomzModeRegister();
            _zomzRegisterEvent.Raise();


            if (ZomzMode.ManaConsumeType == ZomzManaConsumeType.TIME_BOUND)
                _zomzManaUseCoroutine = StartCoroutine(UseTimeBasedZomzMana());
        }
        _zomzEndEvent.Raise();
        ZomzMode.CurrentValue = false;
        _pointerArrowObj.SetActive(false);

        ResetZomzModePostProcess();
    }

    public void UnregisterZomzMode()
    {
        if(ZomzMode.CurrentSelectedZombie)
            ZomzMode.CurrentSelectedZombie.OnZomzModeUnRegister();
        _zomzUnregisterEvent.Raise();
        ZomzMode.CurrentSelectedZombie = null;
        ZomzMode.IsRegistered = false;
        _characterControls.ResetDirectionVectors();
        if(_zomzManaUseCoroutine!=null)
            StopCoroutine(_zomzManaUseCoroutine);
        _zomzManaUseCoroutine = null;

        ResetZomzModePostProcess();

        StartCoroutine(ZomzCoolDown());
    }

    public void EndZomzMode()
    {
        ResetZomzModePostProcess();

        _pointerArrowObj.SetActive(false);
        ZomzMode.CurrentSelectedZombie = null;
        ZomzMode.CurrentValue = false;
        _zomzEndEvent.Raise();    
    }

    IEnumerator UseTimeBasedZomzMana()
    {
        float time = 0f;

        while (time < 1)
        {
            _zomzManaAttribute.CurrentValue = Mathf.Lerp(100, 0, time);     
            time += Time.deltaTime / ZomzUseTime;
            yield return null;
        }

        _zomzManaAttribute.CurrentValue = 0;

        yield return null;
    }

    IEnumerator ZomzCoolDown()
    {
        float time = 0f;

        float curVal = _zomzManaAttribute.CurrentValue;
        float coolDownTime = ZOMZ_COOLDOWN_TIME - ((curVal / 100) * ZOMZ_COOLDOWN_TIME);

        while (time < 1)
        {
            _zomzManaAttribute.CurrentValue = Mathf.Lerp(curVal, 100, time);
            time += Time.deltaTime / coolDownTime;
            yield return null;
        }

        _zomzManaAttribute.CurrentValue = 100;
        _canUseZomzMode = true;

        yield return null;
    }

	void Update () 
    {
        if (!gameData.IsPaused && !currentLevelController.IsConversationInProgress)
        {
            if (Camera.main)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (ZomzMode.CurrentValue)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity, _enemyLayerMask))
                        {
                            if (hit.transform != null)
                            {
                                ZombieBase zBase = hit.transform.gameObject.GetComponent<ZombieBase>();

                                if (zBase != null && zBase.IsAlive)
                                {
                                    _pointerArrowObj.SetActive(true);
                                    ZomzMode.CurrentSelectedZombie = hit.transform.gameObject.GetComponent<ZombieBase>();

                                    Vector3 zombiePos = hit.transform.gameObject.transform.position;
                                    _pointerArrowObj.transform.position = new Vector3(zombiePos.x, zombiePos.y + 3, zombiePos.z);
                                }
                            }
                        }
                    }
                }

                //Request Zomz Mode
                if (Input.GetKeyDown(KeyCode.Z) && _zomzManaAttribute.CurrentValue >= 100 && _characterControls.IsAlive)
                {
                    if (gameData.CurrentLevelData.CanUseZomzMode)
                    {
                        if (!ZomzMode.IsRegistered)
                        {
                            if (!ZomzMode.CurrentValue)
                                ProcessZomzMode();
                            else
                            {
                                RegisterZomzMode();
                            }
                        }
                    }
                }

                //End Zomz Mode if ESC or if out of mana
                if (Input.GetKeyDown(KeyCode.Escape) || _zomzManaAttribute.CurrentValue <= 0 && _characterControls.IsAlive)
                {
                    if (gameData.CurrentLevelData.CanUseZomzMode)
                    {
                        if (ZomzMode.CurrentSelectedZombie)
                            UnregisterZomzMode();

                        EndZomzMode();
                    }

                }
            }
        }
	}
}
