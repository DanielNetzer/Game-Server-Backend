using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class RegisterController : MonoBehaviour {

	// Login Objects (Input fields and Buttons)
	public Button registerButton;
	public InputField userNameInput;
	public InputField passwordInput;
	public InputField emailInput;

	public void OnRegisterClick()
	{
		if (userNameInput.text.Equals(""))
		{
			Debug.Log ("Empty Username");
		} else if (passwordInput.text.Equals("")) {
			Debug.Log ("Empty Password");
		} else if (emailInput.text.Equals("")) {
			Debug.Log ("Empty Email");
		} else {
			MessageData msg = new MessageData ();
			msg.type = 2;
			msg.stringData = (userNameInput.text + '|' + passwordInput.text + '|' + emailInput.text);
			Client.Send (msg);
			string[] response = Client.Receive ();
			if (response [1] == "1")
				Debug.Log ("Registration Succesful");
			// TODO: Implement Scene(2) -> CharacterSelection Scene.
			// SceneManager.LoadScene (2);
			else if (response [2] == "1")
				Debug.Log ("Username Exists");
			else if (response [2] == "0")
				Debug.Log ("Email Exists");
		}
	}
		
}
