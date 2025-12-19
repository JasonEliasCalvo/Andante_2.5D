using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAnimatorController : MonoBehaviour
{
    public static UIAnimatorController instance;

    public GameObject curPanel;
    public GameObject loginPanel;
    public GameObject welcomePanel;

    public TextMeshProUGUI welcomeText;

    public TMP_InputField userNameInputField;
    public string userName;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.H))
        {
            ShowPanel(curPanel);
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            HidenPanel(curPanel);
        }
    }

    public void LoadData()
    {
        userName = userNameInputField.text;
        welcomeText.text = "Hola " + userName + " Gracias por elegir nuestro juego";
        loginPanel.SetActive(false);
        welcomePanel.SetActive(true);
    }

    public void ShowPanel(GameObject _panel)
    {
        //_panel.transform.localScale = Vector3.zero;
        //_panel.transform.DOScale(1, 3f);
        _panel.transform.DOShakePosition(1, 200, 90);
    }
    public void HidenPanel(GameObject _panel)
    {
        _panel.transform.localScale = Vector3.one;
        _panel.transform.DOScale(0, 3f);
    }
}

