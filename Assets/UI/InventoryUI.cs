using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public TMP_Text keysText;

    void Update()
    {
        if (Inventory.Instance != null && keysText != null)
        {
            keysText.text = $"Keys: {Inventory.Instance.keys.Count}";
        }
    }
}