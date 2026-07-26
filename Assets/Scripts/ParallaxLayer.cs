using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Vector2 parallaxFactor = new Vector2(0.5f, 0.5f);

    private Vector3 lastCameraPosition;

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera != null)
        {
            lastCameraPosition = targetCamera.transform.position;
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            return;
        }

        Vector3 cameraPosition = targetCamera.transform.position;
        Vector3 delta = cameraPosition - lastCameraPosition;

        transform.position += new Vector3(delta.x * parallaxFactor.x, delta.y * parallaxFactor.y, 0f);
        lastCameraPosition = cameraPosition;
    }
}
