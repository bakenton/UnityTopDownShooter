using UnityEngine;
using UnityEngine.UI;

public class ButtonClickSound : MonoBehaviour
{
    public AudioClip clickSound;
    [Range(0f, 1f)] public float volume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    private void Start()
    {
        var button = GetComponent<Button>();
        if (button == null)
            button = GetComponentInChildren<Button>();

        if (button != null)
        {
            button.onClick.RemoveListener(PlayClickSound);
            button.onClick.AddListener(PlayClickSound);
        }
    }

    public void PlayClickSound()
    {
        if (clickSound == null)
            return;

        if (audioSource != null)
        {
            audioSource.PlayOneShot(clickSound, volume);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clickSound, Camera.main != null ? Camera.main.transform.position : transform.position, volume);
        }
    }
}