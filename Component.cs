using LiveSplit.UI.Components;
using System;
using LiveSplit.Model;
using LiveSplit.UI;
using System.Xml;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;

namespace LiveSplit.NetControlClient
{
    class Component : LogicComponent
    {
        public override string ComponentName => "NetControlClient";

        private LiveSplitState State;
        private Connection Connection = new Connection();

        private Stopwatch Stopwatch = Stopwatch.StartNew();
        private long TenthsOfSecondsPassed = 0;

        public Component(LiveSplitState state)
        {
            State = state;
            State.OnStart += State_OnStart;
            State.OnSplit += State_OnSplit;
            State.OnUndoSplit += State_OnUndoSplit;
            State.OnPause += State_OnPause;
            State.OnResume += State_OnResume;
            State.OnUndoAllPauses += State_OnUndoAllPauses;
            State.OnReset += State_OnReset;

            ContextMenuControls = new Dictionary<string, Action>();
            ContextMenuControls.Add("Connect to SourceRuns", Connect);
        }

        public override void Dispose()
        {
            State.OnStart -= State_OnStart;
            State.OnSplit -= State_OnSplit;
            State.OnUndoSplit -= State_OnUndoSplit;
            State.OnPause -= State_OnPause;
            State.OnResume -= State_OnResume;
            State.OnUndoAllPauses -= State_OnUndoAllPauses;
            State.OnReset -= State_OnReset;
        }

        public override void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            var passed = Stopwatch.Elapsed.Seconds / 10;
            if (TenthsOfSecondsPassed != passed)
            {
                TenthsOfSecondsPassed = passed;

                var time = state.CurrentTime.RealTime ?? TimeSpan.Zero;
                Connection.SendCurrentTime(time);
            }
        }

        private void Connect()
        {
            string password = "";
            if (InputBox.Show("Connect to SourceRuns", "Please enter the password:", ref password) == DialogResult.OK)
            {
                switch (Connection.Connect(password))
                {
                    case Connection.Result.Success:
                        MessageBox.Show("Connection established", "Connect to SourceRuns");
                        break;
                    case Connection.Result.WrongPassword:
                        MessageBox.Show("Wrong password", "Connect to SourceRuns");
                        break;
                    case Connection.Result.Error:
                        MessageBox.Show("Connection error", "Connect to SourceRuns");
                        break;
                    case Connection.Result.ConnectionTimeout:
                        MessageBox.Show("Connection error: timeout", "Connect to SourceRuns");
                        break;
                }
            }
        }

        private void State_OnStart(object sender, EventArgs e)
        {
            Connection.SendStart(State.Run.Offset);
        }

        private void State_OnSplit(object sender, EventArgs e)
        {
            // The timer on the other end has only one split.
            if (State.CurrentPhase == TimerPhase.Ended)
                Connection.SendSplit();
        }

        private void State_OnUndoSplit(object sender, EventArgs e)
        {
            Connection.SendUnsplit();
        }

        private void State_OnPause(object sender, EventArgs e)
        {
            Connection.SendPause();
        }

        private void State_OnResume(object sender, EventArgs e)
        {
            Connection.SendResume();
        }

        private void State_OnUndoAllPauses(object sender, EventArgs e)
        {
            Connection.SendUndoAllPauses();
        }

        private void State_OnReset(object sender, TimerPhase value)
        {
            Connection.SendReset();
        }

        public override XmlNode GetSettings(XmlDocument document) => document.CreateElement("Settings");
        public override Control GetSettingsControl(LayoutMode mode) => null;
        public override void SetSettings(XmlNode settings) {}
    }
}
