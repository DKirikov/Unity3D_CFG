using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CFGEngine;
using System.Text;

public class SaveLoad : MonoBehaviour {

	public GameObject saved_games;

	// Use this for initialization
	void Start ()
	{
		DirectoryInfo directoryInfo = new DirectoryInfo (Application.persistentDataPath);
		FileInfo[] fileInfo = directoryInfo.GetFiles ("*.xml", SearchOption.AllDirectories);

		Dropdown dropdown = saved_games.GetComponent<Dropdown> ();
		dropdown.options.Clear ();

		foreach (FileInfo file in fileInfo) 
		{
			Dropdown.OptionData optionData = new Dropdown.OptionData (file.Name);
			dropdown.options.Add (optionData);
		}
	}

	public void OnItemChanged()
	{
		Dropdown dropdown = saved_games.GetComponent<Dropdown> ();

		XmlSerializer serializer = new XmlSerializer(typeof(GameboardImpl));
		
		string path = Path.Combine (Application.persistentDataPath, dropdown.options[dropdown.value].text);
		
		FileStream stream = new FileStream(path, FileMode.Open);
		
		DebugConsole.Log("Game loaded from the " + path);

		GameboardImpl new_gameboard = (GameboardImpl) serializer.Deserialize(stream);

		GameBoard.instance.LoadGame (new_gameboard);
		
		stream.Close();

		Destroy (this.gameObject);
	}

	public static void SaveGame(GameboardImpl gameboard)
	{
		string path = null;
		int count = 1;
		
		do
		{
			path = Path.Combine (Application.persistentDataPath, count + ".xml");
			++count;
		}
		while(File.Exists (path));
		
		XmlSerializer serializer = new XmlSerializer(typeof(GameboardImpl));
		DebugConsole.Log("Game saved to the " + path);

		Encoding encoding = Encoding.GetEncoding("UTF-8");
		
		using(StreamWriter sw = new StreamWriter(path, false, encoding))
		{
			serializer.Serialize(sw, gameboard);
		}

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
