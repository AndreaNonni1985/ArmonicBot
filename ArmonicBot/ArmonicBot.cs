using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.NetworkInformation;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class ArmonicBot : Robot
    {
        [Parameter(DefaultValue = 120, Group = "Scanner", MinValue = 10, MaxValue = 200, Step = 1)]
        public int Periods { get; set; }
        [Parameter(DefaultValue = false, Group = "Scanner")]
        public bool DrawSwing { get; set; }
        [Parameter("SubPeriods", DefaultValue = 96, Group = "Scanner", MinValue = 96, MaxValue = 192, Step = 1)]
        public int SubPeriods { get; set; }

        public ArmonicFinderEngine armonicFinder;
        public List<ArmonicFinderEngine> multipleFinder;
        public List<ArmonicPattern> Patterns;
        public GUI userInterface;

        protected override void OnStart()
        {
            Debug.sender = this;

            GlobalParameter.DrawSwing = DrawSwing;
            GlobalParameter.SubPeriods = SubPeriods;
            GlobalParameter.Periods = Periods;

            userInterface = new GUI(Chart, Watchlists);
            userInterface.OnClickStart += OnFindStart;

            armonicFinder = new ArmonicFinderEngine(MarketData, Symbol, TimeFrame, Chart, Periods, true);
            armonicFinder.Initialize(OnEngineLoaded, OnEngineLoading);
            armonicFinder.onPatternStateChanged += ManagePattern;

            userInterface.LoadingBar.Value = 0;
            userInterface.LoadingBar.MaxValue = 100;
            userInterface.LoadingBar.IsVisible = true;

            multipleFinder = new List<ArmonicFinderEngine>();
            Patterns = new List<ArmonicPattern>();
        }

        private void ManagePattern(ArmonicPattern pattern, PatternEvent e)
        {
            switch (e)
            {
                case PatternEvent.Add:
                    Patterns.Add(pattern);
                    userInterface.AddResult(pattern);
                    break;
                case PatternEvent.Remove:
                    Patterns.Remove(pattern);
                    userInterface.DeleteResult(pattern);
                    break;
                default:

                    Patterns.First(args => args.GetKey() == pattern.GetKey()).Update(pattern);

                    switch (e)
                    {
                        case PatternEvent.Compleated:

                            break;
                        case PatternEvent.Closed:

                            break;
                        case PatternEvent.Target1:

                            break;
                        case PatternEvent.Target2:

                            break;

                    }
                    break;

            }
        }
        protected override void OnTick()
        {
            if (armonicFinder != null)
                armonicFinder.On_NewTick();
        }
        protected override void OnBar()
        {
            if (armonicFinder != null)
                armonicFinder.On_NewBar();
        }
        protected override void OnStop()
        {

        }
        protected override void OnTimer()
        {
            base.OnTimer();
        }
        protected void OnFindStart()
        {
            foreach(GUI.WatchListItem WLItem in userInterface.WatchlistItems.Where(obj => obj.Selected)) {
                foreach(GUI.SymbolItem SYItem in WLItem.SymbolItems.Where(obj => obj.Selected)) {
                    foreach(GUI.TimeFrameItem TFItem in userInterface.TimeFrameItems.Where(obj => obj.Selected)) {
                        ArmonicFinderEngine tmpEngine = new ArmonicFinderEngine(MarketData, Symbols.GetSymbol(SYItem.SymbolName), TFItem.TimeFrame, Chart, Periods);
                        if (!multipleFinder.Exists(engine => engine.Key == tmpEngine.Key) ) {
                            tmpEngine.Initialize(OnEngineLoaded, OnEngineLoading);
                            tmpEngine.onPatternStateChanged += ManagePattern;
                            multipleFinder.Add(tmpEngine);
                        }   
                    }
                }
            }
        }
        protected void OnOtherSymbolBar(BarOpenedEventArgs e)
        {
            Print("New Bar Opened At {0}  Incoming From Symbol {1} in TimeFrame {2}", e.Bars.LastBar.OpenTime.ToString("dd/MM/yyyy:HHmmss"), e.Bars.SymbolName, e.Bars.TimeFrame.ToString());
        }
        protected void OnEngineLoading(ArmonicFinderEngine sender, double percentage)
        {
            Debug.Print("Loading Data for Symbol {1} : {0}% ...", percentage, sender.MainData.BarsData.SymbolName);
            userInterface.LoadingBar.Value = Convert.ToInt32(percentage);
        }
        protected void OnEngineLoaded(ArmonicFinderEngine sender)
        {
            Debug.Print("Loading Finisced for Symbol {0}.", sender.MainData.BarsData.SymbolName);
            userInterface.LoadingBar.IsVisible = false;
        }
    }
}
