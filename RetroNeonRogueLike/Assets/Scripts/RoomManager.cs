using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    [SerializeField] int Lvl1ID;
    
    [SerializeField]GameObject playerPrefab;

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
            CreatePlayer();
        }
    }
    
    void CreatePlayer() 
    {
        int temp = Random.Range(0, SpawnPoints.Length);
        Instantiate(playerPrefab, SpawnPoints[temp], Quaternion.identity);
    }
}
