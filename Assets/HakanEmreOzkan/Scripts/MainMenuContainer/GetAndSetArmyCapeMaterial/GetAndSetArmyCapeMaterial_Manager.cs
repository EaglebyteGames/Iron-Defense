using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class GetAndSetArmyCapeMaterial_Manager : MonoBehaviour
{
    [SerializeField, ReadOnly] private GetContainer_OnScene _container;
    [SerializeField, ReadOnly] private SkinnedMeshRenderer sMeshRenderer;
    [SerializeField] private Material emptyMat;
    [SerializeField, ReadOnly] private List<Material> _containerFlagMaterials = new List<Material>();

    private void Awake()
    {
        if (FindAnyObjectByType<GetContainer_OnScene>() != null)
            _container = FindAnyObjectByType<GetContainer_OnScene>();

        sMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
    }
    private void Start()
    {
        #region First Step
        if (_container != null)
        {
            foreach (var mat in _container._flagMaterials)
            {
                if (!_containerFlagMaterials.Contains(mat))
                {
                    _containerFlagMaterials.Add(mat);
                }
            }
        }
        #endregion

        #region Second Setup
        //bendeki materyalleri ekle
        if (_containerFlagMaterials.Count > 0)
        {
            Material[] mats = sMeshRenderer.materials;
            mats[0] = emptyMat;
            mats[0].color = _containerFlagMaterials[1].color;
            sMeshRenderer.materials = mats;
        }
        #endregion
    }
}
