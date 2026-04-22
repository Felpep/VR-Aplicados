using UnityEngine;
using UnityEngine.Events;

public class VerdictButton : MonoBehaviour
{
    [Header("Configuración del Botón")]
    [Tooltip("Si es TRUE, este botón representa BLOQUEAR. Si es FALSE, representa ARCHIVAR.")]
    public bool isBlockButton;

    // Para evitar que el jugador presione el botón 20 veces por segundo (Bounce)
    private bool hasBeenPressed = false;

    // Llama a este método desde el evento "On Select" o "On Poke" de tu Meta Interactable
    public void OnButtonPressed()
    {
        if (hasBeenPressed) return;

        if (!PrototypeGameManager.Instance.ActiveCase.HasAnyMarked())
        {
            Debug.Log("Marcá al menos un mensaje antes de enviar.");
            return;
        }

        hasBeenPressed = true;
        PrototypeGameManager.Instance.SubmitVerdict(isBlockButton);
    }

    public void ResetButton() => hasBeenPressed = false;
    public void OnHover()
    {
        Debug.Log("Mensaje Hover");
    }

}
