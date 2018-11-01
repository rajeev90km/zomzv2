using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelControllerBase : MonoBehaviour {
    
    public GameData GameData;

    public Animator FadeBgAnimator;

    [Header("CutScene Borders")]
    public Image TopBorder;

    public Image BottomBorder;

    [Header("Conversation Canvas")]
    public GameObject ConversationPanel;

    public GameObject AvatarHolder;

    public Image Avatar1;

    public Image TargetAvatar;

    public Text AvatarName;

    public Text ConversationText;

    [Header("Init Camera")]
    public Camera InitCamera;

    private Transform InitCameraPans;

    private List<Transform> panData = new List<Transform>();

    protected int conversationIndex = 0;

    private Animator _topCutSceneBorder;
    private Animator _bottomCutSceneBorder;

    private Conversation currentConversation;

    public bool IsConversationInProgress = false;

    public float CameraPanTime = 1f;

    private GameObject mainCameraObj;

    public bool EntrySequenceInProgress = false;

    Transform currentPanTransform = null;

    Coroutine midConvoPanCoroutine = null;

    public IEnumerator StartInitCameraPans()
    {
        mainCameraObj = Camera.main.gameObject;
        mainCameraObj.SetActive(false);
        InitCamera.gameObject.SetActive(true);

        for (int i = 0; i < panData.Count; i++)
        {

            float t = 0;

            Vector3 initPos = panData[i].GetChild(0).position;
            Vector3 endPos = panData[i].GetChild(1).position;

            Quaternion initRotation = panData[i].GetChild(0).rotation;
            Quaternion endRotation = panData[i].GetChild(1).rotation;

            InitCamera.transform.position = initPos;
            InitCamera.transform.rotation = initRotation;

            FadeBgAnimator.ResetTrigger("fadein");
            FadeBgAnimator.ResetTrigger("fadeout");
            FadeBgAnimator.SetTrigger("fadein");
            FadeBgAnimator.speed = 3;

            while(t<1)
            {
                InitCamera.transform.position = Vector3.Lerp(initPos, endPos, t);
                InitCamera.transform.rotation = Quaternion.Lerp(initRotation,endRotation, t);

                t += Time.deltaTime / CameraPanTime;
                yield return null;
            }

            InitCamera.transform.position = endPos;
            InitCamera.transform.rotation = endRotation;

            if (i < panData.Count - 1)
            {
                FadeBgAnimator.gameObject.SetActive(true);
                FadeBgAnimator.ResetTrigger("fadein");
                FadeBgAnimator.ResetTrigger("fadeout");
                FadeBgAnimator.SetTrigger("fadeout");
                FadeBgAnimator.speed = 7;
                yield return new WaitForSeconds(0.5f);
            }
        }

        FadeBgAnimator.gameObject.SetActive(true);
        FadeBgAnimator.SetTrigger("fadeout");
        FadeBgAnimator.speed = 7;
        yield return new WaitForSeconds(0.75f);
        FadeBgAnimator.SetTrigger("fadein");
        FadeBgAnimator.speed = 2;

        mainCameraObj.SetActive(true);
        InitCamera.gameObject.SetActive(false);


        yield return null;
    }


    protected virtual void Awake()
	{
        InitCameraPans = GameObject.FindWithTag("InitCameraPans").transform;

        for (int i = 0; i < InitCameraPans.childCount; i++)
        {
            panData.Add(InitCameraPans.GetChild(i));
        }

        _topCutSceneBorder = TopBorder.GetComponent<Animator>();
        _bottomCutSceneBorder = BottomBorder.GetComponent<Animator>();

        //StartCoroutine(StartInitCameraPans());
	}

	public void BeginConversation(Conversation pConversation)
    {
        InitCamera.transform.position = mainCameraObj.transform.position;
        InitCamera.transform.rotation = mainCameraObj.transform.rotation;

        IsConversationInProgress = true;

        if (ConversationPanel)
            ConversationPanel.SetActive(true);

        if (AvatarHolder)
            AvatarHolder.SetActive(true);

        currentConversation = pConversation;
        conversationIndex = 0;

        if (pConversation.AllDialogues.Count > 0)
            ProcessConversationEntity(currentConversation.AllDialogues[conversationIndex]);
    }


    public void EndConversation()
    {
        StartCoroutine(PanCameraDuringConversation(mainCameraObj.transform.position, mainCameraObj.transform.rotation, 0.5f, true));

        if (ConversationPanel)
            ConversationPanel.SetActive(false);

        if (AvatarHolder)
            AvatarHolder.SetActive(false);

        currentConversation = null;
        conversationIndex = 0;

        _topCutSceneBorder.SetTrigger("fadeout");
        _bottomCutSceneBorder.SetTrigger("fadeout");

        IsConversationInProgress = false;


    }

    void ProcessConversationEntity(ConversationEntity pConversationEntity)
    {
        if(Avatar1)
        {
            if (pConversationEntity.Avatar)
                Avatar1.sprite = pConversationEntity.Avatar;
        }

        if (TargetAvatar)
        {
            if (pConversationEntity.RightCharacter)
            {
                TargetAvatar.gameObject.SetActive(true);
                TargetAvatar.sprite = pConversationEntity.RightCharacter;
            }
            else
            {
                TargetAvatar.gameObject.SetActive(false);    
            }
        }

        if(AvatarName)
        {
            AvatarName.text = pConversationEntity.CharacterName;
        }

        if(ConversationText)
        {
            ConversationText.text = pConversationEntity.Text;
        }

        if(pConversationEntity.PanTransform != "")
        {
            Transform pantransform = GameObject.Find(pConversationEntity.PanTransform).transform;

            if (currentPanTransform != pantransform)
            {
                currentPanTransform = pantransform;

                mainCameraObj.SetActive(false);
                InitCamera.gameObject.SetActive(true);
                //InitCamera.transform.position = pantransform.position;
                //InitCamera.transform.rotation = pantransform.rotation;
                if(midConvoPanCoroutine!=null)
                {
                    StopCoroutine(midConvoPanCoroutine);
                    midConvoPanCoroutine = null;
                }
                midConvoPanCoroutine = StartCoroutine(PanCameraDuringConversation(pantransform.position, pantransform.rotation,0.5f,false));
            }

        }
        else
        {
            //if (midConvoPanCoroutine != null)
            //{
            //    StopCoroutine(midConvoPanCoroutine);
            //    midConvoPanCoroutine = null;
            //}

            //midConvoPanCoroutine = StartCoroutine(PanCameraDuringConversation(mainCameraObj.transform.position, mainCameraObj.transform.rotation,true));

            //InitCamera.gameObject.SetActive(false);
            //mainCameraObj.SetActive(true);
        }
    }

    IEnumerator PanCameraDuringConversation(Vector3 endPos, Quaternion endRot, float transitionTime, bool disableAtEnd)
    {
        float t = 0;

        Vector3 initPos = InitCamera.transform.position;
        Quaternion initRot = InitCamera.transform.rotation;

        while(t<1)
        {
            InitCamera.transform.position = Vector3.Lerp(initPos, endPos, t);
            InitCamera.transform.rotation = Quaternion.Lerp(initRot, endRot, t);

            t += Time.deltaTime / transitionTime;
            yield return null;
        }

        if (disableAtEnd)
        {
            InitCamera.gameObject.SetActive(false);
            mainCameraObj.SetActive(true);
        }

        midConvoPanCoroutine = null;
    }


	private void Update()
	{
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if (currentConversation)
            {
                if (conversationIndex < currentConversation.AllDialogues.Count - 1)
                {
                    conversationIndex++;
                    ProcessConversationEntity(currentConversation.AllDialogues[conversationIndex]);
                }
                else
                {
                    EndConversation();
                }
            }
        }
	}
}
