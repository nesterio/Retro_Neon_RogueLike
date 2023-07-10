using UnityEngine;
using UnityEngine.Serialization;

namespace Interactable.Items
{
    public abstract class Item : Interactable
    {
        public ItemInfo itemInfo;
        public GameObject itemGameObject;

        [Space]

        [SerializeField] internal Vector3 relaxedPos;

        [FormerlySerializedAs("pickedUp")] [Space]
    
        public bool isPickable = true;
    }
}
