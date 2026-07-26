using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private bool enablePauseMenu = false;
    [SerializeField] private GameObject panel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button returnToMenuButton;
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape;

    private bool isPaused;

    private void Awake()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(Resume);
        }

        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);
        }

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private void Update()
    {
        if (enablePauseMenu && Input.GetKeyDown(toggleKey))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    private void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;

        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    private void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private void OnReturnToMenuClicked()
    {
        Resume();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }
    }
}
