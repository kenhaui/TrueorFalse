using UnityEngine;
using System;
using System.Xml;
using System.Xml.Serialization;

public enum Difficulty
{
	Easy,
	Medium,
	Hard,
	Bonus
}

[Serializable]
public class ToF {

	[XmlAttribute("question")]
    public string question;

	[XmlElement("Correct")]
    public bool isTrue;

	[XmlElement("difficulty")]
    public Difficulty difficulty;

	[XmlAttribute("fact")]
	public string fact;
}

