using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

	void OnCollisionEnter(Collision collision)
	{
		GameObject hit = collision.gameObject;
		if (hit.tag == "Player") {
			HealthManaExp healthManaExp = hit.GetComponent<HealthManaExp> ();
			if (healthManaExp != null) {
				healthManaExp.TakeDamage (10);
			}
		} else if (hit.tag == "Enemy") {
			Health health = hit.GetComponent<Health> ();
			if (health != null) {
				health.TakeDamage (10);
			}
		}
		Destroy (gameObject);
	}
}
