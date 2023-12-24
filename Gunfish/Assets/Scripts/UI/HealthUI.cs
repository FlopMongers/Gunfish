using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Policy;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour {
    public GameObject pip;

    [SerializeField]
    private Canvas _canvas;
    [SerializeField]
    private RawImage _redBar;
    [SerializeField]
    private RawImage _orangeBar;
    [SerializeField]
    private RawImage _greenBar;

    [SerializeField]
    private RawImage _whiteBar;

    [SerializeField]
    private RectTransform _pipBar;

    Gunfish _gunfish;
    Shootable _shootable;

    [SerializeField]
    ParentConstraint pc;

    public void Start() {
        _redBar = transform.FindDeepChild("Red").GetComponent<RawImage>();
        _orangeBar = transform.FindDeepChild("Orange").GetComponent<RawImage>();
        _greenBar = transform.FindDeepChild("Green").GetComponent<RawImage>();
    }

    private void EnableBars(bool enable) {
        _redBar.enabled = enable;
        _orangeBar.enabled = enable;
        _greenBar.enabled = enable;
    }

    float GetTargetMaxHealth() {
        return (_gunfish != null) ? _gunfish.data.maxHealth : _shootable.maxHealth;
    }

    void SetUpConstraint(Transform target, Vector3? offset) {
        var src = new ConstraintSource();
        src.sourceTransform = target;
        src.weight = 1f;
        pc.AddSource(src);
        pc.SetTranslationOffset(0, (offset != null) ? offset.Value : Vector3.zero);
        pc.constraintActive = true;
    }

    public void Init(Shootable shootable, Vector3? offset = null) {
        _shootable = shootable;

        _shootable.OnHealthUpdated += UpdateHealth;
        _shootable.OnDead += OnShootableDeath;
        SetHealth(_shootable.health);

        SetUpConstraint(shootable.transform, offset);
    }

    void UpdateWhiteBar(float value) {
        _whiteBar.rectTransform.localScale = new Vector3(value, 1f, 1f);
    }

    public void Init(Gunfish gunfish, Vector3? offset = null) {
        _gunfish = gunfish;

        _gunfish.OnHealthUpdated += UpdateHealth;
        _gunfish.OnDeath += OnGunfishDeath;
        SetHealth(_gunfish.statusData.health);

        // get ammo and hook into ammo change
        for (int i = 0; i < gunfish.data.gun.maxAmmo; i++) {
            // add pip
            Instantiate(pip, _pipBar);
        }
        _whiteBar.rectTransform.localScale = new Vector3(1f, 1f, 1f);
        gunfish.gun.OnAmmoChanged += UpdateWhiteBar;

        SetUpConstraint(_gunfish.MiddleSegment.transform, offset);
        transform.FindDeepChild("FishTitle").GetComponent<TextMeshProUGUI>().text = $"Player {_gunfish.playerNum + 1}";

        var offscreenTracker = GetComponentInChildren<OffscreenTracker>();
        if (offscreenTracker != null) {
            offscreenTracker.goToTrack = _gunfish.MiddleSegment;
        }
    }

    public void SetHealth(float health) {
        _greenBar.rectTransform.localScale = new Vector3(health / GetTargetMaxHealth(), 1f, 1f);
        _orangeBar.rectTransform.localScale = new Vector3(health / GetTargetMaxHealth(), 1f, 1f);
        // _canvas.enabled = false;
        EnableBars(!Mathf.Approximately(health, GetTargetMaxHealth()));
    }

    private bool _hitInProgress = false;
    private float _targetPercentage = 0f;
    private const float _hitDuration = 2f;
    // private float _timeSpentWaiting = 0f;

    public void UpdateHealth(float health) {
        if (!_canvas)
            return;
        // _canvas.enabled = true;
        EnableBars(!Mathf.Approximately(health, GetTargetMaxHealth()));
        // _timeSpentWaiting = 0f;
        _targetPercentage = health / GetTargetMaxHealth();
        _greenBar.rectTransform.localScale = new Vector3(_targetPercentage, 1f, 1f);

        if (!_hitInProgress) {
            _hitInProgress = true;
            StartCoroutine(UpdateHealthCR());
        }
    }

    private IEnumerator UpdateHealthCR() {
        yield return new WaitForSeconds(0.2f);
        float currentPercentage = _orangeBar.rectTransform.localScale.x;
        while (currentPercentage > _targetPercentage) {
            _orangeBar.rectTransform.localScale = new Vector3(currentPercentage, 1f, 1f);
            currentPercentage -= Time.deltaTime / _hitDuration;
            yield return new WaitForEndOfFrame();
        }
        _orangeBar.rectTransform.localScale = new Vector3(_targetPercentage, 1f, 1f);
        _hitInProgress = false;
    }

    void OnGunfishDeath(Player player) {
        Destroy(gameObject);
    }

    void OnShootableDeath() {
        Destroy(gameObject);
    }

    void OnDestroy() {
        if (_gunfish) {
            _gunfish.OnHealthUpdated -= UpdateHealth;
            _gunfish.OnDeath -= OnGunfishDeath;
            _gunfish.gun.OnAmmoChanged -= UpdateWhiteBar;
        }
        if (_shootable) {
            _shootable.OnHealthUpdated -= UpdateHealth;
            _shootable.OnDead -= OnShootableDeath;
        }
    }
}