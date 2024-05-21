using UnityEngine;

public class TempPlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float sensitivity = 2f;
    
    private float verticalLookRotation = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // Поворот игрока с помощью мыши
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;
        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
        transform.Rotate(Vector3.up * mouseX);
        transform.localEulerAngles = new Vector3(verticalLookRotation, transform.localEulerAngles.y, 0);
        
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 moveDirectionX = new Vector3(horizontalInput, 0, verticalInput).normalized;
        
        // Поворот игрока в направлении камеры
        if (moveDirectionX == Vector3.zero)
            return;

        // Движение игрока в направлении взгляда
        float x = transform.forward.x;
        float z = transform.forward.z;
        Vector3 moveDirection = new Vector3(x, 0, z).normalized;

        if (verticalInput < 0.0f)
            moveDirection *= -1;
        
        transform.Translate(moveDirection * (moveSpeed * Time.deltaTime), Space.World);
    }
}