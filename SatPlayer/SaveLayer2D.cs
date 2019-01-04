using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer
{
    public class SaveLayer2D : UI.UILayer2D
    {
        public bool IsEnd { get; private set; }

        public SaveLayer2D(int id)
        {
            for (int i = 0; i < 6; i++)
            {
                SaveDataComponent saveDataComponent = new SaveDataComponent(string.Format("Save/Data{0}", i + 1), 360f * i / 6);
                var pos = new asd.Vector2DF();
                pos.X = i % 2 == 0 ? 550 : 1380;
                if (i < 2) pos.Y = 265;
                else if (i < 4) pos.Y = 545;
                else if (i < 6) pos.Y = 825;
                saveDataComponent.Position = pos;
                saveDataComponent.OnPushed = (obj) =>
                {
                    var comp = obj as SaveDataComponent;
                    IsEnd = true;
                    var game = comp?.Layer.Scene as Game;
                    var saveData = game?.ToSaveData();
                    saveData.SavePointID = id;
                    if (!Directory.Exists("Save")) Directory.CreateDirectory("Save");
                    saveData.Save(comp.Path);
                };
                saveDataComponent.IsEnable = true;
                AddObject(saveDataComponent);
            }
        }

        protected override void OnAdded()
        {
            Connect();
            base.OnAdded();
        }
    }
}
