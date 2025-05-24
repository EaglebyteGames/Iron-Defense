using FriendlyCharacterController = EagleByte.FriendlyCharacterController.FriendlyCharacterController;
using System;
using UnityEngine;

public class IronValor_CharacterController : FriendlyCharacterController
{
    private void Awake() { LegacyFirstSetup(); }

    private void Update() { LegacyRunUpdateMethod(); }

    private void OnDrawGizmosSelected() { LegacyGimzosSelectedSetup(); }

}
