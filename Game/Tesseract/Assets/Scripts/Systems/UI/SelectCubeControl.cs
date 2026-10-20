using UnityEngine;
using UnityEngine.EventSystems;

// The spinning cube that marks the selected menu control. Hidden while a dialogue line is
// being written, because the selectable then is the invisible finish-line button.
public class SelectCubeControl : MonoBehaviour
{
    private const float RotationSpeed = 90f; // degrees per second, around two axes

    [SerializeField] private GameObject selectCube;

    private bool isWritingDialogue;

    // Guarded because scenes without dialogue, like a future start menu, still use the cube.
    private void OnEnable()
    {
        if (DialogueManager.Current != null) DialogueManager.Current.OnWritingLine += SetWriting;
    }

    private void OnDisable()
    {
        if (DialogueManager.Current != null) DialogueManager.Current.OnWritingLine -= SetWriting;
    }

    private void Update()
    {
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        bool shown = selected != null && !(isWritingDialogue && IsDialogueButton(selected));
        selectCube.transform.localScale = shown ? Vector3.one * 4f : Vector3.zero;
        selectCube.transform.Rotate(RotationSpeed * Time.deltaTime * new Vector3(1f, 0f, 1f));

        if (selected != null)
        {
            Vector3 target = selected.transform.position;
            selectCube.transform.localPosition = new Vector3(target.x, target.y, transform.localPosition.z);
        }
    }

    private void SetWriting(bool writing) => isWritingDialogue = writing;

    // Only reached while a line is being written, which only DialogueManager reports, so Current is set.
    private static bool IsDialogueButton(GameObject selected) =>
        selected == DialogueManager.Current.ProgressButton || selected == DialogueManager.Current.ForceLineButton;
}
