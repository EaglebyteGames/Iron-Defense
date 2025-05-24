using EnemyCharacterController = EagleByte.EnemyCharacterController.EnemyCharacterController;
using UnityEngine;
using System;

public class Deathshiver_CharacterController : EnemyCharacterController
{
    private void Awake() { LegacyFirstSetup(); }

    private void Start() { LegacySecondSetup(); }

    private void Update() { LegacyRunUpdateMethod(); }

    private void OnDrawGizmosSelected() { LegacyGimzosSelectedSetup(); }
}
