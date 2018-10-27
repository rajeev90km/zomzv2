using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1Controller : LevelControllerBase {

    public Level1Data LevelData;

    protected override void Awake()
    {
        base.Awake();
        GameData.CurrentLevelData = LevelData;
    }

    public void OnGuyKilled()
    {
        
    }
}
