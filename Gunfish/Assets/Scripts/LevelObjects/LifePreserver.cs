using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using System.Collections.Generic;

[RequireComponent(typeof(ObjectMaterial))]
[RequireComponent(typeof(CompositeCollisionDetector))]
public class LifePreserver : MonoBehaviour
{
    [SerializeField] private CompositeCollisionDetector fishCollisionDetector;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Transform spriteTransform;

    [SerializeField] [Range(0f, 2f)] private float boingScale = 1.2f;
    [SerializeField] [Range(0f, 1f)] private float boingDuration = 0.2f;
    [SerializeField] [Range(0f, 10f)] private float boingElasticAmplitude = 1f;
    [SerializeField] [Range(0f, 10f)] private float boingElasticPeriod = 1f;
    [SerializeField] [Range(0f, 200f)] private float explosionForce = 100f;


    private Vector3 initialSpriteScale;

    private void Start() {
        fishCollisionDetector.OnComponentCollideEnter += delegate (GameObject src, Collision2D collision) {
            if (!collision.collider.GetComponent<ObjectMaterial>()) return;
            var allowedMaterials = new List<MaterialType> {
                MaterialType.Fish,
                MaterialType.Rock,
            };
            if (allowedMaterials.Contains(collision.collider.GetComponent<ObjectMaterial>().materialType)) {
                Boing();
                if (collision.collider.GetComponent<Rigidbody2D>()) {
                    collision.collider.GetComponent<Rigidbody2D>().AddForce((collision.collider.transform.position - transform.position).normalized * explosionForce, ForceMode2D.Impulse);
                }
            }
        };

        initialSpriteScale = spriteTransform.localScale;
    }

    void Update() {
        if (Keyboard.current != null && Keyboard.current.lKey.wasPressedThisFrame) {
            Boing();
        }
    }

    private void Boing() {
        spriteTransform.DOKill();
        audioSource?.Play();
        spriteTransform.localScale = initialSpriteScale;
        spriteTransform.DOScale(Vector3.one * boingScale, boingDuration / 2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutElastic, boingElasticAmplitude, boingElasticPeriod);
    }

}
