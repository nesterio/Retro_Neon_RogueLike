using DG.Tweening;
using UnityEngine;

namespace Items.Weapons
{
    public class Semi_Automatic_Gun : Gun
    {
        [SerializeField] Animator anim;

        [SerializeField] GunShutter GS;
        GunRecoil GR;

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
            GR = GetComponent<GunRecoil>();

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
            if (bulletsInMag > 0 && !IsRealoading)
            {
                if (_timeToShoot <= 0)
                {
                    Shoot();
                }
            }
            
        }

        public override void Reload() 
        {
            if (!IsRealoading)
                anim.SetBool(Reload1, true);
        }
        public override void StopReload() 
        {
            if (IsRealoading)
                anim.SetBool(Reload1, false);
        }

        public override void Aim(bool shouldAim) 
        {
            if (shouldAim && !isAiming && !IsRealoading) 
            {
                containerTrans.DOLocalMove(aimedGunPos, ((GunInfo)itemInfo).aimingSpeed, false);
                isAiming = true;
                WSAB.isAiming = true;

            }

            if (!shouldAim && isAiming || IsRealoading) 
            {
                containerTrans.DOLocalMove(relaxedPos, ((GunInfo)itemInfo).aimingSpeed, false);
                isAiming = false;
                WSAB.isAiming = false;
            }
        }

        public void FinishReload() 
        {
            bulletsInMag = MagCapacity;
            anim.SetBool(Reload1, false);
        }

        void Shoot() 
        {
            if(isAiming)
                CR.RecoilFire(((GunInfo)itemInfo).recoilAimedX, ((GunInfo)itemInfo).recoilAimedY, ((GunInfo)itemInfo).recoilAimedZ);
            else
                CR.RecoilFire( ((GunInfo)itemInfo).recoilX , ((GunInfo)itemInfo).recoilY , ((GunInfo)itemInfo).recoilZ);

            GR.RecoilFire(isAiming);

            GS.PlayShutter();

            if (shootingParticles != null)
                shootingParticles.Play();

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
