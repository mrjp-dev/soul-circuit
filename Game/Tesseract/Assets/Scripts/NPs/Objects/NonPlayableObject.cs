using UnityEngine;

// A non-playable that is a thing rather than a character, such as a save point.
public class NonPlayableObject : NonPlayable
{
    [SerializeField] protected NPOData objectData;

    public override NPData Data => objectData;
}
