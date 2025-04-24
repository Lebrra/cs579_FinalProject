using System.Collections;
using UnityEngine;

public class Float : MonoBehaviour
{
    [SerializeField]
    private float hoverSpeed = 1F;
    [SerializeField]
    private float hoverDist = 1F;
    Coroutine hoverRoutine = null;

    private void OnEnable()
    {
        if (hoverRoutine != null)
        {
            StopCoroutine(hoverRoutine);
        }

        hoverRoutine = StartCoroutine(Hover());
    }

    private void OnDisable()
    {
        if (hoverRoutine != null)
        {
            StopCoroutine(hoverRoutine);
        }
    }

    IEnumerator Hover()
    {
        float startY = transform.position.y;
        float time = 0F;
        while (true)
        {
            time = (time + Time.deltaTime) % hoverSpeed;
            float current = time / hoverSpeed;  // %
            float sinOffset = Mathf.Sin(current * 2 * Mathf.PI);

            transform.position = new Vector3(transform.position.x, startY + (sinOffset * hoverDist), transform.position.z);
            yield return null;
        }
    }
}
