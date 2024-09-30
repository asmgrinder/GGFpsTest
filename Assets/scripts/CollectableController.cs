using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableController : MonoBehaviour
{
    public float RotSpeed = 30;
    [Range(0.1f, 5)] public float FloatPeriod = 2;
    [Range(0.01f, 1)] public float FloatAmplitude = 0.05f;
    [Range(0, 300)] public float HideTime = 15;
    public int Amount;

    float startY;
    Collider col;

    protected void initialize()
    {
        startY = transform.localPosition.y;

        col = GetComponent<Collider>();
    }

    protected void onUpdate()
    {
        float y = startY + FloatAmplitude * Mathf.Sin(Time.time * 2 * Mathf.PI / FloatPeriod);
        transform.localPosition += (y - transform.localPosition.y) * Vector3.up;
        transform.Rotate(Vector3.up, RotSpeed * Time.deltaTime);
    }

    public void Respawn()
    {
        StartCoroutine(respawn());
    }

    IEnumerator respawn()
    {
        col.enabled = false;
        transform.localScale = Vector3.zero;
        if (HideTime > 0)
        {
            yield return new WaitForSeconds(HideTime);
            col.enabled = true;
            transform.localScale = Vector3.one;
        }
        yield return null;
    }
}
