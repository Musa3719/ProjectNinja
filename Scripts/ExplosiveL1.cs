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
    private void Update()
    {
        if (GameManager._instance.isGameStopped || GameManager._instance.isPlayerDead || GameManager._instance.isOnCutscene) return;

        if ((_playerTransform.position - transform.position).magnitude < 10f) _isTriggered = true;

        if (_isAboutToExplode || !_isTriggered) return;

        if (_rb.velocity.magnitude < 0.1f)
        {
            Vector3 direction = (_playerTransform.position - transform.position).normalized;
            _rb.velocity = direction;
        }

        timer += Time.deltaTime;
        if (timer > 6.5f)
        {
            _isAboutToExplode = true;
            _explodeCoroutine = StartCoroutine(Explode());
            return;
        }
        Vector3 dir = (_playerTransform.position - transform.position).normalized;
        float targetVelocityMagnitude= (_playerTransform.position - transform.position).magnitude;
        targetVelocityMagnitude = Mathf.Clamp(6f * Mathf.Log(targetVelocityMagnitude), 0.2f, 20f);
        _rb.velocity = Vector3.Lerp(_rb.velocity, targetVelocityMagnitude * dir, Time.deltaTime * 4f);

        if ((_playerTransform.position - transform.position).magnitude < 2.5f)
        {
            _isAboutToExplode = true;
            _explodeCoroutine = StartCoroutine(Explode());
        }
    }
    private IEnumerator Explode()
    {
        GetComponent<Animator>().SetTrigger("Exploding");
        GameObject explodingSound = SoundManager._instance.PlaySound(SoundManager._instance.ReadyForExplosion, transform.position, 0.2f, false, 1f);
        float waitTime = Random.Range(1f, 2f);
        float startTime = Time.time;
        AudioSource explodingSource = explodingSound.GetComponent<AudioSource>();
        while (startTime + waitTime > Time.time)
        {
            explodingSource.pitch = Mathf.Lerp(0.75f, 2.5f, (Time.time - startTime) / waitTime);
            _rb.velocity = Vector3.Lerp(_rb.velocity, Vector3.zero, Time.deltaTime * 10f);
            yield return null;
        }
        SoundManager._instance.PlaySound(SoundManager._instance.BombExplode, transform.position, 0.15f, false, UnityEngine.Random.Range(1.25f, 1.5f));
        GameObject sparksVFX = Instantiate(GameManager._instance.GetRandomFromList(GameManager._instance.ExplosionVFX), transform.position, Quaternion.identity);

        float bombRange = 5f;
        Collider[] sphereCastColliders = Physics.OverlapSphere(transform.position, bombRange);
        foreach (var collider in sphereCastColliders)
        {
            Vector3 direction = (collider.transform.position - transform.position).normalized;
            RaycastHit hit;
            Physics.Raycast(transform.position, direction, out hit, bombRange);

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
    private void OnTriggerEnter(Collider other)
    {
        if(other.name == "AttackCollider")
        {
            if (_explodeCoroutine != null)
                StopCoroutine(_explodeCoroutine);
            //destroyed vfx and sound
            other.transform.parent.GetComponent<IKillable>().BombDeflected();
            Destroy(gameObject);
        }
    }
}
