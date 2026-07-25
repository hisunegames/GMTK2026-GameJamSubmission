using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform target;
    [SerializeField] private Canvas canvas;

    private void Awake()
    {
        if (target == null)
        {
            target = transform as RectTransform;
        }

        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        target.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        float scale = canvas != null ? canvas.scaleFactor : 1f;
        target.anchoredPosition += eventData.delta / scale;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
    }
}
