using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRButtonLoopSound : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField, Range(0f, 1f)] private float volume = 0.5f;

    public void StartLoopSound(SelectEnterEventArgs args)
    {
        if (audioSource == null)
            return;

        audioSource.volume = volume;
        audioSource.loop = true;

        if (!audioSource.isPlaying)
            audioSource.Play();
    }
}