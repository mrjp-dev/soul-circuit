using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One save slot button on the start menu. The id names the slot's folder on disk, so
// renaming it orphans that slot's existing save.
public class SaveDataProfile : MonoBehaviour
{
    [Header("Data Profiles")]
    [SerializeField] private string dataProfileID = "";
    [SerializeField] private TextMeshProUGUI dataProfileNameText;
    [SerializeField] private TextMeshProUGUI dataProfileTimeText;

    public string GetDataProfileID() => dataProfileID;

    public void SetData(GameData data)
    {
        dataProfileNameText.text = data == null ? "New Save" : dataProfileID;
        dataProfileTimeText.text = data == null ? "" : TimeSpan.FromSeconds(data.totalPlaytime).ToString("c");
    }

    public void SetInteractable(bool state) => GetComponent<Button>().interactable = state;
}
