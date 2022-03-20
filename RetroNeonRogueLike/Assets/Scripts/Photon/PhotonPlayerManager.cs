using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class PhotonPlayerManager : MonoBehaviour
{
    PhotonView PV;

    GameObject playerObj;

    [SerializeField] Vector3[] SpawnPoints;

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

        int temp = Random.Range(0, SpawnPoints.Length);


        playerObj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), SpawnPoints[temp], Quaternion.identity, 0, new object[] { PV.ViewID } );

    }

    public void DestroyPlayer() 
    {
        PhotonNetwork.Destroy(playerObj);
        CreateController();
    } 
}
