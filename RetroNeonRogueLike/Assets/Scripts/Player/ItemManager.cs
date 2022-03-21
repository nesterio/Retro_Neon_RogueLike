using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ItemManager : MonoBehaviourPunCallbacks
{
    PhotonView PV;

    [SerializeField] InputManager IM;
    [SerializeField] CameraRecoil CR;
    [SerializeField] Transform CameraParentTrans;
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
    List<Item> items = new List<Item>();

    float dropTimer;

    int itemIndex = 1;

    bool droppingItem;
    bool pickingUp;

    delegate void pickedUpActions();

    void Awake()
    {
        PV = GetComponent<PhotonView>();

        if(PV.IsMine)
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

        Debug.Log(items.Count);
        Debug.Log(itemIndex);


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
            EquipItem(itemIndex + 1);
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f && items.Count > 1)
            EquipItem(itemIndex - 1);


        if (IM.shooting)
            items[itemIndex].Use();

        if (IM.reloading && items[itemIndex] is Gun)
            ((Gun)items[itemIndex]).Reload();

        if (items[itemIndex] is Gun)
            ((Gun)items[itemIndex]).Aim(IM.aiming);


        if (droppingItem && dropTimer > 0)
            dropTimer -= Time.deltaTime;
        else if (droppingItem && dropTimer <= 0)
            droppingItem = false;

        if (!droppingItem && IM.droppingItem)
            DropItem(items[itemIndex].gameObject);

    }

    void EquipItem(int _index) 
    {

        if (items.Count == 0)
            return;


        if (items.Count == 1)
            _index = 0;

        if (_index >= items.Count)
            _index = 0;

        if (_index < 0 && items.Count > 1)
            _index = items.Count - 1;


        if (_index == itemIndex && items.Count != 1)
            return;


        if (items.Count > 0) 
        {
            UnequipHeldItem();

            if (items[itemIndex] is Gun)
                ((Gun)items[itemIndex]).StopReload();
        }
            

        itemIndex = _index;

        items[itemIndex].itemGameObject.SetActive(true);

        if (PV.IsMine) 
        {
            Hashtable hash = new Hashtable();
            hash.Add("itemIndex", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }

    }

    void UnequipHeldItem()
    {
        if (items.Count > 0)
            items[itemIndex].itemGameObject.SetActive(false);
    }

    void DropItem(GameObject item) 
    {
        dropTimer = itemDropSpd;
        droppingItem = true;

        Item itemScrpt = item.GetComponent<Item>();

        itemScrpt.pickedUp = false;

        item.GetComponent<Rigidbody>().isKinematic = false;
        item.GetComponent<Collider>().enabled = true;

        item.transform.parent = null;

        item.GetComponent<Rigidbody>().AddForce(CameraParentTrans.up * itemDroppingForceUpward + CameraParentTrans.forward * itemDroppingForceForward, ForceMode.Impulse);

        itemScrpt.pickable = true;

        if (items.Count > 1)
            EquipItem(itemIndex -1);
        else if (items.Count == 1)
            EquipItem(0);

        items.Remove(itemScrpt);
        itemScrpt.itemGameObject.SetActive(true);


        if (items.Count == 0)
            itemIndex = 0;
        else if (itemIndex > 0)
            itemIndex--;
        else
            itemIndex = items.Count - 1;

    }

    void ItemDetection() 
    {
        RaycastHit hit;
        if (Physics.Raycast(CameraParentTrans.position, CameraParentTrans.forward, out hit, itemPickupRange))
        {
            GameObject hitObj = hit.collider.gameObject;

            if (hitObj.CompareTag("Item"))
            {
                Item item = hitObj.GetComponent<Item>();

                if (item.pickable && !item.pickedUp && IM.interacting && !pickingUp)
                    PickUpItem(hitObj);
            }

        }
    }

    void PickUpItem(GameObject item) 
    {
        pickingUp = true;

        UnequipHeldItem();

        Item itemScrpt = item.GetComponent<Item>();

        itemScrpt.pickable = false;

        item.GetComponent<Rigidbody>().isKinematic = true;
        item.GetComponent<Collider>().enabled = false;

        if (itemScrpt is Gun) 
        {
            item.GetComponent<Gun>().camTrans = CameraParentTrans;
            item.GetComponent<Gun>().CR = CR;
        }

        item.transform.SetParent(itemHolder);

        pickedUpActions actions;
        actions = () => itemScrpt.pickedUp = true;
        actions += () => pickingUp = false;
        actions += () => items.Add(itemScrpt);
        actions += () => EquipItem(items.Count -1);

        item.transform.DOLocalRotate(Vector3.zero, itemPickupSpd);
        item.transform.DOLocalMove(Vector3.zero, itemPickupSpd, false).OnComplete( () => actions());
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) 
    {
        if (!PV.IsMine && targetPlayer == PV.Owner && items.Count > 0) 
        {
            EquipItem((int)changedProps["itemIndex"]);
        }
    }
}
