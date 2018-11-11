using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Zomz/Data/Level3Data", fileName = "Level3Data")]
public class Level3Data : LevelData {

	private void OnEnable()
	{
        CanScreenGlitch = true;
        IsInjured = true;
	}
}
