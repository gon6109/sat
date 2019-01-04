using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SatCore
{
    public delegate void OnUpdate();

    public static class UndoRedoManager
    {
        static Stack<UndoRedoCommand> UndoStack { get; set; }
            = new Stack<UndoRedoCommand>();

        static Stack<UndoRedoCommand> RedoStack { get; set; }
            = new Stack<UndoRedoCommand>();
        
        public static OnUpdate OnUpdateData { get; set; } = () => { };

        static bool isAction = false;

        public static bool Enable { get; set; } = true; 

        public static void Undo()
        {
            if (UndoStack.Count == 0) return;
            var command = UndoStack.Pop();
            isAction = true;
            command.Undo();
            isAction = false;
            RedoStack.Push(command);
            OnUpdateData();
        }

        public static void Redo()
        {
            if (RedoStack.Count == 0) return;
            var command = RedoStack.Pop();
            isAction = true;
            command.Redo();
            isAction = false;
            UndoStack.Push(command);
            OnUpdateData();
        }

        public static void ChangeProperty(object source, object after, [CallerMemberName]string path = null)
        {
            if (!Enable) return;
            if (isAction)
            {
                isAction = false;
                return;
            }
            if (path == null || after == source.GetType().GetProperty(path).GetValue(source)) return;
            UndoStack.Push(new PropertyChangedCommand(source, path, after, source.GetType().GetProperty(path).GetValue(source)));
            RedoStack.Clear();
            OnUpdateData();
        }

        public static void ChangeProperty(object source, object after, object before,[CallerMemberName]string path = null)
        {
            if (!Enable) return;
            if (isAction)
            {
                isAction = false;
                return;
            }
            if (path == null) return;
            UndoStack.Push(new PropertyChangedCommand(source, path, after, before));
            RedoStack.Clear();
            OnUpdateData();
        }

        public static void ChangeCollection<T>(UndoRedoCollection<T> collection, T changeObject, int? after, int? before)
        {
            if (!Enable) return;
            if (isAction)
            {
                isAction = false;
                return;
            }
            UndoStack.Push(new CollectionChangedCommand<T>(collection, changeObject, after, before));
            RedoStack.Clear();
            OnUpdateData();
        }

        public static void ChangeObject2D(asd.Layer2D layer, asd.Object2D object2D, bool isAdd)
        {
            if (!Enable) return;
            if (isAction)
            {
                isAction = false;
                return;
            }
            UndoStack.Push(new Object2DChangedCommand(layer, object2D, isAdd));
            RedoStack.Clear();
            OnUpdateData();
        }

        public static void Reset()
        {
            UndoStack.Clear();
            RedoStack.Clear();
            OnUpdateData();
        }

        public static bool IsCanUndo => UndoStack.Count > 0;
        public static bool IsCanRedo => RedoStack.Count > 0;

        public abstract class UndoRedoCommand
        {
            public virtual void Undo()
            {

            }

            public virtual void Redo()
            {

            }
        }

        public class PropertyChangedCommand : UndoRedoCommand
        {
            public object Source { get; private set; }
            public string Path { get; private set; }
            public object After { get; private set; }
            public object Before { get; private set; }

            public PropertyChangedCommand(object source, string path, object after, object before)
            {
                Source = source;
                Path = path;
                After = after;
                Before = before;
            }

            public override void Undo()
            {
                Source.GetType().GetProperty(Path).SetValue(Source, Before);
            }

            public override void Redo()
            {
                Source.GetType().GetProperty(Path).SetValue(Source, After);
            }
        }

        public class CollectionChangedCommand<T> : UndoRedoCommand
        {
            public UndoRedoCollection<T> Collection { get; private set; }
            public T ChangedObject { get; private set; }
            public int? After { get; private set; }
            public int? Before { get; private set; }

            public CollectionChangedCommand(UndoRedoCollection<T> collection, T changeObject, int? after, int? before)
            {
                Collection = collection;
                ChangedObject = changeObject;
                After = after;
                Before = before;
            }

            public override void Undo()
            {
                if (After != null && Before == null)
                {
                    Collection.RemoveAt((int)After);
                }
                else if (After == null && Before != null)
                {
                    Collection.Insert((int)Before, ChangedObject);
                }
                else
                {
                    Collection.Move((int)After, (int)Before);
                }
            }

            public override void Redo()
            {
                if (After != null && Before == null)
                {
                    Collection.Insert((int)After, ChangedObject);
                }
                else if (After == null && Before != null)
                {
                    Collection.RemoveAt((int)Before);
                }
                else
                {
                    Collection.Move((int)Before, (int)After);
                }
            }
        }

        public class Object2DChangedCommand : UndoRedoCommand
        {
            public asd.Layer2D Layer { get; private set; }
            public asd.Object2D Object2D { get; private set; }
            public bool IsAdd { get; private set; }

            public Object2DChangedCommand(asd.Layer2D layer, asd.Object2D object2D, bool isAdd)
            {
                Layer = layer;
                Object2D = object2D;
                IsAdd = isAdd;
            }

            public override void Undo()
            {
                if (IsAdd)
                {
                    Layer.RemoveObject(Object2D);
                }
                else
                {
                    Layer.AddObject(Object2D);
                }
            }

            public override void Redo()
            {
                if (!IsAdd)
                {
                    Layer.RemoveObject(Object2D);
                }
                else
                {
                    Layer.AddObject(Object2D);
                }
            }
        }

    }
}
