using Random = UnityEngine.Random;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using VInspector;
using UnityEngine.UI;
using MoreMountains.Feedbacks;

namespace EagleByte
{
    namespace EnemyCharacterController
    {
        public class EnemyCharacterController : MonoBehaviour
        {
            [Foldout("Core/Get Parameters")]
            [ShowInInspector, ReadOnly] public NavMeshAgent agent;
            [ShowInInspector, ReadOnly] public Animator myAnimator;
            [ShowInInspector, ReadOnly] public CapsuleCollider myCol;
            [ShowInInspector, ReadOnly] private Cloth myCloth;
            [ShowInInspector, ReadOnly] public Slider healthBarSlider;
            [EndFoldout]

            [Foldout("Core/Set Parameters")]
            public Vector3 targetPosition;
            public bool goMoveNow = false;
            [EndFoldout]

            [Foldout("Core/Health Parameters")]
            [Range(0, 500)] public float health;
            [Range(0, 500), ReadOnly] public float legacyHealth;
            [Range(0, 500)] public float damage;
            [EndFoldout]

            [Foldout("Core/Set Parameters/Speed And Move Parameters")]
            [Range(0, 12), ReadOnly] public float moveCurrentlySpeed = 1f;
            [Range(0, 60), ReadOnly] public float moveSlopeAngel = 0f;
            [Range(0, 12)] public float moveMaxSpeed = 1f;
            [Range(0, 12)] public float moveMinSpeed = 1f;
            [Range(0, 12)] public float moveClimbSpeed = 1f;
            [SerializeField] public LayerMask moveGroundLayer;
            [SerializeField] public float moveRaycastLength = 2f;
            [Tooltip("Eger 0 ise karakter yuruyor, eger 1 ise karakter tirmaniyor, eger 2 ise karakter yokus asagi iniyor")]
            [ReadOnly] public int moveStraightUpDown = 0;

            [Foldout("Core/Set Parameters/SpawnParameters")]
            [Range(0.1f, 5f)] public float spawnTime = 3.5f;
            [Range(0, 5f), ReadOnly] public float spawnTimeCount = 0f;
            [Foldout("Core/Set Parameters/Attack System Parameters")]
            public Vector3 enemyTransform;
            [Tooltip("Hali hazirda saldirdigim kule")]
            [ReadOnly] public GameObject currentlyAttackingTower;
            [Tooltip("Karakteri olay mahaline goturmek icin bu degiskeni true olarak ayarlayin.")]
            public bool goAttackNow = false;
            [Tooltip("Karakter saldiri anindayken bu degeri true olarak dondurur.")]
            public bool letGoAttackNow = false;
            public bool iSeeTower = false;
            public bool setHit = false;
            public bool setDeath = false;
            public bool amIDeath = false;
            [Range(0, 5)] public float attackSpeed = 1f;
            [Tooltip("letGoAttackNow genel bir saldiri cesididir, attackNow farki her saldiri animasyonu oynadiginde tetiklenir.")]
            public bool attackNow = false;
            [Range(0, 5), ReadOnly] public float attackSpeedCount = 0f;
            [Range(0, 100)] public float attackAreaSize = 5f;
            [Foldout("Core/Set Parameters/Attack System Parameters/Enemy Parameters")]
            [SerializeField] public LayerMask enemyMask;
            [SerializeField] public float enemyRaycastLength = 2f;
            [ReadOnly] public Collider[] enemyColliders;
            [Tooltip("Daha oncesinde dusman gordum mu, bu degiskenin amaci, goMoveNow degiskenini tek seferlik triggerlamak")]
            [ReadOnly] public bool iHaveSeenEnemy = false;
            [Foldout("Core/Set Parameters/Attack System Parameters/Tower Parameters")]
            [ReadOnly] public GameObject towerParents;
            [ReadOnly] public List<GameObject> towerObjects;
            [ReadOnly] public Vector3 closestTowerPosition;
            [ReadOnly] public GameObject closestTower;
            [Range(0.1f, 5.0f)] public float scanFrequency = 2f;
            [Range(0, 5.0f), ReadOnly] public float scanFrequencyCount = 0;
            [SerializeField, ReadOnly, Tooltip("Bu arkadas _goNow degiskenini triggerlamak icin koyulan bir karsilastirma degiskenidir." +
                                               " Once ilk pozisyon yedeklenir, sonra closestTower'in pozisyon degeriyle karsilastirmasi yapilir," +
                                               "eger degisiklik varsa _goNow degiskeni triggerlanir.")]
            public Vector3 previouslyTargetPosition = Vector3.zero;
            private bool myClothOnce = false;

            CoinSystem coinSystem;
            public int money;

            [Foldout("Core/Feel/FeedBacks/AttackFeedBacks")]
            [SerializeField] private MMF_Player attackFeedBack;
            #region Setups

            public virtual void LegacyFirstSetup()
            {
                if (gameObject.GetComponentInChildren<Cloth>() != null)
                    myCloth = gameObject.GetComponentInChildren<Cloth>();
                myCol = gameObject.GetComponent<CapsuleCollider>();
                agent = gameObject.GetComponent<NavMeshAgent>();
                myAnimator = gameObject.GetComponent<Animator>();
                towerParents = GameObject.Find("Towers");
                healthBarSlider = GetComponentInChildren<Canvas>().gameObject.transform.GetChild(0).gameObject.GetComponent<Slider>();
                coinSystem = FindAnyObjectByType<CoinSystem>();
                attackFeedBack = gameObject.GetComponent<MMF_Player>();
            }

            public virtual void LegacySecondSetup()
            {
                #region Enemy Scan Tower and Search Closest Tower
                float minDistance = Mathf.Infinity;
                scanFrequencyCount = 0f;
                //tumunu temizle sorna cek
                towerObjects.Clear();
                foreach (Transform child in towerParents.transform)
                {
                    float distance = Vector3.Distance(gameObject.transform.localPosition, child.gameObject.transform.localPosition);
                    if (!towerObjects.Contains(child.gameObject)) towerObjects.Add(child.gameObject);
                    //en yakin kuleyi bul
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestTower = child.gameObject;

                        if (gameObject.CompareTag("EnemyFar"))
                        {
                            closestTowerPosition = closestTower.gameObject.GetComponent<Collider>().bounds.center;
                        }
                        else if (gameObject.CompareTag("EnemyClose"))
                        {
                            closestTowerPosition = child.gameObject.GetComponent<TowerSeatManager>().GetRandomPoint();
                        }
                    }
                }
                #endregion

                //Default Setup Enemy
                goMoveNow = true;

                //pelerinlilere ozel
                if (myCloth != null)
                    myCloth.enabled = false;

                //Health Bar slider setup
                legacyHealth = health;
                healthBarSlider.maxValue = legacyHealth;
                healthBarSlider.minValue = 0;
                healthBarSlider.value = legacyHealth;
            }

            #endregion

            #region Core Methods

            #region Updater Methods
            public virtual void LegacyRunUpdateMethod()
            {
                //Karakterin dogmasini bekle aq
                if (spawnTimeCount >= spawnTime)
                {
                    #region For Enemy

                    ScanForTowersAtRegularIntervals();

                    #endregion

                    #region Core
                    CharacterAnimatorController();
                    SetSpeedRelativeToGround();
                    MoveToTarget(goMoveNow);
                    MoveToTargetThanAttack();
                    DeathControl();
                    IAttackTheEnemyInFrontOfMe();
                    SliderHealthBarController();
                    #endregion

                    //pelerinlilere ozel
                    if (myCloth != null && !myClothOnce) { myCloth.enabled = true; /*tek seferlik*/ myClothOnce = true; }
                }
                else spawnTimeCount += Time.deltaTime;
            }
            public virtual void LegacyGimzosSelectedSetup()
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(gameObject.transform.position, attackAreaSize);

                Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, Vector3.down * moveRaycastLength);

                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position + Vector3.up, transform.TransformDirection(Vector3.forward) * enemyRaycastLength);
            }
            public void SliderHealthBarController() { healthBarSlider.value = health; healthBarSlider.gameObject.transform.parent.transform.LookAt(Camera.main.transform.position); }
            public virtual void DeathControl()
            {
                if (health <= 0) { amIDeath = true; setDeath = true; gameObject.GetComponent<Collider>().enabled = false; }
            }
            public virtual void IAttackTheEnemyInFrontOfMe()
            {
                RaycastHit hit;
                if (Physics.Raycast(gameObject.transform.position + Vector3.up, transform.TransformDirection(Vector3.forward), out hit, enemyRaycastLength, enemyMask))
                {
                    if (attackNow)
                    {
                        //if (hit.collider.gameObject.GetComponent<FriendlyCharacterController.FriendlyCharacterController>().amIDeath == true) 
                        //{ 

                        //}
                        if (hit.collider.gameObject.GetComponent<FriendlyCharacterController.FriendlyCharacterController>() != null)
                            hit.collider.gameObject.GetComponent<FriendlyCharacterController.FriendlyCharacterController>().health -= damage;
                        if (hit.collider.gameObject.GetComponent<TowerSeatManager>() != null)
                            hit.collider.gameObject.GetComponent<TowerSeatManager>().health -= damage;
                    }
                }
            }
            public virtual void ScanForTowersAtRegularIntervals()
            {
                if (scanFrequencyCount >= scanFrequency)
                {
                    float minDistance = Mathf.Infinity;
                    scanFrequencyCount = 0f;
                    //tumunu temizle sorna cek
                    towerObjects.Clear();
                    foreach (Transform child in towerParents.transform)
                    {
                        float distance = Vector3.Distance(gameObject.transform.localPosition, child.gameObject.transform.localPosition);
                        if (!towerObjects.Contains(child.gameObject)) towerObjects.Add(child.gameObject);
                        //en yakin kuleyi bul
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestTower = child.gameObject;

                            if (gameObject.CompareTag("EnemyFar"))
                            {
                                closestTowerPosition = closestTower.gameObject.GetComponent<Collider>().bounds.center;
                            }
                            else if (gameObject.CompareTag("EnemyClose"))
                            {
                                closestTowerPosition = child.gameObject.GetComponent<TowerSeatManager>().GetRandomPoint();
                            }
                        }
                    }
                }
                else scanFrequencyCount += Time.deltaTime;
            }

            public virtual void SetSpeedRelativeToGround()
            {
                //Eger 0 ise karakter yuruyor, eger 1 ise karakter tirmaniyor, eger 2 ise karakter yokus asagi iniyor
                if (agent.velocity.y == 0) moveStraightUpDown = 0;
                else if (agent.velocity.y > 0) moveStraightUpDown = 1;
                else moveStraightUpDown = 2;

                RaycastHit hit;
                if (Physics.Raycast(transform.position + Vector3.up * 0.5f,
                        Vector3.down,
                        out hit,
                        moveRaycastLength,
                        moveGroundLayer))
                {
                    Vector3 groundNormal = hit.normal;
                    moveSlopeAngel = Vector3.Angle(groundNormal, Vector3.up);

                    float t = moveSlopeAngel / 60f;  // 0°-60° aci normallestirme

                    if (agent.velocity.magnitude == 0) moveCurrentlySpeed = 0f;
                    else if (moveStraightUpDown == 0) { moveCurrentlySpeed = moveMinSpeed; agent.speed = moveCurrentlySpeed; myAnimator.speed = 1f; }
                    else if (moveStraightUpDown == 1) { moveCurrentlySpeed = Mathf.Lerp(moveMinSpeed, moveClimbSpeed, t); agent.speed = moveCurrentlySpeed; myAnimator.speed = 0.75f; }
                    else if (moveStraightUpDown == 2) { moveCurrentlySpeed = Mathf.Lerp(moveMinSpeed, moveMaxSpeed, t); agent.speed = moveCurrentlySpeed; myAnimator.speed = 1f; }
                }
            }

            public virtual void MoveToTarget(bool _goNow)
            {
                if (closestTowerPosition != null)
                {
                    targetPosition = closestTowerPosition;
                    if (previouslyTargetPosition != targetPosition)
                    {
                        _goNow = true;
                        previouslyTargetPosition = targetPosition;
                    }
                    if (_goNow && !goAttackNow && !letGoAttackNow)
                    {
                        agent.isStopped = false;
                        agent.SetDestination(targetPosition);
                        goMoveNow = !goMoveNow;
                    }
                }
            }

            public virtual void MoveToTargetThanAttack()
            {
                enemyColliders = Physics.OverlapSphere(gameObject.transform.position, attackAreaSize, enemyMask);

                foreach (Collider collider in enemyColliders)
                {
                    if (collider.gameObject.CompareTag("Wall") || (collider.gameObject.CompareTag("Tower")) && collider.gameObject != null && closestTower != null && collider.gameObject.transform.position == closestTower.gameObject.transform.position)
                    {
                        if (collider.gameObject != null)
                            currentlyAttackingTower = collider.gameObject;
                        iSeeTower = true;
                        goAttackNow = true;
                        enemyTransform = closestTowerPosition;
                        break;
                    }
                }
                if (currentlyAttackingTower == null)
                {
                    currentlyAttackingTower = null;
                    iSeeTower = false;
                }

                if (!iSeeTower && goAttackNow && agent.enabled == true)
                {
                    iHaveSeenEnemy = true;
                    agent.isStopped = false;
                    agent.SetDestination(enemyTransform);
                }
                if (!iSeeTower && enemyColliders.Length > 0) DetectEnemy();
                if (!iSeeTower && enemyColliders.Length == 0)
                {
                    goAttackNow = false;
                    letGoAttackNow = false;
                    //sksten sonra kuleye gitmeye devam et la
                    if (iHaveSeenEnemy) { goMoveNow = true; iHaveSeenEnemy = false; }
                }

                if (moveCurrentlySpeed == 0f && enemyColliders.Length > 0)
                {
                    letGoAttackNow = true;
                    if (iSeeTower && closestTower != null)
                    {
                        Vector3 directon = closestTower.transform.position - gameObject.transform.position;
                        directon.y = 0f;
                        Quaternion targetRotation = Quaternion.LookRotation(directon);
                        gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, targetRotation, 5 * Time.deltaTime);
                    }
                    else
                    {
                        Vector3 directon = enemyTransform - gameObject.transform.position;
                        directon.y = 0f;
                        Quaternion targetRotation = Quaternion.LookRotation(directon);

                        gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, targetRotation, 5 * Time.deltaTime);
                    }
                    //transform.LookAt(new Vector3(enemyTransform.x, gameObject.transform.position.y, enemyTransform.z));
                }
                else letGoAttackNow = false;
            }

            public virtual void DetectEnemy()
            {
                goAttackNow = true;
                float minDistance = Mathf.Infinity;

                //Turkcesi ben bir veya birden fazla kulenin menzilindeyim uzaklik hesabi yapmana gerek yok ben zaten kulelerle haberlesiyorum.
                if (!iSeeTower)
                {
                    foreach (var col in enemyColliders)
                    {
                        float distance = Vector3.Distance(gameObject.transform.position,
                            col.gameObject.transform.position);

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            enemyTransform = col.gameObject.transform.position;
                        }
                    }
                }
            }
            public virtual void CharacterAnimatorController()
            {
                //Karakter hareketleri
                myAnimator.SetFloat("speed", moveCurrentlySpeed);

                //Karakterin hareket haline gectiginde saldiri animasyonlarindan cikmasi icin
                if (moveCurrentlySpeed != 0)
                {
                    myAnimator.SetBool("attackMethod1", false);
                    myAnimator.SetBool("attackMethod2", false);
                    myAnimator.SetBool("attackMethod3", false);
                    myAnimator.SetBool("attackMethod4", false);
                }

                //Karakter Random Saldiri 
                if (letGoAttackNow)
                {
                    if (attackSpeedCount > attackSpeed)
                    {
                        int randomValue = Random.Range(1, 5);
                        switch (randomValue)
                        {
                            case 1:
                                myAnimator.SetBool("attackMethod1", true);
                                attackNow = true;
                                attackFeedBack.PlayFeedbacks();
                                break;
                            case 2:
                                myAnimator.SetBool("attackMethod2", true);
                                attackNow = true;
                                attackFeedBack.PlayFeedbacks();
                                break;
                            case 3:
                                myAnimator.SetBool("attackMethod3", true);
                                attackNow = true;
                                attackFeedBack.PlayFeedbacks();
                                break;
                            case 4:
                                myAnimator.SetBool("attackMethod4", true);
                                attackNow = true;
                                attackFeedBack.PlayFeedbacks();
                                break;
                        }

                        attackSpeedCount = 0f;
                    }
                    else
                    {
                        myAnimator.SetBool("attackMethod1", false);
                        myAnimator.SetBool("attackMethod2", false);
                        myAnimator.SetBool("attackMethod3", false);
                        myAnimator.SetBool("attackMethod4", false);
                        attackNow = false;
                        attackSpeedCount += Time.deltaTime;
                    }
                }

                //Hit
                if (setHit)
                {
                    myAnimator.SetBool("hit", true);
                    setHit = !setHit;
                }
                if (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hit_A") && myAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.1f)
                {
                    myAnimator.SetBool("hit", false);
                }

                //Death
                if (setDeath)
                {
                    coinSystem.coin = coinSystem.coin + money;
                    myCol.enabled = false;
                    myAnimator.SetBool("death", true);
                    Destroy(gameObject, 5f);
                    setDeath = false;
                    agent.enabled = false;
                    gameObject.GetComponent<EnemyCharacterController>().enabled = false;
                }
            }

            #endregion

            #endregion
        }
    }
}
