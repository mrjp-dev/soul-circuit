using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;

public class MenuManager : Menu
{
    public PlayerControl plyControl;

    public GameObject menuContainer;
    public List<GameObject> uiObjectsList;
    public List<GameObject> activeMenuList;
    public GameObject currentMenu;

    public GameObject progressButton;
    public GameObject forceLineButton;

    public string lastActiveMenu;
    public int currentMenuId;

    public bool isInMenu = false;
    //public bool showUI = true;

    public static MenuManager Current { get; private set; }

    private void Awake()
    {
        if (Current == null)
            Current = this;
    }

    private void OnEnable()
    {
        if (EventManager.Current != null)
        {
            EventManager.Current.OnOpenMenu += MenuManagement;
            EventManager.Current.OnBack += CloseCurrentMenu;
        }
        else
            Debug.LogError("Event Manager not found");
    }

    private void OnDisable()
    {
        EventManager.Current.OnOpenMenu -= MenuManagement;
        EventManager.Current.OnBack -= CloseCurrentMenu;
    }

    private void Start()
    {
        ManageInputMap(isInMenu? 1 : 0);

        foreach(GameObject menu in menuList)
        {
            menu.SetActive(isInMenu);
        }

        foreach(GameObject uiObj in uiObjectsList)
        {
            uiObj.SetActive(true); // show UI
        }
    }

    public void MenuManagement(int menuId) // This method manages the activation of the menu state
    {
        isInMenu = !isInMenu;
        ManageInputMap(1);
        OpenMenu(menuId);
    }

    public void OpenMenu(int menuId)
    {
        isInMenu = true;
        AddCurrentMenu(menuId);
        ManageCurrentMenu(true);

        switch(menuId)
        {
            case 0: // Options Menu
                // Debug.Log(currentMenu.transform.GetChild(0).gameObject.name);
                SetSelection(currentMenu.transform.GetChild(0).gameObject);
                break;
            case 1: // Inventory Menu
                break;
            /*case 22: // Confirm Menu
                SetSelection(currentMenu.transform.GetChild(1).GetChild(1).gameObject);
                break;*/
            case 2: // Return To Start
                SetSelection(currentMenu.transform.GetChild(1).GetChild(1).gameObject);
                break;
            case 3: // Exit Game
                SetSelection(currentMenu.transform.GetChild(1).GetChild(1).gameObject);
                break;
            case 4: // Dialogue Menu
                StartCoroutine(SetActiveWithDelay(currentMenu.transform.GetChild(1).GetChild(1).GetChild(0).gameObject, true));
                //SetSelection(currentMenu.transform.GetChild(1).GetChild(1).GetChild(0).gameObject);
                break;
        }
    }

    public void CloseCurrentMenu()
    {
        lastActiveMenu = currentMenu.name;

        ManageCurrentMenu(false);
    }

    private void AddCurrentMenu(int menuId)
    {
        SetCurrentMenu(menuId);
        activeMenuList.Add(currentMenu);
        currentMenuId = activeMenuList.IndexOf(currentMenu);
    }

    private void SetCurrentMenu(int menuId)
    {
        currentMenu = menuList[menuId];
    }

    private void ManageCurrentMenu(bool activate)
    {
        currentMenu.SetActive(activate);

        if (activate == false)
        {
            activeMenuList.Remove(currentMenu);
            CheckForActiveMenus();
        }
    }

    private void ManageInputMap(int mode)
    {
        

        switch(mode) // Change player input map modes (0 : Player, 1 : UI)
        {
            case 0:
                menuContainer.SetActive(false);
                PlayerControl.Current?.InputComponent.SwitchCurrentActionMap("Player");
                SetSelection(null);
                break;

            case 1:
                menuContainer.SetActive(true);
                PlayerControl.Current?.InputComponent.SwitchCurrentActionMap("UI");
                break;
        }            
    }

    private void CheckForActiveMenus()
    {
        if (activeMenuList.Count < 1)
        {
            TriggerMenuClose();
        }
        else
        {
            currentMenuId--;
            SetCurrentMenu(currentMenuId);
            if (currentMenu.name == "Menu_Options")
                if (lastActiveMenu == "Menu_ConfirmReturnToStart")
                    SetSelection(currentMenu.transform.GetChild(2).gameObject);
                else if (lastActiveMenu == "Menu_ConfirmExitGame")
                    SetSelection(currentMenu.transform.GetChild(3).gameObject);
        } 
    }

    private void TriggerMenuClose()
    {
        isInMenu = false;
        ManageInputMap(0);
    }

    private IEnumerator SetActiveWithDelay(GameObject obj, bool select)
    {
        yield return new WaitForNextFrameUnit();
        obj.SetActive(true);

        if(select)
            SetSelection(obj);
    }

    public void ChangeScene(string newScene)
    {
        SceneManager.LoadScene(newScene);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
