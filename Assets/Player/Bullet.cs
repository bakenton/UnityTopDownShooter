using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 3f;
    public int damage = 25;
    public LayerMask hitMask; // optional: what layers bullet can hit
    public GameObject hitEffectPrefab; // optional visual
    public Sprite hitSprite;
    public float hitSpriteDuration = 0.08f;
    public float hitSpriteScale = 0.18f;
    [Header("Hit Sounds")]
    public AudioClip[] hitSounds;
    [Range(0f, 1f)] public float hitSoundVolume = 1f;

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
        if (other.isTrigger) return;
        if (other.transform == transform) return;

        var enemy = other.GetComponent<Enemy>();
        if (enemy == null)
        {
            var boss = other.GetComponent<BossEnemy>();
            if (boss == null)
            {
                return;
            }

            if (hitMask != 0 && ((1 << other.gameObject.layer) & hitMask) == 0)
                return;

            // spawn hit effect
            if (hitEffectPrefab != null)
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

            SpawnHitSprite();
            PlayRandomHitSound();
            boss.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (hitMask != 0 && ((1 << other.gameObject.layer) & hitMask) == 0)
            return;

        // spawn hit effect
        if (hitEffectPrefab != null)
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

        SpawnHitSprite();
        PlayRandomHitSound();
        enemy.TakeDamage(damage);
        Destroy(gameObject);
    }

    private void SpawnHitSprite()
    {
        var hitObject = new GameObject("HitSprite");
        hitObject.transform.position = transform.position;
        hitObject.transform.rotation = Quaternion.identity;
        hitObject.transform.localScale = Vector3.one * hitSpriteScale;

        var spriteRenderer = hitObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = hitSprite != null ? hitSprite : CreateHitSprite();
        spriteRenderer.color = new Color(1f, 0.9f, 0.2f, 0.95f);
        spriteRenderer.sortingOrder = 90;

        var effect = hitObject.AddComponent<HitSpriteEffect>();
        effect.Initialize(spriteRenderer, hitSpriteScale, hitSpriteDuration);
    }

    private void PlayRandomHitSound()
    {
        if (hitSounds == null || hitSounds.Length == 0)
            return;

        var clip = hitSounds[Random.Range(0, hitSounds.Length)];
        if (clip == null)
            return;

        AudioSource.PlayClipAtPoint(clip, transform.position, hitSoundVolume);
    }

    private Sprite CreateHitSprite()
    {
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.SetPixels(new[]
        {
            Color.white, Color.white,
            Color.white, Color.white
        });
        texture.Apply();

        return Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f), 100f);
    }

}

public class HitSpriteEffect : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private float startScale;
    private float duration;
    private float elapsed;

    public void Initialize(SpriteRenderer renderer, float scale, float lifeTime)
    {
        spriteRenderer = renderer;
        startScale = scale;
        duration = lifeTime;
        elapsed = 0f;
    }

    void Update()
    {
        if (spriteRenderer == null)
        {
            Destroy(gameObject);
            return;
        }

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, duration));

        float alpha = Mathf.Lerp(0.95f, 0f, t);
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);

        float scale = Mathf.Lerp(startScale, startScale * 1.3f, t);
        transform.localScale = Vector3.one * scale;

        if (t >= 1f)
            Destroy(gameObject);
    }
}
