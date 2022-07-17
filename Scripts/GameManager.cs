using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class GameManager : MonoBehaviour
{
    public static GameManager _instance;
    public bool isGameStopped { get; private set; }
    public bool isPlayerDead { get; private set; }

    [SerializeField]
    private GameObject StopScreen;
    [SerializeField]
    private GameObject InGameScren;
    [SerializeField]
    private SlicedFilledImage StaminaBar;
    [SerializeField]
    private TextMeshProUGUI SpeedText;

    private Rigidbody playerRb;
    

    private void Awake()
    {
        _instance = this;
        playerRb = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
    }
    private void Update()
    {
        StaminaBar.fillAmount = PlayerMovement._instance._Stamina / 100f;
        SpeedText.text = playerRb.velocity.magnitude.ToString("n0") + " m/s";
        

        if (Input.GetKeyDown(KeyCode.Escape) && !isPlayerDead)
        {
            if (isGameStopped)
            {
                CloseStopScreen();
            }
            else
            {
                OpenStopScreen();
            }
        }
    }
    private void OpenStopScreen()
    {
        StopScreen.SetActive(true);
        InGameScren.SetActive(false);
        Time.timeScale = 0f;
        isGameStopped = true;
    }
    public void CloseStopScreen()
    {
        StopScreen.SetActive(false);
        InGameScren.SetActive(true);
        Time.timeScale = 1f;
        isGameStopped = false;
    }
    public void Die()
    {
        isPlayerDead = true;
        StopScreen.SetActive(false);
        InGameScren.SetActive(false);
        Time.timeScale = Mathf.Lerp(Time.timeScale, 0.1f, Time.deltaTime * 15f);
    }
}
