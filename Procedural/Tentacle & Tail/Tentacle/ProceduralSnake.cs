using UnityEngine;

namespace Toolbox.Procedural.Tentacle {
    /// <summary>
    /// tentacule fait a partir d'un linerenderer a placer sur le point ou elle doit etre connecté
    ///
    ///      \
    ///     #---#   #---#
    ///     | 1 |---| 2 |
    ///     #---#   #---#
    ///      /
    ///
    ///     1   tete        =>  ProceduralTentacleWiggle et LineRenderer
    ///     2   direction   =>  definie la direction avec sa rotation
    /// </summary>
    public class ProceduralSnake : MonoBehaviour {
        int _length;

        [SerializeField] Transform targetDir;
        [SerializeField] float targetDist;
        [SerializeField] float smoothSpeed;

        [Space] [SerializeField] LineRenderer lineRenderer;


        private Vector3[] _segmentPoses;
        private Vector3[] _segmentV;
        [SerializeField] private GameObject[] bodyParts;

        private void Start() {
            _length = bodyParts.Length;

            lineRenderer.positionCount = _length;

            _segmentPoses = new Vector3[_length];
            _segmentV = new Vector3[_length];
            for (int i = bodyParts.Length - 1; i >= 1; i--) {
                bodyParts[i].GetComponent<BodyPartsRotate>().target = bodyParts[i - 1].transform;
            }

            bodyParts[0].GetComponent<BodyPartsRotate>().target = transform;
        }

        void Update() {
            _segmentPoses[0] = targetDir.position;

            for (int i = 1; i < _segmentPoses.Length; i++) {
                Vector3 targetPos = _segmentPoses[i - 1] +
                                    (_segmentPoses[i] - _segmentPoses[i - 1]).normalized * targetDist;
                _segmentPoses[i] = Vector3.SmoothDamp(_segmentPoses[i], targetPos, ref _segmentV[i], smoothSpeed);

                bodyParts[i - 1].transform.position = _segmentPoses[i];
            }

            lineRenderer.SetPositions(_segmentPoses);
        }
    }
}