using UnityEngine;

public class XRShowTarget : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private GameObject target2;

    public void ShowTarget()
    {
        if (target != null)
            target.SetActive(true);
        if (target2 != null)
            target2.SetActive(true);
    }
}