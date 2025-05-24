using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BuildingSystem : MonoBehaviour
{
    public Vector3 place;
    private RaycastHit hit;

    public GameObject objectToPlace;
    public GameObject tempObject;
    public GameObject[] buildObjects, tempObjects;
    private int currentObjects;

    public bool placeNow;
    public bool placeBarricade;
    public bool tempObjectExists;

    public bool rotateLeft, rotateRight;

    public int gold;
    private void Start()
    {
        
    }

    private void Update()
    {
        if (placeNow)
        {
            SendRay();
        }

        if (placeBarricade)
        {
            objectToPlace = buildObjects[currentObjects];
        }

        if(Input.GetKeyDown(KeyCode.Y))
        {  
            if(placeNow)
            {
                rotateLeft = true;
            }
        }

        if(Input.GetKeyUp(KeyCode.Y))
        {
            rotateLeft = false;
        }


        if (Input.GetKeyDown(KeyCode.U))
        {
            if (placeNow)
            {
                rotateRight = true;
            }
        }

        if (Input.GetKeyUp(KeyCode.U))
        {
            rotateRight = false;
        }

        if (rotateLeft)
        {
            RotateLeft();
        }

        if (rotateRight)
        {
            RotateRight();
        }
    }

    public void SendRay()
    {
        if (Physics.Raycast (Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        {
            place = new Vector3(hit.point.x, hit.point.y, hit.point.z);

            if (hit.transform.tag == "Graound")
            {
                if (!tempObjectExists)
                {
                    if (placeNow)
                    {
                        Instantiate(tempObjects[currentObjects], place, Quaternion.identity);
                        tempObject = GameObject.Find(buildObjects[currentObjects].name + "Temp(Clone)");
                        tempObjectExists = true;
                    }                
                }

                if(Input.GetMouseButton(0))
                {
                    if (placeNow)
                    {
                        Instantiate(objectToPlace, place, tempObject.transform.rotation);
                        placeNow = false;
                        placeBarricade = false;

                        Destroy(tempObject);
                        tempObjectExists = false;

                        gold--;
                    }
                }

                if(tempObject != null)
                {
                    tempObject.transform.position = place;
                }

                if(Input.GetMouseButtonDown(1))
                {
                    placeNow = false;
                    placeBarricade = false;

                    Destroy(tempObject);
                }
            }
        }
    }

    public void PlaceBarricade()
    {
        placeNow = true;
        placeBarricade = true;
    }

    public void RotateLeft()
    {
        tempObject.transform.Rotate(0f, -0.5f, 0f, Space.World);
    }

    public void RotateRight()
    {
        tempObject.transform.Rotate(0f, 0.5f, 0f, Space.World);
    }

    public void CurrentObject(int index)
    {
        currentObjects = index;

        if (gold > 0)
        {
            PlaceBarricade();
        }
    }
}