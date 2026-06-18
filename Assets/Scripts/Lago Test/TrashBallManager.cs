using UnityEngine;

public class TrashBallManager : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("Ball"))
        {         
            Destroy(other.gameObject);
           
            Debug.Log("¡Bola 3D destruida en la oficina!");
        }
    }
}
