using UnityEngine;
using System.Collections;

namespace Assets.Scripts.Runtime
{
	[RequireComponent(typeof(BoxCollider))]
	public class KillY: MonoBehaviour
	{
        void Awake()
        {
            GetComponent<BoxCollider>().isTrigger = true;
        }
        private void OnTriggerEnter(Collider other)
        {
            Destroy(other.gameObject);
        }
    }
}