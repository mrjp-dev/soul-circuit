using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveDataProfile : MonoBehaviour
{
    [Header("Data Profiles")]
    [SerializeField] private string dataProfileID = "";
    [SerializeField] private TextMeshProUGUI dataProfileNameText;
    [SerializeField] private TextMeshProUGUI dataProfileTimeText;

    private bool hasData;

    private Button saveButton;

    public bool HasData { get => hasData; }

    private void Awake()
    {
        saveButton = GetComponent<Button>();
    }

    public void SetData(GameData data)
    {
        if (data == null)
        {
            dataProfileNameText.text = "New Save";
            dataProfileTimeText.text = "";
        }
        else
        {
            hasData = true;
            dataProfileNameText.text = dataProfileID;
            dataProfileTimeText.text = TimeSpan.FromSeconds(data.totalPlaytime).ToString("c");
        }
    }

    public string GetDataProfileID()
    {
        return dataProfileID;
    }

    public void SetInteractable(bool state)
    {
        saveButton.interactable = state;
    }
}
