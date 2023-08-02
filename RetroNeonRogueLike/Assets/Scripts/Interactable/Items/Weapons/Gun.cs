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
        [SerializeField] GunRecoil GR;
        [SerializeField] GunShutter GS;
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
        
        protected float ShotSpeed => ((GunInfo)itemInfo).shotSpeed;
        protected float TimeToShoot;
        protected bool IsRealoading => anim.GetCurrentAnimatorStateInfo(0).IsName("Reload");


        private void Start() => Initialize();
        protected virtual void Initialize() => TimeToShoot = ShotSpeed;

        void Update() 
        {
            if (TimeToShoot > 0)
                TimeToShoot -= Time.deltaTime;
        }

        public abstract override void Use();

        protected virtual void Shoot()
        {
            // Camera recoil
            (float,float,float) recoils = IsAiming ? (((GunInfo)itemInfo).recoilAimedX, ((GunInfo)itemInfo).recoilAimedY, ((GunInfo)itemInfo).recoilAimedZ)
                : (((GunInfo)itemInfo).recoilX, ((GunInfo)itemInfo).recoilY, ((GunInfo)itemInfo).recoilZ);
            PlayerManager.CamRecoil.RecoilFire(recoils.Item1, recoils.Item2, recoils.Item3);

            // Gun recoil
            GR.RecoilFire(IsAiming);

            // Gun shutter
            GS.PlayShutter();

            // Particles
            if (shootingParticles != null)
            {
                if(shootingParticles.isPlaying)
                    shootingParticles.Stop();
                
                shootingParticles.Play();
            }
            
            // Sound
            FModAudioManager.PlayGunSound(GunSoundsName, GunSoundType.Shot, shootPoint.position);
            
            RaycastHit hit;

            if (Physics.Raycast(camTrans.position, camTrans.forward, out hit, Mathf.Infinity)) 
            {
                GameObject tempBullet = Instantiate(bulletPrefab, shootPoint.position, camTrans.rotation);
                tempBullet.GetComponent<BulletScript>().Initialize(hit.point, hit.normal, ((GunInfo)itemInfo).damage);
            }
        }

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
    }
}
