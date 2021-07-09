using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum MenuType { Food, Tiles, Animals };
public class MenuManager : MonoBehaviour
{
    readonly string[] menuNames = { "Food", "Tiles", "Animals" };
    GameObject currentMenu = null;
    [SerializeField] GameObject PlayerBalanceHUD = default;
    [SerializeField] List<StoreSection> StoreMenus = default;
    [SerializeField] ResourceManager ResourceManager = default;
    [SerializeField] PauseManager PauseManager = default;
    [Header("Shared menu dependencies")]
    [SerializeField] PlayerBalance PlayerBalance = default;
    [SerializeField] CanvasObjectStrobe PlayerBalanceDisplay = default;
    [SerializeField] LevelDataReference LevelDataReference = default;
    [SerializeField] CursorItem CursorItem = default;
    [SerializeField] GridSystem GridSystem = default;
    [SerializeField] List<RectTransform> UIElements = default;
    [SerializeField] RectTransform StoreCanvas = default;
    [SerializeField] RectTransform MenuSelectPanel = default;
    [SerializeField] Text CurrentMenuText = default;
    public bool IsInStore { get; private set; }
    private int curMenu = 0;


    public void Start()
    {
        this.IsInStore = false;
        foreach (StoreSection storeMenu in this.StoreMenus)
        {
            storeMenu?.SetupDependencies(this.LevelDataReference, this.CursorItem, this.UIElements, this.GridSystem, this.PlayerBalance, this.PlayerBalanceDisplay, this.ResourceManager);
            storeMenu?.Initialize();
        }
        this.PlayerBalanceHUD.GetComponent<TopHUD>().SetupPlayerBalance(this.PlayerBalance);
        StoreMenus[curMenu]?.gameObject.SetActive(true);
        StoreCanvas.localScale = Vector3.zero;

    }

    public void OnToggleMenu(GameObject menu)
    {
        if (currentMenu != menu)
        {
            if (!this.IsInStore)
            {
                this.PauseManager.TryToPause();
                EventManager.Instance.InvokeEvent(EventType.StoreOpened, null);
            }
            this.StoreToggledOn(menu);
        }
        else
        {
            this.StoreToggledOff(menu);
        }
    }

    public void ToggleStore()
    {
        if (!this.IsInStore)
        {
            OpenStore();
        }
        else
        {
            CloseStore();
        }
    }


    public void OpenStore()
    {
        if (!this.IsInStore)
        {
            this.PauseManager.TryToPause();
            EventManager.Instance.InvokeEvent(EventType.StoreOpened, null);
        }
        StoreCanvas.DOScale(0.8f, 0.5f);
        this.IsInStore = true;
    }

    public void CloseStore()
    {
        StoreCanvas.DOScale(0, 0.5f);
        this.IsInStore = false;
        EventManager.Instance.InvokeEvent(EventType.StoreClosed, null);
    }


    private void StoreToggledOn(GameObject menu)
    {
        if (this.currentMenu)
        {
            this.currentMenu.SetActive(false);
        }
        menu.SetActive(true);
        currentMenu = menu;
        //this.PlayerBalanceHUD.SetActive(true);
        this.IsInStore = true;
    }

    private void StoreToggledOff(GameObject menu)
    {
        if (menu != null)
        {
            menu.SetActive(false);
            this.currentMenu = null;
            ///this.PlayerBalanceHUD.SetActive(false);
            this.IsInStore = false;
            this.PauseManager.TryToUnpause();
        }

        EventManager.Instance.InvokeEvent(EventType.StoreClosed, null);
    }

    public void OpenMenu(int menu) {

        StoreMenus[curMenu]?.gameObject.SetActive(false);

        curMenu = menu;

        StoreMenus[curMenu]?.gameObject.SetActive(true);

        MenuSelectPanel.gameObject.SetActive(false);
        CurrentMenuText.text = menuNames[curMenu];
        AudioManager.instance.PlayOneShot(SFXType.TabSwitch);
    }

    public void ToggleMenuSelectPanel() {
        MenuSelectPanel.gameObject.SetActive(!MenuSelectPanel.gameObject.activeSelf);
    }
}
