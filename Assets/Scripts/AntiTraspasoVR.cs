using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AntiTraspasoVR : MonoBehaviour
{
    [Header("Configuración de Físicas")]
    [Tooltip("Selecciona aquí la capa (Layer) de las paredes y escritorios.")]
    public LayerMask capasSolidas;

    [Header("Configuración de Agarre")]
    [Tooltip("Arrastra aquí el componente 'Grabbable' directamente desde el objeto padre.")]
    public Behaviour componenteGrabbable;

    private Collider miCollider;

    private void Start()
    {
        miCollider = GetComponent<Collider>();

        if (componenteGrabbable == null)
        {
            Debug.LogError($"[Anti-Traspaso] Ojo: No asignaste el componente Grabbable en el inspector de {gameObject.name}");
        }
    }

    private void LateUpdate()
    {
        // Si no hay componente asignado o el objeto ya está cayendo (apagado), no hacemos nada
        if (componenteGrabbable == null || !componenteGrabbable.enabled) return;

        // 1. Buscamos si el collider de este objeto se metió en la capa "EntornoSólido"
        Collider[] paredesTocadas = Physics.OverlapBox(
            miCollider.bounds.center,
            miCollider.bounds.extents,
            Quaternion.identity,
            capasSolidas,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider pared in paredesTocadas)
        {
            // Evitamos pelearnos con nosotros mismos
            if (pared != miCollider)
            {
                // 2. ¡Atravesó la pared! Forzamos la caída
                ForzarSoltarObjeto();
                break;
            }
        }
    }

    private void ForzarSoltarObjeto()
    {
        Debug.Log("<color=red>[Físicas VR]</color> El objeto chocó contra la pared. ¡Soltando!");

        // Apagamos el script que pasaste por Inspector, obligando a la mano a soltarlo
        componenteGrabbable.enabled = false;

        // Lo volvemos a encender medio segundo después para poder volver a agarrarlo
        Invoke(nameof(ReactivarAgarre), 0.5f);
    }

    private void ReactivarAgarre()
    {
        if (componenteGrabbable != null)
        {
            componenteGrabbable.enabled = true;
        }
    }
}