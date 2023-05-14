using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System;
using Random = UnityEngine.Random;

public class GameSelectManager : MonoBehaviourPunCallbacks
{
    public static GameSelectManager Instance;

    public InputField RoomNameInputField;
    public InputField SearchInputField;
    public Dropdown MaxPlayerDrop;
    public Button CreateRoomBtn;
    public Text ErrorText;
    public Text NoRoomText;

    public RectTransform Container;
    public RoomItem ItemObject;

    public List<RoomItem> Rooms = new();

    string currentSearch;

    public bool CanCreateRoom
    {
        get { return !string.IsNullOrEmpty(RoomNameInputField.text); }
    }

    private void Awake()
    {
        Instance = this;
        Screen.SetResolution(960, 540, FullScreenMode.Windowed);
    }

    private void Start()
    {
        ConnectMaster();
        SearchInputField.onValueChanged.AddListener(Search);
        PhotonNetwork.LocalPlayer.NickName = "Player" + Random.Range(0, 100).ToString();
    }

    public void ReloadButton()
    {
        ErrorText.text = "";
        CreateRoomBtn.interactable = CanCreateRoom;
    }

    public void CreateRoom()
    {
        ErrorText.text = "";
        var ro = new RoomOptions();
        ro.MaxPlayers = (byte)(Convert.ToInt16(MaxPlayerDrop.options[MaxPlayerDrop.value].text));
        PhotonNetwork.CreateRoom(RoomNameInputField.text, ro);
        LoadingCanvas.Instance.Show(true);
        LoadingCanvas.Instance.UpdateText("방 생성중");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        LoadingCanvas.Instance.Show(false);
        ErrorText.text = "에러: " + message;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        LoadingCanvas.Instance.Show(false);
        ErrorText.text = "에러: " + message;
    }

    public override void OnJoinedRoom()
    {
        LoadingCanvas.Instance.Show(true);
        LoadingCanvas.Instance.UpdateText("");
        PhotonNetwork.LoadLevel("Game");
    }

    public void Search(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            if(Rooms.Count == 0)
                NoRoomText.text = "현재 존재하는 방이 없습니다.\n화면 왼쪽에서 방을 만들어보세요.";
            else
                NoRoomText.text = "";


            foreach (var room in Rooms)
            {
                room.RoomNameText.text = room.RoomInfo.Name;
                room.gameObject.SetActive(true);
            }

            return;
        }
        currentSearch = text.ToLower();
        bool noRoom = true;
        foreach (var room in Rooms)
        {
            if (room.RoomInfo.Name.ToLower().Contains(text))
            {
                room.gameObject.SetActive(true);
                room.SetTextColor(currentSearch);
                noRoom = false;
            }
            else
            {
                room.RoomNameText.text = room.RoomInfo.Name;
                room.gameObject.SetActive(false);
            }
        }

        if(noRoom)
        {
            if(!string.IsNullOrEmpty(text))
            {
                NoRoomText.text = text + "(을)를 찾을 수 없습니다!";
            }
        }
        else
        {
            NoRoomText.text = "";
        }

    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
        LoadingCanvas.Instance.Show(true);
        LoadingCanvas.Instance.UpdateText("방 참가중");
    }




    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (var roomInfo in roomList)
        {
            var index = Rooms.FindIndex(x => x.RoomInfo.Name == roomInfo.Name);
            if (index != -1)
            {
                if (roomInfo.RemovedFromList)
                {
                    Destroy(Rooms[index].gameObject);
                    Rooms.RemoveAt(index);
                }
                else
                {
                    Rooms[index].Init(roomInfo);
                }
            }
            else
            {
                if (!roomInfo.RemovedFromList)
                {
                    var roomItem = Instantiate(ItemObject, Vector3.zero, Quaternion.identity, Container);
                    roomItem.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
                    roomItem.Init(roomInfo);
                    Rooms.Add(roomItem);
                }
            }

        }

        Search(currentSearch);
    }

    public override void OnConnectedToMaster()
    {
        JoinLobby();
    }

    public override void OnLeftLobby()
    {
        JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        LoadingCanvas.Instance.Show(false);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        LoadingCanvas.Instance.Show(true);
        LoadingCanvas.Instance.UpdateError(cause.ToString());
        PhotonNetwork.ConnectUsingSettings();
    }

    void ConnectMaster()
    {
        PhotonNetwork.ConnectUsingSettings();
        LoadingCanvas.Instance.Show(true);
        LoadingCanvas.Instance.UpdateText("서버 연결중");
    }

    void JoinLobby()
    {
        PhotonNetwork.JoinLobby();
        LoadingCanvas.Instance.Show(true);
        LoadingCanvas.Instance.UpdateText("로비 연결중");
    }
}
