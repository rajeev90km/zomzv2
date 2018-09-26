using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Rendering.PostProcessing;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class CharacterControls : Being
{

    [SerializeField]
    private GameData _gameData;

    [SerializeField]
    private CharacterStats _characterStats;
    public CharacterStats CharacterStats
    {
        get { return _characterStats; }
    }

    [SerializeField]
    private Inventory _inventory;

    [SerializeField]
    private int _attackModifier;
    public int AttackModifier{
        get { return _attackModifier; }
        set { _attackModifier = value; }
    }

    private InventoryItem _currentWeapon;
    public InventoryItem CurrentWeapon {
        get { return _currentWeapon; }
        set { _currentWeapon = value; }
    }

    [SerializeField]
    private Transform _eyes;
    public Transform Eyes
    {
        get { return _eyes; }
    }

    private bool _canPush = false;
    public bool CanPush{
        get { return _canPush; }
        set { _canPush = value; }
    }

    private bool _isPushing = false;
    public bool IsPushing {
        get { return _isPushing; }
        set { _isPushing = value; }
    }

    public bool _canClimb = false;
    private bool _isClimbing = false;
    private Ladder _currentLadder = null;

    private bool _beginPush = false;

    public float _currentHealth;

    private float _speedSmoothTime = 0.1f;
    private float _speedSmoothVelocity;
    private float _currentSpeed;

    [SerializeField]
    private bool _debugMode = false;

    Vector3 forward, right;

    private Animator _animator;
    private ZomzController _zomzControls;

    private bool _isAttacking = false;

    private bool _isHurting = false;
    public bool IsHurting 
    {
        get { return _isHurting; }    
    }

    public bool IsGrounded = false;
    public bool IsFalling = false;

    public float HitDistance = 0.35f;

    public LayerMask GroundedLayerMask;

    private bool _canAttack = true;
    private bool _isDiving = false;

    private Coroutine _attackCoroutine;
    private Coroutine _hurtCoroutine;

    private string[] _attackAnimations = { "attack1", "attack2" };
    private string[] _hurtAnimations = { "hurt1"};
    private string _dodgeAnimation = "roll";

    private int pushableMask;

    private const float PUSH_ANGLE_RANGE = 45f;

    private bool _isCrouching = false;
    public bool IsCrouching{
        get { return _isCrouching; }
        set { _isCrouching = value; }
    }

    [Header("Prompt Settings")]
    [SerializeField]
    private float _normalSaturation = 5f;

    [SerializeField]
    private float _promptSaturation = -50f;

    [SerializeField]
    private float _normalVignette = 0.3f;

    [SerializeField]
    private float _promptVignette = 0.6f;

    [SerializeField]
    private float _promptTransitionTime = 0.75f;

    [SerializeField]
    private GameObject _promptCanvas;

    [SerializeField]
    private Text _promptText;

    [Header("Interactable Canvas Settings")]
    [SerializeField]
    private GameObject _interactBg;

    [SerializeField]
    private GameObject _interactText;

    [SerializeField]
    private GameObject _interactWarningIcon;

    [SerializeField]
    private Text _interactWarningText;

    [SerializeField]
    private float _showWarningTime = 2f;

    [Header("Objectives")]
    [SerializeField]
    private GameObject _objectiveCanvas;

    [SerializeField]
    private Text _objectiveSideText;

    [Header("FX")]
    [SerializeField]
    private GameObject _hurtFX;

    [Header("Warning Texts")]
    [SerializeField]
    private string HEALTH_FULL_WARNING = "Health already full!";

    GameObject currentPushable = null;
    Vector3 pushableOffset;

    private CapsuleCollider _ownCollider;

    private bool climbStarted = false;

    private Rigidbody rigidbody;

    private const float DEFAULT_COLLIDER_HEIGHT = 1.75f;
    private Vector3 DEFAULT_COLLIDER_CENTER = new Vector3(0, 0.85f, 0);
    private const float CROUCH_COLLIDER_HEIGHT = 1.1f;
    private Vector3 CROUCH_COLLIDER_CENTER = new Vector3(0,0.55f,0);

    int humanLayerMask;
    int zombieLayerMask;
    int finalLayerMask;

    Coroutine endClimbCoroutine;

    public float endClimbWait = 3.5f;

    GateSwitch currentGateSwitch;
    InventoryItem currentHealthPack;
    InventoryItem currentWeapon;

    PostProcessVolume _postProcessVolume;
    ColorGrading colorGradingLayer = null;
    Vignette vignetteLayer = null;

    [SerializeField]
    private float _levelInitWaitTime = 5f;


    void Start()
    {
        Application.targetFrameRate = 60;

        _zomzControls = GetComponent<ZomzController>();
        _currentHealth = _characterStats.Health;

        _animator = GetComponent<Animator>();
        _ownCollider = GetComponent<CapsuleCollider>();
        rigidbody = GetComponent<Rigidbody>();

        pushableMask = (1 << LayerMask.NameToLayer("Pushable"));

        //LayerMasks
        humanLayerMask = (1 << LayerMask.NameToLayer("Human"));
        zombieLayerMask = (1 << LayerMask.NameToLayer("Enemy"));
        finalLayerMask = humanLayerMask | zombieLayerMask;

        GameObject postProcessObj = GameObject.FindWithTag("PostProcessing");
        if (postProcessObj)
        {
            _postProcessVolume = postProcessObj.GetComponent<PostProcessVolume>();
            _postProcessVolume.profile.TryGetSettings<ColorGrading>(out colorGradingLayer);
            _postProcessVolume.profile.TryGetSettings<Vignette>(out vignetteLayer);
        }

        ResetDirectionVectors();

        _gameData.IsPaused = true;

        StartCoroutine(RemoveObjectiveCanvasAndPlaceObjectiveOnSide());
    }

    public IEnumerator RemoveObjectiveCanvasAndPlaceObjectiveOnSide()
    {
        yield return new WaitForSeconds(_levelInitWaitTime);
        _objectiveCanvas.SetActive(false);

        _objectiveSideText.gameObject.SetActive(true);

        _gameData.IsPaused = false;
    }

    public void ResetDirectionVectors()
    {
        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);

        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;
    }


    public void BeginAttack()
	{
		if (_isAlive)
		{
			if (_attackCoroutine != null)
			{
				StopCoroutine (_attackCoroutine);
				_attackCoroutine = null;
			}

			_attackCoroutine = StartCoroutine (Attack ());
		}
	}

	public override IEnumerator Hurt(float pDamage = 0.0f)
	{
		if (_isAlive)
        {
            _isCrouching = false;

            _isHurting = true;

            if (_currentHealth - pDamage > 0)
                _currentHealth -= pDamage;
            else
                _currentHealth = 0;

            if (_hurtFX != null)
                Instantiate(_hurtFX, _eyes.transform.position, Quaternion.identity);

            if (_currentHealth > 0 && !_isAttacking)
            {
                _animator.SetTrigger(_hurtAnimations[Random.Range(0, _hurtAnimations.Length)]);
            }
            else if (_currentHealth <= 0)
            {
                Die();
            }

            yield return new WaitForSeconds(1f);
            _isHurting = false;

        }

		yield return null;
	}


    public override IEnumerator Attack()
    {
        _isAttacking = true;
        _canAttack = false;
        _animator.SetTrigger(_attackAnimations[Random.Range(0, _attackAnimations.Length)]);

        Being closestEnemy = GetClosestBeingToAttack(finalLayerMask,_characterStats.AttackRange);

        yield return new WaitForSeconds(_characterStats.AttackRate / 2);

        if (closestEnemy)
        {
            transform.LookAt(closestEnemy.transform);
            if (closestEnemy && closestEnemy.IsAlive)
                StartCoroutine(closestEnemy.Hurt(_characterStats.AttackStrength + _attackModifier));
        }

        yield return new WaitForSeconds(_characterStats.AttackRate/2);
        _canAttack = true;

        AkSoundEngine.PostEvent("abe_attack", gameObject);


        //Update Weapon Durability if Any
        if (_currentWeapon != null)
        {
            if (_currentWeapon.CurrentDurability > 1)
                _currentWeapon.CurrentDurability -= 1;
            else
            {
                _inventory._weapons.Remove(_currentWeapon);
                _currentWeapon = null;
                _attackModifier = 0;
            }
        }


        yield return new WaitForSeconds(0.35f);
        _isAttacking = false;
    }

	public void Die()
	{
		_animator.SetTrigger ("die");
		_isAlive = false;
        _zomzControls.EndZomzMode();
	}

    public GameObject GetClosestPushable()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.5f, pushableMask);

        Collider closestCollider = null;

        foreach (Collider hit in colliders)
        {
            Pushable pushable = hit.gameObject.GetComponent<Pushable>();

            if (pushable == null)
            {
                continue;
            }

            if (!closestCollider)
            {
                closestCollider = hit;
            }
            //compares distances
            if (Vector3.Distance(transform.position, hit.transform.position) <= Vector3.Distance(transform.position, closestCollider.transform.position))
            {
                closestCollider = hit;
            }
        }

        if (!closestCollider)
            return null;

        return closestCollider.gameObject;
    }

    IEnumerator Dive()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position + transform.forward * _characterStats.DiveDistance;

        _animator.SetTrigger("roll");

        float time = 0;

        while(time < 1)
        {
            transform.position = Vector3.Lerp(startPos, endPos, time);
            time = time / _characterStats.DiveTime + Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;

        _isDiving = false;
    }


    void EndCrouch()
    {
        if (_isCrouching)
        {
            _isCrouching = false;
            _animator.SetTrigger("endcrouch");
            _ownCollider.height = DEFAULT_COLLIDER_HEIGHT;
            _ownCollider.center = DEFAULT_COLLIDER_CENTER;
        }    
    }

    IEnumerator PlayFallAnimation()
    {
        yield return new WaitForSeconds(0.2f);
        if(!IsGrounded && !IsFalling)
        {
            IsFalling = true;
            _animator.SetTrigger("fall");
            yield return new WaitForSeconds(1.7f);
            IsFalling = false;
        } 
    }

	private void OnTriggerEnter(Collider other)
	{
        //IF LADDER
        if(other.CompareTag("Ladder") && other.GetComponent<Ladder>())
        {
            if (!IsFalling)
            {
                _currentLadder = other.GetComponent<Ladder>();

                if (transform.localPosition.y <= _currentLadder.LadderHeight / 2)
                {
                    _canClimb = true;
                    //transform.position = _currentLadder.LadderBottom.position;
                    //transform.LookAt(_currentLadder.transform);
                }
                else
                {
                    _canClimb = false;
                    _currentLadder = null;
                }
            }
            //transform.LookAt(other.transform);
        }

        //IF PROMPT ZONE
        if (other.CompareTag("PromptZone"))
        {
            PromptTriggerZone ptz = other.GetComponent<PromptTriggerZone>();
            StartCoroutine(ShowPrompt(ptz));
        }

        //IF SWITCH
        if(other.CompareTag("SwitchPanel"))
        {
            _interactBg.SetActive(true);
            _interactText.SetActive(true);

            currentGateSwitch = other.GetComponent<GateSwitch>();

            if (currentGateSwitch.IsActivated)
            {
                currentGateSwitch = null;
                _interactBg.SetActive(false);
                _interactText.SetActive(false);
            }
        }

        //IF HEALTH PACK
        if (other.CompareTag("HealthPack"))
        {
            _interactBg.SetActive(true);
            _interactText.SetActive(true);

            currentHealthPack = other.GetComponent<InventoryItem>();
        }

	}

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SwitchPanel") || other.CompareTag("HealthPack"))
        {
            _interactBg.SetActive(false);
            _interactText.SetActive(false);
        }
    }

    public IEnumerator ShowPrompt(PromptTriggerZone pTriggerZone)
    {
        if (!pTriggerZone.IsPromptShown)
        {
            _gameData.IsPaused = true;
            _animator.SetFloat("speedPercent", 0f);

            _promptCanvas.SetActive(true);
            _promptText.text = pTriggerZone.PromptText;

            float t = 0;
            while (t<1)
            {
                colorGradingLayer.saturation.value = Mathf.Lerp(_normalSaturation, _promptSaturation, t);
                vignetteLayer.intensity.value = Mathf.Lerp(_normalVignette, _promptVignette, t);

                t += Time.deltaTime / _promptTransitionTime;

                yield return null;    
            }

            //colorGradingLayer.saturation.value = _promptSaturation;
            //vignetteLayer.intensity.value = _promptVignette;
            pTriggerZone.IsPromptShown = true;
        }
    }

    public void HidePrompt()
    {
        _gameData.IsPaused = false;
        colorGradingLayer.saturation.value = _normalSaturation;
        vignetteLayer.intensity.value = _normalVignette;
        _promptText.text = "";
        _promptCanvas.SetActive(false);
    }

    IEnumerator EndClimb()
    {
        //_animator.ResetTrigger("climb");
        _animator.SetTrigger("endclimb");

        //float t = 0;
        //while(t < 1){

        //    transform.position = Vector3.Lerp(transform.position, _currentLadder.LadderTop.position,t);
        //    t += Time.deltaTime / endClimbWait;
        //    yield return null;
        //}

        yield return new WaitForSeconds(endClimbWait);
        _animator.SetTrigger("finishclimb");

        if (transform.localPosition.y >= 0.01f)
            transform.position = _currentLadder.LadderTop.position;
        else
            transform.localPosition = new Vector3(transform.localPosition.x , 0.01f, transform.localPosition.z);
        _isClimbing = false;
        rigidbody.useGravity = true;
        _canClimb = false;
        climbStarted = false;
        _currentLadder = null;
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        endClimbCoroutine = null;

    }

    void ShowInteractWarning(string pWarning)
    {
        _interactWarningIcon.SetActive(true);
        _interactWarningText.gameObject.SetActive(true);

        _interactWarningText.text = pWarning;

        StartCoroutine(CloseWarning());
    }

    IEnumerator CloseWarning()
    {
        yield return new WaitForSeconds(_showWarningTime);

        _interactWarningIcon.SetActive(false);
        _interactWarningText.gameObject.SetActive(false);

    }

	void Update () 
	{
        if (!_gameData.IsPaused)
            Time.timeScale = 1;
        else
        {
            Time.timeScale = 0.5f;

            if(Input.GetKeyDown(KeyCode.Space))
            {
                HidePrompt();
                return;
            }
        }

        if (!_gameData.IsPaused)
        {
            if (_isAlive)
            {
                //INTERACT
                if(Input.GetKeyDown(KeyCode.E))
                {
                    //GATE OPEN
                    if(currentGateSwitch!=null)
                    {
                        _animator.SetTrigger("pulllever");
                        transform.LookAt(currentGateSwitch.transform);
                        _interactBg.SetActive(false);
                        _interactText.SetActive(false);
                        currentGateSwitch.Activate();
                        currentGateSwitch = null;
                    }


                    //HEALTH PACK
                    if(currentHealthPack!=null && currentHealthPack.InventoryType == InventoryType.HEALTH_PACK && _currentHealth < 100)
                    {
                        //TODO - VFX
                        _interactBg.SetActive(false);
                        _interactText.SetActive(false);

                        _currentHealth += currentHealthPack.HealthPack.Health;

                        if (_currentHealth > 100)
                            _currentHealth = 100;

                        Destroy(currentHealthPack.gameObject);
                        currentHealthPack = null;
                    }
                    else if(currentHealthPack != null && currentHealthPack.InventoryType == InventoryType.HEALTH_PACK && _currentHealth >= 100)
                    {
                        ShowInteractWarning(HEALTH_FULL_WARNING);
                    }


                }


                if (_canClimb && !_isAttacking && !_isHurting && !_isDiving && !_isPushing && !_isCrouching)
                {
                    rigidbody.useGravity = false;
                    rigidbody.constraints = RigidbodyConstraints.FreezeAll;
                    _isClimbing = true;

                    if (transform.localPosition.y <= _currentLadder.LadderHeight && transform.localPosition.y >= 0.01f)
                    {
                        float keyInput = Input.GetAxis("Vertical");

                        if(!climbStarted)
                        {
                            climbStarted = true;
                            _animator.ResetTrigger("endclimb");
                            _animator.SetTrigger("climb");
                        }

                        if (Mathf.Abs(keyInput) <= 0.1f)
                        {
                            climbStarted = false;
                            _animator.enabled = false;
                        }
                        else
                            _animator.enabled = true;

                        Vector3 upMovement = Vector3.up * _characterStats.ClimbSpeed * Time.deltaTime * Input.GetAxis("Vertical");
                        transform.position += upMovement;
                    }
                    else
                    {
                        if(endClimbCoroutine == null)
                            endClimbCoroutine = StartCoroutine(EndClimb());
                    }
                }

                if (!_isClimbing)
                {
                    if (Physics.Raycast(transform.position, -transform.up, HitDistance, GroundedLayerMask))
                    {
                        IsGrounded = true;
                    }
                    else
                    {
                        IsGrounded = false;
                        StartCoroutine(PlayFallAnimation());
                    }
                }
                else
                {
                    IsGrounded = false;
                }

                #region -------------Push----------------
                if (Input.GetKeyDown(KeyCode.P))
                {
                    _beginPush = !_beginPush;

                    _isCrouching = false;

                    if (_beginPush)
                    {
                        currentPushable = GetClosestPushable();

                        if (currentPushable != null)
                        {
                            _canPush = true;
                        }
                        else
                        {
                            _canPush = false;
                            _beginPush = false;
                        }
                    }
                    else
                    {
                        _canPush = false;
                        _beginPush = false;
                    }


                    if (_canPush)
                    {
                        transform.LookAt(currentPushable.transform);
                        pushableOffset = currentPushable.transform.position - transform.position;
                        //currentPushable.GetComponent<Collider>().enabled = false;
                        if(!_isPushing)
                            _animator.SetTrigger("pushstart");
                    }
                    else if(!_canPush && currentPushable!=null)
                    {
                        //currentPushable.GetComponent<Collider>().enabled = true;
                        pushableOffset = Vector3.zero;
                        _animator.SetTrigger("endpush");
                        currentPushable = null;
                        _isPushing = false;
                    }

                }
                #endregion


                if((Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) && !_isClimbing && !_zomzControls.ZomzMode.CurrentValue && !_isDiving && !_zomzControls.ZomzMode.CurrentSelectedZombie && !_canPush && !_isPushing)
                {
                    _isCrouching = !_isCrouching;

                    if(_isCrouching){
                        _ownCollider.height = CROUCH_COLLIDER_HEIGHT;
                        _ownCollider.center = CROUCH_COLLIDER_CENTER;
                        _animator.SetTrigger("crouch");
                    }
                    else{
                        _animator.SetTrigger("endcrouch");
                        _ownCollider.height = DEFAULT_COLLIDER_HEIGHT;
                        _ownCollider.center = DEFAULT_COLLIDER_CENTER;
                    }
                }

                //Attack
                if (Input.GetKeyDown(KeyCode.Space) && !_isClimbing && !_zomzControls.ZomzMode.CurrentValue && !_isDiving && !_zomzControls.ZomzMode.CurrentSelectedZombie && !_canPush && !_isPushing)
                {
                    if (_canAttack)
                    {
                        _isCrouching = false;
                        BeginAttack();
                    }
                }

                //Debug.Log(_isDiving);

                //DIVE
                if(Input.GetKeyDown(KeyCode.R) && !IsFalling && !_isClimbing && !_zomzControls.ZomzMode.CurrentValue && !_isDiving && !_isHurting && !_zomzControls.ZomzMode.CurrentSelectedZombie)
                {
                    _isDiving = true;
                    StartCoroutine(Dive());
                }

                if (!_isAttacking && !IsFalling && !_isHurting && !_isDiving && !_isClimbing && !_canClimb)
                {
                    bool running = Input.GetKey(KeyCode.LeftShift);

                    if (_canPush)
                        running = false;

                    float targetSpeed = 0;
                    if (!_canPush)
                        targetSpeed = ((running) ? _characterStats.RunSpeed : _characterStats.WalkSpeed);
                    else
                    {
                        targetSpeed = _characterStats.PushSpeed;
                    }
                    _currentSpeed = Mathf.SmoothDamp(_currentSpeed, targetSpeed, ref _speedSmoothVelocity, _speedSmoothTime);

                    Vector3 rightMovement = right * _currentSpeed * Time.deltaTime * Input.GetAxis("Horizontal");
                    Vector3 upMovement = forward * _currentSpeed * Time.deltaTime * Input.GetAxis("Vertical");

                    //Push Object
                    if(_canPush)
                    {
                        float rightPushAngle = Vector3.Angle(right, transform.forward);

                        rightMovement = Vector3.zero;
                        upMovement = transform.forward * _currentSpeed * Time.deltaTime * Input.GetAxis("Vertical");
                       

                        if(Input.GetAxis("Vertical") > 0 && upMovement!=Vector3.zero)
                        {
                            if (!_isPushing)
                            {
                                _animator.ResetTrigger("pushstart");
                                _animator.SetTrigger("push");
                            }
                        }
                        else if (Input.GetAxis("Vertical") < 0 && upMovement!=Vector3.zero)
                        {
                            if (!_isPushing)
                                _animator.SetTrigger("pull");
                        }
                    }


                    Vector3 heading = Vector3.Normalize(rightMovement + upMovement);

                    if (_zomzControls.ZomzMode.CurrentValue || _zomzControls.ZomzMode.CurrentSelectedZombie!=null || _isHurting)
                        heading = Vector3.zero;

                    if (heading != Vector3.zero)
                    {
                        _animator.ResetTrigger("hurt1");

                        EndCrouch();

                        if (!_canPush)
                            transform.forward = heading;
                        else
                            _isPushing = true;

                        transform.position += rightMovement + upMovement;

                        if (currentPushable != null)
                            currentPushable.transform.position = transform.position + pushableOffset;
                    }
                    else
                    {
                        if (_canPush)
                        {
                            _animator.SetTrigger("pushstart");
                            _isPushing = false;
                        }
                    }

                    float animationSpeedPercent = ((running) ? 1 : 0.5f) * heading.magnitude;

                    if (_animator)
                        _animator.SetFloat("speedPercent", animationSpeedPercent, _speedSmoothTime, Time.deltaTime);
                }
            }


            //DEBUG MODE
            if (_debugMode)
            {
                //Die
                if (Input.GetKeyDown(KeyCode.X))
                {
                    //Die();
                }

                //Hurt
                if (Input.GetKeyDown(KeyCode.H))
                {
                    //Hurt ();
                }
            }
        }
	}
}
