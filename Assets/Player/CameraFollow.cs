using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);
    
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        
        // Автоматически находим игрока, если не указан
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }

    private void LateUpdate()
    {
        if (playerTransform == null)
            return;

        // Целевая позиция камеры с учётом смещения
        Vector3 targetPosition = playerTransform.position + offset;
        
        // Плавное движение камеры
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}
