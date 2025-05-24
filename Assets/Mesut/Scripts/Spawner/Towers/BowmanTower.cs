using EagleByte.EnemyCharacterController;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using VInspector;


public class BowmanTower : MonoBehaviour
{
    [SerializeField] private LayerMask enemyMask;
    private Collider[] enemyColliders;
    public Transform enemyTransform;

    //Shotting
    [SerializeField] TowerSeatManager towerManager;
    [SerializeField] private Transform[] shootPoints;
    public List<GameObject> arrow;

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
        towerManager = gameObject.GetComponent<TowerSeatManager>();
        levelText = gameObject.transform.GetChild(0).GetChild(3).GetComponent<TextMeshProUGUI>();
        isSpawn = false;
        navMesh.enabled = false;
        col.enabled = false;

        levelText.text = "Level " + levelCount;
    }

    private void Update()
    {
        enemyColliders = Physics.OverlapSphere(gameObject.transform.position, attackAreaSize, enemyMask);

        if (isSpawn)
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
        if (enemyTransform.gameObject.GetComponent<EnemyCharacterController>().amIDeath)
        {
            Target();
        }

        if (shootSpeedCount > shootSpeed && enemyTransform.gameObject != null && !enemyTransform.gameObject.GetComponent<EnemyCharacterController>().amIDeath && !towerManager.amIDeath)
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
        for (int i = 0; i < arrow.Count; i++)
        {
            Transform targetTransform = enemyTransform.transform.GetChild(2).gameObject.transform;

            Vector3 direction = targetTransform.position - transform.position;
            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            shootPoints[i].gameObject.transform.localRotation = Quaternion.Euler(0, angle, 0);

            GameObject obj = Instantiate(arrow[levelCount - 1], shootPoints[i].position, shootPoints[i].rotation * shootPoints[i].rotation);

            if (obj.GetComponent<Rigidbody>() != null)
            {
                Vector3 direction1 = (enemyTransform.position - obj.transform.position).normalized;

                obj.GetComponent<Rigidbody>().AddForce(direction1 * shootForce, ForceMode.Impulse);
                obj.GetComponent<Rigidbody>().AddForce(Vector3.up * upForce, ForceMode.Impulse);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gameObject.transform.position, attackAreaSize);
    }
}
