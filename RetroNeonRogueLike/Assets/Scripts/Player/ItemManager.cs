using System.Collections.Generic;
using DG.Tweening;
using Photon.Pun;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using IM = InputManager;

namespace Player
{
    public class ItemManager : MonoBehaviourPunCallbacks
    {
        PhotonView PV;

        [SerializeField] CameraRecoil cameraRecoil;
        PlayerStats _playerStats;
        [SerializeField] Transform cameraParentTrans;
        [SerializeField] Transform itemHolder;
        [Space(10)]
        [SerializeField] float itemDroppingForceUpward = 2f;
        [SerializeField] float itemDroppingForceForward = 2f;
        [Space(3)]
        [SerializeField] float itemDropSpd = 1f;
        [SerializeField] float itemPickupSpd = 0.5f;
        [Space(3)]
        [SerializeField] float itemPickupRange = 3f;
        [Space(10)]
        [SerializeField] Item[] startItems;
        public List<Item> items = new List<Item>();

        float _dropTimer;

        int _itemIndex = 0;

        bool _droppingItem;
        bool _pickingUp;

        delegate void pickedUpActions();

        void Awake()
        {
            PV = GetComponent<PhotonView>();

            if(PV.IsMine && startItems != null)
                foreach (Item item in startItems) 
                {
                    items.Add(item);
                    item.pickable = false;
                    item.pickedUp = true;
                }

            if (PV.IsMine && items.Count > 0)
                EquipItem(0);
        }

        void Update() 
        {
            if (!PV.IsMine)
                return;

            ItemDetection();


            if (items.Count == 0)
                return;

            //Debug.Log(itemIndex);


            if (items.Count > 1)
                for(int i = 0; i < items.Count; i++)
                {
                    if (Input.GetKeyDown((i + 1).ToString())) 
                    {
                        EquipItem(i);
                        break;
                    }
                }

            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f && items.Count > 1)
                EquipItem(_itemIndex + 1);
            else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f && items.Count > 1)
                EquipItem(_itemIndex - 1);


            if (IM.Shooting)
                items[_itemIndex].Use();

            if (IM.Reloading && items[_itemIndex] is Gun)
                ((Gun)items[_itemIndex]).Reload();

            if (items[_itemIndex] is Gun)
                ((Gun)items[_itemIndex]).Aim(IM.Aiming);


            if (_droppingItem && _dropTimer > 0)
                _dropTimer -= Time.deltaTime;
            else if (_droppingItem && _dropTimer <= 0)
                _droppingItem = false;

            if (!_droppingItem && IM.DroppingItem)
                DropItem(items[_itemIndex].gameObject, itemDroppingForceUpward, itemDroppingForceForward, 0);

        }

        void EquipItem(int index) 
        {

            if (items.Count == 0)
                return;


            if (items.Count == 1)
                index = 0;

            if (index >= items.Count)
                index = 0;

            if (index < 0 && items.Count > 1)
                index = items.Count - 1;


            if (index == _itemIndex && items.Count != 1)
                return;


            if (items.Count > 0) 
            {
                UnequipHeldItem();

                if (items[_itemIndex] is Gun)
                    ((Gun)items[_itemIndex]).StopReload();
            }
            

            _itemIndex = index;

            items[_itemIndex].itemGameObject.SetActive(true);

            if (PV.IsMine) 
            {
                Hashtable hash = new Hashtable();
                hash.Add("itemIndex", _itemIndex);
                PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
            }

        }

        void UnequipHeldItem()
        {
            if (items.Count > 0)
                items[_itemIndex].itemGameObject.SetActive(false);
        }

        public void DropItem(GameObject item, float dropForceY, float dropForceZ, float dropForceX) 
        {
            _dropTimer = itemDropSpd;
            _droppingItem = true;

            PV.RPC("RPC_DropItem", RpcTarget.AllBufferedViaServer, item.GetComponent<PhotonView>().ViewID, dropForceY, dropForceZ, dropForceX);

        
            if (items.Count > 1)
                EquipItem(_itemIndex -1);
            else if (items.Count == 1)
                EquipItem(0);

            items.Remove(item.GetComponent<Item>());
            item.GetComponent<Item>().itemGameObject.SetActive(true);


            if (items.Count == 0)
                _itemIndex = 0;
            else if (_itemIndex > 0)
                _itemIndex--;
            else
                _itemIndex = items.Count - 1;

        }

        [PunRPC]
        void RPC_DropItem(int VID, float dropForceY, float dropForceZ, float dropForceX) 
        {
            GameObject item = PhotonView.Find(VID).gameObject;

            Item itemScrpt = item.GetComponent<Item>();

            itemScrpt.pickedUp = false;

            item.GetComponent<Rigidbody>().isKinematic = false;
            item.GetComponent<Collider>().enabled = true;

            item.transform.parent = null;

            itemScrpt.pickable = true;

            item.GetComponent<Rigidbody>().AddForce(cameraParentTrans.up * dropForceY + cameraParentTrans.forward * dropForceZ + cameraParentTrans.right * dropForceX, ForceMode.Impulse);
        }

        void ItemDetection() 
        {
            if (Physics.Raycast(cameraParentTrans.position, cameraParentTrans.forward, out var hit, itemPickupRange))
            {
                GameObject hitObj = hit.collider.gameObject;

                if (hitObj.CompareTag("Item"))
                {
                    Item item = hitObj.GetComponent<Item>();

                    if (item.pickable && !item.pickedUp && IM.Interacting && !_pickingUp && items.Count < _playerStats.maxItems)
                        PickUpItem(hitObj);
                }

            }
        }

        void PickUpItem(GameObject item) 
        { 
            _pickingUp = true;

            UnequipHeldItem();

            PV.RPC("RPC_PickUpItem", RpcTarget.AllBufferedViaServer, item.GetComponent<PhotonView>().ViewID);
        }

        [PunRPC]
        void RPC_PickUpItem(int VID)
        {
            GameObject item = PhotonView.Find(VID).gameObject;

            item.GetComponent<PhotonView>().TransferOwnership(PV.Owner);

            Item itemScrpt = item.GetComponent<Item>();

            itemScrpt.pickable = false;

            item.GetComponent<Rigidbody>().isKinematic = true;
            item.GetComponent<Collider>().enabled = false;

            if (itemScrpt is Gun)
            {
                item.GetComponent<Gun>().camTrans = cameraParentTrans;
                item.GetComponent<Gun>().CR = cameraRecoil;
                item.GetComponent<Gun>().WSAB = itemHolder.GetComponent<WeaponSwayAndBob>();
                item.GetComponent<Gun>().WSAB = itemHolder.GetComponent<WeaponSwayAndBob>();
            }

            item.transform.SetParent(itemHolder);

            pickedUpActions actions;
            actions = () => itemScrpt.pickedUp = true;
            actions += () => _pickingUp = false;
            actions += () => items.Add(itemScrpt);
            actions += () => EquipItem(items.Count - 1);

            item.transform.DOLocalRotate(Vector3.zero, itemPickupSpd);
            item.transform.DOLocalMove(Vector3.zero, itemPickupSpd, false).OnComplete(() => actions());
        }

        public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps) 
        {
            if (!PV.IsMine && targetPlayer == PV.Owner && items.Count > 0) 
            {
                EquipItem((int)changedProps["itemIndex"]);
            }
        }
    }
}
