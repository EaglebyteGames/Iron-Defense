using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] LineRenderer _Line;
    private float _Step = 0.1f;
    [SerializeField] private Transform _FirePoint;
    [SerializeField] private Transform TargetPoint;
    [SerializeField] private Camera _cam;

    [SerializeField] private bool controlled;
    [SerializeField] private float maxHeight;

    private void Update()
    {
        Vector3 direction = TargetPoint.position - _FirePoint.position;
        Vector3 graoundDirection = new Vector3(direction.x, 0, direction.z);
        Vector3 targetPos = new Vector3(graoundDirection.magnitude, direction.y, 0);
        float height = targetPos.y + targetPos.magnitude / 2f;
        height = Mathf.Max(0.001f, maxHeight);
        float angle;
        float v0;
        float time;
        CalculatePathWithHeight(targetPos, height, out v0, out angle, out time);
        DrawPath(graoundDirection.normalized, v0, angle, time, _Step);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StopAllCoroutines();
            StartCoroutine(Coroutine_Movement(graoundDirection.normalized, v0, angle, time));
        }
    }

    private void DrawPath(Vector3 direction,float v0, float angle, float time, float step)
    {
        step = Mathf.Max(0.01f, step);
        _Line.positionCount = (int)(time / step) + 2;
        int count = 0;
        for (float i = 0; i < time; i+= step) 
        {
            float x = v0 * i * Mathf.Cos(angle);
            float y = v0 * i * Mathf.Sin(angle) - 0.5f * -Physics.gravity.y * Mathf.Pow(i, 2);
            _Line.SetPosition(count,_FirePoint.position + direction * x + Vector3.up * y);
            count++;
        }
        float xfinal = v0 * time * Mathf.Cos(angle);
        float yfinal = v0 * time * Mathf.Sin(angle) - 0.5f * -Physics.gravity.y * Mathf.Pow(time, 2);
        _Line.SetPosition(count, _FirePoint.position + direction*xfinal +Vector3.up * yfinal);
    }
    private float QuadraticEquation(float a, float b, float c, float sign)
    {
        return (-b + sign * Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a);
    }

    private void CalculatePathWithHeight(Vector3 targetPos, float h, out float v0, out float angle, out float time)
    {
        float xt = targetPos.x;
        float yt = targetPos.y;
        float g = -Physics.gravity.y;

        float b = Mathf.Sqrt(2 * g * h);
        float a = (-0.5f * g);
        float c = -yt;

        float tplus = QuadraticEquation(a, b, c, 1);
        float tmin = QuadraticEquation(a, b, c, -1);
        time = tplus > tmin ? tplus : tmin;

        angle = Mathf.Atan(b * time / xt);

        v0 = b / Mathf.Sin(angle);
    }
    IEnumerator Coroutine_Movement(Vector3 direction,float v0, float angle, float time)
    {
        float t = 0;
        while (t< time)
        {
            float x = v0 * t *Mathf.Cos(angle);
            float y = v0 * t * Mathf.Sin(angle) - (1f /2f) * -Physics.gravity.y * Mathf.Pow(t,2);

            transform.position = _FirePoint.position + direction * x + Vector3.up *y;
            t += Time.deltaTime;
            yield return null;
        }
    }
}