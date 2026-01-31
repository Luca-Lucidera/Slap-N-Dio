using UnityEngine;

namespace Assets.Scripts.Runtime
{
    public class SlapCube : MonoBehaviour
    {
        [SerializeField] private float lifeTime = 1f;

        public void Setup(float slapForce, float slapRadius, Vector3 direction)
        {
            // raggio = hitbox più grande in scala
            transform.localScale = Vector3.one * slapRadius;

            if (TryGetComponent<Rigidbody>(out var rb))
            {
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                rb.AddForce(direction.normalized * slapForce, ForceMode.Impulse);
            }

            Destroy(gameObject, lifeTime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Ci sta l'effetto qua 
        }
    }
}
