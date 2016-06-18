using UnityEngine;
using System.Collections;

//This is an XML loader test script
public class ToFLoader : MonoBehaviour {

    public const string path = "db";

	// Use this for initialization
	void Start () 
    {
		ToFContainer ic = ToFContainer.Load(path);

		foreach (ToF item in ic.Quizes)
        {
			print(item.isTrue);
        }
	}
	
	
}
