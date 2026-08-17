using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CustomTag : MonoBehaviour
{
    [SerializeField]
    private List<string> tags = new List<string>();

    public int GetTagIndex(string tagName)
    {
        return tags.IndexOf(tagName);
    }

    public bool HasTag(string tag)
    {
        return tags.Contains(tag);
    }

    public IEnumerable<string> GetTags()
    {
        return tags;
    }

    public void Add(string tagName)
    {
        tags.Add(tagName);
    }
    public void Remove(string tagName)
    {
        tags.Remove(tagName);
    }

    public void Rename(int index, string tagName)
    {
        tags[index] = tagName;
    }

    public string GetAtIndex(int index)
    {
        return tags[index];
    }

    public int Count
    {
        get { return tags.Count; }
    }
}