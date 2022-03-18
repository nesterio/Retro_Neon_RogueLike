using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;

    [SerializeField] int Lvl1ID;

    void Awake() 
    {
        if (Instance) 
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    public override void OnEnable() 
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable() 
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode) 
    {
        if (scene.buildIndex == Lvl1ID) 
        {
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonPlayerManager"), Vector3.zero, Quaternion.identity);
        }
    }
}
