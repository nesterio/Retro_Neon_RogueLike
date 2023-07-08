using System;
using PlayerScripts;
using UnityEngine;

namespace Items.Weapons
{
    public abstract class Gun : Item
    {

        public Transform camTrans;
        public CameraRecoil CR;
        public WeaponSwayAndBob WSAB;
        [SerializeField] internal Transform shootPoint;
        [SerializeField] internal Transform containerTrans;
        [Space(5)]
        [SerializeField] internal ParticleSystem shootingParticles;
        [Space]
        [SerializeField] internal GameObject bulletPrefab;
        [Space(5)]
        [SerializeField] internal GameObject bulletImpactPrefab;
        [Space(5)]

        [Header("Weapon positions")]
        [SerializeField] internal Vector3 aimedGunPos = new Vector3(0, 0, 0);

        internal bool isAiming = true;
        
        private void Update()
        {
            transform.localRotation = camTrans.localRotation;
        }

        public abstract override void Use();
        public abstract void Reload();
        public abstract void StopReload();
        public abstract void Aim(bool shouldAim);
    }
}
