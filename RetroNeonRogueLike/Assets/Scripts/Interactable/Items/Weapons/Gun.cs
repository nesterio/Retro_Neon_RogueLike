using UnityEngine;

namespace Interactable.Items.Weapons
{
    public abstract class Gun : Item
    {
        [HideInInspector]public Transform camTrans;
        [SerializeField] protected Transform shootPoint;
        [SerializeField] protected Transform containerTrans;
        [SerializeField] protected AudioSource audioSource;
        [Space(5)]
        [SerializeField] protected ParticleSystem shootingParticles;
        [Space]
        [SerializeField] protected GameObject bulletPrefab;
        [Space(5)]

        [Header("Weapon positions")]
        [SerializeField] protected Vector3 aimedGunPos = new Vector3(0, 0, 0);
        protected bool IsAiming = true;
        
        private void Update() =>
            transform.localRotation = camTrans.localRotation;
        
        public abstract override void Use();
        public abstract void Reload();
        public abstract void StopReload();
        public abstract void Aim(bool shouldAim);
    }
}
