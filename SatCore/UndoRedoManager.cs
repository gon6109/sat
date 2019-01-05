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

    /// <summary>
    /// UndoRedo管理クラス
    /// </summary>
    public static class UndoRedoManager
    {
        static Stack<UndoRedoCommand> UndoStack { get; set; }
            = new Stack<UndoRedoCommand>();

        static Stack<UndoRedoCommand> RedoStack { get; set; }
            = new Stack<UndoRedoCommand>();
        
        public static OnUpdate OnUpdateData { get; set; } = () => { };

        static bool isAction = false;

        /// <summary>
        /// UndoRedoを有効にするか
        /// </summary>
        public static bool Enable { get; set; } = true; 

        /// <summary>
        /// 戻る
        /// </summary>
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

        /// <summary>
        /// やり直す
        /// </summary>
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

        /// <summary>
        /// プロパティ変更
        /// </summary>
        /// <param name="source">プロパティを持つインスタンス</param>
        /// <param name="after">変更後の値</param>
        /// <param name="path">プロパティ名</param>
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

        /// <summary>
        /// プロパティ変更
        /// </summary>
        /// <param name="source">プロパティを持つインスタンス</param>
        /// <param name="after">変更後の値</param>
        /// <param name="before">変更前の値</param>
        /// <param name="path">プロパティ名</param>
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

        /// <summary>
        /// コレクションの変更
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">コレクション</param>
        /// <param name="changeObject">コレクションを持つインスタンス</param>
        /// <param name="after">加えた場所のindex</param>
        /// <param name="before">削除した場所のindex</param>
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

        /// <summary>
        /// Object2Dコレクションの変更
        /// </summary>
        /// <param name="layer">Object2Dをもつレイヤー</param>
        /// <param name="object2D">Object2D</param>
        /// <param name="isAdd">加えたか</param>
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

        /// <summary>
        /// リセット
        /// </summary>
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
