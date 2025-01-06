using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GhostCarData
{
    [SerializeField]
    List<GhostCarDataListItem> ghostCarRecorderList = new List<GhostCarDataListItem>();

    public void AddDataItem(GhostCarDataListItem ghostCarDataListItem)
    {
        ghostCarRecorderList.Add(ghostCarDataListItem);
    }

    public List<GhostCarDataListItem> GetDataList()
    {
        return ghostCarRecorderList;
    }
}
