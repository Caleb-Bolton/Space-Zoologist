using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NoteController : MonoBehaviour
{
    [SerializeField] public string note;
    [SerializeField] public string ID;
    [SerializeField] GameObject TextContainer;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Close()
    {
        Destroy(this.gameObject);
    }
    public void CreateNote(string s, string id)
    {
        if(s != null)
        {
            ID = id;
            note = s;
            TextContainer.GetComponent<TMP_Text>().text = s;
            //TODO:: Save Notes
        }

    }
}
