using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.NetworkInformation;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots {


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
        public ArmonicMultiFinder multiFinder;

        protected override void OnStart()
        {
            Debug.sender = this;

            GlobalParameter.DrawSwing = DrawSwing;
            GlobalParameter.SubPeriods = SubPeriods;
            GlobalParameter.Periods = Periods;

            armonicFinder = new ArmonicFinderEngine(MarketData, Symbol, TimeFrame, Chart, true);

            multiFinder = new ArmonicMultiFinder(MarketData,Watchlists,Symbols,Chart);
            multiFinder.SetMainEngine(armonicFinder);
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
        //protected override void OnTimer()
        //{
        //    base.OnTimer();
        //}
        
    }
}
