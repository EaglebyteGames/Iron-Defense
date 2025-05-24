using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.AI;

public class Gate : MonoBehaviour
{

    [SerializeField] private int targetLayer;
    Animator gateAnim;
    private NavMeshObstacle navMesh;
    [SerializeField] private MMF_Player gateSound;
    [SerializeField] private bool stay;

    private void Start()
    {
        gateAnim = GetComponent<Animator>();
        navMesh = GetComponent<NavMeshObstacle>();
        gateSound = GetComponent<MMF_Player>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == targetLayer)
        {
            gateAnim.SetBool("GateOpen", true);
            if (!stay)
            {
                gateSound.PlayFeedbacks();
            }
            navMesh.enabled = false;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == targetLayer)
        {
            gateAnim.SetBool("GateOpen", true);
            navMesh.enabled = false;
            stay = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == targetLayer)
        {
            gateAnim.SetBool("GateOpen", false);
            navMesh.enabled = true;
            stay = false;
        }
    }

}