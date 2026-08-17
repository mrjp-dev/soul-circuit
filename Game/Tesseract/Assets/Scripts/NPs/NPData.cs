using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPData : ScriptableObject
{
    [SerializeField] protected GameObject gameObj; // non-playable's game object
    [SerializeField] protected string npName; // name of the non-playable
    [SerializeField] protected Dialogue dialogue; // non-playable's dialogue if interacted with
    [SerializeField] protected bool isInteractable; // true if can be interacted with

    public Dialogue Dialogue { get => dialogue; }
    public string NpName { get => npName; }
    public bool IsInteractable { get => isInteractable; set => isInteractable = value; }
}
