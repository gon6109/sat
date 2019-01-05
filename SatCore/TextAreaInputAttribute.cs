﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore
{
    /// <summary>
    /// 複数行テキスト入力
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class TextAreaInputAttribute : IOAttribute
    {
        public TextAreaInputAttribute(string itemName)
        {
            ItemName = itemName;
        }

        public string ItemName { get; private set; }
    }
}
