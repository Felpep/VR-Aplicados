using UnityEngine;
using UnityEngine.Events;

public class PlayerZoneTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerHead;
    [SerializeField] private Collider zone;

    [Header("Settings")]
    [SerializeField] private float requiredTimeInside = 0.5f;
    [SerializeField] private bool triggerOnlyOnce = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    [Header("Events")]
    public UnityEvent onPlayerEnter;
    public UnityEvent onPlayerExit;

    private bool isInside;
    private bool hasTriggered;
    private float timer;

    private void Reset()
    {
        zone = GetComponent<Collider>();
    }

    private void Awake()
    {
        if (zone == null)
            zone = GetComponent<Collider>();

        if (playerHead == null && Camera.main != null)
            playerHead = Camera.main.transform;

        if (debugLogs)
        {
            Debug.Log($"[PlayerZoneTrigger] Head: {(playerHead == null ? "NULL" : playerHead.name)} | Zone: {(zone == null ? "NULL" : zone.GetType().Name)}", this);

            if (zone != null)
                Debug.Log($"[PlayerZoneTrigger] Zone bounds center {zone.bounds.center} size {zone.bounds.size} | lossyScale {zone.transform.lossyScale}", this);
        }
    }

    private void Update()
    {
        if (playerHead == null || zone == null)
            return;

        bool inside = IsInside(playerHead.position);

        if (inside && !isInside)
        {
            isInside = true;
            timer = 0f;

            if (debugLogs)
                Debug.Log("[PlayerZoneTrigger] Cabeza DENTRO de la zona", this);
        }
        else if (!inside && isInside)
        {
            isInside = false;
            timer = 0f;

            if (!triggerOnlyOnce)
                hasTriggered = false;

            if (debugLogs)
                Debug.Log("[PlayerZoneTrigger] Cabeza FUERA de la zona", this);

            onPlayerExit.Invoke();
        }

        if (isInside && !hasTriggered)
        {
            timer += Time.deltaTime;

            if (timer >= requiredTimeInside)
            {
                hasTriggered = true;

                if (debugLogs)
                    Debug.Log($"[PlayerZoneTrigger] Disparando onPlayerEnter ({onPlayerEnter.GetPersistentEventCount()} listeners)", this);

                onPlayerEnter.Invoke();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Collider target = zone != null ? zone : GetComponent<Collider>();

        if (target == null)
            return;

        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.DrawCube(target.bounds.center, target.bounds.size);
    }

    private bool IsInside(Vector3 point)
    {
        return (zone.ClosestPoint(point) - point).sqrMagnitude < 0.0001f;
    }
}