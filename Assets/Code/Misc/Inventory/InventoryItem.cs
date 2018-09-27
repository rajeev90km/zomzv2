using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InventoryType {
    HEALTH_PACK = 0,
    WEAPON = 1
}

public enum WeaponType
{
    MACHETE = 0,
    AXE = 1
}

public class InventoryItem : MonoBehaviour {
    
    [SerializeField]
    private InventoryType _inventoryType;
    public InventoryType InventoryType{
        get { return _inventoryType; }
    }

    [DrawIf("_inventoryType", InventoryType.HEALTH_PACK)]
    [SerializeField]
    private HealthPack _healthPack;
    public HealthPack HealthPack
    {
        get { return _healthPack; }
    }

    [DrawIf("_inventoryType", InventoryType.WEAPON)]
    [SerializeField]
    private Weapon _weapon;
    public Weapon Weapon
    {
        get { return _weapon; }
    }

    [DrawIf("_inventoryType", InventoryType.WEAPON)]
    [SerializeField]
    public WeaponType WeaponType;

    [DrawIf("_inventoryType", InventoryType.WEAPON)]
    [SerializeField]
    private int _currentDurability;
    public int CurrentDurability {
        get { return _currentDurability; }
        set { _currentDurability = value; }
    }

    public bool IsWeaponPicked = false;

	private void Start()
	{
        if (_inventoryType == InventoryType.WEAPON)
            _currentDurability = _weapon.Durability;
	}

}
