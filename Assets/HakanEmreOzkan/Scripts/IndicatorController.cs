using UnityEngine;

public class IndicatorController : MonoBehaviour
{
    [Tooltip("Zemini içeren layer'ý seçin.")]
    public LayerMask groundLayer;
    [Tooltip("Raycast'in ne kadar mesafeye kadar kontrol edeceðini belirler.")]
    public float raycastDistance = 10f;
    [Tooltip("Indicator'ýn dönüþ hýzýný ayarlar.")]
    public float rotationSpeed = 10f;
    [Tooltip("Indicator'ýn raycast baþlangýç noktasýnýn yüksekliði (nesnenin pozisyonuna eklenir).")]
    public float raycastHeightOffset = 1f;

    void Update()
    {
        // Indicator'ýn hemen üstünden aþaðý doðru bir raycast atýyoruz.
        Vector3 rayOrigin = transform.position + Vector3.up * raycastHeightOffset;
        Ray ray = new Ray(rayOrigin, Vector3.down);
        RaycastHit hit;

        // Eðer raycast, groundLayer katmanýndaki bir nesneyle temas ederse...
        if (Physics.Raycast(ray, out hit, raycastDistance, groundLayer))
        {
            // Zeminin normalini alýyoruz.
            Vector3 groundNormal = hit.normal;

            // Ýstediðimiz rotasyonu oluþturmak için, indicator'ýn up vektörünü zeminin normaline eþitliyoruz.
            // Burada Quaternion.FromToRotation ile mevcut up vektörü ile hedef normal arasýndaki rotasyonu hesaplýyoruz.
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, groundNormal) * transform.rotation;

            // Yumuþak geçiþ ile rotasyonu uyguluyoruz.
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    // Ýsteðe baðlý: Sahne üzerinde raycast çizgilerini göstermek için.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 rayOrigin = transform.position + Vector3.up * raycastHeightOffset;
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * raycastDistance);
    }
}