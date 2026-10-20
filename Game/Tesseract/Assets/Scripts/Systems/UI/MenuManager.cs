using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// Opens and closes the in-game menus as a stack. Each panel focuses itself (MenuPanel); this
// class only tracks order and, when a panel closes, returns focus to whatever opened it. That
// one rule replaces the old per-menu switch and the name checks that sent a cancelled confirm
// dialog back to its button, and it works for any menu added later.
public class MenuManager : MonoBehaviour
{
    public static MenuManager Current { get; private set; }

    [SerializeField] private GameObject menuContainer;
    [SerializeField] private List<GameObject> menuList; // indexed by MenuId
    [SerializeField] private List<GameObject> uiObjectsList; // HUD shown during play

    private readonly Stack<(GameObject menu, GameObject opener)> open = new Stack<(GameObject, GameObject)>();

    private void Awake()
    {
        if (Current == null) Current = this;
    }

    private void OnEnable()
    {
        EventManager.Current.OnOpenMenu += OpenMenu;
        EventManager.Current.OnBack += CloseCurrentMenu;
    }

    private void OnDisable()
    {
        EventManager.Current.OnOpenMenu -= OpenMenu;
        EventManager.Current.OnBack -= CloseCurrentMenu;
    }

    private void Start()
    {
        foreach (GameObject menu in menuList) menu.SetActive(false);
        foreach (GameObject hud in uiObjectsList) hud.SetActive(true);
        SetMenuMode(false);
    }

    public bool IsOpen(MenuId id) => menuList[(int)id].activeSelf;

    // Scene buttons call this with raw ints; see MenuId for the order.
    public void OpenMenu(int id)
    {
        GameObject menu = menuList[id];
        if (menu.activeSelf) return; // an open panel is never stacked twice

        open.Push((menu, EventSystem.current.currentSelectedGameObject));
        SetMenuMode(true);
        menu.SetActive(true);
    }

    public void CloseCurrentMenu()
    {
        if (open.Count == 0) return;

        (GameObject menu, GameObject opener) = open.Pop();
        menu.SetActive(false);

        if (open.Count == 0) SetMenuMode(false);
        else EventSystem.current.SetSelectedGameObject(opener);
    }

    public void ChangeScene(string scene)
    {
        // Save while this scene's objects still exist; by sceneUnloaded they are destroyed.
        DataManager.Current.SaveGame();
        SceneManager.LoadScene(scene);
    }

    public void ExitGame() => Application.Quit();

    // Menus take input from the UI action map and the HUD-only state takes it from Player.
    private void SetMenuMode(bool inMenu)
    {
        menuContainer.SetActive(inMenu);
        if (PlayerControl.Current != null)
            PlayerControl.Current.InputComponent.SwitchCurrentActionMap(inMenu ? "UI" : "Player");
        if (!inMenu) EventSystem.current.SetSelectedGameObject(null);
    }
}
