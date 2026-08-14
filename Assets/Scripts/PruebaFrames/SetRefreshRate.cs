using UnityEngine;

public class SetRefreshRate : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OVRPlugin.systemDisplayFrequency = 90.0f;
        Debug.Log("Frecuencia actual de la pantalla: " + OVRPlugin.systemDisplayFrequency + " Hz");
    }

    
}
