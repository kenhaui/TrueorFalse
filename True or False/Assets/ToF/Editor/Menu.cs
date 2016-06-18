using UnityEngine;
using UnityEditor;

public class Menu: EditorWindow {

	[MenuItem("Mintonne/Documentation")]
	static void Documentation()
	{
		string path = "File://" + Application.dataPath + "/ToF/Documentation.pdf";
		Application.OpenURL(path);
		Debug.Log("Opening file at " + Application.dataPath + "/ToF/Documentation.pdf");
	}

	[MenuItem("Mintonne/Contact Support")]
	static void Support()
	{
		Application.OpenURL("mailto:mintonne@gmail.com");
	}

	[MenuItem("Mintonne/Rate ToF")]
	static void Rate()
	{
		UnityEditorInternal.AssetStore.Open("content/60091"); 
	}

	[MenuItem("Mintonne/Request A Feature")]
	static void Request()
	{
		Application.OpenURL("mailto:mintonne@gmail.com?subject=ToF Feature Request");
	}

	[MenuItem("Mintonne/Upgrade To QuizApp")]
	static void QuizApp()
	{
		UnityEditorInternal.AssetStore.Open("content/57254"); 
	}

	[MenuItem("Mintonne/More Unity Assets")]
	static void More()
	{
		Application.OpenURL("https://www.assetstore.unity3d.com/en/#!/search/page=1/sortby=popularity/query=publisher:18385");
	}
}