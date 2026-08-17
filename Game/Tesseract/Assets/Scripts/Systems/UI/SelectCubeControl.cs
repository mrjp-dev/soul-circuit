using UnityEngine;
using UnityEngine.EventSystems;

public class SelectCubeControl : MonoBehaviour
{
    public GameObject selectCube;
    public GameObject currentSelection;

    private readonly float rotationSpeed = 90f;
    public Vector3 offset = new Vector3(-290f, -5, -4f);

    public bool isWritingDialogue = false;

    private void Awake()
    {
    }

    private void OnEnable()
    {
        if (DialogueManager.Current != null)
            DialogueManager.Current.OnWritingLine += IsWritingDialogue;
        else
            Debug.LogWarning("Dialogue Manager not found");
    }

    private void OnDisable()
    {
        if(DialogueManager.Current != null)
            DialogueManager.Current.OnWritingLine -= IsWritingDialogue;
    }

    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            currentSelection = EventSystem.current.currentSelectedGameObject;
            ChangePosition(currentSelection.transform);

            if (DialogueManager.Current != null)
            {
                if (EventSystem.current.currentSelectedGameObject == DialogueManager.Current.ProgressButton || EventSystem.current.currentSelectedGameObject == DialogueManager.Current.ForceLineButton)
                    SetDialogueVisibility();
                else
                    SetVisibility(true);
            }
        }
        else
            SetVisibility(false);
    }

    private void FixedUpdate()
    {
        selectCube.transform.Rotate(rotationSpeed * Time.deltaTime * Vector3.right);
        selectCube.transform.Rotate(rotationSpeed * Time.deltaTime * Vector3.forward);
    }

    private void ChangePosition(Transform newPosition)
    {
        selectCube.transform.localPosition = new Vector3(newPosition.position.x, newPosition.position.y, transform.localPosition.z);
    }

    private void IsWritingDialogue(bool state)
    {
        isWritingDialogue = state;
    }

    private void SetDialogueVisibility()
    {
        selectCube.transform.localScale = !isWritingDialogue ? selectCube.transform.localScale = Vector3.one * 4f : selectCube.transform.localScale = Vector3.zero;
    }
    private void SetVisibility(bool state)
    {
        selectCube.transform.localScale = state ? selectCube.transform.localScale = Vector3.one * 4f : selectCube.transform.localScale = Vector3.zero;
    }
}
