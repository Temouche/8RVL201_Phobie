using UnityEngine;

public class XRHideTarget : MonoBehaviour
{
    [SerializeField] private GameObject target;

    public void HideTarget()
    {
        if (target != null)
            target.SetActive(false);
    }
}