using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1Controller : LevelControllerBase {

    public Level1Data LevelData;

    private void Awake()
    {
        GameData.CurrentLevelData = LevelData;
    }

    public void OnGuyKilled()
    {
        
    }
}
