using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollForWeapon : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> _weapons;

    public List<GameObject> _Weapons => _weapons;
    public void SeperateWeaponsFromRagdoll(Vector3 tempDir, float forceMultiplier, float forceUpMultiplier, float killersVelocityMagnitude)
    {
        foreach (var weapon in _weapons)
        {
            weapon.transform.parent = null;
            if (weapon.transform.Find("AttackCollider") != null)
                weapon.transform.Find("AttackCollider").GetComponent<Collider>().enabled = false;
            if (weapon.transform.Find("AttackColliderWarning") != null)
                weapon.transform.Find("AttackColliderWarning").GetComponent<Collider>().enabled = false;

            PlaySoundOnCollision weaponMeshPlaySound = weapon.GetComponentInChildren<PlaySoundOnCollision>();
            weaponMeshPlaySound._soundClip = SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.WeaponHitSounds);
            weaponMeshPlaySound._isEnabled = true;

            GameObject weaponMesh = weaponMeshPlaySound.gameObject;
            if (weaponMesh.name == "Katana(Clone)")
            {
                weaponMesh.GetComponent<BoxCollider>().enabled = true;
            }
            else
            {
                BoxCollider col = weaponMesh.AddComponent(typeof(BoxCollider)) as BoxCollider;
                BoxCollider col2 = weaponMesh.gameObject.AddComponent(typeof(BoxCollider)) as BoxCollider;

                if (col.size.x > col.size.y && col.size.x > col.size.z)
                {
                    col.size = new Vector3(col.size.x / 2f, col.size.y, col.size.z);
                    col2.size = new Vector3(col2.size.x / 2f, col2.size.y, col2.size.z);

                    col.center = new Vector3(col.center.x - 1f / weapon.transform.localScale.x / 2f, 0f, 0f);
                    col2.center = new Vector3(col2.center.x + 1f / weapon.transform.localScale.x / 2f, 0f, 0f);
                }
                else if (col.size.y > col.size.x && col.size.y > col.size.z)
                {
                    col.size = new Vector3(col.size.x, col.size.y / 2f, col.size.z);
                    col2.size = new Vector3(col2.size.x, col2.size.y / 2f, col2.size.z);

                    col.center = new Vector3(0f, col.center.y - 1f / weapon.transform.localScale.y / 2f, 0f);
                    col2.center = new Vector3(0f, col2.center.y + 1f / weapon.transform.localScale.y / 2f, 0f);
                }
                else
                {
                    col.size = new Vector3(col.size.x, col.size.y, col.size.z / 2f);
                    col2.size = new Vector3(col2.size.x, col2.size.y, col2.size.z / 2f);

                    col.center = new Vector3(0f, 0f, col.center.z - 1f / weapon.transform.localScale.z / 2f);
                    col2.center = new Vector3(0f, 0f, col2.center.z + 1f / weapon.transform.localScale.z / 2f);
                }


                col.size *= 0.9f;
                col2.size *= 0.9f;

            }


            Rigidbody rb = weapon.AddComponent(typeof(Rigidbody)) as Rigidbody;
            rb.mass = 18f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            //rb.AddForce(tempDir * forceMultiplier * 1.5f + Vector3.up * forceUpMultiplier * 1.2f);
            rb.AddForce((tempDir * killersVelocityMagnitude * forceMultiplier / 50f + tempDir * forceMultiplier + Vector3.up * forceUpMultiplier) * 4f);
        }
    }
}
