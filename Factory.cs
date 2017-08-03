using System;
using System.Reflection;
using LiveSplit.Model;
using LiveSplit.UI.Components;

[assembly: ComponentFactory(typeof(LiveSplit.NetControlClient.Factory))]

namespace LiveSplit.NetControlClient
{
    public class Factory : IComponentFactory
    {
        public ComponentCategory Category => ComponentCategory.Control;
        public string ComponentName => "NetControlClient";
        public string Description => "Sends LiveSplit timer events to the SourceRuns server.";

        public string UpdateName => ComponentName;
        public string UpdateURL => "http://play.sourceruns.org/NetControlClient/";
        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public string XMLURL => UpdateURL + "updates.xml";

        public IComponent Create(LiveSplitState state) => new Component(state);
    }
}
