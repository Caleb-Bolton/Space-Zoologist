using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class InputController : MonoBehaviour
{
    [SerializeField] string OwnerID;
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
        LoadAnimal(InitialAnimal);
    }

    // Update is called once per frame
    void Update()
    {
        selectPos = InputField.GetComponent<TMP_InputField>().selectionAnchorPosition;
        selectEndPos = InputField.GetComponent<TMP_InputField>().selectionFocusPosition;
    }
    public void SaveInputField(string s)
    {
        if(this.CurrentAnimal == null)
        {
            Debug.LogError("null CurrentAnimal");
        }
        else
        {
            CurrentAnimal.NoteText = InputField.GetComponent<TMP_InputField>().text;
        }
    }
    public void LoadAnimal(SaveDataPerAnimal Animal)
    {
        this.CurrentAnimal = Animal;
        //load text
        InputField.GetComponent<TMP_InputField>().text = CurrentAnimal.NoteText;
        //delete previous sticky notes if any
        foreach (Transform child in Scrollpanel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        //load sticky notes

        for (int i = 0; i <CurrentAnimal.StickyNotes.Count; i++)
        {
            var SN = CurrentAnimal.StickyNotes[i];
            var NewNote = Instantiate(NotesPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            NewNote.transform.SetParent(Scrollpanel.transform);
            NewNote.GetComponent<NoteController>().CreateNote(SN, i);
        }
    }

    public void DeleteStickyNote(int index)
    {
        this.CurrentAnimal.StickyNotes.RemoveAt(index);
    }
    public void RetrieveText()
    {
        var SP = selectPos;
        var SEP = selectEndPos;
        if (SEP < SP)
        {
            var temp = SP;
            SP = SEP;
            SEP = temp;
        }
        if(selectEndPos != selectPos)
        {
            var OriginalText = InputField.GetComponent<TMP_InputField>().text;
            var SelectedText = OriginalText.Substring(SP, SEP - SP);
            this.CurrentAnimal.StickyNotes.Add(SelectedText);
            var NewNote = Instantiate(NotesPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            NewNote.transform.SetParent(Scrollpanel.transform);
            NewNote.GetComponent<NoteController>().CreateNote(SelectedText, this.CurrentAnimal.StickyNotes.Count);
        }

    }
}
