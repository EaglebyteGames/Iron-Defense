using System;
using UnityEngine;

public class SelectedSytemObject : MonoBehaviour
{
    private void Start()
    {
        ObjectSelectionManager.instance.allObjectsList.Add(gameObject);
    }

    private void OnDestroy()
    {
        ObjectSelectionManager.instance.allObjectsList.Remove(gameObject);
    }
}
