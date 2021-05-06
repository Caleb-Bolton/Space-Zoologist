using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class InputController : MonoBehaviour
{
    [SerializeField] string OwnerID;
    [SerializeField] GameObject InputField;
    [SerializeField] GameObject Initial;
    [SerializeField] int selectPos;
    [SerializeField] int selectEndPos;
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
           // Debug.Log("OwnerID: "+ OwnerID);
            //Debug.Log("Message: " + s);
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

    public void ChangeSelectionColor()
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
            Debug.Log(SelectedText);

        }

    }
}
