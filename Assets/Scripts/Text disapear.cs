using UnityEngine;

public class ShowTextAtStart : MonoBehaviour
{
    [SerializeField] private GameObject textObject;
    [SerializeField] private float duration = 3f;

    void Start()
    {
        textObject.SetActive(true);
        Invoke(nameof(HideText), duration);
    }

    void HideText()
    {
        textObject.SetActive(false);
    }
}