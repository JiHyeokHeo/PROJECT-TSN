using TST;
using UnityEngine;

public class Camera2DFrontVisual : MonoBehaviour
{
    [SerializeField] Transform visualObj;
    private Camera mainCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (visualObj == null)
            visualObj = transform;

        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera != null)
            visualObj.rotation = mainCamera.transform.rotation;
    }
}
