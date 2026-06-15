using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class TrashDetector : MonoBehaviour
{
    [Tooltip("Tag que tienen que tener los objetos válidos (ej: la manzana).")]
    [SerializeField] private string validObjectTag = "Apple";

    [Tooltip("Si está activo, destruye el objeto cuando entra en el tacho.")]
    [SerializeField] private bool destroyOnEnter = false;

    [Tooltip("Se dispara cuando un objeto válido entra en el tacho. Conectá efectos, sonidos, etc.")]
    public UnityEvent onObjectScored;

    private int score = 0;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // --- LOG TEMPORAL DE DIAGNÓSTICO: sacalo cuando ya funcione ---
        Debug.Log($"[TrashDetector] Entró algo: {other.name} (tag: {other.tag})");
        // -------------------------------------------------------------

        if (!other.CompareTag(validObjectTag))
            return;

        score++;
        Debug.Log($"¡Embocaste! Puntaje: {score}");
        onObjectScored?.Invoke();

        if (destroyOnEnter)
        {
            GameObject target = other.attachedRigidbody != null
                ? other.attachedRigidbody.gameObject
                : other.gameObject;
            Destroy(target);
        }
    }
}