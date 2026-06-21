using UnityEngine;
using Oculus.Interaction;

public class AutoSnapStarter : MonoBehaviour
{
    [SerializeField] private SnapInteractable _targetSnapZone;
    [SerializeField] private SnapInteractor _myInteractor;

    private void Start()
    {
        // Forzamos al sistema a registrar el interactor en la zona 
        // apenas arranca la escena.
        //_targetSnapZone.sele(_myInteractor);
    }
}