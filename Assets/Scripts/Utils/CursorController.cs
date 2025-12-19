using UnityEngine;

public class CursorController : MonoBehaviour
{
    void Update()
    {
        if (UIManager.instance != null && UIManager.instance.IsPanelActive())
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
