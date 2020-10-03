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
        public int MaxPatternPeriods { get; set; }

        [Parameter(DefaultValue = 120, Group = "Scanner", MinValue = 0, MaxValue = 500, Step = 1)]
        public int InitialPeriods { get; set; }

        [Parameter(DefaultValue = false, Group = "Scanner")]
        public bool DrawSwing { get; set; }

        [Parameter("MicroBars", DefaultValue = 96, Group = "Scanner", MinValue = 96, MaxValue = 192, Step = 1)]
        public int MicroBarsInBarForFineCalculate { get; set; }

        [Parameter(DefaultValue = EnterMode.Manual, Group = "Action")]
        public EnterMode Mode { get; set; }

        [Parameter("Enabled", DefaultValue = EnterMode.Manual, Group = "Multi-Symbol")]
        public bool MultiSymbolEnabled { get; set; }
        [Parameter("Watchlist", Group = "Multi-Symbol")]
        public String MultiSymbolWatchListName { get; set; }

        [Parameter("TimeFrame", DefaultValue = 8, Group = "Multi-Symbol", Step = 1)]
        public TimeFrame MultiSymbolTimeFrame { get; set; }

        public EnterMode EnterMode;
        public SegmentTracerEngine segmentTracer;
        public ArmonicFinderEngine armonicFinder;

        public Button button;
        public Panel panel;

        protected override void OnStart()
        {
            DateTime FromDate, ToDate;
            int MinutesFineCalc;
            TimeFrame TFFineCalc;
            
            segmentTracer = new SegmentTracerEngine(this);
            armonicFinder = new ArmonicFinderEngine(this);

            //Timer.Start(TimeSpan.FromMilliseconds(50));
            button = new Button() {
                Text = "<Nome del Pattern...>",
                Margin = 9
            };
            button.Click += OnButtonClick;

            panel = new StackPanel() {
                Width = 120,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            
            panel.AddChild(button);
            Chart.AddControl(panel);
            if (InitialPeriods > 0 && !IsBacktesting)
            {
                //Disabilito l'entrata
                EnterMode = EnterMode.None;

                Bars BarsCalculate = MarketData.GetBars(TimeFrame);
                Print("Gross Data - Load - Bar Count:{0}   From:{1}   To:{2}", BarsCalculate.Count(), BarsCalculate.First().OpenTime, BarsCalculate.Last().OpenTime);
                while (BarsCalculate.Count() < InitialPeriods)
                {
                    BarsCalculate.LoadMoreHistory();
                    Print("Gross Data - Load More History - Bar Count:{0}   From:{1}   To:{2}", BarsCalculate.Count(), BarsCalculate.First().OpenTime, BarsCalculate.Last().OpenTime);
                }

                MinutesFineCalc = TimeframeToMinutes(TimeFrame);
                TFFineCalc = MinutesToTimeFrame(MinutesFineCalc / MicroBarsInBarForFineCalculate);

                Bars BarsFineCalculate = MarketData.GetBars(TFFineCalc);
                Print("Fine Data - Load - Bar Count:{0}   From:{1}   To:{2}", BarsFineCalculate.Count(), BarsFineCalculate.First().OpenTime, BarsFineCalculate.Last().OpenTime);
                while (BarsFineCalculate.First().OpenTime > BarsCalculate.Last(InitialPeriods).OpenTime)
                {
                    String dbg = String.Format("ToReach : {0}     first {1} last{2}", BarsCalculate.Last(InitialPeriods).OpenTime, BarsFineCalculate.First().OpenTime, BarsFineCalculate.Last().OpenTime);
                    BarsFineCalculate.LoadMoreHistory();
                    Print("Fine Data - Load More History- Bar Count:{0}   From:{1}   To:{2}", BarsFineCalculate.Count(), BarsFineCalculate.First().OpenTime, BarsFineCalculate.Last().OpenTime);
                }

                if (BarsCalculate.Count() > InitialPeriods)
                {
                    for (int _index = InitialPeriods; _index > 1; _index--)
                    {
                        segmentTracer.Calculate(BarsCalculate.Last(_index - 1), BarsCalculate.Last(_index), false);
                        armonicFinder.Calculate();
                        if (_index - 2 > 0)
                        {
                            FromDate = BarsCalculate.Last(_index - 2).OpenTime;
                            if (_index - 3 > 0)
                            {
                                ToDate = BarsCalculate.Last(_index - 3).OpenTime;
                            }
                            else
                            {
                                ToDate = Time;
                            }

                            string dbg = string.Format("{0} Nested Bars in {1} TF", BarsFineCalculate.Where(data => (data.OpenTime >= FromDate) && (data.OpenTime <= ToDate)).Count(), MicroBarsInBarForFineCalculate.ToString());

                            foreach (Bar FineBar in BarsFineCalculate.Where(data => (data.OpenTime >= FromDate) && (data.OpenTime <= ToDate)))
                            {
                                armonicFinder.FineCalculate(true, 0, FineBar, BarsCalculate.Last(_index - 2).OpenTime);
                            }
                        }
                    }
                }

                //Riabilito l'entrata
                EnterMode = Mode;

                //stampo le statistiche
                armonicFinder.Statistics.Print();

                //stampo i timeframe utilizzati per la ricerca dei pattern 
                string text;
                text = string.Format("Original TimeFrame {0} , Fine TimeFrame {1} \n", TimeFrame.ToString(), TFFineCalc.ToString());
                text += string.Format("PatternCount {0} ", armonicFinder.GetPatternCount());
                text += string.Format("CompleatedPatternCount {0} ", armonicFinder.GetPatternCount(true));
                text += string.Format("DrawablePatternCount {0} ", armonicFinder.GetPatternCount(false,true ));

                Chart.DrawStaticText("TIMEFRAME", text, VerticalAlignment.Bottom, HorizontalAlignment.Right, Color.Brown);

                Bar _null = new Bar();
                armonicFinder.FineCalculate(false, Bid, _null, Bars.Last(0).OpenTime);
            }


        }

        private void OnButtonClick(ButtonClickEventArgs obj) {
            foreach(Watchlist wlst in Watchlists) {
                Print(wlst.Name);

            }
        }

        protected override void OnTick()
        {
            Bar _null = new Bar();
            armonicFinder.FineCalculate(false, Bid, _null, Bars.Last(0).OpenTime);
            armonicFinder.Statistics.Print();
        }
        protected override void OnBar()
        {
            segmentTracer.Calculate(Bars.Last(1), Bars.Last(2), true);
            armonicFinder.Calculate();
        }
        protected override void OnStop()
        {

        }
        protected override void OnTimer()
        {
            base.OnTimer();


        }
        private TimeFrame MinutesToTimeFrame(int Minutes)
        {
            //return TimeFrame.Daily;

            if (Minutes >= 0 && Minutes <= 1)
                return TimeFrame.Minute;
            if (Minutes == 2)
                return TimeFrame.Minute2;
            if (Minutes == 3)
                return TimeFrame.Minute3;
            if (Minutes == 4)
                return TimeFrame.Minute4;
            if (Minutes == 5)
                return TimeFrame.Minute5;
            if (Minutes == 6)
                return TimeFrame.Minute6;
            if (Minutes == 7)
                return TimeFrame.Minute7;
            if (Minutes == 8)
                return TimeFrame.Minute8;
            if (Minutes == 9)
                return TimeFrame.Minute9;
            if (Minutes >= 10 && Minutes <= 12)
                return TimeFrame.Minute10;

            if (Minutes >= 13 && Minutes <= 17)
                return TimeFrame.Minute15;
            if (Minutes >= 18 && Minutes <= 32)
                return TimeFrame.Minute20;
            if (Minutes >= 33 && Minutes <= 37)
                return TimeFrame.Minute30;
            if (Minutes >= 38 && Minutes <= 52)
                return TimeFrame.Minute45;

            if (Minutes >= 53 && Minutes <= 90)
                return TimeFrame.Hour;
            if (Minutes >= 91 && Minutes <= 150)
                return TimeFrame.Hour2;
            if (Minutes >= 151 && Minutes <= 210)
                return TimeFrame.Hour3;
            if (Minutes >= 211 && Minutes <= 300)
                return TimeFrame.Hour4;
            if (Minutes >= 301 && Minutes <= 420)
                return TimeFrame.Hour6;
            if (Minutes >= 421 && Minutes <= 600)
                return TimeFrame.Hour8;
            if (Minutes >= 601 && Minutes <= 1080)
                return TimeFrame.Hour12;

            if (Minutes >= 1081 && Minutes <= 2160)
                return TimeFrame.Daily;
            if (Minutes >= 2061 && Minutes <= 3600)
                return TimeFrame.Day2;
            if (Minutes >= 3601 && Minutes <= 7200)
                return TimeFrame.Day3;

            if (Minutes >= 7201 && Minutes <= 26640)
                return TimeFrame.Weekly;

            if (Minutes >= 26640 && Minutes <= 43200)
                return TimeFrame.Monthly;

            return TimeFrame.Daily;
        }
        private int TimeframeToMinutes(TimeFrame MyCandle)
        {

            if (MyCandle == TimeFrame.Daily)
                return 60 * 24;
            if (MyCandle == TimeFrame.Day2)
                return 60 * 24 * 2;
            if (MyCandle == TimeFrame.Day3)
                return 60 * 24 * 3;
            if (MyCandle == TimeFrame.Hour)
                return 60;
            if (MyCandle == TimeFrame.Hour12)
                return 60 * 12;
            if (MyCandle == TimeFrame.Hour2)
                return 60 * 2;
            if (MyCandle == TimeFrame.Hour3)
                return 60 * 3;
            if (MyCandle == TimeFrame.Hour4)
                return 60 * 4;
            if (MyCandle == TimeFrame.Hour6)
                return 60 * 6;
            if (MyCandle == TimeFrame.Hour8)
                return 60 * 8;
            if (MyCandle == TimeFrame.Minute)
                return 1;
            if (MyCandle == TimeFrame.Minute10)
                return 10;
            if (MyCandle == TimeFrame.Minute15)
                return 15;
            if (MyCandle == TimeFrame.Minute2)
                return 2;
            if (MyCandle == TimeFrame.Minute20)
                return 20;
            if (MyCandle == TimeFrame.Minute3)
                return 3;
            if (MyCandle == TimeFrame.Minute30)
                return 30;
            if (MyCandle == TimeFrame.Minute4)
                return 4;
            if (MyCandle == TimeFrame.Minute45)
                return 45;
            if (MyCandle == TimeFrame.Minute5)
                return 5;
            if (MyCandle == TimeFrame.Minute6)
                return 6;
            if (MyCandle == TimeFrame.Minute7)
                return 7;
            if (MyCandle == TimeFrame.Minute8)
                return 8;
            if (MyCandle == TimeFrame.Minute9)
                return 9;
            if (MyCandle == TimeFrame.Monthly)
                return 60 * 24 * 30;
            if (MyCandle == TimeFrame.Weekly)
                return 60 * 24 * 7;

            return 0;

        }
    }
    public class ArmonicPattern
    {
        //private ArmonicBot Bot;
        public Segment XA;
        public Segment AB;
        public Segment BC;
        public Segment BC_OverA;
        public Segment CD_OverB;
        public Segment CD;
        public Segment IdealCD;
        public double AB_Percent;
        public double BC_Percent;
        public double CD_Percent;
        public int XA_Period;
        public int AB_Period;
        public int BC_Period;
        public int CD_Period;
        public PatternType Type;
        public PatternMode Mode;
        public PatternStep Step;
        public Area PRZ;
        public Area SLZ;
        public Area T1Z;
        public Area T2Z;
        public Area DrawableArea;
        public Area CPointValidArea;
        public bool CPointOverA;
        public bool DPointOverB;
        public bool Compleated;
        public bool Failed;
        public bool Succeed;
        public bool Drawable;
        public double StopLoss;
        public double Target1;
        public double Target2;
        public bool Target1Compleated;
        public bool Target2Compleated;
        public bool Closed;
        public double StopLossPips;
        public double Target1Pips;
        public double Target2Pips;
        public bool Enter;
        public double RiskReward;
        public int PositionID;
        public double EnterPrice;
        public double Volume;
        public ArmonicPattern(Segment bornSegment)
        {
            //Bot = bot;
            Reset(bornSegment);
        }
        public ArmonicPattern(ArmonicPattern PatternToCopy)
        {
            //Bot = bot;
            XA = PatternToCopy.XA;
            AB = PatternToCopy.AB;
            BC = PatternToCopy.BC;
            BC_OverA = PatternToCopy.BC_OverA;
            CD_OverB = PatternToCopy.CD_OverB;
            CD = PatternToCopy.CD;
            IdealCD = PatternToCopy.IdealCD;
            AB_Percent = PatternToCopy.AB_Percent;
            BC_Percent = PatternToCopy.BC_Percent;
            CD_Percent = PatternToCopy.CD_Percent;
            XA_Period = PatternToCopy.XA_Period;
            AB_Period = PatternToCopy.AB_Period;
            BC_Period = PatternToCopy.BC_Period;
            CD_Period = PatternToCopy.CD_Period;
            Type = PatternToCopy.Type;
            Mode = PatternToCopy.Mode;
            Step = PatternToCopy.Step;
            PRZ = PatternToCopy.PRZ;
            SLZ = PatternToCopy.SLZ;
            T1Z = PatternToCopy.T1Z;
            T2Z = PatternToCopy.T2Z;
            DrawableArea = PatternToCopy.DrawableArea;
            CPointValidArea = PatternToCopy.CPointValidArea;
            CPointOverA = PatternToCopy.CPointOverA;
            DPointOverB = PatternToCopy.DPointOverB;
            Compleated = PatternToCopy.Compleated;
            Failed = PatternToCopy.Failed;
            Succeed = PatternToCopy.Succeed;
            Drawable = PatternToCopy.Drawable;
            StopLoss = PatternToCopy.StopLoss;
            Target1 = PatternToCopy.Target1;
            Target2 = PatternToCopy.Target2;
            Target1Compleated = PatternToCopy.Target1Compleated;
            Target2Compleated = PatternToCopy.Target2Compleated;
            Closed = PatternToCopy.Closed;
            StopLossPips = PatternToCopy.StopLossPips;
            Target1Pips = PatternToCopy.Target1Pips;
            Target2Pips = PatternToCopy.Target2Pips;
            Enter = PatternToCopy.Enter;
            RiskReward = PatternToCopy.RiskReward;
            PositionID = PatternToCopy.PositionID;
            EnterPrice = PatternToCopy.EnterPrice;
            Volume = PatternToCopy.Volume;
        }
        public void Reset(Segment bornSegment)
        {

            XA = bornSegment;
            AB = null;
            BC = null;
            BC_OverA = null;
            CD_OverB = null;
            CD = null;
            IdealCD = null;
            AB_Percent = 0;
            BC_Percent = 0;
            CD_Percent = 0;
            XA_Period = 0;
            AB_Period = 0;
            BC_Period = 0;
            CD_Period = 0;
            Type = PatternType.Undefined;
            Mode = bornSegment.Direction == Direction.Up ? PatternMode.Bullish : PatternMode.Bearish;
            Step = PatternStep.BFinding;
            PRZ = null;
            SLZ = null;
            T1Z = null;
            T2Z = null;
            DrawableArea = null;
            CPointValidArea = null;
            CPointOverA = false;
            DPointOverB = false;
            Compleated = false;
            Failed = false;
            Succeed = false;
            Drawable = false;
            StopLoss = 0;
            Target1 = 0;
            Target2 = 0;
            Target1Compleated = false;
            Target2Compleated = false;
            Closed = false;
            StopLossPips = 0;
            Target1Pips = 0;
            Target2Pips = 0;
            Enter = false;
            RiskReward = 0;
            PositionID = 0;
            EnterPrice = 0;
            Volume = 0;
        }

        public string Report() {
            string result = "";
            result += string.Format("INIZIO ----------------------------------------------");
            result += string.Format("\n ID : {0}",GetKey());
            result += string.Format("\n Type : {0}", Type.ToString());
            result += string.Format("\n Mode : {0}", Mode.ToString());
            result += string.Format("\n Step : {0}", Step.ToString());
            result += string.Format("\n Compleated : {0}", Compleated.ToString());
            result += string.Format("\n Drawable : {0}", Drawable.ToString());
            result += string.Format("\n Closed : {0}", Closed.ToString());
            result += string.Format("\n Succeed : {0}", Succeed.ToString());
            result += string.Format("\n Failed : {0}", Failed.ToString());
            result += string.Format("\n Target1Reached : {0}", Target1Compleated.ToString());
            result += string.Format("\n Target2Reached : {0}", Target2Compleated.ToString());
            result += string.Format("\n XA_Period : {0}", XA_Period.ToString());
            result += string.Format("\n AB_Period : {0}", AB_Period.ToString());
            result += string.Format("\n BC_Period : {0}", BC_Period.ToString());
            result += string.Format("\n CD_Period : {0}", CD_Period.ToString());
            
            return result;
        }

        public string GetKey()
        {
            return String.Format("X{0}_", XA.FromOpenTime.ToString("dd/MM/yyyy:HHmmss"));
        }


    }
    public class Statistics
    {
        private readonly List<ArmonicPattern> Info;
        private readonly ArmonicBot Bot;

        public Statistics(ArmonicBot bot)
        {
            Bot = bot;
            Info = new List<ArmonicPattern>();

        }

        public void Add(ArmonicPattern pattern)
        {
            ArmonicPattern _info = new ArmonicPattern(pattern);

            //controllo se ho già registrato questo pattern
            bool _found = false;
            for (int _patternIndex = 0; _patternIndex < Info.Count(); _patternIndex++)
            {
                ArmonicPattern _pattern = Info[_patternIndex];
                if (_pattern.GetKey() == pattern.GetKey())
                {
                    _found = true;
                    break;
                }
            }
            //se non ho già il pattern in lista allora lo aggiungo
            if (!_found)
            {
                Info.Add(_info);
            }



        }
        public int GetCompleated(PatternType type)
        {
            int count = 0;

            foreach (ArmonicPattern pattern in Info)
            {
                if (pattern.Type == type && pattern.Compleated)
                {
                    count++;
                }
            }
            return count;
        }
        public int GetSucceed(PatternType type)
        {
            int count = 0;

            foreach (ArmonicPattern pattern in Info)
            {
                if (pattern.Type == type && pattern.Succeed)
                {
                    count++;
                }
            }
            return count;
        }
        public int GetFailed(PatternType type)
        {
            int count = 0;

            foreach (ArmonicPattern pattern in Info)
            {
                if (pattern.Type == type && pattern.Failed)
                {
                    count++;
                }
            }
            return count;
        }
        public double GetWinLossRatio(PatternType type)
        {
            int countCompleated = GetCompleated(type);

            return countCompleated > 0 ? 100 * GetSucceed(type) / countCompleated : 0;
        }
        public void Print()
        {
            string text;
            text = Bot.Time.ToString();
            Bot.Chart.DrawStaticText("TIME", text, VerticalAlignment.Top, HorizontalAlignment.Right, Color.DarkCyan);
            text = string.Concat("GARTLEY Compleated ", GetCompleated(PatternType.Gartley));
            text = string.Concat(text, "     Succees ", GetSucceed(PatternType.Gartley));
            text = string.Concat(text, "     Failed ", GetFailed(PatternType.Gartley));
            text = string.Concat(text, "     Win/Loss Ratio ", GetWinLossRatio(PatternType.Gartley));
            Bot.Chart.DrawStaticText("STAT#1", text, VerticalAlignment.Top, HorizontalAlignment.Left, Color.Blue);
            text = string.Concat("\nCYPHER Compleated ", GetCompleated(PatternType.Cypher));
            text = string.Concat(text, "     Succees ", GetSucceed(PatternType.Cypher));
            text = string.Concat(text, "     Failed ", GetFailed(PatternType.Cypher));
            text = string.Concat(text, "     Win/Loss Ratio ", GetWinLossRatio(PatternType.Cypher));
            Bot.Chart.DrawStaticText("STAT#2", text, VerticalAlignment.Top, HorizontalAlignment.Left, Color.Orange);
            text = string.Concat("\n\nBUTTERFLY Compleated ", GetCompleated(PatternType.Butterfly));
            text = string.Concat(text, "     Succees ", GetSucceed(PatternType.Butterfly));
            text = string.Concat(text, "     Failed ", GetFailed(PatternType.Butterfly));
            text = string.Concat(text, "     Win/Loss Ratio ", GetWinLossRatio(PatternType.Butterfly));
            Bot.Chart.DrawStaticText("STAT#3", text, VerticalAlignment.Top, HorizontalAlignment.Left, Color.Purple);
            text = string.Concat("\n\n\nBAT Compleated ", GetCompleated(PatternType.Bat));
            text = string.Concat(text, "     Succees ", GetSucceed(PatternType.Bat));
            text = string.Concat(text, "     Failed ", GetFailed(PatternType.Bat));
            text = string.Concat(text, "     Win/Loss Ratio ", GetWinLossRatio(PatternType.Bat));
            Bot.Chart.DrawStaticText("STAT#4", text, VerticalAlignment.Top, HorizontalAlignment.Left, Color.Gray);

        }
    }
    public class ArmonicFinderEngine
    {
        private readonly ArmonicBot Bot;
        private readonly List<ArmonicPattern> PatternList;
        public Statistics Statistics;
        public ArmonicFinderEngine(ArmonicBot bot)
        {
            Bot = bot;
            PatternList = new List<ArmonicPattern>();
            Statistics = new Statistics(bot);
        }

        public List<ArmonicPattern> Patterns() {
            return PatternList;
        }
        //public int GetPatternCount(bool compleatedOnly, bool incompletedAndDrawableOnly, bool incompleatedAndInPRZ )
        //{
        //    int count = 0;
        //    foreach (ArmonicPattern pattern in PatternList) {
        //        if (compleatedOnly && !(pattern.Compleated)) continue;
        //        if (incompletedAndDrawableOnly && !(pattern.Compleated && pattern.Drawable)) continue;
        //        //if (incompleatedAndInPRZ && !(pattern.Compleated && pattern.PRZ.InArea())) continue;
        //        count++;

        //    }
        //    return count;
        //}
        public int GetPatternCount() {
            return PatternList.Count();
        }
        public int GetPatternCount(bool compleatedOnly) {
            int count = 0;
            foreach (ArmonicPattern pattern in PatternList) {
                if (compleatedOnly && !(pattern.Compleated)) continue;
                count++;

            }
            return count;
        }
        public int GetPatternCount(bool compleatedOnly, bool drawableOnly) {
            int count = 0;
            foreach (ArmonicPattern pattern in PatternList) {
                if (compleatedOnly && !(pattern.Compleated)) continue;
                if (drawableOnly && !(pattern.Drawable)) continue;
                
                count++;
            }
            return count;
        }

        public int SegmentPeriod(Segment segment)
        {
            int _retValue;
            int _leftIndex;
            int _rightIndex;
            _leftIndex = Bot.Bars.OpenTimes.GetIndexByExactTime(segment.FromOpenTime);
            _rightIndex = Bot.Bars.OpenTimes.GetIndexByExactTime(segment.ToOpenTime);
            _retValue = _rightIndex - _leftIndex;
            return _retValue;

        }
        public double PriceRetracement(Segment segment, double retracement)
        {
            double delta = segment.Direction == Direction.Up ? segment.ToPrice - segment.FromPrice : segment.FromPrice - segment.ToPrice;
            double deltaRetracement = delta * retracement;
            double value = segment.Direction == Direction.Up ? segment.ToPrice - deltaRetracement : segment.ToPrice + deltaRetracement;

            return value;
        }
        public double MeasueSwing(Segment leftSegment, Segment rightSegment)
        {
            double a, b;
            a = rightSegment.Measure();
            b = leftSegment.Measure();
            return a / b;
        }
        public bool VerifyAndUpdateAB(ArmonicPattern pattern, Segment AB)
        {
            double _value;
            //CALCOLA IL RAPPORTO TRA XA E AB
            _value = MeasueSwing(pattern.XA, AB);
            ////PUO' SUCCEDERE CHE
            if (_value >= 1)
            {
                //B è sotto ad X : IL CALCOLO INVALIDA IL PATTERN
                //pattern.CanDraw = false;
                return false;
            }
            else if (B_InRange(_value))
            {
                //B è valido per almeno un pattern

                pattern.AB_Percent = _value;
                pattern.AB = AB;
                pattern.AB_Period = SegmentPeriod(pattern.AB);

                pattern.Step = PatternStep.CFinding;
                return true;
            }
            else
            {
                //IL CALCOLO NON SEGNALA UN OBIETTIVO, LA RICERCA DI B PROSEGUE CON IL PROSSIMO SEGMENTO 
                return true;
            }
        }

        public bool VerifyAndUpdateBC(ArmonicPattern pattern, Segment BC)
        {
            double _valueABBC;
            double _valueXAXC;

            Segment _segment;
            Segment _XC;

            //CALCOLA IL RAPPORTO TRA AB E BC
            _valueABBC = MeasueSwing(pattern.AB, BC);
            //CALCOLA IL RAPPORTO TRA XA E XC ( necessario per il cypher )
            _XC = new Segment(pattern.XA.FromPrice, BC.ToPrice, pattern.XA.FromOpenTime, BC.ToOpenTime);
            _valueXAXC = MeasueSwing(pattern.XA, _XC);

            //SE IL PUNTO B è TRA 0.786 E 1 DI XA SIGNIFICA CHE PUò ESSERE SOLO UN BUTTERFLY , 
            //PERTANTO SE BC SUPERA L'ULTIMO VALORE ACCETTABILE  PER IL BUTTERFLY (>= 0.886) ALLORA IL PATTERN E' INVALIDATO
            if (pattern.AB_Percent >= 0.786 && _valueABBC >= 0.886)
            {
                return false;
            }

            //PUO' SUCCEDERE CHE
            if (_valueXAXC > 1.414)
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //C è oltre l'ultimo pattern accettabile (cypher), in questo caso ridefinisco XA e riparto alla ricerca di B 
                // in realtà bisognerebbe vedere se la chiusura della candela supera il 1.414 perchè se solo l'ombra va oltre il pattern è accettato
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                _segment = pattern.XA;
                _segment.Extend(BC.ToPrice, BC.ToOpenTime);
                // se il pattern era in lista allora devi eliminarlo.
                DeletePatternInList(pattern);
                pattern.Reset(_segment);
                pattern.XA_Period = SegmentPeriod(pattern.XA);
                return true;
            }
            else
            {
                PatternType _type = C_IdentifyPattern(pattern, _valueABBC, _valueXAXC);
                switch (_type)
                {
                    case PatternType.Undefined:
                        //IL CALCOLO NON SEGNALA UN OBIETTIVO, LA RICERCA DI C PROSEGUE CON IL PROSSIMO SEGMENTO 
                        //un eventuale pattern già registrato per la gamba XA va eliminato
                        DeletePatternInList(pattern);

                        return true;
                    default:
                        //C è valido per almeno un pattern
                        // !!! IN REALTA' NON SERVE PERCHE' APPENA TROVA C VALIDO CERCA SUBITO D è eventulmete la ricerca di D che estenderà il punto C
                        // aggiorno BC solo se è più grande dell'eventuale precedente BC 
                        //if (pattern.BC == null) {
                        pattern.BC = BC;
                        //} else {
                        //    if ((pattern.Mode==PatternMode.Bullish && BC.ToPrice > pattern.BC.ToPrice) || (pattern.Mode == PatternMode.Bearish && BC.ToPrice < pattern.BC.ToPrice)) {
                        //        pattern.BC = BC;
                        //    }
                        //}

                        pattern.Step = PatternStep.DFinding;
                        pattern.Type = _type;
                        if (_type == PatternType.Cypher)
                        {
                            pattern.BC_Percent = _valueXAXC;
                        }
                        else
                        {
                            pattern.BC_Percent = _valueABBC;
                        }
                        pattern.BC_Period = SegmentPeriod(pattern.BC);


                        switch (_type)
                        {
                            case PatternType.Bat:
                                pattern.IdealCD = new Segment(pattern.BC.ToPrice, PriceRetracement(pattern.XA, 0.886), pattern.BC.ToOpenTime, pattern.BC.ToOpenTime);
                                pattern.PRZ = new Area(pattern.IdealCD.ToPrice, pattern.XA.FromPrice);
                                pattern.CPointValidArea = new Area(PriceRetracement(pattern.AB, 0.382), pattern.AB.FromPrice);
                                pattern.StopLoss = PriceRetracement(pattern.XA, 1.13);
                                pattern.SLZ = new Area(pattern.PRZ.ToPrice, pattern.StopLoss);
                                break;
                            case PatternType.Gartley:
                                pattern.IdealCD = new Segment(pattern.BC.ToPrice, PriceRetracement(pattern.XA, 0.786), pattern.BC.ToOpenTime, pattern.BC.ToOpenTime);
                                pattern.PRZ = new Area(pattern.IdealCD.ToPrice, pattern.XA.FromPrice);
                                pattern.CPointValidArea = new Area(PriceRetracement(pattern.AB, 0.618), pattern.AB.FromPrice);
                                pattern.StopLoss = PriceRetracement(pattern.XA, 1.13);
                                pattern.SLZ = new Area(pattern.PRZ.ToPrice, pattern.StopLoss);
                                break;
                            case PatternType.Cypher:
                                pattern.IdealCD = new Segment(pattern.BC.ToPrice, PriceRetracement(_XC, 0.786), pattern.BC.ToOpenTime, pattern.BC.ToOpenTime);
                                pattern.PRZ = new Area(pattern.IdealCD.ToPrice, pattern.XA.FromPrice);
                                pattern.CPointValidArea = new Area(PriceRetracement(pattern.XA, 1.272), PriceRetracement(pattern.XA, 1.414));
                                pattern.StopLoss = PriceRetracement(_XC, 1.13);
                                pattern.SLZ = new Area(pattern.PRZ.ToPrice, pattern.StopLoss);
                                break;
                            case PatternType.Butterfly:
                                pattern.IdealCD = new Segment(pattern.BC.ToPrice, PriceRetracement(pattern.XA, 1.272), pattern.BC.ToOpenTime, pattern.BC.ToOpenTime);
                                pattern.PRZ = new Area(pattern.IdealCD.ToPrice, PriceRetracement(pattern.XA, 1.414));
                                pattern.CPointValidArea = new Area(PriceRetracement(pattern.AB, 0.382), PriceRetracement(pattern.AB, 0.886));
                                pattern.StopLoss = PriceRetracement(pattern.XA, 1.618);
                                pattern.SLZ = new Area(pattern.PRZ.ToPrice, pattern.StopLoss);
                                break;

                        }

                        // calcolo l'area di prezzo in cui il pattern è disegnabile (il doppio della PRZ)
                        if (pattern.PRZ.FromPrice > pattern.PRZ.ToPrice)
                        {
                            double _fromPrice = pattern.PRZ.FromPrice + ((pattern.PRZ.FromPrice - pattern.PRZ.ToPrice) * 2);
                            //double _fromPrice = pattern.BC.ToPrice;
                            double _toPrice = pattern.StopLoss;
                            pattern.DrawableArea = new Area(_fromPrice, _toPrice);
                        }
                        else
                        {
                            double _fromPrice = pattern.PRZ.FromPrice - ((pattern.PRZ.ToPrice - pattern.PRZ.FromPrice) * 2);
                            //double _fromPrice = pattern.BC.ToPrice;
                            double _toPrice = pattern.StopLoss;
                            pattern.DrawableArea = new Area(_fromPrice, _toPrice);
                        }

                        RecordPatternInList(pattern);
                        return true;
                }
            }
        }


        public bool B_InRange(double retracementValue)
        {
            if ((retracementValue >= 0.5 && retracementValue < 0.618) || (retracementValue >= 0.618 && retracementValue < 0.786) || (retracementValue >= 0.382 && retracementValue < 0.618) || (retracementValue >= 0.786 && retracementValue < 1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public PatternType C_IdentifyPattern(ArmonicPattern pattern, double retracementValueABBC, double retracementValueXAXC)
        {
            PatternType _type = PatternType.Undefined;

            if (!pattern.CPointOverA && (pattern.AB_Percent >= 0.5 && pattern.AB_Percent < 0.618) && (retracementValueABBC >= 0.382 && retracementValueABBC < 1))
            {
                //BAT
                _type = PatternType.Bat;
            }
            if (!pattern.CPointOverA && (pattern.AB_Percent >= 0.618 && pattern.AB_Percent < 0.786) && (retracementValueABBC >= 0.618 && retracementValueABBC < 1))
            {
                //GARTLEY
                _type = PatternType.Gartley;
            }
            if ((pattern.AB_Percent >= 0.382 && pattern.AB_Percent < 0.618) && (retracementValueABBC >= 1) && (retracementValueXAXC >= 1.272 && retracementValueXAXC < 1.414))
            {
                //CYPHER
                _type = PatternType.Cypher;
            }
            if (!pattern.CPointOverA && (pattern.AB_Percent >= 0.786 && pattern.AB_Percent < 1) && (retracementValueABBC >= 0.382 && retracementValueABBC < 0.886))
            {
                //BUTTERFLY
                _type = PatternType.Butterfly;
            }

            return _type;

        }

        public void RecordPatternInList(ArmonicPattern pattern)
        {
            //controllo se ho già registrato questo pattern
            bool _found = false;
            for (int _patternIndex = 0; _patternIndex < PatternList.Count(); _patternIndex++)
            {
                ArmonicPattern _pattern = PatternList[_patternIndex];
                if (_pattern.GetKey() == pattern.GetKey())
                {
                    _found = true;
                    //effettuo l'assegnazione sole se il pattern non è copletato
                    if (!_pattern.Compleated)
                    {
                        PatternList[_patternIndex] = pattern;
                    }

                    break;
                }
            }
            //se non ho già il pattern in lista allora lo aggiungo
            if (!_found)
            {
                PatternList.Add(pattern);
            }
        }
        public void DeletePatternInList(ArmonicPattern pattern)
        {

            //if (pattern.Compleated) {
            //    return;
            //}
            DeleteDrawPattern(pattern);

            for (int _patternIndex = 0; _patternIndex < PatternList.Count(); _patternIndex++)
            {
                ArmonicPattern _pattern = PatternList[_patternIndex];
                if (_pattern.GetKey() == pattern.GetKey())
                {
                    PatternList.RemoveAt(_patternIndex);
                    break;
                }
            }

        }

        public void Calculate()
        {
            int _dimensionArea;
            Segment _startSegment;
            Segment _measureSegment;
            Segment _newXA, _newAB, _newBC;
            Segment[] Area;
            ArmonicPattern Pattern;
            bool _stopFind;

            //se non ho almeno 4 segmenti da analizzare è inutile che cerco i pattern
            if (Bot.segmentTracer.SegmentList.Count() < 3)
            {
                return;
            }
            //analizzo in ordine ogni segmento da sinistra verso destra
            for (int _index = 0; _index < Bot.segmentTracer.SegmentList.Count(); _index++)
            {
                // calcolo al dimensione dell'area a destra del segmento che stò analizzando
                _dimensionArea = Bot.segmentTracer.SegmentList.Count() - (_index + 1);
                //se non ho almeno 3 segmenti a destra allora è inutile che cerco i pattern
                if (_dimensionArea < 2)
                {
                    break;
                }
                //il primo segmento rappresenta inizialmente la gamba XA
                _startSegment = new Segment(Bot.segmentTracer.SegmentList[_index].FromPrice, Bot.segmentTracer.SegmentList[_index].ToPrice, Bot.segmentTracer.SegmentList[_index].FromOpenTime, Bot.segmentTracer.SegmentList[_index].ToOpenTime);
                //creo un nuovo pattern assegnando la gamba iniziale XA
                Pattern = new ArmonicPattern(_startSegment);
                Pattern.XA_Period = SegmentPeriod(Pattern.XA);
                //popolo l'area dei segmenti a destra da analizzare rispetto al segmento che stò analizzando
                Area = new Segment[_dimensionArea];
                Bot.segmentTracer.SegmentList.CopyTo((_index + 1), Area, 0, Bot.segmentTracer.SegmentList.Count() - (_index + 1));
                //resetto questa variabile che indica la condizione in cui non ha più senso continuare la ricerca nell'area dei segmenti perchè nessun pattern può essere valido rispetto al segmento che stò analizzando
                _stopFind = false;
                //trace debug
                //debugOn = false;
                if (Bot.Time.ToString("dd/MM/yyyy:HHmmss") == "14/01/2020:180000")
                {
                    if (_startSegment.FromOpenTime.ToString("dd/MM/yyyy:HHmmss") == "14/01/2020:080000")
                    {
                        //strumento ITA40
                        //debugOn = true;
                    }
                }

                foreach (Segment _segment in Area)
                {
                    switch (Pattern.Step)
                    {
                        case PatternStep.BFinding:
                            //se XA appartiene ad un patter completato allora salto l'elaborazione 
                            //questo perchè gli algoritmi di ricerca dei punti B e C (che ricalcolano tutto al completamento di un periodo) 
                            //possono trovarsi nella situazione di dover eliminare un pattern registrato per ridefinirne le gambe
                            //ed in questo caso si perderebbero le informazioni di completamento del pattern.
                            for (int _patternIndex = 0; _patternIndex < PatternList.Count(); _patternIndex++)
                            {
                                ArmonicPattern _pattern = PatternList[_patternIndex];
                                if (_pattern.GetKey() == Pattern.GetKey())
                                {

                                    //esco dalla ricerca se esiste un pattern completo per questa gamba
                                    if (_pattern.Compleated)
                                    {
                                        _stopFind = true;
                                    }
                                    break;
                                }
                            }

                            //controllo se la segmentazione è arrivata ad una continuazione di XA
                            if ((_segment.ToPrice > Pattern.XA.ToPrice && Pattern.Mode == PatternMode.Bullish) || (_segment.ToPrice < Pattern.XA.ToPrice && Pattern.Mode == PatternMode.Bearish))
                            {
                                // estendo la gamba XA
                                Pattern.XA.Extend(_segment.ToPrice, _segment.ToOpenTime);
                                Pattern.XA_Period = SegmentPeriod(Pattern.XA);
                                break;
                            }
                            //creo il segmento che va da A a B
                            _measureSegment = new Segment(Pattern.XA.ToPrice, _segment.ToPrice, Pattern.XA.ToOpenTime, _segment.ToOpenTime);
                            //faccio i conti con la gamba AB ed eventualmente aggiorno il pattern se ho trovato un punto B valido
                            if (VerifyAndUpdateAB(Pattern, _measureSegment) == false)
                            {
                                //B invalida la gamba XA, comporta che : dal segmento iniziale non può più nascere nessun pattern quindi interrompo la ricerca e passo al segmento successivo
                                _stopFind = true;
                            }
                            break;
                        case PatternStep.CFinding:
                            //controllo se la segmentazione è arrivata sopra ad A
                            //quindi gestisco il punto BC_OverA
                            if ((Pattern.Mode == PatternMode.Bullish && _segment.ToPrice > Pattern.XA.ToPrice) || (Pattern.Mode == PatternMode.Bearish && _segment.ToPrice < Pattern.XA.ToPrice))
                            {
                                Pattern.CPointOverA = true;
                                if (Pattern.BC_OverA == null)
                                {
                                    Pattern.BC_OverA = new Segment(Pattern.AB.ToPrice, _segment.ToPrice, Pattern.AB.ToOpenTime, _segment.ToOpenTime);
                                }
                                else
                                {
                                    if ((Pattern.Mode == PatternMode.Bullish && _segment.ToPrice > Pattern.BC_OverA.ToPrice) || (Pattern.Mode == PatternMode.Bearish && _segment.ToPrice < Pattern.BC_OverA.ToPrice))
                                    {
                                        Pattern.BC_OverA.Extend(_segment.ToPrice, _segment.ToOpenTime);
                                    }
                                }
                            }

                            //controllo se la segmentazione è arrivata ad una continuazione di AB
                            if ((_segment.ToPrice < Pattern.AB.ToPrice && Pattern.Mode == PatternMode.Bullish) || (_segment.ToPrice > Pattern.AB.ToPrice && Pattern.Mode == PatternMode.Bearish))
                            {
                                // controllo se il punto C NON ha mai oltrepassato il punto A
                                if (!Pattern.CPointOverA)
                                {
                                    // estendo la gamba AB
                                    Pattern.AB.Extend(_segment.ToPrice, _segment.ToOpenTime);
                                    //faccio i conti con la gamba AB ed eventualmente aggiorno il pattern se ho trovato un punto B valido
                                    if (VerifyAndUpdateAB(Pattern, Pattern.AB) == false)
                                    {
                                        //B invalida la gamba XA, comporta che : dal segmento iniziale non può più nascere nessun pattern quindi interrompo la ricerca e passo al segmento successivo
                                        _stopFind = true;
                                    }
                                }
                                else
                                {
                                    //E' una continuazione di AB ma il punto C ha precedentemente optreppassato A
                                    // estendo la gamba XA fino al punto C sopra ad A
                                    // ridefinisco la gamba AB
                                    _newXA = new Segment(Pattern.XA.FromPrice, Pattern.BC_OverA.ToPrice, Pattern.XA.FromOpenTime, Pattern.BC_OverA.ToOpenTime);
                                    _newAB = new Segment(Pattern.BC_OverA.ToPrice, _segment.ToPrice, Pattern.BC_OverA.ToOpenTime, _segment.ToOpenTime);

                                    Pattern.Reset(_newXA);
                                    Pattern.XA_Period = SegmentPeriod(Pattern.XA);
                                    //faccio i conti con la gamba AB ed eventualmente aggiorno il pattern se ho trovato un punto B valido
                                    if (VerifyAndUpdateAB(Pattern, _newAB) == false)
                                    {
                                        //B invalida la gamba XA, comporta che : dal segmento iniziale non può più nascere nessun pattern quindi interrompo la ricerca e passo al segmento successivo
                                        _stopFind = true;
                                    }

                                }
                                // esco dalla ricerca del punto C
                                break;
                            }
                            //creo il segmento per la misurazione del ritracciamento BC
                            _measureSegment = new Segment(Pattern.AB.ToPrice, _segment.ToPrice, Pattern.AB.ToOpenTime, _segment.ToOpenTime);
                            if (VerifyAndUpdateBC(Pattern, _measureSegment) == false)
                            {
                                DeletePatternInList(Pattern);
                                _stopFind = true;
                            }
                            break;
                        case PatternStep.DFinding:


                            //controllo se la segmentazione ridefinisce il punto B
                            //quindi gestisco il punto CD_OverB
                            if ((Pattern.Mode == PatternMode.Bullish && _segment.ToPrice < Pattern.AB.ToPrice) || (Pattern.Mode == PatternMode.Bearish && _segment.ToPrice > Pattern.AB.ToPrice))
                            {
                                Pattern.DPointOverB = true;
                                if (Pattern.CD_OverB == null)
                                {
                                    Pattern.CD_OverB = new Segment(Pattern.BC.ToPrice, _segment.ToPrice, Pattern.BC.ToOpenTime, _segment.ToOpenTime);
                                }
                                else
                                {
                                    if ((Pattern.Mode == PatternMode.Bullish && _segment.ToPrice < Pattern.CD_OverB.ToPrice) || (Pattern.Mode == PatternMode.Bearish && _segment.ToPrice > Pattern.CD_OverB.ToPrice))
                                    {
                                        Pattern.CD_OverB.Extend(_segment.ToPrice, _segment.ToOpenTime);
                                    }
                                }
                            }

                            // controllo se il punto D è arrivato oltre lo StopLoss
                            // in questo caso devo eliminare il pattern
                            if (_segment.ToPrice < Pattern.StopLoss && Pattern.Mode == PatternMode.Bullish || _segment.ToPrice > Pattern.StopLoss && Pattern.Mode == PatternMode.Bearish)
                            {
                                DeletePatternInList(Pattern);
                                _stopFind = true;
                            }

                            // controllo se il punto D è arrivato ad una continuazione di BC
                            if ((_segment.ToPrice > Pattern.BC.ToPrice && Pattern.Mode == PatternMode.Bullish) || (_segment.ToPrice < Pattern.BC.ToPrice && Pattern.Mode == PatternMode.Bearish))
                            {
                                if (Pattern.DPointOverB)
                                {

                                    //se il segmento CD_OverB  nel suo percorso è arrivato sotto al punto X allora elimino il pattern qualsiasi esso sia
                                    if ((Pattern.CD_OverB.ToPrice < Pattern.XA.FromPrice && Pattern.Mode == PatternMode.Bullish) || (Pattern.CD_OverB.ToPrice > Pattern.XA.FromPrice && Pattern.Mode == PatternMode.Bearish))
                                    {
                                        //DeletePatternInList(Pattern);
                                        //_stopFind = true;

                                        //? possibile ?
                                        //debugOn = true;
                                    }


                                    // nel caso del cypher vanno ridefinite le gambe XA AB BC in un modo differente ossia:
                                    // XA = XC, AB = C-D_OverB, BC = D_OverB - D
                                    if (Pattern.Type == PatternType.Cypher)
                                    {
                                        //debugOn = true;
                                        _newXA = new Segment(Pattern.XA.FromPrice, Pattern.BC.ToPrice, Pattern.XA.FromOpenTime, Pattern.BC.ToOpenTime);
                                        _newAB = new Segment(Pattern.BC.ToPrice, Pattern.CD_OverB.ToPrice, Pattern.BC.ToOpenTime, Pattern.CD_OverB.ToOpenTime);
                                        _newBC = new Segment(Pattern.CD_OverB.ToPrice, _segment.ToPrice, Pattern.CD_OverB.ToOpenTime, _segment.ToOpenTime);
                                    }
                                    else
                                    {
                                        // per tutti gli altri pattern va bene quanto sotto
                                        // devo ridefinire la gamba AB testando che sia valida e devo testare una nuova gamba BC
                                        _newXA = new Segment(Pattern.XA.FromPrice, Pattern.XA.ToPrice, Pattern.XA.FromOpenTime, Pattern.XA.ToOpenTime);
                                        _newAB = new Segment(Pattern.XA.ToPrice, Pattern.CD_OverB.ToPrice, Pattern.XA.ToOpenTime, Pattern.CD_OverB.ToOpenTime);
                                        _newBC = new Segment(Pattern.CD_OverB.ToPrice, _segment.ToPrice, Pattern.CD_OverB.ToOpenTime, _segment.ToOpenTime);
                                    }

                                    Pattern.Reset(_newXA);
                                    Pattern.XA_Period = SegmentPeriod(Pattern.XA);
                                    if (VerifyAndUpdateAB(Pattern, _newAB) == false)
                                    {
                                        //B invalida la gamba XA, comporta che : dal segmento iniziale non può più nascere nessun pattern quindi interrompo la ricerca e passo al segmento successivo
                                        DeletePatternInList(Pattern);
                                        _stopFind = true;
                                        break;
                                    }

                                    //controllo se la segmentazione è arrivata sopra ad A
                                    //quindi gestisco il punto BC_OverA
                                    if ((Pattern.Mode == PatternMode.Bullish && _segment.ToPrice > Pattern.XA.ToPrice) || (Pattern.Mode == PatternMode.Bearish && _segment.ToPrice < Pattern.XA.ToPrice))
                                    {
                                        //debugOn = true;
                                        Pattern.CPointOverA = true;
                                        if (Pattern.BC_OverA == null)
                                        {
                                            Pattern.BC_OverA = new Segment(Pattern.AB.ToPrice, _segment.ToPrice, Pattern.AB.ToOpenTime, _segment.ToOpenTime);
                                        }
                                        else
                                        {
                                            if ((Pattern.Mode == PatternMode.Bullish && _segment.ToPrice > Pattern.BC_OverA.ToPrice) || (Pattern.Mode == PatternMode.Bearish && _segment.ToPrice < Pattern.BC_OverA.ToPrice))
                                            {
                                                Pattern.BC_OverA.Extend(_segment.ToPrice, _segment.ToOpenTime);
                                            }
                                        }
                                    }

                                    if (VerifyAndUpdateBC(Pattern, _newBC) == false)
                                    {
                                        DeletePatternInList(Pattern);
                                        _stopFind = true;
                                    }

                                }
                                else
                                {
                                    //D è una continuazione di BC ma il punto B rimane invariato quindi ridefinisco la gamba BC
                                    _newXA = new Segment(Pattern.XA.FromPrice, Pattern.XA.ToPrice, Pattern.XA.FromOpenTime, Pattern.XA.ToOpenTime);
                                    _newAB = new Segment(Pattern.AB.FromPrice, Pattern.AB.ToPrice, Pattern.AB.FromOpenTime, Pattern.AB.ToOpenTime);
                                    _newBC = new Segment(Pattern.AB.ToPrice, _segment.ToPrice, Pattern.AB.ToOpenTime, _segment.ToOpenTime);

                                    Pattern.Reset(_newXA);
                                    Pattern.XA_Period = SegmentPeriod(Pattern.XA);
                                    if (VerifyAndUpdateAB(Pattern, _newAB) == false)
                                    {
                                        //B invalida la gamba XA, comporta che : dal segmento iniziale non può più nascere nessun pattern quindi interrompo la ricerca e passo al segmento successivo
                                        DeletePatternInList(Pattern);
                                        _stopFind = true;
                                        break;
                                    }

                                    //OCCORRE FARE UNA VALUTAZIONE IN PIU' 
                                    //copia e incolla dal CFinding
                                    //----------------------------------------------------------
                                    //controllo se la segmentazione è arrivata sopra ad A
                                    //quindi gestisco il punto BC_OverA
                                    if ((Pattern.Mode == PatternMode.Bullish && _segment.ToPrice > Pattern.XA.ToPrice) || (Pattern.Mode == PatternMode.Bearish && _segment.ToPrice < Pattern.XA.ToPrice))
                                    {
                                        Pattern.CPointOverA = true;
                                        if (Pattern.BC_OverA == null)
                                        {
                                            Pattern.BC_OverA = new Segment(Pattern.AB.ToPrice, _segment.ToPrice, Pattern.AB.ToOpenTime, _segment.ToOpenTime);
                                        }
                                        else
                                        {
                                            if ((Pattern.Mode == PatternMode.Bullish && _segment.ToPrice > Pattern.BC_OverA.ToPrice) || (Pattern.Mode == PatternMode.Bearish && _segment.ToPrice < Pattern.BC_OverA.ToPrice))
                                            {
                                                Pattern.BC_OverA.Extend(_segment.ToPrice, _segment.ToOpenTime);
                                            }
                                        }
                                    }
                                    if (VerifyAndUpdateBC(Pattern, _newBC) == false)
                                    {
                                        DeletePatternInList(Pattern);
                                        _stopFind = true;
                                    }
                                }
                                break;
                            }
                            break;
                    }
                    if (_stopFind)
                    {
                        break;
                    }
                }
            }

            //si applica qui il criterio di filtro per pattern dello stesso tipo con punti A,B,C in comune
            //si deve tener conto del rapporto di ampiezza tra le gambe XA e del rapporto dei periodi delle gambe XA


            //scorro i pattern per disegnarli a video
            for (int _index = 0; _index < PatternList.Count(); _index++)
            {
                DrawPattern(PatternList[_index]);
            }
        }

        public void FineCalculate(bool initialize, double price, Bar bar, DateTime openTime)
        {
            ArmonicPattern pattern;
            double TradePrice;
            bool trigCompleated = false;

            for (int _index = 0; _index < PatternList.Count(); _index++)
            {
                pattern = PatternList[_index];

                //SE IL PREZZO SUPERA IL PUNTO C ED IL PATTENR NON E' STATO COMPLETATO ALLORA DEVO ELIMINARE QUESTO PATTERN
                if ((!initialize ? price : bar.High) > pattern.BC.ToPrice && pattern.Mode == PatternMode.Bullish || (!initialize ? price : bar.Low) < pattern.BC.ToPrice && pattern.Mode == PatternMode.Bearish)
                {
                    if (pattern.Compleated == false)
                    {
                        DeletePatternInList(pattern);
                        continue;
                    }
                }

                if (pattern.Compleated && !pattern.Closed)
                {
                    // controllo se il punto D è arrivato oltre lo StopLoss
                    if ((!initialize ? price : bar.Low) < pattern.StopLoss && pattern.Mode == PatternMode.Bullish || (!initialize ? price : bar.High) > pattern.StopLoss && pattern.Mode == PatternMode.Bearish)
                    {
                        if (!pattern.Target1Compleated)
                        {
                            pattern.Failed = true;
                        }
                        pattern.Closed = true;
                        //registra statistiche
                        Statistics.Add(pattern);
                        continue;
                    }

                    //SE IL PREZZO E' ARRIVATO AL PRIMO TARGET ALLORA SPOSTO LO STOPLOSS IN PAREGGIO
                    if ((!initialize ? price : bar.High) >= pattern.Target1 && pattern.Mode == PatternMode.Bullish || (!initialize ? price : bar.Low) <= pattern.Target1 && pattern.Mode == PatternMode.Bearish)
                    {
                        //se ho la posizione aperta devo proteggerla mettendo a pareggio lo stop
                        if (pattern.Enter)
                        {
                            pattern.StopLoss = pattern.EnterPrice;
                            pattern.StopLossPips = 0;
                        }

                        if (pattern.Enter && !pattern.Target1Compleated)
                        {


                            //if (Bot.Positions.Find(pattern.GetKey()) != null)
                            //{
                            //    double volume_norm2 = Bot.Symbol.NormalizeVolumeInUnits(pattern.Volume / 2, RoundingMode.Down);
                            //    Bot.Positions.Find(pattern.GetKey()).ModifyVolume(volume_norm2);
                            //    Bot.Positions.Find(pattern.GetKey()).ModifyStopLossPips(-0.01);
                            //    Bot.Positions.Find(pattern.GetKey()).ModifyTakeProfitPrice(pattern.Target2);
                            //}
                        }

                        pattern.Target1Compleated = true;
                        pattern.Succeed = true;

                        Statistics.Add(pattern);


                    }

                    //SE IL PREZZO E' ARRIVATO AL SECONDO TARGET ALLORA CHIUDO LA POSIZIONE
                    if ((!initialize ? price : bar.High) >= pattern.Target2 && pattern.Mode == PatternMode.Bullish || (!initialize ? price : bar.Low) <= pattern.Target2 && pattern.Mode == PatternMode.Bearish)
                    {
                        if (pattern.Enter && !pattern.Target2Compleated)
                        {
                            if (Bot.Positions.Find(pattern.GetKey()) != null)
                            {
                                Bot.Positions.Find(pattern.GetKey()).Close();
                            }
                        }
                        pattern.Target2Compleated = true;
                        pattern.Succeed = true;
                        pattern.Closed = true;
                        Statistics.Add(pattern);

                        continue;
                    }
                }

                //stabilisco quale prezzo utlizzare
                if (initialize)
                {
                    TradePrice = pattern.Mode == PatternMode.Bullish ? bar.Low : bar.High;
                }
                else
                {
                    TradePrice = price;
                }

                //SE SONO IN PRZ E IL PATTERN E' ANCORA DA TRADARE
                if (pattern.PRZ.InArea(TradePrice) && !pattern.Succeed && !pattern.Failed && !pattern.Closed)
                {
                    if (pattern.CD == null)
                    {
                        pattern.CD = new Segment(pattern.BC.ToPrice, TradePrice, pattern.BC.ToOpenTime, openTime);
                        pattern.Compleated = true;
                        trigCompleated = true;
                    }
                    else
                    {
                        if (TradePrice < pattern.CD.ToPrice && pattern.Mode == PatternMode.Bullish || TradePrice > pattern.CD.ToPrice && pattern.Mode == PatternMode.Bearish)
                        {
                            pattern.CD.Extend(TradePrice, openTime);
                        }
                    }
                    pattern.CD_Percent = MeasueSwing(pattern.BC, pattern.CD);
                    pattern.CD_Period = SegmentPeriod(pattern.CD);
                    Segment _AD = new Segment(pattern.XA.ToPrice, pattern.CD.ToPrice, pattern.XA.ToOpenTime, pattern.CD.ToOpenTime);

                    //calcola target dinamici
                    switch (pattern.Type)
                    {
                        case PatternType.Bat:
                            pattern.Target1 = PriceRetracement(_AD, 0.382);
                            pattern.Target2 = PriceRetracement(_AD, 0.618);
                            break;
                        case PatternType.Gartley:
                            pattern.Target1 = PriceRetracement(_AD, 0.382);
                            pattern.Target2 = PriceRetracement(_AD, 0.618);
                            break;
                        case PatternType.Cypher:
                            pattern.Target1 = PriceRetracement(pattern.CD, 0.382);
                            pattern.Target2 = PriceRetracement(pattern.CD, 0.618);
                            break;
                        case PatternType.Butterfly:
                            pattern.Target1 = PriceRetracement(_AD, 0.382);
                            pattern.Target2 = PriceRetracement(_AD, 0.618);
                            break;
                    }
                    pattern.T1Z = new Area(_AD.ToPrice, pattern.Target1);
                    pattern.T2Z = new Area(pattern.Target1, pattern.Target2);

                    //calcola rischio/rendimento
                    pattern.Target1Pips = (pattern.Mode == PatternMode.Bullish ? pattern.Target1 - TradePrice : TradePrice - pattern.Target1) / Bot.Symbol.PipSize;
                    pattern.Target2Pips = (pattern.Mode == PatternMode.Bullish ? pattern.Target2 - TradePrice : TradePrice - pattern.Target2) / Bot.Symbol.PipSize;
                    pattern.StopLossPips = (pattern.Mode == PatternMode.Bullish ? TradePrice - pattern.StopLoss : pattern.StopLoss - TradePrice) / Bot.Symbol.PipSize;
                    pattern.RiskReward = (100 * pattern.Target1Pips) / (pattern.StopLossPips + pattern.Target1Pips);


                    //gestiso l'entrata automatica
                    if (!pattern.Enter && trigCompleated && Bot.EnterMode == EnterMode.Automatic)
                    {
                        //double euroPerUnita = pattern.StopLossPips * Bot.Symbol.PipValue;
                        //double rischioInEuro = 20;
                        //double unita = (rischioInEuro / euroPerUnita) / Bot.Symbol.LotSize;
                        //double volume = Bot.Symbol.QuantityToVolumeInUnits(unita);
                        //double volume_norm = Bot.Symbol.NormalizeVolumeInUnits(volume);

                        //TradeResult TR = Bot.ExecuteMarketOrder(pattern.Mode == PatternMode.Bullish ? TradeType.Buy : TradeType.Sell, Bot.Symbol.Name, volume_norm, pattern.GetKey());

                        //if (TR.IsSuccessful)
                        //{
                        //    TR.Position.ModifyStopLossPrice(pattern.StopLoss);
                        //    TR.Position.ModifyTakeProfitPrice(pattern.Target2);
                        //    pattern.PositionID = TR.Position.Id;
                        //    pattern.Enter = true;
                        //    pattern.EnterPrice = TradePrice;
                        //    pattern.Volume = volume_norm;
                        //}
                    }
                }



                if (pattern.DrawableArea.InArea(TradePrice) || pattern.Compleated)               
                {
                    pattern.Drawable = true;
                }
                else
                {
                    pattern.Drawable = false;
                }

                DrawPattern(pattern);
            }
        }

        public void DrawPattern(ArmonicPattern pattern)
        {
            int _thickness = 2;
            Color _borderColor = pattern.Mode == PatternMode.Bullish ? Color.Green : Color.Red;
            Color _primitiveColor = Color.White;
            Color _patternColor;
            Color _PRZColor = Color.FromArgb(100, Color.LightYellow);
            Color _PRZBorderColor = Color.FromArgb(255, Color.LightYellow);
            Color _SLZColor = Color.FromArgb(50, Color.Red);
            Color _SLZBorderColor = Color.FromArgb(255, Color.Red);
            Color _T1ZColor = Color.FromArgb(50, Color.LightGreen);
            Color _T1ZBorderColor = Color.FromArgb(255, Color.LightGreen);
            Color _T2ZColor = Color.FromArgb(50, Color.DarkGreen);
            Color _T2ZBorderColor = Color.FromArgb(255, Color.DarkGreen);


            ChartTriangle XAB, BCD;
            switch (pattern.Type)
            {
                case PatternType.Bat:
                    _primitiveColor = Color.DarkGray;
                    break;
                case PatternType.Gartley:
                    _primitiveColor = Color.Blue;
                    break;
                case PatternType.Cypher:
                    _primitiveColor = Color.Yellow;
                    break;
                case PatternType.Butterfly:
                    _primitiveColor = Color.Purple;
                    break;
            }
            _patternColor = Color.FromArgb(50, _primitiveColor.R, _primitiveColor.G, _primitiveColor.B);
            DateTime _RArea = Bot.Bars.Last(0).OpenTime;


            //BONE
            if (pattern.Drawable)
            {
                Bot.Chart.DrawTrendLine(string.Concat(pattern.GetKey(), "XA"), pattern.XA.FromOpenTime, pattern.XA.FromPrice, pattern.XA.ToOpenTime, pattern.XA.ToPrice, _borderColor, _thickness, LineStyle.Solid);
                Bot.Chart.DrawTrendLine(string.Concat(pattern.GetKey(), "AB"), pattern.AB.FromOpenTime, pattern.AB.FromPrice, pattern.AB.ToOpenTime, pattern.AB.ToPrice, _borderColor, _thickness, LineStyle.Solid);
                Bot.Chart.DrawTrendLine(string.Concat(pattern.GetKey(), "BC"), pattern.BC.FromOpenTime, pattern.BC.FromPrice, pattern.BC.ToOpenTime, pattern.BC.ToPrice, _borderColor, _thickness, LineStyle.Solid);

                if (pattern.Compleated)
                {
                    Bot.Chart.DrawTrendLine(string.Concat(pattern.GetKey(), "CD"), pattern.CD.FromOpenTime, pattern.CD.FromPrice, pattern.CD.ToOpenTime, pattern.CD.ToPrice, _borderColor, _thickness, LineStyle.Solid);

                    Bot.Chart.DrawTrendLine(string.Concat(pattern.GetKey(), "XB"), pattern.XA.FromOpenTime, pattern.XA.FromPrice, pattern.AB.ToOpenTime, pattern.AB.ToPrice, _borderColor, _thickness, LineStyle.DotsRare);
                    Bot.Chart.DrawTrendLine(string.Concat(pattern.GetKey(), "AC"), pattern.AB.FromOpenTime, pattern.AB.FromPrice, pattern.BC.ToOpenTime, pattern.BC.ToPrice, _borderColor, _thickness, LineStyle.DotsRare);
                    Bot.Chart.DrawTrendLine(string.Concat(pattern.GetKey(), "BD"), pattern.BC.FromOpenTime, pattern.BC.FromPrice, pattern.CD.ToOpenTime, pattern.CD.ToPrice, _borderColor, _thickness, LineStyle.DotsRare);
                    Bot.Chart.DrawTrendLine(string.Concat(pattern.GetKey(), "XD"), pattern.XA.FromOpenTime, pattern.XA.FromPrice, pattern.CD.ToOpenTime, pattern.CD.ToPrice, _borderColor, _thickness, LineStyle.DotsRare);
                }
                else
                {
                    Bot.Chart.DrawTrendLine(string.Concat(pattern.GetKey(), "CD"), pattern.IdealCD.FromOpenTime, pattern.IdealCD.FromPrice, _RArea, pattern.IdealCD.ToPrice, _borderColor, _thickness, LineStyle.Solid);
                }

            }
            else
            {
                Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "XA"));
                Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "AB"));
                Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "BC"));
                Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "CD"));
                Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "XB"));
                Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "AC"));
                Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "BD"));
                Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "XD"));
            }

            //TRIANGLE
            if (pattern.Drawable)
            {
                XAB = Bot.Chart.DrawTriangle((string.Concat(pattern.GetKey(), "XAB")), pattern.XA.FromOpenTime, pattern.XA.FromPrice, pattern.XA.ToOpenTime, pattern.XA.ToPrice, pattern.AB.ToOpenTime, pattern.AB.ToPrice, _patternColor, 1, LineStyle.Lines);
                if (pattern.Compleated)
                {
                    BCD = Bot.Chart.DrawTriangle((string.Concat(pattern.GetKey(), "BCD")), pattern.BC.FromOpenTime, pattern.BC.FromPrice, pattern.BC.ToOpenTime, pattern.BC.ToPrice, pattern.CD.ToOpenTime, pattern.CD.ToPrice, _patternColor, 1, LineStyle.Lines);
                }
                else
                {
                    BCD = Bot.Chart.DrawTriangle((string.Concat(pattern.GetKey(), "BCD")), pattern.BC.FromOpenTime, pattern.BC.FromPrice, pattern.BC.ToOpenTime, pattern.BC.ToPrice, _RArea, pattern.IdealCD.ToPrice, _patternColor, 1, LineStyle.Lines);
                }

                XAB.IsFilled = true;
                BCD.IsFilled = true;
                //BCD.IsInteractive = true;
                //BCD.IsLocked = true;


            }
            else
            {
                Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "XAB"));
                Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "BCD"));
            }

            //LEVEL
            if (!pattern.Closed && pattern.Drawable)
            {
                if (!pattern.Target1Compleated)
                {
                    ChartRectangle PRZ = Bot.Chart.DrawRectangle(string.Concat(pattern.GetKey(), "PRZ"), pattern.XA.FromOpenTime, pattern.PRZ.FromPrice, pattern.PRZ.InArea(Bot.Ask) ? _RArea : pattern.XA.ToOpenTime, pattern.PRZ.ToPrice, _PRZColor);
                    Bot.Chart.DrawRectangle(string.Concat(pattern.GetKey(), "PRZ_BORDER"), pattern.XA.FromOpenTime, pattern.PRZ.FromPrice, pattern.PRZ.InArea(Bot.Ask) ? _RArea : pattern.XA.ToOpenTime, pattern.PRZ.ToPrice, _PRZBorderColor);
                    PRZ.IsFilled = true;
                    ChartRectangle SLZ = Bot.Chart.DrawRectangle(string.Concat(pattern.GetKey(), "SLZ"), pattern.XA.FromOpenTime, pattern.SLZ.FromPrice, pattern.SLZ.InArea(Bot.Ask) ? _RArea : pattern.XA.ToOpenTime, pattern.SLZ.ToPrice, _SLZColor);
                    Bot.Chart.DrawRectangle(string.Concat(pattern.GetKey(), "SLZ_BORDER"), pattern.XA.FromOpenTime, pattern.SLZ.FromPrice, pattern.SLZ.InArea(Bot.Ask) ? _RArea : pattern.XA.ToOpenTime, pattern.SLZ.ToPrice, _SLZBorderColor);
                    SLZ.IsFilled = true;
                }
                else
                {
                    Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "PRZ"));
                    Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "PRZ_BORDER"));
                    Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "SLZ"));
                    Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "SLZ_BORDER"));
                }

                if (pattern.Compleated)
                {
                    ChartRectangle T1Z = Bot.Chart.DrawRectangle(string.Concat(pattern.GetKey(), "T1Z"), pattern.XA.FromOpenTime, pattern.T1Z.FromPrice, pattern.T1Z.InArea(Bot.Ask) ? _RArea : pattern.XA.ToOpenTime, pattern.T1Z.ToPrice, _T1ZColor);
                    Bot.Chart.DrawRectangle(string.Concat(pattern.GetKey(), "T1Z_BORDER"), pattern.XA.FromOpenTime, pattern.T1Z.FromPrice, pattern.T1Z.InArea(Bot.Ask) ? _RArea : pattern.XA.ToOpenTime, pattern.T1Z.ToPrice, _T1ZBorderColor);
                    T1Z.IsFilled = true;
                    ChartRectangle T2Z = Bot.Chart.DrawRectangle(string.Concat(pattern.GetKey(), "T2Z"), pattern.XA.FromOpenTime, pattern.T2Z.FromPrice, pattern.T2Z.InArea(Bot.Ask) ? _RArea : pattern.XA.ToOpenTime, pattern.T2Z.ToPrice, _T2ZColor);
                    Bot.Chart.DrawRectangle(string.Concat(pattern.GetKey(), "T2Z_BORDER"), pattern.XA.FromOpenTime, pattern.T2Z.FromPrice, pattern.T2Z.InArea(Bot.Ask) ? _RArea : pattern.XA.ToOpenTime, pattern.T2Z.ToPrice, _T2ZBorderColor);
                    T2Z.IsFilled = true;


                }

            }
            else
            {

                Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "PRZ"));
                Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "PRZ_BORDER"));
                Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "SLZ"));
                Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "SLZ_BORDER"));
                Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "T1Z"));
                Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "T1Z_BORDER"));
                Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "T2Z"));
                Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "T2Z_BORDER"));
            }

            //INFO
            string _text = "";
            if (pattern.Drawable)
            {
                _text = string.Concat(pattern.Type.ToString(), " ");
            }

            if (pattern.Compleated)
            {
                _text = string.Concat(_text, "(", SegmentPeriod(pattern.XA).ToString(), ", ");
                _text = string.Concat(_text, SegmentPeriod(pattern.AB).ToString(), ", ");
                _text = string.Concat(_text, SegmentPeriod(pattern.BC).ToString(), ", ");
                _text = string.Concat(_text, SegmentPeriod(pattern.CD).ToString(), ")");
                //_text = string.Concat(_text, "R/Rw : ", Math.Round(pattern.RiskReward, 2).ToString(), "%  ");
            }
            _text = _text.ToUpper();
            if (pattern.Target1Compleated == true)
            {
                _text = string.Concat(_text, "\nTarget #1 Reached");
            }
            if (pattern.Target2Compleated == true)
            {
                _text = string.Concat(_text, "\nTarget #2 Reached");
            }
            if (pattern.Closed)
            {
                _text = string.Concat(_text, "\nClosed (", (pattern.Succeed == true ? "Succeed" : "Failed"), ")");
            }

            //string.Concat(_text.ToUpper(), "\n", pattern.Compleated.ToString());

            ChartText _message = Bot.Chart.DrawText(string.Concat(pattern.GetKey(), "msg"), _text, pattern.XA.FromOpenTime, pattern.XA.FromPrice, _borderColor);
            if (pattern.Mode == PatternMode.Bearish)
            {
                _message.VerticalAlignment = VerticalAlignment.Top;
            }
            else
            {
                _message.VerticalAlignment = VerticalAlignment.Bottom;

            }
            _message.FontSize = 8;


        }
        public void DeleteDrawPattern(ArmonicPattern pattern)
        {


            //BONE
            Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "XA"));
            Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "AB"));
            Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "BC"));
            Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "CD"));
            Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "XB"));
            Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "AC"));
            Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "BD"));
            Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "XD"));
            //INFO
            Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "msg"));

            //TRIANGLE
            Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "XAB"));
            Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "BCD"));

            //LEVEL
            Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "PRZ"));
            Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "PRZ_BORDER"));
            Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "SLZ"));
            Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "SLZ_BORDER"));
            Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "T1Z"));
            Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "T1Z_BORDER"));
            Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "T2Z"));
            Bot.Chart.RemoveObject(string.Concat(pattern.GetKey(), "T2Z_BORDER"));
        }
    }
    public class SegmentTracerEngine
    {
        private readonly ArmonicBot Bot;

        private Direction Direction = Direction.NoDirection;
        private Segment Segment;

        public List<Segment> SegmentList;
        private bool _firstBarEvent = true;

        public SegmentTracerEngine(ArmonicBot bot)
        {
            Bot = bot;
            SegmentList = new List<Segment>();
        }

        public void Calculate(Bar _curr, Bar _prev, bool clearBack)
        {

            //Bar _limitPeriod = Bot.Bars.Last(2000);
            bool _swing = false;
            bool _nested = false;

            //salto il primo evento perchè riporta una barra iniziale non corretta
            if (_firstBarEvent)
            {
                _firstBarEvent = false;
                return;
            }

            // leggo la direzione del prezzo
            //[      ] BUG ==> GBPUSD H1 : 31/07/2020 doppio innesto
            switch (Direction)
            {
                case Direction.Up:
                    // se le barre sono innestate ed in espansione
                    if (_curr.High > _prev.High && _curr.Low < _prev.Low)
                    {
                        _nested = true;
                        //se l'innesto in espansione è eccedente per altezza allora mantengo la direzione up
                        if (_curr.High - _prev.High <= _prev.Low - _curr.Low)
                        {
                            //l'innesto in espansione accentua in basso quindi lo considero uno swing
                            Direction = Direction.Down;
                            _swing = true;
                        }
                    }
                    // se le barre non sono innestate
                    else
                    {
                        //controllo se c'è stato uno swing
                        if (_curr.High <= _prev.High)
                        {
                            // il prezzo ha invertito
                            Direction = Direction.Down;
                            _swing = true;
                        }
                    }
                    break;
                case Direction.Down:
                    // se le barre sono innestate ed in espansione
                    if (_curr.High > _prev.High && _curr.Low < _prev.Low)
                    {
                        _nested = true;
                        //se l'innesto in espansione è eccedente per altezza allora lo considero uno swing
                        if (_curr.High - _prev.High > _prev.Low - _curr.Low)
                        {
                            Direction = Direction.Up;
                            _swing = true;
                        }
                    }
                    else
                    {
                        // se i prezzi scendono allora è una continuazione
                        if (_curr.Low >= _prev.Low)
                        {
                            // se il prezzo non registra un nuovo minimo allora è uno swing
                            Direction = Direction.Up;
                            _swing = true;
                        }
                    }
                    break;
                case Direction.NoDirection:
                    if (_curr.High >= _prev.High && _curr.Low >= _prev.Low)
                    {
                        Direction = Direction.Up;
                    }
                    else if (_curr.Low < _prev.Low && _curr.High < _prev.High)
                    {
                        Direction = Direction.Down;
                    }
                    else
                    {
                        // condizione di innesto delle barre, esco perchè non c'è ancora una direzione
                        Direction = Direction.NoDirection;
                        return;
                    }
                    _swing = true;
                    break;
            }

            if (!_swing)
            {
                // le barre seguono il loro corso quindi aggiorno l'ultimo segmento
                Segment.Update(_curr);
                SegmentList[SegmentList.Count - 1] = Segment;
                DrawSegment();
            }
            else
            {
                // le barre invertono, è uno swing quindi creo un nuovo segmento nella nuova direzione (opposta)
                // MA PRIMA SE C'E' UN INNESTO DEVO ANCHE SPOSTARE IL SEGMENTO PRECEDENTE SUL PREZZO CORRETTO PRIMA DI REGISTRARE UN NUOVO SEGMENTO
                if (_nested && _swing)
                {
                    Segment.UpdatePrice(_curr);
                    DrawSegment();
                }

                Segment = new Segment(Direction, _prev, _curr);

                if (_nested && _swing)
                {
                    Segment.UpdateFromPrice(_curr);
                }
                DrawSegment();
                SegmentList.Add(Segment);
            }

            //elimino i segmenti dallo storico secondo il numero di periodi impostato
            if (Bot.Bars.Count() > Bot.MaxPatternPeriods && clearBack)
            {
                foreach (Segment _app in SegmentList)
                {
                    if (_app.ToOpenTime < Bot.Bars.Last(Bot.MaxPatternPeriods).OpenTime)
                    {
                        DeleteDrawSegment(SegmentList[0]);
                        SegmentList.RemoveAt(0);
                        break;
                    }
                }

            }

        }

        public void DrawSegment()
        {
            if (Bot.DrawSwing)
            {
                string _name = String.Format("Segment_{0} ", Segment.FromOpenTime.ToString("dd/MM/yyyy:HHmmss"));
                Bot.Chart.DrawTrendLine(_name, Segment.FromOpenTime, Segment.FromPrice, Segment.ToOpenTime, Segment.ToPrice, Color.Yellow, 1, LineStyle.Dots);
            }
        }
        public void DeleteDrawSegment(Segment SegmentToDelete)
        {
            if (Bot.DrawSwing)
            {
                string _name = String.Format("Segment_{0} ", SegmentToDelete.FromOpenTime.ToString("dd/MM/yyyy:HHmmss"));
                Bot.Chart.RemoveObject(_name);
            }
        }

    }
    public class Segment
    {
        public double FromPrice;
        public double ToPrice;
        public DateTime FromOpenTime;
        public DateTime ToOpenTime;
        public readonly Direction Direction;

        public Segment(Direction direction, Bar startBar, Bar endBar)
        {
            Direction = direction;
            FromOpenTime = startBar.OpenTime;
            ToOpenTime = endBar.OpenTime;

            if (Direction == Direction.Up)
            {
                FromPrice = startBar.Low;
                ToPrice = endBar.High;
            }
            if (Direction == Direction.Down)
            {
                FromPrice = startBar.High;
                ToPrice = endBar.Low;
            }
        }
        public Segment(double fromPrice, double toPrice, DateTime fromOpenTime, DateTime toOpenTime)
        {
            FromOpenTime = fromOpenTime;
            ToOpenTime = toOpenTime;
            FromPrice = fromPrice;
            ToPrice = toPrice;
            Direction = toPrice > fromPrice ? Direction.Up : Direction.Down;
        }

        public double Measure()
        {
            return Direction == Direction.Up ? ToPrice - FromPrice : FromPrice - ToPrice;
        }

        public void Update(Bar endBar)
        {
            UpdatePrice(endBar);
            ToOpenTime = endBar.OpenTime;
        }
        public void UpdatePrice(Bar endBar)
        {
            ToPrice = Direction == Direction.Down ? endBar.Low : endBar.High;
        }
        public void UpdateFromPrice(Bar endBar)
        {
            FromPrice = Direction == Direction.Down ? endBar.High : endBar.Low;
        }
        public void Extend(double toPrice, DateTime toOpenTime)
        {
            ToPrice = toPrice;
            ToOpenTime = toOpenTime;
        }
    }
    public class Area
    {
        public double FromPrice;
        public double ToPrice;

        public Area(double fromPrice, double toPrice)
        {
            FromPrice = fromPrice;
            ToPrice = toPrice;
        }

        public bool InArea(double price)
        {
            if (FromPrice > ToPrice)
            {
                if (price <= FromPrice && price >= ToPrice)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (price >= FromPrice && price <= ToPrice)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        public bool AboveArea(double price)
        {
            if (FromPrice < ToPrice && price > ToPrice)
            {
                return true;
            }
            else if (FromPrice > ToPrice && price < ToPrice)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool BelowArea(double price)
        {
            if (FromPrice < ToPrice && price < FromPrice)
            {
                return true;
            }
            else if (FromPrice > ToPrice && price > FromPrice)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public enum Direction
    {
        Up,
        Down,
        NoDirection
    }
    public enum PatternType
    {
        Undefined,
        Gartley,
        Bat,
        Cypher,
        Butterfly
    }
    public enum PatternMode
    {
        Bullish,
        Bearish
    }
    public enum SegmentMoveType
    {
        Impulsive,
        corrective
    }
    public enum PatternStep
    {
        BFinding,
        CFinding,
        DFinding,
        Completed
    }
    public enum EnterMode
    {
        Automatic,
        Manual,
        Signal,
        None
    }

}
