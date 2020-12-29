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
using Armonic;
using cAlgo;

namespace Armonic {
    public static class GlobalParameter {
        public static int Periods;
        public static int SubPeriods;
        public static bool DrawSwing;
    }
    public class GUI {

        public static class Styles {
            public static Style CreatePanelBackgroundStyle() {
                var style = new Style();
                style.Set(ControlProperty.CornerRadius, 3);
                style.Set(ControlProperty.BackgroundColor, GetColorWithOpacity(Color.FromHex("#303060"), 0.25m), ControlState.DarkTheme);
                style.Set(ControlProperty.BackgroundColor, GetColorWithOpacity(Color.FromHex("#000030"), 0.25m), ControlState.LightTheme);
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
            public ArmonicPattern Pattern;
            private Button PatternButton;
            public Grid Data;
            private const String _defaultMargin = "10 2 10 2";

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
            public ControlBase NewColumn(string value) {
                return new Column(value);
            }

            public ResultItem(ArmonicPattern pattern) {
                Pattern = pattern;
                Data = new Grid(1, 10) {
                };


                Data.AddChild(NewColumn(pattern.Symbol.Name), 0, 0);
                Data.AddChild(NewColumn(pattern.TimeFrame.ToString()), 0, 1);
                Data.AddChild(NewColumn(pattern.Type.ToString()), 0, 2);
                Data.AddChild(NewColumn(pattern.Mode.ToString()), 0, 3);
                Data.AddChild(NewColumn("<simmetry>"), 0, 4);
                Data.AddChild(NewColumn("<pattern width>"), 0, 5);
                Data.AddChild(NewColumn("<% compleated>"), 0, 6);
                Data.AddChild(NewColumn("<levels>"), 0, 7);
                Data.AddChild(NewColumn("<impulse>"), 0, 8);
                Data.AddChild(NewColumn("<state>"), 0, 9);

                Data.Columns[0].SetWidthInPixels(150);
                Data.Columns[1].SetWidthInPixels(100);
                Data.Columns[2].SetWidthInPixels(150);
                Data.Columns[3].SetWidthInPixels(150);
                Data.Columns[4].SetWidthInPixels(120);
                Data.Columns[5].SetWidthInPixels(120);
                Data.Columns[6].SetWidthInPixels(120);
                Data.Columns[7].SetWidthInPixels(120);
                Data.Columns[8].SetWidthInPixels(120);
                Data.Columns[9].SetWidthInPixels(120);

                PatternButton = new Button() {
                    BackgroundColor = Color.FromArgb(20, Color.Silver),
                    Content = Data,
                    HorizontalAlignment = HorizontalAlignment.Left
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
                    return CheckBox.IsChecked == false ? false : true;
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
                    IsThreeState = true
                };
                CheckBox.Click += onWatchlistCheckChange;
                content.AddChild(CheckBox);

                SymbolItems = new List<SymbolItem>();
                foreach (string symbolname in watchlist.SymbolNames.ToArray()) {
                    SymbolItems.Add(new SymbolItem(symbolname, onSymbolCheckChange));
                }
                foreach (SymbolItem symbolselector in SymbolItems) {
                    content.AddChild(symbolselector);
                }
                AddChild(content);
            }

            private void onWatchlistCheckChange(CheckBoxEventArgs e) {
                if (e.CheckBox.IsChecked == null) {
                    CheckBox.IsChecked = false;
                }
                foreach (SymbolItem selector in SymbolItems) {
                    selector.Selected = CheckBox.IsChecked == false ? false : true;
                }
            }
            private void onSymbolCheckChange(CheckBox sender, CheckBoxEventArgs e) {
                if (e.CheckBox.IsChecked == true) {
                    if (SymbolItems.Count(pred => pred.Selected == true) == SymbolItems.Count()) {
                        CheckBox.IsChecked = true;
                    }
                    else {
                        CheckBox.IsChecked = null;
                    }
                }
                else {
                    if (SymbolItems.Count(pred => pred.Selected == false) == SymbolItems.Count()) {
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

        private StackPanel ResultItemPanel;
        private ScrollViewer ResultScroll;
        private List<ResultItem> ResultItems;
        private DockPanel ResultPanel;
        private Border ResultBorder;
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
                Margin = "10 0 10 0",
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
                Margin = "10 0 10 0",
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
            ResultItemPanel = CreateResultItemPanel();
            ResultScroll = new ScrollViewer() {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = ResultItemPanel,
                BackgroundColor = Color.Transparent
            };
            TextBlock TitleResult = new TextBlock {
                Text = "Result",
                Margin = "10 3 10 3",
                FontSize = 20,
            };
            Border TitleResultBorder = new Border {
                BorderThickness = "0 0 0 1",
                Child = TitleResult,
                Style = Styles.CreateCommonBorderStyle(),
                Dock = Dock.Top,
            };
            ResultPanel = new DockPanel {
                BackgroundColor = Color.Transparent,
            };
            ResultPanel.AddChild(TitleResultBorder);
            ResultPanel.AddChild(ResultScroll);
            ResultBorder = new Border {
                Margin = "10 0 10 0",
                Child = ResultPanel,
                Style = Styles.CreatePanelBackgroundStyle(),
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
            Framework.AddChild(ResultBorder);

            //CHART
            Chart.AddControl(Framework);
        }
        private StackPanel CreateResultItemPanel() {
            return new StackPanel {
                Orientation = Orientation.Vertical
            };
        }
        public void AddResult(ArmonicPattern pattern) {
            ResultItem result = new ResultItem(pattern);
            //ResultItem result = new ResultItem(ResultItemPanel, pattern);
            ResultItems.Add(result);

        }
        public void DeleteResult(ArmonicPattern pattern) {
            ResultItem result;
            result = ResultItems.FirstOrDefault(fnd => fnd.Key == pattern.GetKey());
            if (result != null) {
                ResultItemPanel.RemoveChild(result);
                ResultItems.Remove(result);
            }
        }
        public void ClearResults() {
            ResultItems.Clear();
        }

        public void DrawResults(bool filter = false, bool order = false) {
            foreach (ResultItem item in ResultItems) {
                ResultItemPanel.RemoveChild(item);
            }
            //Chart.RemoveControl(ResultItemPanel);
            //ResultItemPanel = CreateResultItemPanel();
            //ResultScroll.Content = ResultItemPanel;
            //foreach (ResultItem item in ResultItems.Where(_filter => _filter.Pattern.Compleated==true).OrderBy(_order => _order.Pattern.XA_Period)) {
            foreach (ResultItem item in ResultItems.OrderBy(_order => _order.Pattern.Compleated)) {
                double _value = ResultItems.Max(_max => _max.Data.Columns[2].Width.Value);
                item.Data.Columns[2].SetWidthInPixels(_value);
                ResultItemPanel.AddChild(item);
            }

        }

        private void OnButtonClick(ButtonClickEventArgs obj) {
            switch (obj.Button.Text) {
                case ("Option"):
                    ConfigPanel.IsVisible = !ConfigPanel.IsVisible;
                    ResultBorder.IsVisible = false;

                    break;
                case ("Start"):
                    OnClickStart.Invoke();
                    break;
                case ("Result"):
                    ResultBorder.IsVisible = !ResultBorder.IsVisible;
                    ConfigPanel.IsVisible = false;
                    break;
            }
        }
    }


    public class ArmonicMultiFinder {

        private List<ArmonicFinderEngine> multipleFinder;
        public List<ArmonicPattern> Patterns;
        private ArmonicFinderEngine mainEngine;
        private GUI userInterface;

        private Chart Chart;
        private Watchlists Watchlists;
        private readonly Symbols Symbols;
        private readonly MarketData MarketData;
        public ArmonicMultiFinder(MarketData marketdata, Watchlists watchlists, Symbols symbols, Chart chart) {


            MarketData = marketdata;
            Watchlists = watchlists;
            Symbols = symbols;
            Chart = chart;

            userInterface = new GUI(Chart, Watchlists);
            userInterface.OnClickStart += OnFindStart;
            multipleFinder = new List<ArmonicFinderEngine>();
            Patterns = new List<ArmonicPattern>();
            //userInterface.LoadingBar.Value = 0;
            //userInterface.LoadingBar.MaxValue = 100;
            //userInterface.LoadingBar.IsVisible = true;
        }
        public void SetMainEngine(ArmonicFinderEngine engine) {
            mainEngine = engine;
        }
        private void AddEngine(ArmonicFinderEngine engine) {
            engine.Initialize(OnEngineLoaded, OnEngineLoading);
            engine.onPatternStateChanged += ManagePattern;
            multipleFinder.Add(engine);
        }
        public bool EngineExists(ArmonicFinderEngine engineToCheck) {
            return multipleFinder.Exists(engine => engine.Key == engineToCheck.Key);
        }
        private void ManagePattern(ArmonicPattern pattern, PatternEvent e) {
            switch (e) {
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

                    switch (e) {
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
            userInterface.DrawResults();
        }

        protected void OnOtherSymbolBar(BarOpenedEventArgs e) {
            Debug.Print("New Bar Opened At {0}  Incoming From Symbol {1} in TimeFrame {2}", e.Bars.LastBar.OpenTime.ToString("dd/MM/yyyy:HHmmss"), e.Bars.SymbolName, e.Bars.TimeFrame.ToString());
        }
        protected void OnEngineLoading(ArmonicFinderEngine sender, double percentage) {
            Debug.Print("Loading Data for Symbol {1} : {0}% ...", percentage, sender.MainData.BarsData.SymbolName);
            userInterface.LoadingBar.Value = Convert.ToInt32(percentage);
        }
        protected void OnEngineLoaded(ArmonicFinderEngine sender) {
            Debug.Print("Loading Finisced for Symbol {0}.", sender.MainData.BarsData.SymbolName);
            userInterface.LoadingBar.IsVisible = false;
        }
        protected void OnFindStart() {
            AddEngine(mainEngine);

            foreach (GUI.WatchListItem WLItem in userInterface.WatchlistItems.Where(obj => obj.Selected)) {
                foreach (GUI.SymbolItem SYItem in WLItem.SymbolItems.Where(obj => obj.Selected)) {
                    foreach (GUI.TimeFrameItem TFItem in userInterface.TimeFrameItems.Where(obj => obj.Selected)) {
                        ArmonicFinderEngine tmpEngine = new ArmonicFinderEngine(MarketData, Symbols.GetSymbol(SYItem.SymbolName), TFItem.TimeFrame, Chart);
                        if (!EngineExists(tmpEngine)) {
                            AddEngine(tmpEngine);
                        }
                    }
                }
            }
        }
    }


    
    


    //public enum EnterMode
    //{
    //    Automatic,
    //    Manual,
    //    Signal,
    //    None
    //}
}
