using UnityEngine;

public enum PickupType
{
    Ammo,
    Health,
    Key,
}

public class PickupItem : MonoBehaviour
{
    public PickupType pickupType = PickupType.Ammo;
    public int amount = 15;
    public string keyName = "Key"; // for Key type
    public bool destroyOnPickup = true;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        if (pickupType == PickupType.Ammo)
        {
            var shooter = other.GetComponent<PlayerShooting>();
            if (shooter != null)
            {
                shooter.AddAmmo(amount);
                PickupCollected();
            }
        }
        else if (pickupType == PickupType.Health)
        {
            var health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.Heal(amount);
                PickupCollected();
            }
        }
        else if (pickupType == PickupType.Key)
        {
            var inventory = other.GetComponent<Inventory>();
            if (inventory != null)
            {
                inventory.AddKey(keyName);
                PickupCollected();
            }
        }
    }

    void PickupCollected()
    {
        if (destroyOnPickup)
            Destroy(gameObject);
    }
}
