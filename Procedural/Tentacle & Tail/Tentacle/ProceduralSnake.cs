using UnityEngine;

namespace Toolbox.Procedural.Tentacle {
    /// <summary>
    ///     tentacule fait a partir d'un linerenderer a placer sur le point ou elle doit etre connecté
    ///     \
    ///     #---#   #---#
    ///     | 1 |---| 2 |
    ///     #---#   #---#
    ///     /
    ///     1   tete        =>  ProceduralTentacleWiggle et LineRenderer
    ///     2   direction   =>  definie la direction avec sa rotation
    /// </summary>
    [DisallowMultipleComponent]
    public class ProceduralSnake : MonoBehaviour {
        [SerializeField] private Transform targetDir;
        [SerializeField] private float targetDist;
        [SerializeField] private float smoothSpeed;

        [Space] [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private BodyPartsRotate[] bodyParts;
        private int _length;


        private Vector3[] _segmentPoses;
        private Vector3[] _segmentV;

        private void Start() {
            _length = bodyParts.Length;

            lineRenderer.positionCount = _length;

            _segmentPoses = new Vector3[_length];
            _segmentV = new Vector3[_length];

            SetBodyParts();
        }

        [ContextMenu("Set Body Parts")]
        private void SetBodyParts() {
            for (int i = bodyParts.Length - 1; i >= 1; i--) {
                bodyParts[i].target = bodyParts[i - 1].transform;
            }

            bodyParts[0].target = transform;
        }

        private void Update() {
            _segmentPoses[0] = targetDir.position;

            for (int i = 1; i < _segmentPoses.Length; i++) {
                Vector3 targetPos = _segmentPoses[i - 1] +
                                    (_segmentPoses[i] - _segmentPoses[i - 1]).normalized * targetDist;
                _segmentPoses[i] = Vector3.SmoothDamp(_segmentPoses[i], targetPos, ref _segmentV[i], smoothSpeed);

                bodyParts[i - 1].transform.position = _segmentPoses[i];
            }

            lineRenderer.SetPositions(_segmentPoses);
        }


        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.yellow;

            for (int i = 0; i < bodyParts.Length; i++) {
                Gizmos.DrawCube(transform.position + -(transform.forward * i * targetDist), new Vector3(1.5f, 1.5f, .1f));
            }
        }

#if UNITY_EDITOR
        private void OnValidate() {
            SetBodyParts();
            for (int i = 0; i < bodyParts.Length; i++) {
                bodyParts[i].transform.position = transform.position + -(transform.forward * i * targetDist);
            }
        }
#endif

        private Vector3 GetBodyPartsPosition(int index) {
            Vector3 position = Vector3.zero;

            position = transform.position + -(transform.forward * index * targetDist);
            return position;
        }
    }
}