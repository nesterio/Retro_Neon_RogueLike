using Interactable.Items.Weapons;
using PlayerScripts;

public class Automatic_Gun : Semi_Automatic_Gun
{
    public override void Use()
    {
        if(!PlayerManager.CanUse || IsRealoading)
            return;

        if (TimeToShoot > 0)
            return;
                
        if (bulletsInMag > 0) 
            Shoot();
        else
            FModAudioManager.PlayGunSound(GunSoundsName, GunSoundType.EmptyShot, shootPoint.position);
                
        TimeToShoot = ShotSpeed;
    }
}
