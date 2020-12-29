using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo.API;
using cAlgo.API.Internals;

namespace Armonic {
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
    public enum Direction {
        Up,
        Down,
        NoDirection
    }
}
