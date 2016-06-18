using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("QuizCollection")]
public class ToFContainer {

	[XmlArray("Quizes")]
	[XmlArrayItem("Quiz")]
	public List<ToF> Quizes = new List<ToF>();

	public static ToFContainer Load(string path)
    {
        TextAsset _xml = Resources.Load<TextAsset>(path);

		XmlSerializer serializer = new XmlSerializer(typeof(ToFContainer));

        StringReader reader = new StringReader(_xml.text);

		ToFContainer Items = serializer.Deserialize(reader) as ToFContainer;

        reader.Close();

		return Items;
    }
}
