using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FinalZonaTrigger : MonoBehaviour
{
    public GameObject mensajeUI; 

    private bool yaActivado = false;

    void Start()
    {
        if (mensajeUI != null)
            mensajeUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!yaActivado && other.CompareTag("Player"))
        {
            yaActivado = true;
            if (mensajeUI != null)
                mensajeUI.SetActive(true);
        }
    }

    public void IrAlMenu()
    {
        SceneManager.LoadScene(0);
    }
}

