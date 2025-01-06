using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // El carro a seguir
    public float smoothSpeed = 5f; // Velocidad de suavizado
    public Vector3 offset = new Vector3(0, 0, -10); // Offset de la cámara (ajusta el -10 según tu configuración)
    
    private Vector3 desiredPosition;
    private Vector3 smoothedPosition;
    
    private void FixedUpdate()
    {
        if (target == null)
            return;
            
        // Calcula la posición deseada
        desiredPosition = target.position + offset;
        
        // Suaviza el movimiento usando Lerp
        smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.fixedDeltaTime);
        
        // Actualiza la posición de la cámara
        transform.position = smoothedPosition;
    }
} 