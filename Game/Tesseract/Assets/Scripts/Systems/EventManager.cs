using System;
using UnityEngine;

// Scene-wide events, so systems react to each other without holding references. Raise
// methods exist because C# only lets the declaring class invoke an event, and because scene
// buttons can call methods but not raise events. Runs early (execution order -500) so
// every default-order component can subscribe in OnEnable.
public class EventManager : MonoBehaviour
{
    public static EventManager Current { get; private set; }

    public event Action<int> OnOpenMenu;
    public event Action OnBack;

    public event Action OnGameLoaded;
    public event Action OnNoLoadData;

    public event Action OnPlayerDamage; // combat scaffolding: nothing raises it yet
    public event Action OnPlayerDrawHealth;
    public event Action OnPlayerRestore;
    public event Action OnPlayerDeath;

    public event Action OnCameraZoomIn;
    public event Action OnCameraZoomOut;

    private void Awake()
    {
        if (Current == null) Current = this;
        else Debug.LogError("A second EventManager exists; only the first one is used.");
    }

    public void Pause() => OpenMenu((int)MenuId.Options);

    public void OpenMenu(int menuId) => OnOpenMenu?.Invoke(menuId);

    public void Back() => OnBack?.Invoke();

    public void GameLoaded() => OnGameLoaded?.Invoke();

    public void NoLoadData() => OnNoLoadData?.Invoke();

    public void PlayerDamaged() => OnPlayerDamage?.Invoke();

    public void PlayerDeath() => OnPlayerDeath?.Invoke();

    public void PlayerRestored() => OnPlayerRestore?.Invoke();

    public void PlayerDrawHealth() => OnPlayerDrawHealth?.Invoke();

    public void CameraZoomIn() => OnCameraZoomIn?.Invoke();

    public void CameraZoomOut() => OnCameraZoomOut?.Invoke();
}
