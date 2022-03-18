using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PhotonLauncher : MonoBehaviourPunCallbacks
{

    public static PhotonLauncher Instance;

    [SerializeField] TMP_InputField joinroomNameIF;
    [SerializeField] TMP_InputField createroomNameIF;
    [Space]
    [SerializeField] TMP_Text errorText;
    [Space]

    [SerializeField] TMP_Text roomNameText;
    [Space]
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject playerListItem;
    [Space]
    [SerializeField] GameObject startGameButton;
    [Space]

    [SerializeField] Transform roomListContent;
    [SerializeField] GameObject roomListItem;
   
    [Space]
    [SerializeField] int Lvl1ID;

    void Awake() 
    {
        Instance = this;
    }

    void Start() 
    {
        MainMenuManager.Instance.OpenMenu("Loading");
        Debug.Log("Connecting to master");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() 
    {
        Debug.Log("Connected to master");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby() 
    {
        MainMenuManager.Instance.OpenMenu("Main");
        Debug.Log("Joined lobby");
        PhotonNetwork.NickName = "Player " + Random.Range(0, 1000).ToString("0000");
    }

    public void CreateRoom() 
    {
        if (string.IsNullOrEmpty(createroomNameIF.text))
            return;

        PhotonNetwork.CreateRoom(createroomNameIF.text);
        MainMenuManager.Instance.OpenMenu("Loading");
    }

    public void JoinRoomByName() 
    {
        if (string.IsNullOrEmpty(joinroomNameIF.text))
            return;

        PhotonNetwork.JoinRoom(joinroomNameIF.text);
        MainMenuManager.Instance.OpenMenu("Loading");
    }

    public void JoinRoomFromList(RoomInfo info) 
    {
        PhotonNetwork.JoinRoom(info.Name);
        MainMenuManager.Instance.OpenMenu("Loading");
    }

    public override void OnJoinedRoom() 
    {
        MainMenuManager.Instance.OpenMenu("Room");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        Player[] players = PhotonNetwork.PlayerList;

        foreach (Transform child in playerListContent)
            Destroy(child.gameObject);

        for(int i = 0; i < players.Length; i++)
            Instantiate(playerListItem, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);

        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnMasterClientSwitched(Player newMasterClient) 
    {
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public void LeaveRoom() 
    {
        PhotonNetwork.LeaveRoom();
        MainMenuManager.Instance.OpenMenu("Loading");
    }

    public override void OnLeftRoom() 
    {
        MainMenuManager.Instance.OpenMenu("Main");
    }

    public override void OnCreateRoomFailed(short returnCode, string message) 
    {
        MainMenuManager.Instance.OpenMenu("Error");
        errorText.text = "Error: " + message;
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList) 
    {
        foreach (Transform trans in roomListContent)
            Destroy(trans.gameObject);

        for (int i = 0; i < roomList.Count; i++) 
        {
            if (roomList[i].RemovedFromList)
                continue;
            Instantiate(roomListItem, roomListContent).GetComponent<RoomScript>().SetUp(roomList[i]);
        }
           
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) 
    {
        Instantiate(playerListItem, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
    }

    public void StartGame() 
    {
        PhotonNetwork.LoadLevel(Lvl1ID);
    }

}
