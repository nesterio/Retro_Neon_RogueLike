using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    [SerializeField] int Lvl1ID;
    
    [SerializeField]GameObject playerPrefab;
    private GameObject playerObj;

    [SerializeField] Vector3[] SpawnPoints;

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

    public void OnEnable() 
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void OnDisable() 
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode) 
    {
        if (scene.buildIndex == Lvl1ID) 
        {
            SpawnPlayer(false);
        }
    }
    
    public void SpawnPlayer(bool respawn) 
    {
        if (playerObj != null)
        {
            if(respawn)
                Destroy(playerObj);
            else
                return;
        }

        int temp = Random.Range(0, SpawnPoints.Length);
        playerObj = Instantiate(playerPrefab, SpawnPoints[temp], Quaternion.identity);
    }
}
