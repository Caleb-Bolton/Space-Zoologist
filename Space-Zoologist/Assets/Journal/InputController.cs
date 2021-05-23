using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class InputController : MonoBehaviour
{
    [SerializeField] string OwnerID;
    [SerializeField] GameObject InputField;
    [SerializeField] GameObject Initial;
    [SerializeField] int selectPos;
    [SerializeField] int selectEndPos;
    [SerializeField] GameObject NotesPrefab;
    [SerializeField] GameObject Scrollpanel;
    // Start is called before the first frame update
    void Start()
    {
        LoadText(Initial);
    }

    // Update is called once per frame
    void Update()
    {
        selectPos = InputField.GetComponent<TMP_InputField>().selectionAnchorPosition;
        selectEndPos = InputField.GetComponent<TMP_InputField>().selectionFocusPosition;
    }
    public void SaveInput(string s)
    {
        if(OwnerID == null)
        {
            Debug.LogError("null OwnerID");
        }
        else
        {
            PlayerPrefs.SetString(OwnerID, s);
        }
    }
    public void LoadText(GameObject Button)
    {
        this.OwnerID = Button.name;
       // Debug.Log("load OwnerID: " + OwnerID);
        if(InputField != null)
        {
            InputField.GetComponent<TMP_InputField>().text = PlayerPrefs.GetString(OwnerID);
        }
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
            var NewNote = Instantiate(NotesPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            NewNote.transform.SetParent(Scrollpanel.transform);
            NewNote.GetComponent<NoteController>().CreateNote(SelectedText, OwnerID);


        }

    }
}
