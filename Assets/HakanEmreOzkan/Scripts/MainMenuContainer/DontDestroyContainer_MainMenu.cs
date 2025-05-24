using EagleByte.FlagSystem;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class DontDestroyContainer_MainMenu : MonoBehaviour
{
    public List<Material> flagMaterials = new List<Material>();
    [SerializeField, ReadOnly] private GameObject flag;
    [SerializeField, ReadOnly] private SkinnedMeshRenderer flagMeshRenderer;

    private void Awake() { DontDestroyOnLoad(gameObject); }
    private void Start()
    {
        if (FindAnyObjectByType<SymbolController>() != null)
            flag = FindAnyObjectByType<SymbolController>().GameObj[0];
        if (flag.gameObject.GetComponent<SkinnedMeshRenderer>() != null)
            flagMeshRenderer = flag.gameObject.GetComponent<SkinnedMeshRenderer>();

        if (flagMeshRenderer != null)
            foreach (var mat in flagMeshRenderer.materials) if (!flagMaterials.Contains(mat)) flagMaterials.Add(mat);
    }
}
