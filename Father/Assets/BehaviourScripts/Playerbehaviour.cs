using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public float mouseSensitivityYaw = 2f;   // Drehgeschwindigkeit horizontal
    public float mouseSensitivityPitch = 2f; // Drehgeschwindigkeit vertikal
    private float cameraPitch = 0f;          // Aktueller Pitch-Winkel der Kamera
    public float maxPitch = 90f;             // Maximale Neigung nach oben/unten
    public float minPitch = -90f;            // Minimale Neigung nach unten
    public float moveSpeed = 5f; // Bewegungsgeschwindigkeit des Spielers
    //public float sideMoveSpeed = 3f; // Seitliche Bewegungsgeschwindigkeit des Spielers
    public float jumpForce = 5f; // Sprungkraft des Spielers
    public float attackRange = 2f; // Angriffsreichweite des Spielers
    public int attackDamage = 10; // Angriffsschaden des Spielers
    public Transform cameraTransform; // Referenz zur Kamera
    public float CameraHeight = 1.7f; // Höhe der Kamera über dem Boden
    public bool isRunning = false; // Flag, ob der Spieler rennt

    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Sperrt die Rotation um die X- und Z-Achse, um das Umkippen zu verhindern
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;


        // Mauszeiger verstecken und zentrieren
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Move();
        Jump();
        Attack();
        HandleMouseLook();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityYaw;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityPitch;

        // Spieler horizontal drehen (Yaw)
        transform.Rotate(Vector3.up * mouseX);

        // Kamera vertikal drehen (Pitch)
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);

        if (cameraTransform != null)
        {
            // Setze die Kamera-Rotation auf Pitch und Yaw
            cameraTransform.rotation = Quaternion.Euler(cameraPitch, transform.eulerAngles.y, 0);
        }
    }


    void LateUpdate()
    {
        UpdateCameraPosition();
    }

    void Move()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Forward und Right Vektoren der Kamera auf die XZ-Ebene projizieren
        Vector3 camForward = cameraTransform.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = cameraTransform.right;
        camRight.y = 0;
        camRight.Normalize();

        Vector3 movement = (camForward * moveVertical + camRight * moveHorizontal).normalized * moveSpeed * Time.deltaTime;

        // Bewege den Spieler
        rb.MovePosition(transform.position + movement);

        // Entferne die automatische Drehung!
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
            Vector3 headOffset = new Vector3(0, CameraHeight, 0);
            cameraTransform.position = transform.position + headOffset;
            // Die Rotation wird in HandleMouseLook gesetzt
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







