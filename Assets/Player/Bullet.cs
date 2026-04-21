using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 3f;
    public LayerMask hitMask; // optional: what layers bullet can hit
    public GameObject hitEffectPrefab; // optional visual

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // optional: ignore triggers or own player layer as needed
        if (hitMask != 0 && ((1 << other.gameObject.layer) & hitMask) == 0) return;

        // spawn hit effect
        if (hitEffectPrefab != null)
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

        // add damage logic here (if target has health component, etc.)

        Destroy(gameObject);
    }
}
