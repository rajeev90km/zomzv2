using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level3Controller : LevelControllerBase
{
    public Level3Data LevelData;

    protected override void Awake()
    {
        base.Awake();
        GameData.CurrentLevelData = LevelData;
    }
}
