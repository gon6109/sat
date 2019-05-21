using BaseComponent;
using SatPlayer.Game;
using SatPlayer.Game.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer.Game.Object
{
    /// <summary>
    /// セーブポイント
    /// </summary>
    public class SavePoint : MultiAnimationObject2D
    {
        private IEnumerator enumerator;

        public int ID { get; private set; }

        public bool IsActive { get; private set; }

        public SavePoint(SatIO.SavePointIO savePointIO)
        {
            Texture = TextureManager.LoadTexture("Static/save_point.png");
            CenterPosition = Texture.Size.To2DF() / 2;
            Position = savePointIO.Position;
            ID = savePointIO.ID;
        }

        void OpenSaveMenu()
        {
            enumerator = Save();
        }

        public IEnumerator Save()
        {
            IsActive = true;
            Layer.Scene.HDRMode = true;
            foreach (var item in Layer.Objects.Where(obj => obj != this && !(obj is MapEvent.MapEvent)))
            {
                item.IsUpdated = false;
            }
            var blur = new asd.PostEffectGaussianBlur();
            Layer.AddPostEffect(blur);
            foreach (Player item in Layer.Objects.Where(obj => obj is Player))
            {
                item.HP = Player.MaxHP;
            }

            for (int i = 0; i < 20; i++) yield return 0;

            YesNoLayer2D askSave = new YesNoLayer2D("セーブする?");
            askSave.DrawingPriority = 3;
            Layer.Scene.AddLayer(askSave);
            askSave.IsUpdated = false;

            for (int i = 0; i < 15; i++)
            {
                blur.Intensity = i * 0.2f;
                foreach (asd.DrawnObject2D item in askSave.Objects)
                {
                    item.Color = new asd.Color(255, 255, 255, (int)(255f * i / 15));
                }
                yield return 0;
            }
            askSave.IsUpdated = true;

            while (!askSave.IsEnd) yield return 0;

            for (int i = 0; i < 15; i++)
            {
                foreach (asd.DrawnObject2D item in askSave.Objects)
                {
                    item.Color = new asd.Color(255, 255, 255, (int)(255f - 255f * i / 15));
                }
                yield return 0;
            }

            if (askSave.IsYes)
            {
                askSave.Dispose();

                SaveLayer2D save = new SaveLayer2D(ID);
                save.DrawingPriority = 3;
                Layer.Scene.AddLayer(save);
                save.IsUpdated = false;

                for (int i = 0; i < 15; i++)
                {
                    foreach (asd.DrawnObject2D item in save.Objects)
                    {
                        var temp = item.Color;
                        temp.A = (byte)(255f * i / 15);
                        item.Color = temp;
                    }
                    yield return 0;
                }
                save.IsUpdated = true;

                while (!save.IsEnd) yield return 0;

                for (int i = 0; i < 15; i++)
                {
                    foreach (asd.DrawnObject2D item in save.Objects)
                    {
                        item.Color = new asd.Color(255, 255, 255, (int)(255f - 255f * i / 15));
                    }
                    yield return 0;
                }

                save.Dispose();

                YesNoLayer2D askEnd = new YesNoLayer2D("タイトルに戻る?");
                askEnd.DrawingPriority = 3;
                Layer.Scene.AddLayer(askEnd);
                askEnd.IsUpdated = false;

                for (int i = 0; i < 15; i++)
                {
                    foreach (asd.DrawnObject2D item in askEnd.Objects)
                    {
                        item.Color = new asd.Color(255, 255, 255, (int)(255f * i / 15));
                    }
                    yield return 0;
                }
                askEnd.IsUpdated = true;

                while (!askEnd.IsEnd) yield return 0;

                if (askEnd.IsYes) (Layer.Scene as GameScene)?.End();

                for (int i = 0; i < 15; i++)
                {
                    blur.Intensity = 3f - i * 0.2f;
                    foreach (asd.DrawnObject2D item in askEnd.Objects)
                    {
                        item.Color = new asd.Color(255, 255, 255, (int)(255f - 255f * i / 15));
                    }
                    yield return 0;
                }

                askEnd.Dispose();
            }
            else
            {
                askSave.Dispose();
                for (int i = 0; i < 15; i++)
                {
                    blur.Intensity = 3f - i * 0.2f;
                    yield return 0;
                }
            }

            foreach (var item in Layer.Objects.Where(obj => !(obj is MapEvent.MapEvent)))
            {
                item.IsUpdated = true;
            }
            Layer.ClearPostEffects();
            IsActive = false;
            Layer.Scene.HDRMode = false;
            yield return 0;
        }

        protected override void OnUpdate()
        {
            if (enumerator?.MoveNext() ?? false) return;
            var mainLayer = Layer as MapLayer;
            if (mainLayer != null && Input.GetInputState(Inputs.A) == 1 && (mainLayer.Player.Position - Position).Length < 40) OpenSaveMenu();
            base.OnUpdate();
        }
    }
}
