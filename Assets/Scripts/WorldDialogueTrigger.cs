using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class WorldDialogueTrigger : MonoBehaviour
{
    [Header("Referencias UI")]
    [Tooltip("El objeto Canvas que contiene el texto")]
    [SerializeField] private GameObject dialogueCanvas;
    [Tooltip("El componente de texto donde se escribirá el mensaje")]
    [SerializeField] private TMP_Text dialogueTextComponent;

    [Header("Configuración del Diálogo")]
    [TextArea(2, 4)]
    [SerializeField] private string dialogueMessage = "¡Hola! ¿Qué haces en mi escritorio?";
    [Tooltip("Tiempo en segundos que el texto estará visible")]
    [SerializeField] private float displayDuration = 7f;

    [Tooltip("Abre este menú desplegable y selecciona la capa (Layer) 'Player'")]
    [SerializeField] private LayerMask targetLayer;

    [Header("Comportamiento VR")]
    [Tooltip("Si está activo, el texto siempre rotará para mirar a tu cámara")]
    [SerializeField] private bool facePlayer = true;

    private Transform mainCamera;
    private Coroutine activeCoroutine;

    private void Awake()
    {
        // 1. CHEQUEOS DE SEGURIDAD (Referencias faltantes)
        if (dialogueCanvas == null) Debug.LogError($"[DIÁLOGO - {gameObject.name}]: ¡ERROR! Falta asignar el Canvas.");
        if (dialogueTextComponent == null) Debug.LogError($"[DIÁLOGO - {gameObject.name}]: ¡ERROR! Falta asignar el Texto.");

        GetComponent<Collider>().isTrigger = true;

        if (dialogueCanvas != null) dialogueCanvas.SetActive(false);

        if (facePlayer)
        {
            if (Camera.main != null)
                mainCamera = Camera.main.transform;
            else
                Debug.LogWarning($"[DIÁLOGO - {gameObject.name}]: No hay ninguna cámara con el Tag 'MainCamera'. El texto no rotará.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 2. DETECCIÓN CRUDA (¿Alguien tocó el gatillo?)
        Debug.Log($"[DIÁLOGO - {gameObject.name}]: ALGO TOCÓ EL TRIGGER -> Objeto: '{other.gameObject.name}' | Layer: {LayerMask.LayerToName(other.gameObject.layer)} ({other.gameObject.layer})");

        // Operación binaria para comparar la capa
        if ((targetLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            // 3. ÉXITO (La capa es correcta)
            Debug.Log($"<color=green>[DIÁLOGO - {gameObject.name}]: ¡MATCH DE LAYER CORRECTO! Iniciando diálogo.</color>");

            if (activeCoroutine != null) StopCoroutine(activeCoroutine);
            activeCoroutine = StartCoroutine(ShowDialogueRoutine());
        }
        else
        {
            // 4. FRACASO (Es el objeto equivocado)
            Debug.Log($"<color=orange>[DIÁLOGO - {gameObject.name}]: Se rechazó a '{other.gameObject.name}' porque su Layer no es el seleccionado en el Target Layer.</color>");
        }
    }

    private IEnumerator ShowDialogueRoutine()
    {
        if (dialogueTextComponent != null)
            dialogueTextComponent.text = dialogueMessage;

        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(true);

        Debug.Log($"[DIÁLOGO - {gameObject.name}]: Mostrando mensaje por {displayDuration} segundos.");

        yield return new WaitForSeconds(displayDuration);

        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);

        Debug.Log($"[DIÁLOGO - {gameObject.name}]: Diálogo apagado.");
        activeCoroutine = null;
    }

    private void Update()
    {
        if (facePlayer && dialogueCanvas != null && dialogueCanvas.activeSelf && mainCamera != null)
        {
            dialogueCanvas.transform.LookAt(dialogueCanvas.transform.position + mainCamera.forward);
        }
    }
}