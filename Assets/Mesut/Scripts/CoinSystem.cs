using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoinSystem : MonoBehaviour
{
    public int coin;
    [SerializeField] private int coinPlus;
    [SerializeField] private float interval = 5f;
    [SerializeField] private TextMeshProUGUI coinText;

    
    private void Start()
    {
        StartCoroutine(IncreaseCoin());
    }

    private void Update()
    {
        coinText.text = coin.ToString();
        if (Input.GetKeyDown(KeyCode.Y))
        {
            coin += 100;
        }
    }

    IEnumerator IncreaseCoin()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            coin = coin + coinPlus;
            coinText.text = coin.ToString();
        }
    }
}