﻿using System;

namespace ThuCommix.EntityFramework.Entities
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EagerLoadAttribute : Attribute
    {
    }
}