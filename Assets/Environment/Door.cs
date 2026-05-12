using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    public string requiredKey = "Key";
    public bool isOpen = false;
    public GameObject doorObject; // the visual door to disable
    public Collider2D doorCollider; // the collider to disable

    [Header("Animation")]
    public float openSpeed = 2f;
    public Vector3 openOffset = new Vector3(0, -2, 0); // move down to "open"

    Vector3 closedPosition;
    Vector3 openPosition;
    bool isAnimating = false;

    void Start()
    {
        if (doorObject == null)
            doorObject = gameObject;

        closedPosition = doorObject.transform.position;
        openPosition = closedPosition + openOffset;

        if (doorCollider == null)
            doorCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (isAnimating)
        {
            doorObject.transform.position = Vector3.MoveTowards(
                doorObject.transform.position,
                openPosition,
                openSpeed * Time.deltaTime
            );

            if (Vector3.Distance(doorObject.transform.position, openPosition) < 0.01f)
            {
                isAnimating = false;
                doorObject.SetActive(false); // fully hide after animation
            }
        }
    }

    public void TryOpen()
    {
        if (isOpen) return;

        if (Inventory.Instance != null && Inventory.Instance.HasKey(requiredKey))
        {
            OpenDoor();
        }
        else
        {
            Debug.Log($"[Door] Need key '{requiredKey}' to open this door!");
        }
    }

    void OpenDoor()
    {
        isOpen = true;
        isAnimating = true;

        if (doorCollider != null)
            doorCollider.enabled = false;

        Debug.Log($"[Door] Door opened with key '{requiredKey}'!");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TryOpen();
        }
    }
}