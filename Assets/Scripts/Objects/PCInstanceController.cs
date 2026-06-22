using UnityEngine;

/// <summary>
/// Controla el feedback visual inmediato de una computadora individual.
/// Cambia quirúrgicamente el segundo material (Índice 1) del monitor.
/// </summary>
public class PCInstanceController : MonoBehaviour
{
    [Header("Pantalla (Multi-Material Support)")]
    [SerializeField] private MeshRenderer _screenRenderer;
    [SerializeField] private Material _screenOnMaterial;
    [SerializeField] private Material _screenOffMaterial;

    // Buffer en memoria para evitar instanciar arrays dinámicos por frame (GC Clean)
    private Material[] _materialsBuffer;

    private void Awake()
    {
        if (_screenRenderer == null)
        {
            Debug.LogError($"<color=red>[PCInstance]</color> Falta asignar '_screenRenderer' en {name}.", this);
            enabled = false;
            return;
        }

        // Leemos el array original compartido. 
        // Index 0 = Carcasa de plástico, Index 1 = Vidrio de la pantalla.
        _materialsBuffer = _screenRenderer.sharedMaterials;

        if (_materialsBuffer.Length < 2)
        {
            Debug.LogError($"<color=red>[PCInstance]</color> {name} tiene menos de 2 materiales en su MeshRenderer. Esta lógica requiere que la pantalla sea el segundo slot (Índice 1).", this);
            enabled = false;
        }
    }

    /// <summary>
    /// Cambia el segundo slot de material a negro. Conectar al WhenUnselect del cable.
    /// </summary>
    public void TurnOff()
    {
        if (_screenOffMaterial == null || _materialsBuffer == null) return;

        // Modificamos estrictamente el Índice 1 (segundo material)
        _materialsBuffer[1] = _screenOffMaterial;

        // Reinyectamos el array modificado usando sharedMaterials (Plural) para optimizar memoria en Android
        _screenRenderer.sharedMaterials = _materialsBuffer;

        Debug.Log($"<color=orange>[PCInstance]</color> Pantalla APAGADA (Índice 1 cambiado) en: {name}.");
    }

    /// <summary>
    /// Cambia el segundo slot de material al emisor encendido. Conectar al WhenSelect si es reversible.
    /// </summary>
    public void TurnOn()
    {
        if (_screenOnMaterial == null || _materialsBuffer == null) return;

        _materialsBuffer[1] = _screenOnMaterial;
        _screenRenderer.sharedMaterials = _materialsBuffer;

        Debug.Log($"<color=orange>[PCInstance]</color> Pantalla ENCENDIDA (Índice 1 cambiado) en: {name}.");
    }
}