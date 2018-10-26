using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level2Controller : LevelControllerBase {

    public Level2Data LevelData;

    private void Awake()
    {
        GameData.CurrentLevelData = LevelData;
    }

	public void OnBonfireLit(Flammable pBonfire)
	{
        AkSoundEngine.PostEvent("Bonfire_Ignite", gameObject);
        AkSoundEngine.PostEvent("Bonfire_Loop", gameObject);
        if (!LevelData.BonfiresLit.Contains(pBonfire))
        {
            
            LevelData.BonfiresLit.Add(pBonfire);
        }

        if(LevelData.BonfiresLit.Count == LevelData.BonfiresRequiredToWin)
        {
            LevelData.ObjectiveComplete = true;
        }
	}

}
