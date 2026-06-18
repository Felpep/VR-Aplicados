using UnityEngine;

public class TrashBallManager : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.gameObject.CompareTag("Ball"))
        {
            Destroy(collision.gameObject);

            
            Debug.Log("¡Bola destruida!");
        }
    }
}
