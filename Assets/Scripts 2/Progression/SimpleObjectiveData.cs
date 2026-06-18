using UnityEngine;

[CreateAssetMenu(fileName = "NewObjective", menuName = "Progression/Simple Objective")]
public class SimpleObjectiveData : BaseObjectiveData
{
    // Al heredar todo de la base, no necesita código extra para misiones simples de trigger.
    // Esto te permite extender lógicas complejas a futuro en otros scripts (ej: misiones con tiempo, contador de bajas, etc).
}