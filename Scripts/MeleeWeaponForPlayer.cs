using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MeleeWeaponType
{
    Sword,
    Katana,
    Mace,
    Hammer,
    Axe,
    Zweihander
}
public class MeleeWeaponForPlayer : MonoBehaviour
{
    [SerializeField]
    public MeleeWeaponType WeaponType;

    public bool IsTeleportWeapon()
    {
        if (WeaponType == MeleeWeaponType.Zweihander || WeaponType == MeleeWeaponType.Axe)
            return false;
        return true;
    }
}
