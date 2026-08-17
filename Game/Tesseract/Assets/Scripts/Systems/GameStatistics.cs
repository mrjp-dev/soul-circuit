using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStatistics : MonoBehaviour, IDataPersistence
{
    private float playtime = 0f;
    private void Update()
    {
        playtime += Time.deltaTime;
    }

    #region Saving & Loading

    public void SaveData(GameData data)
    {
        data.totalPlaytime += playtime;
    }

    public void LoadData(GameData data)
    {
    }

    #endregion
}
