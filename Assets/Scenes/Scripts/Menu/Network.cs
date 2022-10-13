using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine.SceneManagement;

public class Network : MonoBehaviourPunCallbacks
{
    private static string RoomName;
    public static string Url;
    public static event EventHandler<EventPlayer> OnPlayerEnteredRoomEventHandler;
    public static int GetNbPlayer { get => PhotonNetwork.CurrentRoom.PlayerCount; }

    public static void Connection(string roomName, string url)
    {
        if (roomName == string.Empty)
        {
            RoomName = "default-room";
        }
        else
        {
            RoomName = roomName;
        }

        if (url == string.Empty)
        {
            Url = "https://d1a370nemizbjq.cloudfront.net/209a1bc2-efed-46c5-9dfd-edc8a1d9cbe4.glb";
        }
        else
        {
            Url = url;
        }
        
        PhotonNetwork.ConnectUsingSettings();
    }

    #region Pun Callbacks

    public override void OnConnectedToMaster()
    {
        //Debug.Log("Connection OK");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        //Debug.Log("Lobby OK");
        RoomOptions roomOptions = new RoomOptions() { IsVisible = false, IsOpen = true, MaxPlayers = 10 };
        PhotonNetwork.JoinOrCreateRoom(RoomName, roomOptions, TypedLobby.Default);
    }

    public override void OnCreatedRoom()
    {
        //Debug.Log("Room OK = " + PhotonNetwork.CurrentRoom);
    }

    public override void OnJoinedRoom()
    {
        //Debug.Log("Joined OK");
        //Debug.Log("Room OK = " + PhotonNetwork.CurrentRoom);
        JoinTheRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        /*Debug.Log("Un nouveau joueur vient de se connecter : ");
        Debug.Log("Il y a " + PhotonNetwork.CurrentRoom.PlayerCount + " joueurs dans la room");*/
        OnPlayerEnteredRoomEvent(newPlayer);
    }

    #endregion

    public void OnPlayerEnteredRoomEvent(Player player)
    {
        EventPlayer playersending = new EventPlayer();
        playersending.player = player;
        OnPlayerEnteredRoomEventHandler?.Invoke(this, playersending);
    }

    private void JoinTheRoom()
    {
        SceneManager.LoadScene("Jeux");
    }

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
}

public class EventPlayer : EventArgs
{
    public Player player;
}