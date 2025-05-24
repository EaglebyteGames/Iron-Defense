using UnityEngine;
using Unity.Cinemachine;
using Unity.VisualScripting;

public class CameraSystem : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    private Vector3 followOffset;
    [SerializeField] private bool useEdgeScroolling;

    //Variables
    [SerializeField] private float normalMoveSpeed;
    [SerializeField] private float fastMoveSpeed;
    private float moveSpeed;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float zoomAmount;

    [SerializeField] private float followOffsetMin;
    [SerializeField] private float followOffsetMax;

    //Col
    [SerializeField] private BoxCollider boundaryCollider;

    [SerializeField] private Transform minimapCamera;

    private void Awake()
    {
        followOffset = cinemachineCamera.GetComponent<CinemachineFollow>().FollowOffset;
    }

    private void Update()
    {
        MinimapCameraTransform();
        CameraMovement();
        CameraRotation();
        CameraZoom();
        BoundsController();
    }

    private void CameraMovement()
    {
        Vector3 inputDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W)) inputDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) inputDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) inputDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) inputDir.x = +1f;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed = fastMoveSpeed;
        }
        else
        {
            moveSpeed = normalMoveSpeed;
        }

        //
        if (useEdgeScroolling)
        {
            int edgeScrollSize = 20;
            if (Input.mousePosition.x < edgeScrollSize) inputDir.x = -1f;
            if (Input.mousePosition.y < edgeScrollSize) inputDir.z = -1f;
            if (Input.mousePosition.x > Screen.width - edgeScrollSize) inputDir.x = +1f;
            if (Input.mousePosition.y > Screen.height - edgeScrollSize) inputDir.z = +1f;
        }

        Vector3 moveDir = transform.forward * inputDir.z + transform.right * inputDir.x;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    private void CameraRotation()
    {
        float rotateDir = 0f;
        if (Input.GetKey(KeyCode.Q)) rotateDir = 1f;
        if (Input.GetKey(KeyCode.E)) rotateDir = -1f;

        transform.eulerAngles += new Vector3(0, rotateDir * rotateSpeed * Time.deltaTime, 0);
    }

    private void CameraZoom()
    {
        if (Input.mouseScrollDelta.y < 0)
        {
            followOffset.y += zoomAmount;
        }
        if (Input.mouseScrollDelta.y > 0)
        {
            followOffset.y -= zoomAmount;
        }

        if (followOffsetMin > followOffset.y)
        {
            followOffset.y = followOffsetMin;
        }
        if (followOffsetMax < followOffset.y)
        {
            followOffset.y = followOffsetMax;
        }

        cinemachineCamera.GetComponent<CinemachineFollow>().FollowOffset =
        Vector3.Lerp(cinemachineCamera.GetComponent<CinemachineFollow>().FollowOffset, followOffset, Time.deltaTime * zoomSpeed);
    }

    private void BoundsController()
    {
        // Collider’ýn global sýnýrlarýný al
        Bounds bounds = boundaryCollider.bounds;
        Vector3 pos = transform.position;

        // x, y ve z eksenlerinde konumu clamp et
        pos.x = Mathf.Clamp(pos.x, bounds.min.x, bounds.max.x);
        pos.y = Mathf.Clamp(pos.y, bounds.min.y, bounds.max.y);
        pos.z = Mathf.Clamp(pos.z, bounds.min.z, bounds.max.z);

        transform.position = pos;
    }
    private void MinimapCameraTransform() { minimapCamera.transform.position = new Vector3(gameObject.transform.position.x, minimapCamera.gameObject.transform.position.y, gameObject.transform.position.z); }
}