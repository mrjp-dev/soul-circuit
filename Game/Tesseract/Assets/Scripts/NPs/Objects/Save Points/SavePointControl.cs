using UnityEngine;

// A save point. Saving itself happens through the point's Ink dialogue (#save tag), so this
// only makes it interactable and keeps it spinning.
public class SavePointControl : NonPlayableObject
{
    [SerializeField] private float rotationSpeed = 60f; // degrees per second

    private void Start() => objectData.IsInteractable = true;

    private void FixedUpdate() => transform.Rotate(rotationSpeed * Time.fixedDeltaTime * Vector3.down);
}
