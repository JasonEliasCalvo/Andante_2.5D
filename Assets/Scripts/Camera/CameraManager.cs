using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;
    public GameObject basicCam;
    public GameObject topDownCam;

    public Camera manualCamera;
    public CinemachineCamera cinematicCam;

    public enum CameraStyle
    {
        Basic,
        Topdown,
        Cinematic
    }

    public CameraStyle currentStyle;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        manualCamera.enabled = true;
        cinematicCam.gameObject.SetActive(false);
        SwitchCameraStyle(currentStyle);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchCameraStyle(CameraStyle.Basic);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchCameraStyle(CameraStyle.Topdown);
    }

    public void SwitchCameraStyle(CameraStyle newStyle)
    {
        basicCam.SetActive(false);
        topDownCam.SetActive(false);
        cinematicCam.gameObject.SetActive(false);

        if (newStyle == CameraStyle.Basic) basicCam.SetActive(true);
        if (newStyle == CameraStyle.Topdown) topDownCam.SetActive(true);
        if (newStyle == CameraStyle.Cinematic) cinematicCam.gameObject.SetActive(true);

        currentStyle = newStyle;
    }

    public void PlayCinematic()
    {
        manualCamera.enabled = false;
        cinematicCam.gameObject.SetActive(true);
    }

    public void EndCinematic()
    {
        cinematicCam.gameObject.SetActive(false);
        manualCamera.enabled = true;
    }
}
