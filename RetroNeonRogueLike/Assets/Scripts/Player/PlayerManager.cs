using Photon.Pun;
using UnityEngine;

namespace Player
{
    public class PlayerManager : MonoBehaviour
    {
        [SerializeField] private GameObject playerModel;

        [SerializeField] private Transform playerOrientation;

        [SerializeField] PlayerStats playerStats;
        ItemManager _itemManager;

        [SerializeField] GameObject uiObject;

        PhotonPlayerManager _photonPlayerManager;
        PhotonView PV;

        void Awake()
        {
            PV = GetComponent<PhotonView>();
            _itemManager = GetComponent<ItemManager>();
        }

        void OnEnable() => playerStats.deathEvent += PlayerDeath;
        void OnDisable() => playerStats.deathEvent -= PlayerDeath;

        void Start() 
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (!PV.IsMine) 
            {
                Destroy(GetComponentInChildren<Camera>().gameObject);
                Destroy(GetComponentInChildren<Rigidbody>());
                Destroy(uiObject);
            }

            _photonPlayerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PhotonPlayerManager>();

        }

        void Update()
        {
            if (!PV.IsMine)
                return;

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
            _photonPlayerManager.DestroyPlayer();
        }
    }
}
