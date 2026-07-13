using UnityEngine;
using Oculus.Interaction; // Necesario para verificar si el objeto está seleccionado

[RequireComponent(typeof(Collider))]
public class AntiTraspasoVR : MonoBehaviour
{
    [Header("Configuración de Físicas")]
    public LayerMask capasSolidas;

    [Tooltip("Arrastra aquí el componente GrabInteractable de Meta que le pertenece a este objeto.")]
    [SerializeField] private GrabInteractable _grabInteractable;

    [Header("Optimización de Ticks")]
    [Tooltip("Cada cuántos segundos se comprueba la física (Ej: 0.05s = 20 veces por segundo).")]
    [SerializeField] private float _checkInterval = 0.05f;

    private Collider miCollider;
    private float _nextCheckTime;

    // Buffer estático en RAM para evitar Garbage Collection
    private readonly Collider[] _resultsBuffer = new Collider[4];

    private void Start()
    {
        miCollider = GetComponent<Collider>();

        // Auto-recuperación si te olvidaste de arrastrarlo al Inspector
        if (_grabInteractable == null)
        {
            _grabInteractable = GetComponentInChildren<GrabInteractable>();
        }

        if (_grabInteractable == null)
        {
            Debug.LogError($"<color=red>[Anti-Traspaso]</color> FALTAL: No se encontró un GrabInteractable en {name}. El script no funcionará.", this);
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (_grabInteractable == null) return;

        // CORTOCIRCUITO VR: Si el jugador NO tiene el objeto en la mano, 
        // cancelamos el cálculo físico. Ahorra CPU y evita que se safe solo en el suelo.
        if (_grabInteractable.State != InteractableState.Select) return;

        // SISTEMA DE TICKS: Controla la tasa de refresco inter-frame
        if (Time.time < _nextCheckTime) return;
        _nextCheckTime = Time.time + _checkInterval;

        // OPTIMIZACIÓN CORE: Pasamos la rotación REAL del objeto (transform.rotation) en lugar de identity
        int count = Physics.OverlapBoxNonAlloc(
            miCollider.bounds.center,
            miCollider.bounds.extents,
            _resultsBuffer,
            transform.rotation,
            capasSolidas,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < count; i++)
        {
            Collider pared = _resultsBuffer[i];

            // Verificación limpia e inmune a datos residuales fantasmas
            if (pared != null && pared != miCollider)
            {
                ForzarSoltarObjeto();
                break;
            }
        }

        // LIMPIEZA DE BUFFER MANDATORIA: Borra los datos del frame para que el próximo Overlap no arrastre basura
        System.Array.Clear(_resultsBuffer, 0, _resultsBuffer.Length);
    }

    private void ForzarSoltarObjeto()
    {
        Debug.Log("<color=red>[Físicas VR]</color> Objeto atravesó la oficina mientras se sostenía. Forzando Drop.");

        // La manera oficial y limpia de forzar un drop en Meta Interaction SDK 
        // es deshabilitar momentáneamente el interactuable que la mano está sujetando
        _grabInteractable.Disable();

        // Lo reactivamos medio segundo después para que el jugador pueda volver a agarrarlo limpiamente
        Invoke(nameof(ReactivarAgarre), 0.5f);
    }

    private void ReactivarAgarre()
    {
        if (_grabInteractable != null)
        {
            _grabInteractable.Enable();
        }
    }
}