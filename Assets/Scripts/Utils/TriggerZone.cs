using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class TriggerZone : MonoBehaviour
{
    public string interactionTag;
    public InteractableOptions interactableOptions;
    [Space(20)]

    Transform objective;
    public float rango;
    float rangoAlCuadrado;
    RaycastHit hit;
    public LayerMask detectionMask;
    public float radiusDetection;
    [SerializeField] bool isPlayerInTrigger = false;
    [SerializeField] bool isPlayerNear = false;
    public UnityEvent onTriggerEnter;
    public UnityEvent onTriggerStay;
    public UnityEvent onTriggerExit;

    private void Start()
    {
        objective = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        if (!interactableOptions.possibleInteract) return;

        rangoAlCuadrado = rango * rango;

        if ((transform.position - objective.position).sqrMagnitude < rangoAlCuadrado)
        {
            isPlayerNear = true;
        }
        else
        {
            isPlayerNear = false;
        }

        switch (isPlayerNear)
        {
            case true:

                if (IsPlayerDetected() && !isPlayerInTrigger)
                {
                    GameManager.instance.HidePanels();
                    onTriggerEnter.Invoke();
                    isPlayerInTrigger = true;
                }
                else if (isPlayerInTrigger && IsPlayerDetected())
                {
                    onTriggerStay.Invoke();
                }
                break;

            case false:

                if (isPlayerInTrigger)
                {
                    GameManager.instance.HidePanels();
                    onTriggerExit.Invoke();
                    isPlayerInTrigger = false;
                }
                break;
        }
    }

    bool IsPlayerDetected()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radiusDetection, detectionMask);

        if (hits.Count() > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radiusDetection);
    }

    public bool IsPlayerNear() => isPlayerNear;
    public bool IsPlayerInTrigger() => isPlayerInTrigger;

}