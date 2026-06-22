using UnityEngine;
using Oculus.Interaction;

public class DoorSnapController : MonoBehaviour
{
    [Header("Lector de Tarjeta")]
    public SnapInteractable cardReader;    

    [Header("Puerta")]
    public Transform doorPivot;           

    [Header("Rotaciones")]
    public Vector3 closedRotation = new Vector3(0, 0, 0);   
    public Vector3 openRotation = new Vector3(0, 90, 0);     

    [Header("Velocidad")]
    public float openSpeed = 4f;

    private Quaternion targetRotation;
    private bool isOpen = false;

    private void Start()
    {
        if (doorPivot != null)
        {
            targetRotation = doorPivot.rotation; 
        }
    }

    private void Update()
    {
        if (cardReader == null || doorPivot == null) return;

        bool cardInserted = cardReader.State == InteractableState.Select;

        // Detectar cambio
        if (cardInserted && !isOpen)
        {
            isOpen = true;
            targetRotation = Quaternion.Euler(openRotation);
        }
        else if (!cardInserted && isOpen)
        {
            isOpen = false;
            targetRotation = Quaternion.Euler(closedRotation);
        }

        // Suavizar el movimiento
        doorPivot.rotation = Quaternion.Slerp(doorPivot.rotation, targetRotation, Time.deltaTime * openSpeed);
    }
}