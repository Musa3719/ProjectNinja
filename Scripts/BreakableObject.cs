using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BreakableObject : MonoBehaviour
{
    [SerializeField]
    private AudioClip _brokenSound;
    [SerializeField]
    private float _volumeMultiplier = 1f;

    public Rigidbody[] _rigidbodies { get; private set; }
    private void Awake()
    {
        _rigidbodies = transform.parent.Find("Broken").GetComponentsInChildren<Rigidbody>();
        foreach (var rb in _rigidbodies)
        {
            Mesh mesh = rb.GetComponent<MeshFilter>().mesh;
            float volume = VolumeOfMesh(mesh);//mesh.bounds.size.x * mesh.bounds.size.y * mesh.bounds.size.z;
            rb.mass = volume * 10000000f;
        }
    }
    public void BrakeObject(Vector3 direction, float force = 100f)
    {
        transform.parent.GetComponent<NavMeshObstacle>().enabled = false;
        SoundManager._instance.PlaySound(_brokenSound, transform.position, 0.6f * _volumeMultiplier, false, Random.Range(0.9f, 1.1f));
        transform.parent.Find("Broken").gameObject.SetActive(true);

        GameObject hitSmoke = GameObject.Instantiate(GameManager._instance.HitSmokeVFX, transform.position + Vector3.up * 1f, Quaternion.identity);
        hitSmoke.transform.localScale *= 6f;
        Color temp = hitSmoke.GetComponentInChildren<SpriteRenderer>().color;
        hitSmoke.GetComponentInChildren<SpriteRenderer>().color = new Color(temp.r, temp.g, temp.b, 6.5f / 255f);
        GameObject.Destroy(hitSmoke, 3f);

        foreach (var brokenRb in _rigidbodies)
        {
            brokenRb.AddForce((direction * (force + Random.Range(0f, 75f)) - Vector3.up * Random.Range(125f, 250f)) * 0.04f);
            GameManager._instance.CallForAction(()=> { brokenRb.isKinematic = true; brokenRb.GetComponent<Collider>().enabled = false; }, 20f);
        }
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.GetComponent<IKillable>() != null)
        {
            if (other.GetComponent<IKillable>()._CollisionVelocity > 13.5f)
            {
                other.GetComponent<IKillable>().HitBreakable(gameObject);
                BrakeObject((transform.position - other.transform.position).normalized, other.GetComponent<Rigidbody>().velocity.magnitude * 40f);
            }
        }
    }


    public float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float v321 = p3.x * p2.y * p1.z;
        float v231 = p2.x * p3.y * p1.z;
        float v312 = p3.x * p1.y * p2.z;
        float v132 = p1.x * p3.y * p2.z;
        float v213 = p2.x * p1.y * p3.z;
        float v123 = p1.x * p2.y * p3.z;

        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }

    public float VolumeOfMesh(Mesh mesh)
    {
        float volume = 0;

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 p1 = vertices[triangles[i + 0]];
            Vector3 p2 = vertices[triangles[i + 1]];
            Vector3 p3 = vertices[triangles[i + 2]];
            volume += SignedVolumeOfTriangle(p1, p2, p3);
        }
        return Mathf.Abs(volume);
    }
}
