using System.Collections;
using UnityEngine;

/// <summary>
/// Controla una secuencia visual autónoma optimizada para VR:
/// Gira sobre su eje por N segundos, luego se achica suavemente y se desactiva.
/// Diseñado para ser gatillado mediante UnityEvents (Event-Driven).
/// </summary>
public class SequenceController : MonoBehaviour
{
    [Header("Configuración del Giro")]
    [SerializeField] private Vector3 _rotationAxis = Vector3.up;
    [SerializeField] private float _rotationSpeed = 360f;
    [SerializeField] private float _spinDuration = 2.0f;

    [Header("Configuración del Achique")]
    [SerializeField] private float _shrinkDuration = 0.4f;

    private Vector3 _originalScale;
    private bool _isSequenceRunning = false;

    private void Awake()
    {
        // Cacheamos la escala original del inspector para poder reusar el objeto 
        // si se vuelve a activar en el futuro (muy útil si usas object pooling)
        _originalScale = transform.localScale;
    }

    /// <summary>
    /// API PÚBLICA: Conéctalo al evento WhenSelect(), OnMissionCompleted, etc.
    /// Inicia la secuencia de giro y desaparición de forma segura.
    /// </summary>
    public void PlaySequence()
    {
        if (_isSequenceRunning) return; // Evita que se duplique la corrutina si lo gatillan dos veces

        StartCoroutine(SequenceRoutine());
    }

    private IEnumerator SequenceRoutine()
    {
        _isSequenceRunning = true;

        // Nos aseguramos de restaurar la escala original por si acaso
        transform.localScale = _originalScale;

        float elapsed = 0f;

        // FASE 1: Girar sobre su eje durante los segundos configurados
        while (elapsed < _spinDuration)
        {
            elapsed += Time.deltaTime;

            // Rotación suave e independiente de los FPS
            transform.Rotate(_rotationAxis * (_rotationSpeed * Time.deltaTime), Space.Self);

            yield return null;
        }

        // FASE 2: Achicarse hasta desaparecer por completo (Escala 0)
        elapsed = 0f;
        while (elapsed < _shrinkDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / _shrinkDuration;

            // Interpolamos la escala desde su valor original a cero absoluto
            transform.localScale = Vector3.Lerp(_originalScale, Vector3.zero, percent);

            // Seguimos girando un poquito en el achique para que quede más dinámico
            transform.Rotate(_rotationAxis * ((_rotationSpeed * 0.5f) * Time.deltaTime), Space.Self);

            yield return null;
        }

        // FASE 3: Asegurar el cero físico y apagar el GameObject
        transform.localScale = Vector3.zero;
        _isSequenceRunning = false;

        Debug.Log($"<color=cyan>[VFXSequence]</color> Secuencia finalizada. Desactivando {name}.");
        gameObject.SetActive(false);
    }
}