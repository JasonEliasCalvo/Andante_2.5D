using UnityEngine;
using UnityEngine.Events;
public class PickupItem : MonoBehaviour
{
    public Item item;
    public UnityEvent PickupEvent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (InventoryManager.Instance.AddItem(item))
            {
                PickupEvent?.Invoke();
                Destroy(gameObject);
            }
        }
    }

    public void DebugS()
    {
        Debug.Log("evento invocado");
    }
}
