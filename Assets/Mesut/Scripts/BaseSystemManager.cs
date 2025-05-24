using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseSystemManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> wallsObj;
    [SerializeField] private List<TowerSeatManager> wallsSc;
    [SerializeField] private float castleHealth;
    public Slider castleHealthSlider;
    

    private void Awake()
    {
        castleHealthSlider = gameObject.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.GetComponent<Slider>();
    }

    private void Start()
    {
        foreach (Transform obj in gameObject.transform)
        {
            if (!wallsObj.Contains(obj.gameObject) && obj.gameObject.CompareTag("Wall"))
            {
                wallsObj.Add(obj.gameObject);
            }
        }

        foreach (GameObject walls in wallsObj)
        {
            if (!wallsSc.Contains(walls.gameObject.GetComponent<TowerSeatManager>()))
            {
                wallsSc.Add(walls.gameObject.GetComponent<TowerSeatManager>());
            }
        }

        //CastleHealth gate den geliyor
        castleHealth = wallsSc[0].health;
    }

    private void FixedUpdate()
    {
        HealthControl();
        HealthSliderController();
        CheckLoseCondition();
    }

    private void HealthControl()
    {
        foreach (TowerSeatManager sc in wallsSc)
        {
            if (sc.health < castleHealth)
            {
                castleHealth = sc.health;
            }
        }
    }

    private void HealthSliderController()
    {
        castleHealthSlider.value = castleHealth;
        castleHealthSlider.transform.parent.LookAt(Camera.main.transform.position);
    }

    public bool CheckLoseCondition()
    {
        if (castleHealthSlider.value <= 0) return true;
        else return false;
    }
}
