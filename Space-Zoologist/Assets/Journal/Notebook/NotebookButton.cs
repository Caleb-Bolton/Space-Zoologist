using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class NotebookButton : MonoBehaviour, IPointerClickHandler 
{
    [SerializeField] private GameObject Notebook;
    public void OnPointerClick(PointerEventData eventData){
        Notebook.SetActive(true);

    }
}
