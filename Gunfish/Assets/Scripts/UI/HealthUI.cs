using DG.Tweening;
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

    [SerializeField]
    private RawImage _respawnBar;
    [SerializeField]
    private CanvasGroup _respawnBarCanvasGroup;

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

    public void Init(Gunfish gunfish, Vector3? offset = null) {
        _gunfish = gunfish;

        _gunfish.OnHealthUpdated += UpdateHealth;
        _gunfish.OnDeath += OnGunfishDeath;
        _gunfish.OnRespawnUpdated += UpdateRespawnBar;
        SetHealth(_gunfish.statusData.health);

        // get ammo and hook into ammo change
        List<GameObject> pips = new List<GameObject>();
        foreach (Transform t in _pipBar.transform) {
            pips.Add(t.gameObject);
        }
        foreach (var p in pips) {
            Destroy(p);
        }
        for (int i = 0; i < gunfish.data.gun.maxAmmo; i++) {
            // add pip
            Instantiate(pip, _pipBar);
        }
        _whiteBar.rectTransform.localScale = new Vector3(1f, 1f, 1f);
        UpdateWhiteBar(0);
        gunfish.gun.OnAmmoChanged += UpdateWhiteBar;

        SetUpConstraint(_gunfish.segments[(int)((float)_gunfish.segments.Count / 3)].transform, offset);

        var playerColor = PlayerManager.Instance.playerColors[gunfish.player.PlayerNumber];

        var fishTitle = transform.FindDeepChild("FishTitle").GetComponent<TextMeshProUGUI>();
        fishTitle.text = $"P{_gunfish.player.VisiblePlayerNumber}";
        fishTitle.color = playerColor;

        var offscreenTracker = GetComponentInChildren<OffscreenTracker>();
        if (offscreenTracker != null) {
            offscreenTracker.goToTrack = _gunfish.MiddleSegment;
            offscreenTracker.color = playerColor;
        }
    }

    void UpdateWhiteBar(float value) {
        if (_whiteBar == null) {
            return;
        }
        _whiteBar.rectTransform.localScale = new Vector3(value, 1f, 1f);
    }

    bool showRespawnBar = false;
    public void UpdateRespawnBar(float value) {
        // if off, then turn on
        //
        if (_respawnBar == null)
            return;
        if (showRespawnBar == false && value > 0) {
            _respawnBarCanvasGroup.DOFade(1f, 0.25f);
            showRespawnBar = true;
        } else if (showRespawnBar == true && value <= 0) {
            _respawnBarCanvasGroup.DOFade(0f, 0.1f);
            showRespawnBar = false;
        }
        _respawnBar.rectTransform.localScale = new Vector3(value, 1f, 1f);
    }

    public void SetHealth(float health) {
        health = Mathf.Clamp(health, 0, health);
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
        Unhook();
    }

    public void Unhook() {
        if (_gunfish) {
            _gunfish.OnHealthUpdated -= UpdateHealth;
            _gunfish.OnDeath -= OnGunfishDeath;
            _gunfish.gun.OnAmmoChanged -= UpdateWhiteBar;
        }
        if (_shootable) {
            _shootable.OnHealthUpdated -= UpdateHealth;
            _shootable.OnDead -= OnShootableDeath;
        }
        EnableBars(false);
        UpdateWhiteBar(0);
        UpdateRespawnBar(0);
    }
}