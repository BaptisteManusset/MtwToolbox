using UnityEngine;

namespace ItsBaptiste.Toolbox.Maths {
    public class SinMovement : MonoBehaviour {
        private Vector3 _position;

        [SerializeField] private Vector3 direction = Vector3.up;
        [SerializeField] private float speed = 1;

        void Start() {
            _position = transform.position;
        }

        private void FixedUpdate() {
            transform.position = _position + Mathf.Sin(Time.timeSinceLevelLoad * speed) * direction;
        }
    }
}