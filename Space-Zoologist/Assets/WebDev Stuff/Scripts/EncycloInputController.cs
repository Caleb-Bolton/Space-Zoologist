using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class EncycloInputController : MonoBehaviour
{
    [SerializeField] GameObject InputField;
    [SerializeField] SaveDataPerAnimal InitialAnimal;
    [SerializeField] SaveDataPerAnimal CurrentAnimal;
    [SerializeField] int selectPos;
    [SerializeField] int selectEndPos;
    [SerializeField] GameObject NotesPrefab;
    [SerializeField] GameObject Scrollpanel;
    // Start is called before the first frame update
    void Start()
    {
        LoadEncycloAnimal(InitialAnimal);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void LoadEncycloAnimal(SaveDataPerAnimal Animal)
    {
        this.CurrentAnimal = Animal;
       
    }

    public void RetrieveText()
    {

        var SelectedText = InputField.GetComponent<TMP_InputField>().text;
        this.CurrentAnimal.StickyNotes.Add(SelectedText);
        InputField.GetComponent<TMP_InputField>().text = "";
        InputField.GetComponent<TMP_InputField>().placeholder.GetComponent<TMP_Text>().text = "Saved";

    }
    
}
