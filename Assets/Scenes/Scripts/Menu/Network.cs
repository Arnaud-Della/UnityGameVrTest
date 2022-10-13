using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Network : MonoBehaviourPunCallbacks
{
    private string RoomName = "default-room";
    public string Url;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
    // Start is called before the first frame update
    public void Connection(string RoomName, string url)
    {
        if (RoomName != "")
        {
            this.RoomName = RoomName;
        }
            
        if (url != "")
        {
            this.Url = url;

        }
        PhotonNetwork.ConnectUsingSettings();
    }

    #region Pun Callbacks

    public override void OnConnectedToMaster()
    {
        // Try to join a random room
        Debug.Log("Connection OK");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        // Try to join a random room
        Debug.Log("Lobby OK");
        RoomOptions roomOptions = new RoomOptions() { IsVisible = false, IsOpen = true, MaxPlayers = 10 };
        PhotonNetwork.JoinOrCreateRoom(RoomName, roomOptions, TypedLobby.Default);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Room OK = " + PhotonNetwork.CurrentRoom);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined OK");
        Debug.Log("Room OK = " + PhotonNetwork.CurrentRoom);
        JoinTheRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Un nouveau joueur vient de se connecter : ");
        Debug.Log("Il y a " + PhotonNetwork.CurrentRoom.PlayerCount + " joueurs dans la room");
        Perturbation(newPlayer);
    }

    #endregion

    private void JoinTheRoom()
    {
        SceneManager.LoadScene("Jeux");
    }
    public int GetNbPlayer()
    {
        return PhotonNetwork.CurrentRoom.PlayerCount;
    }

    public event EventHandler<EventPlayer> SomethingHappened;

    public void Perturbation(Player player)
    {
        EventPlayer playersending = new EventPlayer();
        playersending.player = player;
        SomethingHappened?.Invoke(this, playersending);
    }
}

public class EventPlayer :EventArgs
{
    public Player player;
}