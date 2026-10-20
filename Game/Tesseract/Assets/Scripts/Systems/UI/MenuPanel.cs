using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

// Selects the panel's default control whenever the panel is shown, so a new menu needs only
// this component and a slot in MenuManager's list; MenuManager never learns its layout.
public class MenuPanel : MonoBehaviour
{
    [SerializeField] private GameObject defaultFocus;

    private void OnEnable() => StartCoroutine(Focus(defaultFocus));

    // Clear, wait a frame, then select: carried over from the original Menu.SetSelected,
    // because a selection made in the frame a control is activated does not highlight.
    public static IEnumerator Focus(GameObject target)
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(target);
    }
}
