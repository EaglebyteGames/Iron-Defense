using UnityEngine;

public class CursorDetectorSystem : MonoBehaviour
{
    public bool detected;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("Tower")
            || other.gameObject.CompareTag("FriendClose") || other.gameObject.CompareTag("FriendFar")
            || other.gameObject.CompareTag("EnemyFar") || other.gameObject.CompareTag("EnemyClose"))
        {
            detected = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("Tower")
           || other.gameObject.CompareTag("FriendClose") || other.gameObject.CompareTag("FriendFar")
           || other.gameObject.CompareTag("EnemyFar") || other.gameObject.CompareTag("EnemyClose"))
        {
            detected = false;
        }
    }
}
