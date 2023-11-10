using System;
using UnityEngine;

namespace Cephei
{
    public static class VectorDivider
    {
        public static Vector3 DividVectorByThreeAxis(Vector3 direction, float magnitude, float forwardPercent, float upPercent, float sidePercent)
        {
            Vector3 forwardForce = direction * magnitude * forwardPercent;
            Vector3 upForce = Vector3.up * magnitude * upPercent;
            Vector3 sideForce = Vector3.Cross(direction, Vector3.up) * magnitude * new System.Random().Sign() * sidePercent;

            return forwardForce + upForce + sideForce;
        }

        [Serializable]
        public class DividVectorByThreeAxi
        {
            [SerializeField] private float _magnitude = 5;
            [SerializeField, Range(0, 1)] private float _upPercent = 0.5f;
            [SerializeField, Range(0, 1)] private float _sidePercent = 0.2f;

            public Vector3 DivideDirection(Vector3 direction, float multiply)
            {
                float magnitude = _magnitude * multiply;
                Vector3 forwardForce = direction * magnitude * (1 - _upPercent - _sidePercent);
                Vector3 upForce = Vector3.up * magnitude * _upPercent;
                Vector3 sideForce = Vector3.Cross(direction, Vector3.up) * magnitude * new System.Random().Sign() * _sidePercent;

                return forwardForce + upForce + sideForce;
            }
        }
    }
}