using UnityEngine;

namespace Interactable.Items
{
    public abstract class Item : Interactable
    {
        public ItemInfo itemInfo;
        public GameObject itemGameObject;

        [Space]

        [SerializeField] internal Vector3 relaxedPos;

        [Space]
    
        public bool pickedUp = false;
    }
}
