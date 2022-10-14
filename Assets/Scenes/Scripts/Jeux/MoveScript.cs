using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveScript : MonoBehaviour
{
    private Animator anim;
    public int speed = 2;
    public GameObject ManetteGauche;
    public GameObject ManetteDroite;
    public GameObject Casque;
    void Start()
    {
        anim = GetComponent<Animator>();
        this.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
    }

    // Update is called once per frame
    void Update()
    {
        float y = Input.GetAxis("Vertical");
        anim.SetFloat("Vertical", y);
        float x = Input.GetAxis("Horizontal");
        anim.SetFloat("Horizontal", x);

        if (Input.GetKey(KeyCode.UpArrow))
        {
            Casque.transform.position += Casque.transform.forward * Time.deltaTime * speed;
            ManetteDroite.transform.position += Casque.transform.forward * Time.deltaTime * speed;
            ManetteGauche.transform.position += Casque.transform.forward * Time.deltaTime * speed;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            Casque.transform.position -= Casque.transform.forward * Time.deltaTime * speed;
            ManetteDroite.transform.position -= Casque.transform.forward * Time.deltaTime * speed;
            ManetteGauche.transform.position -= Casque.transform.forward * Time.deltaTime * speed;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            Casque.transform.position += Casque.transform.right * Time.deltaTime * speed;
            ManetteDroite.transform.position += Casque.transform.right * Time.deltaTime * speed;
            ManetteGauche.transform.position += Casque.transform.right * Time.deltaTime * speed;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            Casque.transform.position -= Casque.transform.right * Time.deltaTime * speed;
            ManetteDroite.transform.position -= Casque.transform.right * Time.deltaTime * speed;
            ManetteGauche.transform.position -= Casque.transform.right * Time.deltaTime * speed;
        }

    }
}