using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Runtime
{
	public class CubeDemonstratePhysic: MonoBehaviour
	{
		[SerializeField] private float force = 1f;
		void Update()
		{
			if(Keyboard.current.spaceKey.wasPressedThisFrame)
			{
				LaunchSlapCube();
			}
		}

		void LaunchSlapCube()
		{
			GameObject launchCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			launchCube.transform.position = transform.position + Vector3.right * 2;
			launchCube.AddComponent<SlapCube>();
			Rigidbody rb = launchCube.AddComponent<Rigidbody>();
			rb.mass = 0.01f;
			rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			rb.AddForce(Vector3.right * force, ForceMode.Impulse);
		}
	}
}
