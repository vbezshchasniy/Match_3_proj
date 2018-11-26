using System.Collections;
using UnityEngine;

public static class AnimationHelper
{
    public static IEnumerator Move(this Transform t, Vector3 target, float duration)
    {
        Vector3 diffVector = target - t.position;
        float diffLength = diffVector.magnitude;
        diffVector.Normalize();

        float counter = 0f;
        while (duration > counter)
        {
            float k = Time.deltaTime * diffLength / duration;
            t.position += diffVector * k;
            counter += Time.deltaTime;
            yield return null;
        }

        t.position = target;
    }

    public static IEnumerator Scale(this Transform t, Vector3 target, float duration)
    {
        Vector3 diffVector = target - t.localScale;
        float diffLength = diffVector.magnitude;
        diffVector.Normalize();

        float counter = 0f;
        while (duration > counter)
        {
            float k = Time.deltaTime * diffLength / duration;
            t.localScale += diffVector * k;
            counter += Time.deltaTime;
            yield return null;
        }

        t.localScale = target;
    }
}