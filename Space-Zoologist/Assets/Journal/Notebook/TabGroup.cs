using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TabGroup : MonoBehaviour
{


    public List<TabButton> tabButtons;
    public Sprite tabIdle;
    public Sprite tabHover;
    public Sprite tabActive;

    public TabButton selectedTab;
    public List<GameObject> objectsToSwap;

    public int leftTabIndex = 0;
    public void Subscribe(TabButton button){
        if(tabButtons == null)
        {
            tabButtons = new List<TabButton>();
        }
        tabButtons.Add(button);
    }

    public void OnTabEnter(TabButton button)
    {
        ResetTabs();
        if(selectedTab == null || button != selectedTab)
        {
            button.background.sprite = tabHover;
        }
        
    }
    public void OnTabExit(TabButton button)
    {
        ResetTabs();
    }
    public void OnTabSelected(TabButton button)
    {
        selectedTab = button;
        ResetTabs();
        button.background.sprite = tabActive;
        int index = button.transform.GetSiblingIndex();
        for (int i =0; i < objectsToSwap.Count; i++){
            objectsToSwap[i].SetActive(false);
        }
        for(int i =0; i < objectsToSwap.Count; i++){
            if (i == index){
                if (i < 4){
                    leftTabIndex = i;
                    objectsToSwap[i].SetActive(true);
                    objectsToSwap[4].SetActive(true);
                }
                else if (i == 4){
                    objectsToSwap[i].SetActive(true);
                    objectsToSwap[leftTabIndex].SetActive(true);
                }
                else {
                    objectsToSwap[i].SetActive(true);
                }
                break;
            }
        }
    }

    public void ResetTabs()
    {
        foreach(TabButton button in tabButtons)
        {
            if(selectedTab!=null && button == selectedTab)
            {
                continue;
            }
            button.background.sprite = tabIdle;
        }
    }

}
