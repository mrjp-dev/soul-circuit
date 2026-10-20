using System.Collections.Generic;
using UnityEngine;

// Free-form tags saying what kind of thing an object is ("NPC", "Item", allegiances). Unity
// allows one built-in tag per object, which is already spent on "Interactable".
public class CustomTag : MonoBehaviour
{
    [SerializeField] private List<string> tags = new List<string>();

    public bool HasTag(string tag) => tags.Contains(tag);

    // Present or absent, never duplicated, so callers can mirror a flag without checking first.
    public void Set(string tag, bool present)
    {
        tags.Remove(tag);
        if (present) tags.Add(tag);
    }
}
