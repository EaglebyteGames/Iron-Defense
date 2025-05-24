using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using VInspector;
using UnityEditor;
using UnityEngine.EventSystems;

public class TowerSeatManager : MonoBehaviour, IPointerDownHandler
{
    [Header("Tower Settings")]
    [Tooltip("Dairenin yarýçapý, yapay zekanýn hedef noktasý bu mesafede belirlenecek.")]
    public float radius = 5f;
    [Tooltip("Dairenin yüksekliði, objenin pozisyonuna eklenecek.")]
    public float circleHeight = 0f;

    [Header("Wall Settings")]
    [Tooltip("Wall objesi için çizgi uzunluðu.")]
    public float lineLength = 5f;
    [Tooltip("Wall objesi için çizgi rotasyonu (derece cinsinden).")]
    public float lineRotation = 0f;
    [Tooltip("Wall objesi için çizginin merkeze olan uzaklýðý.")]
    public float lineCenterOffset = 0f;

    [Foldout("Health & Damage Parameters")]
    [Range(0, 500)] public float health;
    [Range(0, 500), ReadOnly] public float legacyHealth;
    [Range(0, 5000), ReadOnly] public float castleHealth;
    [SerializeField, ReadOnly] private Slider healthBarSlider;
    [SerializeField, ReadOnly] private GameObject levelButton;
    public bool amIDeath;
    public bool isSelected;

    [SerializeField, ReadOnly] ParticleSystem buildingSmokeEffect;
    [SerializeField, ReadOnly] BoxCollider col;

    [SerializeField] GetAndSetTowerMaterial_Manager towerMaterialManager;
    [EndFoldout]

    private void Awake()
    {
        if (gameObject.CompareTag("Tower"))
        {
            healthBarSlider = GetComponentInChildren<Canvas>().transform.GetChild(0).GetComponent<Slider>();
            levelButton = GetComponentInChildren<Canvas>().transform.GetChild(1).gameObject;
            buildingSmokeEffect = GetComponentInChildren<ParticleSystem>();
            buildingSmokeEffect.gameObject.SetActive(false);
            col = GetComponent<BoxCollider>();
        }
        if (gameObject.CompareTag("Wall"))
        {
            healthBarSlider = GetComponentInParent<BaseSystemManager>().castleHealthSlider;
        }
    }

    private void Start()
    {
        // Health Bar slider setup
        if (gameObject.CompareTag("Tower"))
        {
            legacyHealth = health;
            healthBarSlider.maxValue = legacyHealth;
            healthBarSlider.minValue = 0;
            healthBarSlider.value = legacyHealth;
            levelButton.SetActive(false);
        }
    }

    private void Update()
    {
        if (gameObject.CompareTag("Tower"))
        {
            UnSelected();

            if (col.enabled)
            {
                buildingSmokeEffect.gameObject.SetActive(true);

                if (towerMaterialManager != null)
                {
                    towerMaterialManager.enabled = true;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        SliderHealthBarController();
        DeathControl();
    }

    public void SliderHealthBarController()
    {
        if (gameObject.CompareTag("Tower"))
        {
            healthBarSlider.value = health;
            healthBarSlider.transform.parent.LookAt(Camera.main.transform.position);
        }
    }

    private void UnSelected()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    levelButton.SetActive(false);
        //}
        if (Input.GetMouseButtonDown(1))
        {
            isSelected = false;
            levelButton.SetActive(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (gameObject.CompareTag("Tower"))
        {
            isSelected = true;
            levelButton.SetActive(true);
        }
    }

    public virtual void DeathControl()
    {
        if (health <= 0)
        {
            amIDeath = true;
            if (gameObject.CompareTag("Tower"))
            {
                buildingSmokeEffect.gameObject.transform.SetParent(null);
                buildingSmokeEffect.Play();
                StartCoroutine(DeathNumerator());
            }
        }
    }

    private IEnumerator DeathNumerator()
    {
        yield return new WaitForSeconds(2);
        Destroy(gameObject);
    }

    public Vector3 GetRandomPoint()
    {
        if (gameObject.CompareTag("Tower"))
        {
            return GetRandomPointOnCircle();
        }
        else if (gameObject.CompareTag("Wall"))
        {
            return GetRandomPointOnLine();
        }
        else
        {
            return GetRandomPointOnCircle();
        }
    }

    private Vector3 GetRandomPointOnCircle()
    {
        float angle = Random.Range(0f, 2 * Mathf.PI);
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
        return new Vector3(transform.position.x + offset.x, transform.position.y + circleHeight, transform.position.z + offset.z);
    }

    private Vector3 GetRandomPointOnLine()
    {
        Vector3 baseCenter = transform.position + Vector3.up * circleHeight;

        Quaternion rotationQuat = Quaternion.Euler(0, lineRotation, 0);
        Vector3 lineDirection = rotationQuat * Vector3.right;

        Vector3 perpendicular = Vector3.Cross(Vector3.up, lineDirection).normalized;
        Vector3 lineCenter = baseCenter + perpendicular * lineCenterOffset;
        float randomOffset = Random.Range(-lineLength / 2f, lineLength / 2f);
        return lineCenter + lineDirection * randomOffset;
    }

    //private void OnDrawGizmos()
    //{
    //    Vector3 baseCenter = transform.position + Vector3.up * circleHeight;
    //    if (gameObject.CompareTag("Wall"))
    //    {
    //        Quaternion rotationQuat = gameObject.transform.rotation * Quaternion.Euler(0, lineRotation, 0);
    //        Vector3 lineDirection = rotationQuat * Vector3.right;
    //        Vector3 perpendicular = Vector3.Cross(Vector3.up, lineDirection).normalized;
    //        Vector3 lineCenter = baseCenter + perpendicular * lineCenterOffset;
    //        Vector3 startPoint = lineCenter - lineDirection * (lineLength / 2f);
    //        Vector3 endPoint = lineCenter + lineDirection * (lineLength / 2f);
    //        Handles.color = Color.yellow;
    //        Handles.DrawLine(startPoint, endPoint);
    //    }
    //    else if (gameObject.CompareTag("Tower"))
    //    {
    //        Handles.color = Color.yellow;
    //        Handles.DrawWireDisc(baseCenter, Vector3.up, radius);
    //    }
    //    else
    //    {
    //        Handles.color = Color.yellow;
    //        Handles.DrawWireDisc(baseCenter, Vector3.up, radius);
    //    }
    //}
}
