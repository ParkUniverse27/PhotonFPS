using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

public class RoomItem : MonoBehaviour
{
    public Text RoomNameText;
    public Text PlayerCountText;
    public RoomInfo RoomInfo;

    public void Init(RoomInfo info)
    {
        RoomInfo = info;
        RoomNameText.text = info.Name;
        if(info.PlayerCount == info.MaxPlayers)
        {
            PlayerCountText.text = "<color=#FF5F5F>FULL!</color>";
        } 
        else 
        {
            PlayerCountText.text = $"{info.PlayerCount}/{info.MaxPlayers}";
        }

        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(()=>GameSelectManager.Instance.JoinRoom(RoomInfo.Name));
    }

    public void SetTextColor(string value)
    {
        RoomNameText.text = RoomInfo.Name;
        int i = RoomInfo.Name.ToLower().IndexOf(value);
        if (i >= 0) RoomNameText.text = RoomInfo.Name.Insert(i + value.Length, "</color>").Insert(i, "<color=#78DE78>");
    }
}
