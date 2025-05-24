using MoreMountains.Feedbacks;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField]
    private InputManager inputManager;
    [SerializeField]
    Grid grid;

    [SerializeField]
    private ObjecstDatabaseSO dataBase;
    private int selectedObjectIndex = -1;

    [SerializeField] private GameObject gridVirtulization;

    private GridData floorData, furnitureData;

    private List<GameObject> placedGameObjects = new();

    [SerializeField]
    private PreviewSystem preview;

    private Vector3Int lastDetectedPosition = Vector3Int.zero;

    [SerializeField] private GameObject towersParent;

    [SerializeField] private float previewObjectHeight;

    //
    [SerializeField] private int firstTargetLayer = 2; // Ýlk Terrain Layer indexi (örneðin mavi)
    [SerializeField] private int secondTargetLayer = 3; // Ýkinci Terrain Layer indexi (örneðin kýrmýzý)
    [SerializeField] private float threshold = 0.5f; // Algýlama için minimum baskýnlýk eþiði

    [SerializeField] private CursorDetectorSystem cursorSc;
    [SerializeField] private CoinSystem coinSystem;

    [SerializeField] private MMF_Player buildingShake;
    [SerializeField] private MMF_Player buildSound;
    [SerializeField] private MMF_Player ironBuilSound;

    private void Start()
    {
        buildingShake = GetComponent<MMF_Player>();
        StopPlacement();
        floorData = new();
        furnitureData = new();
        buildSound = GameObject.Find("BuildingDownFeedBack").gameObject.GetComponent<MMF_Player>();
        ironBuilSound = GameObject.Find("BuildingDownIronFeedBack").gameObject.GetComponent<MMF_Player>();
    }
    public void StartPlacement(int ID)
    {
        StopPlacement();
        selectedObjectIndex = dataBase.objectsData.FindIndex(data => data.ID == ID);
        if (selectedObjectIndex < 0)
        {
            Debug.LogError($"No ID Found{ID}");
            return;
        }
        gridVirtulization.SetActive(true);
        preview.StartShowingPlacementPreview(
            dataBase.objectsData[selectedObjectIndex].Prefab,
            dataBase.objectsData[selectedObjectIndex].Size);
        InputManager.OnClicked += PlaceStructure;
        InputManager.OnExit += StopPlacement;
    }

    private void PlaceStructure()
    {
        if (inputManager.IsPointerOverUI())
        {
            return;
        }

        if (cursorSc.detected)
        {
            return;
        }

        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
        if (placementValidity == false)
        {
            preview.UpdatePositions(grid.CellToWorld(gridPosition), placementValidity);
            return;
        }

        if (inputManager.layerHit.collider != null && inputManager.layerHit.collider.GetComponent<Terrain>() != null && IsOnTargetTerrainLayer(inputManager.layerHit.point, GetTerrainFromHit()))
        {
            return;
        }

        if (coinSystem.coin - dataBase.objectsData[selectedObjectIndex].Money < 0)
        {
            return;
        }
        else
        {
            coinSystem.coin = coinSystem.coin - dataBase.objectsData[selectedObjectIndex].Money;
        }

        GameObject newObject = Instantiate(dataBase.objectsData[selectedObjectIndex].Prefab);
        newObject.transform.position = grid.CellToWorld(gridPosition);
        placedGameObjects.Add(newObject);
        GridData selectedData = dataBase.objectsData[selectedObjectIndex].ID == 0 ?
            floorData :
            furnitureData;
        selectedData.AddObjectAt(gridPosition,
            dataBase.objectsData[selectedObjectIndex].Size,
            dataBase.objectsData[selectedObjectIndex].ID,
            placedGameObjects.Count - 1);
        preview.UpdatePositions(grid.CellToWorld(gridPosition), false);

        switch (dataBase.objectsData[selectedObjectIndex].ObjectType)
        {
            case "CannonTower":
                newObject.transform.SetParent(towersParent.transform);
                newObject.GetComponentInChildren<TowerCannon>().isSpawn = true;
                newObject.GetComponent<BoxCollider>().enabled = true;
                buildingShake.PlayFeedbacks();
                ironBuilSound.PlayFeedbacks();
                break;
            case "CatapultTower":
                newObject.transform.SetParent(towersParent.transform);
                newObject.GetComponentInChildren<Catapult>().isSpawn = true;
                newObject.GetComponent<BoxCollider>().enabled = true;
                buildingShake.PlayFeedbacks();
                ironBuilSound.PlayFeedbacks();
                break;
            case "BowmanTower":
                newObject.transform.SetParent(towersParent.transform);
                newObject.GetComponentInChildren<BowmanTower>().isSpawn = true;
                newObject.GetComponent<BoxCollider>().enabled = true;
                buildingShake.PlayFeedbacks();
                ironBuilSound.PlayFeedbacks();
                break;
            case "MiniBase":
                newObject.transform.SetParent(towersParent.transform);
                newObject.GetComponentInChildren<FriendSpawner>().isSpawn = true;
                newObject.GetComponent<BoxCollider>().enabled = true;
                buildingShake.PlayFeedbacks();
                buildSound.PlayFeedbacks();
                break;
            case "Base":
                newObject.transform.SetParent(towersParent.transform);
                newObject.GetComponentInChildren<FriendSpawner>().isSpawn = true;
                newObject.GetComponent<BoxCollider>().enabled = true;
                buildingShake.PlayFeedbacks();
                buildSound.PlayFeedbacks();
                break;
            default:
                newObject.transform.SetParent(towersParent.transform);
                break;

        }
        StopPlacement();  // Böylece preview.StopShowingPreview() çaðrýlýr, event'ler unsubscribe edilir
    }

    Terrain GetTerrainFromHit()
    {
        if (inputManager.layerHit.collider != null && inputManager.layerHit.collider.gameObject.GetComponent<Terrain>() != null)
        {
            return inputManager.layerHit.collider.gameObject.GetComponent<Terrain>();
        }
        return null; // Hiçbir Terrain’e çarpmadýysa null döndür
    }
    bool IsOnTargetTerrainLayer(Vector3 worldPos, Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = worldPos - terrain.transform.position;
        float normX = terrainPos.x / terrainData.size.x;
        float normZ = terrainPos.z / terrainData.size.z;

        int x = Mathf.FloorToInt(normX * terrainData.alphamapWidth);
        int z = Mathf.FloorToInt(normZ * terrainData.alphamapHeight);

        float[,,] splatmapData = terrainData.GetAlphamaps(x, z, 1, 1);

        if (splatmapData[0, 0, firstTargetLayer] > threshold || splatmapData[0, 0, secondTargetLayer] > threshold)
        {
            return true; // Seçili iki layer’dan birine çarptý
        }

        return false;
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex)
    {
        GridData selectedData = dataBase.objectsData[selectedObjectIndex].ID == 0 ?
            floorData :
            furnitureData;

        return selectedData.CanPlaceObejctAt(gridPosition, dataBase.objectsData[selectedObjectIndex].Size);
    }


    private void StopPlacement()
    {
        selectedObjectIndex = -1;
        gridVirtulization.SetActive(false);
        preview.StopShowingPreview();
        InputManager.OnClicked -= PlaceStructure;
        InputManager.OnExit -= StopPlacement;
        lastDetectedPosition = Vector3Int.zero;
        cursorSc.detected = false;
    }

    private void Update()
    {
        if (selectedObjectIndex < 0)
            return;
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        if (lastDetectedPosition != gridPosition)
        {
            bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);

            preview.UpdatePositions(grid.CellToWorld(gridPosition), placementValidity);
            lastDetectedPosition = gridPosition;
        }

        Color c;
        if (cursorSc.detected || inputManager.layerHit.collider != null &&
            inputManager.layerHit.collider.GetComponent<Terrain>() != null &&
            IsOnTargetTerrainLayer(inputManager.layerHit.point, GetTerrainFromHit()) ||
            coinSystem.coin - dataBase.objectsData[selectedObjectIndex].Money < 0)
        {
            c = Color.red;
        }
        else
        {
            c = Color.white;
        }

        c.a = 0.5f;
        preview.cellIndicatorRenderer.material.color = c;
        preview.previewMaterialInstance.color = c;
    }
}