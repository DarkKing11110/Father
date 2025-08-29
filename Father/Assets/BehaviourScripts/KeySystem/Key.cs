using UnityEngine;

public class Key : MonoBehaviour
{
    public string keyID = "DoorA"; // Eindeutige ID für die Tür

    private bool playerInRange = false;
    private Transform player;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && PlayerIsLookingAtKey())
        {
            PlayerKeyInventory inventory = player.GetComponent<PlayerKeyInventory>();
            if (inventory != null)
            {
                inventory.AddKey(keyID);
                Destroy(gameObject); // Schlüssel aufnehmen und entfernen
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            player = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
        }
    }

    private bool PlayerIsLookingAtKey()
    {
        if (player == null) return false;

        // Raycast von Spieler-Kamera (oder Spieler-Position) zur Key-Position
        Vector3 origin = player.position + Vector3.up * 1.7f; // Augenhöhe
        Vector3 direction = (transform.position - origin).normalized;

        // Optional: Passe den Sichtwinkel an
        float maxAngle = 30f;
        Vector3 playerForward = player.forward;
        float angle = Vector3.Angle(playerForward, direction);

        // Prüfe, ob der Schlüssel im Sichtfeld ist und kein Hindernis dazwischen
        if (angle <= maxAngle)
        {
            Ray ray = new Ray(origin, direction);
            if (Physics.Raycast(ray, out RaycastHit hit, 3f))
            {
                return hit.transform == transform;
            }
        }
        return false;
    }
}
