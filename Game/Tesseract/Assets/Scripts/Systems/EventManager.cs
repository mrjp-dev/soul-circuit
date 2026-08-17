using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class EventManager : MonoBehaviour
{
    public static EventManager current { get; private set; }

    public event Action<bool> OnInteractable;

    public event Action OnNoDataPersistence;
    public event Action OnNoSaveData;
    public event Action OnNoLoadData;
    public event Action OnGameLoaded;
    public event Action OnGameSaved;

    public event Action OnPlayerDamage;
    public event Action OnPlayerDrawHealth;
    public event Action OnPlayerRestore;
    public event Action OnPlayerDeath;

    public event Action OnPause;
    public event Action<int> OnOpenMenu;
    public event Action OnBack;

    public event Action OnCameraZoomIn;
    public event Action OnCameraZoomOut;

    private void Awake()
    {
        if (current == null)
            current = this;
        else
            Debug.LogError("Event Manager already exists");
    }

    #region Menu Events
    public void Pause()
    {
        OnPause?.Invoke();
        OnOpenMenu?.Invoke(0);
    }

    public void OpenMenu(int menuId)
    {
        OnOpenMenu?.Invoke(menuId);
    }

    public void Back()
    {
        OnBack?.Invoke();
    }
    #endregion

    #region Player Status Events

    public void GameSaved()
    {
        OnGameSaved?.Invoke();
    }
    public void GameLoaded()
    {
        OnGameLoaded?.Invoke();
    }

    public void NoDataPersistence()
    {
        OnNoDataPersistence?.Invoke();
    }
    public void NoSaveData()
    {
        OnNoSaveData?.Invoke();
    }
    public void NoLoadData()
    {
        OnNoLoadData?.Invoke();
    }

    public void PlayerDamaged()
    {
        OnPlayerDamage?.Invoke();
    }

    public void PlayerDeath()
    {
        OnPlayerDeath?.Invoke();
    }
    
    public void PlayerRestored()
    {
        OnPlayerRestore?.Invoke();
    }
    
    public void PlayerDrawHealth()
    {
        OnPlayerDrawHealth?.Invoke();
    }

    public void Interactable(bool state)
    {
        OnInteractable?.Invoke(state);
    }

    #endregion

    #region Camera Events

    public void CameraZoomIn()
    {
        OnCameraZoomIn?.Invoke();
    }

    public void CameraZoomOut()
    {
        OnCameraZoomOut?.Invoke();
    }

    #endregion
}
