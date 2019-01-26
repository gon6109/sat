using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using PhysicAltseed;
using RoslynPad.Editor;
using RoslynPad.Roslyn;
using SatCore.ScriptEditor;
using SatPlayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SatUI
{
    /// <summary>
    /// CodeEditor.xaml の相互作用ロジック
    /// </summary>
    public partial class CodeEditor : UserControl, IDisposable
    {
        ScriptOjectRoslynHost host;

        IScriptObject BindingSource;

        string Path;

        public CodeEditor(IScriptObject bindingSource, string path)
        {
            InitializeComponent();
            roslynCodeEditor.TextChanged += RoslynCodeEditor_TextChanged;
            BindingSource = bindingSource;
            Path = path;
        }

        private void RoslynCodeEditor_TextChanged(object sender, EventArgs e)
        {
            if ((string)BindingSource.GetType().GetProperty(Path).GetValue(BindingSource) == roslynCodeEditor.Text) return;
            BindingSource.GetType().GetProperty(Path).SetValue(BindingSource, roslynCodeEditor.Text);
        }

        DocumentId document;
        private void roslynCodeEditor_Loaded(object sender, RoutedEventArgs e)
        {
            var additionalAssemblies = new List<Assembly>
            {
                Assembly.Load("RoslynPad.Roslyn.Windows"),
                Assembly.Load("RoslynPad.Editor.Windows"),
            };
            additionalAssemblies.AddRange(ScriptOption.ScriptOptions[BindingSource.ScriptOptionName].Assemblies);
            host = new ScriptOjectRoslynHost(
                ScriptOption.ScriptOptions[BindingSource.ScriptOptionName].GlobalType,
                additionalAssemblies: additionalAssemblies,
                references: RoslynHostReferences.Empty.With(
                    imports: ScriptOption.ScriptOptions[BindingSource.ScriptOptionName].UseNameSpaces,
                    assemblyReferences: ScriptOption.ScriptOptions[BindingSource.ScriptOptionName].Assemblies)
            );
            document = roslynCodeEditor.Initialize(host, new ClassificationHighlightColors(), Directory.GetCurrentDirectory(), BindingSource.GetType().GetProperty(Path).GetValue(BindingSource) as string ?? string.Empty);
        }

        public void Dispose()
        {
            BindingSource = null;
            host.CloseDocument(document);
            if (roslynCodeEditor.GetType().GetField("_contextActionsRenderer", BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.GetField |
                BindingFlags.FlattenHierarchy |
                BindingFlags.SetField)
                .GetValue(roslynCodeEditor) is ContextActionsRenderer renderer)
            {
                renderer.Dispose();
            }
        }
    }
}