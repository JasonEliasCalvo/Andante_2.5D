using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeControllerText : MonoBehaviour
{
    TextMeshProUGUI textMeshPro;
    [SerializeField]
    TimeController time;
    [SerializeField]
    private string formato;

    private void Start()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        time.eventTimeModified += UpdateTimetext;
    }

    public void UpdateTimetext(float newTime)
    {
        textMeshPro.text = newTime.ToString(formato);
    }
}
