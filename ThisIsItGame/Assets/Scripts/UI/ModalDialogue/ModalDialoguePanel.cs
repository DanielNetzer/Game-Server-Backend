using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class ModalDialoguePanel : MonoBehaviour {

	public Text topPanelText;
	public Image topPanelIcon;
	public Button okButton;
	public Button yesButton;
	public Button noButton;
	public Button cancelButton;

	public GameObject modalPanelObject;

	private static ModalDialoguePanel modalDialoguePanel;

	public static ModalDialoguePanel Instance() {
		if (!modalDialoguePanel) {
			modalDialoguePanel = FindObjectOfType (typeof(ModalDialoguePanel)) as ModalDialoguePanel;
			if (!modalDialoguePanel)
				Debug.LogError ("There needs to be on active ModalDialoguePanel script on GameObject in your scene");
		}

		return modalDialoguePanel;
	}

	public void Selection (string topPanelText, UnityAction yesEvent, UnityAction noEvent, UnityAction okEvent, UnityAction cancelEvent){
		modalPanelObject.SetActive (true);

		yesButton.onClick.RemoveAllListeners ();
		yesButton.onClick.AddListener (yesEvent);
		yesButton.onClick.AddListener (ClosePanel);

		noButton.onClick.RemoveAllListeners ();
		noButton.onClick.AddListener (noEvent);
		noButton.onClick.AddListener (ClosePanel);

		okButton.onClick.RemoveAllListeners ();
		okButton.onClick.AddListener (okEvent);
		okButton.onClick.AddListener (ClosePanel);

		cancelButton.onClick.RemoveAllListeners ();
		cancelButton.onClick.AddListener (cancelEvent);
		cancelButton.onClick.AddListener (ClosePanel);

		this.topPanelText.text = topPanelText;

		this.topPanelIcon.gameObject.SetActive (false);
		cancelButton.gameObject.SetActive (true);
	}

	void ClosePanel() {
		modalPanelObject.SetActive (false);
	}
}
