using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonPlayableObject : NonPlayable // NPO class inherits from NP class, which is a monobehaviour
{
    [SerializeField] protected NPOData objectData; // NPO data inherits from NP data, which is a scriptable object
    public NPOData ObjectData { get => objectData; }

    protected override void Awake()
    {
        base.Awake();

        baseData = objectData; // NPO data is assigned to Non-Playable's base data
    }
}
