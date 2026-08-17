using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class StringEvent : UnityEvent<string> { }

[System.Serializable]
public class TutorialRequirement
{
    public string id;
    [Min(1)] public int requiredCount = 1;
}

public class TutorialStep : MonoBehaviour
{
    [Header("Requirements")]
    [SerializeField] private TutorialRequirement[] requirements;
    [SerializeField] private bool activeOnStart = false;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    [Header("Events")]
    public UnityEvent onStepStarted;
    public StringEvent onRequirementCompleted;
    public StringEvent onRequirementUncompleted;
    public UnityEvent onProgressChanged;
    public UnityEvent onStepCompleted;

    private readonly Dictionary<string, int> progress = new Dictionary<string, int>();
    private bool isActive;
    private bool isCompleted;

    public bool IsActive => isActive;
    public bool IsCompleted => isCompleted;

    public int TotalCount
    {
        get
        {
            int total = 0;

            foreach (TutorialRequirement req in requirements)
                total += Mathf.Max(1, req.requiredCount);

            return total;
        }
    }

    public int CompletedCount
    {
        get
        {
            int done = 0;

            foreach (TutorialRequirement req in requirements)
                done += Mathf.Min(GetProgress(req.id), Mathf.Max(1, req.requiredCount));

            return done;
        }
    }

    private void Start()
    {
        if (activeOnStart)
            Activate();
    }

    public void Activate()
    {
        if (isActive || isCompleted)
            return;

        isActive = true;

        if (debugLogs)
            Debug.Log($"[TutorialStep] '{name}' activado ({TotalCount} requisitos)", this);

        onStepStarted.Invoke();
        onProgressChanged.Invoke();
    }

    public void Deactivate()
    {
        isActive = false;
    }

    public void ReportCompleted(string id)
    {
        if (string.IsNullOrEmpty(id))
            return;

        if (!isActive)
        {
            if (debugLogs)
                Debug.Log($"[TutorialStep] '{name}' recibió '{id}' pero no está activo, se ignora", this);

            return;
        }

        TutorialRequirement req = Find(id);

        if (req == null)
        {
            if (debugLogs)
                Debug.Log($"[TutorialStep] '{name}' no pide '{id}', se ignora", this);

            return;
        }

        int target = Mathf.Max(1, req.requiredCount);
        int current = GetProgress(id);

        if (current >= target)
            return;

        progress[id] = current + 1;

        if (debugLogs)
            Debug.Log($"[TutorialStep] '{name}': {id} {progress[id]}/{target} — total {CompletedCount}/{TotalCount}", this);

        onRequirementCompleted.Invoke(id);
        onProgressChanged.Invoke();

        if (CompletedCount >= TotalCount)
            Complete();
    }

    public void ReportUncompleted(string id)
    {
        if (string.IsNullOrEmpty(id) || !isActive)
            return;

        int current = GetProgress(id);

        if (current <= 0)
            return;

        progress[id] = current - 1;

        if (debugLogs)
            Debug.Log($"[TutorialStep] '{name}': -{id} — total {CompletedCount}/{TotalCount}", this);

        onRequirementUncompleted.Invoke(id);
        onProgressChanged.Invoke();
    }

    public void ResetStep()
    {
        progress.Clear();
        isCompleted = false;
        isActive = false;
        onProgressChanged.Invoke();
    }

    public int GetProgress(string id)
    {
        return progress.TryGetValue(id, out int value) ? value : 0;
    }

    private TutorialRequirement Find(string id)
    {
        foreach (TutorialRequirement req in requirements)
        {
            if (req.id == id)
                return req;
        }

        return null;
    }

    private void Complete()
    {
        isCompleted = true;
        isActive = false;

        if (debugLogs)
            Debug.Log($"[TutorialStep] '{name}' COMPLETADO", this);

        onStepCompleted.Invoke();
    }
}