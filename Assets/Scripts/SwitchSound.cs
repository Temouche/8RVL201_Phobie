using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRButtonSound : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;

    public void PlaySound(SelectEnterEventArgs args)
    {
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
}