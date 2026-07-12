using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WorldDialogueRequester : MonoBehaviour
{
    [Header("Configuración del Mensaje")]
    [TextArea(2, 4)]
    [SerializeField] private string dialogueMessage = "¡Hola! Soy un NPC.";
    [SerializeField] private float displayDuration = 7f;
    [SerializeField] private LayerMask targetLayer;

    [Header("Anclaje (Opcional)")]
    [Tooltip("El objeto vacío donde aparecerá el globo de texto. Si está vacío, usará el centro de este objeto.")]
    [SerializeField] private Transform textAnchor;

    private Coroutine activeCoroutine;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        // Si olvidaste ponerle un ancla, usamos al mismo NPC como ancla
        if (textAnchor == null) textAnchor = this.transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Revisamos si es el jugador (usando tu LayerMask)
        if ((targetLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            if (activeCoroutine != null) StopCoroutine(activeCoroutine);
            activeCoroutine = StartCoroutine(DialogueSequence());
        }
    }

    private IEnumerator DialogueSequence()
    {
        // 1. LLAMAMOS AL CANVAS VIAJERO
        // Le pasamos 'this' para reclamar la propiedad, el ancla para que sepa dónde ir, y el texto.
        InteractionPromptController.Instance.RequestShow(this, textAnchor, dialogueMessage);

        // 2. Esperamos los 7 segundos
        yield return new WaitForSeconds(displayDuration);

        // 3. DESPEDIMOS AL CANVAS
        // Solo este NPC (this) puede apagarlo. Si otro NPC se activó en el medio, esta orden se ignorará automáticamente.
        InteractionPromptController.Instance.RequestHide(this);

        activeCoroutine = null;
    }
}