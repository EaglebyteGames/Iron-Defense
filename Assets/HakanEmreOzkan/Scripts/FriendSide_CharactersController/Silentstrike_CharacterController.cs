using FriendlyCharacterController = EagleByte.FriendlyCharacterController.FriendlyCharacterController;
using System;
using UnityEngine;

public class Silentstrike_CharacterController : FriendlyCharacterController
{
    private void Awake() { LegacyFirstSetup(); }

    private void Update() { LegacyRunUpdateMethod(); }

    private void OnDrawGizmosSelected() { LegacyGimzosSelectedSetup(); }
}
