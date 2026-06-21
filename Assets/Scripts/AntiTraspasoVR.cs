using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AntiTraspasoVR : MonoBehaviour
{
    [Header("Configuración de Físicas")]
    [Tooltip("Selecciona aquí la capa (Layer) de las paredes, suelo y escritorios.")]
    public LayerMask capasSolidas;

    private Collider miCollider;

    private void Start()
    {
        miCollider = GetComponent<Collider>();
    }

    private void LateUpdate()
    {
        // 1. Buscamos cualquier cosa sólida que esté tocando la "caja" imaginaria de nuestro objeto
        // Usamos Quaternion.identity porque bounds ya es una caja alineada al mundo
        Collider[] paredesTocadas = Physics.OverlapBox(
            miCollider.bounds.center,
            miCollider.bounds.extents,
            Quaternion.identity,
            capasSolidas,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider pared in paredesTocadas)
        {
            // Evitamos que el objeto se pelee consigo mismo
            if (pared == miCollider) continue;

            // 2. La función ComputePenetration de Unity es magia pura. 
            // Nos dice EXACTAMENTE qué tan profundo se metió la pelota en la pared y en qué dirección.
            if (Physics.ComputePenetration(
                miCollider, transform.position, transform.rotation,
                pared, pared.transform.position, pared.transform.rotation,
                out Vector3 direccionSalida, out float distanciaPenetracion))
            {
                // 3. Empujamos el objeto hacia afuera de la pared al instante (más 1 milímetro por seguridad)
                transform.position += direccionSalida * (distanciaPenetracion + 0.001f);
            }
        }
    }
}