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
        SceneController._instance.LoadNextSceneAsync();
        GetComponent<Newspaper>().OpenNewspaper();
        SoundManager._instance.PlaySound(SoundManager._instance.RoomPassed, transform.position, 0.5f, false, Random.Range(0.9f, 1.1f));
    }
    
    private void ToNextScene()//escape or UI button
    {
        Time.timeScale = 0.05f;
        if (SceneController.NextSceneAsyncOperation != null)
            SceneController.NextSceneAsyncOperation.allowSceneActivation = true;
    }
    private void Update()
    {
        if (_isInInteract && InputHandler.GetButtonDown("Esc"))
        {
            ToNextScene();
        }
    }
}
