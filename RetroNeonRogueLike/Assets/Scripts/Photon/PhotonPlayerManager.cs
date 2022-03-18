using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class PhotonPlayerManager : MonoBehaviour
{
    PhotonView PV;

    GameObject playerObj;

    void Awake() 
    {
        PV = GetComponent<PhotonView>();
    }

    void Start() 
    {
        if (PV.IsMine)
            CreateController();
    }

    void CreateController() 
    {
        playerObj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), new Vector3(0, 2f, 0f), Quaternion.identity, 0, new object[] { PV.ViewID } );
    }

    public void DestroyPlayer() 
    {
        PhotonNetwork.Destroy(playerObj);
        CreateController();
    } 
}
