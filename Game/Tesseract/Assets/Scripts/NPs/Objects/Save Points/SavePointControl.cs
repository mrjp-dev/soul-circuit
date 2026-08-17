using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePointControl : NonPlayableObject
{
    [SerializeField] private float rotationSpeed = 60f; // speed at which the game object rotates

    protected override void Awake()
    {
        base.Awake();

        gameObj = transform.gameObject;
        CustomTagComp = GetComponent<CustomTag>();
        RendererComp = GetComponent<Renderer>();
        ColliderComp = GetComponent<BoxCollider>();
    }

    private void Start()
    {
        objectData.IsInteractable = true;
    }

    private void FixedUpdate()
    {
        transform.Rotate(rotationSpeed * Time.fixedDeltaTime * Vector3.down); // Make game object rotate constantly at a fixed speed
    }
}
