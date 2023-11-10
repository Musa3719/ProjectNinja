using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveL1 : MonoBehaviour, IKillObject
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
    public GameObject Owner => gameObject;


    private Transform _playerTransform;
    private Rigidbody _rb;
    private bool _isAboutToExplode;
    public bool _isTriggered;
    private float timer;

    private Coroutine _explodeCoroutine;
    private GameObject explodingSound;

    private void OnEnable()
    {
        GameManager._instance.Projectiles.Add(gameObject);
    }
    private void OnDisable()
    {
        GameManager._instance.Projectiles.Remove(gameObject);
    }

    private void Awake()
    {
        _playerTransform = GameManager._instance.PlayerRb.transform;
        _rb = transform.parent.GetComponentInChildren<Rigidbody>();
    }
    private Transform GetParent(Transform trans)
    {
        Transform temp = trans;
        while (temp.parent != null)
            temp = temp.parent;
        return temp;
    }
    private void Update()
    {
        if (GameManager._instance.isGameStopped || GameManager._instance.isPlayerDead || GameManager._instance.isOnCutscene) return;

        if (!_isTriggered)
        {
            Physics.Raycast(transform.position, (_playerTransform.position - transform.position).normalized, out RaycastHit hit, 10f, GameManager._instance.LayerMaskForVisible);
            if (hit.collider != null && GetParent(hit.collider.transform).CompareTag("Player")) _isTriggered = true;
        }

        if (_isAboutToExplode || !_isTriggered) return;

        if (_rb.velocity.magnitude < 0.1f)
        {
            Vector3 direction = (_playerTransform.position - transform.position).normalized;
            _rb.velocity = direction;
        }

        timer += Time.deltaTime;
        if (timer > 6.5f)
        {
            StartExploding();
            return;
        }
        Vector3 dir = (_playerTransform.position - transform.position).normalized;
        float targetVelocityMagnitude= (_playerTransform.position - transform.position).magnitude;
        targetVelocityMagnitude = Mathf.Clamp(4f * Mathf.Log(targetVelocityMagnitude), 0.2f, 13f);
        _rb.velocity = Vector3.Lerp(_rb.velocity, targetVelocityMagnitude * dir, Time.deltaTime * 2f);

        if ((_playerTransform.position - transform.position).magnitude < 2.25f)
        {
            StartExploding();
        }
    }
    private void StartExploding()
    {
        _explodeCoroutine = StartCoroutine(ExplodeCoroutine());
    }
    private IEnumerator ExplodeCoroutine()
    {
        if (_isAboutToExplode) yield break;

        _isAboutToExplode = true;

        GetComponent<Animator>().SetTrigger("Exploding");
        explodingSound = SoundManager._instance.PlaySound(SoundManager._instance.ReadyForExplosion, transform.position, 0.12f, false, 1f);
        float waitTime = Random.Range(1f, 2f);
        float startTime = Time.time;
        AudioSource explodingSource = explodingSound.GetComponent<AudioSource>();
        while (startTime + waitTime > Time.time)
        {
            explodingSource.pitch = Mathf.Lerp(0.75f, 2.5f, (Time.time - startTime) / waitTime);
            _rb.velocity = Vector3.Lerp(_rb.velocity, Vector3.zero, Time.deltaTime * 10f);
            yield return null;
        }

        Explode();
    }
    public void Explode()
    {
        SoundManager._instance.PlaySound(SoundManager._instance.BombExplode, transform.position, 0.3f, false, UnityEngine.Random.Range(1f, 1.2f));
        GameObject VFX = Instantiate(GameManager._instance.GetRandomFromList(GameManager._instance.ExplosionVFX), transform.position, Quaternion.identity);

        float bombRange = 4f;
        Collider[] sphereCastColliders = Physics.OverlapSphere(transform.position, bombRange);
        foreach (var collider in sphereCastColliders)
        {
            Vector3 direction = (collider.transform.position - transform.position).normalized;
            RaycastHit hit;

            Physics.Raycast(transform.position, direction, out hit, bombRange, GameManager._instance.LayerMaskForVisibleWithSolidTransparent);
            if (hit.collider == collider && collider.CompareTag("Door"))
                collider.GetComponentInChildren<Rigidbody>().AddForce((collider.transform.position - transform.position).normalized * 150f, ForceMode.Impulse);

            Physics.Raycast(transform.position, direction, out hit, bombRange, GameManager._instance.LayerMaskForVisible);

            if (hit.collider != null && hit.collider.gameObject.CompareTag("Wolf"))
            {
                hit.collider.GetComponent<Wolf>().Die((collider.transform.position - transform.position).normalized, 10f, this);
                continue;
            }

            if (hit.collider == null || (!hit.collider.CompareTag("Player") && !hit.collider.CompareTag("BreakableObject") && !hit.collider.CompareTag("Boss") && !hit.collider.CompareTag("Enemy") && !hit.collider.CompareTag("ExplosiveL1") && !hit.collider.CompareTag("HitBox"))) continue;

            if (hit.collider.CompareTag("BreakableObject"))
            {
                hit.collider.GetComponent<BreakableObject>().BrakeObject((transform.position - hit.collider.transform.position).normalized);
            }
            else if (hit.collider.CompareTag("ExplosiveL1") && hit.collider.gameObject != transform.parent.gameObject)
            {
                //hit.collider.GetComponentInChildren<ExplosiveL1>().StopAllCoroutines();
                //hit.collider.GetComponentInChildren<ExplosiveL1>().Explode();
            }

            IKillable colliderKillable = GameManager._instance.GetHitBoxIKillable(hit.collider);

            if (GameManager._instance.GetHitBoxIKillableObject(hit.collider).CompareTag("Boss"))
            {
                GameManager._instance.GetHitBoxIKillableObject(hit.collider).GetComponent<IKillable>().BombDeflected();
                continue;
            }

            if (colliderKillable != null && !Killables.Contains(colliderKillable))
            {
                Killables.Add(colliderKillable);
                Kill(colliderKillable, (collider.transform.position - transform.position).normalized, 15f, this);
            }
        }
        SoundManager.ProjectileTriggeredSoundArtificial?.Invoke(transform.position, 12.5f);

        GameObject broken = Instantiate(PrefabHolder._instance.L1ExplosiveBroken, transform.position, transform.rotation);
        foreach (Transform item in broken.transform)
        {
            Rigidbody tempRB = item.GetComponent<Rigidbody>();
            tempRB.AddForce((item.transform.position - broken.transform.position).normalized * 40f);
            GameManager._instance.CallForAction(() => { tempRB.isKinematic = true; tempRB.GetComponent<Collider>().enabled = false; }, 20f);
        }

        Destroy(explodingSound);
        Destroy(VFX);
        Destroy(transform.parent.gameObject);
    }
    public void Kill(IKillable killable, Vector3 dir, float killersVelocityMagnitude, IKillObject killer)
    {
        killable.Die(dir, _rb.velocity.magnitude, killer, false);
    }
    public void DestroyWithoutExploding(Transform other)
    {
        if (_explodeCoroutine != null)
            StopCoroutine(_explodeCoroutine);

        SoundManager.ProjectileTriggeredSoundArtificial?.Invoke(transform.position, 6f);

        GameObject VFX = Instantiate(GameManager._instance.GetRandomFromList(GameManager._instance.SparksVFX), transform.position, Quaternion.identity);
        SoundManager._instance.PlaySound(SoundManager._instance.BombExplode, transform.position, 0.1f, false, UnityEngine.Random.Range(2f, 2.5f));
        SoundManager._instance.PlaySound(SoundManager._instance.HitWallWithWeapon, transform.position, 0.3f, false, Random.Range(0.75f, 0.85f));
        GameObject broken = Instantiate(PrefabHolder._instance.L1ExplosiveBroken, transform.position, transform.rotation);
        foreach (Transform item in broken.transform)
        {
            Rigidbody tempRB = item.GetComponent<Rigidbody>();
            tempRB.velocity = _rb.velocity;
            tempRB.AddForce(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * 2.5f);
            tempRB.AddForce((broken.transform.position - other.position).normalized * 6f);
            GameManager._instance.CallForAction(() => { Destroy(tempRB.GetComponent<Collider>()); Destroy(tempRB); }, 3f);
        }

        if (explodingSound != null)
            Destroy(explodingSound);
        GameManager._instance.CallForAction(() => Destroy(VFX), 4f);
        Destroy(transform.parent.gameObject);
    }
}
