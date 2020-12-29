using cAlgo.API;
using cAlgo.API.Internals;


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
}
