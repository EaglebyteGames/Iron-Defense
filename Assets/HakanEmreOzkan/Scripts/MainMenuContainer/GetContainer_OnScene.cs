using UnityEngine;
using VInspector;
using System.Collections.Generic;

public class GetContainer_OnScene : MonoBehaviour
{
    [SerializeField, ReadOnly] private DontDestroyContainer_MainMenu _container;
    [ReadOnly] public List<Material> _flagMaterials;

    private void Awake() { if (FindAnyObjectByType<DontDestroyContainer_MainMenu>() != null) _container = FindAnyObjectByType<DontDestroyContainer_MainMenu>();
        if (_container != null) foreach (var mat in _container.flagMaterials) if (!_flagMaterials.Contains(mat)) _flagMaterials.Add(mat);
    }
}
