using UnityEngine;

/// <summary>
/// Controla el feedback visual inmediato de una computadora individual.
/// Cambia el material de su pantalla al ser desconectada.
/// </summary>
public class PCInstanceController : MonoBehaviour
{
    [Header("Pantalla")]
    [SerializeField] private MeshRenderer _screenRenderer;
    [SerializeField] private Material _screenOnMaterial;
    [SerializeField] private Material _screenOffMaterial;

    private void Awake()
    {
        if (_screenRenderer == null)
        {
            Debug.LogError($"<color=red>[PCInstance]</color> Falta asignar '_screenRenderer' en {name}.", this);
            enabled = false;
        }
    }

    public void TurnOff()
    {
        if (_screenOffMaterial == null) return;
        _screenRenderer.sharedMaterial = _screenOffMaterial;
        Debug.Log($"<color=orange>[PCInstance]</color> Computadora APAGADA: {name}.");
    }

    public void TurnOn()
    {
        if (_screenOnMaterial == null) return;
        _screenRenderer.sharedMaterial = _screenOnMaterial;
        Debug.Log($"<color=orange>[PCInstance]</color> Computadora ENCENDIDA: {name}.");
    }
}