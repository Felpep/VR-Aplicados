using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AntiTraspasoVR : MonoBehaviour
{
    [Header("Configuración de Físicas")]
    public LayerMask capasSolidas;
    public Behaviour componenteGrabbable;

    [Header("Optimización de Ticks")]
    [Tooltip("Cada cuántos segundos se comprueba la física (Ej: 0.1s = 10 veces por segundo).")]
    [SerializeField] private float _checkInterval = 0.1f;

    private Collider miCollider;
    private float _nextCheckTime;

    // Buffer estático en RAM para evitar que Overlap cree basura (Zero Alloc)
    private readonly Collider[] _resultsBuffer = new Collider[4];

    private void Start()
    {
        miCollider = GetComponent<Collider>();
        if (componenteGrabbable == null)
        {
           // Debug.LogError($"[Anti-Traspaso] No asignaste el componente Grabbable en {name}");
        }
    }

    private void LateUpdate()
    {
        if (componenteGrabbable == null || !componenteGrabbable.enabled) return;

        // SISTEMA DE TICKS: Filtra la ejecución inter-frame redundante
        if (Time.time < _nextCheckTime) return;
        _nextCheckTime = Time.time + _checkInterval;

        // Usamos NonAlloc pasándole nuestro búfer estático
        int count = Physics.OverlapBoxNonAlloc(
            miCollider.bounds.center,
            miCollider.bounds.extents,
            _resultsBuffer,
            Quaternion.identity,
            capasSolidas,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < count; i++)
        {
            Collider pared = _resultsBuffer[i];
            if (pared != null && pared != miCollider)
            {
                ForzarSoltarObjeto();
                break;
            }
        }
    }

    private void ForzarSoltarObjeto()
    {
        //Debug.Log("<color=red>[Físicas VR]</color> Objeto atravesó el entorno. Soltando.");
        componenteGrabbable.enabled = false;
        Invoke(nameof(ReactivarAgarre), 0.5f);
    }

    private void ReactivarAgarre()
    {
        if (componenteGrabbable != null) componenteGrabbable.enabled = true;
    }
}