using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Menu : MonoBehaviour
{
    [SerializeField] protected List<GameObject> menuList;
    [SerializeField] protected GameObject previousSelected;
    public List<GameObject> MenuList { get => menuList; }

    protected virtual void SetSelection(GameObject gameObjToSelect)
    {
        StartCoroutine(SetSelected(gameObjToSelect));
    }

    protected virtual void ToggleMenu(GameObject menuObj, bool state)
    {
        int menuObjListIndex = menuList.IndexOf(menuObj);

        switch (menuObjListIndex)
        {
            case 0:
                break;
            case 1:
                break;
        }

        menuObj.SetActive(state);
    }

    public IEnumerator SetSelected(GameObject gameObjToSelect)
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(gameObjToSelect);
    }
}
