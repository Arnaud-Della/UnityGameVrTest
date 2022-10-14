using Photon.Pun;
using ReadyPlayerMe;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UIElements;

public class AvatarFactory : MonoBehaviour
{
    public GameObject Casque;
    public GameObject ManetteDroite;
    public GameObject ManetteGauche;
    public RuntimeAnimatorController ControllerAnimator;
    public Avatar SqueletteAvatarAnimator;
    public PhotonView photonView;

    private List<Transform> HumanBones;
    private List<AvatarConfiguration> AvatarStampToCreate = new List<AvatarConfiguration>();
    private AvatarConfiguration MyAvatarConfiguration;
    private bool IsMyOwnAvatar = true;
    private string Url;

    private void Start()
    {
        Url = Network.Url;
        Debug.Log(Url);
        CreateNewAvatar(Url);
    }

    private void CreateNewAvatar(string url)
    {
        Debug.Log($"Started loading avatar");
        AvatarLoader avatarLoader = new AvatarLoader();
        avatarLoader.OnCompleted += AvatarLoadComplete;
        avatarLoader.OnFailed += AvatarLoadFail;
        avatarLoader.LoadAvatar(url);
    }

    private void AvatarLoadComplete(object sender, CompletionEventArgs args)
    {
        //////////////////////////// Initialisation des variables /////////////////////////////////

        // Avatar
        GameObject avatar = args.Avatar;
        RigBuilder rigBuilder = avatar.AddComponent<RigBuilder>();
        BoneRenderer boneRenderer = avatar.AddComponent<BoneRenderer>();
        Animator animator = avatar.GetComponent<Animator>();

        // Rig
        GameObject Rig = addNewNode(avatar, "Rig");
        Rig rigComponent = Rig.AddComponent<Rig>();

        // Bras Droit
        GameObject brasDroit = addNewNode(Rig, "BrasDroit");
        GameObject targetDroit = addNewNode(brasDroit, "Target");
        GameObject hintDroit = addNewNode(brasDroit, "Hint");
        TwoBoneIKConstraint twoBoneBrasDroit = brasDroit.AddComponent<TwoBoneIKConstraint>();

        // Bras Gauche
        GameObject brasGauche = addNewNode(Rig, "BrasGauche");
        GameObject targetGauche = addNewNode(brasGauche, "Target");
        GameObject hintGauche = addNewNode(brasGauche, "Hint");
        TwoBoneIKConstraint twoBoneBrasGauche = brasGauche.AddComponent<TwoBoneIKConstraint>();

        // Tete
        GameObject TeteContrainte = addNewNode(Rig, "TeteContrainte");
        MultiParentConstraint multiParentConstrainTeteContrainte = TeteContrainte.AddComponent<MultiParentConstraint>();

        // PhotonConfig
        PhotonView AvatarPhotonView = avatar.AddComponent<PhotonView>();
        PhotonTransformView avatarPhotonTransformView = avatar.AddComponent<PhotonTransformView>();


        //////////////////////////// Declaration des variables /////////////////////////////////

        // Avatar
        rigBuilder.layers.Clear();
        rigBuilder.layers.Add(new RigLayer(rigComponent));
        GetAllHumanoidBones(animator);
        boneRenderer.transforms = HumanBones.ToArray();


        // Bras Droit
        twoBoneBrasDroit.enabled = true;
        twoBoneBrasDroit.data.root = HumanBones[14];
        twoBoneBrasDroit.data.mid = HumanBones[16];
        twoBoneBrasDroit.data.tip = HumanBones[18];
        twoBoneBrasDroit.data.target = targetDroit.transform;
        twoBoneBrasDroit.data.hint = hintDroit.transform;
        twoBoneBrasDroit.data.targetRotationWeight = 1f;
        twoBoneBrasDroit.data.targetPositionWeight = 1f;
        twoBoneBrasDroit.data.hintWeight = 1f;
        // Aligne transform
        targetDroit.transform.position = HumanBones[18].transform.position;
        hintDroit.transform.position = HumanBones[16].transform.position;
        targetDroit.transform.rotation = HumanBones[18].transform.rotation;
        hintDroit.transform.rotation = HumanBones[16].transform.rotation;

        // Bras Gauche
        twoBoneBrasGauche.enabled = true;
        twoBoneBrasGauche.data.root = HumanBones[13];
        twoBoneBrasGauche.data.mid = HumanBones[15];
        twoBoneBrasGauche.data.tip = HumanBones[17];
        twoBoneBrasGauche.data.target = targetGauche.transform;
        twoBoneBrasGauche.data.hint = hintGauche.transform;
        twoBoneBrasGauche.data.targetRotationWeight = 1f;
        twoBoneBrasGauche.data.targetPositionWeight = 1f;
        twoBoneBrasGauche.data.hintWeight = 1f;
        // Aligne transform
        targetGauche.transform.position = HumanBones[17].transform.position;
        hintGauche.transform.position = HumanBones[15].transform.position;
        targetGauche.transform.rotation = HumanBones[17].transform.rotation;
        hintGauche.transform.rotation = HumanBones[15].transform.rotation;

        // Tete
        multiParentConstrainTeteContrainte.data.constrainedObject = HumanBones[10];
        WeightedTransformArray tamp = new WeightedTransformArray();
        tamp.Add(new WeightedTransform(TeteContrainte.transform, 1f));
        multiParentConstrainTeteContrainte.data.sourceObjects = tamp;
        multiParentConstrainTeteContrainte.data.constrainedPositionXAxis = true;
        multiParentConstrainTeteContrainte.data.constrainedPositionYAxis = true;
        multiParentConstrainTeteContrainte.data.constrainedPositionZAxis = true;
        multiParentConstrainTeteContrainte.data.constrainedRotationXAxis = true;
        multiParentConstrainTeteContrainte.data.constrainedRotationYAxis = true;
        multiParentConstrainTeteContrainte.data.constrainedRotationZAxis = true;
        // Aligne transform
        TeteContrainte.transform.position = HumanBones[10].transform.position;
        TeteContrainte.transform.rotation = HumanBones[10].transform.rotation;


        // Photon Configuration
        avatarPhotonTransformView.m_SynchronizePosition = true;
        avatarPhotonTransformView.m_SynchronizeRotation = true;
        avatarPhotonTransformView.m_UseLocal = true;
        AvatarPhotonView.ObservedComponents = new List<Component>() { avatarPhotonTransformView };

        
        if (IsMyOwnAvatar)
        {
            Debug.LogWarning($"Je cree mon propre Avatar {Url}");

            // Aligne transform du Casque et des Manettes du VR avec l'avatar
            Casque.transform.position = HumanBones[10].transform.position;
            ManetteDroite.transform.position = HumanBones[18].transform.position;
            ManetteGauche.transform.position = HumanBones[17].transform.position;

            // Configuration du script de deplacement
            MoveScript moveScript = avatar.AddComponent<MoveScript>();
            moveScript.speed = 1;
            moveScript.Casque = Casque;
            moveScript.ManetteDroite = ManetteDroite;
            moveScript.ManetteGauche = ManetteGauche;

            // Configuration du script de suivit du Casque et des Manettes liées aux mains et à la tete du joueur
            VRRig VRRigScript = avatar.AddComponent<VRRig>();
            VRRigScript.headConstraint = TeteContrainte.transform;
            VRMap VRMapLeftHand = new VRMap(ManetteGauche.transform, targetGauche.transform, Vector3.zero, Vector3.zero);
            VRMap VRMapRightHand = new VRMap(ManetteDroite.transform, targetDroit.transform, Vector3.zero, Vector3.zero);
            VRMap VRMapTeteContrainte = new VRMap(Casque.transform, TeteContrainte.transform, Vector3.zero, Vector3.zero);
            VRRigScript.head = VRMapTeteContrainte;
            VRRigScript.leftHand = VRMapLeftHand;
            VRRigScript.rightHand = VRMapRightHand;
            VRRigScript.turnSmoothness = 3;

            // Allocation d'un ViewID pour mon avatar
            PhotonNetwork.AllocateViewID(AvatarPhotonView);

            // Recuperation de la configuration de mon avatar
            MyAvatarConfiguration = new AvatarConfiguration(AvatarPhotonView.ViewID, Vector3.zero, Url, avatar);
            IsMyOwnAvatar = false;

            // Finalisation de la Creation de l'avatar
            animator.avatar = SqueletteAvatarAnimator;
            animator.runtimeAnimatorController = ControllerAnimator;
            rigBuilder.Build();

            Debug.LogWarning("L'Avatar a été correctement creer");
            Debug.LogWarning($"INFO : {AvatarPhotonView.ViewID}, {avatar.transform.position}");

            // Creation de mon Avatar chez les autres joueur
            photonView.RPC("Sync", RpcTarget.Others, MyAvatarConfiguration.ViewID, Vector3.zero, Url);

            // Ajout de l'evenement SyncWithNewPlayer
            Network.OnPlayerEnteredRoomEventHandler += SyncWithNewPlayer;
        }
        else if (AvatarStampToCreate.Count > 0)
        {
            // Attribution du ViewID et de la position à l'avatar adverse 
            AvatarPhotonView.ViewID = AvatarStampToCreate[0].ViewID;
            avatar.transform.position = AvatarStampToCreate[0].Position;
            // Suppression des informations de configuration de cette avatar du buffer
            AvatarStampToCreate.RemoveAt(0);
        }


        // Finalisation de la Creation de l'avatar
        animator.avatar = SqueletteAvatarAnimator;
        animator.runtimeAnimatorController = ControllerAnimator;
        rigBuilder.Build();




    }

    [PunRPC]
    protected virtual void Sync(int ViewID, Vector3 position, string url)
    {
        Debug.LogWarning($"SYNC : {ViewID}, {position}, {url}");
        AvatarConfiguration avatarConfiguration = new AvatarConfiguration(ViewID, position, url);
        AvatarStampToCreate.Add(avatarConfiguration);
        CreateNewAvatar(url);
    }

    public void SyncWithNewPlayer(object sender, EventPlayer args)
    {
        Debug.LogWarning("Un nouveau joueur vient d'arriver => event");
        photonView.RPC("Sync", args.player, MyAvatarConfiguration.ViewID, MyAvatarConfiguration.GetAvatar.transform.position, MyAvatarConfiguration.Url);
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

[System.Serializable]
public class AvatarConfiguration
{
    public int ViewID;
    public Vector3 Position;
    public string Url;
    private GameObject Avatar;
    public GameObject GetAvatar { get => Avatar;}
    public AvatarConfiguration(int ViewID, Vector3 Position, string Url, GameObject Avatar = null)
    {
        this.ViewID = ViewID;
        this.Position = Position;
        this.Url = Url;
        this.Avatar = Avatar;
    }
}