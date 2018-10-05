using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level2Controller : MonoBehaviour {

    public Level2Data LevelData;

    public GameData GameData;

	private void Awake()
	{
        GameData.CurrentLevelData = LevelData;	
	}

	public void OnBonfireLit(Flammable pBonfire)
	{
        if(!LevelData.BonfiresLit.Contains(pBonfire))
        {
            LevelData.BonfiresLit.Add(pBonfire);
        }

        if(LevelData.BonfiresLit.Count == LevelData.BonfiresRequiredToWin)
        {
            LevelData.ObjectiveComplete = true;
        }
	}

}
