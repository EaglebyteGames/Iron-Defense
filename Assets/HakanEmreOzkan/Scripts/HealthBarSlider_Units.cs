using EagleByte.EnemyCharacterController;
using EagleByte.FriendlyCharacterController;
using UnityEngine;
using VInspector;

public class HealthBarSlider_Units : MonoBehaviour
{
    [SerializeField, ReadOnly] private Camera mainCamera;
    [Tooltip("canvasin icinde bulundugu parent obje")]
    [SerializeField, ReadOnly] private GameObject theUnit;
    [Tooltip("Eger dost birlik ise burasi dolu olacak")]
    [SerializeField, ReadOnly] private FriendlyCharacterController friendUnit;
    [Tooltip("Eger dusman birlik ise burasi dolu olacak")]
    [SerializeField, ReadOnly] private EnemyCharacterController enemyUnit;

    private void Awake()
    {
        mainCamera = Camera.main;

        theUnit = gameObject.transform.parent.gameObject;
        if (theUnit.gameObject.GetComponent<FriendlyCharacterController>() != null) friendUnit = theUnit.gameObject.transform.parent.gameObject.GetComponent<FriendlyCharacterController>();
        else if (theUnit.gameObject.GetComponent<EnemyCharacterController>() != null) enemyUnit = theUnit.gameObject.transform.parent.gameObject.GetComponent<EnemyCharacterController>();
    }

    private void FixedUpdate()
    {
        gameObject.transform.LookAt(mainCamera.transform.position);
    }
}
