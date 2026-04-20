using System.Collections;
using UnityEngine;

public class MoveAtoB : MonoBehaviour
{
    [Header("Points")]
    public Transform pointA;
    public Transform pointB;

    [Header("Timing")]
    public float duration = 2f;

    [Header("Options")]
    public bool startAtA = true;
    public bool playOnStart = true;

    private Coroutine moveRoutine;

    void Start()
    {
        if (startAtA && pointA != null)
            transform.position = pointA.position;

        if (playOnStart)
            MoveToB();
    }

    public void MoveToB()
    {
        StartMove(pointA.position, pointB.position);
    }

    public void MoveToA()
    {
        StartMove(pointB.position, pointA.position);
    }

    public void StartMove(Vector3 from, Vector3 to)
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveOverTime(from, to, duration));
    }

    private IEnumerator MoveOverTime(Vector3 from, Vector3 to, float time)
    {
        float elapsed = 0f;
        transform.position = from;

        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / time);
            transform.position = Vector3.Lerp(from, to, t);
            yield return null;
        }

        transform.position = to;
        moveRoutine = null;
        MoveToB();
    }
}