using EnemyCharacterController = EagleByte.EnemyCharacterController.EnemyCharacterController;
using UnityEngine;
using System;
using System.Collections.Generic;
using VInspector;

public class Necromage_CharacterController : EnemyCharacterController
{
    [Foldout("Necromage/SpawnSystem Parameters")]
    [SerializeField, ReadOnly] private GameObject spawnPointsParent;
    [SerializeField, ReadOnly] private List<Transform> spawnPoints;
    [Space(10)]
    [SerializeField, Range(0, 10)] private float spawnEnemyTime = 0f;
    [SerializeField, Range(0, 10), ReadOnly] private float spawnEnemyTimeCount = 0f;
    [Space(10)]
    [SerializeField] private GameObject boneEnemyPrefab;
    [SerializeField, ReadOnly] private List<GameObject> spawnedObjects;
    [SerializeField, Range(0, 10)] private int maxSpawnedObjectsAmount = 6;
    private void Awake() { FirstSetup(); }

    private void Start() { SecondSetup(); }

    private void Update() { Updater(); }

    private void OnDrawGizmosSelected() { LegacyGimzosSelectedSetup(); }

    #region Setups

    private void FirstSetup()
    {
        // Default Setup
        LegacyFirstSetup();

        if (gameObject.transform.GetChild(0).gameObject.name == "SpawnPoints")
            spawnPointsParent = gameObject.transform.GetChild(0).gameObject;
    }

    private void SecondSetup()
    {
        // Default Setup
        LegacySecondSetup();


        if (spawnPoints.Count > 0) spawnPoints.Clear();
        else foreach (Transform child in spawnPointsParent.transform) if (!spawnPoints.Contains(child)) spawnPoints.Add(child);
    }

    #endregion

    #region Updater Methods
    private void Updater()
    {
        LegacyRunUpdateMethod();
        SpawnEnemyController();
    }

    private void SpawnEnemyController()
    {
        if (spawnEnemyTimeCount >= spawnEnemyTime)
        {
            if (spawnedObjects.Count < maxSpawnedObjectsAmount)
            {
                myAnimator.SetTrigger("GoSummon");
                GameObject obj1 = Instantiate(boneEnemyPrefab, spawnPoints[0].gameObject.transform.position, Quaternion.identity);
                if (!spawnedObjects.Contains(obj1)) spawnedObjects.Add(obj1);
                if (!(spawnedObjects.Count >= maxSpawnedObjectsAmount))
                {
                    GameObject obj2 = Instantiate(boneEnemyPrefab, spawnPoints[1].gameObject.transform.position, Quaternion.identity);
                    if (!spawnedObjects.Contains(obj2)) spawnedObjects.Add(obj2);
                }
            }

            spawnEnemyTimeCount = 0f;
        }
        else
        {
            spawnEnemyTimeCount += Time.deltaTime;
            spawnedObjects.RemoveAll(item => item == null);
        }
    }

    #endregion
}
