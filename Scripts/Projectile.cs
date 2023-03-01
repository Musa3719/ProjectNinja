using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Projectile : MonoBehaviour, IKillObject
{
    public List<IKillable> killables;
    public List<IKillable> Killables
    {
        get
        {
            if (killables == null)
            {
                killables = new List<IKillable>();
            }
            return killables;
        }
    }

    public Collider IgnoreCollisionCollider;

    public Action<Collider, Vector3> WhenTriggered;

    private Rigidbody rb;
    private GameObject soundObj;

    private void Awake()
    {
        if (GetComponent<Rigidbody>() != null)
        {
            rb = GetComponent<Rigidbody>();
        }
        else
        {
            rb = transform.parent.GetComponent<Rigidbody>();
        }
        soundObj = SoundManager._instance.PlaySound(SoundManager._instance.ProjectileMoving, transform.position, 0.1f, true, 1f);
        soundObj.GetComponent<AudioSource>().maxDistance /= 3f;
    }
    public void ChangeSoundObj(GameObject newObj)
    {
        soundObj = newObj;
    }
    private void Update()
    {
        soundObj.transform.position = transform.position;
    }
    private void OnEnable()
    {
        GameManager._instance.Projectiles.Add(gameObject);
        Killables.Clear();
    }
    private void OnDisable()
    {
        GameManager._instance.Projectiles.Remove(gameObject);
        Destroy(soundObj);
        if (transform.parent != null && transform.parent.GetComponent<VisualEffect>() != null)
        {
            transform.parent.GetComponent<VisualEffect>().enabled = false;
        }
    }
    public void Kill(IKillable killable, Vector3 dir, float killersVelocityMagnitude)
    {
        SoundManager._instance.PlaySound(SoundManager._instance.Stab, transform.position, 0.4f, false, UnityEngine.Random.Range(0.93f, 1.07f));
        killable.Die(dir, killersVelocityMagnitude);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BreakableObject"))
        {
            other.GetComponent<BreakableObject>().BrakeObject((transform.position - other.transform.position).normalized);
        }

        if (other.isTrigger && !other.CompareTag("HitBox")) return;

        WhenTriggered?.Invoke(other, transform.position);
    }
    private bool IsInAngle(Collider other)
    {
        Vector3 otherForward = other.transform.forward;
        otherForward.y = 0f;
        Vector3 selfForward = rb.velocity.normalized;
        selfForward.y = 0f;

        float angle = Vector3.Angle(otherForward, selfForward);

        if (angle < 75f)
            return false;
        else
            return true;
    }
    public void WhenTriggeredForKnife(Collider other, Vector3 position)
    {
        if (IgnoreCollisionCheck(IgnoreCollisionCollider, other)) return;
        if (other == null || other.CompareTag("ProjectileTrap") || other.CompareTag("Enemy") || other.CompareTag("Boss")) return;
        
        if (other.CompareTag("HitBox"))
        {
            IKillable otherKillable = GameManager._instance.GetHitBoxIKillable(other);

            if (otherKillable != null && !otherKillable.IsDead && !Killables.Contains(otherKillable))
            {
                Killables.Add(otherKillable);

                bool isTargetDodging = otherKillable.IsDodgingGetter;
                bool isTargetBlocking = otherKillable.IsBlockingGetter;

                if (!isTargetBlocking && !isTargetDodging)
                {
                    if(!otherKillable.Object.CompareTag("Boss"))
                        Kill(otherKillable, -other.transform.forward, rb.velocity.magnitude / 4f);
                }
                else if (isTargetDodging)
                {
                    if (otherKillable.Object.CompareTag("Enemy"))
                    {
                        Kill(otherKillable, -other.transform.forward, rb.velocity.magnitude / 4f);
                    }
                }
                else if (isTargetBlocking)
                {
                    if (IsInAngle(other))
                    {
                        otherKillable.DeflectWithBlock(other.transform.forward, IgnoreCollisionCollider.GetComponent<IKillable>(), true);
                    }
                    else
                    {
                        if (!otherKillable.Object.CompareTag("Boss"))
                            Kill(otherKillable, other.transform.forward, rb.velocity.magnitude / 4f);
                    }
                }
            }
        }
        else if (other.CompareTag("Wall"))
        {
            SoundManager._instance.PlaySound(SoundManager._instance.HitWallWithWeapon, transform.position, 0.25f, false, UnityEngine.Random.Range(0.93f, 1.07f));
            GameObject decal = Instantiate(GameManager._instance.HoleDecal, transform.position - rb.velocity * 0.02f, Quaternion.identity);
            decal.transform.forward = other.transform.right;
            decal.transform.localEulerAngles = new Vector3(decal.transform.localEulerAngles.x, decal.transform.localEulerAngles.y, UnityEngine.Random.Range(0f, 360f));
            Destroy(Instantiate(GameManager._instance.HitSmokeVFX, IgnoreCollisionCollider.transform.position + IgnoreCollisionCollider.transform.forward * 0.3f, Quaternion.identity), 5f);
        }

        SoundManager.ProjectileTriggeredSoundArtificial?.Invoke(other.transform.position, 15f);
        ProjectileHit();
    }
    public void WhenTriggeredForBomb(Collider other, Vector3 position)
    {
        if (IgnoreCollisionCheck(IgnoreCollisionCollider, other)) return;
        if (other == null || other.CompareTag("ProjectileTrap")) return;

        if (CheckForDeflectBomb(other))
        {
            rb.velocity = rb.velocity.magnitude * -rb.velocity.normalized;

            return;
        }

        position -= rb.velocity * 0.1f;

        float bombRange = 8.5f;
        Collider[] sphereCastColliders = Physics.OverlapSphere(position, bombRange);
        foreach (var collider in sphereCastColliders)
        {
            Vector3 direction = (collider.transform.position - position).normalized;
            RaycastHit hit;
            Physics.Raycast(position, direction, out hit, bombRange);

            if (hit.collider == null || (!hit.collider.CompareTag("Player") && !hit.collider.CompareTag("Boss") && !hit.collider.CompareTag("Enemy") && !hit.collider.CompareTag("HitBox"))) continue;

            IKillable colliderKillable = GameManager._instance.GetHitBoxIKillable(hit.collider);

            if (GameManager._instance.GetHitBoxIKillableObject(hit.collider).CompareTag("Boss"))
            {
                GameManager._instance.GetHitBoxIKillableObject(hit.collider).GetComponent<IKillable>().BombDeflected();
                continue;
            }

            if (colliderKillable != null && !Killables.Contains(colliderKillable))
            {
                Killables.Add(colliderKillable);
                colliderKillable.Die((collider.transform.position - position).normalized, rb.velocity.magnitude);
            }
        }
        SoundManager.ProjectileTriggeredSoundArtificial?.Invoke(position, 50f);
        GameObject explodeVFX = Instantiate(GameManager._instance.GetRandomFromList(GameManager._instance.ExplosionVFX), position, Quaternion.identity);
        SoundManager._instance.PlaySound(SoundManager._instance.BombExplode, transform.position, 0.2f, false, UnityEngine.Random.Range(1.25f, 1.5f));
        Destroy(gameObject);
    }
    private bool CheckForDeflectBomb(Collider other)
    {
        bool deflected = false;

        if (other.name == "AttackCollider")
        {
            deflected = true;
            other.transform.parent.GetComponent<IKillable>().BombDeflected();
        }
        /*else
        {
            IKillable otherKillable = other.GetComponent<IKillable>();
            if (otherKillable != null && otherKillable.AttackCollider.activeInHierarchy)
            {
                float angle = Vector3.Angle(Camera.main.transform.forward, (other.transform.position - transform.position).normalized);
                Debug.Log(angle);
                if (angle <= 120f)
                {
                    deflected = true;
                    otherKillable.BombDeflected();
                }
            }
        }*/
        return deflected;
    }
    public void WhenTriggeredForSmoke(Collider other, Vector3 position)
    {
        if (IgnoreCollisionCheck(IgnoreCollisionCollider, other)) return;
        if (other == null || other.CompareTag("ProjectileTrap") || other.CompareTag("Enemy") || other.CompareTag("Boss")) return;

        Instantiate(GameManager._instance.SmokePrefab, position - rb.velocity * 0.1f, Quaternion.identity);
        SoundManager._instance.PlaySound(SoundManager._instance.SmokeExplode, transform.position, 0.3f, false, UnityEngine.Random.Range(1.25f, 1.5f));
        Destroy(gameObject);
    }
    public void WhenTriggeredForShuriken(Collider other, Vector3 position)
    {
        if (IgnoreCollisionCheck(IgnoreCollisionCollider, other)) return;
        if (other == null || other.CompareTag("ProjectileTrap") || other.CompareTag("Enemy") || other.CompareTag("Boss")) return;

        if (other.CompareTag("HitBox"))
        {
            IKillable otherKillable = GameManager._instance.GetHitBoxIKillable(other);

            if (otherKillable != null && !Killables.Contains(otherKillable))
            {
                Killables.Add(otherKillable);

                bool isTargetDodging = otherKillable.IsDodgingGetter;
                bool isTargetBlocking = otherKillable.IsBlockingGetter;

                if (!isTargetBlocking && !isTargetDodging)
                {
                    if (!otherKillable.Object.CompareTag("Boss"))
                        Kill(otherKillable, -other.transform.forward, rb.velocity.magnitude / 4f);
                }
                else if (isTargetDodging)
                {
                    if (otherKillable.Object.CompareTag("Enemy"))
                    {
                        Kill(otherKillable, -other.transform.forward, rb.velocity.magnitude / 4f);
                    }
                }
                else if (isTargetBlocking)
                {
                    if (IsInAngle(other))
                    {
                        otherKillable.DeflectWithBlock(other.transform.forward, IgnoreCollisionCollider.GetComponent<IKillable>(), true);
                    }
                    else
                    {
                        if (!otherKillable.Object.CompareTag("Boss"))
                            Kill(otherKillable, other.transform.forward, rb.velocity.magnitude / 4f);
                    }
                }
            }
        }
        else if (other.CompareTag("Wall"))
        {
            SoundManager._instance.PlaySound(SoundManager._instance.HitWallWithWeapon, transform.position, 0.25f, false, UnityEngine.Random.Range(0.93f, 1.07f));
            GameObject decal = Instantiate(GameManager._instance.HoleDecal, transform.position - rb.velocity * 0.02f, Quaternion.identity);
            decal.transform.forward = other.transform.right;
            decal.transform.localEulerAngles = new Vector3(decal.transform.localEulerAngles.x, decal.transform.localEulerAngles.y, UnityEngine.Random.Range(0f, 360f));
            Destroy(Instantiate(GameManager._instance.HitSmokeVFX, IgnoreCollisionCollider.transform.position + IgnoreCollisionCollider.transform.forward * 0.3f, Quaternion.identity), 5f);
        }

        SoundManager.ProjectileTriggeredSoundArtificial?.Invoke(other.transform.position, 15f);
        ProjectileHit();
    }
    public void WhenTriggeredForGlass(Collider other, Vector3 position)
    {
        if (IgnoreCollisionCheck(IgnoreCollisionCollider, other)) return;
        if (other == null || other.CompareTag("ProjectileTrap") || other.CompareTag("Enemy") || other.CompareTag("Boss")) return;

        SoundManager._instance.PlaySound(SoundManager._instance.GlassBroken, transform.position, 0.3f, false, UnityEngine.Random.Range(0.93f, 1.07f));

        if (other.CompareTag("HitBox"))
        {
            IKillable otherKillable = GameManager._instance.GetHitBoxIKillable(other);

            if (otherKillable != null && !Killables.Contains(otherKillable))
            {
                Killables.Add(otherKillable);

                otherKillable.Stun(1.5f, false, otherKillable.Object.transform);
            }
        }

        SoundManager.ProjectileTriggeredSoundArtificial?.Invoke(other.transform.position, 20f);
        GameObject brokenGlass = Instantiate(GameManager._instance.GlassBrokenPrefab, transform.position, transform.rotation);
        foreach (Transform shard in brokenGlass.transform)
        {
            Rigidbody rb = shard.GetComponent<Rigidbody>();
            rb.velocity = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f)) * 0.25f;
            GameManager._instance.CallForAction(() => { rb.isKinematic = true; rb.GetComponent<Collider>().enabled = false; }, 5f);
        }
        Destroy(gameObject);
    }
    public void WhenTriggeredForStone(Collider other, Vector3 position)
    {
        if (IgnoreCollisionCheck(IgnoreCollisionCollider, other)) return;
        if (other == null || other.CompareTag("ProjectileTrap") || other.CompareTag("Enemy") || other.CompareTag("Boss")) return;

        SoundManager._instance.PlaySound(SoundManager._instance.StoneHit, transform.position, 0.2f, false, UnityEngine.Random.Range(0.93f, 1.07f));

        if (other.CompareTag("HitBox"))
        {
            IKillable otherKillable = GameManager._instance.GetHitBoxIKillable(other);

            if (otherKillable != null && !Killables.Contains(otherKillable))
            {
                Killables.Add(otherKillable);

                otherKillable.Stun(1.5f, false, otherKillable.Object.transform);
            }
        }

        SoundManager.ProjectileTriggeredSoundArtificial?.Invoke(other.transform.position, 12.5f);
        ProjectileHit();
    }
    private void ProjectileHit()
    {
        GetComponent<Collider>().isTrigger = false;
        rb.MovePosition(transform.position - rb.velocity / 30f);
        rb.velocity /= 2.25f;
        rb.useGravity = true;
        rb.angularVelocity += new Vector3(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(-3f, 3f));
        gameObject.AddComponent(typeof(ProjectileStopped));
        this.enabled = false;
    }

    private bool IgnoreCollisionCheck(Collider Ignored, Collider collisionCollider)
    {
        Transform collisionTransform = collisionCollider.transform;
        while (collisionTransform.parent != null)
        {
            collisionTransform = collisionTransform.parent;
        }
        if (collisionTransform.GetComponent<Collider>() == Ignored)
        {
            return true;
        }
        return false;
    }
}
