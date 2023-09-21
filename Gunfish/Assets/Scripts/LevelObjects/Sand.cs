using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sand : GroundMaterial
{
    public float flopModifier;

    protected override void HandleFishCollisionEnter(GunfishSegment segment, Collision2D collision)
    {
        segment.gunfish.AddEffect(new FlopModify_Effect(segment.gunfish, flopModifier));
    }

    protected override void HandleFishCollisionExit(GunfishSegment segment, Collision2D collision)
    {
        segment.gunfish.AddEffect(new FlopModify_Effect(segment.gunfish, -flopModifier));
    }
}
