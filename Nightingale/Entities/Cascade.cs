﻿using System.Xml.Serialization;

namespace Nightingale.Entities
{
	public enum Cascade
	{
		[XmlEnum(Name = "None")]
		None,

		[XmlEnum(Name = "Save")]
		Save,

		[XmlEnum(Name = "Delete")]
		SaveDelete
	}
}
