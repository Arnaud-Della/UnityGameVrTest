using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
public class GetAvatarFromRPM : MonoBehaviour
{
    public TMP_InputField codeRPM;
    public TMP_InputField Url;
    WebSocket ws;
    private string a;
    private bool pass = false;
    private void Start()
    {
        this.GetComponent<Button>().onClick.AddListener(OnClick);
        ws = new WebSocket("wss://nodetest.ddns.net:443");
        ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
        ws.Connect();
        ws.OnMessage += (sender, e) =>
        {
            Debug.Log($"reception de {e.Data}");
            a = e.Data;
            pass = false;
        };
    }
    private void OnClick()
    {
        Debug.Log($"envoie de {codeRPM.text}");
        ws.Send("Demande de url"+codeRPM.text);
    }
    private void Update()
    {
        if (!pass)
        {
            Url.text = a;
            pass = true;
        }
    }
}