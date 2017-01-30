using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{

	public GameObject bulletPrefab;
	public Transform bulletSpawn;
	public GameObject LocalPlayerUI;
	public GameObject FloatingPlayerUI;


    void Update()
    {
				if (!isLocalPlayer)
				{
					LocalPlayerUI.SetActive (false);	
					return;
				}
		FloatingPlayerUI.SetActive (false);
		float x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
		float z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

        transform.Rotate(0, x, 0);
        transform.Translate(0, 0, z);

		if (Input.GetKeyDown (KeyCode.Space)) 
		{
			CmdFire ();
		}
    }

	[Command]
	void CmdFire()
	{
		// Creating the Bullet from the Bullet Prefab
		GameObject bullet = (GameObject)Instantiate (
			             bulletPrefab,
			             bulletSpawn.position,
			             bulletSpawn.rotation);

		// Adding velocity to the bullet
		bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 6;

		// Spawn the bullet on the Clients
		NetworkServer.Spawn(bullet);

		// Destroying the bullet after 2 seconds
		Destroy(bullet, 2.0f);
	}

	public override void OnStartLocalPlayer()
	{
		GetComponent<MeshRenderer>().material.color = Color.blue;
	}
}
