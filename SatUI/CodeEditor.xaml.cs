using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using PhysicAltseed;
using RoslynPad.Editor;
using RoslynPad.Roslyn;
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
        MapObjectRoslynHost host = new MapObjectRoslynHost(
               additionalAssemblies: new[]
               {
                    Assembly.Load("RoslynPad.Roslyn.Windows"),
                    Assembly.Load("RoslynPad.Editor.Windows"),
               },
               references: RoslynHostReferences.Default.With(typeNamespaceImports: new[] { typeof(SatPlayer.MapObject), typeof(asd.Vector2DF), typeof(PhysicalWorld) })
            );

        INotifyPropertyChanged BindingSource;

        string Path;

        public CodeEditor(INotifyPropertyChanged bindingSource, string path)
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