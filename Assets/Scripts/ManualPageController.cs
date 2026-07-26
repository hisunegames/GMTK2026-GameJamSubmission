using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManualPageController : MonoBehaviour
{
    [SerializeField] private List<GameObject> pages;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private Button prevPageButton;

    private int currentSpreadIndex;

    private int SpreadCount => pages == null || pages.Count == 0 ? 0 : Mathf.CeilToInt(pages.Count / 2f);

    private void Start()
    {
        if (nextPageButton != null)
        {
            nextPageButton.onClick.AddListener(NextPage);
        }

        if (prevPageButton != null)
        {
            prevPageButton.onClick.AddListener(PrevPage);
        }

        ShowSpread(0);
    }

    public void NextPage()
    {
        Debug.Log($"[ManualPageController] NextPage clicked - currentSpreadIndex={currentSpreadIndex}, spreadCount={SpreadCount}, pagesCount={(pages != null ? pages.Count : -1)}");
        ShowSpread(currentSpreadIndex + 1);
    }

    public void PrevPage()
    {
        Debug.Log($"[ManualPageController] PrevPage clicked - currentSpreadIndex={currentSpreadIndex}, spreadCount={SpreadCount}");
        ShowSpread(currentSpreadIndex - 1);
    }

    private void ShowSpread(int spreadIndex)
    {
        int spreadCount = SpreadCount;

        if (spreadCount == 0)
        {
            Debug.LogWarning("[ManualPageController] ShowSpread aborted - spreadCount is 0 (pages list empty or unassigned)");
            return;
        }

        currentSpreadIndex = Mathf.Clamp(spreadIndex, 0, spreadCount - 1);
        int firstPageIndex = currentSpreadIndex * 2;

        for (int i = 0; i < pages.Count; i++)
        {
            if (pages[i] != null)
            {
                pages[i].SetActive(i == firstPageIndex || i == firstPageIndex + 1);
            }
        }

        if (nextPageButton != null)
        {
            nextPageButton.interactable = currentSpreadIndex < spreadCount - 1;
        }

        if (prevPageButton != null)
        {
            prevPageButton.interactable = currentSpreadIndex > 0;
        }
    }
}
