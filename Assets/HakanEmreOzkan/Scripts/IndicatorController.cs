using UnityEngine;

public class IndicatorController : MonoBehaviour
{
    [Tooltip("Zemini i�eren layer'� se�in.")]
    public LayerMask groundLayer;
    [Tooltip("Raycast'in ne kadar mesafeye kadar kontrol edece�ini belirler.")]
    public float raycastDistance = 10f;
    [Tooltip("Indicator'�n d�n�� h�z�n� ayarlar.")]
    public float rotationSpeed = 10f;
    [Tooltip("Indicator'�n raycast ba�lang�� noktas�n�n y�ksekli�i (nesnenin pozisyonuna eklenir).")]
    public float raycastHeightOffset = 1f;

    void Update()
    {
        // Indicator'�n hemen �st�nden a�a�� do�ru bir raycast at�yoruz.
        Vector3 rayOrigin = transform.position + Vector3.up * raycastHeightOffset;
        Ray ray = new Ray(rayOrigin, Vector3.down);
        RaycastHit hit;

        // E�er raycast, groundLayer katman�ndaki bir nesneyle temas ederse...
        if (Physics.Raycast(ray, out hit, raycastDistance, groundLayer))
        {
            // Zeminin normalini al�yoruz.
            Vector3 groundNormal = hit.normal;

            // �stedi�imiz rotasyonu olu�turmak i�in, indicator'�n up vekt�r�n� zeminin normaline e�itliyoruz.
            // Burada Quaternion.FromToRotation ile mevcut up vekt�r� ile hedef normal aras�ndaki rotasyonu hesapl�yoruz.
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, groundNormal) * transform.rotation;

            // Yumu�ak ge�i� ile rotasyonu uyguluyoruz.
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    // �ste�e ba�l�: Sahne �zerinde raycast �izgilerini g�stermek i�in.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 rayOrigin = transform.position + Vector3.up * raycastHeightOffset;
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * raycastDistance);
    }
}