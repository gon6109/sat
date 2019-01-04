using BaseComponent;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace SatCore
{
    public class PlayersListDialog
    {
        public static Func<PlayersListDialog, PlayersListDialogResult> ShowDialogFunc { get; set; } = DefaultFunc;

        static PlayersListDialogResult DefaultFunc(PlayersListDialog playersListDialog)
        {
            return PlayersListDialogResult.Close;
        }

        public static void CheckPlayersList(string root)
        {
            try
            {
                var paths = Directory.GetFiles(root + "Player/", "*.pd");

                List<string> playersList = new List<string>(paths.Select(obj => Path.GetRelativePath(obj, root)));
                using (FileStream listFile = new FileStream(root + "Player/PlayersList.dat", FileMode.Create))
                {
                    BinaryFormatter serializer = new BinaryFormatter();
                    serializer.Serialize(listFile, playersList);
                }
            }
            catch (Exception e)
            {
                ErrorIO.AddError(e);
            }
        }

        public static Dictionary<string, SatIO.PlayerIO> GetPlayersData()
        {
            BinaryFormatter serializser = new BinaryFormatter();
            var playerDataPaths = (List<string>)serializser.Deserialize(IO.GetStream(ListSourceFile));
            Dictionary<string, SatIO.PlayerIO> playerDatas = new Dictionary<string, SatIO.PlayerIO>();
            foreach (var item in playerDataPaths)
            {
                playerDatas.Add(item, SatIO.BaseIO.Load<SatIO.PlayerIO>(item));
            }
            return playerDatas;
        }

        public static string ListSourceFile { get; set; } = "Player/PlayersList.dat";
        public string FileName { get; set; }

        public List<string> PlayerNames { get; private set; }
        public string PlayerName { private get; set; }

        public PlayersListDialog()
        {
            FileName = "";
        }

        public PlayersListDialogResult Show()
        {
            var playerDatas = GetPlayersData();
            PlayerNames = playerDatas.Select(obj => obj.Value.Name).ToList();

            var result = ShowDialogFunc(this);
            if (result != PlayersListDialogResult.OK) return result;

            FileName = playerDatas.First(obj => obj.Value.Name == PlayerName).Key;

            return result;
        }
    }

    public enum PlayersListDialogResult
    {
        OK,
        Cancel,
        Close,
    }
}
