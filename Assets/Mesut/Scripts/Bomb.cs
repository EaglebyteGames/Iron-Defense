using EagleByte.EnemyCharacterController;
using EagleByte.FriendlyCharacterController;
using MoreMountains.Feedbacks;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class Bomb : MonoBehaviour
{
    [SerializeField] private GameObject explosion;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int enemyLay;

    [Range(0, 100)] public float explosionDamage;
    [Range(0, 50)] public float explosionRange;
    [Range(0, 10)] public float lifeTime;

    [Range(0, 50)] public int maxHeight;
    [SerializeField] private float damage;
    [SerializeField] private bool isFriend;

    [SerializeField] private MMF_Player shootSound;
    [SerializeField] private MMF_Player bombSound;

    private void Awake()
    {
        if (GetComponent<MMF_Player>() != null)
        {
            bombSound = GetComponent<MMF_Player>();          
        }

        if (gameObject.transform.GetChild(0) != null && gameObject.transform.GetChild(0).GetComponent<MMF_Player>() != null)
        {
            shootSound = gameObject.transform.GetChild(0).GetComponent<MMF_Player>();
        }

        if (shootSound != null)
        {
            shootSound.PlayFeedbacks();
        }
    }

    private void Update()
    {
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0) Explode();

        gameObject.GetComponent<SphereCollider>().center = Vector3.zero;
    }
    public void ShootMovement(Transform shootPoint, string whichSide, Vector3 enemyVector = default, Transform enemyTransfrom = default)
    {
        switch (whichSide)
        {
            case "friend":
                Vector3 direction1;
                direction1 = enemyTransfrom.position - shootPoint.position;
                Vector3 graoundDirection1 = new Vector3(direction1.x, 0, direction1.z);
                Vector3 targetPos1 = new Vector3(graoundDirection1.magnitude, direction1.y, 0);
                float height1 = targetPos1.y + targetPos1.magnitude / 2f;
                height1 = Mathf.Max(0.001f, maxHeight);
                float angle1;
                float v01;
                float time1;
                CalculatePathWithHeight(targetPos1, height1, out v01, out angle1, out time1);
                StopAllCoroutines();
                StartCoroutine(Coroutine_Movement(graoundDirection1.normalized, v01, angle1, time1, shootPoint));
                break;
            case "enemy":
                Vector3 direction;
                direction = enemyVector - shootPoint.position;
                Vector3 graoundDirection = new Vector3(direction.x, 0, direction.z);
                Vector3 targetPos = new Vector3(graoundDirection.magnitude, direction.y, 0);
                float height = targetPos.y + targetPos.magnitude / 2f;
                height = Mathf.Max(0.001f, maxHeight);
                float angle;
                float v0;
                float time;
                CalculatePathWithHeight(targetPos, height, out v0, out angle, out time);
                StopAllCoroutines();
                StartCoroutine(Coroutine_Movement(graoundDirection.normalized, v0, angle, time, shootPoint));
                break;
        }
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
    IEnumerator Coroutine_Movement(Vector3 direction, float v0, float angle, float time, Transform shotPoint)
    {
        float t = 0;
        while (t < time)
        {
            float x = v0 * t * Mathf.Cos(angle);
            float y = v0 * t * Mathf.Sin(angle) - (1f / 2f) * -Physics.gravity.y * Mathf.Pow(t, 2);

            if (shotPoint != null)
            {
                transform.position = shotPoint.position + direction * x + Vector3.up * y;
            }
            t += Time.deltaTime;
            yield return null;
        }
    }

    private void Explode()
    {
        if (explosion != null)
        {
            GameObject explosionInstance = Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(explosionInstance, 1f);
        }

        Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRange, enemyLayer);
        foreach (Collider enemy in enemies)
        {
            if (enemy.GetComponent<EnemyCharacterController>() != null)
            {
                enemy.gameObject.GetComponent<EnemyCharacterController>().health -= damage;
            }
            if (enemy.GetComponent<FriendlyCharacterController>() != null)
            {
                enemy.gameObject.GetComponent<FriendlyCharacterController>().health -= damage;
            }
            if (enemy.gameObject.GetComponent<TowerSeatManager>() != null)
            {
                enemy.gameObject.GetComponent<TowerSeatManager>().health -= damage;
            }
        }

        if (bombSound != null)
        {
            bombSound.PlayFeedbacks();
        }
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isFriend && (other.gameObject.layer == 6 || other.gameObject.layer == 3))
        {
            Explode();
        }
        if (!isFriend && (other.gameObject.layer == 7 || other.gameObject.layer == 3 || other.gameObject.layer == 11))
        {
            Explode();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(gameObject.transform.position, explosionRange);
    }
}