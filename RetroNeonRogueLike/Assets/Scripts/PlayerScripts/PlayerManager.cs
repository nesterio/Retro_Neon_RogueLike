using UnityEngine;

namespace PlayerScripts
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerStatistics PlayerStats { get; private set; }
        public static ItemManager ItemsManager { get; private set; }
        public static CameraRecoil CamRecoil { get; private set; }
        public static WeaponSwayAndBob WSAB { get; private set; }

        [SerializeField] private GameObject playerModel;
        [SerializeField] private Transform playerOrientation;
        [SerializeField] private Transform cameraParent;
        [SerializeField] private Transform itemHolder;
        [SerializeField] private AudioSource headphones;

        public static bool CanMove = true;
        public static bool CanLook = true;
        public static bool CanUse = true;
        public static bool CanSwitchWeapons = true;

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

        void OnEnable() => PlayerStats.DeathEvent += PlayerDeath;
        void OnDisable() => PlayerStats.DeathEvent -= PlayerDeath;

        void Start() // MOVE THIS SOMEWHERE ELSE??
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        void Awake()
        {
            PlayerStats = GetComponent<PlayerStatistics>();
            ItemsManager = GetComponent<ItemManager>();
            CamRecoil = cameraParent.GetComponent<CameraRecoil>();
            WSAB = itemHolder.GetComponent<WeaponSwayAndBob>();
        }

        void Update()
        {
            //CameraParent.transform.position = PlayerViewPoint.position;
            //CameraParent.transform.localRotation = PlayerViewPoint.rotation;

            playerModel.transform.localRotation = playerOrientation.transform.localRotation;
        }

        void PlayerDeath() 
        {
            RoomManager.Instance.SpawnPlayer(true);
        }
    }
}
