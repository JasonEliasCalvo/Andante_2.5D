using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class DroppableZone : MonoBehaviour
{
    public string interactionTag;
    public string requiredItemName;
    public UnityEvent onCorrectItemDropped;

    [Space(20)]

    Transform objective;
    public float rango;
    float rangoAlCuadrado;
    public LayerMask detectionMask;
    public float radiusDetection;
    [SerializeField] bool isPlayerInTrigger = false;
    [SerializeField] bool isPlayerNear = false;

    private void OnEnable()
    {
        UIInputHandler.OnRemoveItemPressed += OnRemoveItemPressed;
    }

    private void OnDisable()
    {
        UIInputHandler.OnRemoveItemPressed -= OnRemoveItemPressed;
    }

    private void Start()
    {
        objective = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        Item currentItem = InventoryManager.Instance.slots[InventoryManager.Instance.selectedSlotIndex].currentItem;
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

                if (isPlayerInTrigger && IsPlayerDetected())
                {
                    RevisarSoltarItem();
                }
                else if (IsPlayerDetected() && !isPlayerInTrigger)
                {
                    Debug.Log("Entró el player");
                    isPlayerInTrigger = true;
                    RevisarSoltarItem();
                }
                break;

            case false:
                if (isPlayerInTrigger)
                {
                    Debug.Log("Salió el player");
                    UIManager.instance.ShowRemoveItemPanel(false);
                    UIManager.instance.ShowHintPanel(false);
                    isPlayerInTrigger = false;
                }
                break;
        }
    }

    private void OnRemoveItemPressed()
    {
        if (!isPlayerInTrigger) return;

        Item currentItem = InventoryManager.Instance.slots[InventoryManager.Instance.selectedSlotIndex].currentItem;

        if (currentItem != null && currentItem.itemName == requiredItemName)
        {
            InventoryManager.Instance.RemoveItem();
            UIManager.instance.ShowRemoveItemPanel(false);
            UIManager.instance.ShowHintPanel(false);
            onCorrectItemDropped?.Invoke();
        }
        else
        {
            Debug.Log("Este objeto no se puede soltar aquí.");
        }
    }

    bool IsPlayerDetected()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radiusDetection, detectionMask);

        if (hits.Length > 0)
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

    private void RevisarSoltarItem()
    {
        Item currentItem = InventoryManager.Instance.slots[InventoryManager.Instance.selectedSlotIndex].currentItem;

        if (currentItem != null && currentItem.itemName == requiredItemName)
        {
            UIManager.instance.ShowRemoveItemPanel(true);
            UIManager.instance.ShowHintPanel(false);
        }
        else
        {
            UIManager.instance.ShowRemoveItemPanel(false);
            UIManager.instance.ShowHintPanel(true, $"Necesitas: {requiredItemName}");
        }
    }

    public bool IsPlayerNear() => isPlayerNear;
    public bool IsPlayerInTrigger() => isPlayerInTrigger;
}
