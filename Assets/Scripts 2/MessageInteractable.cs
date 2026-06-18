using UnityEngine;

public class MessageInteractable : MonoBehaviour
{
    [Header("Datos del Mensaje")]
    [Tooltip("ID único para este mensaje (ej. 0, 1, 2).")]
    public int messageID;

    [Header("Visuales")]
    public Material normalMaterial;
    public Material markedMaterial; // Material color amarillo
    private MeshRenderer meshRenderer;

    private bool isMarked = false;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null && normalMaterial != null)
        {
            meshRenderer.material = normalMaterial;
        }
    }

    private void OnEnable()
    {
        ResetVisuals();
    }

    // Llama a este método desde el evento "On Select" de tu Meta Interactable (ej. RayInteractable)
    public void OnPointerClick()
    {
        // Cambiamos el estado local
        isMarked = !isMarked;

        // Actualizamos lo visual
        if (meshRenderer != null)
        {
            meshRenderer.material = isMarked ? markedMaterial : normalMaterial;
        }

        // Avisamos al GameManager
        CaseManager.Instance.ToggleMessageMark(messageID);
        //PrototypeGameManager.Instance.ToggleMessageMark(messageID, isMarked);
    }

    private void ResetVisuals()
    {
        isMarked = false;
        if (meshRenderer != null && normalMaterial != null)
        {
            meshRenderer.material = normalMaterial;
        }
    }

    //public void OnHover()
    //{
    //    Debug.Log("Mensaje Hover");
    //}
}
