using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class TriggerZone : MonoBehaviour
{
    public string interactionTag;

    [Space(10)]
    public UnityEvent onTriggerEnter;
    public UnityEvent onTriggerStay;
    public UnityEvent onTriggerExit;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(interactionTag))
        {
            onTriggerEnter.Invoke();
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(interactionTag))
        {
            onTriggerStay.Invoke();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(interactionTag))
        {
            onTriggerExit.Invoke();
        }
    }

}