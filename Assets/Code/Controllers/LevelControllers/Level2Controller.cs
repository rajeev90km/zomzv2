using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level2Controller : MonoBehaviour {

    public int BonfiresRequiredToWin = 4;

    public List<Flammable> BonfiresLit = new List<Flammable>();

    public void OnBonfireLit(Flammable pBonfire)
	{
        if(!BonfiresLit.Contains(pBonfire))
        {
            BonfiresLit.Add(pBonfire);
        }
	}

}
