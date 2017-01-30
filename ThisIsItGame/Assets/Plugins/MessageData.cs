/*
 * Created by SharpDevelop.
 * User: Daniel
 * Date: 5/27/2016
 * Time: 12:35 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

	
	[Serializable]
	public class MessageData {
		
		public int type = 0;
		public string stringData = "";
		
		public static MessageData FromByteArray(byte[] input) {
			MemoryStream stream = new MemoryStream(input);
			BinaryFormatter formatter = new BinaryFormatter();
			MessageData data = new MessageData();
			data.type = (int)formatter.Deserialize(stream);
			data.stringData = (string)formatter.Deserialize(stream);
			
			if(data.stringData == "") {
				data.type = 999;
				data.stringData = "No command included";
			}
			
			return data;
		}
		
		public static byte[] ToByteArray(MessageData msg) {
			MemoryStream stream = new MemoryStream();
			BinaryFormatter formatter = new BinaryFormatter();
			
			formatter.Serialize(stream, msg.type);
			formatter.Serialize(stream, msg.stringData);
			
			return stream.ToArray();
		}
	}

