using UnityEngine;

public class BossCatchPlayer : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("TOCOOOOOO");
            PlayerResetManager.Instance.ResetPlayer();
        }
    }
}
