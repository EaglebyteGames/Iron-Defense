using EnemyCharacterController = EagleByte.EnemyCharacterController.EnemyCharacterController;
using UnityEngine;
using VInspector;

public class Lich_CharacterController : EnemyCharacterController
{
    private void Awake() { LegacyFirstSetup(); }

    private void Start() { LegacySecondSetup(); }

    private void Update() { LegacyRunUpdateMethod(); }

    private void OnDrawGizmosSelected() { LegacyGimzosSelectedSetup(); }


    [Foldout("Set Parameters/Shooting System Parameters")]
    [SerializeField] private GameObject pot, shootingEffect;
    public Transform shootPoint;

    public override void CharacterAnimatorController()
    {
        base.CharacterAnimatorController();

        if (attackSpeedCount > attackSpeed)
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
            currentPot.GetComponent<Bomb>().ShootMovement(shootPoint, "enemy", enemyTransform, default);
            GameObject currnetEffect = Instantiate(shootingEffect, shootPoint.position, gameObject.transform.rotation);
            Destroy(currnetEffect, attackSpeedCount - 0.1f);
        }
    }
}