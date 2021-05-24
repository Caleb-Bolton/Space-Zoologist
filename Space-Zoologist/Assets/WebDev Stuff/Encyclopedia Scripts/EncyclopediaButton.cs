using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class EncyclopediaButton : MonoBehaviour,  IPointerClickHandler 
{
    // Start is called before the first frame update
    [SerializeField] private GameObject Encylopedia;
    public void OnPointerClick(PointerEventData eventData){
        Encylopedia.SetActive(true);

    }
}

