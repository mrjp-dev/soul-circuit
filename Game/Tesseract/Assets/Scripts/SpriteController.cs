using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteController : MonoBehaviour
{
    private Quaternion defaultOrientationQuaternion = Quaternion.Euler(0f, 0f, 0f);
    private void FixedUpdate()
    {
        transform.rotation = defaultOrientationQuaternion;
    }
}
