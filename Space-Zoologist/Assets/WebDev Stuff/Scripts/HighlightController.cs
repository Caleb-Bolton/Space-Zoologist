using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightController : MonoBehaviour
{
    [SerializeField] GameObject NotesPrefab;
    [SerializeField] GameObject Scrollpanel;
    [SerializeField] SaveDataPerAnimal InitialAnimal;
    [SerializeField] SaveDataPerAnimal CurrentAnimal;
    // Start is called before the first frame update
    void Start()
    {
        LoadEAnimal(InitialAnimal);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void LoadEAnimal(SaveDataPerAnimal EAnimal)
    {
        this.CurrentAnimal = EAnimal;

        //delete previous sticky notes if any
        foreach (Transform child in Scrollpanel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        //load sticky notes

        for (int i = 0; i < EAnimal.StickyNotes.Count; i++)
        {
            var SN = EAnimal.StickyNotes[i];
            var NewNote = Instantiate(NotesPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            NewNote.transform.SetParent(Scrollpanel.transform);
            NewNote.GetComponent<ENoteController>().CreateNote(SN, i, this.gameObject);
        }
    }

    public void DeleteStickyNote(int index)
    {
        this.CurrentAnimal.StickyNotes.RemoveAt(index);
        foreach (Transform child in Scrollpanel.transform)
        {
            if (child.gameObject.GetComponent<ENoteController>().index > index)
            {
                child.gameObject.GetComponent<ENoteController>().index -= 1;
            }
        }
    }
}
