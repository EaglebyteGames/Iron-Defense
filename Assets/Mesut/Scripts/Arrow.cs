using EagleByte.EnemyCharacterController;
using EagleByte.FriendlyCharacterController;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private int enemyLayer;
    [SerializeField] private int enemyLayer2;
    [SerializeField] private int enemyLayer3;
    [SerializeField] private float damage;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer == enemyLayer || collider.gameObject.layer == 3 ||collider.gameObject.layer == enemyLayer2 || collider.gameObject.layer == enemyLayer3)
        {
            Destroy(gameObject);
            if (collider.GetComponent<EnemyCharacterController>() != null)
            {
                collider.gameObject.GetComponent<EnemyCharacterController>().health -= damage;
            }
            if (collider.GetComponent<FriendlyCharacterController>() != null)
            {
                collider.gameObject.GetComponent<FriendlyCharacterController>().health -= damage;
            }
            if (collider.gameObject.GetComponent<TowerSeatManager>() != null)
            {
                collider.gameObject.GetComponent<TowerSeatManager>().health -= damage;
            }
        }  
    }
}