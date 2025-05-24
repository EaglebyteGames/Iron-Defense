using FriendlyCharacterController = EagleByte.FriendlyCharacterController.FriendlyCharacterController;
using System;
using UnityEditor;
using UnityEngine;
using VInspector;
using EagleByte.EnemyCharacterController;

public class Spellwright_CharacterController : FriendlyCharacterController
{
    private void Awake() { LegacyFirstSetup(); }

    private void Update() { LegacyRunUpdateMethod(); }

    private void OnDrawGizmosSelected() { LegacyGimzosSelectedSetup(); }


    [Foldout("Set Parameters/Shooting System Parameters")]
    [SerializeField] private GameObject pot, shootingEffect;
    public Transform shootPoint;

    public override void CharacterAnimatorController()
    {
        base.CharacterAnimatorController();

        if (attackSpeedCount > attackSpeed && !enemyTransform.GetComponent<EnemyCharacterController>().amIDeath)
        {
            Invoke(nameof(Shoot), 1.1f);
        }
    }
    private void Shoot()
    {
        if (amIDeath) return;
        if (enemyTransform != null)
        {
            GameObject currentPot = Instantiate(pot, shootPoint.position, shootPoint.rotation);
            currentPot.GetComponent<Bomb>().ShootMovement(shootPoint, "friend", default, enemyTransform);
            GameObject currnetEffect = Instantiate(shootingEffect, shootPoint.position, gameObject.transform.rotation);
            Destroy(currnetEffect, attackSpeedCount - 0.1f);
        }
    }
}
