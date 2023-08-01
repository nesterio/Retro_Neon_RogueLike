using PlayerScripts;
using UnityEngine;

namespace Interactable.Items.Weapons
{
    public class Semi_Automatic_Gun : Gun
    {
        [SerializeField] GunShutter GS;
        [SerializeField] GunRecoil GR;

        float ShotSpeed => ((GunInfo)itemInfo).shotSpeed;
        float _timeToShoot;

        int MagCapacity => ((GunInfo)itemInfo).magCapacity;
        public int bulletsInMag;
        
        [Space]

        [SerializeField] bool hasCustomShutter;

        void Awake() 
        {
            if (GS != null && !hasCustomShutter)
                GS.shutterSpeed = ShotSpeed;

            bulletsInMag = MagCapacity;

            _timeToShoot = ShotSpeed; 
        }

        void Update() 
        {
            if (_timeToShoot > 0)
                _timeToShoot -= Time.deltaTime;
        }

        public override void Use() 
        {
            if(!PlayerManager.CanUse || IsRealoading)
                return;

            if (_timeToShoot <= 0)
            {
                if (bulletsInMag > 0) 
                    Shoot();
                else
                    FModAudioManager.PlayGunSound(GunSoundsName, GunSoundType.EmptyShot, shootPoint.position);
                
                _timeToShoot = ShotSpeed;
            }

        }
        
        public override void StopReload() 
        {
            base.StopReload();

            bulletsInMag = MagCapacity;
        }

        protected override void Shoot() 
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

            bulletsInMag--;

            RaycastHit hit;

            if (Physics.Raycast(camTrans.position, camTrans.forward, out hit, Mathf.Infinity)) 
            {
                GameObject tempBullet = Instantiate(bulletPrefab, shootPoint.position, camTrans.rotation);
                tempBullet.GetComponent<BulletScript>().Initialize(hit.point, hit.normal, ((GunInfo)itemInfo).damage);
            }
        }

    }
}
