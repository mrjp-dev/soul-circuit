using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Ink.Runtime;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour, IDataPersistence
{
    public static DialogueManager current { get; private set; }

    [SerializeField] private GameObject[] choices;
    [SerializeField] private GameObject progressButton;
    public GameObject ProgressButton { get => progressButton;}

    [SerializeField] private GameObject forceLineButton;
    public GameObject ForceLineButton { get => forceLineButton;}

    [Header("Load Global Variables JSON")]
    [SerializeField] private TextAsset loadGlobalVariablesJSON;

    [SerializeField] private TextMeshProUGUI textDialogueSource;
    [SerializeField] private TextMeshProUGUI textDialogue;
    [SerializeField] private TextMeshProUGUI[] textChoices;

    [SerializeField] private Animator portraitAnimator;

    [SerializeField] private NonPlayable currentNonPlayable;

    private const string sourceTag = "source";
    private const string voiceTag = "voice";
    private const string portraitTag = "portrait";
    private const string layoutTag = "layout";
    private const string saveTag = "save";
    private const string restoreTag = "restore";

    private const string sourceKnown = "known";
    private const string sourceUnknown = "unknown";
    
    public LayoutElement layoutTextDialogue;
    public RectTransform rTransformTextDialogue;

    public float maxWidthTextDialogue = 600;
    public float maxHeightTextDialogue = 40;

    [SerializeField] private bool canDisplayNextLine = true;
    [SerializeField] private bool forceFinishLine = false;

    private Story currentDialogue;
    private DialogueVariables globalDialogueVariables;

    [SerializeField] private float textDelay;

    public float TextDelay { get => textDelay; set => textDelay = value; }
    

    public event Action<bool> OnWritingLine;

    public event Action OnPassedMaxWidth;
    public event Action OnPassedMaxHeight;

    public void PassedMaxWidth()
    {
        OnPassedMaxWidth?.Invoke();
    }

    public void PassedMaxHeight()
    {
        OnPassedMaxHeight?.Invoke();
    }

    public void WritingLine(bool state)
    {
        OnWritingLine?.Invoke(state);
    }

    private void Awake()
    {
        if (current == null)
            current = this;
        else
            Debug.LogError("Another instance of Dialogue Manager already exists");

        if (MenuManager.Current != null)
        {
            progressButton = MenuManager.Current.progressButton;
            forceLineButton = MenuManager.Current.forceLineButton;
        }
        else
            Debug.LogError("Menu Manager not found");
    }

    public void OnEnable()
    {
        if (current != null)
        {
            OnWritingLine += DisplayChoices;
            OnWritingLine += SetProgressButtonActive;
            OnWritingLine += SetProgressButtonVisibility;

            EventManager.current.OnNoDataPersistence += LoadDataDefaults;
        }
        else
            Debug.LogError("Dialogue Manager not found");
        
    }
    public void OnDisable()
    {
        OnWritingLine -= DisplayChoices;
        OnWritingLine -= SetProgressButtonActive;
        OnWritingLine += SetProgressButtonVisibility;

        EventManager.current.OnNoDataPersistence -= LoadDataDefaults;
    }

    private void Start()
    {
        //globalDialogueVariables = new DialogueVariables(loadGlobalVariablesJSON, null);

        textChoices = new TextMeshProUGUI[choices.Length];
        int index = 0;
        foreach(GameObject choice in choices)
        {
            textChoices[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }
    }

    public void StartDialogue(GameObject interactable)
    {
        if(interactable.GetComponent<CustomTag>().HasTag("NPC"))
        {
            float lookAtAngle = (PlayerControl.Current.gameObject.transform.eulerAngles.y + 180);
            interactable.GetComponentInChildren<NPAnimation>().SetDirection(lookAtAngle);
        }

        currentNonPlayable = interactable.GetComponent<NonPlayable>();

        if (currentNonPlayable.BaseData.Dialogue.DialogueJSON != null)
            currentDialogue = new Story(currentNonPlayable.BaseData.Dialogue.DialogueJSON.text);
        else
        {
            Debug.LogWarning("Non-playable has no dialogue text asset attached");
            return;
        }

        EventManager.current.OpenMenu(4);
        globalDialogueVariables.StartListening(currentDialogue);

        textDialogueSource.text = "";
        textDialogue.text = ""; 
        portraitAnimator.Play("default"); 

        DisplayDialogueSource(currentNonPlayable.BaseData.NpName);
        DisplayNextLine();
    }

    public void EndDialogue()
    {
        EventManager.current.Back();
        textDialogue.text = "";
        globalDialogueVariables.StopListening(currentDialogue);
        currentDialogue = null;
    }

    public void DisplayDialogueSource(string sourceName)
    {
        textDialogueSource.text = sourceName;
    }

    public void DisplayNextLine()
    {
        if (currentDialogue.canContinue && canDisplayNextLine)
        {
            layoutTextDialogue.preferredWidth = -1;
            layoutTextDialogue.preferredHeight = -1;

            string nextLine = currentDialogue.Continue();

            StopAllCoroutines();

            if (string.IsNullOrEmpty(nextLine))
                EndDialogue();
            else
            {
                StartCoroutine(WriteLine(nextLine, textDelay)); // Write the corresponding line

                HandleTags(currentDialogue.currentTags);

                SetTextDialogueWidth();
                SetTextDialogueHeight();
            }
        }
        else if (currentDialogue.canContinue == false && canDisplayNextLine)
            EndDialogue();
        else
            EndDialogue();
    }

    private void HandleTags(List<string> tagsList)
    {
        foreach(string tag in tagsList)
        {
            string tagKey;
            string tagValue;

            if (tag.Contains(":"))
            {
                string[] splitTag = tag.Split(':');

                if (splitTag.Length != 2)
                    Debug.LogError("Tag not parsed appropiately!");

                tagKey = splitTag[0].Trim();
                tagValue = splitTag[1].Trim();
            }
            else
            {
                tagKey = tag;
                tagValue = null;
            }

            switch(tagKey)
            {
                case sourceTag:
                    textDialogueSource.text = tagValue switch
                    {
                        sourceKnown => currentNonPlayable.BaseData.NpName,// display the non-playable's name as the source if it's known, and as '???' otherwise
                        sourceUnknown => "???",
                        _ => tagValue,
                    };
                    break;

                case portraitTag:
                    portraitAnimator.Play(tagValue);
                    break;

                case voiceTag:
                    // set the sound to play
                    break;

                case layoutTag:
                    break;

                case saveTag:
                    DataManager.current.SaveGame();
                    break;
                
                case restoreTag:
                    EventManager.current.PlayerRestored();
                    break;

                default:
                    break;
            }
        }
    }

    public void DisplayChoices(bool state)
    {
        if(state == false)
        {
            List<Choice> currentChoices = currentDialogue.currentChoices;

            int index = 0;
            foreach (Choice choice in currentChoices)
            {
                choices[index].SetActive(true);
                textChoices[index].text = choice.text;
                index++;
            }

            for (int i = index; i < choices.Length; i++)
            {
                choices[i].SetActive(false);
            }

            if (currentChoices.Count != 0)
                EventSystem.current.SetSelectedGameObject(choices[0]);
            else
                EventSystem.current.SetSelectedGameObject(progressButton);
        }
        
        else
        {
            for (int i = 0; i < choices.Length; i++)
            {
                choices[i].SetActive(false);
            }
            EventSystem.current.SetSelectedGameObject(progressButton);
        }
    }

    public void MakeChoice(int choiceIndex)
    {
        if(canDisplayNextLine)
        {
            currentDialogue.ChooseChoiceIndex(choiceIndex);
            DisplayNextLine();
        }
    }

    public void SetProgressButtonActive(bool state)
    {
        if (currentDialogue.currentChoices.Count != 0)
            progressButton.SetActive(false);
        else
            if(MenuManager.Current.MenuList[4].activeSelf)
                progressButton.SetActive(true);
            else
                progressButton.SetActive(false);
    }

    IEnumerator WriteLine(string line, float delay)
    {
        canDisplayNextLine = false;
        WritingLine(true);
        bool isAddingRichText = false;
        textDialogue.text = line;
        textDialogue.maxVisibleCharacters = 0;

        if(forceFinishLine == false)
            EventSystem.current.SetSelectedGameObject(forceLineButton);

        foreach (char character in line.ToCharArray())
        {
            if(forceFinishLine)
            {
                textDialogue.maxVisibleCharacters = line.Length;
                ResetFinishLine();
                break;
            }

            if (character == '<' || isAddingRichText)
            {
                isAddingRichText = true;
                if (character == '>')
                    isAddingRichText = false;
            }
            else
            {
                textDialogue.maxVisibleCharacters++;
                // play sound
                yield return new WaitForSeconds(delay);
            }

        }
        WritingLine(false);
        canDisplayNextLine = true;
    }

    public void ForceFinishLine()
    {
        //Debug.Log("Forcing Line to finish");
        forceFinishLine = true;
    }
    public void ResetFinishLine()
    {
        forceFinishLine = false;
        EventSystem.current.SetSelectedGameObject(progressButton);
    }

    private void SetTextDialogueWidth()
    {
        if (rTransformTextDialogue.rect.width >= maxWidthTextDialogue)
            layoutTextDialogue.preferredWidth = maxWidthTextDialogue;
        else
            layoutTextDialogue.preferredWidth = -1;
    }

    private void SetTextDialogueHeight()
    {
        if (rTransformTextDialogue.rect.height >= maxHeightTextDialogue)
            layoutTextDialogue.preferredHeight = maxHeightTextDialogue;
        else
            layoutTextDialogue.preferredHeight = -1;
    }

    public Ink.Runtime.Object GetVariableState(string variableName)
    {
        Ink.Runtime.Object variableValue;
        globalDialogueVariables.variables.TryGetValue(variableName, out variableValue);
        if (variableValue != null)
            Debug.LogWarning("Ink variable not found or its value is null");
        return variableValue;
    }

    private void SetProgressButtonVisibility(bool state)
    {
        progressButton.transform.localScale = state ? Vector3.zero : progressButton.transform.localScale = Vector3.one;
    }

    public void SaveData(GameData data)
    {
        data.inkVariables = globalDialogueVariables.SaveVariables();
    }

    public void LoadData(GameData data)
    {
        /*Debug.Log(data.inkVariables);
        if (data.inkVariables != null)
            globalDialogueVariables.LoadVariables(data.inkVariables);*/
        globalDialogueVariables = new DialogueVariables(loadGlobalVariablesJSON, data.inkVariables);
    }

    public void LoadDataDefaults()
    {
        //Debug.Log("Loading default global variables");
        globalDialogueVariables = new DialogueVariables(loadGlobalVariablesJSON, null);
    }
}