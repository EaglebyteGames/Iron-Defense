using EagleByte.EnemyCharacterController;
using TMPro;
using Unity.Cinemachine.Samples;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using VInspector;

public class TowerCannon : MonoBehaviour
{
    [SerializeField] private LayerMask enemyMask;
    private Collider[] enemyColliders;
    private Transform enemyTransform;

    //Shotting
    [SerializeField] TowerSeatManager towerManager;
    [SerializeField] private Transform cannon, shootPoint;
    [SerializeField] private GameObject[] cannonball, shootingEffect;
    [Range(0, 300)] public float upForce;
    [Range(0, 500)] public float shootForce;
    [Range(0, 100)] public float attackAreaSize = 5f;
    [Range(0, 5), ReadOnly] public float shootSpeedCount = 0f;
    [Range(0, 10)] public float shootSpeed;
    public bool isSpawn;
    [SerializeField] private NavMeshObstacle navMesh;
    [SerializeField] private Collider col;
    [SerializeField] private Collider[] obstacleCols;

    [SerializeField] private int levelCount = 1;
    [SerializeField] private int levelPrice;
    [SerializeField] private CoinSystem coinSystem;

    [SerializeField] private TextMeshProUGUI levelText;
    private void Awake()
    {
        coinSystem = FindAnyObjectByType<CoinSystem>();
        towerManager = gameObject.GetComponentInParent<TowerSeatManager>();
        levelText = gameObject.transform.parent.gameObject.transform.GetChild(0).GetChild(3).GetComponent<TextMeshProUGUI>();
        isSpawn = false;
        navMesh.enabled = false;
        col.enabled = false;

        levelText.text = "Level " + levelCount;
    }

    private void Update()
    {
        enemyColliders = Physics.OverlapSphere(gameObject.transform.position, attackAreaSize, enemyMask);

        if(isSpawn)
        {
            navMesh.enabled = true;
            col.enabled = true;
            if (enemyTransform == null || !IsEnemyInRange(enemyTransform))
            {
                Target();
            }

            if (enemyTransform != null)
            {
                Attack();
            }
        }

        obstacleCols = Physics.OverlapSphere(gameObject.transform.position, attackAreaSize, enemyMask);
    }

    public void LevelUp()
    {
        if (coinSystem.coin - levelPrice >= 0 && levelCount < 5)
        {
           coinSystem.coin = coinSystem.coin - levelPrice;
           levelCount++;
           levelText.text = "Level " + levelCount;
        }
    }

    private bool IsEnemyInRange(Transform enemy)
    {
        return Vector3.Distance(transform.position, enemy.position) <= attackAreaSize;
    }

    private void Target()
    {
        float minDistance = Mathf.Infinity;
        enemyTransform = null;

        foreach (var col in enemyColliders)
        {
            float distance = Vector3.Distance(transform.position, col.transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                if (!col.gameObject.GetComponent<EnemyCharacterController>().amIDeath)
                {
                    enemyTransform = col.transform;
                }
            }
        }
    }

    private void Attack()
    {
        Transform targetTransform = enemyTransform.transform.GetChild(2).gameObject.transform;

        Vector3 direction = targetTransform.position - transform.position;
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        transform.localRotation = Quaternion.Euler(0, angle, 0);

        Vector3 cannonAngle = (targetTransform.position - cannon.position).normalized;
        float angleX = Mathf.Atan2(cannonAngle.y, Mathf.Sqrt(cannonAngle.x * cannonAngle.x + cannonAngle.z * cannonAngle.z)) * Mathf.Rad2Deg;
        angleX = Mathf.Clamp(angleX, -60f, 60f);
        cannon.localRotation = Quaternion.Euler(-angleX, 0, 0);

        if (enemyTransform.gameObject.GetComponent<EnemyCharacterController>().amIDeath)
        {
            Target();
        }

        if (shootSpeedCount > shootSpeed && !enemyTransform.gameObject.GetComponent<EnemyCharacterController>().amIDeath && !towerManager.amIDeath)
        {
            Shoot();
            shootSpeedCount = 0f;
        }
        else
        {
            shootSpeedCount += Time.deltaTime;
        }
    }

    private void Shoot()
    {
        if (towerManager.amIDeath) return;
        GameObject currentCannonball = Instantiate(cannonball[levelCount-1], shootPoint.position, shootPoint.rotation * shootPoint.rotation);
        if (enemyTransform != null)
        {
            if (currentCannonball != null && currentCannonball.GetComponent<Rigidbody>() != null)
            {
                Vector3 direction = (enemyTransform.position - currentCannonball.transform.position).normalized;

                currentCannonball.GetComponent<Rigidbody>().AddForce(direction * shootForce, ForceMode.Impulse);
                currentCannonball.GetComponent<Rigidbody>().AddForce(Vector3.up * upForce, ForceMode.Impulse);

                GameObject currnetEffect = Instantiate(shootingEffect[levelCount-1], shootPoint.position, gameObject.transform.rotation);
                Destroy(currnetEffect, 1f);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gameObject.transform.position, attackAreaSize);
    }
}