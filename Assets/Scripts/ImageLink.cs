using UnityEngine;
using UnityEngine.EventSystems;

public class ImageLink : MonoBehaviour, IPointerClickHandler
{
    public string link;

    public void OnPointerClick(PointerEventData eventData)
    {
        Application.OpenURL(link);
    }
}
