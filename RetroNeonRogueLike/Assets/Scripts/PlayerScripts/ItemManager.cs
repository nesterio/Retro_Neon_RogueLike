using System.Collections.Generic;
using DG.Tweening;
using Interactable.Items;
using Items.Weapons;
using UnityEngine;
using IM = InputManagerData;

namespace PlayerScripts
{
    public class ItemManager : MonoBehaviour
    {
        [SerializeField] CameraRecoil cameraRecoil;
        [SerializeField] Transform cameraParentTrans;
        [SerializeField] Transform itemHolder;
        [SerializeField] private PlayerStats playerStats;
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

        public bool CanPickupItem => items.Count < playerStats.maxItems;

        bool _droppingItem;
        public bool PickingUpItem { get; private set; }

        delegate void PickedUpActions();

        void Awake()
        {
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

            //Debug.Log(itemIndex);

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
            if (IM.Shooting && PlayerManager.CanUse && item.isUsable || item.isUsable && !item.awaitInput)
                item.Use();

            if (item is Gun gun)
            {
                if(IM.Reloading)
                    gun.Reload();
                
                gun.Aim(IM.Aiming);
            }

            if (_droppingItem && _dropTimer > 0)
                _dropTimer -= Time.deltaTime;
            else if (_droppingItem && _dropTimer <= 0)
                _droppingItem = false;

            if (!_droppingItem && IM.DroppingItem)
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

        public void DropItem(GameObject item, float dropForceY, float dropForceZ, float dropForceX) 
        {
            _dropTimer = itemDropSpd;
            _droppingItem = true;

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
            
            Item itemScript = item.GetComponent<Item>();

            itemScript.isPickable = false;

            item.GetComponent<Rigidbody>().isKinematic = false;
            item.GetComponent<Collider>().enabled = true;

            item.transform.parent = null;

            itemScript.isUsable = false;

            item.GetComponent<Rigidbody>().AddForce(cameraParentTrans.up * dropForceY + cameraParentTrans.forward * dropForceZ + cameraParentTrans.right * dropForceX, ForceMode.Impulse);
        }
        
        public void PickUpItem(GameObject item) 
        { 
            PickingUpItem = true;

            UnequipHeldItem();

            Item itemScrpt = item.GetComponent<Item>();

            item.GetComponent<Rigidbody>().isKinematic = true;
            item.GetComponent<Collider>().enabled = false;

            if (itemScrpt is Gun)
            {
                var wsab = itemHolder.GetComponent<WeaponSwayAndBob>();
                item.GetComponent<Gun>().camTrans = cameraParentTrans;
                item.GetComponent<Gun>().CR = cameraRecoil;
                item.GetComponent<Gun>().WSAB = wsab;
            }

            item.transform.SetParent(itemHolder);

            PickedUpActions actions;
            actions = () => itemScrpt.isPickable = true;
            actions += () => PickingUpItem = false;
            actions += () => items.Add(itemScrpt);
            actions += () => EquipItem(items.Count - 1);
            actions += () => itemScrpt.isUsable = true;

            item.transform.DOLocalRotate(Vector3.zero, itemPickupSpd);
            item.transform.DOLocalMove(Vector3.zero, itemPickupSpd, false).OnComplete(() => actions());
        }
    }
}
