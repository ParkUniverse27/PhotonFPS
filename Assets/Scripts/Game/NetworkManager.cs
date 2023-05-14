using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;

    public PhotonView PV;
    public Text F5Text;

    public Transform[] SpawnPoints;

    [Header("Chat")]
    public InputField ChatInput;
    public RectTransform ChatContent;
    public ChatItem ChatItemObject;
    public ScrollRect ChatScroll;

    public bool GameStarted;

    public GameObject MyCharacter;

    bool chatOpened;

    private void Awake() 
    {
        Instance = this;    
    }

    private void Start() 
    {
        CreateCharacter();
        ReloadRoom();
    }

    private void Update() 
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            if(chatOpened)
            {
                if(!string.IsNullOrEmpty(ChatInput.text))
                {   
                    var content = PhotonNetwork.LocalPlayer.NickName + ": " + ChatInput.text;
                    PV.RPC("Send", RpcTarget.All, content);
                }
                ChatInput.text = "";
                chatOpened = false;
                ChatInput.DeactivateInputField();
                ChatInput.Select();
            }
            else
            {
                ChatInput.ActivateInputField();
                ChatInput.Select();
                chatOpened = true;
            }
        }    

        if(PhotonNetwork.IsMasterClient)
        {
            if(Input.GetKeyDown(KeyCode.F5))
            {
                TryGameStart();
            }
        }
    }

    void TryGameStart()
    {
        if(PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            var content = $"<color=red>플레이어가 부족합니다!</color>";
            Send(content);
            return;
        }

        PV.RPC("GameStart", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void GameStart()
    {
        GameStarted = true;
        F5Text.text = "";


        if(PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PV.RPC("CreateCharacter", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    public void CreateCharacter()
    {
        if(MyCharacter != null)
            PhotonNetwork.Destroy(MyCharacter);

        MyCharacter = PhotonNetwork.Instantiate("Player", SpawnPoints[Random.Range(0, SpawnPoints.Length)].position, Quaternion.identity);
    }

    void ReloadRoom()
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            F5Text.text = "";
            return;
        }

        F5Text.text = "F5키를 눌러 시작";
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        var content = $"<color=yellow>{newMasterClient.NickName}이(가) 새 방장이 되었습니다.</color>";
        Send(content);
        ReloadRoom();
    }


    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        var content = $"<color=yellow>{newPlayer.NickName}이(가) 게임에 참여하였습니다.</color>";
        Send(content);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        var content = $"<color=yellow>{otherPlayer.NickName}이(가) 게임을 떠났습니다.</color>";
        Send(content);
    }

    

    [PunRPC]
    public void Send(string text)
    {
        var chatItem = Instantiate(ChatItemObject, Vector3.zero, Quaternion.identity, ChatContent);
        chatItem.Init(text);

        ChatScroll.normalizedPosition = new Vector2(0, -1);
        LayoutRebuilder.ForceRebuildLayoutImmediate(ChatContent);
    }
}
