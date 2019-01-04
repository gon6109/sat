using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore
{
    public class UndoRedoCollection<T> : ObservableCollection<T>
    {
        public UndoRedoCollection()
        {
            CollectionChanged += UndoRedoCollection_CollectionChanged;
        }

        public UndoRedoCollection(IEnumerable<T> collection) : base(collection)
        {
            CollectionChanged += UndoRedoCollection_CollectionChanged;
        }

        public UndoRedoCollection(List<T> list) : base(list)
        {
            CollectionChanged += UndoRedoCollection_CollectionChanged;
        }

        private void UndoRedoCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    UndoRedoManager.ChangeCollection(this, (T)e.NewItems[0], e.NewStartingIndex, null);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    UndoRedoManager.ChangeCollection(this, (T)e.OldItems[0], null, e.OldStartingIndex);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    UndoRedoManager.ChangeCollection(this, (T)e.OldItems[0], e.NewStartingIndex, e.OldStartingIndex);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    break;
            }
        }
    }
}
