using MoreMountains.Feedbacks;
using System.Collections;
using TMPro;
using UnityEngine;
using static WaveSpawner;

public class WaveSpawner : MonoBehaviour
{
    private enum SpawnState {SPAWNING, WAITING, COUNTING}

    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public Transform[] enemies;
        public int count;
        public float spawnTime;
    }

    [SerializeField] private BoxCollider[] spawnAreas;
    [SerializeField] private Wave[] waves;
    [SerializeField] private float timeBetweenWaves = 5f;
    private int nextWave = 0;
    [SerializeField] private float waveCountdown;
    private float searchCountdown = 1f;

    private SpawnState state = SpawnState.COUNTING;

    public bool winCondition;

    [SerializeField] private MMF_Player startSoundFeel;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private string currentWaveName;

    private void Start()
    {
        waveCountdown = timeBetweenWaves;
        startSoundFeel = gameObject.GetComponent<MMF_Player>();
    }

    private void Update()
    {
        if (state == SpawnState.WAITING)
        {
            if (!EnemyIsAlive()) WaveCompleted();
            else return;
        }

        if(waveCountdown <= 0)
        {
            if (state != SpawnState.SPAWNING) StartCoroutine(SpawnWave(waves[nextWave]));
        }
        else waveCountdown -= Time.deltaTime;
    }

    void WaveCompleted()
    {
        state = SpawnState.COUNTING;
        waveCountdown = timeBetweenWaves;

        if (nextWave + 1 > waves.Length - 1) nextWave = 0;
        else nextWave++;
    }

    bool EnemyIsAlive()
    {
        searchCountdown -= Time.deltaTime;

        if (searchCountdown <= 0f)
        {
            searchCountdown = 1f;
            if (GameObject.FindGameObjectWithTag("EnemyClose") == null && GameObject.FindGameObjectWithTag("EnemyFar") == null) return false;
        } 

        return true;
    }

    IEnumerator SpawnWave(Wave _wave)
    {
        state = SpawnState.SPAWNING;
        startSoundFeel.PlayFeedbacks();
        currentWaveName = _wave.waveName;
        Invoke("TextName", 0.05f);
        Invoke("DeleteName", 5f);

        for (int i = 0; i < _wave.count; i++)
        {
            if (_wave.enemies.Length > 0)
            {
                Transform enemy = _wave.enemies[Random.Range(0, _wave.enemies.Length)];
                SpawnEnemy(enemy);
            }

            if (_wave.waveName == "LastWave") winCondition = true;

            yield return new WaitForSeconds(_wave.spawnTime);
        }

        state = SpawnState.WAITING;

        yield break;
    }

    void SpawnEnemy(Transform _enemy)
    {
        BoxCollider selectedArea = spawnAreas[Random.Range(0, spawnAreas.Length)];
        Vector3 spawnPosition = GetRandomPointInSpawnArea(selectedArea);

        Instantiate(_enemy, spawnPosition, Quaternion.identity);
    }

    Vector3 GetRandomPointInSpawnArea(BoxCollider area)
    {
        Vector3 center = area.bounds.center;
        Vector3 size = area.bounds.size;

        float randomX = Random.Range(center.x - size.x / 2, center.x + size.x / 2);
        float randomY = center.y;
        float randomZ = Random.Range(center.z - size.z / 2, center.z + size.z / 2);

        return new Vector3(randomX, randomY, randomZ);
    }

    private void TextName()
    {
        if (currentWaveName != "LastWave")
        {
            waveText.text = "Wave " + currentWaveName;
        }
        else
        {
            waveText.text = "";
        }
    }
    private void DeleteName()
    {
        waveText.text = "";
    }
}