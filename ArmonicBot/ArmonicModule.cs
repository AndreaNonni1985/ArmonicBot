using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.NetworkInformation;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo {
    public static class Utils {

        public static TimeFrame[] TimeFrames = { 
            TimeFrame.Minute, 
            TimeFrame.Minute2, 
            TimeFrame.Minute3, 
            TimeFrame.Minute4, 
            TimeFrame.Minute5, 
            TimeFrame.Minute6, 
            TimeFrame.Minute7, 
            TimeFrame.Minute8, 
            TimeFrame.Minute9, 
            TimeFrame.Minute10,
            
            TimeFrame.Minute15,
            TimeFrame.Minute20,
            TimeFrame.Minute30,
            TimeFrame.Minute45,

            TimeFrame.Hour,
            TimeFrame.Hour2,
            TimeFrame.Hour3,
            TimeFrame.Hour4,
            TimeFrame.Hour6,
            TimeFrame.Hour8,
            TimeFrame.Hour12,

            TimeFrame.Daily,
            TimeFrame.Day2,
            TimeFrame.Day3,

            TimeFrame.Weekly,

            TimeFrame.Monthly
        };
        public static TimeFrame MinutesToTimeFrame(int Minutes) {
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
        public static int TimeframeToMinutes(TimeFrame MyCandle) {

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
    public static class Debug {
        public static Algo sender;
        public static void Print(object value) {
            sender.Print(value);
        }
        public static void Print(params object[] parameters) {
            sender.Print(parameters);
        }
        public static void Print(string message, params object[] parameters) {
            sender.Print(message, parameters);
        }
    }
    public static class GlobalParameter {
        public static int Periods;
        public static int SubPeriods;
        public static bool DrawSwing;
    }
    public class DataFlow {
        public Bars BarsData;
        public Ticks TicksData;
        public bool isBarLoadCompleted;
        public bool isTickLoadCompleted;
        private int _barRequested = 0;

        private readonly MarketData _marketdata;
        private readonly Symbol _symbol;
        private readonly TimeFrame _timeframe;

        public int GetBarsRequestedCount() {
            return _barRequested;
        }
        public int GetBarsLoadedCount() {
            if (BarsData != null) {
                return BarsData.Count() > _barRequested ? _barRequested : BarsData.Count();
            }
            else {
                return 0;
            }
        }

        private event Action<DataFlow> on_AsyncBarsLoaded;
        private event Action<double, int, string> on_AsyncBarsLoading;
        public DataFlow(MarketData marketdata, Symbol symbol, TimeFrame timeframe) {
            _marketdata = marketdata;
            _symbol = symbol;
            _timeframe = timeframe;
        }

        public void TraceNewBar(Action<BarOpenedEventArgs> listner) {
            if (BarsData != null) {
                BarsData.BarOpened += listner;
            }
        }
        public void RequestBars(int barRequested, bool __async = false, Action<DataFlow> listnerLoaded = null, Action<double, int, string> listnerLoading = null) {
            _barRequested = barRequested;
            if (!__async) {
                //Richiedo i dati delle barre in maniera sincrona
                BarsData = _marketdata.GetBars(_timeframe, _symbol.Name);
                while (BarsData.Count < _barRequested) {
                    BarsData.LoadMoreHistory();
                }
            }
            else {
                //Action<double, int> hndlLoad = on_AsyncBarsLoading;
                on_AsyncBarsLoaded += listnerLoaded;
                on_AsyncBarsLoading += listnerLoading;
                AsyncBarsLoading(true);
            }
        }

        private void AsyncBarsLoading(bool first = false) {
            if (first) {
                //richiedo i dati
                _marketdata.GetBarsAsync(_timeframe, _symbol.Name, LoadData => AsyncEndBarsLoaded(LoadData));
            }
            else {
                //calcolo la percentuale dei dati caricati fino ad ora
                double AsyncBarsLoadingPercentage = BarsData.Count >= _barRequested ? 100 : Math.Round(Convert.ToDouble(BarsData.Count * 100) / _barRequested, 2);
                if (on_AsyncBarsLoading != null) {
                    on_AsyncBarsLoading.Invoke(AsyncBarsLoadingPercentage, BarsData.Count, _symbol.ToString());
                }
                if (BarsData.Count < _barRequested) {
                    //continuo a richiedere dati
                    BarsData.LoadMoreHistoryAsync(AsyncEndMoreBarsLoaded);
                }
                else {
                    RaiseBarsLoadCompleted();
                }
            }
        }
        private void AsyncEndBarsLoaded(Bars bars) {
            BarsData = bars;
            AsyncBarsLoading();
        }
        private void AsyncEndMoreBarsLoaded(BarsHistoryLoadedEventArgs obj) {
            if (obj.Count > 0) {
                AsyncBarsLoading();
            }
            else {
                RaiseBarsLoadCompleted();
            }
        }
        private void RaiseBarsLoadCompleted() {
            //segnalo il completamento
            isBarLoadCompleted = true;
            if (on_AsyncBarsLoaded != null) {
                on_AsyncBarsLoaded.Invoke(this);
            }
        }



        public void TraceNewTick(Action<TicksTickEventArgs> listner) {
            if (TicksData != null) {
                TicksData.Tick += listner;
            }
        }
        public void RequestTicks(DateTime ticksFromDateTime, bool __async = false) {
            if (!__async) {
                //Richiedo i dati dei tick in maniera sincrona
                TicksData = _marketdata.GetTicks(_symbol.Name);
            }
            else {
                AsyncTicksLoading(true);
            }
        }
        private void AsyncTicksLoading(bool first = false) {

        }
        private void AsyncEndTicksLoaded(Bars bars) {

        }
        private void AsyncEndMoreTicksLoaded(TicksHistoryLoadedEventArgs obj) {

        }
        private void RaiseTicksLoadCompleted() {

        }
    }
    public class GUI {

        public static class Styles {
            public static Style CreatePanelBackgroundStyle() {
                var style = new Style();
                style.Set(ControlProperty.CornerRadius, 3);
                style.Set(ControlProperty.BackgroundColor, GetColorWithOpacity(Color.FromHex("#303060"), 0.45m), ControlState.DarkTheme);
                style.Set(ControlProperty.BackgroundColor, GetColorWithOpacity(Color.FromHex("#000030"), 0.45m), ControlState.LightTheme);
                style.Set(ControlProperty.BorderColor, Color.FromHex("#3C3C3C"), ControlState.DarkTheme);
                style.Set(ControlProperty.BorderColor, Color.FromHex("#C3C3C3"), ControlState.LightTheme);
                style.Set(ControlProperty.BorderThickness, new Thickness(1));

                return style;
            }

            public static Style CreateCommonBorderStyle() {
                var style = new Style();
                style.Set(ControlProperty.BorderColor, GetColorWithOpacity(Color.FromHex("#FFFFFF"), 0.12m), ControlState.DarkTheme);
                style.Set(ControlProperty.BorderColor, GetColorWithOpacity(Color.FromHex("#000000"), 0.12m), ControlState.LightTheme);
                return style;
            }

            public static Style CreateHeaderStyle() {
                var style = new Style();
                style.Set(ControlProperty.ForegroundColor, GetColorWithOpacity("#FFFFFF", 0.70m), ControlState.DarkTheme);
                style.Set(ControlProperty.ForegroundColor, GetColorWithOpacity("#000000", 0.65m), ControlState.LightTheme);
                return style;
            }

            public static Style CreateCheckBoxStyle() {
                var style = new Style(DefaultStyles.CheckBoxStyle);
                style.Set(ControlProperty.BackgroundColor, Color.FromHex("#1A1A1A"), ControlState.DarkTheme);
                style.Set(ControlProperty.BackgroundColor, Color.FromHex("#111111"), ControlState.DarkTheme | ControlState.Hover);
                style.Set(ControlProperty.BackgroundColor, Color.FromHex("#E7EBED"), ControlState.LightTheme);
                style.Set(ControlProperty.BackgroundColor, Color.FromHex("#D6DADC"), ControlState.LightTheme | ControlState.Hover);
                style.Set(ControlProperty.CornerRadius, 3);
                return style;
            }

            public static Style CreateGreenStyle() {
                return CreateButtonStyle(Color.FromHex("#009345"), Color.FromHex("#10A651"));
            }

            public static Style CreateRedStyle() {
                return CreateButtonStyle(Color.FromHex("#F05824"), Color.FromHex("#FF6C36"));
            }

            private static Style CreateButtonStyle(Color color, Color hoverColor) {
                var style = new Style(DefaultStyles.ButtonStyle);
                style.Set(ControlProperty.BackgroundColor, color, ControlState.DarkTheme);
                style.Set(ControlProperty.BackgroundColor, color, ControlState.LightTheme);
                style.Set(ControlProperty.BackgroundColor, hoverColor, ControlState.DarkTheme | ControlState.Hover);
                style.Set(ControlProperty.BackgroundColor, hoverColor, ControlState.LightTheme | ControlState.Hover);
                style.Set(ControlProperty.ForegroundColor, Color.FromHex("#FFFFFF"), ControlState.DarkTheme);
                style.Set(ControlProperty.ForegroundColor, Color.FromHex("#FFFFFF"), ControlState.LightTheme);
                return style;
            }

            private static Color GetColorWithOpacity(Color baseColor, decimal opacity) {
                var alpha = (int)Math.Round(byte.MaxValue * opacity, MidpointRounding.AwayFromZero);
                return Color.FromArgb(alpha, baseColor);
            }
        }

        public class ProgressBar : Canvas {
            private Rectangle border, back, front;
            private int _value;

            public Color BackColor { private get; set; }
            public Color ForeColor { private get; set; }
            public Color BorderColor { private get; set; }
            public double RadiusX { private get; set; }
            public double RadiusY { private get; set; }
            public int MaxValue { private get; set; }
            public int Value {
                set {
                    _value = value;
                    if (front != null)
                        front.Width = (_value * this.Width) / MaxValue;
                    Reflect();
                }
            }

            public ProgressBar() {
                BackColor = (Color)DefaultStyles.ScrollViewerStyle.Get(ControlProperty.BackgroundColor);
                ForeColor = (Color)DefaultStyles.ScrollViewerStyle.Get(ControlProperty.ForegroundColor);
                BorderColor = (Color)DefaultStyles.ScrollViewerStyle.Get(ControlProperty.BorderColor);
                RadiusX = 5;
                RadiusY = 5;
                border = new Rectangle();
                back = new Rectangle();
                front = new Rectangle();
                this.AddChild(border);
                this.AddChild(back);
                this.AddChild(front);
                Value = 0;
                MaxValue = 1;
            }

            public void Reflect() {
                border.Width = this.Width;
                border.Height = this.Height;
                border.Left = this.Left;
                border.Top = this.Top;
                border.RadiusX = this.RadiusX;
                border.RadiusY = this.RadiusY;
                border.StrokeColor = this.BorderColor;

                back.Width = this.Width;
                back.Height = this.Height;
                back.Left = this.Left;
                back.Top = this.Top;
                back.RadiusX = this.RadiusX;
                back.RadiusY = this.RadiusY;
                back.FillColor = this.BackColor;

                //front.Width = this.Width;
                front.Height = this.Height;
                front.Left = this.Left;
                front.Top = this.Top;
                front.RadiusX = this.RadiusX;
                front.RadiusY = this.RadiusY;
                front.FillColor = this.ForeColor;
            }

        }

        private class ResultItem : CustomControl {
            public string Key;
            private Button PatternButton;
            private Grid Data;
            private const String _defaultMargin = "40 2 40 2";

            private class Column : CustomControl {
                public Column(string value) {
                    TextBlock caption = new TextBlock {
                        Text = value,
                        Margin = _defaultMargin
                    };
                    Border border = new Border {
                        Child = caption,
                        BorderThickness = new Thickness(1),
                        Style = Styles.CreateCommonBorderStyle()

                    };
                    AddChild(border);
                }
            }
            public ControlBase newColumn(string value) {
                return new Column(value);
            }
            
            public ResultItem(ArmonicPattern pattern) {

                Data = new Grid(1, 10) {                    
                };
                Data.Columns[0].SetWidthToAuto();
                Data.Columns[1].SetWidthToAuto();
                Data.Columns[2].SetWidthToAuto();
                Data.Columns[3].SetWidthToAuto();
                Data.Columns[4].SetWidthToAuto();
                Data.Columns[5].SetWidthToAuto();
                Data.Columns[6].SetWidthToAuto();
                Data.Columns[7].SetWidthToAuto();
                Data.Columns[8].SetWidthToAuto();
                Data.Columns[9].SetWidthToAuto();

                Data.AddChild(newColumn(pattern.Symbol.Name), 0, 0);
                Data.AddChild(newColumn(pattern.TimeFrame.ToString()), 0, 1);
                Data.AddChild(newColumn(pattern.Type.ToString()), 0, 2);
                Data.AddChild(newColumn(pattern.Mode.ToString()), 0, 3);
                Data.AddChild(newColumn("<simmetry>"), 0, 4);
                Data.AddChild(newColumn("<pattern width>"), 0, 5);
                Data.AddChild(newColumn("<% compleated>"), 0, 6);
                Data.AddChild(newColumn("<levels>"), 0, 7);
                Data.AddChild(newColumn("<impulse>"), 0, 8);
                Data.AddChild(newColumn("<state>"), 0, 9);

                PatternButton = new Button() {
                    Content=Data,
                    HorizontalAlignment=HorizontalAlignment.Left
                };
                
                AddChild(PatternButton);
                Key = pattern.GetKey();
            }
        }
        public class SymbolItem : CustomControl {
            private CheckBox CheckBox;
            public string SymbolName;
            public bool Selected {
                get {
                    return CheckBox.IsChecked == false ? false : true;
                }
                set {
                    CheckBox.IsChecked = value;
                }
            }
            private event Action<CheckBox, CheckBoxEventArgs> OnSymbolSelectorCheck;
            public SymbolItem(string symbol, Action<CheckBox, CheckBoxEventArgs> SymbolSelectorCheck) {
                SymbolName = symbol;
                CheckBox = new CheckBox() {
                    Text = symbol,
                    FontSize = 12,
                    Margin = "20 0 0 0",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    //Style = Styles.CreateCheckBoxStyle()
                };
                CheckBox.Click += OnCheckBoxCheck;
                OnSymbolSelectorCheck += SymbolSelectorCheck;
                AddChild(CheckBox);
            }

            private void OnCheckBoxCheck(CheckBoxEventArgs e) {
                OnSymbolSelectorCheck.Invoke(this.CheckBox, e);
            }
        }

        public class WatchListItem : CustomControl {


            private CheckBox CheckBox;
            public List<SymbolItem> SymbolItems;
            public bool Selected {
                get {
                    return CheckBox.IsChecked==false?false:true;
                }
            }

            public WatchListItem(Watchlist watchlist) {
                StackPanel content = new StackPanel() {
                    Orientation = Orientation.Vertical
                };
                CheckBox = new CheckBox() {
                    Text = watchlist.Name,
                    FontSize = 16,
                    Margin = "0 10 0 10",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    //Style = Styles.CreateCheckBoxStyle(),
                    IsThreeState=true
                };
                CheckBox.Click += onWatchlistCheckChange;
                content.AddChild(CheckBox);

                SymbolItems = new List<SymbolItem>();
                foreach (string symbolname in watchlist.SymbolNames.ToArray()) {
                    SymbolItems.Add(new SymbolItem(symbolname, onSymbolCheckChange));
                }
                foreach(SymbolItem symbolselector in SymbolItems) {
                    content.AddChild(symbolselector);
                }
                AddChild(content);
            }

            private void onWatchlistCheckChange(CheckBoxEventArgs e) {
                if (e.CheckBox.IsChecked==null) {
                    CheckBox.IsChecked = false;
                }
                foreach (SymbolItem selector in SymbolItems) {
                    selector.Selected = CheckBox.IsChecked == false?false:true;
                }
            }
            private void onSymbolCheckChange(CheckBox sender, CheckBoxEventArgs e) {
                if (e.CheckBox.IsChecked==true) {
                    if (SymbolItems.Count(pred => pred.Selected == true) == SymbolItems.Count()) {
                        CheckBox.IsChecked = true;
                    }else {
                        CheckBox.IsChecked = null;
                    }
                } else {
                    if (SymbolItems.Count(pred => pred.Selected == false ) == SymbolItems.Count()) {
                        CheckBox.IsChecked = false;
                    }
                    else {
                        CheckBox.IsChecked = null;
                    }
                }
            }
        }

        public class TimeFrameItem : CustomControl {
            private CheckBox CheckBox;
            public TimeFrame TimeFrame;
            public bool Selected {
                get {
                    return CheckBox.IsChecked == false ? false : true;
                }
            }
            public TimeFrameItem(TimeFrame timeframe, Action<CheckBoxEventArgs> onTimeFrameCheck = null) {
                TimeFrame = timeframe;
                CheckBox = new CheckBox() {
                    Text = timeframe.ToString(),
                    FontSize = 12,
                    Margin = "10 0 0 0",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    //Style = Styles.CreateCheckBoxStyle()
                };
                if (onTimeFrameCheck != null)
                    CheckBox.Click += onTimeFrameCheck;
                
                AddChild(CheckBox);
            }
        }

        private Chart Chart;
        private Watchlists Watchlists;

        public ProgressBar LoadingBar;

        private StackPanel ConfigPanel;
        private ScrollViewer SymbolScroll;
        public List<WatchListItem> WatchlistItems;

        private StackPanel ResultPanel;
        private ScrollViewer ResultScroll;
        private List<ResultItem> ResultItems;

        private ScrollViewer TimeFrameScroll;
        public List<TimeFrameItem> TimeFrameItems;

        private DockPanel Framework;

        public event Action OnClickStart;

        public GUI(Chart chart, Watchlists watchlists) {
            Chart = chart;
            Watchlists = watchlists;

            //String desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);//getting location of user's Desktop folder  
            //String filePath = Path.Combine(desktopFolder, "ServerTimeExample.txt");
            //StreamWriter _fileWriter;
            //_fileWriter = File.WriteAllText(filePath,);
            //_fileWriter.AutoFlush = true;

            //DYNAMIC CONTROLS
            ResultItems = new List<ResultItem>();
            WatchlistItems = new List<WatchListItem>();
            TimeFrameItems = new List<TimeFrameItem>();

            //WATCHLIST & SYMBOL & TIMEFRAME
            StackPanel WatchListScrollPanel = new StackPanel {
                Orientation = Orientation.Vertical,
            };
            foreach (Watchlist entry in Watchlists) {
                WatchListItem control = new WatchListItem(entry);
                WatchlistItems.Add(control);
                WatchListScrollPanel.AddChild(control);
            }
            SymbolScroll = new ScrollViewer {
                BackgroundColor = Color.Transparent,
                Content = WatchListScrollPanel,
            };
            TextBlock TitleWatchlist = new TextBlock {
                Text = "Watchlists",
                Margin = "10 3 10 3",
                FontSize = 20,
            };
            Border TitleWatchlistBorder = new Border {
                BorderThickness = "0 0 0 1",
                Child = TitleWatchlist,
                Style = Styles.CreateCommonBorderStyle(),
                Dock = Dock.Top,
            };
            DockPanel WatchListPanel = new DockPanel {
                BackgroundColor = Color.Transparent,
            };
            WatchListPanel.AddChild(TitleWatchlistBorder);
            WatchListPanel.AddChild(SymbolScroll);
            Border WatchlistBorder = new Border {
                Child = WatchListPanel,
                Style = Styles.CreatePanelBackgroundStyle()
            };



            StackPanel TimeFrameScrollPanel = new StackPanel {
                Orientation = Orientation.Vertical,
            };
            foreach (TimeFrame item in Utils.TimeFrames) {
                TimeFrameItem control = new TimeFrameItem(item);
                TimeFrameItems.Add(control);
                TimeFrameScrollPanel.AddChild(control);
            }
            TimeFrameScroll = new ScrollViewer {
                BackgroundColor = Color.Transparent,
                Content = TimeFrameScrollPanel,
            };
            TextBlock TitleTimeFrame = new TextBlock {
                Text = "TimeFrames",
                Margin = "10 3 10 3",
                FontSize = 20,
            };
            Border TitleTimeFrameBorder = new Border {
                BorderThickness = "0 0 0 1",
                Child = TitleTimeFrame,
                Style = Styles.CreateCommonBorderStyle(),
                Dock = Dock.Top,
            };
            DockPanel TimeFramePanel = new DockPanel {
                BackgroundColor = Color.Transparent,
            };
            TimeFramePanel.AddChild(TitleTimeFrameBorder);
            TimeFramePanel.AddChild(TimeFrameScroll);
            Border TimeFrameBorder = new Border {
                Margin = "10 0 0 0",
                Child = TimeFramePanel,
                Style = Styles.CreatePanelBackgroundStyle()
            };


            //CONFIGURATION
            ConfigPanel = new StackPanel {
                Orientation = Orientation.Horizontal,
                Dock = Dock.Left,
                Margin = "0 0 10 0"
            };
            ConfigPanel.AddChild(WatchlistBorder);
            ConfigPanel.AddChild(TimeFrameBorder);
            ConfigPanel.IsVisible = false;

            //OPTION
            Button StartButton = new Button {
                Text = "Start",
                Width = 100,
                Margin = 5
            };
            StartButton.Click += OnButtonClick;
            Button ConfigButton = new Button {
                Text = "Option",
                Width = 100,
                Margin = 5
            };
            ConfigButton.Click += OnButtonClick;
            Button ResultButton = new Button {
                Text = "Result",
                Width = 100,
                Margin = 5
            };
            ResultButton.Click += OnButtonClick;
            StackPanel OptionPanel = new StackPanel {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Dock = Dock.Top,
                Margin = 2
            };
            
            OptionPanel.AddChild(ResultButton);
            OptionPanel.AddChild(ConfigButton);
            OptionPanel.AddChild(StartButton);

            //RESULT
            ResultPanel = new StackPanel {
                Orientation = Orientation.Vertical
            };
            ResultScroll = new ScrollViewer() {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = ResultPanel,
                IsVisible = false
            };

            //LOADING
            LoadingBar = new ProgressBar {
                Margin = 10,
                Dock = Dock.Bottom,
                Width = Chart.Width / 2,
                Height = 20,
                BackColor = Color.FromArgb(20, Color.Red),
                ForeColor = Color.FromArgb(80, Color.Red),
                BorderColor = Color.FromArgb(255, Color.Red)
            };

            //FRAMEWORK
            Framework = new DockPanel();
            Framework.AddChild(OptionPanel);
            Framework.AddChild(ConfigPanel);
            Framework.AddChild(LoadingBar);
            Framework.AddChild(ResultScroll);

            //CHART
            Chart.AddControl(Framework);
        }
        public void AddResult(ArmonicPattern pattern) {
            ResultItem result = new ResultItem(pattern);
            //ResultItem result = new ResultItem(ResultPanel, pattern);
            ResultItems.Add(result);
            ResultPanel.AddChild(result);
        }
        public void DeleteResult(ArmonicPattern pattern) {
            ResultItem result;
            result = ResultItems.FirstOrDefault(fnd => fnd.Key == pattern.GetKey());
            if (result != null) {
                //result.Destoy();
                ResultPanel.RemoveChild(result);
                ResultItems.Remove(result);
            }
        }

        public void DrawGrid(ArmonicPattern[] patterns) {
            ////int i = 0;

            ////if (GridResult != null) {
            ////    Chart.RemoveControl(GridResult);
            ////}

            ////GridResult = new Grid() {
            ////    BackgroundColor = Color.LightCyan,
            ////    ShowGridLines = true
            ////};

            ////GridResult.AddColumns(3);
            ////GridResult.Columns[0].SetWidthInStars(1);
            ////GridResult.Columns[1].SetWidthToAuto();
            ////GridResult.Columns[2].SetWidthInPixels(40);
            ////TextBlock HeadType = new TextBlock {
            ////    Text = "Pattern Type",
            ////    FontSize = 12,
            ////    Margin = 8
            ////};
            ////TextBlock HeadMode = new TextBlock {
            ////    Text = "Pattern Direction",
            ////    FontSize = 12,
            ////    Margin = 8
            ////};
            ////TextBlock HeadCompleated = new TextBlock {
            ////    Text = "Compleated",
            ////    FontSize = 12,
            ////    Margin = 8
            ////};
            
            ////GridResult.AddRow();
            ////GridResult.AddChild(HeadType, 0, 0);
            ////GridResult.AddChild(HeadMode, 0, 1);
            ////GridResult.AddChild(HeadCompleated, 0, 2);
            
            


            ////for (i = 0;  i == patterns.Count() - 1 ; i++) {
            ////    TextBlock Type = new TextBlock {
            ////        Text = patterns[i].Type.ToString(),
            ////        FontSize = 12,
            ////        Margin = 8
            ////    };
            ////    TextBlock Mode = new TextBlock {
            ////        Text = patterns[i].Mode.ToString(),
            ////        FontSize = 12,
            ////        Margin = 8
            ////    };
            ////    TextBlock Compleated = new TextBlock {
            ////        Text = patterns[i].Compleated == true ? "Yes" : "No",
            ////        FontSize = 12,
            ////        Margin = 8
            ////    };
            ////    GridResult.AddRow();
            ////    GridResult.AddChild(Type, i+1, 0);
            ////    GridResult.AddChild(Mode, i+1, 1);
            ////    GridResult.AddChild(Compleated, i+1, 2);

            ////}
            ////GridResult.Opacity = 0.1;
            ////Framework.AddChild(GridResult);

        }
        //public void AddSymbols(List<String> SymbolList, string WatchlistName) {
        //    CheckBox TitleCheckSymbolWatchlist = new CheckBox {
        //        Text = WatchlistName,
        //        FontSize = 12,
        //        Opacity= 0.8,
        //        Margin = 10
        //    };
        //    TitleCheckSymbolWatchlist.Checked += OnCheckWatchlistTitlesChange;
        //    TitleCheckSymbolWatchlist.Unchecked += OnCheckWatchlistTitlesChange;
        //    SymbolWatchlistTitles.Add(TitleCheckSymbolWatchlist);
        //    SymbolPanel.AddChild(TitleCheckSymbolWatchlist);
        //    foreach (string _symbol in SymbolList) {
        //        CheckBox _CheckBox = new CheckBox {
        //            Margin = 3,
        //            Text = _symbol
        //        };
        //        SymbolChecks.Add(_CheckBox);
        //        SymbolPanel.AddChild(SymbolChecks[SymbolChecks.Count - 1]);
        //    }
        //}
        //public void RemoveSymbols(List<String> SymbolList, string WatchlistName) {
        //    SymbolPanel.RemoveChild(SymbolWatchlistTitles.First(e => e.Text == WatchlistName));
        //    SymbolWatchlistTitles.RemoveAll(e => e.Text == WatchlistName);
        //    foreach (string _symbol in SymbolList) {
        //        SymbolPanel.RemoveChild(SymbolChecks.First(e => e.Text == _symbol));
        //        SymbolChecks.RemoveAll(e => e.Text == _symbol);
        //    }
        //}
        //private void OnCheckWatchChange(CheckBoxEventArgs args) {
        //    if (args.CheckBox.IsChecked == true) {
        //        AddSymbols(Watchlists.FirstOrDefault(e => (e.Name == args.CheckBox.Text)).SymbolNames.ToList(), args.CheckBox.Text);
        //    }
        //    else if (args.CheckBox.IsChecked == false) {
        //        RemoveSymbols(Watchlists.FirstOrDefault(e => (e.Name == args.CheckBox.Text)).SymbolNames.ToList(), args.CheckBox.Text);
        //    }
        //}
        //private void OnCheckWatchlistTitlesChange(CheckBoxEventArgs args) {
        //    //if (args.CheckBox.IsChecked == true) {
        //    //    AddSymbols(Watchlists.FirstOrDefault(e => (e.Name == args.CheckBox.Text)).SymbolNames.ToList(), args.CheckBox.Text);
        //    //}
        //    //else if (args.CheckBox.IsChecked == false) {
        //    //    RemoveSymbols(Watchlists.FirstOrDefault(e => (e.Name == args.CheckBox.Text)).SymbolNames.ToList(), args.CheckBox.Text);
        //    //}
        //}

        private void OnButtonClick(ButtonClickEventArgs obj) {
            switch (obj.Button.Text) {
                case ("Option"):
                    ConfigPanel.IsVisible = !ConfigPanel.IsVisible;
                    ResultScroll.IsVisible = false;
                    break;
                case ("Start"):
                    OnClickStart.Invoke();
                    break;
                case ("Result"):
                    ResultScroll.IsVisible = !ResultScroll.IsVisible;
                    ConfigPanel.IsVisible = false;
                    break;
            }
        }
    }

    public class ArmonicPatternEventArgs {
        public PatternEvent EventValue;
    }
    public class ArmonicPattern {
        //private ArmonicBot Bot;
        public string Key;
        public Symbol Symbol;
        public TimeFrame TimeFrame;
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
        public ArmonicPattern(Symbol symbol, TimeFrame timeframe , Segment bornSegment) {
            //Bot = bot;
            Symbol = symbol;
            TimeFrame = timeframe;
            Reset(bornSegment);
        }

        //public ArmonicPattern(ArmonicPattern PatternToCopy) {
        //    Update(PatternToCopy);
        //}

        public void Reset(Segment bornSegment) {
            Key = CreateKey(bornSegment);
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
        public void Update(ArmonicPattern PatternToCopy) {
            //Bot = bot;
            Symbol = PatternToCopy.Symbol;
            TimeFrame = PatternToCopy.TimeFrame;
            Key = PatternToCopy.Key;
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
        public string Report() {
            string result = "";
            result += string.Format("INIZIO ----------------------------------------------");
            result += string.Format("\n ID : {0}", GetKey());
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
        public string GetKey() {
            return String.Format("Key:{0}_", XA.FromOpenTime.ToString("dd/MM/yyyy:HHmmss"));
        }
        public string CreateKey() {
            return CreateKey(XA);
        }
        public string CreateKey(Segment segment) {
            return String.Format("Key:{0}_{1}_{2}", Symbol.Name, TimeFrame.ToString(), segment.FromOpenTime.ToString("dd/MM/yyyy:HHmmss"));
        }
    }
    //}
    //public class Statistics
    //{
    //    private readonly List<ArmonicPattern> Info;
    //    private readonly ArmonicBot Bot;

    //    public Statistics(ArmonicBot bot)
    //    {
    //        Bot = bot;
    //        Info = new List<ArmonicPattern>();

    //    }

    //    public void Add(ArmonicPattern pattern)
    //    {
    //        ArmonicPattern _info = new ArmonicPattern(pattern);

    //        //controllo se ho già registrato questo pattern
    //        bool _found = false;
    //        for (int _patternIndex = 0; _patternIndex < Info.Count(); _patternIndex++)
    //        {
    //            ArmonicPattern _pattern = Info[_patternIndex];
    //            if (_pattern.GetKey() == pattern.GetKey())
    //            {
    //                _found = true;
    //                break;
    //            }
    //        }
    //        //se non ho già il pattern in lista allora lo aggiungo
    //        if (!_found)
    //        {
    //            Info.Add(_info);
    //        }



    //    }
    //    public int GetCompleated(PatternType type)
    //    {
    //        int count = 0;

    //        foreach (ArmonicPattern pattern in Info)
    //        {
    //            if (pattern.Type == type && pattern.Compleated)
    //            {
    //                count++;
    //            }
    //        }
    //        return count;
    //    }
    //    public int GetSucceed(PatternType type)
    //    {
    //        int count = 0;

    //        foreach (ArmonicPattern pattern in Info)
    //        {
    //            if (pattern.Type == type && pattern.Succeed)
    //            {
    //                count++;
    //            }
    //        }
    //        return count;
    //    }
    //    public int GetFailed(PatternType type)
    //    {
    //        int count = 0;

    //        foreach (ArmonicPattern pattern in Info)
    //        {
    //            if (pattern.Type == type && pattern.Failed)
    //            {
    //                count++;
    //            }
    //        }
    //        return count;
    //    }
    //    public double GetWinLossRatio(PatternType type)
    //    {
    //        int countCompleated = GetCompleated(type);

    //        return countCompleated > 0 ? 100 * GetSucceed(type) / countCompleated : 0;
    //    }
    //    public void Print()
    //    {
    //        string text;
    //        text = Bot.Time.ToString();
    //        Bot.Chart.DrawStaticText("TIME", text, VerticalAlignment.Top, HorizontalAlignment.Right, Color.DarkCyan);
    //        text = string.Concat("GARTLEY Compleated ", GetCompleated(PatternType.Gartley));
    //        text = string.Concat(text, "     Succees ", GetSucceed(PatternType.Gartley));
    //        text = string.Concat(text, "     Failed ", GetFailed(PatternType.Gartley));
    //        text = string.Concat(text, "     Win/Loss Ratio ", GetWinLossRatio(PatternType.Gartley));
    //        Bot.Chart.DrawStaticText("STAT#1", text, VerticalAlignment.Top, HorizontalAlignment.Left, Color.Blue);
    //        text = string.Concat("\nCYPHER Compleated ", GetCompleated(PatternType.Cypher));
    //        text = string.Concat(text, "     Succees ", GetSucceed(PatternType.Cypher));
    //        text = string.Concat(text, "     Failed ", GetFailed(PatternType.Cypher));
    //        text = string.Concat(text, "     Win/Loss Ratio ", GetWinLossRatio(PatternType.Cypher));
    //        Bot.Chart.DrawStaticText("STAT#2", text, VerticalAlignment.Top, HorizontalAlignment.Left, Color.Orange);
    //        text = string.Concat("\n\nBUTTERFLY Compleated ", GetCompleated(PatternType.Butterfly));
    //        text = string.Concat(text, "     Succees ", GetSucceed(PatternType.Butterfly));
    //        text = string.Concat(text, "     Failed ", GetFailed(PatternType.Butterfly));
    //        text = string.Concat(text, "     Win/Loss Ratio ", GetWinLossRatio(PatternType.Butterfly));
    //        Bot.Chart.DrawStaticText("STAT#3", text, VerticalAlignment.Top, HorizontalAlignment.Left, Color.Purple);
    //        text = string.Concat("\n\n\nBAT Compleated ", GetCompleated(PatternType.Bat));
    //        text = string.Concat(text, "     Succees ", GetSucceed(PatternType.Bat));
    //        text = string.Concat(text, "     Failed ", GetFailed(PatternType.Bat));
    //        text = string.Concat(text, "     Win/Loss Ratio ", GetWinLossRatio(PatternType.Bat));
    //        Bot.Chart.DrawStaticText("STAT#4", text, VerticalAlignment.Top, HorizontalAlignment.Left, Color.Gray);

    //    }
    //}
    public class ArmonicFinderEngine {
        private readonly Symbol Symbol;
        private readonly TimeFrame MainTimeFrame;
        private readonly TimeFrame PrecisionTimeFrame;
        private readonly SegmentTracerEngine SegmentTracer;
        private readonly Chart Chart;
        private bool _loadTerminate = false;
        private bool _mainSymbol = false;
        private bool _signalPatternEvent = false;
        private List<ArmonicPattern> PatternList;
        public DataFlow MainData;
        public DataFlow PrecisionData;
        public int Periods { get; private set; }
        public string Key;
        //public event Action<ArmonicPattern> onNewPattern;
        //public event Action<ArmonicPattern> onDeletePattern;
        //public event Action<ArmonicPattern, ArmonicPatternEventArgs> onUpdatePattern;

        private event Action<ArmonicFinderEngine, double> onDataLoading;
        private event Action<ArmonicFinderEngine> onDataLoaded;
        public event Action<ArmonicPattern, PatternEvent> onPatternStateChanged;

        public ArmonicFinderEngine(MarketData marketdata, Symbol symbol, TimeFrame timeframe, Chart chart, int periods, bool mainSymbol = false) {
            int MinutesFineCalc;
            Key = String.Format("{0}-{1}", symbol.Name, timeframe.ToString());
            Periods = periods;
            Symbol = symbol;
            Chart = chart;
            MainTimeFrame = timeframe;
            MinutesFineCalc = Utils.TimeframeToMinutes(timeframe);
            PrecisionTimeFrame = Utils.MinutesToTimeFrame(MinutesFineCalc / GlobalParameter.SubPeriods);
            _mainSymbol = mainSymbol;
            PatternList = new List<ArmonicPattern>();
            SegmentTracer = new SegmentTracerEngine(this, GlobalParameter.DrawSwing);
            MainData = new DataFlow(marketdata, Symbol, MainTimeFrame);
            PrecisionData = new DataFlow(marketdata, Symbol, PrecisionTimeFrame);
        }

        public void Initialize(Action<ArmonicFinderEngine> loadedListner = null, Action<ArmonicFinderEngine, double> loadingListner = null) {
            Debug.Print("Initializing Engine for Symbol {0} {1}", Symbol.Name, MainTimeFrame.ToString());
            MainData.RequestBars(Periods, true, OnBarsLoaded, OnBarsLoading);
            PrecisionData.RequestBars(Periods * GlobalParameter.SubPeriods, true, OnBarsLoaded, OnBarsLoading);

            if (loadingListner != null)
                onDataLoading += loadingListner;
            if (loadedListner != null)
                onDataLoaded += loadedListner;
        }
        protected void OnBarsLoading(double percentage, int count, string SymbolName) {
            int totBars = 0;
            int loadBars = 0;
            double percent = 0;

            totBars = MainData.GetBarsRequestedCount() + PrecisionData.GetBarsRequestedCount();
            loadBars = MainData.GetBarsLoadedCount() + PrecisionData.GetBarsLoadedCount();
            percent = (loadBars * 100) / totBars;
            //Debug.Print("Loading Data : {0}% ({1} of {2} Bars) ...", (loadBars * 100) / totBars, loadBars, totBars);

            if (onDataLoading != null)
                onDataLoading.Invoke(this, percent);
        }
        protected void OnBarsLoaded(DataFlow sender) {
            if (MainData.isBarLoadCompleted && PrecisionData.isBarLoadCompleted) {
                //Debug.Print("Loading Compleated.");
                if (onDataLoaded != null)
                    onDataLoaded.Invoke(this);

                
                MainData.RequestTicks(MainData.BarsData.LastBar.OpenTime);
                
                

                SimulatePatterns(Symbol, MainTimeFrame);

                if (!_mainSymbol) {
                    MainData.TraceNewBar(On_NewBar);
                    MainData.TraceNewTick(On_NewTick);
                }

                _loadTerminate = true;
            }
        }
        public void On_NewBar(BarOpenedEventArgs e = null) {
            //bool ok = false;
            //ArmonicPattern[] tmpList;
            if (!_loadTerminate) return;

            if (e != null)
                Debug.Print("New Bar Opened At {0}  Incoming From Symbol {1} in TimeFrame {2}", e.Bars.LastBar.OpenTime.ToString("dd/MM/yyyy:HHmmss"), e.Bars.SymbolName, e.Bars.TimeFrame.ToString());


            // calcola i segmenti
            SegmentTracer.Calculate(MainData.BarsData.LastBar, MainData.BarsData.Last(1), false);

            //// copia l'array dei pattern prima del calcolo in un nuovo Array
            //tmpList = new ArmonicPattern[PatternList.Count()];
            //PatternList.CopyTo(tmpList);

            Calculate();
            //confronta gli array e genera eventi per Aggiunta , Cancellzaione , Modifica (con oggetto di modifica)
            //scorri pattern calcolati
            //foreach (ArmonicPattern pattern in PatternList) {
            //    string patternkey = pattern.GetKey();
            //    //cerca pattern temporanei per chiave
            //    ok = false;
            //    foreach (ArmonicPattern tmpPattern in tmpList) {
            //        string tmpkey = tmpPattern.GetKey();
            //        if (tmpkey==patternkey) {
            //            ok = true;
            //            break;
            //        }
            //    } 
            //    if (!ok) {
            //        //se non trovi segnala aggiunta
            //        if (onNewPattern != null)
            //            onNewPattern.Invoke(pattern);
            //    }
            //}
            ////cerca pattern temporanei per chiave
            //foreach (ArmonicPattern tmpPattern in tmpList) {
            //    string tmpkey = tmpPattern.GetKey();
            //    //cerca nei pattern calcolati se lo trovi per chiave
            //    //se non lo trovi segnala elimina
            //    ok = false;
            //    foreach (ArmonicPattern pattern in PatternList) {
            //        string patternkey = pattern.GetKey();
            //        if (tmpkey == patternkey) {
            //            ok = true;
            //            break;
            //        }
            //    }
            //    if (!ok) {
            //        //se non lo trovi segnala elimina
            //        if (onDeletePattern != null)
            //            onDeletePattern.Invoke(tmpPattern);
            //    }
            //}
        }
        public void On_NewTick(TicksTickEventArgs e = null) {
            //Debug.Print("New Tick");
            ArmonicPattern[] oldList;
            ArmonicPattern oldPattern;
            string tmpKey;

            if (!_loadTerminate) return;

            //oldList = new ArmonicPattern[PatternList.Count()];
            //int i = 0;
            //foreach (ArmonicPattern pattern in PatternList) {
            //    oldList[i] = new ArmonicPattern(pattern);
            //    i++;
            //}
            

            FineCalculate(false, Symbol.Bid, MainData.BarsData.LastBar.OpenTime);

            //foreach (ArmonicPattern newPattern in PatternList.Where(objNew => objNew.Closed == false)) {
            //    tmpKey = newPattern.GetKey();
            //    oldPattern = oldList.First(objOld => objOld.GetKey() == tmpKey);

            //    if (oldPattern == null) {

            //        //si aggiunge un pattern
            //        if (onNewPattern != null && newPattern.Drawable)
            //            onNewPattern.Invoke(newPattern);
            //    } else {
            //        if (oldPattern.Drawable == false && newPattern.Drawable == true) {
            //            if (onNewPattern != null)
            //                onNewPattern.Invoke(newPattern);
            //        }
            //        else if (oldPattern.Drawable == true && newPattern.Drawable == false) {
            //            if (onDeletePattern != null)
            //                onDeletePattern.Invoke(newPattern);
            //        }
            //        else {
            //            if (newPattern.Drawable) {
            //                ArmonicPatternEventArgs PatternArgs = new ArmonicPatternEventArgs();
            //                PatternArgs.EventValue = PatternEvent.NoEvent;

            //                if (oldPattern.Compleated != newPattern.Compleated) {
            //                    PatternArgs.EventValue = PatternEvent.Compleated;
            //                }
            //                if (oldPattern.Target1Compleated != newPattern.Target1Compleated) {
            //                    PatternArgs.EventValue = PatternEvent.Target1;
            //                }
            //                if (oldPattern.Target2Compleated != newPattern.Target2Compleated) {
            //                    PatternArgs.EventValue = PatternEvent.Target2;
            //                }
            //                if (onUpdatePattern != null)
            //                    onUpdatePattern.Invoke(newPattern, PatternArgs);
            //            }
            //        }
            //    }
            //}
          

        }

        //private void LoadPatterns(Symbol RequestSymbol, TimeFrame RequestTimeFrame) {
        //    DateTime FromDate, ToDate;
        //    int _index = 0;
        //    if (_loadTerminate && MainData.BarsData.Count() >= Periods) {
        //        //creo la segmentazione
        //        for (_index = Periods; _index > 1; _index--) {
        //            SegmentTracer.Calculate(MainData.BarsData.Last(_index - 1), MainData.BarsData.Last(_index), false);
        //        }

        //        //cerco fino al punto "C" all'interno della segmentazione
        //        Calculate();


        //        //effettuo il controllo di precisione che cerca il punto di completamento "D" e il proseguo del pattern
        //        //questo controllo dovrebbe essere fatto per singolo pattern a partire dal punto "C" fino alla fine
        //        //va passata alla funzione un parametro opzionale in più che sarebbe la chiave del patter corrente
        //        //dentro alla funzione va filtrata questa condizione
        //        for (_index = 0; _index < PatternList.Count(); _index++) {
        //            FromDate = PatternList[_index].BC.ToOpenTime;
        //            ToDate = ToDate = MainData.BarsData.LastBar.OpenTime;
        //            foreach (Bar FineBar in PrecisionData.BarsData.Where(data => (data.OpenTime >= FromDate) && (data.OpenTime <= ToDate))) {
        //                FineCalculate(true, 0, FineBar.OpenTime, PatternList[_index], FineBar);
        //            }
        //        }
        //        FineCalculate(false, RequestSymbol.Bid, MainData.BarsData.Last(0).OpenTime);
        //    }
        //}

        private void SimulatePatterns(Symbol RequestSymbol, TimeFrame RequestTimeFrame) {
            DateTime FromDate, ToDate;

            if (MainData.BarsData.Count() >= Periods) {
                for (int _index = Periods; _index > 1; _index--) {
                    SegmentTracer.Calculate(MainData.BarsData.Last(_index - 1), MainData.BarsData.Last(_index), false);

                    Calculate();

                    if (_index - 2 > 0) {
                        FromDate = MainData.BarsData.Last(_index - 2).OpenTime;
                        if (_index - 3 > 0) {
                            ToDate = MainData.BarsData.Last(_index - 3).OpenTime;
                        }
                        else {
                            ToDate = MainData.BarsData.LastBar.OpenTime;
                        }

                        foreach (Bar FineBar in PrecisionData.BarsData.Where(data => (data.OpenTime >= FromDate) && (data.OpenTime <= ToDate))) {
                            FineCalculate(true, 0, MainData.BarsData.Last(_index - 2).OpenTime, null, FineBar);
                        }
                    }
                }
                //Bar _null = new Bar();
                FineCalculate(false, RequestSymbol.Bid, MainData.BarsData.Last(0).OpenTime);

            }

            //scorri tutti i pattern e segnala gli eventi di aggiunta
            foreach (ArmonicPattern pattern in PatternList.Where(obj => obj.Drawable==true && obj.Closed==false)) {
                onPatternStateChanged.Invoke(pattern,PatternEvent.Add);
            }
            _signalPatternEvent = true;
        }


        private int SegmentPeriod(Segment segment) {
            int _retValue;
            int _leftIndex;
            int _rightIndex;
            _leftIndex = MainData.BarsData.OpenTimes.GetIndexByExactTime(segment.FromOpenTime);
            _rightIndex = MainData.BarsData.OpenTimes.GetIndexByExactTime(segment.ToOpenTime);
            _retValue = _rightIndex - _leftIndex;
            return _retValue;

        }
        private double PriceRetracement(Segment segment, double retracement) {
            double delta = segment.Direction == Direction.Up ? segment.ToPrice - segment.FromPrice : segment.FromPrice - segment.ToPrice;
            double deltaRetracement = delta * retracement;
            double value = segment.Direction == Direction.Up ? segment.ToPrice - deltaRetracement : segment.ToPrice + deltaRetracement;

            return value;
        }
        private double MeasueSwing(Segment leftSegment, Segment rightSegment) {
            double a, b;
            a = rightSegment.Measure();
            b = leftSegment.Measure();
            return a / b;
        }
        private bool VerifyAndUpdateAB(ArmonicPattern pattern, Segment AB) {
            double _value;
            //CALCOLA IL RAPPORTO TRA XA E AB
            _value = MeasueSwing(pattern.XA, AB);
            ////PUO' SUCCEDERE CHE
            if (_value >= 1) {
                //B è sotto ad X : IL CALCOLO INVALIDA IL PATTERN
                //pattern.CanDraw = false;
                return false;
            }
            else if (B_InRange(_value)) {
                //B è valido per almeno un pattern

                pattern.AB_Percent = _value;
                pattern.AB = AB;
                pattern.AB_Period = SegmentPeriod(pattern.AB);

                pattern.Step = PatternStep.CFinding;
                return true;
            }
            else {
                //IL CALCOLO NON SEGNALA UN OBIETTIVO, LA RICERCA DI B PROSEGUE CON IL PROSSIMO SEGMENTO 
                return true;
            }
        }
        private bool VerifyAndUpdateBC(ArmonicPattern pattern, Segment BC) {
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
            if (pattern.AB_Percent >= 0.786 && _valueABBC >= 0.886) {
                return false;
            }

            //PUO' SUCCEDERE CHE
            if (_valueXAXC > 1.414) {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //C è oltre l'ultimo pattern accettabile (cypher), in questo caso ridefinisco XA e riparto alla ricerca di B 
                // in realtà bisognerebbe vedere se la chiusura della candela supera il 1.414 perchè se solo l'ombra va oltre il pattern è accettato
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                _segment = pattern.XA;
                _segment.Extend(BC.ToPrice, BC.ToOpenTime);
                // se il pattern era in lista allora devi eliminarlo.
                if (_signalPatternEvent)
                    onPatternStateChanged.Invoke(pattern, PatternEvent.Remove);
                DeletePatternInList(pattern);
                pattern.Reset(_segment);
                pattern.XA_Period = SegmentPeriod(pattern.XA);
                return true;
            }
            else {
                PatternType _type = C_IdentifyPattern(pattern, _valueABBC, _valueXAXC);
                switch (_type) {
                    case PatternType.Undefined:
                        //IL CALCOLO NON SEGNALA UN OBIETTIVO, LA RICERCA DI C PROSEGUE CON IL PROSSIMO SEGMENTO 
                        //un eventuale pattern già registrato per la gamba XA va eliminato
                        if (_signalPatternEvent)
                            onPatternStateChanged.Invoke(pattern, PatternEvent.Remove);
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
                        if (_type == PatternType.Cypher) {
                            pattern.BC_Percent = _valueXAXC;
                        }
                        else {
                            pattern.BC_Percent = _valueABBC;
                        }
                        pattern.BC_Period = SegmentPeriod(pattern.BC);


                        switch (_type) {
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
                        if (pattern.PRZ.FromPrice > pattern.PRZ.ToPrice) {
                            double _fromPrice = pattern.PRZ.FromPrice + ((pattern.PRZ.FromPrice - pattern.PRZ.ToPrice) * 2);
                            //double _fromPrice = pattern.BC.ToPrice;
                            double _toPrice = pattern.StopLoss;
                            pattern.DrawableArea = new Area(_fromPrice, _toPrice);
                        }
                        else {
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
        private bool B_InRange(double retracementValue) {
            if ((retracementValue >= 0.5 && retracementValue < 0.618) || (retracementValue >= 0.618 && retracementValue < 0.786) || (retracementValue >= 0.382 && retracementValue < 0.618) || (retracementValue >= 0.786 && retracementValue < 1)) {
                return true;
            }
            else {
                return false;
            }
        }
        private PatternType C_IdentifyPattern(ArmonicPattern pattern, double retracementValueABBC, double retracementValueXAXC) {
            PatternType _type = PatternType.Undefined;

            if (!pattern.CPointOverA && (pattern.AB_Percent >= 0.5 && pattern.AB_Percent < 0.618) && (retracementValueABBC >= 0.382 && retracementValueABBC < 1)) {
                //BAT
                _type = PatternType.Bat;
            }
            if (!pattern.CPointOverA && (pattern.AB_Percent >= 0.618 && pattern.AB_Percent < 0.786) && (retracementValueABBC >= 0.618 && retracementValueABBC < 1)) {
                //GARTLEY
                _type = PatternType.Gartley;
            }
            if ((pattern.AB_Percent >= 0.382 && pattern.AB_Percent < 0.618) && (retracementValueABBC >= 1) && (retracementValueXAXC >= 1.272 && retracementValueXAXC < 1.414)) {
                //CYPHER
                _type = PatternType.Cypher;
            }
            if (!pattern.CPointOverA && (pattern.AB_Percent >= 0.786 && pattern.AB_Percent < 1) && (retracementValueABBC >= 0.382 && retracementValueABBC < 0.886)) {
                //BUTTERFLY
                _type = PatternType.Butterfly;
            }

            return _type;

        }
        private void RecordPatternInList(ArmonicPattern pattern) {
            //controllo se ho già registrato questo pattern
            bool _found = false;
            for (int _patternIndex = 0; _patternIndex < PatternList.Count(); _patternIndex++) {
                ArmonicPattern _pattern = PatternList[_patternIndex];
                if (_pattern.GetKey() == pattern.GetKey()) {
                    _found = true;
                    //effettuo l'aggiornamento solo se il pattern non è copletato
                    if (!_pattern.Compleated) {
                        PatternList[_patternIndex] = pattern;
                    }

                    break;
                }
            }
            //se non ho già il pattern in lista allora lo aggiungo
            if (!_found) {
                PatternList.Add(pattern);
            }
        }
        private void DeletePatternInList(ArmonicPattern pattern) {

            //if (pattern.Compleated) {
            //    return;
            //}
            DeleteDrawPattern(pattern, Chart);

            for (int _patternIndex = 0; _patternIndex < PatternList.Count(); _patternIndex++) {
                ArmonicPattern _pattern = PatternList[_patternIndex];
                if (_pattern.GetKey() == pattern.GetKey()) {
                    PatternList.RemoveAt(_patternIndex);
                    break;
                }
            }

        }
        private void Calculate() {
            int _dimensionArea;
            Segment _startSegment;
            Segment _measureSegment;
            Segment _newXA, _newAB, _newBC;
            Segment[] Area;
            ArmonicPattern Pattern;
            bool _stopFind;

            //SegmentTracer.Calculate(MainData.BarsData.Last(1), MainData.BarsData.Last(2), true);

            //se non ho almeno 4 segmenti da analizzare è inutile che cerco i pattern
            if (SegmentTracer.SegmentList.Count() < 3) {
                return;
            }
            //analizzo in ordine ogni segmento da sinistra verso destra
            for (int _index = 0; _index < SegmentTracer.SegmentList.Count(); _index++) {
                // calcolo al dimensione dell'area a destra del segmento che stò analizzando
                _dimensionArea = SegmentTracer.SegmentList.Count() - (_index + 1);
                //se non ho almeno 3 segmenti a destra allora è inutile che cerco i pattern
                if (_dimensionArea < 2) {
                    break;
                }
                //il primo segmento rappresenta inizialmente la gamba XA
                _startSegment = new Segment(SegmentTracer.SegmentList[_index].FromPrice, SegmentTracer.SegmentList[_index].ToPrice, SegmentTracer.SegmentList[_index].FromOpenTime, SegmentTracer.SegmentList[_index].ToOpenTime);
                //creo un nuovo pattern assegnando la gamba iniziale XA
                Pattern = new ArmonicPattern(Symbol, MainTimeFrame, _startSegment);
                Pattern.XA_Period = SegmentPeriod(Pattern.XA);
                //popolo l'area dei segmenti a destra da analizzare rispetto al segmento che stò analizzando
                Area = new Segment[_dimensionArea];
                SegmentTracer.SegmentList.CopyTo((_index + 1), Area, 0, SegmentTracer.SegmentList.Count() - (_index + 1));
                //resetto questa variabile che indica la condizione in cui non ha più senso continuare la ricerca nell'area dei segmenti perchè nessun pattern può essere valido rispetto al segmento che stò analizzando
                _stopFind = false;
                //trace debug
                //debugOn = false;
                //if (Bot.Time.ToString("dd/MM/yyyy:HHmmss") == "14/01/2020:180000")
                //{
                //    if (_startSegment.FromOpenTime.ToString("dd/MM/yyyy:HHmmss") == "14/01/2020:080000")
                //    {
                //        //strumento ITA40
                //        //debugOn = true;
                //    }
                //}

                foreach (Segment _segment in Area) {
                    switch (Pattern.Step) {
                        case PatternStep.BFinding:
                            //se XA appartiene ad un patter completato allora salto l'elaborazione 
                            //questo perchè gli algoritmi di ricerca dei punti B e C (che ricalcolano tutto al completamento di un periodo) 
                            //possono trovarsi nella situazione di dover eliminare un pattern registrato per ridefinirne le gambe
                            //ed in questo caso si perderebbero le informazioni di completamento del pattern.
                            for (int _patternIndex = 0; _patternIndex < PatternList.Count(); _patternIndex++) {
                                ArmonicPattern _pattern = PatternList[_patternIndex];
                                if (_pattern.GetKey() == Pattern.GetKey()) {

                                    //esco dalla ricerca se esiste un pattern completo per questa gamba
                                    if (_pattern.Compleated) {
                                        _stopFind = true;
                                    }
                                    break;
                                }
                            }

                            //controllo se la segmentazione è arrivata ad una continuazione di XA
                            if ((_segment.ToPrice > Pattern.XA.ToPrice && Pattern.Mode == PatternMode.Bullish) || (_segment.ToPrice < Pattern.XA.ToPrice && Pattern.Mode == PatternMode.Bearish)) {
                                // estendo la gamba XA
                                Pattern.XA.Extend(_segment.ToPrice, _segment.ToOpenTime);
                                Pattern.XA_Period = SegmentPeriod(Pattern.XA);
                                break;
                            }
                            //creo il segmento che va da A a B
                            _measureSegment = new Segment(Pattern.XA.ToPrice, _segment.ToPrice, Pattern.XA.ToOpenTime, _segment.ToOpenTime);
                            //faccio i conti con la gamba AB ed eventualmente aggiorno il pattern se ho trovato un punto B valido
                            if (VerifyAndUpdateAB(Pattern, _measureSegment) == false) {
                                //B invalida la gamba XA, comporta che : dal segmento iniziale non può più nascere nessun pattern quindi interrompo la ricerca e passo al segmento successivo
                                _stopFind = true;
                            }
                            break;
                        case PatternStep.CFinding:
                            //controllo se la segmentazione è arrivata sopra ad A
                            //quindi gestisco il punto BC_OverA
                            if ((Pattern.Mode == PatternMode.Bullish && _segment.ToPrice > Pattern.XA.ToPrice) || (Pattern.Mode == PatternMode.Bearish && _segment.ToPrice < Pattern.XA.ToPrice)) {
                                Pattern.CPointOverA = true;
                                if (Pattern.BC_OverA == null) {
                                    Pattern.BC_OverA = new Segment(Pattern.AB.ToPrice, _segment.ToPrice, Pattern.AB.ToOpenTime, _segment.ToOpenTime);
                                }
                                else {
                                    if ((Pattern.Mode == PatternMode.Bullish && _segment.ToPrice > Pattern.BC_OverA.ToPrice) || (Pattern.Mode == PatternMode.Bearish && _segment.ToPrice < Pattern.BC_OverA.ToPrice)) {
                                        Pattern.BC_OverA.Extend(_segment.ToPrice, _segment.ToOpenTime);
                                    }
                                }
                            }

                            //controllo se la segmentazione è arrivata ad una continuazione di AB
                            if ((_segment.ToPrice < Pattern.AB.ToPrice && Pattern.Mode == PatternMode.Bullish) || (_segment.ToPrice > Pattern.AB.ToPrice && Pattern.Mode == PatternMode.Bearish)) {
                                // controllo se il punto C NON ha mai oltrepassato il punto A
                                if (!Pattern.CPointOverA) {
                                    // estendo la gamba AB
                                    Pattern.AB.Extend(_segment.ToPrice, _segment.ToOpenTime);
                                    //faccio i conti con la gamba AB ed eventualmente aggiorno il pattern se ho trovato un punto B valido
                                    if (VerifyAndUpdateAB(Pattern, Pattern.AB) == false) {
                                        //B invalida la gamba XA, comporta che : dal segmento iniziale non può più nascere nessun pattern quindi interrompo la ricerca e passo al segmento successivo
                                        _stopFind = true;
                                    }
                                }
                                else {
                                    //E' una continuazione di AB ma il punto C ha precedentemente optreppassato A
                                    // estendo la gamba XA fino al punto C sopra ad A
                                    // ridefinisco la gamba AB
                                    _newXA = new Segment(Pattern.XA.FromPrice, Pattern.BC_OverA.ToPrice, Pattern.XA.FromOpenTime, Pattern.BC_OverA.ToOpenTime);
                                    _newAB = new Segment(Pattern.BC_OverA.ToPrice, _segment.ToPrice, Pattern.BC_OverA.ToOpenTime, _segment.ToOpenTime);

                                    Pattern.Reset(_newXA);
                                    Pattern.XA_Period = SegmentPeriod(Pattern.XA);
                                    //faccio i conti con la gamba AB ed eventualmente aggiorno il pattern se ho trovato un punto B valido
                                    if (VerifyAndUpdateAB(Pattern, _newAB) == false) {
                                        //B invalida la gamba XA, comporta che : dal segmento iniziale non può più nascere nessun pattern quindi interrompo la ricerca e passo al segmento successivo
                                        _stopFind = true;
                                    }

                                }
                                // esco dalla ricerca del punto C
                                break;
                            }
                            //creo il segmento per la misurazione del ritracciamento BC
                            _measureSegment = new Segment(Pattern.AB.ToPrice, _segment.ToPrice, Pattern.AB.ToOpenTime, _segment.ToOpenTime);
                            if (VerifyAndUpdateBC(Pattern, _measureSegment) == false) {
                                if (_signalPatternEvent)
                                    onPatternStateChanged.Invoke(Pattern, PatternEvent.Remove);
                                DeletePatternInList(Pattern);
                                _stopFind = true;
                            }
                            break;
                        case PatternStep.DFinding:


                            //controllo se la segmentazione ridefinisce il punto B
                            //quindi gestisco il punto CD_OverB
                            if ((Pattern.Mode == PatternMode.Bullish && _segment.ToPrice < Pattern.AB.ToPrice) || (Pattern.Mode == PatternMode.Bearish && _segment.ToPrice > Pattern.AB.ToPrice)) {
                                Pattern.DPointOverB = true;
                                if (Pattern.CD_OverB == null) {
                                    Pattern.CD_OverB = new Segment(Pattern.BC.ToPrice, _segment.ToPrice, Pattern.BC.ToOpenTime, _segment.ToOpenTime);
                                }
                                else {
                                    if ((Pattern.Mode == PatternMode.Bullish && _segment.ToPrice < Pattern.CD_OverB.ToPrice) || (Pattern.Mode == PatternMode.Bearish && _segment.ToPrice > Pattern.CD_OverB.ToPrice)) {
                                        Pattern.CD_OverB.Extend(_segment.ToPrice, _segment.ToOpenTime);
                                    }
                                }
                            }

                            // controllo se il punto D è arrivato oltre lo StopLoss
                            // in questo caso devo eliminare il pattern
                            if (_segment.ToPrice < Pattern.StopLoss && Pattern.Mode == PatternMode.Bullish || _segment.ToPrice > Pattern.StopLoss && Pattern.Mode == PatternMode.Bearish) {
                                if (_signalPatternEvent)
                                    onPatternStateChanged.Invoke(Pattern, PatternEvent.Remove);
                                DeletePatternInList(Pattern);
                                _stopFind = true;
                            }

                            // controllo se il punto D è arrivato ad una continuazione di BC
                            if ((_segment.ToPrice > Pattern.BC.ToPrice && Pattern.Mode == PatternMode.Bullish) || (_segment.ToPrice < Pattern.BC.ToPrice && Pattern.Mode == PatternMode.Bearish)) {
                                if (Pattern.DPointOverB) {

                                    //se il segmento CD_OverB  nel suo percorso è arrivato sotto al punto X allora elimino il pattern qualsiasi esso sia
                                    if ((Pattern.CD_OverB.ToPrice < Pattern.XA.FromPrice && Pattern.Mode == PatternMode.Bullish) || (Pattern.CD_OverB.ToPrice > Pattern.XA.FromPrice && Pattern.Mode == PatternMode.Bearish)) {
                                        //DeletePatternInList(Pattern);
                                        //_stopFind = true;

                                        //? possibile ?
                                        //debugOn = true;
                                    }


                                    // nel caso del cypher vanno ridefinite le gambe XA AB BC in un modo differente ossia:
                                    // XA = XC, AB = C-D_OverB, BC = D_OverB - D
                                    if (Pattern.Type == PatternType.Cypher) {
                                        //debugOn = true;
                                        _newXA = new Segment(Pattern.XA.FromPrice, Pattern.BC.ToPrice, Pattern.XA.FromOpenTime, Pattern.BC.ToOpenTime);
                                        _newAB = new Segment(Pattern.BC.ToPrice, Pattern.CD_OverB.ToPrice, Pattern.BC.ToOpenTime, Pattern.CD_OverB.ToOpenTime);
                                        _newBC = new Segment(Pattern.CD_OverB.ToPrice, _segment.ToPrice, Pattern.CD_OverB.ToOpenTime, _segment.ToOpenTime);
                                    }
                                    else {
                                        // per tutti gli altri pattern va bene quanto sotto
                                        // devo ridefinire la gamba AB testando che sia valida e devo testare una nuova gamba BC
                                        _newXA = new Segment(Pattern.XA.FromPrice, Pattern.XA.ToPrice, Pattern.XA.FromOpenTime, Pattern.XA.ToOpenTime);
                                        _newAB = new Segment(Pattern.XA.ToPrice, Pattern.CD_OverB.ToPrice, Pattern.XA.ToOpenTime, Pattern.CD_OverB.ToOpenTime);
                                        _newBC = new Segment(Pattern.CD_OverB.ToPrice, _segment.ToPrice, Pattern.CD_OverB.ToOpenTime, _segment.ToOpenTime);
                                    }

                                    Pattern.Reset(_newXA);
                                    Pattern.XA_Period = SegmentPeriod(Pattern.XA);
                                    if (VerifyAndUpdateAB(Pattern, _newAB) == false) {
                                        //B invalida la gamba XA, comporta che : dal segmento iniziale non può più nascere nessun pattern quindi interrompo la ricerca e passo al segmento successivo
                                        if (_signalPatternEvent)
                                            onPatternStateChanged.Invoke(Pattern, PatternEvent.Remove);
                                        DeletePatternInList(Pattern);
                                        _stopFind = true;
                                        break;
                                    }

                                    //controllo se la segmentazione è arrivata sopra ad A
                                    //quindi gestisco il punto BC_OverA
                                    if ((Pattern.Mode == PatternMode.Bullish && _segment.ToPrice > Pattern.XA.ToPrice) || (Pattern.Mode == PatternMode.Bearish && _segment.ToPrice < Pattern.XA.ToPrice)) {
                                        //debugOn = true;
                                        Pattern.CPointOverA = true;
                                        if (Pattern.BC_OverA == null) {
                                            Pattern.BC_OverA = new Segment(Pattern.AB.ToPrice, _segment.ToPrice, Pattern.AB.ToOpenTime, _segment.ToOpenTime);
                                        }
                                        else {
                                            if ((Pattern.Mode == PatternMode.Bullish && _segment.ToPrice > Pattern.BC_OverA.ToPrice) || (Pattern.Mode == PatternMode.Bearish && _segment.ToPrice < Pattern.BC_OverA.ToPrice)) {
                                                Pattern.BC_OverA.Extend(_segment.ToPrice, _segment.ToOpenTime);
                                            }
                                        }
                                    }

                                    if (VerifyAndUpdateBC(Pattern, _newBC) == false) {
                                        if (_signalPatternEvent)
                                            onPatternStateChanged.Invoke(Pattern, PatternEvent.Remove);
                                        DeletePatternInList(Pattern);
                                        _stopFind = true;
                                    }

                                }
                                else {
                                    //D è una continuazione di BC ma il punto B rimane invariato quindi ridefinisco la gamba BC
                                    _newXA = new Segment(Pattern.XA.FromPrice, Pattern.XA.ToPrice, Pattern.XA.FromOpenTime, Pattern.XA.ToOpenTime);
                                    _newAB = new Segment(Pattern.AB.FromPrice, Pattern.AB.ToPrice, Pattern.AB.FromOpenTime, Pattern.AB.ToOpenTime);
                                    _newBC = new Segment(Pattern.AB.ToPrice, _segment.ToPrice, Pattern.AB.ToOpenTime, _segment.ToOpenTime);

                                    Pattern.Reset(_newXA);
                                    Pattern.XA_Period = SegmentPeriod(Pattern.XA);
                                    if (VerifyAndUpdateAB(Pattern, _newAB) == false) {
                                        //B invalida la gamba XA, comporta che : dal segmento iniziale non può più nascere nessun pattern quindi interrompo la ricerca e passo al segmento successivo
                                        if (_signalPatternEvent)
                                            onPatternStateChanged.Invoke(Pattern, PatternEvent.Remove);
                                        DeletePatternInList(Pattern);
                                        _stopFind = true;
                                        break;
                                    }

                                    //OCCORRE FARE UNA VALUTAZIONE IN PIU' 
                                    //copia e incolla dal CFinding
                                    //----------------------------------------------------------
                                    //controllo se la segmentazione è arrivata sopra ad A
                                    //quindi gestisco il punto BC_OverA
                                    if ((Pattern.Mode == PatternMode.Bullish && _segment.ToPrice > Pattern.XA.ToPrice) || (Pattern.Mode == PatternMode.Bearish && _segment.ToPrice < Pattern.XA.ToPrice)) {
                                        Pattern.CPointOverA = true;
                                        if (Pattern.BC_OverA == null) {
                                            Pattern.BC_OverA = new Segment(Pattern.AB.ToPrice, _segment.ToPrice, Pattern.AB.ToOpenTime, _segment.ToOpenTime);
                                        }
                                        else {
                                            if ((Pattern.Mode == PatternMode.Bullish && _segment.ToPrice > Pattern.BC_OverA.ToPrice) || (Pattern.Mode == PatternMode.Bearish && _segment.ToPrice < Pattern.BC_OverA.ToPrice)) {
                                                Pattern.BC_OverA.Extend(_segment.ToPrice, _segment.ToOpenTime);
                                            }
                                        }
                                    }
                                    if (VerifyAndUpdateBC(Pattern, _newBC) == false) {
                                        if (_signalPatternEvent)
                                            onPatternStateChanged.Invoke(Pattern, PatternEvent.Remove);
                                        DeletePatternInList(Pattern);
                                        _stopFind = true;
                                    }
                                }
                                break;
                            }
                            break;
                    }
                    if (_stopFind) {
                        break;
                    }
                }
            }

            //si applica qui il criterio di filtro per pattern dello stesso tipo con punti A,B,C in comune
            //si deve tener conto del rapporto di ampiezza tra le gambe XA e del rapporto dei periodi delle gambe XA


            //scorro i pattern per disegnarli a video
            for (int _index = 0; _index < PatternList.Count(); _index++) {
                DrawPattern(PatternList[_index], Chart, Symbol.Bid);
            }
        }
        private void FineCalculate(bool initialize, double price, DateTime openTime, ArmonicPattern justThisPattern = null, Bar bar = new Bar()) {
            ArmonicPattern pattern;
            double TradePrice;
            //bool trigCompleated = false;

            for (int _index = 0; _index < PatternList.Count(); _index++) {
                //se viene passato il filtor per un solo pattern controllo ed eventualmente passo al prossimo
                pattern = PatternList[_index];
                if (justThisPattern != null) {
                    if (justThisPattern.GetKey() != pattern.GetKey()) {
                        continue;
                    }
                }
                //SE IL PREZZO SUPERA IL PUNTO C ED IL PATTENR NON E' STATO COMPLETATO ALLORA DEVO ELIMINARE QUESTO PATTERN
                if ((!initialize ? price : bar.High) > pattern.BC.ToPrice && pattern.Mode == PatternMode.Bullish || (!initialize ? price : bar.Low) < pattern.BC.ToPrice && pattern.Mode == PatternMode.Bearish) {
                    if (pattern.Compleated == false) {
                        if (_signalPatternEvent)
                            onPatternStateChanged.Invoke(pattern, PatternEvent.Remove);
                        DeletePatternInList(pattern);
                        continue;
                    }
                }

                if (pattern.Compleated && !pattern.Closed) {
                    // controllo se il punto D è arrivato oltre lo StopLoss
                    if ((!initialize ? price : bar.Low) < pattern.StopLoss && pattern.Mode == PatternMode.Bullish || (!initialize ? price : bar.High) > pattern.StopLoss && pattern.Mode == PatternMode.Bearish) {
                        if (!pattern.Target1Compleated) {
                            pattern.Failed = true;
                        }
                        pattern.Closed = true;
                        if (_signalPatternEvent)
                            onPatternStateChanged.Invoke(pattern, PatternEvent.Closed);
                        continue;
                    }

                    //SE IL PREZZO E' ARRIVATO AL PRIMO TARGET ALLORA SPOSTO LO STOPLOSS IN PAREGGIO
                    if ((!initialize ? price : bar.High) >= pattern.Target1 && pattern.Mode == PatternMode.Bullish || (!initialize ? price : bar.Low) <= pattern.Target1 && pattern.Mode == PatternMode.Bearish) {
                        //se ho la posizione aperta devo proteggerla mettendo a pareggio lo stop
                        if (pattern.Enter) {
                            pattern.StopLoss = pattern.EnterPrice;
                            pattern.StopLossPips = 0;
                        }

                        if (pattern.Enter && !pattern.Target1Compleated) {
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
                        if (_signalPatternEvent)
                            onPatternStateChanged.Invoke(pattern, PatternEvent.Target1);
                    }

                    //SE IL PREZZO E' ARRIVATO AL SECONDO TARGET ALLORA CHIUDO LA POSIZIONE
                    if ((!initialize ? price : bar.High) >= pattern.Target2 && pattern.Mode == PatternMode.Bullish || (!initialize ? price : bar.Low) <= pattern.Target2 && pattern.Mode == PatternMode.Bearish) {
                        //if (pattern.Enter && !pattern.Target2Compleated)
                        //{
                        //    if (Bot.Positions.Find(pattern.GetKey()) != null)
                        //    {
                        //        Bot.Positions.Find(pattern.GetKey()).Close();
                        //    }
                        //}
                        pattern.Target2Compleated = true;
                        pattern.Succeed = true;
                        pattern.Closed = true;
                        if (_signalPatternEvent)
                            onPatternStateChanged.Invoke(pattern, PatternEvent.Target2);
                        continue;
                    }
                }

                //stabilisco quale prezzo utlizzare
                if (initialize) {
                    TradePrice = pattern.Mode == PatternMode.Bullish ? bar.Low : bar.High;
                }
                else {
                    TradePrice = price;
                }

                //SE SONO IN PRZ E IL PATTERN E' ANCORA DA TRADARE
                if (pattern.PRZ.InArea(TradePrice) && !pattern.Succeed && !pattern.Failed && !pattern.Closed) {
                    if (pattern.CD == null) {
                        pattern.CD = new Segment(pattern.BC.ToPrice, TradePrice, pattern.BC.ToOpenTime, openTime);
                        pattern.Compleated = true;
                        if (_signalPatternEvent)
                            onPatternStateChanged.Invoke(pattern, PatternEvent.Compleated);
                    }
                    else {
                        if (TradePrice < pattern.CD.ToPrice && pattern.Mode == PatternMode.Bullish || TradePrice > pattern.CD.ToPrice && pattern.Mode == PatternMode.Bearish) {
                            pattern.CD.Extend(TradePrice, openTime);
                        }
                    }
                    pattern.CD_Percent = MeasueSwing(pattern.BC, pattern.CD);
                    pattern.CD_Period = SegmentPeriod(pattern.CD);
                    Segment _AD = new Segment(pattern.XA.ToPrice, pattern.CD.ToPrice, pattern.XA.ToOpenTime, pattern.CD.ToOpenTime);

                    //calcola target dinamici
                    switch (pattern.Type) {
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
                    pattern.Target1Pips = (pattern.Mode == PatternMode.Bullish ? pattern.Target1 - TradePrice : TradePrice - pattern.Target1) / Symbol.PipSize;
                    pattern.Target2Pips = (pattern.Mode == PatternMode.Bullish ? pattern.Target2 - TradePrice : TradePrice - pattern.Target2) / Symbol.PipSize;
                    pattern.StopLossPips = (pattern.Mode == PatternMode.Bullish ? TradePrice - pattern.StopLoss : pattern.StopLoss - TradePrice) / Symbol.PipSize;
                    pattern.RiskReward = (100 * pattern.Target1Pips) / (pattern.StopLossPips + pattern.Target1Pips);


                    //gestiso l'entrata automatica
                    //if (!pattern.Enter && trigCompleated && Bot.enterMode == EnterMode.Automatic)
                    //{
                    //    //double euroPerUnita = pattern.StopLossPips * Bot.Symbol.PipValue;
                    //    //double rischioInEuro = 20;
                    //    //double unita = (rischioInEuro / euroPerUnita) / Bot.Symbol.LotSize;
                    //    //double volume = Bot.Symbol.QuantityToVolumeInUnits(unita);
                    //    //double volume_norm = Bot.Symbol.NormalizeVolumeInUnits(volume);

                    //    //TradeResult TR = Bot.ExecuteMarketOrder(pattern.Mode == PatternMode.Bullish ? TradeType.Buy : TradeType.Sell, Bot.Symbol.Name, volume_norm, pattern.GetKey());

                    //    //if (TR.IsSuccessful)
                    //    //{
                    //    //    TR.Position.ModifyStopLossPrice(pattern.StopLoss);
                    //    //    TR.Position.ModifyTakeProfitPrice(pattern.Target2);
                    //    //    pattern.PositionID = TR.Position.Id;
                    //    //    pattern.Enter = true;
                    //    //    pattern.EnterPrice = TradePrice;
                    //    //    pattern.Volume = volume_norm;
                    //    //}
                    //}
                }



                if (pattern.DrawableArea.InArea(TradePrice) || pattern.Compleated) {
                    if (pattern.Drawable == false && _signalPatternEvent) {
                        onPatternStateChanged.Invoke(pattern, PatternEvent.Add);
                    } 
                    pattern.Drawable = true;
                }
                else {
                    if (pattern.Drawable == true && _signalPatternEvent) {
                        onPatternStateChanged.Invoke(pattern, PatternEvent.Remove);
                    }
                    pattern.Drawable = false;
                }

                DrawPattern(pattern, Chart, Symbol.Bid);
            }
        }
        private void DrawPattern(ArmonicPattern pattern, Chart chart, double price) {
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
            switch (pattern.Type) {
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
            DateTime _RArea = MainData.BarsData.Last(0).OpenTime;


            //BONE
            if (pattern.Drawable) {
                chart.DrawTrendLine(string.Concat(pattern.GetKey(), "XA"), pattern.XA.FromOpenTime, pattern.XA.FromPrice, pattern.XA.ToOpenTime, pattern.XA.ToPrice, _borderColor, _thickness, LineStyle.Solid);
                chart.DrawTrendLine(string.Concat(pattern.GetKey(), "AB"), pattern.AB.FromOpenTime, pattern.AB.FromPrice, pattern.AB.ToOpenTime, pattern.AB.ToPrice, _borderColor, _thickness, LineStyle.Solid);
                chart.DrawTrendLine(string.Concat(pattern.GetKey(), "BC"), pattern.BC.FromOpenTime, pattern.BC.FromPrice, pattern.BC.ToOpenTime, pattern.BC.ToPrice, _borderColor, _thickness, LineStyle.Solid);

                if (pattern.Compleated) {
                    chart.DrawTrendLine(string.Concat(pattern.GetKey(), "CD"), pattern.CD.FromOpenTime, pattern.CD.FromPrice, pattern.CD.ToOpenTime, pattern.CD.ToPrice, _borderColor, _thickness, LineStyle.Solid);

                    chart.DrawTrendLine(string.Concat(pattern.GetKey(), "XB"), pattern.XA.FromOpenTime, pattern.XA.FromPrice, pattern.AB.ToOpenTime, pattern.AB.ToPrice, _borderColor, _thickness, LineStyle.DotsRare);
                    chart.DrawTrendLine(string.Concat(pattern.GetKey(), "AC"), pattern.AB.FromOpenTime, pattern.AB.FromPrice, pattern.BC.ToOpenTime, pattern.BC.ToPrice, _borderColor, _thickness, LineStyle.DotsRare);
                    chart.DrawTrendLine(string.Concat(pattern.GetKey(), "BD"), pattern.BC.FromOpenTime, pattern.BC.FromPrice, pattern.CD.ToOpenTime, pattern.CD.ToPrice, _borderColor, _thickness, LineStyle.DotsRare);
                    chart.DrawTrendLine(string.Concat(pattern.GetKey(), "XD"), pattern.XA.FromOpenTime, pattern.XA.FromPrice, pattern.CD.ToOpenTime, pattern.CD.ToPrice, _borderColor, _thickness, LineStyle.DotsRare);
                }
                else {
                    chart.DrawTrendLine(string.Concat(pattern.GetKey(), "CD"), pattern.IdealCD.FromOpenTime, pattern.IdealCD.FromPrice, _RArea, pattern.IdealCD.ToPrice, _borderColor, _thickness, LineStyle.Solid);
                }

            }
            else {
                chart.RemoveObject(string.Concat(pattern.GetKey(), "XA"));
                chart.RemoveObject(string.Concat(pattern.GetKey(), "AB"));
                chart.RemoveObject(string.Concat(pattern.GetKey(), "BC"));
                chart.RemoveObject(string.Concat(pattern.GetKey(), "CD"));
                chart.RemoveObject(string.Concat(pattern.GetKey(), "XB"));
                chart.RemoveObject(string.Concat(pattern.GetKey(), "AC"));
                chart.RemoveObject(string.Concat(pattern.GetKey(), "BD"));
                chart.RemoveObject(string.Concat(pattern.GetKey(), "XD"));
            }

            //TRIANGLE
            if (pattern.Drawable) {
                XAB = chart.DrawTriangle((string.Concat(pattern.GetKey(), "XAB")), pattern.XA.FromOpenTime, pattern.XA.FromPrice, pattern.XA.ToOpenTime, pattern.XA.ToPrice, pattern.AB.ToOpenTime, pattern.AB.ToPrice, _patternColor, 1, LineStyle.Lines);
                if (pattern.Compleated) {
                    BCD = chart.DrawTriangle((string.Concat(pattern.GetKey(), "BCD")), pattern.BC.FromOpenTime, pattern.BC.FromPrice, pattern.BC.ToOpenTime, pattern.BC.ToPrice, pattern.CD.ToOpenTime, pattern.CD.ToPrice, _patternColor, 1, LineStyle.Lines);
                }
                else {
                    BCD = chart.DrawTriangle((string.Concat(pattern.GetKey(), "BCD")), pattern.BC.FromOpenTime, pattern.BC.FromPrice, pattern.BC.ToOpenTime, pattern.BC.ToPrice, _RArea, pattern.IdealCD.ToPrice, _patternColor, 1, LineStyle.Lines);
                }

                XAB.IsFilled = true;
                BCD.IsFilled = true;
                //BCD.IsInteractive = true;
                //BCD.IsLocked = true;


            }
            else {
                chart.RemoveObject(string.Concat(pattern.GetKey(), "XAB"));
                chart.RemoveObject(string.Concat(pattern.GetKey(), "BCD"));
            }

            //LEVEL
            if (!pattern.Closed && pattern.Drawable) {
                if (!pattern.Target1Compleated) {
                    ChartRectangle PRZ = chart.DrawRectangle(string.Concat(pattern.GetKey(), "PRZ"), pattern.XA.FromOpenTime, pattern.PRZ.FromPrice, pattern.PRZ.InArea(price) ? _RArea : pattern.XA.ToOpenTime, pattern.PRZ.ToPrice, _PRZColor);
                    chart.DrawRectangle(string.Concat(pattern.GetKey(), "PRZ_BORDER"), pattern.XA.FromOpenTime, pattern.PRZ.FromPrice, pattern.PRZ.InArea(price) ? _RArea : pattern.XA.ToOpenTime, pattern.PRZ.ToPrice, _PRZBorderColor);
                    PRZ.IsFilled = true;
                    ChartRectangle SLZ = chart.DrawRectangle(string.Concat(pattern.GetKey(), "SLZ"), pattern.XA.FromOpenTime, pattern.SLZ.FromPrice, pattern.SLZ.InArea(price) ? _RArea : pattern.XA.ToOpenTime, pattern.SLZ.ToPrice, _SLZColor);
                    chart.DrawRectangle(string.Concat(pattern.GetKey(), "SLZ_BORDER"), pattern.XA.FromOpenTime, pattern.SLZ.FromPrice, pattern.SLZ.InArea(price) ? _RArea : pattern.XA.ToOpenTime, pattern.SLZ.ToPrice, _SLZBorderColor);
                    SLZ.IsFilled = true;
                }
                else {
                    chart.RemoveObject(string.Concat(pattern.GetKey(), "PRZ"));
                    chart.RemoveObject(string.Concat(pattern.GetKey(), "PRZ_BORDER"));
                    chart.RemoveObject(string.Concat(pattern.GetKey(), "SLZ"));
                    chart.RemoveObject(string.Concat(pattern.GetKey(), "SLZ_BORDER"));
                }

                if (pattern.Compleated) {
                    ChartRectangle T1Z = chart.DrawRectangle(string.Concat(pattern.GetKey(), "T1Z"), pattern.XA.FromOpenTime, pattern.T1Z.FromPrice, pattern.T1Z.InArea(price) ? _RArea : pattern.XA.ToOpenTime, pattern.T1Z.ToPrice, _T1ZColor);
                    chart.DrawRectangle(string.Concat(pattern.GetKey(), "T1Z_BORDER"), pattern.XA.FromOpenTime, pattern.T1Z.FromPrice, pattern.T1Z.InArea(price) ? _RArea : pattern.XA.ToOpenTime, pattern.T1Z.ToPrice, _T1ZBorderColor);
                    T1Z.IsFilled = true;
                    ChartRectangle T2Z = chart.DrawRectangle(string.Concat(pattern.GetKey(), "T2Z"), pattern.XA.FromOpenTime, pattern.T2Z.FromPrice, pattern.T2Z.InArea(price) ? _RArea : pattern.XA.ToOpenTime, pattern.T2Z.ToPrice, _T2ZColor);
                    chart.DrawRectangle(string.Concat(pattern.GetKey(), "T2Z_BORDER"), pattern.XA.FromOpenTime, pattern.T2Z.FromPrice, pattern.T2Z.InArea(price) ? _RArea : pattern.XA.ToOpenTime, pattern.T2Z.ToPrice, _T2ZBorderColor);
                    T2Z.IsFilled = true;


                }

            }
            else {

                chart.RemoveObject(string.Concat(pattern.GetKey(), "PRZ"));
                chart.RemoveObject(string.Concat(pattern.GetKey(), "PRZ_BORDER"));
                chart.RemoveObject(string.Concat(pattern.GetKey(), "SLZ"));
                chart.RemoveObject(string.Concat(pattern.GetKey(), "SLZ_BORDER"));
                chart.RemoveObject(string.Concat(pattern.GetKey(), "T1Z"));
                chart.RemoveObject(string.Concat(pattern.GetKey(), "T1Z_BORDER"));
                chart.RemoveObject(string.Concat(pattern.GetKey(), "T2Z"));
                chart.RemoveObject(string.Concat(pattern.GetKey(), "T2Z_BORDER"));
            }

            //INFO
            string _text = "";
            if (pattern.Drawable) {
                _text = string.Concat(pattern.Type.ToString(), " ");
            }

            if (pattern.Compleated) {
                _text = string.Concat(_text, "(", pattern.XA_Period.ToString(), ", ");
                _text = string.Concat(_text, pattern.AB_Period.ToString(), ", ");
                _text = string.Concat(_text, pattern.BC_Period.ToString(), ", ");
                _text = string.Concat(_text, pattern.CD_Period.ToString(), ")");
                //_text = string.Concat(_text, "R/Rw : ", Math.Round(pattern.RiskReward, 2).ToString(), "%  ");
            }
            _text = _text.ToUpper();
            if (pattern.Target1Compleated == true) {
                _text = string.Concat(_text, "\nTarget #1 Reached");
            }
            if (pattern.Target2Compleated == true) {
                _text = string.Concat(_text, "\nTarget #2 Reached");
            }
            if (pattern.Closed) {
                _text = string.Concat(_text, "\nClosed (", (pattern.Succeed == true ? "Succeed" : "Failed"), ")");
            }

            //string.Concat(_text.ToUpper(), "\n", pattern.Compleated.ToString());

            ChartText _message = chart.DrawText(string.Concat(pattern.GetKey(), "msg"), _text, pattern.XA.FromOpenTime, pattern.XA.FromPrice, _borderColor);
            if (pattern.Mode == PatternMode.Bearish) {
                _message.VerticalAlignment = VerticalAlignment.Top;
            }
            else {
                _message.VerticalAlignment = VerticalAlignment.Bottom;

            }
            _message.FontSize = 8;


        }
        private void DeleteDrawPattern(ArmonicPattern pattern, Chart chart) {


            //BONE
            chart.RemoveObject(string.Concat(pattern.GetKey(), "XA"));
            chart.RemoveObject(string.Concat(pattern.GetKey(), "AB"));
            chart.RemoveObject(string.Concat(pattern.GetKey(), "BC"));
            chart.RemoveObject(string.Concat(pattern.GetKey(), "CD"));
            chart.RemoveObject(string.Concat(pattern.GetKey(), "XB"));
            chart.RemoveObject(string.Concat(pattern.GetKey(), "AC"));
            chart.RemoveObject(string.Concat(pattern.GetKey(), "BD"));
            chart.RemoveObject(string.Concat(pattern.GetKey(), "XD"));
            //INFO
            chart.RemoveObject(string.Concat(pattern.GetKey(), "msg"));

            //TRIANGLE
            chart.RemoveObject(string.Concat(pattern.GetKey(), "XAB"));
            chart.RemoveObject(string.Concat(pattern.GetKey(), "BCD"));

            //LEVEL
            chart.RemoveObject(string.Concat(pattern.GetKey(), "PRZ"));
            chart.RemoveObject(string.Concat(pattern.GetKey(), "PRZ_BORDER"));
            chart.RemoveObject(string.Concat(pattern.GetKey(), "SLZ"));
            chart.RemoveObject(string.Concat(pattern.GetKey(), "SLZ_BORDER"));
            chart.RemoveObject(string.Concat(pattern.GetKey(), "T1Z"));
            chart.RemoveObject(string.Concat(pattern.GetKey(), "T1Z_BORDER"));
            chart.RemoveObject(string.Concat(pattern.GetKey(), "T2Z"));
            chart.RemoveObject(string.Concat(pattern.GetKey(), "T2Z_BORDER"));
        }
    }
    public class SegmentTracerEngine {
        //private readonly ArmonicBot Bot;
        private readonly ArmonicFinderEngine Engine;
        private Direction Direction = Direction.NoDirection;
        private Segment Segment;
        public List<Segment> SegmentList;
        private bool _firstBarEvent = true;
        private bool _drawSwing = false;
        public SegmentTracerEngine(ArmonicFinderEngine engine, bool drawswing = false) {
            Engine = engine;
            _drawSwing = drawswing;
            SegmentList = new List<Segment>();
        }

        public void Calculate(Bar _curr, Bar _prev, bool clearBack) {

            //Bar _limitPeriod = Bot.Bars.Last(2000);
            bool _swing = false;
            bool _nested = false;

            //salto il primo evento perchè riporta una barra iniziale non corretta
            if (_firstBarEvent) {
                _firstBarEvent = false;
                return;
            }

            // leggo la direzione del prezzo
            //[      ] BUG ==> GBPUSD H1 : 31/07/2020 doppio innesto
            switch (Direction) {
                case Direction.Up:
                    // se le barre sono innestate ed in espansione
                    if (_curr.High > _prev.High && _curr.Low < _prev.Low) {
                        _nested = true;
                        //se l'innesto in espansione è eccedente per altezza allora mantengo la direzione up
                        if (_curr.High - _prev.High <= _prev.Low - _curr.Low) {
                            //l'innesto in espansione accentua in basso quindi lo considero uno swing
                            Direction = Direction.Down;
                            _swing = true;
                        }
                    }
                    // se le barre non sono innestate
                    else {
                        //controllo se c'è stato uno swing
                        if (_curr.High <= _prev.High) {
                            // il prezzo ha invertito
                            Direction = Direction.Down;
                            _swing = true;
                        }
                    }
                    break;
                case Direction.Down:
                    // se le barre sono innestate ed in espansione
                    if (_curr.High > _prev.High && _curr.Low < _prev.Low) {
                        _nested = true;
                        //se l'innesto in espansione è eccedente per altezza allora lo considero uno swing
                        if (_curr.High - _prev.High > _prev.Low - _curr.Low) {
                            Direction = Direction.Up;
                            _swing = true;
                        }
                    }
                    else {
                        // se i prezzi scendono allora è una continuazione
                        if (_curr.Low >= _prev.Low) {
                            // se il prezzo non registra un nuovo minimo allora è uno swing
                            Direction = Direction.Up;
                            _swing = true;
                        }
                    }
                    break;
                case Direction.NoDirection:
                    if (_curr.High >= _prev.High && _curr.Low >= _prev.Low) {
                        Direction = Direction.Up;
                    }
                    else if (_curr.Low < _prev.Low && _curr.High < _prev.High) {
                        Direction = Direction.Down;
                    }
                    else {
                        // condizione di innesto delle barre, esco perchè non c'è ancora una direzione
                        Direction = Direction.NoDirection;
                        return;
                    }
                    _swing = true;
                    break;
            }

            if (!_swing) {
                // le barre seguono il loro corso quindi aggiorno l'ultimo segmento
                Segment.Update(_curr);
                SegmentList[SegmentList.Count - 1] = Segment;
                //DrawSegment();
            }
            else {
                // le barre invertono, è uno swing quindi creo un nuovo segmento nella nuova direzione (opposta)
                // MA PRIMA SE C'E' UN INNESTO DEVO ANCHE SPOSTARE IL SEGMENTO PRECEDENTE SUL PREZZO CORRETTO PRIMA DI REGISTRARE UN NUOVO SEGMENTO
                if (_nested && _swing) {
                    Segment.UpdatePrice(_curr);
                    //DrawSegment();
                }

                Segment = new Segment(Direction, _prev, _curr);

                if (_nested && _swing) {
                    Segment.UpdateFromPrice(_curr);
                }
                //DrawSegment();
                SegmentList.Add(Segment);
            }

            //elimino i segmenti dallo storico secondo il numero di periodi impostato
            if (Engine.MainData.BarsData.Count() > Engine.Periods && clearBack) {
                foreach (Segment _app in SegmentList) {
                    if (_app.ToOpenTime < Engine.MainData.BarsData.Last(Engine.Periods).OpenTime) {
                        //DeleteDrawSegment(SegmentList[0]);
                        SegmentList.RemoveAt(0);
                        break;
                    }
                }
            }
        }
        //public void DrawSegment()
        //{
        //    if (_drawSwing)
        //    {
        //        string _name = String.Format("Segment_{0} ", Segment.FromOpenTime.ToString("dd/MM/yyyy:HHmmss"));
        //        Engine.Chart.DrawTrendLine(_name, Segment.FromOpenTime, Segment.FromPrice, Segment.ToOpenTime, Segment.ToPrice, Color.Yellow, 1, LineStyle.Dots);



        //    }
        //}
        //public void DeleteDrawSegment(Segment SegmentToDelete)
        //{
        //    if (_drawSwing)
        //    {
        //        string _name = String.Format("Segment_{0} ", SegmentToDelete.FromOpenTime.ToString("dd/MM/yyyy:HHmmss"));
        //        Engine.Chart.RemoveObject(_name);
        //    }
        //}
    }
    public class Segment {
        public double FromPrice;
        public double ToPrice;
        public DateTime FromOpenTime;
        public DateTime ToOpenTime;
        public readonly Direction Direction;

        public Segment(Direction direction, Bar startBar, Bar endBar) {
            Direction = direction;
            FromOpenTime = startBar.OpenTime;
            ToOpenTime = endBar.OpenTime;

            if (Direction == Direction.Up) {
                FromPrice = startBar.Low;
                ToPrice = endBar.High;
            }
            if (Direction == Direction.Down) {
                FromPrice = startBar.High;
                ToPrice = endBar.Low;
            }

        }
        public Segment(double fromPrice, double toPrice, DateTime fromOpenTime, DateTime toOpenTime) {
            FromOpenTime = fromOpenTime;
            ToOpenTime = toOpenTime;
            FromPrice = fromPrice;
            ToPrice = toPrice;
            Direction = toPrice > fromPrice ? Direction.Up : Direction.Down;
        }

        public double Measure() {
            return Direction == Direction.Up ? ToPrice - FromPrice : FromPrice - ToPrice;
        }
        public void Update(Bar endBar) {
            UpdatePrice(endBar);
            ToOpenTime = endBar.OpenTime;
        }
        public void UpdatePrice(Bar endBar) {
            ToPrice = Direction == Direction.Down ? endBar.Low : endBar.High;
        }
        public void UpdateFromPrice(Bar endBar) {
            FromPrice = Direction == Direction.Down ? endBar.High : endBar.Low;
        }
        public void Extend(double toPrice, DateTime toOpenTime) {
            ToPrice = toPrice;
            ToOpenTime = toOpenTime;
        }
    }
    public class Area {
        public double FromPrice;
        public double ToPrice;

        public Area(double fromPrice, double toPrice) {
            FromPrice = fromPrice;
            ToPrice = toPrice;
        }

        public bool InArea(double price) {
            if (FromPrice > ToPrice) {
                if (price <= FromPrice && price >= ToPrice) {
                    return true;
                }
                else {
                    return false;
                }
            }
            else {
                if (price >= FromPrice && price <= ToPrice) {
                    return true;
                }
                else {
                    return false;
                }
            }

        }
        public bool AboveArea(double price) {
            if (FromPrice < ToPrice && price > ToPrice) {
                return true;
            }
            else if (FromPrice > ToPrice && price < ToPrice) {
                return true;
            }
            else {
                return false;
            }
        }
        public bool BelowArea(double price) {
            if (FromPrice < ToPrice && price < FromPrice) {
                return true;
            }
            else if (FromPrice > ToPrice && price > FromPrice) {
                return true;
            }
            else {
                return false;
            }
        }
    }

    public enum Direction {
        Up,
        Down,
        NoDirection
    }
    public enum PatternType {
        Undefined,
        Gartley,
        Bat,
        Cypher,
        Butterfly
    }
    public enum PatternMode {
        Bullish,
        Bearish
    }
    //public enum SegmentMoveType
    //{
    //    Impulsive,
    //    corrective
    //}
    public enum PatternStep {
        BFinding,
        CFinding,
        DFinding,
        Completed
    }
    public enum PatternEvent {
        Target1,
        Target2,
        Closed,
        Compleated,
        Add,
        Remove,
        NoEvent
    }

    //public enum EnterMode
    //{
    //    Automatic,
    //    Manual,
    //    Signal,
    //    None
    //}

}
