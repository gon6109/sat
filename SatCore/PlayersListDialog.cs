using BaseComponent;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace SatCore
{
    /// <summary>
    /// プレイアブルキャラ選択ダイアログモデル
    /// </summary>
    public class PlayersListDialog
    {
        public static Func<PlayersListDialog, PlayersListDialogResult> ShowDialogFunc { get; set; } = DefaultFunc;

        static PlayersListDialogResult DefaultFunc(PlayersListDialog playersListDialog)
        {
            return PlayersListDialogResult.Close;
        }

        /// <summary>
        /// プレイヤーリストを構築
        /// </summary>
        /// <param name="root">ルートディレクトリ</param>
        public static void CheckPlayersList(string root)
        {
            try
            {
                var paths = Directory.GetFiles(root + "Player/", "*.pd");

                List<string> playersList = new List<string>(paths.Select(obj => Path.GetRelativePath(obj, root)));
                using (FileStream listFile = new FileStream(root + "Player/PlayersList.dat", FileMode.Create))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<string>));
                    serializer.Serialize(listFile, playersList);
                }
            }
            catch (Exception e)
            {
                ErrorIO.AddError(e);
            }
        }

        /// <summary>
        /// プレイヤー情報を得る
        /// </summary>
        /// <returns>キーがプレイヤー名、バリューがパスの辞書</returns>
        public static Dictionary<string, string> GetPlayersScriptPath()
        {
            try
            {
                Dictionary<string, string> result = new Dictionary<string, string>();

                using (var stream = IO.GetStream(ListSourceFile))
                {
                    XmlSerializer serializser = new XmlSerializer(typeof(List<string>));
                    var paths = ((List<string>)serializser.Deserialize(stream));

                    Regex regex = new Regex(@";[\s|\n]*Name[\s|\n]*=""(.*)"";");
                    foreach (var item in paths)
                    {
                        using (var scriptStream = IO.GetStream(item))
                        {
                            var matches = regex.Matches(scriptStream.ToString());
                            var name = matches.OfType<Match>().LastOrDefault()?.Groups[1].Value;
                            result.Add(name ?? "", item);
                        }
                    }
                }
                return result;
            }
            catch
            {
                throw;
            }
        }

        public static string ListSourceFile { get; set; } = "Player/PlayersList.dat";
        public string FileName { get; set; }

        public List<string> PlayerNames { get; private set; }
        public string PlayerName { get; set; }

        public PlayersListDialog()
        {
            FileName = "";
        }

        /// <summary>
        ///　ダイアログを表示する
        /// </summary>
        /// <returns>終了状態</returns>
        public PlayersListDialogResult Show()
        {
            try
            {
                var playerDatas = GetPlayersScriptPath();
                PlayerNames = playerDatas.Select(obj => obj.Key).ToList();

                var result = ShowDialogFunc(this);
                if (result != PlayersListDialogResult.OK) return result;

                FileName = playerDatas.First(obj => obj.Key == PlayerName).Value;

                return result;
            }
            catch (Exception e)
            {
                ErrorIO.AddError(e);
                return PlayersListDialogResult.Cancel;
            }
        }
    }

    public enum PlayersListDialogResult
    {
        OK,
        Cancel,
        Close,
    }
}
