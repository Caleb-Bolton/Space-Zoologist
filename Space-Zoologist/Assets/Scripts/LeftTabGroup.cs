using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LeftTabGroup : MonoBehaviour
{


    public List<LeftTabButton> tabButtons;
    public Sprite tabIdle;
    public Sprite tabHover;
    public Sprite tabActive;

    public LeftTabButton selectedTab;
    public List<GameObject> objectsToSwap;
    public void Subscribe(LeftTabButton button){
        if(tabButtons == null)
        {
            tabButtons = new List<LeftTabButton>();
        }
        tabButtons.Add(button);
    }

    public void OnTabEnter(LeftTabButton button)
    {
        ResetTabs();
        if(selectedTab == null || button != selectedTab)
        {
            button.background.sprite = tabHover;
        }
        
    }
    public void OnTabExit(LeftTabButton button)
    {
        ResetTabs();
    }
    public void OnTabSelected(LeftTabButton button)
    {
        selectedTab = button;
        ResetTabs();
        button.background.sprite = tabActive;
        int index = button.transform.GetSiblingIndex();
        for(int i =0; i < objectsToSwap.Count; i++){
            if (i == index){
                objectsToSwap[i].SetActive(true);
            }
            else{
                objectsToSwap[i].SetActive(false);
            }
        }
    }

    public void ResetTabs()
    {
        foreach(LeftTabButton button in tabButtons)
        {
            if(selectedTab!=null && button == selectedTab)
            {
                continue;
            }
            button.background.sprite = tabIdle;
        }
    }

}
