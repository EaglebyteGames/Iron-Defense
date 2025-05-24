using System.Collections.Generic;
using EagleByte.FriendlyCharacterController;
using UnityEngine;

public class ObjectSelectionManager : MonoBehaviour
{
    public static ObjectSelectionManager instance { get; set; }
    public List<GameObject> allObjectsList = new List<GameObject>();
    public List<GameObject> objectSelected = new List<GameObject>();

    public LayerMask friendLayer;
    public LayerMask enemyLayer;
    public LayerMask ground;
    public GameObject groundMarker;
    RaycastHit hitGround;

    private Camera cam;

    // Cursor
    public Texture2D enemyCursor;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;

    public GameObject selectedObj;
    public bool dusmanaTiklandi;

    // Formation deðiþkenleri
    private bool isFormingFormation = false;
    private Vector3 formationStartPos;
    private Vector3 formationCurrentPos;
    private Vector3 formationNormal; // Týklanan yerin yüzey normali
    public float formationDragThreshold = 1f; // Formation moduna geçmek için gereken minimum mesafe

    // Minimum nokta aralýðý (eðer bu deðerin altýna düþerse, noktalar uzun olan eksene göre sýralanýr)
    public float minPointSpacing = 1f;

    // Formation noktalarýný temsil eden prefab ve runtime’da oluþturulmuþ marker listesi
    public GameObject formationPointPrefab;
    private List<GameObject> formationPointMarkers = new List<GameObject>();

    // Tek bir LineRenderer ile (hem formation dýþ sýnýrýný hem de grid çizgilerini) çizim yapýlacak
    public LineRenderer formationRectangleLineRenderer;

    // Hesaplanan formation noktalarýnýn konumlarýný tutan liste
    private List<Vector3> currentFormationPoints = new List<Vector3>();
    // ComputeFormationPoints() tarafýndan üretilen grid boyutlarý
    private int currentFormationColumns;
    private int currentFormationRows;

    public GameObject animationObj;

    private void Awake()
    {
        groundMarker = gameObject.transform.GetChild(0).gameObject;
        if (instance != null && instance != this)
            Destroy(gameObject);
        else
            instance = this;
    }

    private void Start()
    {
        cam = Camera.main;
        // Eðer LineRenderer atanmadýysa otomatik oluþtur.
        if (formationRectangleLineRenderer == null)
        {
            GameObject lineObj = new GameObject("FormationGridLines");
            formationRectangleLineRenderer = lineObj.AddComponent<LineRenderer>();
            formationRectangleLineRenderer.loop = false;
            formationRectangleLineRenderer.startWidth = 0.1f;
            formationRectangleLineRenderer.endWidth = 0.1f;
        }
    }

    private void Update()
    {
        if (objectSelected.Count > 0)
        {
            foreach (GameObject obj in objectSelected)
            {
                if (obj != null)
                    obj.transform.GetChild(4).gameObject.SetActive(false);
            }
            animationObj = objectSelected[0];
            animationObj.transform.GetChild(4).gameObject.SetActive(true);
        }
        else animationObj = null;

        foreach (var obj in allObjectsList)
        {
            if (obj != null)
            {
                FriendlyCharacterController friendly = obj.GetComponent<FriendlyCharacterController>();

                friendly.selectedUI.SetActive(false);
            }
        }

        // Seçili birimler için mevcut FriendlyCharacterController ayarlamalarý
        foreach (var obj in objectSelected)
        {
            if (obj != null)
            {
                FriendlyCharacterController friendly = obj.GetComponent<FriendlyCharacterController>();

                friendly.selectedUI.SetActive(true);

                if (selectedObj != null)
                    dusmanaTiklandi = true;

                if (obj.CompareTag("FriendFar") && selectedObj != null && hitGround.collider != null)
                    friendly.hitObj = hitGround.collider.gameObject;
                if (obj.CompareTag("FriendFar") && friendly.enemyColliders.Length > 0)
                {
                    friendly.agent.stoppingDistance = 30f;
                    if (groundMarker.activeInHierarchy && Vector3.Distance(obj.transform.position, groundMarker.transform.position) > 1 && !dusmanaTiklandi)
                        friendly.agent.stoppingDistance = 1f;
                }
                else if (obj.CompareTag("FriendFar") && friendly.enemyColliders.Length <= 0)
                    friendly.agent.stoppingDistance = 1f;
            }
        }

        // Sol fareyle seçim (MultiSelect ve Tekli Seçim)
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, friendLayer))
            {
                if (!hit.collider.gameObject.GetComponent<FriendlyCharacterController>().amIDeath)
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                        MultiSelect(hit.collider.gameObject);
                    else
                        SelecByClicking(hit.collider.gameObject);
                }
            }
            else
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                    DeselectAll();
            }
        }

        // Sað fare ile formation komutlarý
        if (objectSelected.Count > 0)
        {
            // Sað fareye basýldýðýnda formation baþlangýç noktasýný ve yüzey normalini belirle
            if (Input.GetMouseButtonDown(1))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hitGround, Mathf.Infinity, ground))
                {
                    formationStartPos = hitGround.point;
                    formationCurrentPos = formationStartPos;
                    formationNormal = hitGround.normal;
                    isFormingFormation = true;
                }
            }

            // Sað fare basýlýyken formation güncelle
            if (Input.GetMouseButton(1) && isFormingFormation)
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hitGround, Mathf.Infinity, ground))
                    formationCurrentPos = hitGround.point;

                DrawFormationRectangle();
                currentFormationPoints = ComputeFormationPoints();
                UpdateFormationMarkers(currentFormationPoints);
            }

            // Sað fare býrakýldýðýnda formation komutunu uygula veya basit komut ver, ardýndan görselleri temizle
            if (Input.GetMouseButtonUp(1) && isFormingFormation)
            {
                if (Vector3.Distance(formationStartPos, formationCurrentPos) > formationDragThreshold)
                    IssueFormationOrders();
                else
                    IssueSimpleCommand();
                isFormingFormation = false;
                ClearFormationVisuals();
            }
        }

        // Ýmleç enemyLayer üzerindeyse özel cursor ayarla
        Ray ray1 = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray1, out hitGround, Mathf.Infinity, enemyLayer))
            Cursor.SetCursor(enemyCursor, hotSpot, cursorMode);
        else
            Cursor.SetCursor(null, hotSpot, cursorMode);

        if (isFormingFormation)
            dusmanaTiklandi = false;
    }

    private void MultiSelect(GameObject unit)
    {
        if (!objectSelected.Contains(unit))
        {
            TriggerSelectionIndicator(unit, true);
            objectSelected.Add(unit);
        }
        else
        {
            TriggerSelectionIndicator(unit, false);
            objectSelected.Remove(unit);
        }
    }

    public void DeselectAll()
    {
        foreach (var obj in objectSelected)
        {
            if (obj != null)
            {
                TriggerSelectionIndicator(obj, false);
                obj.GetComponent<FriendlyCharacterController>().targetPosition = Vector3.zero;
                obj.GetComponent<FriendlyCharacterController>().justStop = true;
            }
        }
        groundMarker.SetActive(false);
        objectSelected.Clear();
        animationObj = null;
    }

    private void SelecByClicking(GameObject unit)
    {
        DeselectAll();
        TriggerSelectionIndicator(unit, true);
        if (!objectSelected.Contains(unit))
            objectSelected.Add(unit);
    }

    private void TriggerSelectionIndicator(GameObject unit, bool isVisible)
    {
        unit.transform.GetChild(0).gameObject.SetActive(isVisible);
    }

    internal void DragSelect(GameObject unit)
    {
        if (!objectSelected.Contains(unit))
        {
            objectSelected.Add(unit);
            TriggerSelectionIndicator(unit, true);
        }
    }

    // ----- Formation & Komut Metotlarý -----

    // Týklanan yerin eðimine göre formation dörtgeninin köþelerini çizer
    private void DrawFormationRectangle()
    {
        // Hesaplama yapmak için formationStartPos ve formationCurrentPos arasýndaki farký kullanacaðýz.
        Vector3 diff = formationCurrentPos - formationStartPos;

        // formationNormal'dan yola çýkarak iki lokal eksen oluþturuyoruz.
        Vector3 planeRight = Vector3.Cross(formationNormal, Vector3.up);
        if (planeRight.sqrMagnitude < 0.001f)
            planeRight = Vector3.right;
        planeRight.Normalize();
        Vector3 planeForward = Vector3.Cross(planeRight, formationNormal).normalized;

        // Diff'yi planeRight ve planeForward eksenlerine projekte ediyoruz.
        float u = Vector3.Dot(diff, planeRight);
        float v = Vector3.Dot(diff, planeForward);

        // Dörtgen köþeleri
        Vector3 A = formationStartPos;
        Vector3 B = formationStartPos + planeRight * u;
        Vector3 C = formationStartPos + planeRight * u + planeForward * v;
        Vector3 D = formationStartPos + planeForward * v;

        // Çizgiler: A -> B -> C -> D -> A
        formationRectangleLineRenderer.positionCount = 5;
        formationRectangleLineRenderer.SetPosition(0, A);
        formationRectangleLineRenderer.SetPosition(1, B);
        formationRectangleLineRenderer.SetPosition(2, C);
        formationRectangleLineRenderer.SetPosition(3, D);
        formationRectangleLineRenderer.SetPosition(4, A);
    }

    // Seçili birim sayýsýna göre formation noktalarýný, týklanan yüzeyin eðimine uygun þekilde hesaplar.
    private List<Vector3> ComputeFormationPoints()
    {
        List<Vector3> formationPoints = new List<Vector3>();
        int count = objectSelected.Count;
        if (count == 0)
            return formationPoints;

        // formationStartPos ve formationCurrentPos arasýndaki fark
        Vector3 diff = formationCurrentPos - formationStartPos;

        // formationNormal'dan lokal eksenleri oluþtur
        Vector3 planeRight = Vector3.Cross(formationNormal, Vector3.up);
        if (planeRight.sqrMagnitude < 0.001f)
            planeRight = Vector3.right;
        planeRight.Normalize();
        Vector3 planeForward = Vector3.Cross(planeRight, formationNormal).normalized;

        // Farký, lokal eksenlere projekte et
        float u = Vector3.Dot(diff, planeRight);
        float v = Vector3.Dot(diff, planeForward);

        // Grid boyutlarý: karekök temelli hesaplama
        int columns = Mathf.CeilToInt(Mathf.Sqrt(count));
        int rows = Mathf.CeilToInt((float)count / columns);

        // Eðer dar olan eksende spacing istenen deðerin altýndaysa, tek satýr/sütun þeklinde düzenle
        float spacingX = (columns > 1) ? Mathf.Abs(u) / (columns - 1) : Mathf.Abs(u);
        float spacingZ = (rows > 1) ? Mathf.Abs(v) / (rows - 1) : Mathf.Abs(v);
        if (Mathf.Abs(u) < Mathf.Abs(v) && spacingX < minPointSpacing)
        {
            columns = 1;
            rows = count;
        }
        else if (Mathf.Abs(v) < Mathf.Abs(u) && spacingZ < minPointSpacing)
        {
            rows = 1;
            columns = count;
        }

        currentFormationColumns = columns;
        currentFormationRows = rows;

        // Grid þeklinde formation noktalarýný hesapla (satýr-sütun üzerinden)
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (formationPoints.Count >= count)
                    break;
                // Eðer tek sütun veya satýr varsa orta deðeri al
                float t = (columns > 1) ? c / (float)(columns - 1) : 0.5f;
                float s = (rows > 1) ? r / (float)(rows - 1) : 0.5f;
                Vector3 point = formationStartPos + planeRight * (t * u) + planeForward * (s * v);
                formationPoints.Add(point);
            }
        }
        return formationPoints;
    }

    // Formation noktalarýný marker prefablarý ile günceller.
    private void UpdateFormationMarkers(List<Vector3> positions)
    {
        while (formationPointMarkers.Count < positions.Count)
        {
            GameObject marker = Instantiate(formationPointPrefab);
            formationPointMarkers.Add(marker);
        }
        for (int i = 0; i < formationPointMarkers.Count; i++)
        {
            if (i < positions.Count)
            {
                formationPointMarkers[i].transform.position = positions[i];
                if (!formationPointMarkers[i].activeSelf)
                    formationPointMarkers[i].SetActive(true);
            }
            else
            {
                if (formationPointMarkers[i].activeSelf)
                    formationPointMarkers[i].SetActive(false);
            }
        }
    }

    // Formation komutunu uygular: Hesaplanan formation noktalarýný birimlere atar.
    private void IssueFormationOrders()
    {
        currentFormationPoints = ComputeFormationPoints();
        int index = 0;
        foreach (Vector3 formationPos in currentFormationPoints)
        {
            if (index >= objectSelected.Count)
                break;
            GameObject unit = objectSelected[index];
            if (unit != null)
            {
                FriendlyCharacterController friendly = unit.GetComponent<FriendlyCharacterController>();
                friendly.targetPosition = formationPos;
                friendly.goMoveNow = true;
                friendly.justStop = false;
                friendly.letGoAttackNow = false;
                friendly.goAttackNow = false;
            }
            index++;
        }
    }

    // Klasik sað týklama komutlarýný uygular.
    private void IssueSimpleCommand()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hitGround, Mathf.Infinity, enemyLayer))
        {
            foreach (var obj in objectSelected)
            {
                if (obj != null)
                {
                    FriendlyCharacterController friendly = obj.GetComponent<FriendlyCharacterController>();
                    friendly.enemyTransform = hitGround.collider.gameObject.transform;
                    friendly.goAttackNow = true;
                    selectedObj = hitGround.collider.gameObject;
                }
            }
            return;
        }
        if (Physics.Raycast(ray, out hitGround, Mathf.Infinity, ground))
        {
            groundMarker.transform.position = hitGround.point;
            groundMarker.SetActive(false);
            groundMarker.SetActive(true);
            foreach (var obj in objectSelected)
            {
                if (obj != null)
                {
                    FriendlyCharacterController friendly = obj.GetComponent<FriendlyCharacterController>();
                    friendly.targetPosition = hitGround.point;
                    friendly.goMoveNow = true;
                    friendly.justStop = false;
                    friendly.letGoAttackNow = false;
                    friendly.goAttackNow = false;
                }
            }
        }
    }

    // Formation iþlemi tamamlandýðýnda, marker prefablarý ve LineRenderer görsellerini temizler.
    private void ClearFormationVisuals()
    {
        foreach (GameObject marker in formationPointMarkers)
        {
            if (marker != null)
                Destroy(marker);
        }
        formationPointMarkers.Clear();

        if (formationRectangleLineRenderer != null)
            formationRectangleLineRenderer.positionCount = 0;
    }
}
