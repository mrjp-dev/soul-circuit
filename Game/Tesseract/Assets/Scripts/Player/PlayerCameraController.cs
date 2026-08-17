using System;
using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class PlayerCameraController : MonoBehaviour
{
    private CinemachineCamera cmCamera;
    private CinemachineThirdPersonFollow cm3PFollowerComponent;
    private CinemachineHardLookAt cmHardLookAt;
    
    /*
        11.24, 11.25, and 11.26 work as perfect orthographic size for 640x360px at zoom level 1
    */
    private readonly int[] zoomLevels = new[]{1, 2, 4, 5, 8, 10, 20, 40};
    private readonly float[] zoomLevelsOrtho = new[]{11.24f, 5.62f, 2.81f, 2.248f, 1.405f, 1.124f, 0.562f, 0.281f};
    private readonly float[] zoomLevelsPersp = new[]{20f, 10f, 5f, 4f, 2.5f, 2f, 1f, 0.5f};
    private float orthoSize;
    private int zoomLevelIndex = 0;

    public float OrthoSize
    {
        get => orthoSize;
        set
        {
            orthoSize = Mathf.Clamp(value, 2.2f, 11f);
        }
    }
    public int ZoomLevelIndex
    {
        get => zoomLevelIndex;
        set
        {
            zoomLevelIndex = Mathf.Clamp(value, 0, 7);
        }

    }

    private void OnEnable()
    {
        EventManager.Current.OnCameraZoomIn += ZoomIn;
        EventManager.Current.OnCameraZoomOut += ZoomOut;
    }

    private void OnDisable()
    {
        EventManager.Current.OnCameraZoomIn -= ZoomIn;
        EventManager.Current.OnCameraZoomOut -= ZoomOut;
    }

    private void Awake()
    {
        cmCamera = GetComponent<CinemachineCamera>();
        cm3PFollowerComponent = GetComponent<CinemachineThirdPersonFollow>();
        cmHardLookAt = GetComponent<CinemachineHardLookAt>();
        
        orthoSize = 11f;
    }

    private void ZoomIn()
    {
        ZoomLevelIndex++;
        if (cmCamera.Lens.Orthographic)
            cmCamera.Lens.OrthographicSize = zoomLevelsOrtho[ZoomLevelIndex]; // Hardcoded zoom orthographic size according to zoom level
            //cmCamera.Lens.OrthographicSize /= zoomLevels[ZoomLevelIndex];   // Calculated zoom orthographic size according to zoom level
        else
            cm3PFollowerComponent.CameraDistance = zoomLevelsPersp[ZoomLevelIndex];
            //cmCamera.Lens.OrthographicSize /= zoomLevelsPersp[zoomLevelIndex];
        CheckForOddResolution(ZoomLevelIndex);
    }
    private void ZoomOut()
    {
        ZoomLevelIndex--;
        if (cmCamera.Lens.Orthographic)
            cmCamera.Lens.OrthographicSize = zoomLevelsOrtho[ZoomLevelIndex]; // Hardcoded zoom orthographic size according to zoom level
            //cmCamera.Lens.OrthographicSize *= zoomLevels[ZoomLevelIndex];   // Calculated zoom orthographic size according to zoom level
        else
            cm3PFollowerComponent.CameraDistance = zoomLevelsPersp[ZoomLevelIndex];
            //cmCamera.Lens.OrthographicSize *= zoomLevelsPersp[zoomLevelIndex];
        CheckForOddResolution(ZoomLevelIndex);
    }

    private void CheckForOddResolution(int index)
    {
        cmHardLookAt.LookAtOffset.y = index switch
        {
            4 or 7 => 0.03125f,
            _ => 0,
        };
    }
}
