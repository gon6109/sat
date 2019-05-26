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

        public static async Task<CharacterImage> CreateCharacterImageAsync(string path)
        {
            var characterImageIO = await CharacterImageIO.LoadAsync<CharacterImageIO>(path);
            CharacterImage characterImage = new CharacterImage();
            characterImage.Texture = await TextureManager.LoadTextureAsync(characterImageIO.BaseImagePath);
            characterImage.Name = characterImageIO.Name;
            characterImage.DiffImages = new Dictionary<string, asd.Texture2D>();
            foreach (var item in characterImageIO.DiffImagePaths)
            {
                var texture = await TextureManager.LoadTextureAsync(item.Value);
                characterImage.DiffImages.Add(item.Key, texture);
            }
            if (characterImage.DiffImages.Count > 0) characterImage.SelectedDiff = characterImage.DiffImages.First().Key;
            return characterImage;
        }
    }
}
