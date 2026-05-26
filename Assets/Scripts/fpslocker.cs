using UnityEngine;

public class fpslocker : MonoBehaviour
{
    [SerializeField] private int _TargetFrameRate = 60;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FPSLock(_TargetFrameRate);
    }

    private void FPSLock(int frame)
    {
        // Turn off vsync
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = frame;
    }
    
}
