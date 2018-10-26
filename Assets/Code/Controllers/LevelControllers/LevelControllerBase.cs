using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelControllerBase : MonoBehaviour {
    
    public GameData GameData;

    [Header("Conversation Canvas")]
    public GameObject ConversationPanel;

    public GameObject AvatarHolder;

    public Image Avatar1;

    public Image TargetAvatar;

    public Text AvatarName;

    public Text ConversationText;

    protected int conversationIndex = 0;

    private Conversation currentConversation;

    public bool IsConversationInProgress = false;

    public void BeginConversation(Conversation pConversation)
    {
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
        if (ConversationPanel)
            ConversationPanel.SetActive(false);

        if (AvatarHolder)
            AvatarHolder.SetActive(false);

        currentConversation = null;
        conversationIndex = 0;

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
