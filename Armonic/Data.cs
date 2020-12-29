using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo.API;
using cAlgo.API.Internals;

namespace Data {
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

}
