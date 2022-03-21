using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public ItemInfo itemInfo;
    public GameObject itemGameObject;

    [Space]

    [SerializeField] internal Vector3 relaxedPos;

    [Space]

    public bool pickable = true;
    public bool pickedUp = false;

    public abstract void Use();
}
