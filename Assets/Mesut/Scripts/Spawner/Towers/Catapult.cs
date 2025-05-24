using EagleByte.EnemyCharacterController;
using MoreMountains.Feedbacks;
using TMPro;
using Unity.Cinemachine.Samples;
using UnityEngine;
using UnityEngine.AI;
using VInspector;

public class Catapult : MonoBehaviour
{
    [SerializeField] private LayerMask enemyMask;
    private Collider[] enemyColliders;
    private Transform enemyTransform;
    [ShowInInspector, ReadOnly] private Animator towerAnimator;

    //Shotting
    [SerializeField] TowerSeatManager towerManager;
    [SerializeField] private Transform cannon;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject[] cannonball;
    [Range(0, 100)] public float attackAreaSize;
    [Range(0, 100)] public float minAttackAreaSize;
    private float shootSpeedCount = 0f;
    [Range(0, 10)] public float shootSpeed;
    public bool isSpawn;

    [SerializeField] private NavMeshObstacle navMesh;
    [SerializeField] private int levelCount = 1;
    [SerializeField] private int levelPrice;
    [SerializeField] private CoinSystem coinSystem;
    [SerializeField] private TextMeshProUGUI levelText;

    [SerializeField] private MMF_Player cannonShootingFeel;

    private void Awake()
    {
        towerAnimator = gameObject.GetComponent<Animator>();
        towerManager = gameObject.GetComponentInParent<TowerSeatManager>();
        coinSystem = FindAnyObjectByType<CoinSystem>();
        levelText = gameObject.transform.parent.gameObject.transform.GetChild(0).GetChild(3).GetComponent<TextMeshProUGUI>();
        cannonShootingFeel = gameObject.GetComponent<MMF_Player>();
        isSpawn = false;
        navMesh.enabled = false;
        levelText.text = "Level " + levelCount;
    }

    private void Update()
    {
        enemyColliders = Physics.OverlapSphere(gameObject.transform.position, attackAreaSize, enemyMask);

        if(isSpawn)
        {
            navMesh.enabled = true;
            if (enemyTransform == null || !IsEnemyInRange(enemyTransform))
            {
                Target();
            }

            if (enemyTransform != null)
            {
                Attack();
            }
        }
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
        float distance = Vector3.Distance(transform.position, enemy.position);
        return distance <= attackAreaSize && distance >= minAttackAreaSize;
    }

    private void Target()
    {
        float minDistance = Mathf.Infinity;
        enemyTransform = null;

        foreach (var col in enemyColliders)
        {
            float distance = Vector3.Distance(transform.position, col.transform.position);

            if (distance >= minAttackAreaSize && distance < minDistance)
            {
                minDistance = distance;
                enemyTransform = col.transform;
            }
        }
    }

    private void Attack()
    {
        Vector3 direction = enemyTransform.position - transform.position;
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        transform.localRotation = Quaternion.Euler(0, angle, 0);

        Vector3 cannonAngle = (enemyTransform.position - cannon.position).normalized;
        float angleX = Mathf.Atan2(cannonAngle.y, Mathf.Sqrt(cannonAngle.x * cannonAngle.x + cannonAngle.z * cannonAngle.z)) * Mathf.Rad2Deg;
        angleX = Mathf.Clamp(angleX, -60f, 60f);

        if (shootSpeedCount > shootSpeed && !enemyTransform.GetComponent<EnemyCharacterController>().amIDeath && !towerManager.amIDeath)
        {
            towerAnimator.SetBool("Attack", true);
            cannonShootingFeel.PlayFeedbacks();
            Invoke(nameof(Shoot), 0.5f);
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
        if (enemyTransform != null)
        {
            GameObject currentCannonball = Instantiate(cannonball[levelCount-1], shootPoint.position, shootPoint.rotation);

            if (currentCannonball != null)
            {
                Vector3 direction = (enemyTransform.position - currentCannonball.transform.position).normalized;
                currentCannonball.GetComponent<Bomb>().ShootMovement(shootPoint, "friend", default, enemyTransform);
            }
        }
        Invoke(nameof(TowerAnimController), 1f);
    }

    private void TowerAnimController()
    {
        towerAnimator.SetBool("Attack", false);
    }
}
