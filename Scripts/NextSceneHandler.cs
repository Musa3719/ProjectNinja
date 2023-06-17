using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextSceneHandler : MonoBehaviour
{
    public bool _isInInteract { get; private set; }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null && collision.collider != null && collision.collider.CompareTag("Player"))
            Interact();
    }
    public void Interact()
    {
        _isInInteract = true;
        GameManager._instance.isGameStopped = true;
        Time.timeScale = 0f;
        GetComponent<Newspaper>().OpenNewspaper();
        SoundManager._instance.PlaySound(SoundManager._instance.RoomPassed, transform.position, 0.1f, false, Random.Range(0.9f, 1.1f));
    }
    private void Update()
    {
        if (_isInInteract && InputHandler.GetButtonDown("Esc"))
        {
            SceneController._instance.NextScene();
        }
    }
}
