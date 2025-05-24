using EnemyCharacterController = EagleByte.EnemyCharacterController.EnemyCharacterController;
using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using System.Collections.Generic;
using EagleByte.FriendlyCharacterController;

public class WorldDevourer_CharacterController : EnemyCharacterController
{
    private void Awake() { LegacyFirstSetup(); }

    private void Start() { LegacySecondSetup(); }

    private void Update()
    { LegacyRunUpdateMethod(); }

    private void OnDrawGizmosSelected() { LegacyGimzosSelectedSetup(); }

    private int hit;
    [SerializeField] private int upForce;
    [SerializeField] private int force;
    [SerializeField] private float sleepTime;
    public List<Animator> friendAnimators;

    public override void CharacterAnimatorController()
    {
        base.CharacterAnimatorController();

        if (attackSpeedCount > attackSpeed)
        {
            hit++;

            if (hit >= 4)
            {
                hit = 0;
                myAnimator.SetBool("attackMethod1", true);
                Invoke(nameof(AreaAttack), 1.1f);
            }
        }
    }

    private void AreaAttack()
    {
        if (setDeath) return;

        foreach (Collider enemyCollider in enemyColliders)
        {
            Rigidbody enemyRb = enemyCollider.GetComponent<Rigidbody>();
            NavMeshAgent navAgent = enemyCollider.GetComponent<NavMeshAgent>();
            FriendlyCharacterController script = enemyCollider.GetComponent<FriendlyCharacterController>();
            Animator animator = enemyCollider.GetComponent<Animator>();

            if (!friendAnimators.Contains(animator))
            {
                friendAnimators.Add(animator);
            }

            if (enemyRb != null && navAgent != null)
            {
                StartCoroutine(ApplyKnockback(enemyRb, navAgent, script, animator));
            }
        }
    }

    private IEnumerator ApplyKnockback(Rigidbody enemyRb, NavMeshAgent navAgent, FriendlyCharacterController script, Animator animator)
    {
        yield return null;

        script.enabled = false;
        navAgent.enabled = false;

        enemyRb.useGravity = true;
        enemyRb.isKinematic = false;
        if (animator != null && enemyRb != null && navAgent != null && script != null)
        {
            animator.SetInteger("LieDown", 1);
            Vector3 dir = (enemyRb.transform.position - gameObject.transform.position).normalized;
            enemyRb.AddForce(new Vector3(force * dir.x, upForce, force * dir.z), ForceMode.Impulse);
        }

        yield return new WaitForFixedUpdate();
        yield return new WaitForSeconds(sleepTime);
        if (animator != null && enemyRb != null && navAgent != null && script != null)
        {
            animator.SetInteger("LieDown", 2);
            navAgent.Warp(enemyRb.transform.position);
            navAgent.enabled = true;
            enemyRb.useGravity = false;
            enemyRb.isKinematic = true;
        }

        yield return new WaitForSeconds(1f);
        if (animator != null && enemyRb != null && navAgent != null && script != null)
        {
            animator.SetInteger("LieDown", 0);
            script.enabled = true;
        }
        yield return null;
    }
}