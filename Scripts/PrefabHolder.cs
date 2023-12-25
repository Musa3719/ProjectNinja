using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrefabHolder : MonoBehaviour
{
    public static PrefabHolder _instance;

    [SerializeField]
    public GameObject Boss3;

    [SerializeField]
    public GameObject LaserPrefabBoss;

    [SerializeField]
    public GameObject KnifePrefab;
    [SerializeField]
    public GameObject ShurikenPrefab;
    [SerializeField]
    public GameObject BombPrefab;
    [SerializeField]
    public GameObject SmokeProjectilePrefab;
    [SerializeField]
    public GameObject GlassPrefab;
    [SerializeField]
    public GameObject StonePrefab;
    [SerializeField]
    public GameObject Spell1;
    [SerializeField]
    public GameObject Spell2;
    [SerializeField]
    public GameObject L1Explosive;
    [SerializeField]
    public GameObject L5Explosive;
    [SerializeField]
    public GameObject L1ExplosiveBroken;
    [SerializeField]
    public GameObject Wolf;

    [SerializeField]
    public GameObject KnifeItemMesh;
    [SerializeField]
    public GameObject ShurikenItemMesh;
    [SerializeField]
    public GameObject BombItemMesh;
    [SerializeField]
    public GameObject SmokeItemMesh;
    [SerializeField]
    public GameObject GlassItemMesh;
    [SerializeField]
    public GameObject StoneItemMesh;

    [SerializeField]
    public GameObject SwordPrefab;
    [SerializeField]
    public GameObject AxePrefab;
    [SerializeField]
    public GameObject HalberdPrefab;
    [SerializeField]
    public GameObject MacePrefab;
    [SerializeField]
    public GameObject HammerPrefab;
    [SerializeField]
    public GameObject BowPrefab;
    [SerializeField]
    public GameObject CrossbowPrefab;
    [SerializeField]
    public GameObject GunPrefab;
    [SerializeField]
    public GameObject KatanaPrefab;
    [SerializeField]
    public GameObject ZweihanderPrefab;



    public Knife KnifeHolder;
    public Bomb BombHolder;
    public Smoke SmokeHolder;
    public Shuriken ShurikenHolder;
    public Glass GlassHolder;
    public Stone StoneHolder;

    [SerializeField]
    public Sprite EmptyImage;
    [SerializeField]
    public Sprite KnifeImage;
    [SerializeField]
    public Sprite BombImage;
    [SerializeField]
    public Sprite SmokeImage;
    [SerializeField]
    public Sprite ShurikenImage;
    [SerializeField]
    public Sprite GlassImage;
    [SerializeField]
    public Sprite StoneImage;

    private void Awake()
    {
        _instance = this;
        KnifeHolder = new Knife();
        BombHolder = new Bomb();
        SmokeHolder = new Smoke();
        ShurikenHolder = new Shuriken();
        GlassHolder = new Glass();
        StoneHolder = new Stone();
    }
}
