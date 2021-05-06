using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RightTabGroup : MonoBehaviour
{


    public List<RightTabButton> tabButtons;
    public Sprite tabIdle;
    public Sprite tabHover;
    public Sprite tabActive;

    public RightTabButton selectedTab;
    public List<GameObject> objectsToSwap;
    public void Subscribe(RightTabButton button){
        if(tabButtons == null)
        {
            tabButtons = new List<RightTabButton>();
        }
        tabButtons.Add(button);
    }

    public void OnTabEnter(RightTabButton button)
    {
        ResetTabs();
        if(selectedTab == null || button != selectedTab)
        {
            button.background.sprite = tabHover;
        }
        
    }
    public void OnTabExit(RightTabButton button)
    {
        ResetTabs();
    }
    public void OnTabSelected(RightTabButton button)
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
        foreach(RightTabButton button in tabButtons)
        {
            if(selectedTab!=null && button == selectedTab)
            {
                continue;
            }
            button.background.sprite = tabIdle;
        }
    }

}
