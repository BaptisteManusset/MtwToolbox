using CarterGames.Assets.AudioManager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ItsBaptiste.Toolbox.UIScript {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public class ButtonSound : MonoBehaviour, IPointerClickHandler {
        public void OnPointerClick(PointerEventData eventData) {
            AudioManager.Instance.Play("click1");
        }
    }
}