using System;
using System.Collections;
using System.Collections.Generic;
using Scriptabbles.ScriptableEvent;
using UnityEngine;
using UnityEngine.UI;

public class UiFillUpdater : MonoBehaviour {
    [Header("Image")] [SerializeField] private Image _image;

    [Header("References")] [SerializeField]
    private GameEvent _gameEvent;

    private void Awake() { }
}