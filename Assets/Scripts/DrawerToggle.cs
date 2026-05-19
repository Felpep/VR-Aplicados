using UnityEngine;

public class DrawerToggle : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private Vector3 openOffset = new Vector3(0, 0, 0.3f); // en local space
    [SerializeField] private float duration = 0.5f;

    private Vector3 closedPos;
    private Vector3 openPos;
    private bool isOpen = false;
    private bool isAnimating = false;
    private float animTimer = 0f;
    private Vector3 animFrom, animTo;

    private void Awake()
    {
        closedPos = transform.localPosition;
        openPos = closedPos + openOffset;
    }

    private void Update()
    {
        if (!isAnimating) return;

        animTimer += Time.deltaTime;
        float t = Mathf.Clamp01(animTimer / duration);
        // Easing suave (smoothstep)
        float smoothT = t * t * (3f - 2f * t);
        transform.localPosition = Vector3.Lerp(animFrom, animTo, smoothT);

        if (t >= 1f)
        {
            isAnimating = false;
        }
    }

    // Llamá a este método desde el evento del RayInteractable
    public void ToggleDrawer()
    {
        isOpen = !isOpen;
        animFrom = transform.localPosition;
        animTo = isOpen ? openPos : closedPos;
        animTimer = 0f;
        isAnimating = true;

        Debug.Log($"[DrawerToggle] Cajón {(isOpen ? "ABRIENDO" : "CERRANDO")}");
    }
}