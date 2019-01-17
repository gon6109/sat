﻿using AlteseedScript.Common;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer
{
    /// <summary>
    /// スクリプト管理
    /// </summary>
    public class ScriptOption
    {
        public static Dictionary<string, ScriptOption> ScriptOptions { get; } = new Dictionary<string, ScriptOption>()
        {
            {"MapObject", new ScriptOption()
            {
                UseNameSpaces = { "SatScript.Common", "SatScript.Player", "SatScript.Collision", "SatScript.MapObject", "AlteseedScript.Common"},
                Assemblies = {Assembly.GetAssembly(typeof(SatScript.MapObject.MapObject)), Assembly.GetAssembly(typeof(Vector))},
                GlobalType = typeof(SatScript.MapObject.IMapObject)
            }
            },
            {"EventObject", new ScriptOption()
            {
                UseNameSpaces = { "SatScript.Common", "SatScript.Player", "SatScript.Collision", "SatScript.MapObject", "AlteseedScript.Common" },
                Assemblies = {Assembly.GetAssembly(typeof(SatScript.MapObject.MapObject)), Assembly.GetAssembly(typeof(Vector))},
                GlobalType = typeof(SatScript.MapObject.IEventObject)
            }
            },
            {"BackGround", new ScriptOption()
            {
                UseNameSpaces = { "SatScript.Common", "SatScript.Player", "SatScript.MapObject", "AlteseedScript.Common" },
                Assemblies = {Assembly.GetAssembly(typeof(SatScript.MapObject.MapObject)), Assembly.GetAssembly(typeof(Vector))},
                GlobalType = typeof(SatScript.BackGround.IBackGround)
            }
            },
            {"Player", new ScriptOption()
            {
                UseNameSpaces = { "SatScript.Common", "SatScript.Player", "SatScript.Collision", "SatScript.MapObject", "AlteseedScript.Common" },
                Assemblies = {Assembly.GetAssembly(typeof(SatScript.MapObject.MapObject)), Assembly.GetAssembly(typeof(Vector))},
                GlobalType = typeof(SatScript.Player.IPlayer)
            }
            },
        };

        /// <summary>
        /// 使用する名前空間
        /// </summary>
        public List<string> UseNameSpaces { get; private set; }

        /// <summary>
        /// 使用するアセンブリ
        /// </summary>
        public List<Assembly> Assemblies { get; private set; }

        public Type GlobalType { get; private set; }

        public ScriptOptions ToScriptOptions()
            => Microsoft.CodeAnalysis.Scripting.ScriptOptions.Default
            .AddImports(UseNameSpaces)
            .AddReferences(Assemblies);

        public Script<T> CreateScript<T>(string code)
            => CSharpScript.Create<T>(code, ToScriptOptions(), GlobalType);
    }
}
