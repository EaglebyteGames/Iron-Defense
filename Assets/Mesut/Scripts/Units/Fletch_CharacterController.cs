using FriendlyCharacterController = EagleByte.FriendlyCharacterController.FriendlyCharacterController;
using System;
using UnityEngine;
using VInspector;
using System.Collections;

public class Fletch_CharacterController : FriendlyCharacterController
{
    private void Awake() { LegacyFirstSetup(); }

    private void Update() { LegacyRunUpdateMethod(); }

    private void OnDrawGizmosSelected() { LegacyGimzosSelectedSetup(); }

    [Foldout("Set Parameters/Shooting System Parameters")]
    [Range(0, 500)] public float shootForceValue;
    [Range(0, 500)] public float upForceValue;
    [Range(0, 500), ReadOnly] public float upForce;
    private bool reload = false;

    [SerializeField] private GameObject arrow;
    [SerializeField] private Transform shootPoint;

    public override void CharacterAnimatorController()
    {
        base.CharacterAnimatorController();

        if (attackSpeedCount > attackSpeed)
        {
            StartCoroutine(ShootDelay(0.4f));
        }

        //Reload
        if (reload)
        {
            myAnimator.SetBool("reload", true);
            reload = !reload;
        }
        else
        {
            myAnimator.SetBool("reload", false);
        }
    }

    IEnumerator ShootDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Shoot();
    }

    private void Shoot()
    {
        if (amIDeath) return;

        reload = true;
        GameObject currentArrow = Instantiate(arrow, shootPoint.position, shootPoint.rotation);
        Transform targetTransform = enemyTransform.transform.GetChild(2).gameObject.transform;
        currentArrow.transform.LookAt(targetTransform);

        if (currentArrow != null && currentArrow.GetComponent<Rigidbody>() != null)
        {
            Vector3 direction = (targetTransform.position - currentArrow.transform.position).normalized;

            currentArrow.GetComponent<Rigidbody>().AddForce(direction * shootForceValue, ForceMode.Impulse);
            currentArrow.GetComponent<Rigidbody>().AddForce(Vector3.up * upForceValue, ForceMode.Impulse);
        }
    }
}
