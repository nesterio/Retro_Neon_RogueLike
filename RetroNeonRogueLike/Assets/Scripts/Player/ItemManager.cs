using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ItemManager : MonoBehaviourPunCallbacks
{
    PhotonView PV;

    [SerializeField] InputManager IM;

    [SerializeField] Item[] items;

    int itemIndex = 1;

    int previousItemIndex 
    {
        get 
        {
            if (itemIndex == 0)
                return items.Length - 1;
            else
                return itemIndex - 1;      
        } 
    }

    void Awake()
    {
        PV = GetComponent<PhotonView>();

        if (PV.IsMine)
            EquipItem(0);
    }

    void Update() 
    {
        if (!PV.IsMine)
            return;

        for(int i = 0; i < items.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString())) 
            {
                EquipItem(i);
                break;
            }
        }

        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
            EquipItem(itemIndex + 1);
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
            EquipItem(previousItemIndex);

        if (IM.shooting)
            items[itemIndex].Use();

        if (IM.reloading && items[itemIndex] is Gun)
            ((Gun)items[itemIndex]).Reload();

        if (items[itemIndex] is Gun)
            ((Gun)items[itemIndex]).Aim(IM.aiming);

    }

    void EquipItem(int _index) 
    {
        if (_index == itemIndex)
            return;

        if (_index >= items.Length)
            _index = 0;

        itemIndex = _index;

        items[previousItemIndex].itemGameObject.SetActive(false);
        items[itemIndex].itemGameObject.SetActive(true);

        if (PV.IsMine) 
        {
            Hashtable hash = new Hashtable();
            hash.Add("itemIndex", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }

    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) 
    {
        if (!PV.IsMine && targetPlayer == PV.Owner) 
        {
            EquipItem((int)changedProps["itemIndex"]);
        }
    }
}
