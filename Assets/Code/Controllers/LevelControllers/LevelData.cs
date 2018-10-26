using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LevelData : ScriptableObject {

    public bool ObjectiveComplete;

    public bool CanUseZomzMode;

    public string ObjectiveText;

    public Conversation LevelBeginConversation;
}
