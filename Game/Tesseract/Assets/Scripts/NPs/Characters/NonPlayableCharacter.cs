using UnityEngine;

// A non-playable character. Allegiance is combat scaffolding: nothing calls the toggles yet.
public class NonPlayableCharacter : NonPlayable
{
    [SerializeField] protected NPCData characterData;

    public override NPData Data => characterData;

    // Allegiances are exclusive: setting one clears the others, clearing one leaves the rest.
    // The original also forgot to clear Neutral and never found its CustomTag component.
    public void ToggleHostile(bool state) =>
        SetAllegiance(state, characterData.IsFriendly && !state, characterData.IsNeutral && !state);

    public void ToggleFriendly(bool state) =>
        SetAllegiance(characterData.IsHostile && !state, state, characterData.IsNeutral && !state);

    public void ToggleNeutral() => SetAllegiance(false, false, true);

    // The CustomTag mirrors the data so tag queries and the asset always agree.
    private void SetAllegiance(bool hostile, bool friendly, bool neutral)
    {
        characterData.IsHostile = hostile;
        characterData.IsFriendly = friendly;
        characterData.IsNeutral = neutral;

        CustomTag tags = GetComponent<CustomTag>();
        tags.Set("Hostile", hostile);
        tags.Set("Friendly", friendly);
        tags.Set("Neutral", neutral);
    }
}
