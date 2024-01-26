using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Stungun : Gun
{
    // keep track of which gunfish you have hit, if the fish falls within timezone, tell the stun bullet to ignore the gunfish
    public Dictionary<Gunfish, float> fishZapMap = new Dictionary<Gunfish, float>();

    protected override void _Fire() {
        //base.Fire(firingStatus);
        FX_Spawner.Instance?.SpawnFX(
            FXType.Bang, barrels[0].transform.position, Quaternion.LookRotation(barrels[0].transform.forward, barrels[0].transform.up));


        foreach (GunBarrel barrel in barrels) {
            Stunbullet bullet = Instantiate(gunfish.data.gun.bulletPrefab, barrel.transform.position + (barrel.transform.right*0.1f), Quaternion.identity).GetComponent<Stunbullet>();
            bullet.fishZapMap = fishZapMap;
            bullet.gunfish = gunfish;
            bullet.SetSpeed(barrel.transform.right, 1);
        }
    }
}
