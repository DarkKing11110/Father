using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public float moveSpeed = 5f; // Bewegungsgeschwindigkeit des Spielers
    //public float sideMoveSpeed = 3f; // Seitliche Bewegungsgeschwindigkeit des Spielers
    public float jumpForce = 5f; // Sprungkraft des Spielers
    public float attackRange = 2f; // Angriffsreichweite des Spielers
    public int attackDamage = 10; // Angriffsschaden des Spielers
    public Transform cameraTransform; // Referenz zur Kamera
    public float cameraDistance = 5f; // Abstand der Kamera zum Spieler
    public float cameraHeight = 2f; // Höhe der Kamera über dem Spieler
    public float cameraFollowSpeed = 2f; // Geschwindigkeit, mit der die Kamera dem Spieler folgt
    public bool isRunning = false; // Flag, ob der Spieler rennt

    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Sperrt die Rotation um die X- und Z-Achse, um das Umkippen zu verhindern
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        Move();
        Jump();
        Attack();
    }

    void LateUpdate()
    {
        UpdateCameraPosition();
    }

    void Move()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Berechne die Bewegungsrichtung relativ zur Blickrichtung der Kamera
        Vector3 forward = cameraTransform.forward * moveVertical;
        Vector3 right = cameraTransform.right * moveHorizontal;
        Vector3 movement = (forward + right).normalized * moveSpeed * Time.deltaTime;

        // Bewege den Spieler
        rb.MovePosition(transform.position + movement);

        // Aktualisiere die Blickrichtung des Spielers nur, wenn er sich vorwärts oder rückwärts bewegt
        if (moveVertical != 0 || moveHorizontal != 0)
        {
            Vector3 direction = new Vector3(movement.x, 0, movement.z);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 0.1f);
        }

       
    }


    void Jump()
    {
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void Attack()
    {
        if (Input.GetMouseButtonDown(1)) // Rechtsklick
        {

            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, attackRange))
            {
                EnemyBehaviour enemy = hit.collider.GetComponent<EnemyBehaviour>();
                if (enemy != null)
                {
                    enemy.TakeDamage(attackDamage);
                }
            }
        }
    }

    void UpdateCameraPosition()
    {
        if (cameraTransform != null)
        {
            Vector3 targetPosition = transform.position - transform.forward * cameraDistance + Vector3.up * cameraHeight;
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, cameraFollowSpeed * Time.deltaTime);
            cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, Quaternion.LookRotation(transform.position - cameraTransform.position), cameraFollowSpeed * Time.deltaTime);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}







