using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
    // Start is called before the first frame update
    private PhotonView network;
    void Start()
    {
        network = PhotonView.Get(this);
        this.GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        network.GetComponent<PhotonView>().RPC("startGame", RpcTarget.All);
    }

    [PunRPC]
    public void startGame()
    {
        SceneManager.LoadScene("Jeux");
    }
}