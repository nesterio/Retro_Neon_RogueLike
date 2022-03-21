using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using DG.Tweening;

public class Semi_Automatic_Gun : Gun
{
    PhotonView PV;
    [SerializeField] Animator anim;

    [SerializeField] GunShutter GS;
    GunRecoil GR;

    float shotSpeed { get { return ((GunInfo)itemInfo).shotSpeed; } }
    float timeToShoot;

    int magCapacity { get { return ((GunInfo)itemInfo).magCapacity; } }
    public int bulletsInMag;

    bool isRealoading { get { return anim.GetCurrentAnimatorStateInfo(0).IsName("Reload");} }

    [Space]

    [SerializeField] bool hasCustomShutter;

    void Awake() 
    {
        PV = GetComponent<PhotonView>();

        GR = GetComponent<GunRecoil>();

        if (GS != null && !hasCustomShutter)
            GS.shutterSpeed = shotSpeed;

        bulletsInMag = magCapacity;

        timeToShoot = shotSpeed; 
    }

    void Update() 
    {
        if (timeToShoot > 0)
            timeToShoot -= Time.deltaTime;
    }


    public override void Use() 
    {
        if (bulletsInMag > 0 && !isRealoading)
        {
            if (timeToShoot <= 0)
            {
                Shoot();
            }
        }
            
    }

    public override void Reload() 
    {
        if (!isRealoading)
            anim.SetBool("Reload", true);
    }
    public override void StopReload() 
    {
        if (isRealoading)
            anim.SetBool("Reload", false);
    }

    public override void Aim(bool shouldAim) 
    {
        if (shouldAim && !isAiming && !isRealoading) 
        {
            containerTrans.DOLocalMove(aimedGunPos, ((GunInfo)itemInfo).aimingSpeed, false);
            isAiming = true;
            WSAB.isAiming = true;

        }

        if (!shouldAim && isAiming || isRealoading) 
        {
            containerTrans.DOLocalMove(relaxedPos, ((GunInfo)itemInfo).aimingSpeed, false);
            isAiming = false;
            WSAB.isAiming = false;
        }
    }

    public void FinishReload() 
    {
        bulletsInMag = magCapacity;
        anim.SetBool("Reload", false);
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
        timeToShoot = shotSpeed;

        RaycastHit hit;

        if (Physics.Raycast(camTrans.position, camTrans.forward, out hit, Mathf.Infinity)) 
        {
            PV.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);
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

    [PunRPC]
    void RPC_Shoot(Vector3 hitPoint, Vector3 hitNormal) 
    {
        GameObject tempBullet = Instantiate(bulletPrefab, shootPoint.position, camTrans.rotation);
        tempBullet.GetComponent<BulletScript>().Initialize(hitPoint, hitNormal, ((GunInfo)itemInfo).damage);
    }

}
