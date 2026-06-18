using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Case", menuName = "Serious Game/Case Data")]
public class CaseData : ScriptableObject
{
    [Header("Identificación")]
    [Tooltip("El nombre interno del caso para debugear.")]
    public string caseName;

    [Header("Solución Esperada")]
    [Tooltip("La lista de IDs de mensajes que son RED FLAGS y deben ser marcados.")]
    public List<int> correctMessageIDs = new List<int>();

    [Tooltip("La acción final correcta. TRUE = Bloquear, FALSE = Archivar/Permitir.")]
    public bool isActionBlock;

    // Podrías añadir más cosas aquí, como el Modus Operandi esperado o texto de feedback.
}
