using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonPlayableCharacter : NonPlayable // NPC class inherits from NP class, which is a monobehaviour
{
    [SerializeField] protected NPCData characterData; // NPC data inherits from NP data, which is a scriptable object
    public NPCData CharacterData { get => characterData; }

    private const string hostileStr = "Hostile";
    private const string friendlyStr = "Friendly";
    private const string neutralStr = "Neutral";

    /* The ToggleHostile, ToggleFriendly, and ToggleNeutral check for existing friendly/hostile tags and
     * change them according to the value
     * 
     * If the tags don't exist they're added.
     */

    #region Toggles

    public void ToggleHostile(bool state)
    {

        characterData.IsHostile = state;

        if (state)
        {
            ToggleFriendly(!state);

            if (CustomTagComp.HasTag(friendlyStr))
                customTagComp.Rename(customTagComp.GetTagIndex(friendlyStr), hostileStr);

            else if (CustomTagComp.HasTag(neutralStr))
                customTagComp.Rename(customTagComp.GetTagIndex(neutralStr), hostileStr);

            else if (customTagComp.HasTag(hostileStr))
                Debug.LogWarning("NPC is already hostile!");

            else
                customTagComp.Add(hostileStr);
        }
        else
            customTagComp.Remove(hostileStr);
    }

    public void ToggleFriendly(bool state)
    {

        characterData.IsFriendly = state;
        if (state)
        {
            ToggleHostile(!state);

            if (customTagComp.HasTag(hostileStr))
                customTagComp.Rename(customTagComp.GetTagIndex(hostileStr), friendlyStr);

            else if(CustomTagComp.HasTag(neutralStr))
                customTagComp.Rename(customTagComp.GetTagIndex(neutralStr), friendlyStr);

            else if (customTagComp.HasTag(friendlyStr))
                Debug.LogWarning("NPC is already friendly!");

            else
                customTagComp.Add(friendlyStr);
        }
        else
            customTagComp.Remove(friendlyStr);
    }

    public void ToggleNeutral()
    {
        characterData.IsNeutral = true;

        ToggleHostile(false);
        ToggleFriendly(false);

        if (customTagComp.HasTag(hostileStr))
            customTagComp.Rename(customTagComp.GetTagIndex(hostileStr), neutralStr);

        else if (customTagComp.HasTag(friendlyStr))
            customTagComp.Rename(customTagComp.GetTagIndex(friendlyStr), neutralStr);

        else if (customTagComp.HasTag(neutralStr))
            Debug.LogWarning("NPC is already neutral!");

        else
            customTagComp.Add(neutralStr);
    }

    #endregion

    protected override void Awake()
    {
        base.Awake();

        baseData = characterData; // NPC data is assigned to Non-Playable's base data
    }
}
