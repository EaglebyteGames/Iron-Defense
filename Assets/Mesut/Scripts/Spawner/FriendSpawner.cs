using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using EagleByte.FriendlyCharacterController;
using TMPro;

public class FriendSpawner : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private GameObject buttonCanvas;
    [SerializeField] private Transform targetLocation;
    private Camera _cam;

    [SerializeField] private GameObject[] spawnObjects;
    [SerializeField] private GameObject[] level1Objects;
    [SerializeField] private GameObject[] level2Objects;
    [SerializeField] private GameObject[] level3Objects;
    [SerializeField] private GameObject[] level4Objects;
    [SerializeField] private GameObject[] level5Objects;

    [SerializeField] private Button[] spawnButtons;
    [SerializeField] private float[] cooldownTimes;
    [SerializeField] private Image[] cooldownImages;

    private bool isSelected;
    private Coroutine[] cooldownRoutines;

    public bool isSpawn;

    public int levelCount = 1;

    [SerializeField] private int[] money;
    [SerializeField] private int levelPrice;
    [SerializeField] private CoinSystem coinSystem;
    [SerializeField] private TextMeshProUGUI levelText;


    [SerializeField] private TextMeshProUGUI[] coinName;
    [SerializeField] private TextMeshProUGUI levelTextMoney;

    private void Awake()
    {
        coinSystem = FindAnyObjectByType<CoinSystem>();
        isSpawn = false;
        levelText = gameObject.transform.GetChild(0).GetChild(3).GetComponent<TextMeshProUGUI>();
        levelText.text = "Level " + levelCount;
    }

    private void Start()
    {
        _cam = Camera.main;
        isSelected = false;
        cooldownRoutines = new Coroutine[spawnButtons.Length];

        foreach (var img in cooldownImages)
            img.fillAmount = 0f;

        for (int i = 0; i < money.Length; i++)
        {
            coinName[i].text = money[i].ToString();
        }

        levelTextMoney.text = levelPrice.ToString();
    }

    private void Update()
    {
        buttonCanvas.SetActive(isSelected);

        if (Input.GetMouseButtonDown(1))
            isSelected = false;
        buttonCanvas.transform.rotation = Quaternion.LookRotation(buttonCanvas.transform.position - _cam.transform.position);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isSpawn)
        {
            isSelected = true;
        }
    }

    public void LevelUp()
    {
        if (coinSystem.coin - levelPrice > 0 && levelCount < 5)
        {
            coinSystem.coin = coinSystem.coin - levelPrice;

            levelCount++;
            levelText.text = "Level " + levelCount;

            switch (levelCount)
            {
                case 1:
                    spawnObjects = level1Objects;
                    break;
                case 2:
                    spawnObjects = level2Objects;
                    break;
                case 3:
                    spawnObjects = level3Objects;
                    break;
                case 4:
                    spawnObjects = level4Objects;
                    break;
                case 5:
                    spawnObjects = level5Objects;
                    break;
            }
        }
    }

    public void SpawnNewObject(int buttonNumber)
    {
        if (spawnButtons[buttonNumber].interactable && coinSystem.coin - money[buttonNumber] > 0)
        {
            coinSystem.coin = coinSystem.coin - money[buttonNumber];

            GameObject newObject = spawnObjects[buttonNumber];
            newObject.GetComponent<FriendlyCharacterController>().targetPosition = targetLocation.position;
            newObject.GetComponent<FriendlyCharacterController>().goMoveNow = true;

            Instantiate(newObject, transform.position + new Vector3(0, 0, 5), Quaternion.identity);

            ResetAllCooldowns();
        }
    }

    private void ResetAllCooldowns()
    {
        for (int i = 0; i < cooldownRoutines.Length; i++)
        {
            if (cooldownRoutines[i] != null)
                StopCoroutine(cooldownRoutines[i]);

            cooldownRoutines[i] = StartCoroutine(StartCooldown(i, cooldownTimes[i]));
        }
    }

    private IEnumerator StartCooldown(int buttonIndex, float cooldownTime)
    {
        spawnButtons[buttonIndex].interactable = false;
        cooldownImages[buttonIndex].fillAmount = 1f;

        float timer = 0f;
        while (timer < cooldownTime)
        {
            timer += Time.deltaTime;
            cooldownImages[buttonIndex].fillAmount = 1f - (timer / cooldownTime);
            yield return null;
        }

        cooldownImages[buttonIndex].fillAmount = 0f;
        spawnButtons[buttonIndex].interactable = true;
    }
}