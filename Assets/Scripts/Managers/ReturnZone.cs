using UnityEngine;

public class ReturnZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            RespawnManager respawnManager = Object.FindFirstObjectByType<RespawnManager>();

            if (respawnManager != null)
            {
                respawnManager.Respawn();
            }
        }
    }
}

