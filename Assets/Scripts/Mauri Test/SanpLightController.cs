using UnityEngine;
using Oculus.Interaction;

public class SnapLightController : MonoBehaviour
{
    [Header("Referencias")]
    public Light targetLight;                    
    public SnapInteractable snapInteractable;   

    private void Update()
    {
        if (targetLight != null && snapInteractable != null)
        {
            
            targetLight.enabled = snapInteractable.State == InteractableState.Select;
        }
    }
}