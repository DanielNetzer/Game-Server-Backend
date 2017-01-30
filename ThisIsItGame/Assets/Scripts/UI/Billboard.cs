using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour {

	public Camera playerCamera;

	void Update () {
		transform.LookAt(playerCamera.transform);
	}
}