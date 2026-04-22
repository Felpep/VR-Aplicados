using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CaseToEvaluate : MonoBehaviour
{
    [Header("Configuración del Caso")]
    [Tooltip("¿La respuesta correcta para este caso es bloquear?")]
    public bool shouldBlock = true;

    // Se puebla sola con los hijos, sin configuración manual
    private List<MessageInteractable> messages = new List<MessageInteractable>();

    private void Start()
    {
        messages.AddRange(GetComponentsInChildren<MessageInteractable>());
        PrototypeGameManager.Instance.RegisterCase(this);
        gameObject.SetActive(false);
    }

    public bool EvaluateVerdict(bool playerChoseToBlock)
    {
        return playerChoseToBlock == shouldBlock;
    }

    public List<MessageInteractable> GetMissedSuspicious()
    {
        return messages.Where(m => m.isSuspicious && !m.IsMarked).ToList();
    }
    public List<MessageInteractable> GetFalsePositives()
    {
        return messages.Where(m => !m.isSuspicious && m.IsMarked).ToList();
    }

    public bool HasAnyMarked() => messages.Any(m => m.IsMarked);
}