using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class ExitButton : MonoBehaviour, IPointerClickHandler 
{
    [SerializeField] private GameObject Encylopedia;
    [SerializeField] private GameObject Notebook;
    // Start is called before the first frame update
    public void OnPointerClick(PointerEventData eventData){
        Encylopedia.SetActive(false);
        Notebook.SetActive(false);
    }
}
