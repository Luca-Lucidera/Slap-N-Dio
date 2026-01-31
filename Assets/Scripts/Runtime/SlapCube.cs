using UnityEngine;

namespace Assets.Scripts.Runtime
{
	public class SlapCube : MonoBehaviour
	{
		void OnCollisionEnter(Collision collision)
		{
			Destroy(gameObject, 1f);
		}
	}
}