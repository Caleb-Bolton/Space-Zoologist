using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SaveDataPerAnimal", menuName = "SaveDataPerAnimal")]
public class SaveDataPerAnimal : ScriptableObject
{
    public string AnimalName;
    public string NoteText;
    public List<string> StickyNotes;
}
