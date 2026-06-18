using UnityEngine;
using UnityEngine.Events;

public class ButtonVr : MonoBehaviour
{
    public GameObject button;
    public UnityEvent onPress;
    public UnityEvent onRelease;

    [Tooltip("Distancia que bajará el botón al presionarse")]
    public float pressDistance = 0.01f;

    private GameObject presser;
    private bool isPressed;
    private Vector3 startPos; // Variable para memorizar la posición original

    void Start()
    {
        isPressed = false;

        // Memorizamos la posición exacta del botón rojo al iniciar el juego
        if (button != null)
        {
            startPos = button.transform.localPosition;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isPressed)
        {
            // 1. Traducimos la posición global de tu mano a la perspectiva local del botón
            Vector3 localHit = transform.InverseTransformPoint(other.transform.position);

            // 2. Comprobamos si la mano viene desde la parte superior (Eje Y positivo)
            // Si el valor es mayor a 0, significa que está tocando la mitad de arriba.
            if (localHit.y < 0)
            {
                button.transform.localPosition = new Vector3(startPos.x, startPos.y - pressDistance, startPos.z);

                presser = other.gameObject;
                onPress.Invoke();
                isPressed = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // other.gameObject es más seguro que other solo
        if (isPressed && other.gameObject == presser)
        {
            // Devolvemos el botón a su posición original exacta
            button.transform.localPosition = startPos;

            onRelease.Invoke();
            isPressed = false;
        }
    }

    public void SpawnSphere()
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // Hacemos que aparezca a 1 metro de altura
        sphere.transform.position = new Vector3(0, 1f, 0);
        sphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f); // Esfera más pequeña
        sphere.AddComponent<Rigidbody>();
    }
}