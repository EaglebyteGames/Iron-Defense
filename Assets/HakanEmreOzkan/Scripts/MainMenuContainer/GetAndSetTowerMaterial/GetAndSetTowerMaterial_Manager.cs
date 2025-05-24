using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class GetAndSetTowerMaterial_Manager : MonoBehaviour
{
    [SerializeField, ReadOnly] private GetContainer_OnScene _container;
    [SerializeField, ReadOnly] private MeshRenderer meshRenderer;
    [SerializeField] private Material emptyMat;
    [SerializeField, ReadOnly] private List<Material> _containerFlagMaterials = new List<Material>();

    private void Awake()
    {
        if (FindAnyObjectByType<GetContainer_OnScene>() != null)
            _container = FindAnyObjectByType<GetContainer_OnScene>();

        meshRenderer = gameObject.GetComponent<MeshRenderer>();
    }
    private void Start()
    {
        #region First Step
        if (_container != null) foreach (var mat in _container._flagMaterials) if (!_containerFlagMaterials.Contains(mat)) _containerFlagMaterials.Add(mat);
        #endregion

        #region Second Setup
        //bendeki materyalleri ekle
        if (_containerFlagMaterials.Count > 0)
        {
            Material[] mats = meshRenderer.materials;
            if (mats.Length > 1) mats[1] = emptyMat;
            mats[1].color = _containerFlagMaterials[1].color;
            meshRenderer.materials = mats;
        }
        #endregion
    }
}
