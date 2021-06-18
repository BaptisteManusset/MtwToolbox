using UnityEngine;
using UnityEngine.UI;

public class UiFillUpdater : MonoBehaviour {
    [Header("Image")] [SerializeField] private Image _image;

    [Header("References")] [SerializeField]
    private GameEvent.GameEvent _gameEvent;

    private void Awake() { }
}