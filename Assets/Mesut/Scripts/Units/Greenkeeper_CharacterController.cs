using FriendlyCharacterController = EagleByte.FriendlyCharacterController.FriendlyCharacterController;
using UnityEngine;
using VInspector;

public class Greenkeeper_CharacterController : FriendlyCharacterController
{
    private void Awake() { LegacyFirstSetup(); }

    private void Update() { LegacyRunUpdateMethod(); }

    private void OnDrawGizmosSelected() { LegacyGimzosSelectedSetup(); }


    [Foldout("Set Parameters/Shooting System Parameters")]
    [SerializeField] private GameObject healingEffect;
    [SerializeField] private Transform healingPoint;

    public override void CharacterAnimatorController()
    {
        base.CharacterAnimatorController();

        if (attackSpeedCount > attackSpeed)
        {
            Healing();
        }
    }
    private void Healing()
    {
        if (amIDeath) return;
        foreach (var friend in enemyColliders)
        {
            friend.GetComponent<FriendlyCharacterController>().health += 5;
        }

        GameObject currnetEffect = Instantiate(healingEffect, healingPoint.position, healingPoint.rotation);
        Destroy(currnetEffect, attackSpeedCount - 0.1f);
    }
}