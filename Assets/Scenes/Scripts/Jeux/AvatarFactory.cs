using Newtonsoft.Json;
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
    private GameObject MyAvatar;
    private GameObject MyTete;
    private GameObject MyBrasDroit;
    private GameObject MyBrasGauche;
    private bool IsMyOwnAvatar = true;
    private string Url;

    private void Start()
    {
        Url = Network.Url;
        if (Url == String.Empty)
        {
            Url = "https://api.readyplayer.me/v1/avatars/6349385f1ae70b92586e4a8d.glb";
        }
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

        // Main PhotonConfig
        PhotonView AvatarPhotonView = avatar.AddComponent<PhotonView>();
        PhotonTransformView avatarPhotonTransformView = avatar.AddComponent<PhotonTransformView>();

        // Recurrence PhotonConfig
        // Tete
        PhotonView tetePhotonView = TeteContrainte.AddComponent<PhotonView>();
        PhotonTransformView tetePhotonTransformView = TeteContrainte.AddComponent<PhotonTransformView>();
        // Bras Droit
        PhotonView brasDroitPhotonView = brasDroit.AddComponent<PhotonView>();
        PhotonTransformView brasDroitPhotonTransformView = brasDroit.AddComponent<PhotonTransformView>();
        // Bras Gauche
        PhotonView brasGauchePhotonView = brasGauche.AddComponent<PhotonView>();
        PhotonTransformView brasGauchePhotonTransformView = brasGauche.AddComponent<PhotonTransformView>();


        //////////////////////////// Declaration des variables /////////////////////////////////


        // Avatar
        avatar.name = Guid.NewGuid().ToString();
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


        // Main Photon Configuration
        avatarPhotonTransformView.m_SynchronizePosition = true;
        avatarPhotonTransformView.m_SynchronizeRotation = true;
        avatarPhotonTransformView.m_UseLocal = true;
        AvatarPhotonView.ObservedComponents = new List<Component>() { avatarPhotonTransformView };

        // Recurrence Photon Configuration
        tetePhotonTransformView.m_SynchronizePosition = true;
        tetePhotonTransformView.m_SynchronizeRotation = true;
        tetePhotonTransformView.m_UseLocal = true;
        tetePhotonView.ObservedComponents = new List<Component>() { tetePhotonTransformView };




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

            // Configuration du script de suivit du Casque et des Manettes li�es aux mains et � la tete du joueur
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

            // Allocation des ViewID Recurrence
            PhotonNetwork.AllocateViewID(tetePhotonView);

            // Recuperation de la configuration de mon avatar
            MyAvatar = avatar;
            MyTete = TeteContrainte;
            MyBrasDroit = brasDroit;
            MyBrasGauche = brasGauche;
            IsMyOwnAvatar = false;

            Debug.LogWarning("L'Avatar a �t� correctement creer");
            Debug.LogWarning($"INFO : {AvatarPhotonView.ViewID}, {avatar.transform.position}");

            // Creation de mon Avatar chez les autres joueur
            var conf = new AvatarConfiguration(
                    Url,
                    new GameObjectConfig(AvatarPhotonView.ViewID, MyAvatar.transform.position, MyAvatar.transform.rotation),
                    new GameObjectConfig(tetePhotonView.ViewID, MyTete.transform.position, MyTete.transform.rotation),
                    new GameObjectConfig(brasDroitPhotonView.ViewID, MyBrasDroit.transform.position, MyBrasDroit.transform.rotation),
                    new GameObjectConfig(brasGauchePhotonView.ViewID, MyBrasGauche.transform.position, MyBrasGauche.transform.rotation)
            );
            
            photonView.RPC("Sync", RpcTarget.Others, JsonConvert.SerializeObject(conf));

            // Ajout de l'evenement SyncWithNewPlayer
            Network.OnPlayerEnteredRoomEventHandler += SyncWithNewPlayer;
        }
        else if (AvatarStampToCreate.Count > 0)
        {
            // Attribution du ViewID et de la position � l'avatar adverse 
            AvatarPhotonView.ViewID = AvatarStampToCreate[0].Avatar.ViewID;
            avatar.transform.position = AvatarStampToCreate[0].Avatar.Position;
            avatar.transform.rotation = AvatarStampToCreate[0].Avatar.Rotation;
            tetePhotonView.ViewID = AvatarStampToCreate[0].Tete.ViewID;
            TeteContrainte.transform.position = AvatarStampToCreate[0].Tete.Position;
            TeteContrainte.transform.rotation = AvatarStampToCreate[0].Tete.Rotation;
            
            // Suppression des informations de configuration de cette avatar du buffer
            AvatarStampToCreate.RemoveAt(0);
        }


        // Finalisation de la Creation de l'avatar
        //animator.avatar = null;
        //animator.runtimeAnimatorController = null;
        rigBuilder.Build();




    }

    [PunRPC]
    protected virtual void Sync(string stringConf)
    {
        AvatarConfiguration conf = JsonConvert.DeserializeObject<AvatarConfiguration>(stringConf);
        AvatarStampToCreate.Add(conf);
        CreateNewAvatar(conf.AvatarUrl);
    }

    public void SyncWithNewPlayer(object sender, EventPlayer args)
    {
        Debug.LogWarning("Un nouveau joueur vient d'arriver => event");
        var conf = new AvatarConfiguration(
                    Url,
                    new GameObjectConfig(MyAvatar.GetPhotonView().ViewID, MyAvatar.transform.position, MyAvatar.transform.rotation),
                    new GameObjectConfig(MyTete.GetPhotonView().ViewID, MyTete.transform.position, MyTete.transform.rotation),
                    new GameObjectConfig(MyBrasDroit.GetPhotonView().ViewID, MyBrasDroit.transform.position, MyBrasDroit.transform.rotation),
                    new GameObjectConfig(MyBrasGauche.GetPhotonView().ViewID, MyBrasGauche.transform.position, MyBrasGauche.transform.rotation)
        );
        photonView.RPC("Sync", args.player, JsonConvert.SerializeObject(conf));
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
    public string AvatarUrl;
    public GameObjectConfig Avatar;
    public GameObjectConfig Tete;
    public GameObjectConfig MainDroite;
    public GameObjectConfig MainGauche;

    public AvatarConfiguration(string AvatarUrl, GameObjectConfig Avatar, GameObjectConfig Tete, GameObjectConfig MainDroite, GameObjectConfig MainGauche)
    {
        this.AvatarUrl = AvatarUrl;
        this.Avatar = Avatar;
        this.Tete = Tete;
        this.MainDroite = MainDroite;
        this.MainGauche = MainGauche;
    }
}

public class GameObjectConfig
{
    public int ViewID;
    public SerializableVector3 Position;
    public SerializableQuaternion Rotation;

    public GameObjectConfig(int ViewID, SerializableVector3 Position, SerializableQuaternion Rotation)
    {
        this.ViewID = ViewID;
        this.Position = Position;
        this.Rotation = Rotation;
    }
}