using UnityEngine;
using Random = UnityEngine.Random;

namespace PlayerScripts
{
    public class PlayerManager : MonoBehaviour
    {
        [SerializeField] private GameObject playerModel;

        [SerializeField] private Transform playerOrientation;

        [SerializeField] PlayerStats playerStats;
        ItemManager _itemManager;

        public static bool CanMove = true;
        public static bool CanLook = true;
        public static bool CanUse = true;
        public static bool CanSwitchWeapons = true;

        void Awake()
        {
            _itemManager = GetComponent<ItemManager>();
        }

        public static void FreezePlayer()
        {
            CanMove = false;
            CanLook = false;
            CanUse = false;
            CanSwitchWeapons = false;
        }
        public static void UnFreezePlayer()
        {
            CanMove = true;
            CanLook = true;
            CanUse = true;
            CanSwitchWeapons = true;
        }

        void OnEnable() => playerStats.DeathEvent += PlayerDeath;
        void OnDisable() => playerStats.DeathEvent -= PlayerDeath;

        void Start() 
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            //CameraParent.transform.position = PlayerViewPoint.position;
            //CameraParent.transform.localRotation = PlayerViewPoint.rotation;

            playerModel.transform.localRotation = playerOrientation.transform.localRotation;
        }

        void PlayerDeath() 
        {
            while(_itemManager.items.Count != 0)
            {
                _itemManager.DropItem(_itemManager.items[0].gameObject, Random.Range(-6, 6), Random.Range(1, 6), Random.Range(-6, 6));
            }
        }
    }
}
