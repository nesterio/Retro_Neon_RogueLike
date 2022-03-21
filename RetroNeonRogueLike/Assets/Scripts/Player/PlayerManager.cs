using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private GameObject CameraParent;
    [SerializeField] private Transform PlayerViewPoint;

    [SerializeField] private GameObject PlayerModel;

    [SerializeField] private Transform PlayerOrientation;

    [SerializeField] PlayerStats PS;
    ItemManager IM;

    [SerializeField] GameObject UIObject;

    PhotonPlayerManager photonPM;
    PhotonView PV;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        IM = GetComponent<ItemManager>();
    }

    void OnEnable() => PS.deathEvent += playerDeath;
    void OnDisable() => PS.deathEvent -= playerDeath;

    void Start() 
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (!PV.IsMine) 
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(GetComponentInChildren<Rigidbody>());
            Destroy(UIObject);
        }

        photonPM = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PhotonPlayerManager>();

    }

    void Update()
    {
        if (!PV.IsMine)
            return;

        //CameraParent.transform.position = PlayerViewPoint.position;
        //CameraParent.transform.localRotation = PlayerViewPoint.rotation;

        PlayerModel.transform.localRotation = PlayerOrientation.transform.localRotation;
    }

    void playerDeath() 
    {
        while(IM.items.Count != 0)
        {
            IM.DropItem(IM.items[0].gameObject, Random.Range(-6, 6), Random.Range(1, 6), Random.Range(-6, 6));
        }
        photonPM.DestroyPlayer();
    }
}
