using DG.Tweening;
using PlayerScripts;
using UnityEngine;

namespace Interactable.Items.Weapons
{
    public class Semi_Automatic_Gun : Gun
    {
        [SerializeField] Animator anim;
        [SerializeField] GunShutter GS;
        [SerializeField] GunRecoil GR;

        float ShotSpeed => ((GunInfo)itemInfo).shotSpeed;
        float _timeToShoot;

        int MagCapacity => ((GunInfo)itemInfo).magCapacity;
        public int bulletsInMag;

        bool IsRealoading => anim.GetCurrentAnimatorStateInfo(0).IsName("Reload");

        [Space]

        [SerializeField] bool hasCustomShutter;

        private static readonly int Reload1 = Animator.StringToHash("Reload");

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
            if(!PlayerManager.CanUse)
                return;
            
            if (bulletsInMag > 0 && !IsRealoading && _timeToShoot <= 0)
                Shoot();
        }

        public override void Reload() 
        {
            if (!IsRealoading && PlayerManager.CanUse)
                anim.SetBool(Reload1, true);
        }
        public override void StopReload() 
        {
            if (IsRealoading)
                anim.SetBool(Reload1, false);
        }

        public override void Aim(bool shouldAim) 
        {
            if (shouldAim && !IsAiming && !IsRealoading) 
            {
                containerTrans.DOLocalMove(aimedGunPos, ((GunInfo)itemInfo).aimingSpeed, false);
                IsAiming = true;
                PlayerManager.WSAB.isAiming = true;

            }

            if (!shouldAim && IsAiming || IsRealoading) 
            {
                containerTrans.DOLocalMove(relaxedPos, ((GunInfo)itemInfo).aimingSpeed, false);
                IsAiming = false;
                PlayerManager.WSAB.isAiming = false;
            }
        }

        void Shoot() 
        {
            if(IsAiming)
                PlayerManager.CamRecoil.RecoilFire(((GunInfo)itemInfo).recoilAimedX, ((GunInfo)itemInfo).recoilAimedY, ((GunInfo)itemInfo).recoilAimedZ);
            else
                PlayerManager.CamRecoil.RecoilFire( ((GunInfo)itemInfo).recoilX , ((GunInfo)itemInfo).recoilY , ((GunInfo)itemInfo).recoilZ);

            GR.RecoilFire(IsAiming);

            GS.PlayShutter();

            if (shootingParticles != null)
            {
                if(shootingParticles.isPlaying)
                    shootingParticles.Stop();
                
                shootingParticles.Play();
            }

            bulletsInMag--;
            _timeToShoot = ShotSpeed;

            RaycastHit hit;

            if (Physics.Raycast(camTrans.position, camTrans.forward, out hit, Mathf.Infinity)) 
            {
                GameObject tempBullet = Instantiate(bulletPrefab, shootPoint.position, camTrans.rotation);
                tempBullet.GetComponent<BulletScript>().Initialize(hit.point, hit.normal, ((GunInfo)itemInfo).damage);
            }

            //Vector3 direction = GetDirection();

            //if (Physics.Raycast(bulletSpawnPoint.position, direction, out RaycastHit hit)) 
            //{
            //    TrailRenderer trail = Instantiate(bulletTrail, bulletSpawnPoint.position, Quaternion.identity);
            //
            //    StartCoroutine(SpawnTrail(trail, hit));
            //
            //    if (hit.collider.gameObject.CompareTag("Player"))
            //    {
            //        hit.collider.gameObject.GetComponent<PlayerStats>().DrainHealth( ( (GunInfo) itemInfo ) .damage ); 
            //    }
        
            //    PV.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);

            // }

            //Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));

            //ray.origin = cam.transform.position;

            //if (Physics.Raycast(ray, out RaycastHit hit)) 
            //{
            //    if (hit.collider.gameObject.CompareTag("Player")) 
            //    {
            //        hit.collider.gameObject.GetComponent<PlayerStats>().DrainHealth( ( (GunInfo) itemInfo ) .damage ); 
            //    }
            //
            //    PV.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);
            //
            //}
        }

    }
}
