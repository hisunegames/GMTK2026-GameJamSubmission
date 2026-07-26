using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManualPageController : MonoBehaviour
{
    [SerializeField] private List<GameObject> pages;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private Button prevPageButton;

    private int currentPageIndex;

    private int PageCount => pages == null ? 0 : pages.Count;

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

        ShowPage(0);
    }

    public void NextPage()
    {
        Debug.Log($"[ManualPageController] NextPage clicked - currentPageIndex={currentPageIndex}, pageCount={PageCount}");
        ShowPage(currentPageIndex + 1);
    }

    public void PrevPage()
    {
        Debug.Log($"[ManualPageController] PrevPage clicked - currentPageIndex={currentPageIndex}, pageCount={PageCount}");
        ShowPage(currentPageIndex - 1);
    }

    private void ShowPage(int pageIndex)
    {
        int pageCount = PageCount;

        if (pageCount == 0)
        {
            Debug.LogWarning("[ManualPageController] ShowPage aborted - pageCount is 0 (pages list empty or unassigned)");
            return;
        }

        currentPageIndex = Mathf.Clamp(pageIndex, 0, pageCount - 1);

        for (int i = 0; i < pages.Count; i++)
        {
            if (pages[i] != null)
            {
                pages[i].SetActive(i == currentPageIndex);
            }
        }

        if (nextPageButton != null)
        {
            nextPageButton.interactable = currentPageIndex < pageCount - 1;
        }

        if (prevPageButton != null)
        {
            prevPageButton.interactable = currentPageIndex > 0;
        }
    }
}
