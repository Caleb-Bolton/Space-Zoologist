using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using UnityEngine.UI;
public class EncyclopediaScript : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown Category;
    [SerializeField] private TMP_Dropdown Items;
    [SerializeField] private TMP_Text ItemLabel;
    [SerializeField] private TMP_Text TextContainer;

    [SerializeField] private ScrollRect scrollBar;
    List<string> animalList = new List<string> {"Goat", "Hedge", "Snake", "Strot", "Prey"};
    List<string> foodList = new List<string> {"Space Maple", "Apple", "Grass"};
    
    void Start(){
        scrollBar.verticalNormalizedPosition = 1.0f;
    }
    public void HandleCategoryInput(int val){
        Items.options.Clear();
        scrollBar.verticalNormalizedPosition = 1.0f;
        if(val == 0){
            ItemLabel.text = animalList[0];
            TextContainer.text = "Goat Text";
            foreach(string items in animalList){
                Items.options.Add(new TMP_Dropdown.OptionData(items));
            }
        }
        if(val == 1){
            ItemLabel.text = foodList[0];
            TextContainer.text = "Space Maple Text";
            foreach(string items in foodList){
                Items.options.Add(new TMP_Dropdown.OptionData(items));
            }
        }
    }


    public void HandleItemChanges(int val){
        string currentCategory = Category.options[Category.value].text;
        string currentItem = Items.options[Items.value].text;
        if (currentCategory == "Animals"){
            switch(currentItem){
                case "Goat":
                    TextContainer.text = "Goat Text";
                    break;
                case "Hedge":
                    TextContainer.text = "Hedge Text";
                    break;
                case "Snake":
                    TextContainer.text = "Snake Text";
                    break;
                case "Strot":
                    TextContainer.text = "Strot Text";
                    break;
                case "Prey":
                    TextContainer.text = "Prey Text";
                    break;
                default:
                    TextContainer.text = "Error";
                    break;
            }
        }
    }
}
