﻿using BaseComponent;
using InspectorModel;
using SatIO.MapEventIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore.MapEditor.Object.MapEvent
{
    /// <summary>
    /// キャラクターグラフィック
    /// </summary>
    public class CharacterImage : asd.TextureObject2D, IListInput
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string BaseImagePath { get; set; }

        public ObservableCollection<DiffImage> DiffImages { get; set; }

        public CharacterImage()
        {
            DiffImages = new ObservableCollection<DiffImage>();
            Path = "";
            BaseImagePath = "";
        }

        public static async Task<CharacterImage> LoadCharacterImageAsync(string path)
        {
            CharacterImageIO characterImageIO = await CharacterImageIO.LoadAsync<CharacterImageIO>(path);
            CharacterImage characterImage = (CharacterImage)characterImageIO;
            characterImage.Path = path;
            return characterImage;
        }

        public static explicit operator CharacterImageIO(CharacterImage characterImage)
        {
            var result = new CharacterImageIO()
            {
                Name = characterImage.Name,
                BaseImagePath = characterImage.BaseImagePath,
                DiffImagePaths = new SatIO.SerializableDictionary<string, string>(characterImage.DiffImages.ToDictionary(obj => obj.Name, obj => obj.Path)),
            };
            return result;
        }

        public static explicit operator CharacterImage(CharacterImageIO characterImage)
        {
            try
            {
                var result = new CharacterImage()
                {
                    Name = characterImage.Name,
                    BaseImagePath = characterImage.BaseImagePath,
                    DiffImages = new ObservableCollection<DiffImage>(characterImage.DiffImagePaths.Select(obj => new DiffImage()
                    {
                        Name = obj.Key,
                        Path = obj.Value
                    })),
                };
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public class DiffImage : IListInput
        {

            public string Path { get; set; }

            public string Name { get; set; }

            public DiffImage()
            {
                Path = "";
                Name = "";
            }
        }
    }
}
