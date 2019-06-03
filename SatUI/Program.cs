using BaseComponent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;

namespace SatUI
{
    static class Program
    {
        static Dictionary<Inputs, bool> ButtonStates;
        static Dictionary<Inputs, bool> PreButtonStates;

        public static MapEditor MainWindow;
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            ButtonStates = new Dictionary<Inputs, bool>();
            foreach (Inputs item in Enum.GetValues(typeof(Inputs))) ButtonStates[item] = false;
            PreButtonStates = new Dictionary<Inputs, bool>();
            foreach (Inputs item in Enum.GetValues(typeof(Inputs))) PreButtonStates[item] = false;
            Input.GetButtonState = GetButtonState;
            SatCore.PlayersListDialog.ShowDialogFunc = ShowPlayersListDialog;

            bool closed = false;
            MainWindow = new MapEditor();
            MainWindow.Closed += (object sender, EventArgs e) =>
            {
                closed = true;
                using (FileStream data = new FileStream("init.config", FileMode.Create))
                {
                    BinaryFormatter serializer = new BinaryFormatter();
                    serializer.Serialize(data, MainWindow.PlayerExePath);
                }
            };
            MainWindow.EditorPanel.Child.MouseWheel += Child_MouseWheel;
            MainWindow.Show();

            asd.Engine.InitializeByExternalWindow(MainWindow.EditorPanel.Child.Handle, System.IntPtr.Zero, (int)MainWindow.EditorPanel.RenderSize.Width, (int)MainWindow.EditorPanel.RenderSize.Height, new asd.EngineOption()
            {
                IsReloadingEnabled = true
            });
            BinaryFormatter deserializer = new BinaryFormatter();
            try
            {
                MainWindow.PlayerExePath = (string)deserializer.Deserialize(IO.GetStream("init.config"));
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
#if DEBUG
            asd.Engine.ProfilerIsVisible = true;
#endif
            while (asd.Engine.DoEvents())
            {
                SatCore.Debug.PrintTimeWithFlag("asd.Engine.DoEvents ");
                KeyInput();
                MouseInput();
                Input.UpdateInput();
                MainWindow.DoEvents();
                System.Windows.Forms.Application.DoEvents();
                if (closed) break;

                asd.Engine.WindowSize = new asd.Vector2DI((int)MainWindow.EditorPanel.RenderSize.Width, (int)MainWindow.EditorPanel.RenderSize.Height);
                SatCore.Debug.ResetTime();
                asd.Engine.Update();
                SatCore.Debug.PrintTimeWithFlag("asd.Engine.Update ");
                SatCore.Debug.AddCount("Update");
                SatCore.Debug.ResetTime();
            }

            Logger.Save("latest.log");

            asd.Engine.Terminate();
        }

        private static void Child_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            SatCore.Mouse.MouseWheel = (e.Delta / 120);
        }

        static void MouseInput()
        {
            var source = PresentationSource.FromVisual(MainWindow.EditorPanel);
            var pos = MainWindow.EditorPanel.Child.PointToClient(System.Windows.Forms.Cursor.Position);
            SatCore.Mouse.Position = new asd.Vector2DF(pos.X / (float)source.CompositionTarget.TransformToDevice.M11,
                pos.Y / (float)source.CompositionTarget.TransformToDevice.M22);
            SatCore.Mouse.MouseWheel = 0;
            if (!new System.Drawing.Rectangle(new System.Drawing.Point(), MainWindow.EditorPanel.Child.Size).Contains(pos) || !MainWindow.EditorPanel.IsFocused || !MainWindow.IsActive)
            {
                SatCore.Mouse.IsLeftButton = false;
                SatCore.Mouse.IsRightButton = false;
                SatCore.Mouse.IsMiddleButton = false;
            }
            else
            {
                SatCore.Mouse.IsLeftButton = (Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left;
                SatCore.Mouse.IsRightButton = (Control.MouseButtons & MouseButtons.Right) == MouseButtons.Right;
                SatCore.Mouse.IsMiddleButton = (Control.MouseButtons & MouseButtons.Middle) == MouseButtons.Middle;
            }
        }

        static void KeyInput()
        {
            foreach (Inputs item in Enum.GetValues(typeof(Inputs)))
            {
                Key key;
                switch (item)
                {
                    case Inputs.Left:
                        key = Key.Left;
                        break;
                    case Inputs.Right:
                        key = Key.Right;
                        break;
                    case Inputs.Up:
                        key = Key.Up;
                        break;
                    case Inputs.A:
                        key = Key.Z;
                        break;
                    case Inputs.B:
                        key = Key.LeftShift;
                        break;
                    case Inputs.Esc:
                        key = Key.Escape;
                        break;
                    case Inputs.Down:
                        key = Key.Down;
                        break;
                    default:
                        key = Key.LeftShift;
                        break;
                }
                PreButtonStates[item] = ButtonStates[item];
                ButtonStates[item] = Keyboard.IsKeyDown(key);
            }
        }

        static asd.ButtonState GetButtonState(Inputs inputs)
        {
            if (!PreButtonStates[inputs] && ButtonStates[inputs]) return asd.ButtonState.Push;
            if (PreButtonStates[inputs] && ButtonStates[inputs]) return asd.ButtonState.Hold;
            if (PreButtonStates[inputs] && !ButtonStates[inputs]) return asd.ButtonState.Release;
            if (!PreButtonStates[inputs] && !ButtonStates[inputs]) return asd.ButtonState.Free;
            return asd.ButtonState.Free;
        }

        static SatCore.PlayersListDialogResult ShowPlayersListDialog(SatCore.PlayersListDialog playersListDialog)
        {
            var dialog = new PlayersListDialogUI(playersListDialog);
            dialog.ShowDialog();
            return dialog.Result;
        }
    }
}
