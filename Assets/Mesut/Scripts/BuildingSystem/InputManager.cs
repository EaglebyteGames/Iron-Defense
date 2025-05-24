using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    [SerializeField] private Camera sceneCamera;

    private Vector3 lastPosition;

    [SerializeField] private LayerMask placementLayermask;

    public static Action OnClicked, OnExit;
    public GraphicRaycaster raycaster;
    public EventSystem eventSystem;

    public RaycastHit layerHit;

    private void OnGUI()
    {
        if (Input.GetMouseButtonDown(0))
            OnClicked?.Invoke();

        if (Input.GetMouseButtonDown(1))
        {
            OnExit?.Invoke();
        }
    }
    public bool IsPointerOverUI()
    {
        return IsPointerOverUIElement(out string uiElementName);
    }

    public bool IsPointerOverUIElement(out string elementName)
    {
        PointerEventData pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerEventData, results);

        if (results.Count > 0)
        {
            elementName = results[0].gameObject.name;
            return true;
        }

        elementName = "Boþ Alan";
        return false;
    }
    public Vector3 GetSelectedMapPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = sceneCamera.nearClipPlane;
        Ray ray = sceneCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out layerHit, 500, placementLayermask))
        {
            lastPosition = layerHit.point;
        }
        return lastPosition;
    }
}