using UnityEngine;

namespace Interactable.Items
{
    public abstract class Item : Interactable
    {
        public ItemInfo itemInfo;
        public GameObject itemGameObject;
        [SerializeField] private Collider _collider;
        [SerializeField] private Rigidbody _rigidbody;
        [Space]
        [SerializeField] internal Vector3 relaxedPos;
        [Space]
        public bool isPickable = true;

        public virtual void OnPickUp(Transform parent)
        {
            _collider.enabled = false;
            _rigidbody.isKinematic = true;
            isPickable = false;
            transform.SetParent(parent);
        }

        public virtual void OnDrop()
        {
            _collider.enabled = true;
            _rigidbody.isKinematic = false;
            isPickable = true;
            transform.SetParent(null);
        }
    }
}
