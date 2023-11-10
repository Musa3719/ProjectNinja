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
    Zweihander,
    Spear
}
public class MeleeWeaponForPlayer : MonoBehaviour
{
    [SerializeField]
    public MeleeWeaponType WeaponType;

    public bool IsHardHit()
    {
        if (WeaponType == MeleeWeaponType.Mace || WeaponType == MeleeWeaponType.Hammer || WeaponType == MeleeWeaponType.Zweihander)
            return true;
        return false;
    }

}
