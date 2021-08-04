// #############################################################
// #														   #
// #                PriceActionSwingOscillator                 #
// #														   #
// #     19.12.2016 by dorschden, die.unendlichkeit@gmx.de     #
// #														   #
// #        Thanks and comments are highly appreciated         #
// #        Paypal thanks to "die.unendlichkeit@gmx.de"        #
// #														   #
// #############################################################


#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using PriceActionSwing.Base;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.PriceActionSwing
{
    /// <summary>
    /// PriceActionSwingOscillator shows the trend direction, swing relation or developing 
    /// swing volume. !!! The volume is repainting, in case the swing direction changes. !!!
    /// </summary>
	public class PriceActionSwingOscillator : Indicator
	{
        #region Variables
        //#########################################################################################
        #region Parameters
        //=========================================================================================
        /// <summary>
        /// Represents the double top/-bottom strength for the swings.
        /// </summary>
        private int dtbStrength = 20;
        /// <summary>
        /// Represents the swing size for the swings. E.g. 1 = small and 5 = bigger swings.
        /// </summary>
        private double swingSize = 7;
        /// <summary>
        /// Represents the swing type for the swings.
        /// </summary>
        private SwingStyle swingType = SwingStyle.Standard;
        /// <summary>
        /// Indicates if high and low prices are used for the swing calculations or close values.
        /// </summary>
        private bool useCloseValues = false;
        //=========================================================================================
        #endregion

        #region DataSeries
        //=========================================================================================
        private Series<int> swingTrend;
        private Series<int> swingRelation;
        private Series<double> volumeHighSeries;
        private Series<double> volumeLowSeries;
        //=========================================================================================
        #endregion

        #region Misc
        //=====================================================================
        /// <summary>
        /// Indicates if the Gann swings are updated if the last swing high/low is broken. 
        /// </summary>
        private bool useBreakouts = true;
        /// <summary>
        /// Indicates if inside bars are ignored for the Gann swing calculation. If set to 
        /// true it is possible that between consecutive up/down bars are inside bars.
        /// </summary>
        private bool ignoreInsideBars = true;
        /// <summary>
        /// Represents the number of decimal places for the instrument
        /// </summary>
        private int decimalPlaces;
        //=====================================================================
        #endregion

        #region Settings
        //=========================================================================================
        private Show showOscillator = Show.Volume;
        private bool useOldTrend = true;
        private int oldTrend = 0;
        private bool ignoreSwings = true;
		/// <summary>
		/// Represents the name of the indicator.
		/// </summary>
		private string displayName = null;
        //=========================================================================================
        #endregion

        #region Class objects
        //=========================================================================================
        /// <summary>
        /// Represents the properties for the swing.
        /// </summary>
        private SwingProperties swingProperties;
        /// <summary>
        /// Represents the values for the current swing.
        /// </summary>
        private SwingCurrent swingCurrent = new SwingCurrent();
        /// <summary>
        /// Represents the swing high values.
        /// </summary>
        private Swings swingHigh = new Swings();
        /// <summary>
        /// Represents the swing low values.
        /// </summary>
        private Swings swingLow = new Swings();
        //=========================================================================================
        #endregion
        //#########################################################################################
        #endregion
		
        #region OnStateChange()
        //=========================================================================================
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"PriceActionSwingOscillator shows the trend direction, swing relation or developing swing volume. !!! The volume is repainting. !!!";
				Name										= "PriceActionSwingOscillator";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				
		        #region Parameters
		        //=========================================================================================
				DtbStrength = 20;
				SwingSize = 7;
				SwingType = SwingStyle.Standard;
				UseCloseValues = false;
		        //=========================================================================================
		        #endregion	
				
				AddPlot(new Stroke(Brushes.Firebrick, DashStyleHelper.Solid, 20), PlotStyle.Square, "RTDoubleTop");
				AddPlot(new Stroke(Brushes.Red, DashStyleHelper.Solid, 20), PlotStyle.Square, "RTDownTrend");
				AddPlot(new Stroke(Brushes.Gold, DashStyleHelper.Solid, 20), PlotStyle.Square, "RTNoWhere");
				AddPlot(new Stroke(Brushes.Green, DashStyleHelper.Solid, 20), PlotStyle.Square, "RTUpTrend");
				AddPlot(new Stroke(Brushes.Lime, DashStyleHelper.Solid, 20), PlotStyle.Square, "RTDoubleBottom");
				
				AddPlot(new Stroke(Brushes.Lime, DashStyleHelper.Solid, 2), PlotStyle.Bar, "VHigh");
				AddPlot(new Stroke(Brushes.Red, DashStyleHelper.Solid, 2), PlotStyle.Bar, "VLow");
				AddPlot(new Stroke(Brushes.Lime, DashStyleHelper.Solid, 2), PlotStyle.Square, "VHighCurrent");
				AddPlot(new Stroke(Brushes.Red, DashStyleHelper.Solid, 2), PlotStyle.Square, "VLowCurrent");
			}
			else if (State == State.DataLoaded)
			{
	            // Calculate decimal places
	            decimal increment = Convert.ToDecimal(Instrument.MasterInstrument.TickSize);
	            int incrementLength = increment.ToString().Length;
	            decimalPlaces = 0;
	            if (incrementLength == 1)
	                decimalPlaces = 0;
	            else if (incrementLength > 2)
	                decimalPlaces = incrementLength - 2;
				
				if (SwingType != SwingStyle.Percent)
				{
					SwingSize = SwingSize < 1 ? 1 : Math.Round(SwingSize, MidpointRounding.AwayFromZero);
				}
				
				displayName = Name +  "(" + Instrument.FullName + " (" + BarsPeriod.Value + " " + BarsPeriod.BarsPeriodType + "), " + SwingType + ", " + SwingSize + ", " + DtbStrength + ")";


	            swingProperties = new SwingProperties( SwingSize, DtbStrength);
			}
			else if (State == State.Configure)
			{
				swingTrend = new Series<int>(this);
				swingRelation = new Series<int>(this);
				volumeHighSeries = new Series<double>(this);
				volumeLowSeries = new Series<double>(this);
			}
		}
        //=========================================================================================
		#endregion

		protected override void OnBarUpdate()
		{
            #region Initialize varibles
            //=====================================================================================
            if (IsFirstTickOfBar)
            {
                swingCurrent.StopOutsideBarCalc = false;

                if (CurrentBar == 1)
                {
                    swingHigh.CurBar = swingLow.CurBar = CurrentBar;
                    swingHigh.CurPrice = High[1];
                    swingLow.CurPrice = Low[1];
                }
                volumeHighSeries[0] = 0;
                volumeLowSeries[0] = 0;
            }
            // Set new/update high/low back to false, to avoid function 
            // calls which depends on them
            swingHigh.New = swingLow.New = swingHigh.Update = swingLow.Update = false;

            // Checks to ensure there are enough bars before beginning
            if (CurrentBars[BarsInProgress] < swingSize)
                return;
            //=====================================================================================
            #endregion

            #region Swing calculation
            //=====================================================================================
            switch (swingType)
	        {
                case SwingStyle.Standard:
                    CalculateSwingStandard(swingHigh, swingLow, swingCurrent, swingProperties,
                        decimalPlaces, useCloseValues);
                    break;
                case SwingStyle.Gann:
                    CalculateSwingGann(swingHigh, swingLow, swingCurrent, swingProperties, 
                        decimalPlaces);
                    break;
                case SwingStyle.Ticks:
                    CalculateSwingTicks(swingHigh, swingLow, swingCurrent, swingProperties,
                        decimalPlaces, useCloseValues);
                    break;
                case SwingStyle.Percent:
                    CalculateSwingPercent(swingHigh, swingLow, swingCurrent, swingProperties,
                        decimalPlaces, useCloseValues);
                    break;
            }
            //=====================================================================================
            #endregion

            #region Swing trend
            //=====================================================================================
            if ((swingHigh.CurRelation == 1 && swingLow.CurRelation == 1)
                    || (swingTrend[1] == 1 && swingCurrent.SwingSlope == 1)
                    || (ignoreSwings && swingTrend[1] == 1 && swingLow.CurRelation == 1)
                    || (((swingTrend[1] == 1) || (swingCurrent.SwingSlope == 1 
                        && swingHigh.CurRelation == 1))
                        && swingLow.CurRelation == 0))
                swingTrend[0] = 1;
            else if ((swingHigh.CurRelation == -1 && swingLow.CurRelation == -1)
                    || (swingTrend[1] == -1 && swingCurrent.SwingSlope == -1)
                    || (ignoreSwings && swingTrend[1] == -1 && swingHigh.CurRelation == -1)
                    || (((swingTrend[1] == -1) || (swingCurrent.SwingSlope == -1 
                        && swingLow.CurRelation == -1))
                        && swingHigh.CurRelation == 0))
                swingTrend[0] = -1;
            else
                swingTrend[0] = 0;
            //=====================================================================================
            #endregion

            #region Swing relation
            //=====================================================================================
            if (swingLow.CurRelation == 0)
                swingRelation[0] = 2;
            else if (swingHigh.CurRelation == 0)
                swingRelation[0] = -2;
            else if (swingHigh.CurRelation == 1 && swingLow.CurRelation == 1)
                swingRelation[0] = 1;
            else if (swingHigh.CurRelation == -1 && swingLow.CurRelation == -1)
                swingRelation[0] = -1;
            else
                swingRelation[0] = 0;
            //=====================================================================================
            #endregion

            #region Draw
            //=====================================================================================
            switch (showOscillator)
            {
                case Show.Trend:
                    #region Trend
                    //-----------------------------------------------------------------------------
                    int trend = Convert.ToInt32(swingTrend[0]);
                    switch (trend)
                    {
                        case -1:
                            RTDownTrend[0] = 1;
                            oldTrend = -1;
                            break;
                        case 1:
                            oldTrend = 1;
                            RTUpTrend[0] = 1;
                            break;
                        default:
                            if (useOldTrend)
                            {
                                if (oldTrend == 1)
                                    RTDownTrend[0] = 1;
                                else if (oldTrend == -1)
                                    RTUpTrend[0] = 1;
                            }
                            else
                                RTNoWhere[0] = 1;
                            break;
                    }
                    //-----------------------------------------------------------------------------
                    #endregion
                    break;
                case Show.Relation:
                    #region Relation
                    //-----------------------------------------------------------------------------
                    int relation = Convert.ToInt32(swingRelation[0]);
                    switch (relation)
                    {
                        case -2:
                            RTDoubleTop[0] = relation;
                            break;
                        case -1:
                            RTDownTrend[0] = relation;
                            break;
                        case 0:
                            RTNoWhere[0] = relation;
                            break;
                        case 1:
                            RTUpTrend[0] = relation;
                            break;
                        case 2:
                            RTDoubleBottom[0] = relation;
                            break;
                        default:
                            RTNoWhere[0] = relation;
                            break;
                    }
                    //-----------------------------------------------------------------------------
                    #endregion
                    break;
                case Show.Volume:
                    #region Volume
                    //-----------------------------------------------------------------------------
                    if (swingHigh.New == true)
                    {
                        if (swingHigh.Update == false)
                        {
                            double swingVolume = 0.0;
                            for (int i = CurrentBar - swingLow.CurBar - 1; i > -1; i--)
                            {
                                swingVolume = swingVolume + Volume[i];
                                volumeHighSeries[i] = swingVolume;
                                volumeLowSeries[i] = 0;

                                VLow.Reset(i);
                                VHighCurrent.Reset(i);
                                VHigh[i] = swingVolume;
                            }
                            swingVolume = 0.0;
                            for (int i = CurrentBar - swingHigh.CurBar - 1; i > -1; i--)
                            {
                                swingVolume = swingVolume + Volume[i];
                                volumeLowSeries[i] = swingVolume;
                                VLowCurrent[i] = swingVolume;
                            }
                        }
                        else
                        {
                            double tmp = volumeHighSeries[1] + Volume[0];
                            volumeHighSeries[0] = tmp;
                            for (int i = CurrentBar - swingLow.CurBar - 1; i > -1; i--)
                            {
                                VLowCurrent.Reset(i);
                                volumeLowSeries[i] = 0;
                            }
                            VHigh[0] = tmp;
                        }
                    }
                    else if (swingLow.New == true)
                    {
                        if (swingLow.Update == false)
                        {
                            double swingVolume = 0.0;
                            for (int i = CurrentBar - swingHigh.CurBar - 1; i > -1; i--)
                            {
                                swingVolume = swingVolume + Volume[i];
                                volumeLowSeries[i] = swingVolume;
                                volumeHighSeries[i] = 0;

                                VHigh.Reset(i);
                                VLowCurrent.Reset(i);
                                VLow[i] = swingVolume;
                            }
                            swingVolume = 0.0;
                            for (int i = CurrentBar - swingLow.CurBar - 1; i > -1; i--)
                            {
                                swingVolume = swingVolume + Volume[i];
                                volumeHighSeries[i] = swingVolume;
                                VHighCurrent[i] = swingVolume;
                            }
                        }
                        else
                        {
                            double tmp = volumeLowSeries[1] + Volume[0];
                            volumeLowSeries[0] = tmp;
                            for (int i = CurrentBar - swingHigh.CurBar - 1; i > -1; i--)
                            {
                                VHighCurrent.Reset(i);
                                volumeHighSeries[i] = 0;
                            }
                            VLow[0] = tmp;
                        }
                    }
                    else
                    {
                        double tmpH = volumeHighSeries[1] + Volume[0];
                        double tmpL = volumeLowSeries[1] + Volume[0];
                        volumeHighSeries[0] = tmpH;
                        volumeLowSeries[0] = tmpL;

                        if (swingCurrent.SwingSlope == -1)
                        {
                            VLow[0] = tmpL;
                            VHighCurrent[0] = tmpH;
                        }
                        else
                        {
                            VHigh[0] = tmpH;
                            VLowCurrent[0] = tmpL;
                        }

                    }
                    //-----------------------------------------------------------------------------
                    #endregion
                    break;
            }
            //=====================================================================================
            #endregion
		}
		
        #region Calculate Swing Standard
        //#########################################################################################
        protected void CalculateSwingStandard(Swings swingHigh, Swings swingLow,
            SwingCurrent swingCur, SwingProperties swingProp, int decimalPlaces, 
            bool useCloseValues)
        {
            // Check if high and low values are used or only close values
            PriceSeries[] highs;
            PriceSeries[] lows;
            if (useCloseValues == true)
            {
                lows = Closes;
                highs = Closes;
            }
            else
            {
                lows = Lows;
                highs = Highs;
            }

            // For a new swing high in an uptrend, Highs[BarsInProgress][0] must be 
            // greater than the current swing high
            if (swingCur.SwingSlope == 1 && highs[BarsInProgress][0] <= swingHigh.CurPrice)
                swingHigh.New = false;
            else
                swingHigh.New = true;

            // For a new swing low in a downtrend, Lows[BarsInProgress][0] must be 
            // smaller than the current swing low
            if (swingCur.SwingSlope == -1 && lows[BarsInProgress][0] >= swingLow.CurPrice)
                swingLow.New = false;
            else
                swingLow.New = true;

            // CalculatOnBarClose == true
            if (Calculate == Calculate.OnBarClose)
            {
                // test if Highs[BarsInProgress][0] is higher than the last 
                // calculationSize highs = new swing high
                if (swingHigh.New)
                {
                    for (int i = 1; i < swingProp.SwingSize + 1; i++)
                    {
                        if (highs[BarsInProgress][0] <= highs[BarsInProgress][i])
                        {
                            swingHigh.New = false;
                            break;
                        }
                    }
                }
                // test if Lows[BarsInProgress][0] is lower than the last 
                // calculationSize lows = new swing low
                if (swingLow.New)
                {
                    for (int i = 1; i < swingProp.SwingSize + 1; i++)
                    {
                        if (lows[BarsInProgress][0] >= lows[BarsInProgress][i])
                        {
                            swingLow.New = false;
                            break;
                        }
                    }
                }

                // New swing high and new swing low
                if (swingHigh.New && swingLow.New)
                {
                    // Downtrend - ignore the swing high
                    if (swingCur.SwingSlope == -1)
                        swingHigh.New = false;
                    // Uptrend   - ignore the swing low
                    else
                        swingLow.New = false;
                }
            }
            // CalculatOnBarClose == false
            else
            {
                // Used to control, that only one swing is set for 
                // each bar
                if (IsFirstTickOfBar)
                    swingCur.NewSwing = 0;

                // No swing or an up swing is found
                if (swingCur.NewSwing != -1)
                {
                    // test if Highs[BarsInProgress][0] is higher than the last 
                    // calculationSize highs = new swing high
                    if (swingHigh.New)
                    {
                        for (int i = 1; i < swingProp.SwingSize + 1; i++)
                        {
                            if (highs[BarsInProgress][0] <= highs[BarsInProgress][i])
                            {
                                swingHigh.New = false;
                                break;
                            }
                        }
                        // Found a swing high
                        if (swingHigh.New)
                            swingCur.NewSwing = 1;
                    }
                }

                // No swing or an down swing is found
                if (swingCur.NewSwing != 1)
                {
                    // test if Lows[BarsInProgress][0] is lower than the last 
                    // calculationSize lows = new swing low
                    if (swingLow.New)
                    {
                        for (int i = 1; i < swingProp.SwingSize + 1; i++)
                        {
                            if (lows[BarsInProgress][0] >= lows[BarsInProgress][i])
                            {
                                swingLow.New = false;
                                break;
                            }
                        }
                        // Found a swing low
                        if (swingLow.New)
                            swingCur.NewSwing = -1;
                    }
                }

                // Set newLow back to false
                if (swingCur.NewSwing == 1)
                    swingLow.New = false;
                // Set newHigh back to false
                if (swingCur.NewSwing == -1)
                    swingHigh.New = false;
            }

            // Swing high
            if (swingHigh.New)
            {
                int bar;
                double price;
                // New swing high
                if (swingCur.SwingSlope != 1)
                {
                    bar = CurrentBars[BarsInProgress] -
                        HighestBar(highs[BarsInProgress], CurrentBars[BarsInProgress] -
                        swingLow.CurBar);
                    price = highs[BarsInProgress][HighestBar(highs[BarsInProgress],
                        CurrentBars[BarsInProgress] - swingLow.CurBar)];
                    swingHigh.Update = false;
                }
                // Update swing high
                else
                {
                    bar = CurrentBars[BarsInProgress];
                    price = highs[BarsInProgress][0];
                    swingHigh.Update = true;
                }
                CalcUpSwing(bar, price, swingHigh.Update, swingHigh, swingCur, swingProp, 
                    decimalPlaces);
            }
            // Swing low
            else if (swingLow.New)
            {
                int bar;
                double price;
                // New swing low
                if (swingCur.SwingSlope != -1)
                {
                    bar = CurrentBars[BarsInProgress] - LowestBar(lows[BarsInProgress],
                        CurrentBars[BarsInProgress] - swingHigh.CurBar);
                    price = lows[BarsInProgress][LowestBar(lows[BarsInProgress],
                        CurrentBars[BarsInProgress] - swingHigh.CurBar)];
                    swingLow.Update = false;
                }
                // Update swing low
                else
                {
                    bar = CurrentBars[BarsInProgress];
                    price = lows[BarsInProgress][0];
                    swingLow.Update = true;
                }
                CalcDnSwing(bar, price, swingLow.Update, swingLow, swingCur, swingProp, 
                    decimalPlaces);
            }
        }
        //#########################################################################################
        #endregion

        #region Calculate Swing Gann
        //#########################################################################################
        protected void CalculateSwingGann(Swings swingHigh, Swings swingLow, SwingCurrent swingCur,
            SwingProperties swingProp, int decimalPlaces)
        {
            #region Set bar property
            //=================================================================================
            // Represents the bar type. -1 = Down | 0 = Inside | 1 = Up | 2 = Outside
            int barType = 0;
            if (Highs[BarsInProgress][0] > Highs[BarsInProgress][1])
            {
                if (Lows[BarsInProgress][0] < Lows[BarsInProgress][1])
                    barType = 2;
                else
                    barType = 1;
            }
            else
            {
                if (Lows[BarsInProgress][0] < Lows[BarsInProgress][1])
                    barType = -1;
                else
                    barType = 0;
            }
            //=================================================================================
            #endregion

            #region Up swing
            //=================================================================================
            if (swingCur.SwingSlope == 1)
            {
                switch (barType)
                {
                    // Up bar
                    case 1:
                        swingCur.ConsecutiveBars = 0;
                        swingCur.ConsecutiveBarValue = 0.0;
                        if (Highs[BarsInProgress][0] > swingHigh.CurPrice)
                        {
                            swingHigh.New = true;
                            swingHigh.Update = true;
                            CalcUpSwing(CurrentBars[BarsInProgress],
                                Highs[BarsInProgress][0], swingHigh.Update, swingHigh,
                                swingCur, swingProp, decimalPlaces);
                            if ((swingCur.ConsecutiveBars + 1) == swingProp.SwingSize)
                                swingCur.StopOutsideBarCalc = true;
                        }
                        break;
                    // Down bar
                    case -1:
                        if (swingCur.ConsecutiveBarNumber != CurrentBars[BarsInProgress])
                        {
                            if (swingCur.ConsecutiveBarValue == 0.0)
                            {
                                swingCur.ConsecutiveBars++;
                                swingCur.ConsecutiveBarNumber = CurrentBars[BarsInProgress];
                                swingCur.ConsecutiveBarValue = Lows[BarsInProgress][0];
                            }
                            else if (Lows[BarsInProgress][0] < swingCur.ConsecutiveBarValue)
                            {
                                swingCur.ConsecutiveBars++;
                                swingCur.ConsecutiveBarNumber = CurrentBars[BarsInProgress];
                                swingCur.ConsecutiveBarValue = Lows[BarsInProgress][0];
                            }
                        }
                        else if (Lows[BarsInProgress][0] < swingCur.ConsecutiveBarValue)
                            swingCur.ConsecutiveBarValue = Lows[BarsInProgress][0];
                        if (swingCur.ConsecutiveBars == swingProp.SwingSize ||
                            (swingProp.UseBreakouts && Lows[BarsInProgress][0] <
                            swingLow.CurPrice))
                        {
                            swingCur.ConsecutiveBars = 0;
                            swingCur.ConsecutiveBarValue = 0.0;
                            swingLow.New = true;
                            swingLow.Update = false;
                            int bar = CurrentBars[BarsInProgress] -
                                LowestBar(Lows[BarsInProgress],
                                CurrentBars[BarsInProgress] - swingHigh.CurBar);
                            double price =
                                Lows[BarsInProgress][LowestBar(Lows[BarsInProgress],
                                CurrentBars[BarsInProgress] - swingHigh.CurBar)];
                            CalcDnSwing(bar, price, swingLow.Update, swingLow, swingCur, 
                                swingProp, decimalPlaces);
                        }
                        break;
                    // Inside bar
                    case 0:
                        if (!swingProp.IgnoreInsideBars)
                        {
                            swingCur.ConsecutiveBars = 0;
                            swingCur.ConsecutiveBarValue = 0.0;
                        }
                        break;
                    // Outside bar
                    case 2:
                        if (Highs[BarsInProgress][0] > swingHigh.CurPrice)
                        {
                            swingHigh.New = true;
                            swingHigh.Update = true;
                            CalcUpSwing(CurrentBars[BarsInProgress],
                                Highs[BarsInProgress][0], swingHigh.Update, swingHigh,
                                swingCur, swingProp, decimalPlaces);
                        }
                        else if (!swingCur.StopOutsideBarCalc)
                        {
                            if (swingCur.ConsecutiveBarNumber != CurrentBars[BarsInProgress])
                            {
                                if (swingCur.ConsecutiveBarValue == 0.0)
                                {
                                    swingCur.ConsecutiveBars++;
                                    swingCur.ConsecutiveBarNumber =
                                        CurrentBars[BarsInProgress];
                                    swingCur.ConsecutiveBarValue = Lows[BarsInProgress][0];
                                }
                                else if (Lows[BarsInProgress][0] <
                                    swingCur.ConsecutiveBarValue)
                                {
                                    swingCur.ConsecutiveBars++;
                                    swingCur.ConsecutiveBarNumber =
                                        CurrentBars[BarsInProgress];
                                    swingCur.ConsecutiveBarValue = Lows[BarsInProgress][0];
                                }
                            }
                            else if (Lows[BarsInProgress][0] < swingCur.ConsecutiveBarValue)
                                swingCur.ConsecutiveBarValue = Lows[BarsInProgress][0];
                            if (swingCur.ConsecutiveBars == swingProp.SwingSize ||
                                (swingProp.UseBreakouts && Lows[BarsInProgress][0] <
                                swingLow.CurPrice))
                            {
                                swingCur.ConsecutiveBars = 0;
                                swingCur.ConsecutiveBarValue = 0.0;
                                swingLow.New = true;
                                swingLow.Update = false;
                                int bar = CurrentBars[BarsInProgress] -
                                    LowestBar(Lows[BarsInProgress],
                                    CurrentBars[BarsInProgress] - swingHigh.CurBar);
                                double price =
                                    Lows[BarsInProgress][LowestBar(Lows[BarsInProgress],
                                    CurrentBars[BarsInProgress] - swingHigh.CurBar)];
                                CalcDnSwing(bar, price, swingLow.Update, swingLow, swingCur, 
                                    swingProp, decimalPlaces);
                            }
                        }
                        break;
                }
            }
            //=================================================================================
            #endregion

            #region Down swing
            //=================================================================================
            else
            {
                switch (barType)
                {
                    // Dwon bar
                    case -1:
                        swingCur.ConsecutiveBars = 0;
                        swingCur.ConsecutiveBarValue = 0.0;
                        if (Lows[BarsInProgress][0] < swingLow.CurPrice)
                        {
                            swingLow.New = true;
                            swingLow.Update = true;
                            CalcDnSwing(CurrentBars[BarsInProgress],
                                Lows[BarsInProgress][0], swingLow.Update, swingLow, swingCur, 
                                swingProp, decimalPlaces);
                            if ((swingCur.ConsecutiveBars + 1) == swingProp.SwingSize)
                                swingCur.StopOutsideBarCalc = true;
                        }
                        break;
                    // Up bar
                    case 1:
                        if (swingCur.ConsecutiveBarNumber != CurrentBars[BarsInProgress])
                        {
                            if (swingCur.ConsecutiveBarValue == 0.0)
                            {
                                swingCur.ConsecutiveBars++;
                                swingCur.ConsecutiveBarNumber = CurrentBars[BarsInProgress];
                                swingCur.ConsecutiveBarValue = Highs[BarsInProgress][0];
                            }
                            else if (Highs[BarsInProgress][0] > swingCur.ConsecutiveBarValue)
                            {
                                swingCur.ConsecutiveBars++;
                                swingCur.ConsecutiveBarNumber = CurrentBars[BarsInProgress];
                                swingCur.ConsecutiveBarValue = Highs[BarsInProgress][0];
                            }
                        }
                        else if (Highs[BarsInProgress][0] > swingCur.ConsecutiveBarValue)
                            swingCur.ConsecutiveBarValue = Highs[BarsInProgress][0];
                        if (swingCur.ConsecutiveBars == swingProp.SwingSize ||
                            (swingProp.UseBreakouts && Highs[BarsInProgress][0] >
                            swingHigh.CurPrice))
                        {
                            swingCur.ConsecutiveBars = 0;
                            swingCur.ConsecutiveBarValue = 0.0;
                            swingHigh.New = true;
                            swingHigh.Update = false;
                            int bar = CurrentBars[BarsInProgress] -
                                HighestBar(Highs[BarsInProgress],
                                CurrentBars[BarsInProgress] - swingLow.CurBar);
                            double price =
                                Highs[BarsInProgress][HighestBar(Highs[BarsInProgress],
                                CurrentBars[BarsInProgress] - swingLow.CurBar)];
                            CalcUpSwing(bar, price, swingHigh.Update, swingHigh, swingCur, 
                                swingProp, decimalPlaces);
                        }
                        break;
                    // Inside bar
                    case 0:
                        if (!swingProp.IgnoreInsideBars)
                        {
                            swingCur.ConsecutiveBars = 0;
                            swingCur.ConsecutiveBarValue = 0.0;
                        }
                        break;
                    // Outside bar
                    case 2:
                        if (Lows[BarsInProgress][0] < swingLow.CurPrice)
                        {
                            swingLow.New = true;
                            swingLow.Update = true;
                            CalcDnSwing(CurrentBars[BarsInProgress],
                                Lows[BarsInProgress][0], swingLow.Update, swingLow, swingCur, 
                                swingProp, decimalPlaces);
                        }
                        else if (!swingCur.StopOutsideBarCalc)
                        {
                            if (swingCur.ConsecutiveBarNumber != CurrentBars[BarsInProgress])
                            {
                                if (swingCur.ConsecutiveBarValue == 0.0)
                                {
                                    swingCur.ConsecutiveBars++;
                                    swingCur.ConsecutiveBarNumber =
                                        CurrentBars[BarsInProgress];
                                    swingCur.ConsecutiveBarValue = Highs[BarsInProgress][0];
                                }
                                else if (Highs[BarsInProgress][0] >
                                    swingCur.ConsecutiveBarValue)
                                {
                                    swingCur.ConsecutiveBars++;
                                    swingCur.ConsecutiveBarNumber =
                                        CurrentBars[BarsInProgress];
                                    swingCur.ConsecutiveBarValue = Highs[BarsInProgress][0];
                                }
                            }
                            else if (Highs[BarsInProgress][0] > swingCur.ConsecutiveBarValue)
                                swingCur.ConsecutiveBarValue = Highs[BarsInProgress][0];
                            if (swingCur.ConsecutiveBars == swingProp.SwingSize ||
                                (swingProp.UseBreakouts && Highs[BarsInProgress][0] >
                                swingHigh.CurPrice))
                            {
                                swingCur.ConsecutiveBars = 0;
                                swingCur.ConsecutiveBarValue = 0.0;
                                swingHigh.New = true;
                                swingHigh.Update = false;
                                int bar = CurrentBars[BarsInProgress] -
                                    HighestBar(Highs[BarsInProgress],
                                    CurrentBars[BarsInProgress] - swingLow.CurBar);
                                double price =
                                    Highs[BarsInProgress][HighestBar(Highs[BarsInProgress],
                                    CurrentBars[BarsInProgress] - swingLow.CurBar)];
                                CalcUpSwing(bar, price, swingHigh.Update, swingHigh,
                                    swingCur, swingProp, decimalPlaces);
                            }
                        }
                        break;
                }
            }
            //=================================================================================
            #endregion
        }
        //#########################################################################################
        #endregion

        #region Calculate Swing Ticks
        //#########################################################################################
        protected void CalculateSwingTicks(Swings swingHigh, Swings swingLow,
            SwingCurrent swingCur, SwingProperties swingProp, int decimalPlaces, 
            bool useCloseValues)
        {
            // Check if high and low values are used or only close values
            PriceSeries[] highs;
            PriceSeries[] lows;
            if (useCloseValues == true)
            {
                lows = Closes;
                highs = Closes;
            }
            else
            {
                lows = Lows;
                highs = Highs;
            }

            // For a new swing high in an uptrend, Highs[BarsInProgress][0] must be 
            // greater than the current swing high
            if (swingCur.SwingSlope == 1 && highs[BarsInProgress][0] <= swingHigh.CurPrice)
                swingHigh.New = false;
            else
                swingHigh.New = true;

            // For a new swing low in a downtrend, Lows[BarsInProgress][0] must be 
            // smaller than the current swing low
            if (swingCur.SwingSlope == -1 && lows[BarsInProgress][0] >= swingLow.CurPrice)
                swingLow.New = false;
            else
                swingLow.New = true;

            // CalculatOnBarClose == true
            if (Calculate == Calculate.OnBarClose)
            {
                if (swingHigh.New
                    && highs[BarsInProgress][0] <
                    (swingLow.CurPrice + swingProp.SwingSize * TickSize))
                    swingHigh.New = false;

                if (swingLow.New
                    && lows[BarsInProgress][0] >
                    (swingHigh.CurPrice - swingProp.SwingSize * TickSize))
                    swingLow.New = false;

                // New swing high and new swing low
                if (swingHigh.New && swingLow.New)
                {
                    // Downtrend - ignore the swing high
                    if (swingCur.SwingSlope == -1)
                        swingHigh.New = false;
                    // Uptrend   - ignore the swing low
                    else
                        swingLow.New = false;
                }
            }
            // CalculatOnBarClose == false
            else
            {
                // Used to control, that only one swing is set for 
                // each bar
                if (IsFirstTickOfBar)
                    swingCur.NewSwing = 0;

                // No swing or an up swing is found
                if (swingCur.NewSwing != -1)
                {
                    // test if Highs[BarsInProgress][0] is higher than the last 
                    // calculationSize highs = new swing high
                    if (swingHigh.New)
                    {
                        if (highs[BarsInProgress][0] <
                            (swingLow.CurPrice + swingProp.SwingSize * TickSize))
                            swingHigh.New = false;
                        // Found a swing high
                        if (swingHigh.New)
                            swingCur.NewSwing = 1;
                    }
                }

                // No swing or an down swing is found
                if (swingCur.NewSwing != 1)
                {
                    // test if Lows[BarsInProgress][0] is lower than the last 
                    // calculationSize lows = new swing low
                    if (swingLow.New)
                    {
                        if (lows[BarsInProgress][0] >
                            (swingHigh.CurPrice - swingProp.SwingSize * TickSize))
                            swingLow.New = false;
                        // Found a swing low
                        if (swingLow.New)
                            swingCur.NewSwing = -1;
                    }
                }

                // Set newLow back to false
                if (swingCur.NewSwing == 1)
                    swingLow.New = false;
                // Set newHigh back to false
                if (swingCur.NewSwing == -1)
                    swingHigh.New = false;
            }

            // Swing high
            if (swingHigh.New)
            {
                int bar;
                double price;
                // New swing high
                if (swingCur.SwingSlope != 1)
                {
                    bar = CurrentBars[BarsInProgress] -
                        HighestBar(highs[BarsInProgress], CurrentBars[BarsInProgress] -
                        swingLow.CurBar);
                    price = highs[BarsInProgress][HighestBar(highs[BarsInProgress],
                        CurrentBars[BarsInProgress] - swingLow.CurBar)];
                    swingHigh.Update = false;
                }
                // Update swing high
                else
                {
                    bar = CurrentBars[BarsInProgress];
                    price = highs[BarsInProgress][0];
                    swingHigh.Update = true;
                }
                CalcUpSwing(bar, price, swingHigh.Update, swingHigh, swingCur, swingProp,
                    decimalPlaces);
            }
            // Swing low
            else if (swingLow.New)
            {
                int bar;
                double price;
                // New swing low
                if (swingCur.SwingSlope != -1)
                {
                    bar = CurrentBars[BarsInProgress] - LowestBar(lows[BarsInProgress],
                        CurrentBars[BarsInProgress] - swingHigh.CurBar);
                    price = lows[BarsInProgress][LowestBar(lows[BarsInProgress],
                        CurrentBars[BarsInProgress] - swingHigh.CurBar)];
                    swingLow.Update = false;
                }
                // Update swing low
                else
                {
                    bar = CurrentBars[BarsInProgress];
                    price = lows[BarsInProgress][0];
                    swingLow.Update = true;
                }
                CalcDnSwing(bar, price, swingLow.Update, swingLow, swingCur, swingProp,
                    decimalPlaces);
            }
        }
        //#########################################################################################
        #endregion

        #region Calculate Swing Percent
        //#########################################################################################
        protected void CalculateSwingPercent(Swings swingHigh, Swings swingLow,
            SwingCurrent swingCur, SwingProperties swingProp, int decimalPlaces, 
            bool useCloseValues)
        {
            // Check if high and low values are used or only close values
            PriceSeries[] highs;
            PriceSeries[] lows;
            if (useCloseValues == true)
            {
                lows = Closes;
                highs = Closes;
            }
            else
            {
                lows = Lows;
                highs = Highs;
            }

            // For a new swing high in an uptrend, Highs[BarsInProgress][0] must be 
            // greater than the current swing high
            if (swingCur.SwingSlope == 1 && highs[BarsInProgress][0] <= swingHigh.CurPrice)
                swingHigh.New = false;
            else
                swingHigh.New = true;

            // For a new swing low in a downtrend, Lows[BarsInProgress][0] must be 
            // smaller than the current swing low
            if (swingCur.SwingSlope == -1 && lows[BarsInProgress][0] >= swingLow.CurPrice)
                swingLow.New = false;
            else
                swingLow.New = true;

            // CalculatOnBarClose == true
            if (Calculate == Calculate.OnBarClose)
            {
                if (swingHigh.New
                    && highs[BarsInProgress][0] <
                    (swingLow.CurPrice + swingLow.CurPrice / 100 * swingProp.SwingSize))
                    swingHigh.New = false;

                if (swingLow.New
                    && lows[BarsInProgress][0] >
                    (swingHigh.CurPrice - swingLow.CurPrice / 100 * swingProp.SwingSize))
                    swingLow.New = false;

                // New swing high and new swing low
                if (swingHigh.New && swingLow.New)
                {
                    // Downtrend - ignore the swing high
                    if (swingCur.SwingSlope == -1)
                        swingHigh.New = false;
                    // Uptrend   - ignore the swing low
                    else
                        swingLow.New = false;
                }
            }
            // CalculatOnBarClose == false
            else
            {
                // Used to control, that only one swing is set for 
                // each bar
                if (IsFirstTickOfBar)
                    swingCur.NewSwing = 0;

                // No swing or an up swing is found
                if (swingCur.NewSwing != -1)
                {
                    // test if Highs[BarsInProgress][0] is higher than the last 
                    // calculationSize highs = new swing high
                    if (swingHigh.New)
                    {
                        if (highs[BarsInProgress][0] <
                            (swingLow.CurPrice + swingLow.CurPrice / 100 * swingProp.SwingSize))
                            swingHigh.New = false;
                        // Found a swing high
                        if (swingHigh.New)
                            swingCur.NewSwing = 1;
                    }
                }

                // No swing or an down swing is found
                if (swingCur.NewSwing != 1)
                {
                    // test if Lows[BarsInProgress][0] is lower than the last 
                    // calculationSize lows = new swing low
                    if (swingLow.New)
                    {
                        if (lows[BarsInProgress][0] >
                            (swingHigh.CurPrice - swingLow.CurPrice / 100 * swingProp.SwingSize))
                            swingLow.New = false;
                        // Found a swing low
                        if (swingLow.New)
                            swingCur.NewSwing = -1;
                    }
                }

                // Set newLow back to false
                if (swingCur.NewSwing == 1)
                    swingLow.New = false;
                // Set newHigh back to false
                if (swingCur.NewSwing == -1)
                    swingHigh.New = false;
            }

            // Swing high
            if (swingHigh.New)
            {
                int bar;
                double price;
                // New swing high
                if (swingCur.SwingSlope != 1)
                {
                    bar = CurrentBars[BarsInProgress] -
                        HighestBar(highs[BarsInProgress], CurrentBars[BarsInProgress] -
                        swingLow.CurBar);
                    price = highs[BarsInProgress][HighestBar(highs[BarsInProgress],
                        CurrentBars[BarsInProgress] - swingLow.CurBar)];
                    swingHigh.Update = false;
                }
                // Update swing high
                else
                {
                    bar = CurrentBars[BarsInProgress];
                    price = highs[BarsInProgress][0];
                    swingHigh.Update = true;
                }
                CalcUpSwing(bar, price, swingHigh.Update, swingHigh, swingCur, swingProp,
                    decimalPlaces);
            }
            // Swing low
            else if (swingLow.New)
            {
                int bar;
                double price;
                // New swing low
                if (swingCur.SwingSlope != -1)
                {
                    bar = CurrentBars[BarsInProgress] - LowestBar(lows[BarsInProgress],
                        CurrentBars[BarsInProgress] - swingHigh.CurBar);
                    price = lows[BarsInProgress][LowestBar(lows[BarsInProgress],
                        CurrentBars[BarsInProgress] - swingHigh.CurBar)];
                    swingLow.Update = false;
                }
                // Update swing low
                else
                {
                    bar = CurrentBars[BarsInProgress];
                    price = lows[BarsInProgress][0];
                    swingLow.Update = true;
                }
                CalcDnSwing(bar, price, swingLow.Update, swingLow, swingCur, swingProp,
                    decimalPlaces);
            }
        }
        //#########################################################################################
        #endregion

        #region Calculate down swing
        //#########################################################################################
        protected void CalcDnSwing(int bar, double low, bool updateLow, Swings swingLow,
            SwingCurrent swingCur, SwingProperties swingProp, int decimalPlaces)
        {
            if (!updateLow)
            {
                swingLow.LastPrice = swingLow.CurPrice;
                swingLow.LastBar = swingLow.CurBar;
                swingLow.LastRelation = swingLow.CurRelation;
                swingLow.Counter++;
                swingCur.SwingSlope = -1;
                swingCur.SwingSlopeChangeBar = bar;
            }
            swingLow.CurBar = bar;
            swingLow.CurPrice = Math.Round(low, decimalPlaces, MidpointRounding.AwayFromZero);
            double dtbOffset = ATR(BarsArray[BarsInProgress], 14)[CurrentBars[BarsInProgress] -
                swingLow.CurBar] * swingProp.DtbStrength / 100;
            if (swingLow.CurPrice > swingLow.LastPrice - dtbOffset && swingLow.CurPrice <
                swingLow.LastPrice + dtbOffset)
                swingLow.CurRelation = 0;
            else if (swingLow.CurPrice < swingLow.LastPrice)
                swingLow.CurRelation = -1;
            else
                swingLow.CurRelation = 1;
        }
        //#########################################################################################
        #endregion

        #region Calculate up swing
        //#########################################################################################
        private void CalcUpSwing(int bar, double high, bool updateHigh, Swings swingHigh,
            SwingCurrent swingCur, SwingProperties swingProp, int decimalPlaces)
        {
            if (!updateHigh)
            {
                swingHigh.LastPrice = swingHigh.CurPrice;
                swingHigh.LastBar = swingHigh.CurBar;
                swingHigh.LastRelation = swingHigh.CurRelation;
                swingHigh.Counter++;
                swingCur.SwingSlope = 1;
                swingCur.SwingSlopeChangeBar = bar;
            }
            swingHigh.CurBar = bar;
            swingHigh.CurPrice = Math.Round(high, decimalPlaces, MidpointRounding.AwayFromZero);

            double dtbOffset = ATR(BarsArray[BarsInProgress], 14)[CurrentBars[BarsInProgress] -
                swingHigh.CurBar] * swingProp.DtbStrength / 100;
            if (swingHigh.CurPrice > swingHigh.LastPrice - dtbOffset && swingHigh.CurPrice <
                swingHigh.LastPrice + dtbOffset)
                swingHigh.CurRelation = 0;
            else if (swingHigh.CurPrice < swingHigh.LastPrice)
                swingHigh.CurRelation = -1;
            else
                swingHigh.CurRelation = 1;
        }
        //#########################################################################################
        #endregion
		
		#region Properties
		
		#region Plots
        // Plots ==================================================================================
		[Browsable(false)]
        [XmlIgnore]
        public Series<double> RTDoubleTop
        {
            get { return Values[0]; }
        }
		[Browsable(false)]
        [XmlIgnore]
        public Series<double> RTDownTrend
        {
            get { return Values[1]; }
        }
		[Browsable(false)]
        [XmlIgnore]
        public Series<double> RTNoWhere
        {
            get { return Values[2]; }
        }
		[Browsable(false)]
        [XmlIgnore]
        public Series<double> RTUpTrend
        {
            get { return Values[3]; }
        }
		[Browsable(false)]
        [XmlIgnore]
        public Series<double> RTDoubleBottom
        {
            get { return Values[4]; }
        }
		[Browsable(false)]
        [XmlIgnore]
        public Series<double> VHigh
        {
            get { return Values[5]; }
        }
		[Browsable(false)]
        [XmlIgnore]
        public Series<double> VLow
        {
            get { return Values[6]; }
        }
		[Browsable(false)]
        [XmlIgnore]
        public Series<double> VHighCurrent
        {
            get { return Values[7]; }
        }
		[Browsable(false)]
        [XmlIgnore]
        public Series<double> VLowCurrent
        {
            get { return Values[8]; }
        }
		//=========================================================================================
        #endregion

        #region Parameters
        //=========================================================================================		
		[NinjaScriptProperty]
		[Display(Name="Swing type", Description="Represents the swing type for the swings.", Order=1, GroupName="Parameters")]
		public SwingStyle SwingType
		{ get; set; }
		
		[Range(0.00000001, double.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Swing size", Description="Represents the swing size. e.g. 1 = small swings and 5 = bigger swings.", Order=2, GroupName="Parameters")]
		public double SwingSize
		{ get; set; }
		
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Double top/-bottom strength", Description="Represents the double top/-bottom strength. Increase the value to get more DB/DT.", Order=3, GroupName="Parameters")]
		public int DtbStrength
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Use close values", Description="Indicates if high and low prices are used for the swing calculations or close values.", Order=4, GroupName="Parameters")]
		public bool UseCloseValues
		{ get; set; }
        //=========================================================================================
        #endregion

        #region Settings
        //=========================================================================================
		[NinjaScriptProperty]
        [Display(Name = "Choose indication", Description = "Represents which swing indication is shown. Trend | Relation | Volume (repainting).", Order = 1, GroupName = "1. Settings")]
        public Show ShowOscillator
        {
            get { return showOscillator; }
            set { showOscillator = value; }
        }
		[NinjaScriptProperty]
        [Display(Name = "Trend change", Description = "Indicates if the trend direction is changed when the old trend ends or whether a new trend must start first.", Order = 2, GroupName = "1. Settings")]
        public bool UseOldTrend
        {
            get { return useOldTrend; }
            set { useOldTrend = value; }
        }
        //=========================================================================================
        #endregion
		
		#region Gann Swings
		//=========================================================================================
		[NinjaScriptProperty]
		[Display(Name = "Ignore Inside Bars", Description = "Indicates if inside bars are ignored. If set to true it is possible that between consecutive up/down bars are inside bars. Only used if calculationSize > 1.", Order = 1, GroupName = "2. Gann Swings")]
		public bool IgnoreInsideBars
		{
			get { return ignoreInsideBars; }
			set { ignoreInsideBars = value; }
		}

		[NinjaScriptProperty]
		[Display(Name = "Use Breakouts", Description = "Indicates if the swings are updated if the last swing high/low is broken. Only used if calculationSize > 1.", Order = 2, GroupName = "2. Gann Swings")]
		public bool UseBreakouts
		{
			get { return useBreakouts; }
			set { useBreakouts = value; }
		}
		//=========================================================================================
		#endregion

		#endregion
		
        #region CalculateShortRisk
        //#########################################################################################
        public override string DisplayName
        {
            get { return (displayName != null ? displayName : Name); }
		}        
        //#########################################################################################
		#endregion
	}
}


#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private PriceActionSwing.PriceActionSwingOscillator[] cachePriceActionSwingOscillator;
		public PriceActionSwing.PriceActionSwingOscillator PriceActionSwingOscillator(SwingStyle swingType, double swingSize, int dtbStrength, bool useCloseValues, Show showOscillator, bool useOldTrend, bool ignoreInsideBars, bool useBreakouts)
		{
			return PriceActionSwingOscillator(Input, swingType, swingSize, dtbStrength, useCloseValues, showOscillator, useOldTrend, ignoreInsideBars, useBreakouts);
		}

		public PriceActionSwing.PriceActionSwingOscillator PriceActionSwingOscillator(ISeries<double> input, SwingStyle swingType, double swingSize, int dtbStrength, bool useCloseValues, Show showOscillator, bool useOldTrend, bool ignoreInsideBars, bool useBreakouts)
		{
			if (cachePriceActionSwingOscillator != null)
				for (int idx = 0; idx < cachePriceActionSwingOscillator.Length; idx++)
					if (cachePriceActionSwingOscillator[idx] != null && cachePriceActionSwingOscillator[idx].SwingType == swingType && cachePriceActionSwingOscillator[idx].SwingSize == swingSize && cachePriceActionSwingOscillator[idx].DtbStrength == dtbStrength && cachePriceActionSwingOscillator[idx].UseCloseValues == useCloseValues && cachePriceActionSwingOscillator[idx].ShowOscillator == showOscillator && cachePriceActionSwingOscillator[idx].UseOldTrend == useOldTrend && cachePriceActionSwingOscillator[idx].IgnoreInsideBars == ignoreInsideBars && cachePriceActionSwingOscillator[idx].UseBreakouts == useBreakouts && cachePriceActionSwingOscillator[idx].EqualsInput(input))
						return cachePriceActionSwingOscillator[idx];
			return CacheIndicator<PriceActionSwing.PriceActionSwingOscillator>(new PriceActionSwing.PriceActionSwingOscillator(){ SwingType = swingType, SwingSize = swingSize, DtbStrength = dtbStrength, UseCloseValues = useCloseValues, ShowOscillator = showOscillator, UseOldTrend = useOldTrend, IgnoreInsideBars = ignoreInsideBars, UseBreakouts = useBreakouts }, input, ref cachePriceActionSwingOscillator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.PriceActionSwing.PriceActionSwingOscillator PriceActionSwingOscillator(SwingStyle swingType, double swingSize, int dtbStrength, bool useCloseValues, Show showOscillator, bool useOldTrend, bool ignoreInsideBars, bool useBreakouts)
		{
			return indicator.PriceActionSwingOscillator(Input, swingType, swingSize, dtbStrength, useCloseValues, showOscillator, useOldTrend, ignoreInsideBars, useBreakouts);
		}

		public Indicators.PriceActionSwing.PriceActionSwingOscillator PriceActionSwingOscillator(ISeries<double> input , SwingStyle swingType, double swingSize, int dtbStrength, bool useCloseValues, Show showOscillator, bool useOldTrend, bool ignoreInsideBars, bool useBreakouts)
		{
			return indicator.PriceActionSwingOscillator(input, swingType, swingSize, dtbStrength, useCloseValues, showOscillator, useOldTrend, ignoreInsideBars, useBreakouts);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.PriceActionSwing.PriceActionSwingOscillator PriceActionSwingOscillator(SwingStyle swingType, double swingSize, int dtbStrength, bool useCloseValues, Show showOscillator, bool useOldTrend, bool ignoreInsideBars, bool useBreakouts)
		{
			return indicator.PriceActionSwingOscillator(Input, swingType, swingSize, dtbStrength, useCloseValues, showOscillator, useOldTrend, ignoreInsideBars, useBreakouts);
		}

		public Indicators.PriceActionSwing.PriceActionSwingOscillator PriceActionSwingOscillator(ISeries<double> input , SwingStyle swingType, double swingSize, int dtbStrength, bool useCloseValues, Show showOscillator, bool useOldTrend, bool ignoreInsideBars, bool useBreakouts)
		{
			return indicator.PriceActionSwingOscillator(input, swingType, swingSize, dtbStrength, useCloseValues, showOscillator, useOldTrend, ignoreInsideBars, useBreakouts);
		}
	}
}

#endregion
