using DG.Tweening;
using PlayerScripts;
using UnityEngine;

namespace Interactable.Items.Weapons
{
    public abstract class Gun : Item
    {
        [HideInInspector]public Transform camTrans;
        [SerializeField] protected Transform containerTrans;
        [SerializeField] protected Transform shootPoint;
        [Space(5)]
        [SerializeField] protected ParticleSystem shootingParticles;
        [Space]
        [SerializeField] protected GameObject bulletPrefab;
        [Space]
        [SerializeField] protected Animator anim;
        private static readonly int ReloadAnimID = Animator.StringToHash("Reload");
        [Space(5)]

        [Header("Weapon positions")]
        [SerializeField] protected Vector3 aimedGunPos = new Vector3(0, 0, 0);
        protected bool IsAiming = true;

        [Header("Sounds")] 
        [SerializeField] protected string GunSoundsName = "Pistol";
        public override string PickDropSoundName => "Pick&Drop_Gun";
        
        
        protected bool IsRealoading => anim.GetCurrentAnimatorStateInfo(0).IsName("Reload");

        private void Update() =>
            transform.localRotation = camTrans.localRotation;
        
        public abstract override void Use();
        protected abstract void Shoot();

        public virtual void Reload()
        {
            if (!IsRealoading && PlayerManager.CanUse)
                anim.SetBool(ReloadAnimID, true);
        }

        public virtual void StopReload()
        {
            if (anim.GetBool(ReloadAnimID))
                anim.SetBool(ReloadAnimID, false);
        }

        public virtual void Aim(bool shouldAim)
        {
            if (shouldAim && !IsAiming && !IsRealoading) 
            {
                containerTrans.DOLocalMove(aimedGunPos, ((GunInfo)itemInfo).aimingSpeed);
                IsAiming = true;
                PlayerManager.WSAB.isAiming = true;
                FModAudioManager.PlayGunSound(GunSoundsName, GunSoundType.Aim, shootPoint.position);
            }

            if (!shouldAim && IsAiming || IsRealoading && IsAiming) 
            {
                containerTrans.DOLocalMove(relaxedPos, ((GunInfo)itemInfo).aimingSpeed);
                IsAiming = false;
                PlayerManager.WSAB.isAiming = false;
                FModAudioManager.PlayGunSound(GunSoundsName, GunSoundType.Aim, shootPoint.position);
            }
        }

        public virtual void OnMagIn()
        {
            FModAudioManager.PlayGunSound(GunSoundsName, GunSoundType.MagIn, shootPoint.position);
        }
        public virtual void OnMagOut()
        {
            FModAudioManager.PlayGunSound(GunSoundsName, GunSoundType.MagOut, shootPoint.position);
        }
        public virtual void OnCock() // funny meme share on reddit big upvotes funny fedora people yes
        {
            FModAudioManager.PlayGunSound(GunSoundsName, GunSoundType.Cock, shootPoint.position);
        }
    }
}
