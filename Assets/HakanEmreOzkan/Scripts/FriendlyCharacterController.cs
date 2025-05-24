using Random = UnityEngine.Random;
using UnityEngine;
using UnityEngine.AI;
using VInspector;
using UnityEngine.UI;
using MoreMountains.Feedbacks;

namespace EagleByte
{
    namespace FriendlyCharacterController
    {
        public class FriendlyCharacterController : MonoBehaviour
        {
            [Foldout("Core/Get Parameters")]
            [ShowInInspector, ReadOnly] public NavMeshAgent agent;
            [ShowInInspector, ReadOnly] public Animator myAnimator;
            [ShowInInspector, ReadOnly] public CapsuleCollider myCol;
            [ShowInInspector, ReadOnly] public CapsuleCollider[] childCols;
            [ShowInInspector, ReadOnly] public Slider healthBarSlider;
            [EndFoldout]

            [Foldout("Core/Set Parameters")]
            public Vector3 targetPosition;
            public bool goMoveNow = false;
            public bool justStop = false;
            [EndFoldout]

            [Foldout("Core/Health & Damage Parameters")]
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

            [Foldout("Core/Set Parameters/Attack System Parameters")]
            public int level;
            public Transform enemyTransform;
            [Tooltip("Karakteri olay mahaline goturmek icin bu degiskeni true olarak ayarlayin.")]
            public bool goAttackNow = false;
            [Tooltip("Karakter saldiri anindayken bu degeri true olarak dondurur.")]
            public bool letGoAttackNow = false;
            [ReadOnly] public bool attackControl;
            public bool setHit = false;
            public bool setDeath = false;
            public bool amIDeath = false;
            [Range(0, 5)] public float attackSpeed = 1f;
            public bool attackNow = false;
            [Range(0, 5), ReadOnly] public float attackSpeedCount = 0f;
            [Range(0, 100)] public float attackAreaSize = 5f;
            [SerializeField] public LayerMask enemyMask;
            [SerializeField] public float enemyRaycastLength = 2f;
            [ReadOnly] public Collider[] enemyColliders;
            public GameObject hitObj;
            public GameObject flag;

            [Foldout("Core/Feel/FeedBacks/AttackFeedBacks")]
            [SerializeField] private MMF_Player attackFeedBack;

            [Foldout("Core/Minimap Canvas")]
            [SerializeField] private Transform minimapCanvas;
            [SerializeField] public GameObject selectedUI;
            #region Setups

            public virtual void LegacyFirstSetup()
            {
                myCol = gameObject.GetComponent<CapsuleCollider>();
                agent = gameObject.GetComponent<NavMeshAgent>();
                myAnimator = gameObject.GetComponent<Animator>();
                childCols = gameObject.GetComponentsInChildren<CapsuleCollider>();
                healthBarSlider = GetComponentInChildren<Canvas>().gameObject.transform.GetChild(0).gameObject.GetComponent<Slider>();
                flag = FindAnyObjectByType<ObjectSelectionManager>().gameObject.transform.GetChild(0).gameObject;
                attackFeedBack = gameObject.GetComponent<MMF_Player>();
                minimapCanvas = gameObject.transform.GetChild(3).gameObject.transform.GetChild(0).transform;
            }

            public virtual void Start()
            {
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
                DeathControl();
                SliderHealthBarController();
                IAttackTheEnemyInFrontOfMe();
                CharacterAnimatorController();
                SetSpeedRelativeToGround();
                MoveToTarget(goMoveNow, targetPosition);
                MoveToTargetThanAttack();
                HealthBarClamp();
                FlagController();
                CanvasUIRotation();
            }
            public virtual void LegacyGimzosSelectedSetup()
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(gameObject.transform.position, attackAreaSize);

                Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, Vector3.down * moveRaycastLength);

                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position + Vector3.up, transform.TransformDirection(Vector3.forward) * enemyRaycastLength);
            }
            public virtual void HealthBarClamp()
            {
                health = Mathf.Clamp(health, 0, legacyHealth);
            }
            public void SliderHealthBarController() { healthBarSlider.value = health; healthBarSlider.gameObject.transform.parent.transform.LookAt(Camera.main.transform.position); }
            public virtual void DeathControl()
            {
                if (health <= 0)
                {
                    gameObject.transform.GetChild(0).gameObject.SetActive(false);
                    amIDeath = true;
                    setDeath = true;
                    gameObject.GetComponent<CapsuleCollider>().enabled = false;
                    foreach (var childCol in childCols) childCol.enabled = false;
                }
            }
            public virtual void IAttackTheEnemyInFrontOfMe()
            {
                RaycastHit hit;
                if (Physics.Raycast(gameObject.transform.position + Vector3.up, transform.TransformDirection(Vector3.forward), out hit, enemyRaycastLength, enemyMask))
                {
                    if (attackNow) attackControl = false;
                    if (((myAnimator.GetCurrentAnimatorStateInfo(0).IsName("AttackStyle1")
                       && myAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f) ||
                       (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("AttackStyle2")
                       && myAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f) ||
                       (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("AttackStyle3")
                       && myAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f) ||
                       (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("AttackStyle4")
                       && myAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f)) && attackControl == false)
                    {
                        attackControl = true;
                        if (hit.collider.gameObject.GetComponent<EnemyCharacterController.EnemyCharacterController>() != null)
                            hit.collider.gameObject.GetComponent<EnemyCharacterController.EnemyCharacterController>().health -= damage;
                    }
                }
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

            public virtual void MoveToTarget(bool _goNow, Vector3 targetPosition)
            {
                if (_goNow)
                {
                    this.targetPosition = targetPosition;
                    if (justStop)
                    {
                        agent.isStopped = true;
                        agent.ResetPath();
                    }
                    else if (!justStop)
                    {
                        agent.isStopped = false;
                        agent.SetDestination(targetPosition);
                    }
                    goMoveNow = !goMoveNow;
                }
            }

            public virtual void CharacterAnimatorController()
            {
                //Karakter hareketleri
                myAnimator.SetFloat("speed", moveCurrentlySpeed);

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
                    myCol.enabled = false;
                    myAnimator.SetBool("death", true);
                    Destroy(gameObject, 5f);
                    setDeath = false;
                    agent.enabled = false;
                    gameObject.GetComponent<FriendlyCharacterController>().enabled = false;
                }
            }
            public virtual void MoveToTargetThanAttack()
            {
                enemyColliders = Physics.OverlapSphere(gameObject.transform.position, attackAreaSize, enemyMask);

                if (!goAttackNow && !letGoAttackNow && enemyColliders.Length == 0) enemyTransform = null;
                if (enemyTransform != null && goAttackNow && agent.enabled == true)
                {
                    agent.isStopped = false;

                    if (enemyColliders.Length > 0) DetectEnemy();
                }
                else if (enemyTransform == null && enemyColliders.Length > 0 && !goAttackNow && !letGoAttackNow) { DetectEnemy(); }
                else if (enemyTransform == null) { goAttackNow = false; letGoAttackNow = false; }


                if (enemyTransform != null && agent.velocity.magnitude <= 0.1f && enemyColliders.Length > 0 && Vector3.Distance(gameObject.transform.position, enemyTransform.transform.position) <= agent.stoppingDistance)
                {
                    letGoAttackNow = true;
                    //Yumusak bir sekilde don
                    Vector3 directon = enemyTransform.transform.position - gameObject.transform.position;
                    directon.y = 0f;
                    Quaternion targetRotation = Quaternion.LookRotation(directon);

                    gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, targetRotation, 5 * Time.deltaTime);
                }
                else letGoAttackNow = false;
            }
            public virtual void DetectEnemy()
            {
                goAttackNow = true;
                float minDistance = Mathf.Infinity;

                if (hitObj == null)
                {
                    foreach (var col in enemyColliders)
                    {
                        float distance = Vector3.Distance(gameObject.transform.position,
                            col.gameObject.transform.position);

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            enemyTransform = col.gameObject.transform;
                        }
                    }
                }

                if (enemyTransform != null)
                {
                    agent.SetDestination(enemyTransform.position);
                }
            }

            public virtual void FlagController()
            {
                if (Vector3.Distance(gameObject.transform.position, flag.gameObject.transform.position) < 3)
                {
                    if (gameObject.CompareTag("FriendFar"))
                    {
                        agent.stoppingDistance = 30f;
                    }
                }
            }
            private void CanvasUIRotation()
            {
                minimapCanvas.transform.rotation = Quaternion.Euler(90, 0, 0);
            }
            #endregion

            #endregion
        }
    }
}
