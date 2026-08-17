using System.Collections.Generic;
using UnityEngine;

public class HealthBarDisplay : MonoBehaviour
{
    public GameObject healthBar;

    [SerializeField] private GameObject healthUnit;
    [SerializeField] private PlayerData pData;
    List<HealthUnitDisplay> healthUnitsList = new List<HealthUnitDisplay>();

    private void OnEnable()
    {
        if (EventManager.current != null)
        {
            EventManager.current.OnPlayerDrawHealth += DrawUnits;
            EventManager.current.OnGameLoaded += DrawUnits;
        }
        else
            Debug.LogError("Event Manager not found");
    }

    private void OnDisable()
    {
        EventManager.current.OnPlayerDrawHealth -= DrawUnits;
        EventManager.current.OnGameLoaded -= DrawUnits;
    }

    private void Start()
    {
        DrawUnits();
    }

    public void DrawUnits()
    {
        ClearUnits();

        float maxHealthRemainder = pData.MaxHealth % 4;
        int unitsToInstance = Mathf.CeilToInt((pData.MaxHealth / 4) + maxHealthRemainder);
        for(int i = 0; i < unitsToInstance; i++)
        {
            InstantiateUnit(HealthUnitStatus.Empty);
        }

        for(int i = 0; i < healthUnitsList.Count; i++)
        {
            int healthRemainder = (int) Mathf.Clamp(pData.Health - (i * 4), 0, 4);
            healthUnitsList[i].SetUnitImage((HealthUnitStatus)healthRemainder);
        }
    }

    public void InstantiateUnit(HealthUnitStatus status)
    {
        GameObject newUnit = Instantiate(healthUnit);
        newUnit.transform.SetParent(transform);
        newUnit.transform.localScale = Vector3.one;

        HealthUnitDisplay unitDisplayComponent = newUnit.GetComponent<HealthUnitDisplay>();
        unitDisplayComponent.SetUnitImage(status);
        healthUnitsList.Add(unitDisplayComponent);
    }

    public void ClearUnits()
    {
        foreach(Transform t in transform)
        {
            Destroy(t.gameObject);
        }
        healthUnitsList = new List<HealthUnitDisplay>();
    }
}
