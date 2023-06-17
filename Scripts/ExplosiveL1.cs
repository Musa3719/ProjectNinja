using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveL1 : MonoBehaviour
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
            _explodeCoroutine = StartCoroutine(Explode());
            return;
        }
        Vector3 dir = (_playerTransform.position - transform.position).normalized;
        float targetVelocityMagnitude= (_playerTransform.position - transform.position).magnitude;
        targetVelocityMagnitude = Mathf.Clamp(4f * Mathf.Log(targetVelocityMagnitude), 0.2f, 13f);
        _rb.velocity = Vector3.Lerp(_rb.velocity, targetVelocityMagnitude * dir, Time.deltaTime * 2f);

        if ((_playerTransform.position - transform.position).magnitude < 2.25f)
        {
            _explodeCoroutine = StartCoroutine(Explode());
        }
    }
    private IEnumerator Explode()
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
        SoundManager._instance.PlaySound(SoundManager._instance.BombExplode, transform.position, 0.3f, false, UnityEngine.Random.Range(1.25f, 1.5f));
        GameObject sparksVFX = Instantiate(GameManager._instance.GetRandomFromList(GameManager._instance.ExplosionVFX), transform.position, Quaternion.identity);

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
            if (hit.collider == null || (!hit.collider.CompareTag("Player") && !hit.collider.CompareTag("BreakableObject") && !hit.collider.CompareTag("Boss") && !hit.collider.CompareTag("Enemy") && !hit.collider.CompareTag("HitBox"))) continue;

            if (hit.collider.CompareTag("BreakableObject"))
            {
                hit.collider.GetComponent<BreakableObject>().BrakeObject((transform.position - hit.collider.transform.position).normalized);
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
                colliderKillable.Die((collider.transform.position - transform.position).normalized, _rb.velocity.magnitude);
            }
        }
        SoundManager.ProjectileTriggeredSoundArtificial?.Invoke(transform.position, 12.5f);

        GameObject broken = Instantiate(PrefabHolder._instance.L1ExplosiveBroken, transform.position, transform.rotation);
        foreach (Transform item in broken.transform)
        {
            Rigidbody tempRB = item.GetComponent<Rigidbody>();
            tempRB.AddForce((item.transform.position - broken.transform.position) * 40f);
            GameManager._instance.CallForAction(() => { tempRB.isKinematic = true; tempRB.GetComponent<Collider>().enabled = false; }, 20f);
        }

        Destroy(explodingSound);
        Destroy(transform.parent.gameObject);
    }
    public void DestroyWithoutExploding(Transform other)
    {
        if (_explodeCoroutine != null)
            StopCoroutine(_explodeCoroutine);

        SoundManager._instance.PlaySound(SoundManager._instance.HitWallWithWeapon, transform.position, 0.3f, false, Random.Range(0.7f, 0.9f));
        GameObject broken = Instantiate(PrefabHolder._instance.L1ExplosiveBroken, transform.position, transform.rotation);
        foreach (Transform item in broken.transform)
        {
            Rigidbody tempRB = item.GetComponent<Rigidbody>();
            tempRB.AddForce((item.transform.position - broken.transform.position) * 10f);
            tempRB.AddForce((broken.transform.position - other.position).normalized / 3f);
            GameManager._instance.CallForAction(() => { tempRB.isKinematic = true; tempRB.GetComponent<Collider>().enabled = false; }, 20f);
        }

        if (explodingSound != null)
            Destroy(explodingSound);
        Destroy(transform.parent.gameObject);
    }
}
