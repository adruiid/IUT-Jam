using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;


public class MagnifyButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private PlayerInteractor interactor;

    public void OnPointerDown(PointerEventData eventData)
    {
        interactor.SearchHeld = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        interactor.SearchHeld = false;
    }
}
