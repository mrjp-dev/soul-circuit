using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Start screen: a main menu plus one save-slot menu shared by New Game and Load Game.
// Planned, not built yet: StartMenuManager.prefab holds only this component and no scene
// places it. When the UI is built, give the main menu buttons Automatic or Vertical
// navigation, so hiding Continue and Load needs no hand-wired navigation.
public class StartMenuControl : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject saveMenu;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button loadGameButton;

    private SaveDataProfile[] saveSlots;
    private GameObject returnFocus;
    private bool isLoading; // slots load a save; otherwise they start a new game in that slot

    private void Awake() => saveSlots = saveMenu.GetComponentsInChildren<SaveDataProfile>(true);

    private void OnEnable() => EventManager.Current.OnNoLoadData += HideSaveDependentButtons;

    private void OnDisable() => EventManager.Current.OnNoLoadData -= HideSaveDependentButtons;

    // Continue is the natural first choice when a save exists, New Game when none does.
    private void Start()
    {
        saveMenu.SetActive(false);
        mainMenu.SetActive(true);
        bool hasSave = !string.IsNullOrEmpty(DataManager.Current.CurrentDataProfileID);
        Focus(hasSave ? continueButton.gameObject : newGameButton.gameObject);
    }

    public void NewGame() => OpenSaveMenu(false, newGameButton.gameObject);

    public void LoadGame() => OpenSaveMenu(true, loadGameButton.gameObject);

    public void ContinueGame() => ChangeScene(DataManager.Current.GetGameData().lastActiveSceneName);

    public void ChangeScene(string scene)
    {
        SetInteractable(mainMenu, false); // no second click while the scene loads
        SceneManager.LoadSceneAsync(scene);
    }

    public void ExitGame() => Application.Quit();

    // Back out of the slot menu to the button that opened it. Unwired until the UI exists.
    public void CloseSaveMenu()
    {
        saveMenu.SetActive(false);
        mainMenu.SetActive(true);
        Focus(returnFocus);
    }

    // Wired to each save slot button.
    public void LoadWithSaveProfile(SaveDataProfile slot)
    {
        DataManager.Current.LoadWithDataProfileID(slot.GetDataProfileID());
        if (!isLoading) DataManager.Current.NewGame(); // a new game overwrites whatever the slot held
        ChangeScene(DataManager.Current.GetGameData().lastActiveSceneName);
    }

    private void OpenSaveMenu(bool loading, GameObject opener)
    {
        isLoading = loading;
        returnFocus = opener;
        mainMenu.SetActive(false);
        saveMenu.SetActive(true);

        Dictionary<string, GameData> profiles = DataManager.Current.GetAllDataProfiles();
        foreach (SaveDataProfile slot in saveSlots)
        {
            profiles.TryGetValue(slot.GetDataProfileID(), out GameData data);
            slot.SetData(data);
            slot.SetInteractable(data != null || !loading); // only Load needs a save in the slot
        }

        if (saveSlots.Length > 0) Focus(saveSlots[0].gameObject);
    }

    // DataManager raises NoLoadData when the current profile has nothing to load, so
    // Continue and Load would lead nowhere.
    private void HideSaveDependentButtons()
    {
        continueButton.gameObject.SetActive(false);
        loadGameButton.gameObject.SetActive(false);
    }

    private void Focus(GameObject target) => StartCoroutine(MenuPanel.Focus(target));

    private static void SetInteractable(GameObject menu, bool state)
    {
        foreach (Selectable control in menu.GetComponentsInChildren<Selectable>()) control.interactable = state;
    }
}
