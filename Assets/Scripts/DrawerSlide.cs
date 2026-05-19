using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DrawerSlide : MonoBehaviour
{
    [Header("Slide Settings")]
    [Tooltip("Eje local sobre el que se desliza el cajón")]
    [SerializeField] private Vector3 slideAxis = Vector3.forward;

    [Tooltip("Distancia mínima (cerrado, normalmente 0)")]
    [SerializeField] private float minDistance = 0f;

    [Tooltip("Distancia máxima (cuánto se abre)")]
    [SerializeField] private float maxDistance = 0.3f;

    [Header("Snap Settings")]
    [Tooltip("Si está cerca de los extremos, hace snap")]
    [SerializeField] private bool snapToEnds = true;
    [SerializeField] private float snapThreshold = 0.02f;

    [Header("Debug")]
    [Tooltip("Imprime logs en consola para diagnosticar")]
    [SerializeField] private bool debugMode = true;
    [Tooltip("Cuánto debe moverse para considerar que está siendo agarrado")]
    [SerializeField] private float movementThreshold = 0.001f;

    // Referencia al Rigidbody
    private Rigidbody rb;

    // Posición/rotación inicial (cerrado)
    private Vector3 closedLocalPosition;
    private Quaternion closedLocalRotation;
    private Transform parentTransform;

    // Variables para tracking de movimiento (debug)
    private Vector3 lastLocalPosition;
    private bool wasMovingLastFrame = false;
    private float currentOpenAmount = 0f;
    private float lastLoggedOpenAmount = 0f;

    private void Awake()
    {
        // Obtener referencia al Rigidbody
        rb = GetComponent<Rigidbody>();

        // Configuración recomendada del Rigidbody para VR grab
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        // Guardar posición y rotación iniciales
        closedLocalPosition = transform.localPosition;
        closedLocalRotation = transform.localRotation;
        parentTransform = transform.parent;
        lastLocalPosition = transform.localPosition;

        if (debugMode)
        {
            Debug.Log($"[DrawerSlide] Inicializado en '{gameObject.name}'. " +
                      $"Rigidbody: {(rb != null ? "OK" : "NULL!")}. " +
                      $"Kinematic: {(rb != null ? rb.isKinematic.ToString() : "N/A")}. " +
                      $"Posición cerrada: {closedLocalPosition}. " +
                      $"Eje: {slideAxis.normalized}. " +
                      $"Rango: [{minDistance}, {maxDistance}]");
        }
    }

    private void LateUpdate()
    {
        // Posición actual del cajón en espacio local del padre
        Vector3 localPos = parentTransform != null
            ? parentTransform.InverseTransformPoint(transform.position)
            : transform.position;

        // Vector desde la posición cerrada hasta la actual
        Vector3 delta = localPos - closedLocalPosition;

        // Proyectamos sobre el eje permitido (sin clamp todavía, para debug)
        float rawProjected = Vector3.Dot(delta, slideAxis.normalized);
        Vector3 unwantedMovement = delta - (slideAxis.normalized * rawProjected);

        // Clampeamos al rango permitido
        float projected = Mathf.Clamp(rawProjected, minDistance, maxDistance);

        // Snap opcional a los extremos
        if (snapToEnds)
        {
            if (Mathf.Abs(projected - minDistance) < snapThreshold)
                projected = minDistance;
            else if (Mathf.Abs(projected - maxDistance) < snapThreshold)
                projected = maxDistance;
        }

        // Reconstruimos la posición final solo en el eje permitido (en espacio local)
        Vector3 newLocalPos = closedLocalPosition + slideAxis.normalized * projected;

        // Convertimos a posición y rotación de mundo
        Vector3 newWorldPos = parentTransform != null
            ? parentTransform.TransformPoint(newLocalPos)
            : newLocalPos;
        Quaternion newWorldRot = parentTransform != null
            ? parentTransform.rotation * closedLocalRotation
            : closedLocalRotation;

        // Aplicamos via Rigidbody si está disponible (mejor para física kinematic)
        if (rb != null && rb.isKinematic)
        {
            rb.MovePosition(newWorldPos);
            rb.MoveRotation(newWorldRot);
        }
        else
        {
            transform.localPosition = newLocalPos;
            transform.localRotation = closedLocalRotation;
        }

        currentOpenAmount = Mathf.InverseLerp(minDistance, maxDistance, projected);

        // ===== DEBUG =====
        if (debugMode)
        {
            float movementSinceLastFrame = Vector3.Distance(transform.localPosition, lastLocalPosition);
            bool isMovingNow = movementSinceLastFrame > movementThreshold;

            if (isMovingNow && !wasMovingLastFrame)
            {
                Debug.Log($"<color=lime>[DrawerSlide] ▶ MOVIMIENTO DETECTADO!</color> " +
                          $"Algo está agarrando/moviendo el cajón. " +
                          $"Delta total (sin clamp): {delta} | " +
                          $"Eje permitido: {slideAxis.normalized} | " +
                          $"Proyección sobre el eje: {rawProjected:F3}");
            }

            if (!isMovingNow && wasMovingLastFrame)
            {
                Debug.Log($"<color=orange>[DrawerSlide] ■ MOVIMIENTO DETENIDO.</color> " +
                          $"Apertura final: {currentOpenAmount * 100f:F1}%");
            }

            if (isMovingNow && Mathf.Abs(currentOpenAmount - lastLoggedOpenAmount) > 0.1f)
            {
                Debug.Log($"[DrawerSlide] Apertura: {currentOpenAmount * 100f:F0}% " +
                          $"(distancia: {projected:F3}m)");
                lastLoggedOpenAmount = currentOpenAmount;
            }

            if (isMovingNow && unwantedMovement.magnitude > 0.05f)
            {
                Debug.LogWarning($"[DrawerSlide] ⚠ El grab está tirando en una dirección " +
                                 $"que NO es el eje permitido. Movimiento descartado: {unwantedMovement}. " +
                                 $"Quizás el Slide Axis está mal configurado.");
            }

            wasMovingLastFrame = isMovingNow;
            lastLocalPosition = transform.localPosition;
        }
    }

    public float GetOpenAmount()
    {
        return currentOpenAmount;
    }

    // Visualización en Scene View (solo en el editor)
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = Application.isPlaying && parentTransform != null
            ? parentTransform.TransformPoint(closedLocalPosition)
            : transform.position;

        Vector3 worldAxis = parentTransform != null
            ? parentTransform.TransformDirection(slideAxis.normalized)
            : transform.TransformDirection(slideAxis.normalized);

        // Línea verde: rango de movimiento permitido
        Gizmos.color = Color.green;
        Vector3 minPoint = origin + worldAxis * minDistance;
        Vector3 maxPoint = origin + worldAxis * maxDistance;
        Gizmos.DrawLine(minPoint, maxPoint);
        Gizmos.DrawWireSphere(minPoint, 0.02f);
        Gizmos.DrawWireSphere(maxPoint, 0.03f);

        // Flecha amarilla apuntando en la dirección del slide
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(origin, worldAxis * 0.1f);
    }
}