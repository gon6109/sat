using BaseComponent;
using SatIO.MapEventIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer.Game.Object.MapEvent
{
    /// <summary>
    /// キャラクターグラフィック
    /// </summary>
    public class CharacterImage : asd.TextureObject2D
    {

        private string _selectedDiff;

        public string Name { get; set; }

        public Dictionary<string, asd.Texture2D> DiffImages { get; set; }

        public string SelectedDiff
        {
            get => _selectedDiff;
            set
            {
                if (value == null) return;
                _selectedDiff = value;
                diffObject.Texture = DiffImages[value];
            }
        }
        asd.TextureObject2D diffObject;

        public CharacterImage()
        {
            diffObject = new asd.TextureObject2D();
            diffObject.DrawingPriority = 2;
            AddDrawnChild(diffObject, asd.ChildManagementMode.RegistrationToLayer | asd.ChildManagementMode.Disposal, asd.ChildTransformingMode.All, asd.ChildDrawingMode.Nothing);
            DiffImages = new Dictionary<string, asd.Texture2D>();
        }

        public static CharacterImage LoadCharacterImage(string path)
        {
            var characterImageIO = CharacterImageIO.LoadCharacterImage(path);
            CharacterImage characterImage = new CharacterImage();
            characterImage.Texture = TextureManager.LoadTexture(characterImageIO.BaseImagePath);
            characterImage.Name = characterImageIO.Name;
            characterImage.DiffImages = characterImageIO.DiffImagePaths.ToDictionary(obj => obj.Key, obj => TextureManager.LoadTexture(obj.Value));
            if (characterImage.DiffImages.Count > 0) characterImage.SelectedDiff = characterImage.DiffImages.First().Key;
            return characterImage;
        }
    }
}
