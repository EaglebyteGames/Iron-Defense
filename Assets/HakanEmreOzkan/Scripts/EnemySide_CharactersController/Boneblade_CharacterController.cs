using EnemyCharacterController = EagleByte.EnemyCharacterController.EnemyCharacterController;
using System;
using UnityEngine;

public class Boneblade_CharacterController : EnemyCharacterController
{
    private void Awake() { LegacyFirstSetup(); }

    private void Start() { LegacySecondSetup(); }

    private void Update() { LegacyRunUpdateMethod(); }

    private void OnDrawGizmosSelected() { LegacyGimzosSelectedSetup(); }
}
