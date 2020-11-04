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
        public GUI userInterface;

        //public DataFlow testFlow;
        //public DataFlow testFlow2;
        protected override void OnStart()
        {
            Debug.sender = this;

            GlobalParameter.DrawSwing = DrawSwing;
            GlobalParameter.SubPeriods = SubPeriods;
            GlobalParameter.Periods = Periods;

            //testFlow = new DataFlow(MarketData, Symbols.GetSymbol("EURGBP"), TimeFrame.Minute);
            //testFlow.RequestBars(99999, true, OnDataLoaded, OnDataLoading);

            //testFlow2 = new DataFlow(MarketData, Symbols.GetSymbol("EURJPY"), TimeFrame.Minute2);
            //testFlow2.RequestBars(99999, true, OnDataLoaded, OnDataLoading);
            userInterface = new GUI(Chart, Watchlists);
            userInterface.OnClickStart += OnFindStart;


        }
        protected override void OnTick()
        {
            //armonicFinder.FineCalculate(false, Bid, Bars.Last(0).OpenTime);
        }
        protected override void OnBar()
        {

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
            armonicFinder = new ArmonicFinderEngine(MarketData, Symbol, TimeFrame, Chart, Periods);
            armonicFinder.Initialize(OnEngineLoaded, OnEngineLoading);
            userInterface.LoadingBar.Value = 0;
            userInterface.LoadingBar.MaxValue = 100;
            userInterface.LoadingBar.IsVisible = true;
        }

        protected void OnDataLoading(double percentage, int count, string symbol)
        {
            Print("Loading Bars for Symbol {2} : {0}% ({1} Bars) ", percentage, count, symbol);
        }
        protected void OnDataLoaded(DataFlow sender)
        {
            Print("Bars Loaded For Symbol {0}", sender.BarsData.SymbolName);
            if (sender.BarsData.SymbolName != Symbol.Name)
            {
                sender.TraceNewBar(OnOtherSymbolBar);
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
