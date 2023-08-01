using UnityEngine;

namespace Interactable.Items
{
    public abstract class Item : Interactable
    {
        [field:SerializeField]public ItemInfo itemInfo { get; protected set; }

        [field: SerializeField] private Collider _collider;
        [field: SerializeField] private Rigidbody _rigidbody;
        [Space]
        [SerializeField] protected Vector3 relaxedPos;

        [field:SerializeField] public bool isPickable { get; private set; } = true;
        
        public virtual string PickDropSoundName => "Pick&Drop_Default";

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
