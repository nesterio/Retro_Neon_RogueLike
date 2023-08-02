using System;
using PlayerScripts;

namespace Interactable.Items.Weapons
{
    public class Semi_Automatic_Gun : Gun
    {
        int MagCapacity => ((GunInfo)itemInfo).magCapacity;
        public int bulletsInMag;
        private bool wasHoldingShoot = false;

        protected override void Initialize() 
        {
            base.Initialize();
            bulletsInMag = MagCapacity;
        }

        private void FixedUpdate() => wasHoldingShoot = InputManagerData.Shooting;
        

        public override void Use() 
        {
            if(!PlayerManager.CanUse || IsRealoading)
                return;

            if(!wasHoldingShoot && InputManagerData.Shooting)
            {
                if (TimeToShoot > 0)
                    return;
                
                if (bulletsInMag > 0) 
                    Shoot();
                else
                    FModAudioManager.PlayGunSound(GunSoundsName, GunSoundType.EmptyShot, shootPoint.position);
                
                TimeToShoot = ShotSpeed;
            }
        }
        
        public override void StopReload() 
        {
            base.StopReload();

            bulletsInMag = MagCapacity;
        }

        protected override void Shoot() 
        {
            base.Shoot();
            bulletsInMag--;
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
