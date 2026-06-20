using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))] // ¡Súper importante para que Unity detecte el choque!
public class HeadCollisionFade : MonoBehaviour
{
    [Tooltip("El Tag que deben tener tus paredes o escritorios")]
    public string obstacleTag = "Pared";

    private OVRScreenFade screenFade;

    private void Awake()
    {
        screenFade = GetComponent<OVRScreenFade>();

        // Autoconfiguración a prueba de fallos
        GetComponent<SphereCollider>().isTrigger = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // Para que tu cabeza no se caiga al piso
        rb.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(obstacleTag) && screenFade != null)
        {
            Debug.Log("[CABEZA]: Jugador metió la cabeza en la pared. Oscureciendo...");
            screenFade.FadeOut(); // Funde a negro
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(obstacleTag) && screenFade != null)
        {
            Debug.Log("[CABEZA]: Jugador sacó la cabeza. Restaurando visión...");
            screenFade.FadeIn(); // Vuelve la visión normal
        }
    }
}