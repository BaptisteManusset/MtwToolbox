#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// This causes the class' static constructor to be called on load and on starting playmode
namespace ItsBaptiste.Toolbox.Tools {
    [InitializeOnLoad]
    class PhysicsSetter {
        // only ever register once
        static bool _registered = false;

        // are we actively settling physics in our scene
        static bool _active = false;

        // the work list of rigid bodies we can find loaded up
        static List<Rigidbody> _workList;

        // we need to disable auto simulation to manually tick physics
        static bool _cachedAutoSimulation;

        // how long do we run physics for before we give up getting things to sleep
        const float TimeToSettle = 10f;

        // how long have we been running
        static float _activeTime = 0f;

        // this is the static constructor called by [InitializeOnLoad]
        static PhysicsSetter() {
            if (!_registered) {
                // hook into the editor update
                EditorApplication.update += Update;

                // and the scene view OnGui
                SceneView.duringSceneGui += OnSceneGUI;
                _registered = true;
            }
        }

        // let users turn on 
        [MenuItem("GameMenu/Settle Physics")]
        static void Activate() {
            if (!_active) {
                _active = true;


                List<Transform> selections = Selection.transforms.ToList();


                _workList = new List<Rigidbody>();
                for (int i = selections.Count - 1; i >= 0; i--) {
                    Rigidbody rb = selections[i].gameObject.GetComponent<Rigidbody>();
                    if (rb != null) _workList.Add(rb);
                }

                // Normally avoid Find functions, but this is editor time and only happens once
                // _workList = Object.FindObjectsOfType<Rigidbody>();
                // _workList = Object.FindObjectsOfType<Rigidbody>();

                // we will need to ensure autoSimulation is off to manually tick physics
                _cachedAutoSimulation = Physics.autoSimulation;
                _activeTime = 0f;

                // make sure that all rigidbodies are awake so they will actively settle against changed geometry.
                foreach (Rigidbody body in _workList) {
                    body.WakeUp();
                }
            }
        }

        // grey out the menu item while we are settling physics
        [MenuItem("GameMenu/Settle Physics", true)]
        static bool CheckMenu() {
            return !_active;
        }

        static void Update() {
            if (_active) {
                _activeTime += Time.deltaTime;

                // make sure we are not autosimulating
                Physics.autoSimulation = false;

                // see if all our 
                bool allSleeping = true;
                foreach (Rigidbody body in _workList) {
                    if (body != null) {
                        allSleeping &= body.IsSleeping();
                    }
                }

                if (allSleeping || _activeTime >= TimeToSettle) {
                    Physics.autoSimulation = _cachedAutoSimulation;
                    _active = false;
                }
                else {
                    Physics.Simulate(Time.deltaTime);
                }
            }
        }

        static void OnSceneGUI(SceneView sceneView) {
            if (_active) {
                Handles.BeginGUI();
                Color cacheColor = GUI.color;
                GUI.color = Color.red;
                GUILayout.Label("Simulating Physics.", GUI.skin.box, GUILayout.Width(200));
                GUILayout.Label(string.Format("Time Remaining: {0:F2}", (TimeToSettle - _activeTime)), GUI.skin.box, GUILayout.Width(200));
                Handles.EndGUI();

                foreach (Rigidbody body in _workList) {
                    if (body != null) {
                        bool isSleeping = body.IsSleeping();
                        if (!isSleeping) {
                            GUI.color = Color.green;
                            Handles.Label(body.transform.position, "SIMULATING");
                        }
                    }
                }

                GUI.color = cacheColor;
            }
        }
    }
}
#endif