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
        ClampToCanvas();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
    }

    private void ClampToCanvas()
    {
        if (canvas == null)
        {
            return;
        }

        RectTransform canvasRect = canvas.transform as RectTransform;
        if (canvasRect == null)
        {
            return;
        }

        Vector3[] canvasCorners = new Vector3[4];
        canvasRect.GetWorldCorners(canvasCorners);

        Vector3[] targetCorners = new Vector3[4];
        target.GetWorldCorners(targetCorners);

        Vector3 offset = Vector3.zero;

        if (targetCorners[0].x < canvasCorners[0].x)
        {
            offset.x += canvasCorners[0].x - targetCorners[0].x;
        }
        else if (targetCorners[2].x > canvasCorners[2].x)
        {
            offset.x += canvasCorners[2].x - targetCorners[2].x;
        }

        if (targetCorners[0].y < canvasCorners[0].y)
        {
            offset.y += canvasCorners[0].y - targetCorners[0].y;
        }
        else if (targetCorners[2].y > canvasCorners[2].y)
        {
            offset.y += canvasCorners[2].y - targetCorners[2].y;
        }

        target.position += offset;
    }
}
