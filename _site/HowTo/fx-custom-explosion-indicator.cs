using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;

namespace MyCode.Indicators
{
    [Indicator(IsOverlay = false, AccessRights = AccessRights.None)]
    public class CustomExplosion : Indicator
    {
        //----------------------------------------------------------------
        // Input parameters.
        //----------------------------------------------------------------     
        [Parameter("Period", Group = "Bollinger Bands", DefaultValue = 10)]
        public int InpBBPeriod { get; set; }

        [Parameter("Standard Deviation", Group = "Bollinger Bands", DefaultValue = 2, MinValue = 0)]
        public double InpBBStDev { get; set; }

        [Parameter("Long Period", Group = "MACD", DefaultValue = 20)]
        public int InpMACDLongPeriod { get; set; }

        [Parameter("Short Period", Group = "MACD", DefaultValue = 10)]
        public int InpMACDShortPeriod { get; set; }

        //----------------------------------------------------------------
        // Output parameters.
        //----------------------------------------------------------------        
        [Output("Bullish Trend", LineColor = "DarkGreen", PlotType = PlotType.Histogram, Thickness = 5)]
        public IndicatorDataSeries BullishTrend { get; set; }

        [Output("Bearish Trend", LineColor = "Maroon", PlotType = PlotType.Histogram, Thickness = 5)]
        public IndicatorDataSeries BearishTrend { get; set; }

        [Output("Explosion", LineColor = "Red", PlotType = PlotType.Line,Thickness = 1)]
        public IndicatorDataSeries Explosion { get; set; }

        [Output("Threshold", LineColor = "Yellow", PlotType = PlotType.Line, Thickness = 1)]
        public IndicatorDataSeries Threshold { get; set; }
        
        [Output("Buy Signal", LineColor = "Lime", PlotType = PlotType.Points, Thickness = 5)]
        public IndicatorDataSeries BuySignal { get; set; }

        [Output("Sell Signal", LineColor = "Magenta", PlotType = PlotType.Points, Thickness = 5)]
        public IndicatorDataSeries SellSignal { get; set; }         

        //----------------------------------------------------------------
        // Private variables.
        //----------------------------------------------------------------
        private MacdCrossOver mMACD;
        private BollingerBands mBB;
        private AverageTrueRange mATR;

        private double TrendDir;

        //----------------------------------------------------------------
        // Initialize.
        //----------------------------------------------------------------        
        protected override void Initialize()
        {
            mMACD = Indicators.MacdCrossOver(Bars.ClosePrices, InpMACDLongPeriod, InpMACDShortPeriod, 9);
            mBB   = Indicators.BollingerBands(Bars.ClosePrices, InpBBPeriod, InpBBStDev, MovingAverageType.Simple);
            mATR  = Indicators.AverageTrueRange(14, MovingAverageType.Exponential);
        }

        //----------------------------------------------------------------
        // Calculate.
        //----------------------------------------------------------------        
        public override void Calculate(int i)
        {
            BuySignal[i] = double.NaN;
            SellSignal[i] = double.NaN;            
            
            TrendDir = (mMACD.MACD[i] - mMACD.MACD[i - 1]) * 100;

            if (TrendDir >= 0)
               BullishTrend[i] = TrendDir;
            else
               BearishTrend[i] = -1 * TrendDir;

            Explosion[i] = mBB.Top[i] - mBB.Bottom[i];
            
            Threshold[i] = mATR.Result[i] * 5.0;
            
            bool BullishTrendGreaterThanExplosion = BullishTrend[i] > Explosion[i];
            bool BearishTrendGreaterThanExplosion = BearishTrend[i] > Explosion[i];
            bool BullishTrendGreaterThanThreshold = BullishTrend[i] > Threshold[i];
            bool BearishTrendGreaterThanThreshold = BearishTrend[i] > Threshold[i];
            bool ExplosionGreaterThanThreshold = Explosion[i] > Threshold[i];
            //bool ExplosionSlopePositive = Explosion[i] > Explosion[i - 1];
            
            if (
               BullishTrendGreaterThanExplosion
               && BullishTrendGreaterThanThreshold
               && ExplosionGreaterThanThreshold
               )
               BuySignal[i] = BullishTrend[i];
            else if 
               (
               BearishTrendGreaterThanExplosion
               && BearishTrendGreaterThanThreshold
               && ExplosionGreaterThanThreshold
               )
               SellSignal[i] = BearishTrend[i];
        }

    } // End of class.
} // End of namespace.
