using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class HealthManaExp : NetworkBehaviour {

	public const float maxMana = 100;
	public const float maxExp = 100;
	public const float maxHealth = 100;
	public bool destroyOnDeath;

	[SyncVar(hook = "OnChangeHealth")]
	public float currentHealth = maxHealth;

	//[SyncVar(hook = "OnChangeMana")]
	//public float currentMana = maxMana;

	//[SyncVar(hook = "OnChangeExp")]
	//public float currentExp = 0;

	public Image healthBar;
	public Image manaBar;
	public Image expBar;

	private NetworkStartPosition[] spawnPoints;

	void Start ()
	{
		if (isLocalPlayer)
		{
			spawnPoints = FindObjectsOfType<NetworkStartPosition>();
		}
	}

	public void TakeDamage(int amount)
	{
		if (!isServer)
			return;

		currentHealth -= amount;
		if (currentHealth <= 0)
		{
			if (destroyOnDeath)
			{
				Destroy(gameObject);
			} 
			else
			{
				currentHealth = maxHealth;

				// called on the Server, invoked on the Clients
				RpcRespawn();
			}
		}
	}

	void OnChangeHealth (float currentHealth)
	{
		float calcHealth = (currentHealth / maxHealth);
		healthBar.fillAmount = calcHealth;
	}

	[ClientRpc]
	void RpcRespawn()
	{
		if (isLocalPlayer)
		{
			// Set the spawn point to origin as a default value
			Vector3 spawnPoint = Vector3.zero;

			// If there is a spawn point array and the array is not empty, pick one at random
			if (spawnPoints != null && spawnPoints.Length > 0)
			{
				spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
			}

			// Set the player’s position to the chosen spawn point
			transform.position = spawnPoint;
		}
	}
}