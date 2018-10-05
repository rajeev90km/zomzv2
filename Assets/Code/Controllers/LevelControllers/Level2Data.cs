using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Zomz/Data/Level2Data", fileName = "Level2Data")]
public class Level2Data : LevelData {

    public int BonfiresRequiredToWin = 4;

    public List<Flammable> BonfiresLit = new List<Flammable>();

}
