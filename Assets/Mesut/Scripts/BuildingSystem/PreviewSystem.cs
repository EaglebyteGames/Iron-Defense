using System;
using UnityEngine;

public class PreviewSystem : MonoBehaviour
{
    [SerializeField]
    private float previewOffset = 0.06f;

    [SerializeField]
    private GameObject cellIndicator;
    public GameObject previewObject;

    [SerializeField]
    private Material previewMaterialsPrefab;
    public Material previewMaterialInstance;

    public Renderer cellIndicatorRenderer;

    [SerializeField] private Transform seaLevel;

    [SerializeField] private float previewObjectHeight;
    private void Start()
    {
        previewMaterialInstance = new Material(previewMaterialsPrefab);
        cellIndicator.SetActive(false);
        cellIndicatorRenderer = cellIndicator.GetComponentInChildren<Renderer>();
    }

    public void StartShowingPlacementPreview(GameObject prefab, Vector2Int size)
    {
        previewObject = Instantiate(prefab);
        PreparePrevie(previewObject);
        PrepareCursoe(size);
        cellIndicator.SetActive(true);
    }

    private void PrepareCursoe(Vector2Int size)
    {
        if(size.x >0 || size.y > 0)
        {
            cellIndicator.transform.localScale = new Vector3(size.x, 1, size.y);
            cellIndicatorRenderer.material.mainTextureScale = size;
        }
    }

    private void PreparePrevie(GameObject previewObject)
    {
        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = previewMaterialInstance;
            }
            renderer.materials = materials;
        }
    }

    public void StopShowingPreview()
    {
        cellIndicator.SetActive(false);
        Destroy(previewObject);
    }

    public void UpdatePositions(Vector3 position, bool validity)
    {
        // Eðer önizleme objesi ya da malzeme örneklerimiz yoksa çýk
        if (cellIndicatorRenderer == null || previewMaterialInstance == null)
            return;

        MovePreview(position);
        MoveCursor(position);
        ApplyFeedback(validity);
    }

    private void ApplyFeedback(bool validity)
    {
        Color c = validity ? Color.white : Color.red;    
        c.a = 0.5f;
        cellIndicatorRenderer.material.color = c;
        previewMaterialInstance.color = c;
    }

    private void MoveCursor(Vector3 position)
    {
        cellIndicator.transform.position = position;
    }

    private void MovePreview(Vector3 position)
    {
        if (cellIndicatorRenderer == null)
            return;

        previewObject.transform.position = new Vector3(
            position.x, 
            position.y + previewOffset, 
            position.z);
    }
}