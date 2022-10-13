using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Network : MonoBehaviourPunCallbacks
{
    private string RoomName = "default-room";
   public string Url = "https://d1a370nemizbjq.cloudfront.net/209a1bc2-efed-46c5-9dfd-edc8a1d9cbe4.glb";

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
        JoinTheRoom();
        Perturbation();
    }

    #endregion

    private void JoinTheRoom()
    {
        SceneManager.LoadScene("Room");
    }
    public int GetNbPlayer()
    {
        return PhotonNetwork.CurrentRoom.PlayerCount;
    }

    public event EventHandler SomethingHappened;

    public void Perturbation()
    {
        SomethingHappened.Invoke(this, EventArgs.Empty);
    }
}