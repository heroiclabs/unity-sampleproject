using UnityEngine;

public class CameraDepthTextureMode : MonoBehaviour 
{
    [SerializeField] private DepthTextureMode depthTextureMode = DepthTextureMode.None;

    private void OnValidate()
    {
        SetCameraDepthTextureMode();
    }

    private void Awake()
    {
        SetCameraDepthTextureMode();
    }

    private void SetCameraDepthTextureMode()
    {
        GetComponent<Camera>().depthTextureMode = depthTextureMode;
    }
}
