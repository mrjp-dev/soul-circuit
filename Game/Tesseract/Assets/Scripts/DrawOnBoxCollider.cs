using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class DrawOnBoxCollider : MonoBehaviour
{
    public enum DrawStates
    {
        DrawCube,
        DrawWireCube
    }

    public Transform boxColliderToDrawOn;
    [Range(0f, 1f)]
    public float colorTransparency = 1f;
    public bool draw = true;
    public DrawStates drawStates = DrawStates.DrawWireCube;
    public Color colliderColor = Color.white;

    private BoxCollider boxCollider = null;

    // Start is called before the first frame update
    void Start()
    {
        if (boxColliderToDrawOn == null)
        {
            boxCollider = transform.GetComponent<BoxCollider>();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void DrawGizmosOnRunTime(Color color)
    {
        if (boxCollider != null && draw)
        {
            Gizmos.color = color;
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(boxCollider.transform.position, boxCollider.transform.rotation, boxCollider.transform.lossyScale);
            Gizmos.matrix = rotationMatrix;

            switch (drawStates)
            {
                case DrawStates.DrawCube:
                    Gizmos.DrawCube(boxCollider.center, boxCollider.size);
                    break;

                case DrawStates.DrawWireCube:
                    Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
                    break;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if(boxCollider.enabled)
        {
            Color c = colliderColor;
            c.a = colorTransparency;
            DrawGizmosOnRunTime(c);
        }
        
    }
}