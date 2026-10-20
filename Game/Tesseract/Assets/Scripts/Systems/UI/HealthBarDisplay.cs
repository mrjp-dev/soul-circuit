using UnityEngine;

// Draws one heart per four points of max health and redraws whenever health or a loaded
// save changes it.
public class HealthBarDisplay : MonoBehaviour
{
    private const int QuartersPerHeart = 4;

    [SerializeField] private GameObject healthUnit; // prefab with a HealthUnitDisplay
    [SerializeField] private PlayerData pData;

    private void OnEnable()
    {
        EventManager.Current.OnPlayerDrawHealth += DrawUnits;
        EventManager.Current.OnGameLoaded += DrawUnits;
    }

    private void OnDisable()
    {
        EventManager.Current.OnPlayerDrawHealth -= DrawUnits;
        EventManager.Current.OnGameLoaded -= DrawUnits;
    }

    private void Start() => DrawUnits();

    private void DrawUnits()
    {
        foreach (Transform heart in transform) Destroy(heart.gameObject);

        // Rounds up. The old count was max/4 + max%4, so 14 max health drew 5 hearts.
        int hearts = (pData.MaxHealth + QuartersPerHeart - 1) / QuartersPerHeart;
        for (int i = 0; i < hearts; i++)
        {
            int filled = Mathf.Clamp(pData.Health - i * QuartersPerHeart, 0, QuartersPerHeart);
            Instantiate(healthUnit, transform).GetComponent<HealthUnitDisplay>().SetQuarters(filled);
        }
    }
}
