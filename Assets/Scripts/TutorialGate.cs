using UnityEngine;
using UnityEngine.Events;

public class TutorialGate : MonoBehaviour
{
    [Header("Blocking")]
    [SerializeField] private GameObject[] activeWhileLocked;
    [SerializeField] private GameObject[] activeWhileUnlocked;

    [Header("Settings")]
    [SerializeField] private bool lockedOnStart = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    [Header("Events")]
    public UnityEvent onLocked;
    public UnityEvent onUnlocked;
    public UnityEvent onBlockedAttempt;

    private bool isLocked;

    public bool IsLocked => isLocked;

    private void Awake()
    {
        isLocked = lockedOnStart;
        ApplyState();
    }

    public void Unlock()
    {
        if (!isLocked)
            return;

        isLocked = false;
        ApplyState();

        if (debugLogs)
            Debug.Log($"[TutorialGate] '{name}' DESBLOQUEADA", this);

        onUnlocked.Invoke();
    }

    public void Lock()
    {
        if (isLocked)
            return;

        isLocked = true;
        ApplyState();
        onLocked.Invoke();
    }

    public void NotifyBlockedAttempt()
    {
        if (!isLocked)
            return;

        if (debugLogs)
            Debug.Log($"[TutorialGate] '{name}': intento bloqueado", this);

        onBlockedAttempt.Invoke();
    }

    private void ApplyState()
    {
        foreach (GameObject go in activeWhileLocked)
        {
            if (go != null)
                go.SetActive(isLocked);
        }

        foreach (GameObject go in activeWhileUnlocked)
        {
            if (go != null)
                go.SetActive(!isLocked);
        }
    }
}