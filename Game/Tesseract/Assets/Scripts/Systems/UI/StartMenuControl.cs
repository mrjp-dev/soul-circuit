using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StartMenuControl : Menu
{
    [Header("Main Menu Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Save Slots Buttons")]
    [SerializeField] private Button saveSlot1;
    [SerializeField] private Button saveSlot2;
    [SerializeField] private Button saveSlot3;

    private bool hasSelectedLoadGame;


    private SaveDataProfile[] saveDataProfiles;

    private void Awake()
    {
        saveDataProfiles = menuList[1].GetComponentsInChildren<SaveDataProfile>();

        if (string.IsNullOrEmpty(DataManager.Current.CurrentDataProfileID))
            EventSystem.current.SetSelectedGameObject(newGameButton.gameObject);
        else
            EventSystem.current.SetSelectedGameObject(continueButton.gameObject);

        menuList[0].SetActive(false);
        menuList[1].SetActive(false);
    }
    private void OnEnable()
    {
        EventManager.Current.OnNoLoadData += DestroyContinueLoad;
    }

    private void OnDisable()
    {
        EventManager.Current.OnNoLoadData -= DestroyContinueLoad;
    }

    private void Start()
    {
        menuList[0].SetActive(true);
        ToggleMainMenuButtons(true);
    }

    public void NewGame()
    {
        ToggleMainMenu(false);
        hasSelectedLoadGame = false;
        ToggleSaveMenu(true);
        
        previousSelected = newGameButton.gameObject;
    }

    public void ContinueGame()
    {
        ToggleMainMenuButtons(false);
        SceneManager.LoadSceneAsync(DataManager.Current.GetGameData().lastActiveSceneName);
    }

    public void LoadGame()
    {
        ToggleMainMenu(false);
        hasSelectedLoadGame = true;
        ToggleSaveMenu(true);

        previousSelected = loadGameButton.gameObject;
    }

    public void ChangeScene(string newScene)
    {
        SceneManager.LoadSceneAsync(newScene);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void ToggleMainMenuButtons(bool state)
    {
        // Main Menu Buttons
        if(newGameButton != null)
            newGameButton.interactable = state;
        if (continueButton != null)
            continueButton.interactable = state;
        if (loadGameButton != null)
            loadGameButton.interactable = state;
        if (settingsButton != null)
            settingsButton.interactable = state;
        if (quitButton != null)
            quitButton.interactable = state;
    }

    public void ToggleSaveMenuButtons(bool state)
    {
        // Save Slots Buttons
        if (saveSlot1 != null)
            saveSlot1.interactable = state;
        if (saveSlot2 != null)
            saveSlot2.interactable = state;
        if (saveSlot3 != null)
            saveSlot3.interactable = state;
    }

    public void DestroyContinueLoad()
    {
        Navigation newGameButtonNav = new Navigation
        {
            mode = Navigation.Mode.Explicit,

            selectOnUp = quitButton,
            selectOnLeft = quitButton,
            selectOnDown = settingsButton,
            selectOnRight = settingsButton
        };

        newGameButton.navigation = newGameButtonNav;

        Navigation settingsButtonNav = new Navigation
        {
            mode = Navigation.Mode.Explicit,

            selectOnUp = newGameButton,
            selectOnLeft = newGameButton,
            selectOnDown = quitButton,
            selectOnRight = quitButton
        };

        settingsButton.navigation = settingsButtonNav;

        if(continueButton != null)
            Destroy(continueButton.gameObject);
        if (loadGameButton != null)
            Destroy(loadGameButton.gameObject);
    }

    public void ToggleMainMenu(bool state)
    {
        ToggleMainMenuButtons(state);
        menuList[0].SetActive(state);
    }

    public void ToggleSaveMenu(bool state)
    {
        ToggleSaveMenuButtons(state);
        menuList[1].SetActive(state);

        if(state)
        {
            SetSelection(saveSlot1.gameObject);
            Dictionary<string, GameData> gameDataProfiles = DataManager.Current.GetAllDataProfiles();

            foreach (SaveDataProfile save in saveDataProfiles)
            {
                gameDataProfiles.TryGetValue(save.GetDataProfileID(), out GameData profileData);
                save.SetData(profileData);
                if (profileData == null && hasSelectedLoadGame)
                    save.SetInteractable(false);
                else
                    save.SetInteractable(true);
            }
        }
        else
            SetSelection(previousSelected);
    }

    public void LoadWithSaveProfile(SaveDataProfile save)
    {
        ToggleMainMenuButtons(false);

        DataManager.Current.LoadWithDataProfileID(save.GetDataProfileID());

        if (!hasSelectedLoadGame)
            if (save.HasData)
            {
                Debug.LogWarning("Overwriting data");
                DataManager.Current.NewGame();
            }
            else
                DataManager.Current.NewGame();
        //if(DataManager.Current.GetGameData())


        ChangeScene(DataManager.Current.GetGameData().lastActiveSceneName);
    }
}
