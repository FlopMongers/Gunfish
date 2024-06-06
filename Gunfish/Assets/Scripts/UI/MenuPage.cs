using UnityEngine;
using UnityEngine.UIElements;

public class MenuPage : MonoBehaviour {
    public virtual void OnPageStart(MenuPageContext context) {
        gameObject?.SetActive(true);
    }
    public virtual void OnPageStop(MenuPageContext context) {
        gameObject?.SetActive(false);
    }
}