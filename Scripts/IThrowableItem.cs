using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IThrowableItem
{
    public int CountInterface { get; set; }
    public GameObject PrefabGetter { get; }



    /// <summary>
    /// returns true when Count becomes zero
    /// </summary>
    /// <param name="rb"></param>
    /// <returns></returns>
    public bool Use(Rigidbody rb, Collider IgnoredCollider, bool isPlayer = true);
}

public class Knife : IThrowableItem
{
    public int CountInterface { get => Count; set { Count = value; } }
    public GameObject PrefabGetter => PrefabHolder._instance.KnifePrefab;
    public int Count { get; set; }


    public bool Use(Rigidbody rb, Collider IgnoredCollider, bool isPlayer = true)
    {
        if (Count <= 0) return true;

        Vector3 pos = rb.transform.position;
        if (!isPlayer) pos += Vector3.up * 0.5f;
        Count--;
        GameObject throwable = GameObject.Instantiate(PrefabGetter, pos, Quaternion.identity);
        if (isPlayer)
        {
            throwable.GetComponentInChildren<Rigidbody>().velocity = GameManager._instance.MainCamera.transform.forward * 32f;
            throwable.transform.position = GameManager._instance.IsLeftThrowing ? GameManager._instance.PlayerLeftHandTransform.position : GameManager._instance.PlayerRightHandTransform.position;
        }
        else
            throwable.GetComponentInChildren<Rigidbody>().velocity = rb.transform.forward * 22f;
        throwable.transform.forward = throwable.GetComponentInChildren<Rigidbody>().velocity.normalized;
        throwable.GetComponentInChildren<Rigidbody>().angularVelocity = throwable.transform.right * 15f;
        throwable.GetComponentInChildren<Projectile>().IgnoreCollisionCollider = IgnoredCollider;
        throwable.GetComponentInChildren<Projectile>().WhenTriggered = throwable.GetComponentInChildren<Projectile>().WhenTriggeredForKnife;

        if (Count <= 0) return true;
        return false;
    }

}

public class Bomb : IThrowableItem
{
    public int CountInterface { get => Count; set { Count = value; } }
    public GameObject PrefabGetter => PrefabHolder._instance.BombPrefab;
    public int Count { get; set; }

    
    public bool Use(Rigidbody rb, Collider IgnoredCollider, bool isPlayer = true)
    {
        if (Count <= 0) return true;

        Vector3 pos = rb.transform.position;
        if (!isPlayer) pos += Vector3.up * 0.5f;
        Count--;
        GameObject throwable = GameObject.Instantiate(PrefabGetter, pos, Quaternion.identity);
        if (isPlayer)
        {
            throwable.GetComponentInChildren<Rigidbody>().velocity = GameManager._instance.MainCamera.transform.forward * 16f;
            throwable.transform.position = GameManager._instance.IsLeftThrowing ? GameManager._instance.PlayerLeftHandTransform.position : GameManager._instance.PlayerRightHandTransform.position;
        }
        else
            throwable.GetComponentInChildren<Rigidbody>().velocity = rb.transform.forward * 11f;
        throwable.transform.forward = throwable.GetComponentInChildren<Rigidbody>().velocity.normalized;
        throwable.GetComponentInChildren<Rigidbody>().angularVelocity = new Vector3(1f, 1f, 1f);
        throwable.GetComponentInChildren<Projectile>().IgnoreCollisionCollider = IgnoredCollider;
        throwable.GetComponentInChildren<Projectile>().WhenTriggered = throwable.GetComponentInChildren<Projectile>().WhenTriggeredForBomb;

        if (Count <= 0) return true;
        return false;
    }

}
public class Smoke : IThrowableItem
{
    public int CountInterface { get => Count; set { Count = value; } }
    public GameObject PrefabGetter => PrefabHolder._instance.SmokeProjectilePrefab;
    public int Count { get; set; }

    public bool Use(Rigidbody rb, Collider IgnoredCollider, bool isPlayer = true)
    {
        if (Count <= 0) return true;

        Vector3 pos = rb.transform.position;
        if (!isPlayer) pos += Vector3.up * 0.5f;
        Count--;
        GameObject throwable = GameObject.Instantiate(PrefabGetter, pos, Quaternion.identity);
        if (isPlayer)
        {
            throwable.GetComponentInChildren<Rigidbody>().velocity = GameManager._instance.MainCamera.transform.forward * 40f / 2f;
            throwable.transform.position = GameManager._instance.IsLeftThrowing ? GameManager._instance.PlayerLeftHandTransform.position : GameManager._instance.PlayerRightHandTransform.position;
        }
        else
            throwable.GetComponentInChildren<Rigidbody>().velocity = rb.transform.forward * 30f / 3.5f;
        throwable.transform.forward = throwable.GetComponentInChildren<Rigidbody>().velocity.normalized;
        throwable.GetComponentInChildren<Rigidbody>().angularVelocity = new Vector3(1f, 1f, 1f);
        throwable.GetComponentInChildren<Projectile>().IgnoreCollisionCollider = IgnoredCollider;
        throwable.GetComponentInChildren<Projectile>().WhenTriggered = throwable.GetComponentInChildren<Projectile>().WhenTriggeredForSmoke;

        if (Count <= 0) return true;
        return false;
    }

}

public class Shuriken : IThrowableItem
{
    public int CountInterface { get => Count; set { Count = value; } }
    public GameObject PrefabGetter => PrefabHolder._instance.ShurikenPrefab;
    public int Count { get; set; }

   
    public bool Use(Rigidbody rb, Collider IgnoredCollider, bool isPlayer = true)
    {
        if (Count <= 0) return true;

        Vector3 pos = rb.transform.position;
        if (!isPlayer) pos += Vector3.up * 0.5f;
        Count--;
        GameObject throwable = GameObject.Instantiate(PrefabGetter, pos, Quaternion.identity);
        if (isPlayer)
        {
            throwable.GetComponentInChildren<Rigidbody>().velocity = GameManager._instance.MainCamera.transform.forward * 32f;
            throwable.transform.position = GameManager._instance.IsLeftThrowing ? GameManager._instance.PlayerLeftHandTransform.position : GameManager._instance.PlayerRightHandTransform.position;
        }
        else
            throwable.GetComponentInChildren<Rigidbody>().velocity = rb.transform.forward * 22f;
        throwable.transform.forward = throwable.GetComponentInChildren<Rigidbody>().velocity.normalized;
        throwable.GetComponentInChildren<Rigidbody>().angularVelocity = throwable.transform.up * 7f;
        throwable.GetComponentInChildren<Projectile>().IgnoreCollisionCollider = IgnoredCollider;
        throwable.GetComponentInChildren<Projectile>().WhenTriggered = throwable.GetComponentInChildren<Projectile>().WhenTriggeredForShuriken;

        if (Count <= 0) return true;
        return false;
    }

}

public class Glass : IThrowableItem
{
    public int CountInterface { get => Count; set { Count = value; } }
    public GameObject PrefabGetter => PrefabHolder._instance.GlassPrefab;
    public int Count { get; set; }

   
    public bool Use(Rigidbody rb, Collider IgnoredCollider, bool isPlayer = true)
    {
        if (Count <= 0) return true;

        Vector3 pos = rb.transform.position;
        if (!isPlayer) pos += Vector3.up * 0.5f;
        Count--;
        GameObject throwable = GameObject.Instantiate(PrefabGetter, pos, Quaternion.identity);
        if (isPlayer)
        {
            throwable.GetComponentInChildren<Rigidbody>().velocity = GameManager._instance.MainCamera.transform.forward * 60f / 2f;
            throwable.transform.position = GameManager._instance.IsLeftThrowing ? GameManager._instance.PlayerLeftHandTransform.position : GameManager._instance.PlayerRightHandTransform.position;
        }
        else
            throwable.GetComponentInChildren<Rigidbody>().velocity = rb.transform.forward * 30f / 2f;
        throwable.transform.forward = throwable.GetComponentInChildren<Rigidbody>().velocity.normalized;
        throwable.GetComponentInChildren<Rigidbody>().angularVelocity = new Vector3(1f, 1f, 1f);
        throwable.GetComponentInChildren<Projectile>().IgnoreCollisionCollider = IgnoredCollider;
        throwable.GetComponentInChildren<Projectile>().WhenTriggered = throwable.GetComponentInChildren<Projectile>().WhenTriggeredForGlass;

        if (Count <= 0) return true;
        return false;
    }

}

public class Stone : IThrowableItem
{
    public int CountInterface { get => Count; set { Count = value; } }
    public GameObject PrefabGetter => PrefabHolder._instance.StonePrefab;
    public int Count { get; set; }

    public bool Use(Rigidbody rb, Collider IgnoredCollider, bool isPlayer = true)
    {
        if (Count <= 0) return true;

        Vector3 pos = rb.transform.position;
        if (!isPlayer) pos += Vector3.up * 0.5f;
        Count--;
        GameObject throwable = GameObject.Instantiate(PrefabGetter, pos, Quaternion.identity);
        if (isPlayer)
        {
            throwable.GetComponentInChildren<Rigidbody>().velocity = GameManager._instance.MainCamera.transform.forward * 60f / 2f;
            throwable.transform.position = GameManager._instance.IsLeftThrowing ? GameManager._instance.PlayerLeftHandTransform.position : GameManager._instance.PlayerRightHandTransform.position;
        }
        else
            throwable.GetComponentInChildren<Rigidbody>().velocity = rb.transform.forward * 30f / 2f;
        throwable.transform.forward = throwable.GetComponentInChildren<Rigidbody>().velocity.normalized;
        throwable.GetComponentInChildren<Rigidbody>().angularVelocity = new Vector3(1f, 1f, 1f);
        throwable.GetComponentInChildren<Projectile>().IgnoreCollisionCollider = IgnoredCollider;
        throwable.GetComponentInChildren<Projectile>().WhenTriggered = throwable.GetComponentInChildren<Projectile>().WhenTriggeredForStone;

        if (Count <= 0) return true;
        return false;
    }

}