using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button startButton;

    private void Awake()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartClicked);
        }

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public void Show()
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    private void OnStartClicked()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        GameManager.Instance.StartGame();
    }
}
