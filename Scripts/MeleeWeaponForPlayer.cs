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
    public float GetAttackTime()
    {
        switch (WeaponType)
        {
            case MeleeWeaponType.Katana:
                return 0.7f;
            case MeleeWeaponType.Sword:
            case MeleeWeaponType.Mace:
            case MeleeWeaponType.Hammer:
                return 0.9f;
            case MeleeWeaponType.Zweihander:
            case MeleeWeaponType.Axe:
                return 1.1f;
            case MeleeWeaponType.Spear:
            default:
                return 0f;
        }
    }

}
