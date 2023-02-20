using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemEnum
{
    Knife,
    Bomb,
    Smoke,
    Shuriken,
    Glass,
    Stone
}
public class Item : MonoBehaviour
{
    public IThrowableItem _ItemHolder;

    public ItemEnum ItemType;
    private void Start()
    {
        switch (ItemType)
        {
            case ItemEnum.Knife:
                _ItemHolder = PrefabHolder._instance.KnifeHolder;
                Instantiate(PrefabHolder._instance.KnifeItemMesh, transform);
                break;
            case ItemEnum.Bomb:
                _ItemHolder = PrefabHolder._instance.BombHolder;
                Instantiate(PrefabHolder._instance.BombItemMesh, transform);
                break;
            case ItemEnum.Smoke:
                _ItemHolder = PrefabHolder._instance.SmokeHolder;
                Instantiate(PrefabHolder._instance.SmokeItemMesh, transform);
                break;
            case ItemEnum.Shuriken:
                _ItemHolder = PrefabHolder._instance.ShurikenHolder;
                Instantiate(PrefabHolder._instance.ShurikenItemMesh, transform);
                break;
            case ItemEnum.Glass:
                _ItemHolder = PrefabHolder._instance.GlassHolder;
                Instantiate(PrefabHolder._instance.GlassItemMesh, transform);
                break;
            case ItemEnum.Stone:
                _ItemHolder = PrefabHolder._instance.StoneHolder;
                Instantiate(PrefabHolder._instance.StoneItemMesh, transform);
                break;
            default:
                Debug.LogError("Throwable Item Enum is not correct...");
                break;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other!=null && other.gameObject!=null && other.gameObject.CompareTag("Player"))
        {
            other.GetComponent<PlayerCombat>().AddToThrowableInventory(_ItemHolder);
            Destroy(gameObject);
        }
    }
}
