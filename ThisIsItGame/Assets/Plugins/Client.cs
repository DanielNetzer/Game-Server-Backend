using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.Text;

public class Client : MonoBehaviour {

	public string serverAddress = "127.0.0.1";
	public const int serverPort = 32211;
	public bool isConnected = false;

	private static Client singleton;
	private Socket sServer;

	void Awake() {
		sServer = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		IPAddress remoteIPAddress = IPAddress.Parse (serverAddress);
		IPEndPoint remoteEndPoint = new IPEndPoint (remoteIPAddress, serverPort);
		singleton = this;
		sServer.Connect (remoteEndPoint);
		DontDestroyOnLoad (gameObject);
	}

	void Update() {
		if (isConnected != sServer.Connected)
			isConnected = sServer.Connected;
	}

	void OnApplicationQuit() {
		sServer.Close ();
		sServer = null;
	}

	static public void Send(MessageData msgData) {
		if (singleton.sServer == null) {
			return;
		}
		byte[] sendData = MessageData.ToByteArray (msgData);
		byte[] buffer = new byte[1];
		buffer [0] = (byte)sendData.Length;
		singleton.sServer.Send (buffer);
		singleton.sServer.Send (sendData);
	}

	static public string[] Receive(){
		byte[] bytes = new byte[1024];
		int bytesRec = singleton.sServer.Receive (bytes);
		string[] data = Encoding.ASCII.GetString (bytes,0,bytesRec).Split('|');

		// string[] data structure :: 
		// 	Index 0-> Process 
		// 		Process Mapping :: 0 -> Login, 1 -> Character , 2 -> Register, 3->...
		// 	Index 1-> 1 = Success/ 0 = Error, if Error then Index 2 exits with error type.
		//		Error mapping :: Login -> 0-> Wrong username
		//								  1-> Wrong Password, 
		//						Registration -> 0-> Username exists
		//									 -> 1-> Email exists

		return data;
	} 
}
