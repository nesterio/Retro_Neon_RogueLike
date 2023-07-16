using System;
using System.Collections.Generic;
using DG.Tweening;
using Interactable.Items;
using Interactable.Items.Weapons;
using UnityEngine;
using inputManagerInfo = InputManagerData;

namespace PlayerScripts
{
    public class ItemManager : MonoBehaviour
    {
        [Header("Object references")]
        [SerializeField] Transform cameraParentTrans;
        [SerializeField] Transform itemHolder;
        [Space(10)]
        [SerializeField] float itemDroppingForceUpward = 2.5f;
        [SerializeField] float itemDroppingForceForward = 3.5f;
        [Space(3)]
        [SerializeField] float itemDropSpd = 1f;
        [SerializeField] float itemPickupSpd = 0.5f;
        [Space(10)]
        [SerializeField] Item[] startItems;
        public List<Item> items = new List<Item>();
        
        float _dropTimer;
        int _itemIndex = 0;
        
        private PlayerStatistics _playerStats;

        public bool CanPickupItem => _playerStats != null && items.Count < _playerStats.MaxItems;

        bool _droppingItem;
        public bool PickingUpItem { get; private set; }

        delegate void PickedUpActions();

        void Start()
        {
            _playerStats = PlayerManager.PlayerStats;
            
            if(startItems != null)
                foreach (Item item in startItems)
                    PickUpItem(item.gameObject);

            if (items.Count > 0)
                EquipItem(0);
        }

        void Update() 
        {
            if (items.Count == 0)
                return;

            ////// TODO: MOVE THIS TO INPUT MANAGER !!! //////

            if (PlayerManager.CanSwitchWeapons)
            {
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
            }

            ////// ---------------------------------- //////


            var item = items[_itemIndex];
            if (inputManagerInfo.Shooting && PlayerManager.CanUse && item.isUsable || item.isUsable && !item.awaitInput)
                item.Use();

            if (item is Gun gun)
            {
                if(inputManagerInfo.Reloading)
                    gun.Reload();
                
                gun.Aim(inputManagerInfo.Aiming);
            }

            if (_droppingItem && _dropTimer > 0)
                _dropTimer -= Time.deltaTime;
            else if (_droppingItem && _dropTimer <= 0)
                _droppingItem = false;

            if (!_droppingItem && inputManagerInfo.DroppingItem)
                DropItem(item.gameObject, itemDroppingForceUpward, itemDroppingForceForward, 0);

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
        }

        public void UnequipHeldItem()
        {
            if (items.Count > 0)
                items[_itemIndex].itemGameObject.SetActive(false);
        }

        public void DropItem(GameObject itemObj, float dropForceY, float dropForceZ, float dropForceX) 
        {
            _dropTimer = itemDropSpd;
            _droppingItem = true;

            if (items.Count > 1)
                EquipItem(_itemIndex -1);
            else if (items.Count == 1)
                EquipItem(0);

            var item = itemObj.GetComponent<Item>();
            items.Remove(item);
            item.itemGameObject.SetActive(true); // Is "item.itemGameObject" instead of item.gameObject required? // IS THIS REQUIRED AT ALL???
            
            if (items.Count == 0)
                _itemIndex = 0;
            else if (_itemIndex > 0)
                _itemIndex--;
            else
                _itemIndex = items.Count - 1;

            item.OnDrop();
            item.isUsable = false;

            itemObj.GetComponent<Rigidbody>().AddForce(cameraParentTrans.up * dropForceY + cameraParentTrans.forward * dropForceZ + cameraParentTrans.right * dropForceX, ForceMode.Impulse);
        }
        
        public void PickUpItem(GameObject itemObj) 
        { 
            PickingUpItem = true;

            UnequipHeldItem();

            Item item = itemObj.GetComponent<Item>();

            if (item is Gun gun)
                gun.camTrans = cameraParentTrans;
            
            item.OnPickUp(itemHolder);

            Action action = () =>
            {
                PickingUpItem = false;
                items.Add(item);
                EquipItem(items.Count - 1);
                item.isUsable = true;
            };

            itemObj.transform.DOLocalRotate(Vector3.zero, itemPickupSpd);
            itemObj.transform.DOLocalMove(Vector3.zero, itemPickupSpd, false).OnComplete(()=>action.Invoke());
        }
    }
}
