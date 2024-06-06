using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public struct MenuPageContext {
    public MainMenu menu;
    public InputActionMap actionMap;
    public GameObject pageObject;
}

public enum MenuDirection { Right, Left };

public class MainMenu : Singleton<MainMenu> {

    private MenuPageContext context;

    private MenuState state;
    private MenuPage currentPage;

    [SerializeField] private AudioClip uiSound;

    [SerializeField] private SplashMenuPage splashMenuPage;
    [SerializeField] private GameModeSelectMenuPage gameModeSelectMenuPage;
    [SerializeField] private FishSelectMenuPage fishSelectMenuPage;

    [SerializeField] private Transform OffScreenPoint;
    [SerializeField] private Transform StartScreenPoint;
    [SerializeField] private Transform CentralPoint;

    bool animating;

    public override void Initialize() {
        base.Initialize();
        InitializeMenu();
    }

    public void InitializeMenu() {
        context = new MenuPageContext();

        context.menu = this;
        SetState(MenuState.Splash);
    }


    public void SetState(MenuState state, MenuDirection dir = MenuDirection.Left) {
        if (animating || this.state == state) {
            return;
        }
        StartCoroutine(CoSetState(state, dir));
        /*
        currentPage?.OnPageStop(context);

        if (state == MenuState.Splash) {
            currentPage = splashMenuPage;
        } else if (state == MenuState.GameModeSelect) {
            currentPage = gameModeSelectMenuPage;
        } else if (state == MenuState.FishSelect) {
            currentPage = fishSelectMenuPage;
        }

        currentPage?.OnPageStart(context);
        */
    }

    IEnumerator CoSetState(MenuState state, MenuDirection dir) {
        animating = true;

        Transform target = (dir == MenuDirection.Left) ? OffScreenPoint : StartScreenPoint;
        Transform source = (dir == MenuDirection.Left) ? StartScreenPoint : OffScreenPoint;

        if (currentPage != null) {
            var page = currentPage;
            page.transform.DOMove(target.position, 1f).OnComplete(() => {
                page.OnPageStop(context);
            });
            yield return new WaitForSeconds(0.25f);
        }
        if (state == MenuState.Splash) {
            currentPage = splashMenuPage;
        }
        else if (state == MenuState.GameModeSelect) {
            currentPage = gameModeSelectMenuPage;
        }
        else if (state == MenuState.FishSelect) {
            currentPage = fishSelectMenuPage;
        }
        currentPage.OnPageStart(context);
        currentPage.transform.position = source.position;
        currentPage.transform.DOMove(CentralPoint.position, 1f);
        yield return new WaitForSeconds(1);
        animating = false;
        this.state = state;
    }

    public void PlayBloop() {
        GetComponent<AudioSource>()?.PlayOneShot(uiSound);
    }
}