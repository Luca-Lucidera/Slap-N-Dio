using UnityEngine;

namespace Assets.Scripts.Runtime
{
    public class FloatingText : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private TextMesh textMesh;

        [Header("Motion")]
        [SerializeField] private float floatSpeed = 1.0f;
        [SerializeField] private float lifetime = 1.2f;
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.8f, 0f);

        private Transform target;
        private Camera cam;
        private float t;

        private void Awake()
        {
            if (textMesh == null)
                textMesh = GetComponent<TextMesh>();

            cam = Camera.main;
        }

        public void Init(Transform followTarget, string message)
        {
            target = followTarget;
            if (textMesh != null) textMesh.text = message;

            if (target != null)
                transform.position = target.position + worldOffset;
        }

        private void Update()
        {
            t += Time.deltaTime;

            if (target != null)
                transform.position = target.position + worldOffset + Vector3.up * (t * floatSpeed);
            else
                transform.position += Vector3.up * (Time.deltaTime * floatSpeed);

            if (cam != null)
            {
                Vector3 dir = transform.position - cam.transform.position;
                transform.rotation = Quaternion.LookRotation(dir);
            }

            if (t >= lifetime)
                Destroy(gameObject);
        }
    }
}
