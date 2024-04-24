using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomList : MonoBehaviour
{
    public List<RoomListItemUI> RoomListItemUIs;
    public List<SessionInfo> SessionInfos = new List<SessionInfo>();

    public void ClearList()
    {
        SessionInfos.Clear();
    }

    public void AddToList(SessionInfo sesssionInfo)
    {
        SessionInfos.Add(sesssionInfo);
    }

    public void Update()
    {
        foreach (RoomListItemUI RoomListItemUI in RoomListItemUIs) RoomListItemUI.gameObject.SetActive(false);

        for (int i = 0; i < SessionInfos.Count && i < RoomListItemUIs.Count; i++)
        {
            int shiftedIndex = i;
            RoomListItemUIs[shiftedIndex].gameObject.SetActive(true);
            RoomListItemUIs[shiftedIndex].SetInformation(SessionInfos[shiftedIndex]);
        }
    }
}