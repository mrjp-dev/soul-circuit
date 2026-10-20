using System;
using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Runs Ink conversations: writes each line letter by letter, offers its choices, and applies
// its #tags. Global Ink variables outlive conversations and are stored in saves.
public class DialogueManager : MonoBehaviour, IDataPersistence
{
    public static DialogueManager Current { get; private set; }

    private static readonly int DefaultPortrait = Animator.StringToHash("default");

    [SerializeField] private GameObject[] choices;
    [SerializeField] private GameObject progressButton;
    [SerializeField] private GameObject forceLineButton;
    [SerializeField] private TextAsset loadGlobalVariablesJSON;
    [SerializeField] private TextMeshProUGUI textDialogueSource;
    [SerializeField] private TextMeshProUGUI textDialogue;
    [SerializeField] private Animator portraitAnimator;
    [SerializeField] private float textDelay; // seconds per character

    // The dialogue box grows with its text up to these limits, then wraps.
    [SerializeField] private LayoutElement layoutTextDialogue;
    [SerializeField] private RectTransform rTransformTextDialogue;
    [SerializeField] private float maxWidthTextDialogue = 600;
    [SerializeField] private float maxHeightTextDialogue = 40;

    private TextMeshProUGUI[] choiceTexts;
    private Dictionary<string, Action<string>> tagHandlers;
    private DialogueVariables globals;
    private Story story;
    private NonPlayable speaker;
    private bool isWriting;
    private bool skipRequested;

    public GameObject ProgressButton => progressButton;
    public GameObject ForceLineButton => forceLineButton;

    // True when a line starts writing, false when it is fully shown.
    public event Action<bool> OnWritingLine;

    private void Awake()
    {
        if (Current == null) Current = this;
        else Debug.LogError("A second DialogueManager exists; only the first one is used.");

        choiceTexts = Array.ConvertAll(choices, choice => choice.GetComponentInChildren<TextMeshProUGUI>());

        // Defaults until a save is loaded. The old code only set these when persistence was
        // disabled, so a first run with no save hit a null reference on the first conversation.
        globals = new DialogueVariables(loadGlobalVariablesJSON.text, null);

        // One row per Ink tag: supporting a new #tag is one more line here. Unknown tags are ignored.
        tagHandlers = new Dictionary<string, Action<string>>
        {
            ["source"] = value => textDialogueSource.text = value switch
            {
                "known" => speaker.Data.NpName,
                "unknown" => "???",
                _ => value,
            },
            ["portrait"] = value => portraitAnimator.Play(value),
            ["save"] = _ => DataManager.Current.SaveGame(),
            ["restore"] = _ => EventManager.Current.PlayerRestored(),
        };
    }

    public void StartDialogue(GameObject interactable)
    {
        speaker = interactable.GetComponent<NonPlayable>();
        if (speaker.Data.Dialogue == null)
        {
            Debug.LogWarning($"{interactable.name} has no dialogue asset.");
            return;
        }

        // NPCs turn to face the player; other speakers, like save points, stay put.
        if (interactable.GetComponent<CustomTag>().HasTag("NPC"))
            interactable.GetComponentInChildren<NPAnimation>().SetDirection(PlayerControl.Current.transform.eulerAngles.y + 180f);

        story = new Story(speaker.Data.Dialogue.text);
        EventManager.Current.OpenMenu((int)MenuId.Dialogue);
        globals.StartListening(story);

        textDialogue.text = "";
        textDialogueSource.text = speaker.Data.NpName;
        portraitAnimator.Play(DefaultPortrait);
        DisplayNextLine();
    }

    // Wired to the progress button.
    public void DisplayNextLine()
    {
        // Preserved: asking for a line mid-write ends the conversation. The progress button is
        // hidden while writing, so players should not reach it.
        if (!story.canContinue || isWriting)
        {
            EndDialogue();
            return;
        }

        layoutTextDialogue.preferredWidth = -1;
        layoutTextDialogue.preferredHeight = -1;
        string line = story.Continue();

        if (string.IsNullOrEmpty(line))
        {
            EndDialogue();
            return;
        }

        StartCoroutine(WriteLine(line));
        ApplyTags(story.currentTags);
        FitTextDialogue();
    }

    // Wired to the choice buttons.
    public void MakeChoice(int index)
    {
        if (isWriting) return;

        story.ChooseChoiceIndex(index);
        DisplayNextLine();
    }

    // Wired to the finish-line button: shows the rest of the line at once.
    public void ForceFinishLine() => skipRequested = true;

    private void EndDialogue()
    {
        EventManager.Current.Back();
        textDialogue.text = "";
        globals.StopListening(story);
        story = null;
    }

    private void ApplyTags(List<string> tags)
    {
        foreach (string tag in tags)
        {
            int colon = tag.IndexOf(':');
            string key = colon < 0 ? tag : tag.Substring(0, colon).Trim();
            string value = colon < 0 ? null : tag.Substring(colon + 1).Trim();
            if (tagHandlers.TryGetValue(key, out Action<string> apply)) apply(value);
        }
    }

    private IEnumerator WriteLine(string line)
    {
        SetWriting(true);
        textDialogue.text = line;
        textDialogue.maxVisibleCharacters = 0;
        if (!skipRequested) EventSystem.current.SetSelectedGameObject(forceLineButton);

        // maxVisibleCharacters counts visible characters only, so rich-text tags cost no
        // delay; the original skipped them by hand. One cached wait instead of one per letter.
        textDialogue.ForceMeshUpdate();
        int total = textDialogue.textInfo.characterCount;
        WaitForSeconds delay = new WaitForSeconds(textDelay);
        while (textDialogue.maxVisibleCharacters < total && !skipRequested)
        {
            textDialogue.maxVisibleCharacters++;
            yield return delay;
        }

        textDialogue.maxVisibleCharacters = total;
        skipRequested = false;
        SetWriting(false);
    }

    private void SetWriting(bool writing)
    {
        isWriting = writing;
        ShowChoices(writing ? new List<Choice>() : story.currentChoices);
        progressButton.SetActive(story.currentChoices.Count == 0 && MenuManager.Current.IsOpen(MenuId.Dialogue));
        progressButton.transform.localScale = writing ? Vector3.zero : Vector3.one;
        OnWritingLine?.Invoke(writing);
    }

    private void ShowChoices(List<Choice> available)
    {
        for (int i = 0; i < choices.Length; i++)
        {
            bool shown = i < available.Count;
            choices[i].SetActive(shown);
            if (shown) choiceTexts[i].text = available[i].text;
        }

        EventSystem.current.SetSelectedGameObject(available.Count > 0 ? choices[0] : progressButton);
    }

    // -1 hands sizing back to the layout group until the text reaches a limit.
    private void FitTextDialogue()
    {
        Rect rect = rTransformTextDialogue.rect;
        layoutTextDialogue.preferredWidth = rect.width >= maxWidthTextDialogue ? maxWidthTextDialogue : -1;
        layoutTextDialogue.preferredHeight = rect.height >= maxHeightTextDialogue ? maxHeightTextDialogue : -1;
    }

    public void SaveData(GameData data) => data.inkVariables = globals.SaveVariables();

    public void LoadData(GameData data) => globals = new DialogueVariables(loadGlobalVariablesJSON.text, data.inkVariables);
}
