using Photon.Pun;
using ReadyPlayerMe;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AvatarFactory : MonoBehaviour
{
    private string avatarURL;
    private GameObject avatar;
    private List<Transform> HumanBones;
    public RuntimeAnimatorController ControllerAnimator;
    public Avatar SqueletteAvatarAnimator;
    private Network network;
    private Vector3 position;
    private bool IsMine = true;
    private PhotonView AvatarPhotonView;
    private int ViewID;

    private PhotonView photonView;
    private void Start()
    {
        photonView = this.GetComponent<PhotonView>();
        network = FindObjectOfType<Network>();
        avatarURL = network.Url;
        CreateNewAvatar(avatarURL, new Vector3(0,0,0));

        photonView.RPC("CreateNewAvatarSync", RpcTarget.All, avatarURL, new Vector3(0, 0, 0),ViewID);
    }

    [PunRPC]
    public void CreateNewAvatarSync(string url, Vector3 position,int ID)
    {
        string name = CreateNewAvatar(url, position);
        GameObject.Find(name).GetComponent<PhotonView>().ViewID = ID;
    }

    public string CreateNewAvatar(string url, Vector3 position)
    {
        Debug.Log($"Started loading avatar");
        this.position = position;
        AvatarLoader avatarLoader = new AvatarLoader();
        avatarLoader.OnCompleted += AvatarLoadComplete;
        avatarLoader.OnFailed += AvatarLoadFail;
        avatarLoader.LoadAvatar(url);
        return GetAvatarName(url);
    }

    private string GetAvatarName(string url)
    {
        return url.Split("/")[url.Split("/").Length - 1].Split(".")[url.Split("/")[url.Split("/").Length - 1].Split(".").Length - 2];
    }
    private void AvatarLoadComplete(object sender, CompletionEventArgs args)
    {
        Debug.Log($"Avatar loaded");

        // On recupere le gameobject de l'avatar qui vient d'etre creer
        avatar = args.Avatar;
        avatar.transform.position = position;
        GameObject Casque = GameObject.Find("Casque");
        GameObject ManetteDroite = GameObject.Find("ManetteDroite");
        GameObject ManetteGauche = GameObject.Find("ManetteGauche");
        // On recupere le squelettes de l'avatar sous forme de Liste
        Animator myAnimator = avatar.GetComponent<Animator>();
        GetAllHumanoidBones(myAnimator);
        // On oublie pas de remettre le runtimecontroller, sinon erreur (chimpanzé)
        myAnimator.runtimeAnimatorController = null;

        // On ajoute a l'avatar de nouveau components, RigBuilder et BoneRenderer
        RigBuilder myRigBuilder = avatar.AddComponent<RigBuilder>();
        BoneRenderer myBoneRenderer = avatar.AddComponent<BoneRenderer>();

        // On precise le squeletes utiliser au BoneRenderer
        myBoneRenderer.transforms = HumanBones.ToArray();

        // On creer un GameObject Rig dans l'avatar
        // On y ajoute un bras droit et gauche ainsi que une tete
        GameObject MyRig = addNewNode(avatar, "MyRig");
        GameObject BrasDroit = addNewNode(MyRig, "BrasDroit");
        GameObject BrasGauche = addNewNode(MyRig, "BrasGauche");
        GameObject TargetDroit = addNewNode(BrasDroit, "Target");
        GameObject HintDroit = addNewNode(BrasDroit, "Hint");
        GameObject TargetGauche = addNewNode(BrasGauche, "Target");
        GameObject HintGauche = addNewNode(BrasGauche, "Hint");

        MyRig.AddComponent<Rig>();
        myRigBuilder.layers.Clear();
        myRigBuilder.layers.Add(new RigLayer(MyRig.GetComponent<Rig>()));

        TwoBoneIKConstraint TwoBoneDroit = BrasDroit.AddComponent<TwoBoneIKConstraint>();
        TwoBoneDroit.enabled = true;
        TwoBoneDroit.data.root = HumanBones[14];
        TwoBoneDroit.data.mid = HumanBones[16];
        TwoBoneDroit.data.tip = HumanBones[18];
        TwoBoneDroit.data.target = TargetDroit.transform;
        TwoBoneDroit.data.hint = HintDroit.transform;
        TwoBoneDroit.data.targetRotationWeight = 1f;
        TwoBoneDroit.data.targetPositionWeight = 1f;
        TwoBoneDroit.data.hintWeight = 1f;

        TargetDroit.transform.position = HumanBones[18].transform.position;
        HintDroit.transform.position = HumanBones[16].transform.position;
        TargetDroit.transform.rotation = HumanBones[18].transform.rotation;
        HintDroit.transform.rotation = HumanBones[16].transform.rotation;


        TwoBoneIKConstraint TwoBoneGauche = BrasGauche.AddComponent<TwoBoneIKConstraint>();
        TwoBoneGauche.enabled = true;
        TwoBoneGauche.data.root = HumanBones[13];
        TwoBoneGauche.data.mid = HumanBones[15];
        TwoBoneGauche.data.tip = HumanBones[17];
        TwoBoneGauche.data.target = TargetGauche.transform;
        TwoBoneGauche.data.hint = HintGauche.transform;
        TwoBoneGauche.data.targetRotationWeight = 1f;
        TwoBoneGauche.data.targetPositionWeight = 1f;
        TwoBoneGauche.data.hintWeight = 1f;

        TargetGauche.transform.position = HumanBones[17].transform.position;
        HintGauche.transform.position = HumanBones[15].transform.position;
        TargetGauche.transform.rotation = HumanBones[17].transform.rotation;
        HintGauche.transform.rotation = HumanBones[15].transform.rotation;

        GameObject TeteContrainte = addNewNode(MyRig, "TeteContrainte");
        MultiParentConstraint multiParentConstraint = TeteContrainte.AddComponent<MultiParentConstraint>();
        multiParentConstraint.data.constrainedObject = HumanBones[10];
        var tamp = new WeightedTransformArray();
        tamp.Add(new WeightedTransform(TeteContrainte.transform, 1f));
        multiParentConstraint.data.sourceObjects = tamp;

        TeteContrainte.transform.position = HumanBones[10].transform.position;
        TeteContrainte.transform.rotation = HumanBones[10].transform.rotation;

        AvatarPhotonView = avatar.AddComponent<PhotonView>();

        if (IsMine)
        {
            Casque.transform.position = HumanBones[10].transform.position;
            ManetteDroite.transform.position = HumanBones[18].transform.position;
            ManetteGauche.transform.position = HumanBones[17].transform.position;
            MoveScript moveScript = avatar.AddComponent<MoveScript>();
            moveScript.speed = 1;
            moveScript.Casque = Casque;
            moveScript.ManetteDroite = ManetteDroite;
            moveScript.ManetteGauche = ManetteGauche;
            PhotonNetwork.AllocateViewID(AvatarPhotonView);
            ViewID = AvatarPhotonView.ViewID;
            IsMine = false;
        }

        multiParentConstraint.data.constrainedPositionXAxis = true;
        multiParentConstraint.data.constrainedPositionYAxis = true;
        multiParentConstraint.data.constrainedPositionZAxis = true;
        multiParentConstraint.data.constrainedRotationXAxis = true;
        multiParentConstraint.data.constrainedRotationYAxis = true;
        multiParentConstraint.data.constrainedRotationZAxis = true;

        VRRig VRRigScript = avatar.AddComponent<VRRig>();
        VRRigScript.headConstraint = TeteContrainte.transform;
        VRMap VRMapLeftHand = new VRMap(ManetteGauche.transform, TargetGauche.transform, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        VRMap VRMapRightHand = new VRMap(ManetteDroite.transform, TargetDroit.transform, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        VRMap VRMapTeteContrainte = new VRMap(Casque.transform, TeteContrainte.transform, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        VRRigScript.head = VRMapTeteContrainte;
        VRRigScript.leftHand = VRMapLeftHand;
        VRRigScript.rightHand = VRMapRightHand;
        VRRigScript.turnSmoothness = 3;


        Debug.Log($"Avatar loaded Finish");
        myAnimator.avatar = SqueletteAvatarAnimator;
        myAnimator.runtimeAnimatorController = ControllerAnimator;
        myRigBuilder.Build();

    }

    private GameObject addNewNode(GameObject parentOb, string name)
    {
        GameObject childOb = new GameObject(name);
        childOb.transform.SetParent(parentOb.transform);
        return childOb;
    }

    private void AvatarLoadFail(object sender, FailureEventArgs args)
    {
        Debug.Log($"Avatar loading failed with error message: {args.Message}");
    }

    private Transform[] GetAllHumanoidBones(Animator _animator)
    {
        HumanBones = new List<Transform>();

        if (_animator == null) return null;

        foreach (HumanBodyBones bone in Enum.GetValues(typeof(HumanBodyBones)))
        {
            if (bone != HumanBodyBones.LastBone)
            {
                Transform tamp = _animator.GetBoneTransform(bone);
                if (tamp != null)
                {
                    HumanBones.Add(tamp);
                }

            }

        }
        return HumanBones.ToArray();
    }
}