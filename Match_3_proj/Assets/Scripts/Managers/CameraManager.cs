using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public float HorizontalResolution = 1920;

    void OnGUI()
    {
        float currentAspect = (float) Screen.width / (float) Screen.height;
        if (Camera.main != null) 
            Camera.main.orthographicSize = HorizontalResolution / currentAspect / 200;
    }
}