using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoginController : MonoBehaviour {

	// Login Objects (Input fields and Buttons)
	public Button loginButton;
	public Button registerButton;
	public InputField userNameInput;
	public InputField passwordInput;

	// TODO: Handle Login Button Click
	public void OnLoginClick()
	{
		if (userNameInput.text.Equals(""))
		{
			Debug.Log ("Empty Username");
		} else if (passwordInput.text.Equals("")) {
			Debug.Log ("Empty Password");
		} else {
			MessageData msg = new MessageData ();
			msg.type = 0;
			msg.stringData = (userNameInput.text + "|" + passwordInput.text);
			Client.Send (msg);
			string[] response = Client.Receive ();
			if (response [1] == "1")
				Debug.Log ("Login Succesful");
			// TODO: Implement Scene(2) -> CharacterSelection Scene.
			// SceneManager.LoadScene (2);
			else if (response [2] == "1")
				Debug.Log ("Wrong Password");
			else if (response [2] == "0")
				Debug.Log ("Wrong Username");
			else if (response [2] == "2")
				Debug.Log("Account not activated");
		}
	}
		
	public void OnRegisterClick()
	{
		SceneManager.LoadScene (1);
	}
		
}
