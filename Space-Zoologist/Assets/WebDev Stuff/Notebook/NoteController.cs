using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NoteController : MonoBehaviour
{
    [SerializeField] public string note;
    [SerializeField] public int index;
    [SerializeField] GameObject TextContainer;
    [SerializeField] GameObject InputController;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Close()
    {
        InputController.GetComponent<InputController>().DeleteStickyNote(index);
        Destroy(this.gameObject);
    }
    public void CreateNote(string s, int index, GameObject IC)
    {
        if(s != null)
        {
            this.InputController = IC;
            note = s;
            this.index = index;
            TextContainer.GetComponent<TMP_Text>().text = s;
            
        }

    }
}
