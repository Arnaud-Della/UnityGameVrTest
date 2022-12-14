using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinRoom : MonoBehaviour
{
    public TMP_InputField RoomName;
    public TMP_InputField Url;

    void Start()
    {
        this.GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        Network.Connection(RoomName.text, Url.text);
    }
}