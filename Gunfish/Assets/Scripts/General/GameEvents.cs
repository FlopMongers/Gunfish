using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void GameEvent();
public delegate void CountGameEvent(int count);
public delegate void PlayerGameEvent(Player player);
public delegate void FloatGameEvent(float value);
public delegate void FishEvent(Gunfish fish);
public delegate void FishCollisionEvent(GunfishSegment segment, Collision2D collision);
public delegate void FishTriggerEvent(GunfishSegment segment, Collider2D collision);
public delegate void CollisionEvent(GameObject src, Collision2D collision);
public delegate void TriggerEvent(GameObject src, Collider2D collision);
public delegate void FishHitEvent(Gunfish gunfish, FishHitObject hit);