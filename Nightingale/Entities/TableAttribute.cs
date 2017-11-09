﻿using System;

namespace Nightingale.Entities
{
	[AttributeUsage(AttributeTargets.Class)]
	public class TableAttribute : Attribute
	{
		public string Table { get; }

		public TableAttribute(string table)
		{
			Table = table;
		}
	}
}
