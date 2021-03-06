﻿using asd;
using BaseComponent;
using InspectorModel;
using SatCore.MapEditor.Object.MapEvent;
using SatIO.MapEventIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SatCore.CharacterImageEditor
{
    /// <summary>
    /// 編集できるキャラクタグラフィック
    /// </summary>
    public class EditableCharacterImage : CharacterImage, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private DiffImage _selectedDiff;

        [RootPathBinding("root")]
        public string RootPath => Config.Instance.RootPath;

        [TextInput("名前")]
        public new string Name
        {
            get => base.Name;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                base.Name = value;
                OnPropertyChanged();
            }
        }

        [FileInput("キャラ画像", "PNG File|*.png", "root")]
        public new string BaseImagePath
        {
            get => base.BaseImagePath;
            set
            {
                if (!asd.Engine.File.Exists(value)) return;
                UndoRedoManager.ChangeProperty(this, value);
                base.BaseImagePath = value;
                BaseImage = TextureManager.LoadTexture(value);
                Texture = BaseImage;
                OnPropertyChanged();
            }
        }

        [ListInput("差分")]
        public new UndoRedoCollection<DiffImage> DiffImages { get; }

        [SelectedItemBinding("差分")]
        public DiffImage SelectedDiff
        {
            get => _selectedDiff;
            set
            {
                if (value == null) return;
                _selectedDiff = value;
                diffObject.Texture = value.Texture;
            }
        }

        [AddButtonMethodBinding("差分")]
        public void AddDiff()
        {
            DiffImages.Add(new DiffImage());
        }

        [RemoveButtonMethodBinding("差分")]
        public void RemoveDiff(DiffImage diffImage)
        {
            DiffImages.Remove(diffImage);
        }

        public Texture2D BaseImage { get; private set; }

        asd.TextureObject2D diffObject;

        public EditableCharacterImage()
        {
            DiffImages = new UndoRedoCollection<DiffImage>();
            diffObject = new asd.TextureObject2D();
            diffObject.DrawingPriority = 2;
            AddDrawnChild(diffObject, asd.ChildManagementMode.RegistrationToLayer | asd.ChildManagementMode.Disposal, asd.ChildTransformingMode.All, asd.ChildDrawingMode.Nothing);
        }

        public EditableCharacterImage(string path)
        {
            DiffImages = new UndoRedoCollection<DiffImage>();
            diffObject = new asd.TextureObject2D();
            AddDrawnChild(diffObject, asd.ChildManagementMode.RegistrationToLayer | asd.ChildManagementMode.Disposal, asd.ChildTransformingMode.All, asd.ChildDrawingMode.Nothing);
        }

        public async Task LoadCharacterImageIOAsync(string path)
        {
            var characterImage = await CharacterImageIO.LoadAsync<CharacterImageIO>(path);
            Name = characterImage.Name;
            BaseImagePath = characterImage.BaseImagePath;
            foreach (var item in characterImage.DiffImagePaths.Select(obj =>
            {
                return new DiffImage()
                {
                    Path = obj.Value,
                    Name = obj.Key,
                };
            }))
                DiffImages.Add(item);
        }

        public static explicit operator CharacterImageIO(EditableCharacterImage characterImage)
        {

            var result = new CharacterImageIO()
            {
                Name = characterImage.Name,
                BaseImagePath = characterImage.BaseImagePath,
                DiffImagePaths = new SatIO.SerializableDictionary<string, string>(characterImage.DiffImages.ToDictionary(obj => obj.Name, obj => obj.Path)),
            };
            return result;
        }

        /// <summary>
        /// 差分画像
        /// </summary>
        public new class DiffImage : IListInput, INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            private string _path;
            private string _name;

            [RootPathBinding("root")]
            public string RootPath => Config.Instance.RootPath;

            [FileInput("差分画像", "PNG File|*.png", "root")]
            public string Path
            {
                get => _path;
                set
                {
                    if (!asd.Engine.File.Exists(value)) return;
                    UndoRedoManager.ChangeProperty(this, value);
                    _path = value;
                    Texture = TextureManager.LoadTexture(value);
                    OnPropertyChanged();
                }
            }

            [TextInput("名前")]
            public string Name
            {
                get => _name;
                set
                {
                    UndoRedoManager.ChangeProperty(this, value);
                    _name = value;
                    OnPropertyChanged();
                }
            }

            public asd.Texture2D Texture { get; set; }

            public DiffImage()
            {
                _path = "";
                Name = "";
            }
        }
    }
}
