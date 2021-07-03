using UnityEngine;

namespace Toolbox.Procedural.Tentacle {
    public class BodyPartsRotate : MonoBehaviour {
        [SerializeField] private float speed;
        public Transform target;
        private Vector2 direction;


        private void Update() {
            direction = target.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, speed * Time.deltaTime);
        }
    }
}