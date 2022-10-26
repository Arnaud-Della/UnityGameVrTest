using Newtonsoft.Json;
using Photon.Pun;
using ReadyPlayerMe;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using System.Linq;

public class AvatarFactory : MonoBehaviour
{
    public GameObject Casque;
    public GameObject ManetteDroite;
    public GameObject ManetteGauche;
    public RuntimeAnimatorController ControllerAnimator;
    public Avatar SqueletteAvatarAnimator;
    public PhotonView photonView;
    public PhysicMaterial physicMaterial;
    public Vector3 InitPosition;
    public float InitRotation;

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
        PhotonView targetDroitPhotonView = targetDroit.AddComponent<PhotonView>();
        PhotonTransformView targetDroitPhotonTransformView = targetDroit.AddComponent<PhotonTransformView>();
        // Bras Gauche
        PhotonView targetGauchePhotonView = targetGauche.AddComponent<PhotonView>();
        PhotonTransformView targetGauchePhotonTransformView = targetGauche.AddComponent<PhotonTransformView>();


        // Photon Animator View
        PhotonAnimatorView avatarAnimatorView = avatar.AddComponent<PhotonAnimatorView>();

        // Photon Voice View
        PhotonVoiceView photonVoiceView = avatar.AddComponent<PhotonVoiceView>();
        AudioSource audioSource = avatar.AddComponent<AudioSource>();
        Speaker speaker = avatar.AddComponent<Speaker>();

        // Capsule collider + RigideBody => Hit box
        CapsuleCollider capsuleCollider = avatar.AddComponent<CapsuleCollider>();
        Rigidbody rigidbody = avatar.AddComponent<Rigidbody>();

        //////////////////////////// Declaration des variables /////////////////////////////////


        // Avatar
        avatar.name = Guid.NewGuid().ToString();
        avatar.transform.position = InitPosition;
        avatar.transform.rotation = Quaternion.AngleAxis(InitRotation, Vector3.up);
        rigBuilder.layers.Clear();
        rigBuilder.layers.Add(new RigLayer(rigComponent));
        boneRenderer.transforms = GetAllHumanoidBones(animator);


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


        // Photon Animator View Configuration
        avatarAnimatorView.SetParameterSynchronized("Vertical", PhotonAnimatorView.ParameterType.Float, PhotonAnimatorView.SynchronizeType.Continuous);
        avatarAnimatorView.SetParameterSynchronized("Horizontal", PhotonAnimatorView.ParameterType.Float, PhotonAnimatorView.SynchronizeType.Continuous);

        // Main Photon Configuration
        avatarPhotonTransformView.m_SynchronizePosition = true;
        avatarPhotonTransformView.m_SynchronizeRotation = true;
        avatarPhotonTransformView.m_UseLocal = true;
        AvatarPhotonView.ObservedComponents = new List<Component>() { avatarPhotonTransformView, avatarAnimatorView };

        // Recurrence Photon Configuration
        // Tete
        tetePhotonTransformView.m_SynchronizePosition = true;
        tetePhotonTransformView.m_SynchronizeRotation = true;
        tetePhotonTransformView.m_UseLocal = true;
        tetePhotonView.ObservedComponents = new List<Component>() { tetePhotonTransformView };
        // Target Droit
        targetDroitPhotonTransformView.m_SynchronizePosition = true;
        targetDroitPhotonTransformView.m_SynchronizeRotation = true;
        targetDroitPhotonTransformView.m_UseLocal = true;
        targetDroitPhotonView.ObservedComponents = new List<Component>() { targetDroitPhotonTransformView };
        // Target Gauche
        targetGauchePhotonTransformView.m_SynchronizePosition = true;
        targetGauchePhotonTransformView.m_SynchronizeRotation = true;
        targetGauchePhotonTransformView.m_UseLocal = true;
        targetGauchePhotonView.ObservedComponents = new List<Component>() { targetGauchePhotonTransformView };

        // Photon Voice View Configuration
        photonVoiceView.SpeakerInUse = speaker;
        photonVoiceView.UsePrimaryRecorder = true;
        photonVoiceView.SetupDebugSpeaker = true;
        audioSource.loop = true;

        // Hit Box Configuration
        // Capsule Collider
        capsuleCollider.material = physicMaterial;
        capsuleCollider.center = new Vector3(0, 0.84f, 0);
        capsuleCollider.radius = 0.5f;
        capsuleCollider.height = 1.64f;
        capsuleCollider.direction = 1;
        // RigideBody
        rigidbody.mass = 70;
        rigidbody.drag = 0;
        rigidbody.angularDrag = 0.05f;
        rigidbody.useGravity = true;
        rigidbody.isKinematic = false;
        rigidbody.constraints = RigidbodyConstraints.FreezeAll;



        if (IsMyOwnAvatar) // && AvatarStampToCreate.Count == 0
        {
            Debug.LogWarning($"Je cree mon propre Avatar {Url}");

            // Aligne transform du Casque et des Manettes du VR avec l'avatar
            Casque.transform.position = HumanBones[10].transform.position;
            ManetteDroite.transform.position = HumanBones[18].transform.position;
            ManetteGauche.transform.position = HumanBones[17].transform.position;
            Casque.transform.rotation = Quaternion.AngleAxis(InitRotation, Vector3.up);
            ManetteDroite.transform.rotation = HumanBones[18].transform.rotation;
            ManetteGauche.transform.rotation = HumanBones[17].transform.rotation;

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

            // Allocation des ViewID Recurrence
            PhotonNetwork.AllocateViewID(tetePhotonView);
            PhotonNetwork.AllocateViewID(targetDroitPhotonView);
            PhotonNetwork.AllocateViewID(targetGauchePhotonView);

            // Recuperation de la configuration de mon avatar
            MyAvatar = avatar;
            MyTete = TeteContrainte;
            MyBrasDroit = targetDroit;
            MyBrasGauche = targetGauche;
            IsMyOwnAvatar = false;

            Debug.LogWarning("L'Avatar a été correctement creer");
            Debug.LogWarning($"INFO : {AvatarPhotonView.ViewID}, {avatar.transform.position}");

            // Creation de mon Avatar chez les autres joueur
            var conf = new AvatarConfiguration(
                    Url,
                    new GameObjectConfig(AvatarPhotonView.ViewID, MyAvatar.transform.position, MyAvatar.transform.rotation),
                    new GameObjectConfig(tetePhotonView.ViewID, MyTete.transform.position, MyTete.transform.rotation),
                    new GameObjectConfig(targetDroitPhotonView.ViewID, MyBrasDroit.transform.position, MyBrasDroit.transform.rotation),
                    new GameObjectConfig(targetGauchePhotonView.ViewID, MyBrasGauche.transform.position, MyBrasGauche.transform.rotation)
            );
            
            photonView.RPC("Sync", RpcTarget.Others, JsonConvert.SerializeObject(conf));


            // Creation de Tout les avatars adverse deja sur le terrain
            if (AvatarStampToCreate.Count > 0)
            {
                for(int i=0; i<AvatarStampToCreate.Count; i++)
                {
                    CreateNewAvatar(AvatarStampToCreate[i].AvatarUrl);
                }
            }

            // Ajout de l'evenement SyncWithNewPlayer
            Network.OnPlayerEnteredRoomEventHandler += SyncWithNewPlayer;
        }
        else if (AvatarStampToCreate.Count > 0)
        {
            // Recuperation de l'index
            int index = AvatarStampToCreate.FindIndex(x => x.AvatarUrl == args.Url);

            // Attribution du ViewID et de la position à l'avatar adverse 
            AvatarPhotonView.ViewID = AvatarStampToCreate[index].Avatar.ViewID;
            avatar.transform.position = AvatarStampToCreate[index].Avatar.Position;
            avatar.transform.rotation = AvatarStampToCreate[index].Avatar.Rotation;
            // Attribution du ViewID et de la position à la tete adverse
            tetePhotonView.ViewID = AvatarStampToCreate[0].Tete.ViewID;
            TeteContrainte.transform.position = AvatarStampToCreate[index].Tete.Position;
            TeteContrainte.transform.rotation = AvatarStampToCreate[index].Tete.Rotation;
            // Attribution du ViewID et de la position aux mains adverse
            // Target Droit
            targetDroitPhotonView.ViewID = AvatarStampToCreate[index].MainDroite.ViewID;
            targetDroit.transform.position = AvatarStampToCreate[index].MainDroite.Position;
            targetDroit.transform.rotation = AvatarStampToCreate[index].MainDroite.Rotation;
            // Target Gauche
            targetGauchePhotonView.ViewID = AvatarStampToCreate[index].MainGauche.ViewID;
            targetGauche.transform.position = AvatarStampToCreate[index].MainGauche.Position;
            targetGauche.transform.rotation = AvatarStampToCreate[index].MainGauche.Rotation;

            // Suppression des informations de configuration de cette avatar du buffer
            //AvatarStampToCreate.RemoveAt(0);
            AvatarStampToCreate.RemoveAt(index);
        }


        // Finalisation de la Creation de l'avatar
        //animator.avatar = null;
        animator.runtimeAnimatorController = ControllerAnimator;
        rigBuilder.Build();




    }

    [PunRPC]
    protected virtual void Sync(string stringConf)
    {
        AvatarConfiguration conf = JsonConvert.DeserializeObject<AvatarConfiguration>(stringConf);
        AvatarStampToCreate.Add(conf);

        if (!IsMyOwnAvatar)
        {
            CreateNewAvatar(conf.AvatarUrl);
        }
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