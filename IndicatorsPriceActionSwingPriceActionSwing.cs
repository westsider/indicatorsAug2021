// #############################################################
// #														   #
// #                     PriceActionSwing                      #
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
    /// PriceActionSwing calculates swings and visualizes them in different ways
    /// and displays several information about the swings.
    /// </summary>
	public class PriceActionSwing : Indicator
	{
        #region Variables
        //#########################################################################################
        #region Display
        //=========================================================================================
        /// <summary>
        /// Represents the swing length visualization type for the swings.
        /// </summary>
        private SwingLengthStyle swingLengthType = SwingLengthStyle.Ticks;
        /// <summary>
        /// Represents the swing duration visualization type for the swings.
        /// </summary>
        private SwingDurationStyle swingDurationType = SwingDurationStyle.Bars;
        /// <summary>
        /// Indicates if the swing price is shown for the swings.
        /// </summary>
        private bool showSwingPrice = true;
        /// <summary>
        /// Indicates if the swing label is shown for the swings.
        /// </summary>
        private bool showSwingLabel = false;
        /// <summary>
        /// Indicates if the swing percentage in relation to the last swing is shown for the 
        /// swings.
        /// </summary>
        private bool showSwingPercent = false;
        /// <summary>
        /// Represents the swing time visualization type for the swings. 
        /// </summary>
        private SwingTimeStyle swingTimeType = SwingTimeStyle.False;
        /// <summary>
        /// Indicates if the swing volume is shown for the swings.
        /// </summary>
        private SwingVolumeStyle swingVolumeType = SwingVolumeStyle.Absolute;
		/// <summary>
		/// Represents the name of the indicator.
		/// </summary>
		private string displayName = null;
        //=========================================================================================
        #endregion

        #region Visualize swings
        //=========================================================================================
        /// <summary>
        /// Represents the swing visualization type for the swings.
        /// </summary>
        private VisualizationStyle visualizationType = VisualizationStyle.Dots_ZigZag;
        /// <summary>
        /// Indicates if AutoScale is used for the swings. 
        /// </summary>
        private bool useAutoScale = true;
        /// <summary>
        /// Indicates if the swings are drawn on the price panel. 
        /// </summary>
        private bool drawSwingsOnPricePanel = true;
        /// <summary>
        /// Represents the color of the zig-zag up lines for the swings.
        /// </summary>
        private Brush zigZagColorUp = Brushes.LimeGreen;
        /// <summary>
        /// Represents the color of the zig-zag down lines for the swings.
        /// </summary>
        private Brush zigZagColorDn = Brushes.OrangeRed;
        /// <summary>
        /// Represents the line style of the zig-zag lines for the swings.
        /// </summary>
        private DashStyleHelper zigZagStyle = DashStyleHelper.Solid;
        /// <summary>
        /// Represents the line width of the zig-zag lines for the swings.
        /// </summary>
        private int zigZagWidth = 3;
        /// <summary>
        /// Represents the color of the swing value output for higher highs for the swings.
        /// </summary>
        private Brush textColorHigherHigh = Brushes.White;
        /// <summary>
        /// Represents the color of the swing value output for lower highs for the swings.
        /// </summary>
        private Brush textColorLowerHigh = Brushes.Black;
        /// <summary>
        /// Represents the color of the swing value output for double tops for the swings.
        /// </summary>
        private Brush textColorDoubleTop = Brushes.Gold;
        /// <summary>
        /// Represents the color of the swing value output for higher lows for the swings.
        /// </summary>
        private Brush textColorHigherLow = Brushes.White;
        /// <summary>
        /// Represents the color of the swing value output for lower lows swings for the swings.
        /// </summary>
        private Brush textColorLowerLow = Brushes.Black;
        /// <summary>
        /// Represents the color of the swing value output for double bottoms for the swings.
        /// </summary>
        private Brush textColorDoubleBottom = Brushes.Gold;
        /// <summary>
        /// Represents the text font for the swing value output for the swings.
        /// </summary>
        private SimpleFont textFont = new SimpleFont("Courier", 15);
        /// <summary>
        /// Represents the text offset in pixel for the swing length for the swings.
        /// </summary>
        private int textOffsetLength = 15;
        /// <summary>
        /// Represents the text offset in pixel for the retracement value for the swings.
        /// </summary>
        private int textOffsetPercent = 90;
        /// <summary>
        /// Represents the text offset in pixel for the swing price for the swings.
        /// </summary>
        private int textOffsetPrice = 45;
        /// <summary>
        /// Represents the text offset in pixel for the swing labels for the swings.
        /// </summary>
        private int textOffsetLabel = 60;
        /// <summary>
        /// Represents the text offset in pixel for the swing time for the swings.
        /// </summary>
        private int textOffsetTime = 75;
        /// <summary>
        /// Represents the text offset in pixel for the swing volume for the swings.
        /// </summary>
        private int textOffsetVolume = 30;

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
        //=========================================================================================
        #endregion

        #region Class objects and DataSeries
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
        /// <summary>
        /// Indicates if the swing direction changed form down to up swing for the swings.
        /// </summary>
        private Series<bool> upFlip;
        /// <summary>
        /// Indicates if the swing direction changed form up to down swing for the swings.
        /// </summary>
        private Series<bool> dnFlip;
        /// <summary>
        /// Represents a list of all up swings for the swings.
        /// </summary>
        private List<SwingStruct> swingHighs = new List<SwingStruct>();
        /// <summary>
        /// Represents a list of all down swings for the swings.
        /// </summary>
        private List<SwingStruct> swingLows = new List<SwingStruct>();
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
				Description							= @"PriceActionSwing calculates swings and visualize them in different ways and display several information about them.";
				Name								= "PriceActionSwing";
				Calculate							= Calculate.OnEachTick;
				IsOverlay							= true;
				DisplayInDataBox					= true;
				DrawOnPricePanel					= true;
				DrawHorizontalGridLines				= true;
				DrawVerticalGridLines				= true;
				PaintPriceMarkers					= true;
				ScaleJustification					= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive			= true;
				
		        #region Parameters
		        //=========================================================================================
				DtbStrength = 20;
				SwingSize = 7;
				SwingType = SwingStyle.Standard;
				UseCloseValues = false;
		        //=========================================================================================
		        #endregion	        
				
				AddPlot(new Stroke(Brushes.Gold, DashStyleHelper.Solid, 3), PlotStyle.Dot, "DoubleBottom");
				AddPlot(new Stroke(Brushes.Red, DashStyleHelper.Solid, 3), PlotStyle.Dot, "LowerLow");
				AddPlot(new Stroke(Brushes.Green, DashStyleHelper.Solid, 3), PlotStyle.Dot, "HigherLow");

				AddPlot(new Stroke(Brushes.Gold, DashStyleHelper.Solid, 3), PlotStyle.Dot, "DoubleTop");
				AddPlot(new Stroke(Brushes.Red, DashStyleHelper.Solid, 3), PlotStyle.Dot, "LowerHigh");
				AddPlot(new Stroke(Brushes.Green, DashStyleHelper.Solid, 3), PlotStyle.Dot, "HigherHigh");
				
				AddPlot(new Stroke(Brushes.Blue, DashStyleHelper.Solid, 1), PlotStyle.Square, "GannSwing");
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

	            swingProperties = new SwingProperties(SwingType, SwingSize, DtbStrength,
	                swingLengthType, swingDurationType, showSwingPrice, showSwingLabel,
	                showSwingPercent, swingTimeType, swingVolumeType, visualizationType,
	                useBreakouts, ignoreInsideBars, useAutoScale,
	                zigZagColorUp, zigZagColorDn, zigZagStyle, zigZagWidth, textColorHigherHigh,
	                textColorLowerHigh, textColorDoubleTop, textColorHigherLow,
	                textColorLowerLow, textColorDoubleBottom, textFont, textOffsetLength, 
	                textOffsetPercent, textOffsetPrice, textOffsetLabel, textOffsetTime, 
	                textOffsetVolume, UseCloseValues, drawSwingsOnPricePanel);
			}
			else if (State == State.Configure)
			{
				dnFlip = new Series<bool>(this);
				upFlip = new Series<bool>(this);
			}
		}
        //=========================================================================================
		#endregion

		protected override void OnBarUpdate()
		{
			//Print("Size: " + Convert.ToInt32(textFont.Size));
			
            // Checks to ensure there are enough bars before beginning
            if (CurrentBars[BarsInProgress] <= 1 
                || CurrentBars[BarsInProgress] < SwingSize)
                return;

            #region Swing calculation
            //=====================================================================================
            InitializeSwingCalculation(swingHigh, swingLow, swingCurrent, upFlip, swingHighs, 
                dnFlip, swingLows);

            switch (SwingType)
	        {
                case SwingStyle.Standard:
                    CalculateSwingStandard(swingHigh, swingLow, swingCurrent, swingProperties,
                        upFlip, swingHighs, dnFlip, swingLows, decimalPlaces, UseCloseValues,
                        DoubleBottom, LowerLow, HigherLow, DoubleTop, LowerHigh, HigherHigh, 
                        GannSwing);
                    break;
                case SwingStyle.Gann:
                    CalculateSwingGann(swingHigh, swingLow, swingCurrent, swingProperties, upFlip,
                        swingHighs, dnFlip, swingLows, decimalPlaces, DoubleBottom, LowerLow, 
                        HigherLow, DoubleTop, LowerHigh, HigherHigh, GannSwing);
                    break;
                case SwingStyle.Ticks:
                    CalculateSwingTicks(swingHigh, swingLow, swingCurrent, swingProperties,
                        upFlip, swingHighs, dnFlip, swingLows, decimalPlaces, UseCloseValues,
                        DoubleBottom, LowerLow, HigherLow, DoubleTop, LowerHigh, HigherHigh,
                        GannSwing);
                    break;
                case SwingStyle.Percent:
                    CalculateSwingPercent(swingHigh, swingLow, swingCurrent, swingProperties,
                        upFlip, swingHighs, dnFlip, swingLows, decimalPlaces, UseCloseValues,
                        DoubleBottom, LowerLow, HigherLow, DoubleTop, LowerHigh, HigherHigh,
                        GannSwing);
                    break;
            }
            //=====================================================================================
            #endregion
		}
		
        #region Initialize swing calculation
        //#########################################################################################
        public void InitializeSwingCalculation(Swings swingHigh, Swings swingLow,
            SwingCurrent swingCur, Series<bool> upFlip, List<SwingStruct> swingHighs,
            Series<bool> dnFlip, List<SwingStruct> swingLows)
        {
            if (IsFirstTickOfBar)
            {
                swingCur.StopOutsideBarCalc = false;

                // Initialize first swing
                if (swingHighs.Count == 0)
                {
                    swingHigh.CurBar = CurrentBars[BarsInProgress];
                    swingHigh.CurPrice = Highs[BarsInProgress][CurrentBars[BarsInProgress]];
                    swingHigh.CurDateTime = swingHigh.LastDateTime =
                        Times[BarsInProgress][CurrentBars[BarsInProgress]];
                    SwingStruct up = new SwingStruct(swingHigh.CurPrice, swingHigh.CurBar,
                        Times[BarsInProgress][CurrentBars[BarsInProgress] - 1], 0, 0, -1,
                        Convert.ToInt64(Volumes[BarsInProgress][0]));
                    swingHighs.Add(up);
                    swingHigh.ListCount = swingHighs.Count;
                }
                if (swingLows.Count == 0)
                {
                    swingLow.CurBar = CurrentBars[BarsInProgress];
                    swingLow.CurPrice = Lows[BarsInProgress][CurrentBars[BarsInProgress]];
                    swingLow.CurDateTime = swingLow.LastDateTime =
                        Times[BarsInProgress][CurrentBars[BarsInProgress]];
                    SwingStruct dn = new SwingStruct(swingLow.CurPrice, swingLow.CurBar,
                        Times[BarsInProgress][CurrentBars[BarsInProgress] - 1], 0, 0, -1,
                        Convert.ToInt64(Volumes[BarsInProgress][0]));
                    swingLows.Add(dn);
                    swingLow.ListCount = swingLows.Count;
                }
            }
            // Set new/update high/low back to false, to avoid function calls which depends on
            // them
            dnFlip[0] = false;
            upFlip[0] = false;
            swingHigh.New = swingLow.New = swingHigh.Update = swingLow.Update = false;
        }
        //#########################################################################################
        #endregion

        #region Calculate Swing Standard
        //#########################################################################################
        protected void CalculateSwingStandard(Swings swingHigh, Swings swingLow, 
            SwingCurrent swingCur, SwingProperties swingProp, Series<bool> upFlip,
            List<SwingStruct> swingHighs, Series<bool> dnFlip, List<SwingStruct> swingLows,
            int decimalPlaces, bool useCloseValues, Series<double> doubleBottom, Series<double> lowerLow,
            Series<double> higherLow, Series<double> doubleTop, Series<double> lowerHigh,
            Series<double> higherHigh, Series<double> gannSwing)
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
                CalcUpSwing(bar, price, swingHigh.Update, swingHigh, swingLow, swingCur,
                    swingProp, upFlip, swingHighs, decimalPlaces, doubleBottom, lowerLow, 
                    higherLow, doubleTop, lowerHigh, higherHigh, gannSwing);
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
                CalcDnSwing(bar, price, swingLow.Update, swingHigh, swingLow, swingCur,
                    swingProp, dnFlip, swingLows, decimalPlaces, doubleBottom, lowerLow, 
                    higherLow, doubleTop, lowerHigh, higherHigh, gannSwing);
            }            
        }
        //#########################################################################################
        #endregion

        #region Calculate Swing Gann
        //#########################################################################################
        protected void CalculateSwingGann(Swings swingHigh, Swings swingLow, SwingCurrent swingCur,
            SwingProperties swingProp, Series<bool> upFlip, List<SwingStruct> swingHighs,
            Series<bool> dnFlip, List<SwingStruct> swingLows, int decimalPlaces, 
            Series<double> doubleBottom, Series<double> lowerLow, Series<double> higherLow, 
            Series<double> doubleTop, Series<double> lowerHigh, Series<double> higherHigh, 
            Series<double> gannSwing)
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
                                swingLow, swingCur, swingProp, upFlip, swingHighs,
                                decimalPlaces, doubleBottom, lowerLow, higherLow, doubleTop, 
                                lowerHigh, higherHigh, gannSwing);
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
                            CalcDnSwing(bar, price, swingLow.Update, swingHigh, swingLow,
                                swingCur, swingProp, dnFlip, swingLows, decimalPlaces, 
                                doubleBottom, lowerLow, higherLow, doubleTop, lowerHigh, 
                                higherHigh, gannSwing);
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
                                swingLow, swingCur, swingProp, upFlip, swingHighs,
                                decimalPlaces, doubleBottom, lowerLow, higherLow, doubleTop,
                                lowerHigh, higherHigh, gannSwing);
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
                                CalcDnSwing(bar, price, swingLow.Update, swingHigh, swingLow,
                                    swingCur, swingProp, dnFlip, swingLows, decimalPlaces, 
                                    doubleBottom, lowerLow, higherLow, doubleTop, lowerHigh,
                                    higherHigh, gannSwing);
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
                                Lows[BarsInProgress][0], swingLow.Update, swingHigh,
                                swingLow, swingCur, swingProp, dnFlip, swingLows,
                                decimalPlaces, doubleBottom, lowerLow, higherLow, doubleTop,
                                lowerHigh, higherHigh, gannSwing);
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
                            CalcUpSwing(bar, price, swingHigh.Update, swingHigh, swingLow,
                                swingCur, swingProp, upFlip, swingHighs, decimalPlaces, 
                                doubleBottom, lowerLow, higherLow, doubleTop, lowerHigh,
                                higherHigh, gannSwing);
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
                                Lows[BarsInProgress][0], swingLow.Update, swingHigh,
                                swingLow, swingCur, swingProp, dnFlip, swingLows,
                                decimalPlaces, doubleBottom, lowerLow, higherLow, doubleTop,
                                lowerHigh, higherHigh, gannSwing);
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
                                    swingLow, swingCur, swingProp, upFlip, swingHighs,
                                    decimalPlaces, doubleBottom, lowerLow, higherLow, 
                                    doubleTop, lowerHigh, higherHigh, gannSwing);
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
            SwingCurrent swingCur, SwingProperties swingProp, Series<bool> upFlip,
            List<SwingStruct> swingHighs, Series<bool> dnFlip, List<SwingStruct> swingLows,
            int decimalPlaces, bool useCloseValues, Series<double> doubleBottom, Series<double> lowerLow,
            Series<double> higherLow, Series<double> doubleTop, Series<double> lowerHigh,
            Series<double> higherHigh, Series<double> gannSwing)
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
                CalcUpSwing(bar, price, swingHigh.Update, swingHigh, swingLow, swingCur,
                    swingProp, upFlip, swingHighs, decimalPlaces, doubleBottom, lowerLow,
                    higherLow, doubleTop, lowerHigh, higherHigh, gannSwing);
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
                CalcDnSwing(bar, price, swingLow.Update, swingHigh, swingLow, swingCur,
                    swingProp, dnFlip, swingLows, decimalPlaces, doubleBottom, lowerLow,
                    higherLow, doubleTop, lowerHigh, higherHigh, gannSwing);
            }
        }
        //#########################################################################################
        #endregion

        #region Calculate Swing Percent
        //#########################################################################################
        protected void CalculateSwingPercent(Swings swingHigh, Swings swingLow, 
            SwingCurrent swingCur, SwingProperties swingProp, Series<bool> upFlip, 
            List<SwingStruct> swingHighs, Series<bool> dnFlip, List<SwingStruct> swingLows,
            int decimalPlaces, bool useCloseValues, Series<double> doubleBottom, Series<double> lowerLow, 
            Series<double> higherLow, Series<double> doubleTop, Series<double> lowerHigh, 
            Series<double> higherHigh, Series<double> gannSwing)
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
                CalcUpSwing(bar, price, swingHigh.Update, swingHigh, swingLow, swingCur,
                    swingProp, upFlip, swingHighs, decimalPlaces, doubleBottom, lowerLow,
                    higherLow, doubleTop, lowerHigh, higherHigh, gannSwing);
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
                CalcDnSwing(bar, price, swingLow.Update, swingHigh, swingLow, swingCur,
                    swingProp, dnFlip, swingLows, decimalPlaces, doubleBottom, lowerLow,
                    higherLow, doubleTop, lowerHigh, higherHigh, gannSwing);
            }
        }
        //#########################################################################################
        #endregion

        #region Calculate down swing
        //#########################################################################################
        protected void CalcDnSwing(int bar, double low, bool updateLow, Swings swingHigh,
            Swings swingLow, SwingCurrent swingCur, SwingProperties swingProp, Series<bool> dnFlip,
            List<SwingStruct> swingLows, int decimalPlaces, Series<double> doubleBottom,
            Series<double> lowerLow, Series<double> higherLow, Series<double> doubleTop, Series<double> lowerHigh,
            Series<double> higherHigh, Series<double> gannSwing)
        {
            #region New and update Swing values
            //=====================================================================================
            if (!updateLow)
            {
                if (swingProp.VisualizationType == VisualizationStyle.GannStyle)
                {
                    for (int i = CurrentBar - swingCur.SwingSlopeChangeBar; i >= 0; i--)
                        gannSwing[i] = swingHigh.CurPrice;
                    gannSwing[0] = low;
                }
                swingLow.LastPrice = swingLow.CurPrice;
                swingLow.LastBar = swingLow.CurBar;
                swingLow.LastDateTime = swingLow.CurDateTime;
                swingLow.LastDuration = swingLow.CurDuration;
                swingLow.LastLength = swingLow.CurLength;
                swingLow.LastTime = swingLow.CurTime;
                swingLow.LastPercent = swingLow.CurPercent;
                swingLow.LastRelation = swingLow.CurRelation;
                swingLow.LastVolume = swingLow.CurVolume;
                swingLow.Counter++;
                swingCur.SwingSlope = -1;
                swingCur.SwingSlopeChangeBar = bar;
                dnFlip[0] = true;
            }
            else
            {
                if (swingProp.VisualizationType == VisualizationStyle.Dots
                    || swingProp.VisualizationType == VisualizationStyle.Dots_ZigZag)
                {
                    doubleBottom.Reset(CurrentBar - swingLow.CurBar);
                    higherLow.Reset(CurrentBar - swingLow.CurBar);
                    lowerLow.Reset(CurrentBar - swingLow.CurBar);
                }
                swingLows.RemoveAt(swingLows.Count - 1);
            }
            swingLow.CurBar = bar;
            swingLow.CurPrice = Math.Round(low, decimalPlaces, MidpointRounding.AwayFromZero);
            swingLow.CurTime = ToTime(Times[BarsInProgress][CurrentBars[BarsInProgress] -
                swingLow.CurBar]);
            swingLow.CurDateTime = Times[BarsInProgress][CurrentBars[BarsInProgress] -
                swingLow.CurBar];
            swingLow.CurLength = Convert.ToInt32(Math.Round((swingLow.CurPrice -
                swingHigh.CurPrice) / TickSize, 0, MidpointRounding.AwayFromZero));
            if (swingHigh.CurLength != 0)
                swingLow.CurPercent = Math.Round(100.0 / swingHigh.CurLength *
                    Math.Abs(swingLow.CurLength), 1);
            swingLow.CurDuration = swingLow.CurBar - swingHigh.CurBar;
            double dtbOffset = ATR(BarsArray[BarsInProgress], 14)[CurrentBars[BarsInProgress] -
                swingLow.CurBar] * swingProp.DtbStrength / 100;
            if (swingLow.CurPrice > swingLow.LastPrice - dtbOffset && swingLow.CurPrice <
                swingLow.LastPrice + dtbOffset)
                swingLow.CurRelation = 0;
            else if (swingLow.CurPrice < swingLow.LastPrice)
                swingLow.CurRelation = -1;
            else
                swingLow.CurRelation = 1;
            if (Calculate != Calculate.OnBarClose)
                swingHigh.SignalBarVolume = Volumes[BarsInProgress][0];
            double swingVolume = 0.0;
            for (int i = 0; i < swingLow.CurDuration; i++)
                swingVolume = swingVolume + Volumes[BarsInProgress][i];
            if (Calculate != Calculate.OnBarClose)
                swingVolume = swingVolume + (Volumes[BarsInProgress][CurrentBars[BarsInProgress] -
                    swingHigh.CurBar] - swingLow.SignalBarVolume);
            if (swingProp.SwingVolumeType == SwingVolumeStyle.Relative)
                swingVolume = Math.Round(swingVolume / swingLow.CurDuration, 0,
                    MidpointRounding.AwayFromZero);
            swingLow.CurVolume = Convert.ToInt64(swingVolume);
            //=====================================================================================
            #endregion

            #region Visualize swing
            //=====================================================================================
            switch (swingProp.VisualizationType)
            {
                case VisualizationStyle.False:
                    break;
                case VisualizationStyle.Dots:
                    switch (swingLow.CurRelation)
                    {
                        case 1:
                            higherLow[CurrentBar - swingLow.CurBar] = swingLow.CurPrice;
                            break;
                        case -1:
                            lowerLow[CurrentBar - swingLow.CurBar] = swingLow.CurPrice;
                            break;
                        case 0:
                            doubleBottom[CurrentBar - swingLow.CurBar] = swingLow.CurPrice;
                            break;
                    }
                    break;
                case VisualizationStyle.Dots_ZigZag:
                    switch (swingLow.CurRelation)
                    {
                        case 1:
                            higherLow[CurrentBar - swingLow.CurBar] = swingLow.CurPrice;
                            break;
                        case -1:
                            lowerLow[CurrentBar - swingLow.CurBar] = swingLow.CurPrice;
                            break;
                        case 0:
                            doubleBottom[CurrentBar - swingLow.CurBar] = swingLow.CurPrice;
                            break;
                    }
                    Draw.Line(this, "ZigZagDown" + swingLow.Counter,
                        swingProp.UseAutoScale, CurrentBar - swingHigh.CurBar, swingHigh.CurPrice,
                        CurrentBar - swingLow.CurBar, swingLow.CurPrice, swingProp.ZigZagColorDn,
                        swingProp.ZigZagStyle, swingProp.ZigZagWidth, swingProp.DrawSwingsOnPricePanel);
                    break;
                case VisualizationStyle.ZigZag:
                    Draw.Line(this, "ZigZagDown" + swingLow.Counter,
                        swingProp.UseAutoScale, CurrentBar - swingHigh.CurBar, swingHigh.CurPrice,
                        CurrentBar - swingLow.CurBar, swingLow.CurPrice, swingProp.ZigZagColorDn,
                        swingProp.ZigZagStyle, swingProp.ZigZagWidth, swingProp.DrawSwingsOnPricePanel);
                    break;
                case VisualizationStyle.GannStyle:
                    for (int i = CurrentBar - swingCur.SwingSlopeChangeBar; i >= 0; i--)
                        gannSwing[i] = swingLow.CurPrice;
                    break;
                case VisualizationStyle.ZigZagVolume:
                    if (swingLow.CurVolume > swingHigh.CurVolume)
                        Draw.Line(this, "ZigZagDown" + swingLow.Counter,
                            swingProp.UseAutoScale, CurrentBar - swingHigh.CurBar, 
                            swingHigh.CurPrice, CurrentBar - swingLow.CurBar, swingLow.CurPrice, 
                            swingProp.ZigZagColorDn, swingProp.ZigZagStyle, swingProp.ZigZagWidth,
							swingProp.DrawSwingsOnPricePanel);
                    else
                        Draw.Line(this, "ZigZagDown" + swingLow.Counter,
                            swingProp.UseAutoScale, CurrentBar - swingHigh.CurBar,
                            swingHigh.CurPrice, CurrentBar - swingLow.CurBar, swingLow.CurPrice,
                            swingProp.ZigZagColorUp, swingProp.ZigZagStyle, swingProp.ZigZagWidth, 
							swingProp.DrawSwingsOnPricePanel);
                    break;
            }
            //=====================================================================================
            #endregion

            #region Swing value output
            //=====================================================================================
            string output = "";
            switch (swingProp.SwingLengthType)
            {
                case SwingLengthStyle.False:
                    break;
                case SwingLengthStyle.Ticks:
                    output = swingLow.CurLength.ToString();
                    break;
                case SwingLengthStyle.Ticks_Price:
                    output = swingLow.CurLength.ToString() + " / " + swingLow.CurPrice.ToString();
                    break;
                case SwingLengthStyle.Price_Ticks:
                    output = swingLow.CurPrice.ToString() + " / " + swingLow.CurLength.ToString();
                    break;
                case SwingLengthStyle.Points:
                    output = (swingLow.CurLength * TickSize).ToString();
                    break;
                case SwingLengthStyle.Points_Price:
                    output = (swingLow.CurLength * TickSize).ToString() + " / " +
                        swingLow.CurPrice.ToString();
                    break;
                case SwingLengthStyle.Price_Points:
                    output = swingLow.CurPrice.ToString() + " / " +
                        (swingLow.CurLength * TickSize).ToString();
                    break;
                case SwingLengthStyle.Price:
                    output = swingLow.CurPrice.ToString();
                    break;
                case SwingLengthStyle.Percent:
                    output = (Math.Round((100.0 / swingHigh.CurPrice * (swingLow.CurLength *
                        TickSize)), 2, MidpointRounding.AwayFromZero)).ToString();
                    break;
            }
            string outputDuration = "";
            TimeSpan timeSpan;
            int hours, minutes, seconds = 0;
            switch (swingProp.SwingDurationType)
            {
                case SwingDurationStyle.False:
                    break;
                case SwingDurationStyle.Bars:
                    outputDuration = swingLow.CurDuration.ToString();
                    break;
                case SwingDurationStyle.MMSS:
                    timeSpan = swingLow.CurDateTime.Subtract(swingHigh.CurDateTime);
                    minutes = timeSpan.Minutes;
                    seconds = timeSpan.Seconds;
                    if (minutes == 0)
                        outputDuration = "0:" + seconds.ToString();
                    else if (seconds == 0)
                        outputDuration = minutes + ":00";
                    else
                        outputDuration = minutes + ":" + seconds;
                    break;
                case SwingDurationStyle.HHMM:
                    timeSpan = swingLow.CurDateTime.Subtract(swingHigh.CurDateTime);
                    hours = timeSpan.Hours;
                    minutes = timeSpan.Minutes;
                    if (hours == 0)
                        outputDuration = "0:" + minutes.ToString();
                    else if (minutes == 0)
                        outputDuration = hours + ":00";
                    else
                        outputDuration = hours + ":" + minutes;
                    break;
                case SwingDurationStyle.SecondsTotal:
                    timeSpan = swingLow.CurDateTime.Subtract(swingHigh.CurDateTime);
                    outputDuration = Math.Round(timeSpan.TotalSeconds, 1,
                        MidpointRounding.AwayFromZero).ToString();
                    break;
                case SwingDurationStyle.MinutesTotal:
                    timeSpan = swingLow.CurDateTime.Subtract(swingHigh.CurDateTime);
                    outputDuration = Math.Round(timeSpan.TotalMinutes, 1,
                        MidpointRounding.AwayFromZero).ToString();
                    break;
                case SwingDurationStyle.HoursTotal:
                    timeSpan = swingLow.CurDateTime.Subtract(swingHigh.CurDateTime);
                    outputDuration = timeSpan.TotalHours.ToString();
                    break;
                case SwingDurationStyle.Days:
                    timeSpan = swingLow.CurDateTime.Subtract(swingHigh.CurDateTime);
                    outputDuration = Math.Round(timeSpan.TotalDays, 1,
                        MidpointRounding.AwayFromZero).ToString();
                    break;
            }
            if (swingProp.SwingLengthType != SwingLengthStyle.False)
            {
                if (swingProp.SwingDurationType != SwingDurationStyle.False)
                    output = output + " / " + outputDuration;
            }
            else
                output = outputDuration;

            string swingLabel = null;
            Brush textColor = Brushes.Transparent;
            switch (swingLow.CurRelation)
            {
                case 1:
                    swingLabel = "HL";
                    textColor = swingProp.TextColorHigherLow;
                    break;
                case -1:
                    swingLabel = "LL";
                    textColor = swingProp.TextColorLowerLow;
                    break;
                case 0:
                    swingLabel = "DB";
                    textColor = swingProp.TextColorDoubleBottom;
                    break;
            }
            if (output != null)
                Draw.Text(this, "DnLength" + swingLow.Counter, swingProp.UseAutoScale,
                    output.ToString(), CurrentBar - swingLow.CurBar, swingLow.CurPrice,
                    -swingProp.TextOffsetLength, textColor, swingProp.TextFont,
                    TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
            if (swingProp.ShowSwingLabel)
                Draw.Text(this, "DnLabel" + swingLow.Counter, swingProp.UseAutoScale,
                    swingLabel, CurrentBar - swingLow.CurBar, swingLow.CurPrice,
                    -swingProp.TextOffsetLabel, textColor, swingProp.TextFont,
                    TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
            if (swingProp.ShowSwingPrice)
                Draw.Text(this, "DnPrice" + swingLow.Counter, swingProp.UseAutoScale,
                    String.Format("{0:F" + decimalPlaces + "}", swingLow.CurPrice),
                    CurrentBar - swingLow.CurBar, swingLow.CurPrice, -swingProp.TextOffsetPrice,
                    textColor, swingProp.TextFont, TextAlignment.Center, Brushes.Transparent,
                    Brushes.Transparent, 0);
            if (swingProp.ShowSwingPercent && swingLow.CurPercent != 0)
                Draw.Text(this, "DnPerc" + swingLow.Counter, swingProp.UseAutoScale,
                    String.Format("{0:F1}", swingLow.CurPercent) + "%", CurrentBar - swingLow.CurBar,
                    swingLow.CurPrice, -swingProp.TextOffsetPercent, textColor,
                    swingProp.TextFont, TextAlignment.Center, Brushes.Transparent,
                    Brushes.Transparent, 0);
            if (swingProp.SwingTimeType != SwingTimeStyle.False)
            {
                string timeOutput = "";
                switch (swingProp.SwingTimeType)
                {
                    case SwingTimeStyle.False:
                        break;
                    case SwingTimeStyle.Integer:
                        timeOutput = swingLow.CurTime.ToString();
                        break;
                    case SwingTimeStyle.HHMM:
                        timeOutput = string.Format("{0:t}",
                            Times[BarsInProgress][CurrentBars[BarsInProgress] - swingLow.CurBar]);
                        break;
                    case SwingTimeStyle.HHMMSS:
                        timeOutput = string.Format("{0:T}",
                            Times[BarsInProgress][CurrentBars[BarsInProgress] - swingLow.CurBar]);
                        break;
                    case SwingTimeStyle.DDMM:
                        timeOutput = string.Format("{0:dd.MM}",
                            Times[BarsInProgress][CurrentBars[BarsInProgress] - swingHigh.CurBar]);
                        break;
                }
                Draw.Text(this, "DnTime" + swingLow.Counter, swingProp.UseAutoScale,
                    timeOutput, CurrentBar - swingLow.CurBar, swingLow.CurPrice, -swingProp.TextOffsetTime,
                    textColor, swingProp.TextFont, TextAlignment.Center, Brushes.Transparent,
                    Brushes.Transparent, 0);
            }
            if (swingProp.SwingVolumeType != SwingVolumeStyle.False)
                Draw.Text(this, "DnVolume" + swingLow.Counter, swingProp.UseAutoScale,
                    TruncIntToStr(swingLow.CurVolume), CurrentBar - swingLow.CurBar,
                    swingLow.CurPrice, -swingProp.TextOffsetVolume, textColor, swingProp.TextFont,
                    TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
            //=====================================================================================
            #endregion

            SwingStruct dn = new SwingStruct(swingLow.CurPrice, swingLow.CurBar,
                swingLow.CurDateTime, swingLow.CurDuration, swingLow.CurLength,
                swingLow.CurRelation, swingLow.CurVolume);
            swingLows.Add(dn);
            swingLow.ListCount = swingLows.Count - 1;
        }
        //#########################################################################################
        #endregion

        #region Calculate up swing
        //#########################################################################################
        private void CalcUpSwing(int bar, double high, bool updateHigh, Swings swingHigh,
            Swings swingLow, SwingCurrent swingCur, SwingProperties swingProp, Series<bool> upFlip,
            List<SwingStruct> swingHighs, int decimalPlaces, Series<double> doubleBottom, 
            Series<double> lowerLow, Series<double> higherLow, Series<double> doubleTop, Series<double> lowerHigh, 
            Series<double> higherHigh, Series<double> gannSwing)
        {
            #region New and update swing values
            //=====================================================================================
            if (!updateHigh)
            {
                if (swingProp.VisualizationType == VisualizationStyle.GannStyle)
                {
                    for (int i = CurrentBar - swingCur.SwingSlopeChangeBar; i >= 0; i--)
                        gannSwing[i] = swingLow.CurPrice;
                    gannSwing[0] = high;
                }
                swingHigh.LastPrice = swingHigh.CurPrice;
                swingHigh.LastBar = swingHigh.CurBar;
                swingHigh.LastDateTime = swingHigh.CurDateTime;
                swingHigh.LastDuration = swingHigh.CurDuration;
                swingHigh.LastLength = swingHigh.CurLength;
                swingHigh.LastTime = swingHigh.CurTime;
                swingHigh.LastPercent = swingHigh.CurPercent;
                swingHigh.LastRelation = swingHigh.CurRelation;
                swingHigh.LastVolume = swingHigh.CurVolume;
                swingHigh.Counter++;
                swingCur.SwingSlope = 1;
                swingCur.SwingSlopeChangeBar = bar;
                upFlip[0] = true;
            }
            else
            {
                if (swingProp.VisualizationType == VisualizationStyle.Dots
                    || swingProp.VisualizationType == VisualizationStyle.Dots_ZigZag)
                {
                    doubleTop.Reset(CurrentBar - swingHigh.CurBar);
                    higherHigh.Reset(CurrentBar - swingHigh.CurBar);
                    lowerHigh.Reset(CurrentBar - swingHigh.CurBar);
                }
                swingHighs.RemoveAt(swingHighs.Count - 1);
            }
            swingHigh.CurBar = bar;
            swingHigh.CurPrice = Math.Round(high, decimalPlaces, MidpointRounding.AwayFromZero);
            swingHigh.CurTime = ToTime(Times[BarsInProgress][CurrentBars[BarsInProgress] -
                swingHigh.CurBar]);
            swingHigh.CurDateTime = Times[BarsInProgress][CurrentBars[BarsInProgress] -
                swingHigh.CurBar];
            swingHigh.CurLength = Convert.ToInt32(Math.Round((swingHigh.CurPrice -
                swingLow.CurPrice) / TickSize, 0, MidpointRounding.AwayFromZero));
            if (swingLow.CurLength != 0)
                swingHigh.CurPercent = Math.Round(100.0 / Math.Abs(swingLow.CurLength) *
                    swingHigh.CurLength, 1);
            swingHigh.CurDuration = swingHigh.CurBar - swingLow.CurBar;
            double dtbOffset = ATR(BarsArray[BarsInProgress], 14)[CurrentBars[BarsInProgress] -
                swingHigh.CurBar] * swingProp.DtbStrength / 100;
            if (swingHigh.CurPrice > swingHigh.LastPrice - dtbOffset && swingHigh.CurPrice <
                swingHigh.LastPrice + dtbOffset)
                swingHigh.CurRelation = 0;
            else if (swingHigh.CurPrice < swingHigh.LastPrice)
                swingHigh.CurRelation = -1;
            else
                swingHigh.CurRelation = 1;
            if (Calculate != Calculate.OnBarClose)
                swingLow.SignalBarVolume = Volumes[BarsInProgress][0];
            double swingVolume = 0.0;
            for (int i = 0; i < swingHigh.CurDuration; i++)
                swingVolume = swingVolume + Volumes[BarsInProgress][i];
            if (Calculate != Calculate.OnBarClose)
                swingVolume = swingVolume + (Volumes[BarsInProgress][CurrentBars[BarsInProgress] -
                    swingLow.CurBar] - swingHigh.SignalBarVolume);
            if (swingProp.SwingVolumeType == SwingVolumeStyle.Relative)
                swingVolume = Math.Round(swingVolume / swingHigh.CurDuration, 0,
                    MidpointRounding.AwayFromZero);
            swingHigh.CurVolume = Convert.ToInt64(swingVolume);
            //=====================================================================================
            #endregion

            #region Visualize swing
            //=====================================================================================
            switch (swingProp.VisualizationType)
            {
                case VisualizationStyle.False:
                    break;
                case VisualizationStyle.Dots:
                    switch (swingHigh.CurRelation)
                    {
                        case 1:
                            higherHigh[CurrentBar - swingHigh.CurBar] = swingHigh.CurPrice;
                            break;
                        case -1:
                            lowerHigh[CurrentBar - swingHigh.CurBar] = swingHigh.CurPrice;
                            break;
                        case 0:
                            doubleTop[CurrentBar - swingHigh.CurBar] = swingHigh.CurPrice;
                            break;
                    }
                    break;
                case VisualizationStyle.Dots_ZigZag:
                    switch (swingHigh.CurRelation)
                    {
                        case 1:
                            higherHigh[CurrentBar - swingHigh.CurBar] = swingHigh.CurPrice;
                            break;
                        case -1:
                            lowerHigh[CurrentBar - swingHigh.CurBar] = swingHigh.CurPrice;
                            break;
                        case 0:
                            doubleTop[CurrentBar - swingHigh.CurBar] = swingHigh.CurPrice;
                            break;
                    }
                    Draw.Line(this, "ZigZagUp" + swingHigh.Counter,
                        swingProp.UseAutoScale, CurrentBar - swingLow.CurBar, swingLow.CurPrice,
                        CurrentBar - swingHigh.CurBar, swingHigh.CurPrice, swingProp.ZigZagColorUp,
                        swingProp.ZigZagStyle, swingProp.ZigZagWidth);
                    break;
                case VisualizationStyle.ZigZag:
                    Draw.Line(this, "ZigZagUp" + swingHigh.Counter,
                        swingProp.UseAutoScale, CurrentBar - swingLow.CurBar, swingLow.CurPrice,
                        CurrentBar - swingHigh.CurBar, swingHigh.CurPrice, swingProp.ZigZagColorUp,
                        swingProp.ZigZagStyle, swingProp.ZigZagWidth);
                    break;
                case VisualizationStyle.GannStyle:
                    for (int i = CurrentBar - swingCur.SwingSlopeChangeBar; i >= 0; i--)
                        gannSwing[i] = swingHigh.CurPrice;
                    break;
                case VisualizationStyle.ZigZagVolume:
                    if (swingHigh.CurVolume > swingLow.CurVolume)
                        Draw.Line(this, "ZigZagUp" + swingHigh.Counter,
                            swingProp.UseAutoScale, CurrentBar - swingLow.CurBar, 
                            swingLow.CurPrice, CurrentBar - swingHigh.CurBar, swingHigh.CurPrice, 
                            swingProp.ZigZagColorUp, swingProp.ZigZagStyle, swingProp.ZigZagWidth);
                    else
                        Draw.Line(this, "ZigZagUp" + swingHigh.Counter,
                            swingProp.UseAutoScale, CurrentBar - swingLow.CurBar,
                            swingLow.CurPrice, CurrentBar - swingHigh.CurBar, swingHigh.CurPrice,
                            swingProp.ZigZagColorDn, swingProp.ZigZagStyle, swingProp.ZigZagWidth);
                    break;
            }
            //=====================================================================================
            #endregion

            #region Swing value output
            //=====================================================================================
            string output = "";
            switch (swingProp.SwingLengthType)
            {
                case SwingLengthStyle.False:
                    break;
                case SwingLengthStyle.Ticks:
                    output = swingHigh.CurLength.ToString();
                    break;
                case SwingLengthStyle.Ticks_Price:
                    output = swingHigh.CurLength.ToString() + " / " +
                        swingHigh.CurPrice.ToString();
                    break;
                case SwingLengthStyle.Price_Ticks:
                    output = swingHigh.CurPrice.ToString() + " / " +
                        swingHigh.CurLength.ToString();
                    break;
                case SwingLengthStyle.Points:
                    output = (swingHigh.CurLength * TickSize).ToString();
                    break;
                case SwingLengthStyle.Points_Price:
                    output = (swingHigh.CurLength * TickSize).ToString() + " / " +
                        swingHigh.CurPrice.ToString();
                    break;
                case SwingLengthStyle.Price_Points:
                    output = swingHigh.CurPrice.ToString() + " / " +
                        (swingHigh.CurLength * TickSize).ToString();
                    break;
                case SwingLengthStyle.Price:
                    output = swingHigh.CurPrice.ToString();
                    break;
                case SwingLengthStyle.Percent:
                    output = (Math.Round((100.0 / swingLow.CurPrice * (swingHigh.CurLength *
                        TickSize)), 2, MidpointRounding.AwayFromZero)).ToString();
                    break;
            }
            string outputDuration = "";
            TimeSpan timeSpan;
            int hours, minutes, seconds = 0;
            switch (swingProp.SwingDurationType)
            {
                case SwingDurationStyle.False:
                    break;
                case SwingDurationStyle.Bars:
                    outputDuration = swingHigh.CurDuration.ToString();
                    break;
                case SwingDurationStyle.MMSS:
                    timeSpan = swingHigh.CurDateTime.Subtract(swingLow.CurDateTime);
                    minutes = timeSpan.Minutes;
                    seconds = timeSpan.Seconds;
                    if (minutes == 0)
                        outputDuration = "0:" + seconds.ToString();
                    else if (seconds == 0)
                        outputDuration = minutes + ":00";
                    else
                        outputDuration = minutes + ":" + seconds;
                    break;
                case SwingDurationStyle.HHMM:
                    timeSpan = swingHigh.CurDateTime.Subtract(swingLow.CurDateTime);
                    hours = timeSpan.Hours;
                    minutes = timeSpan.Minutes;
                    if (hours == 0)
                        outputDuration = "0:" + minutes.ToString();
                    else if (minutes == 0)
                        outputDuration = hours + ":00";
                    else
                        outputDuration = hours + ":" + minutes;
                    break;
                case SwingDurationStyle.SecondsTotal:
                    timeSpan = swingHigh.CurDateTime.Subtract(swingLow.CurDateTime);
                    outputDuration = Math.Round(timeSpan.TotalSeconds, 1,
                        MidpointRounding.AwayFromZero).ToString();
                    break;
                case SwingDurationStyle.MinutesTotal:
                    timeSpan = swingHigh.CurDateTime.Subtract(swingLow.CurDateTime);
                    outputDuration = Math.Round(timeSpan.TotalMinutes, 1,
                        MidpointRounding.AwayFromZero).ToString();
                    break;
                case SwingDurationStyle.HoursTotal:
                    timeSpan = swingHigh.CurDateTime.Subtract(swingLow.CurDateTime);
                    outputDuration = Math.Round(timeSpan.TotalHours, 1,
                        MidpointRounding.AwayFromZero).ToString();
                    break;
                case SwingDurationStyle.Days:
                    timeSpan = swingHigh.CurDateTime.Subtract(swingLow.CurDateTime);
                    outputDuration = Math.Round(timeSpan.TotalDays, 1,
                        MidpointRounding.AwayFromZero).ToString();
                    break;
            }
            if (swingProp.SwingLengthType != SwingLengthStyle.False)
            {
                if (swingProp.SwingDurationType != SwingDurationStyle.False)
                    output = output + " / " + outputDuration;
            }
            else
                output = outputDuration;

            string swingLabel = null;
            Brush textColor = Brushes.Transparent;
            switch (swingHigh.CurRelation)
            {
                case 1:
                    swingLabel = "HH";
                    textColor = swingProp.TextColorHigherHigh;
                    break;
                case -1:
                    swingLabel = "LH";
                    textColor = swingProp.TextColorLowerHigh;
                    break;
                case 0:
                    swingLabel = "DT";
                    textColor = swingProp.TextColorDoubleTop;
                    break;
            }
            if (output != null)
                Draw.Text(this, "UpLength" + swingHigh.Counter,
                    swingProp.UseAutoScale, output.ToString(), CurrentBar - swingHigh.CurBar,
                    swingHigh.CurPrice, swingProp.TextOffsetLength + Convert.ToInt32(textFont.Size), textColor, swingProp.TextFont,
                    TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
            if (swingProp.ShowSwingLabel)
                Draw.Text(this, "UpLabel" + swingHigh.Counter, swingProp.UseAutoScale,
                    swingLabel, CurrentBar - swingHigh.CurBar, swingHigh.CurPrice,
                    swingProp.TextOffsetLabel + Convert.ToInt32(textFont.Size), textColor, swingProp.TextFont,
                    TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
            if (swingProp.ShowSwingPrice)
                Draw.Text(this, "UpPrice" + swingHigh.Counter, swingProp.UseAutoScale,
                    String.Format("{0:F" + decimalPlaces + "}", swingHigh.CurPrice),
                    CurrentBar - swingHigh.CurBar, swingHigh.CurPrice, swingProp.TextOffsetPrice + Convert.ToInt32(textFont.Size),
                    textColor, swingProp.TextFont, TextAlignment.Center, Brushes.Transparent,
                    Brushes.Transparent, 0);
            if (swingProp.ShowSwingPercent && swingHigh.CurPercent != 0)
                Draw.Text(this, "UpPerc" + swingHigh.Counter, swingProp.UseAutoScale,
                    String.Format("{0:F1}", swingHigh.CurPercent) + "%", CurrentBar - swingHigh.CurBar,
                    swingHigh.CurPrice, swingProp.TextOffsetPercent + Convert.ToInt32(textFont.Size), textColor, swingProp.TextFont,
                    TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
            if (swingProp.SwingTimeType != SwingTimeStyle.False)
            {
                string timeOutput = "";
                switch (swingProp.SwingTimeType)
                {
                    case SwingTimeStyle.False:
                        break;
                    case SwingTimeStyle.Integer:
                        timeOutput = swingHigh.CurTime.ToString();
                        break;
                    case SwingTimeStyle.HHMM:
                        timeOutput = string.Format("{0:t}",
                            Times[BarsInProgress][CurrentBars[BarsInProgress] - swingHigh.CurBar]);
                        break;
                    case SwingTimeStyle.HHMMSS:
                        timeOutput = string.Format("{0:T}",
                            Times[BarsInProgress][CurrentBars[BarsInProgress] - swingHigh.CurBar]);
                        break;
                    case SwingTimeStyle.DDMM:
                        timeOutput = string.Format("{0:dd.MM}",
                            Times[BarsInProgress][CurrentBars[BarsInProgress] - swingHigh.CurBar]);
                        break;
                }
                Draw.Text(this, "UpTime" + swingHigh.Counter, swingProp.UseAutoScale,
                    timeOutput, CurrentBar - swingHigh.CurBar, swingHigh.CurPrice,
                    swingProp.TextOffsetTime+ Convert.ToInt32(textFont.Size), textColor, swingProp.TextFont,
                    TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
            }
            if (swingProp.SwingVolumeType != SwingVolumeStyle.False)
                Draw.Text(this, "UpVolume" + swingHigh.Counter, swingProp.UseAutoScale,
                    TruncIntToStr(swingHigh.CurVolume), CurrentBar - swingHigh.CurBar,
                    swingHigh.CurPrice, swingProp.TextOffsetVolume + Convert.ToInt32(textFont.Size), textColor, swingProp.TextFont,
                    TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
            //=========================================================================================
            #endregion

            SwingStruct up = new SwingStruct(swingHigh.CurPrice, swingHigh.CurBar,
                swingHigh.CurDateTime, swingHigh.CurDuration, swingHigh.CurLength,
                swingHigh.CurRelation, swingHigh.CurVolume);
            swingHighs.Add(up);
            swingHigh.ListCount = swingHighs.Count - 1;
        }
        //#########################################################################################
        #endregion

        #region Trunc integer to string
        //#########################################################################################
        /// <summary>
        /// Converts long integer numbers in a number-string format.
        /// </summary>
        protected string TruncIntToStr(long number)
        {
            long numberAbs = Math.Abs(number);
            string output = "";
            double convertedNumber = 0.0;
            if (numberAbs > 1000000000)
            {
                convertedNumber = Math.Round(number / 1000000000.0, 1,
                    MidpointRounding.AwayFromZero);
                output = convertedNumber.ToString() + "B";
            }
            else if (numberAbs > 1000000)
            {
                convertedNumber = Math.Round(number / 1000000.0, 1,
                    MidpointRounding.AwayFromZero);
                output = convertedNumber.ToString() + "M";
            }
            else if (numberAbs > 1000)
            {
                convertedNumber = Math.Round(number / 1000.0, 1,
                    MidpointRounding.AwayFromZero);
                output = convertedNumber.ToString() + "K";
            }
            else
                output = number.ToString();

            return output;
        }
        //#########################################################################################
        #endregion
		
		#region Properties
		
		#region Plots
        // Plots ==================================================================================
		[Browsable(false)]
        [XmlIgnore]
        public Series<double> DoubleBottom
        {
            get { return Values[0]; }
        }
		[Browsable(false)]
        [XmlIgnore]
        public Series<double> LowerLow
        {
            get { return Values[1]; }
        }
		[Browsable(false)]
        [XmlIgnore]
        public Series<double> HigherLow
        {
            get { return Values[2]; }
        }
		[Browsable(false)]
        [XmlIgnore]
        public Series<double> DoubleTop
        {
            get { return Values[3]; }
        }
		[Browsable(false)]
        [XmlIgnore]
        public Series<double> LowerHigh
        {
            get { return Values[4]; }
        }
		[Browsable(false)]
        [XmlIgnore]
        public Series<double> HigherHigh
        {
            get { return Values[5]; }
        }
		[Browsable(false)]
        [XmlIgnore]
        public Series<double> GannSwing
        {
            get { return Values[6]; }
        }
		//=========================================================================================
        #endregion

        #region Parameters
        //=========================================================================================
		[NinjaScriptProperty]
		[Display(Name = "Swing type", Description = "Represents the swing type for the swings.", Order = 1, GroupName = "Parameters")]
		public SwingStyle SwingType
		{ get; set; }
		
		[Range(0.00000001, double.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "Swing size", Description = "Represents the swing size. e.g. 1 = small swings and 5 = bigger swings.", Order = 2, GroupName = "Parameters")]
		public double SwingSize
		{ get; set; }
		
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "Double top/-bottom strength", Description = "Represents the double top/-bottom strength. Increase the value to get more DB/DT.", Order = 3, GroupName = "Parameters")]
		public int DtbStrength
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Use close values", Description = "Indicates if high and low prices are used for the swing calculations or close values.", Order = 4, GroupName = "Parameters")]
		public bool UseCloseValues
		{ get; set; }
        //=========================================================================================
        #endregion

		#region Swings Values
		//=========================================================================================
		[NinjaScriptProperty]
		[Display(Name = "Length", Description = "Represents the swing length visualization type for the swings.", Order = 1, GroupName = "1. Swing Values")]
		public SwingLengthStyle SwingLengthType
		{
			get { return swingLengthType; }
			set { swingLengthType = value; }
		}

		[NinjaScriptProperty]
		[Display(Name = "Duration", Description = "Represents the swing duration visualization type for the swings.", Order = 2, GroupName = "1. Swing Values")]
		public SwingDurationStyle SwingDurationType
		{
			get { return swingDurationType; }
			set { swingDurationType = value; }
		}

		[NinjaScriptProperty]
		[Display(Name = "Show Price", Description = "Indicates if the swing price is shown for the swings.", Order = 3, GroupName = "1. Swing Values")]
		public bool ShowSwingPrice
		{
			get { return showSwingPrice; }
			set { showSwingPrice = value; }
		}

		[NinjaScriptProperty]
		[Display(Name = "Show Labels", Description = "Indicates if the swing label is shown (HH, HL, LL, LH, DB, DT).", Order = 4, GroupName = "1. Swing Values")]
		public bool ShowSwingLabel
		{
			get { return showSwingLabel; }
			set { showSwingLabel = value; }
		}

		[NinjaScriptProperty]
		[Display(Name = "Percentage", Description = "Indicates if the swing percentage in relation to the last swing is shown.", Order = 5, GroupName = "1. Swing Values")]
		public bool ShowSwingPercent
		{
			get { return showSwingPercent; }
			set { showSwingPercent = value; }
		}

		[NinjaScriptProperty]
		[Display(Name = "Time", Description = "Represents the swing time visualization type for the swings.", Order = 6, GroupName = "1. Swing Values")]
		public SwingTimeStyle SwingTimeType
		{
			get { return swingTimeType; }
			set { swingTimeType = value; }
		}

		[NinjaScriptProperty]
		[Display(Name = "Volume", Description = "Represents the swing volume visualization type for the swings.", Order = 7, GroupName = "1. Swing Values")]
		public SwingVolumeStyle SwingVolumeType
		{
			get { return swingVolumeType; }
			set { swingVolumeType = value; }
		}
		//=========================================================================================
		#endregion

		#region Visualize Swings
		//=========================================================================================
		[NinjaScriptProperty]
		[Display(Name = "Visualization Type", Description = "Represents the swing visualization type for the swings.", Order = 1, GroupName = "2. Visualize Swings")]
		public VisualizationStyle VisualizationType
		{
			get { return visualizationType; }
			set { visualizationType = value; }
		}

		[NinjaScriptProperty]
		[Display(Name = "Text Font", Description = "Represents the text font for the swing value output.", Order = 2, GroupName = "2. Visualize Swings")]
		public NinjaTrader.Gui.Tools.SimpleFont TextFont
		{
			get { return textFont; }
			set { textFont = value; }
		}

		[XmlIgnore]
		[Display(Name = "Text Color Higher High", Description = "Represents the color of the swing value output for higher highs.", Order = 3, GroupName = "2. Visualize Swings")]
		public Brush TextColorHigherHigh
		{
			get { return textColorHigherHigh; }
			set { textColorHigherHigh = value; }
		}

		[Browsable(false)]
		public string TextColorHigherHighSerializable
		{
			get { return Serialize.BrushToString(textColorHigherHigh); }
			set { textColorHigherHigh = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "Text Color Lower High", Description = "Represents the color of the swing value output for lower highs.", Order = 4, GroupName = "2. Visualize Swings")]
		public Brush TextColorLowerHigh
		{
			get { return textColorLowerHigh; }
			set { textColorLowerHigh = value; }
		}
		[Browsable(false)]
		public string TextColorLowerHighSerializable
		{
			get { return Serialize.BrushToString(textColorLowerHigh); }
			set { textColorLowerHigh = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "Text Color Double Top", Description = "Represents the color of the swing value output for double tops.", Order = 5, GroupName = "2. Visualize Swings")]
		public Brush TextColorDoubleTop
		{
			get { return textColorDoubleTop; }
			set { textColorDoubleTop = value; }
		}
		[Browsable(false)]
		public string TextColorDoubleTopSerializable
		{
			get { return Serialize.BrushToString(textColorDoubleTop); }
			set { textColorDoubleTop = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "Text Color Higher Low", Description = "Represents the color of the swing value output for higher lows.", Order = 6, GroupName = "2. Visualize Swings")]
		public Brush TextColorHigherLow
		{
			get { return textColorHigherLow; }
			set { textColorHigherLow = value; }
		}
		[Browsable(false)]
		public string TextColorHigherLowSerializable
		{
			get { return Serialize.BrushToString(textColorHigherLow); }
			set { textColorHigherLow = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "Text Color Lower Low", Description = "Represents the color of the swing value output for lower lows.", Order = 7, GroupName = "2. Visualize Swings")]
		public Brush TextColorLowerLow
		{
			get { return textColorLowerLow; }
			set { textColorLowerLow = value; }
		}
		[Browsable(false)]
		public string TextColorLowerLowSerializable
		{
			get { return Serialize.BrushToString(textColorLowerLow); }
			set { textColorLowerLow = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "Text Color Double Bottom", Description = "Represents the color of the swing value output for double bottoms.", Order = 8, GroupName = "2. Visualize Swings")]
		public Brush TextColorDoubleBottom
		{
			get { return textColorDoubleBottom; }
			set { textColorDoubleBottom = value; }
		}
		[Browsable(false)]
		public string TextColorDoubleBottomSerializable
		{
			get { return Serialize.BrushToString(textColorDoubleBottom); }
			set { textColorDoubleBottom = Serialize.StringToBrush(value); }
		}

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "Text Offset Length/Duration", Description = "Represents the text offset in pixel for the swing length/duration.", Order = 9, GroupName = "2. Visualize Swings")]
		public int TextOffsetLength
		{
			get { return textOffsetLength; }
			set { textOffsetLength = Math.Max(1, value); }
		}

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "Text Offset Volume", Description = "Represents the text offset in pixel for the swing volume.", Order = 10, GroupName = "2. Visualize Swings")]
		public int TextOffsetVolume
		{
			get { return textOffsetVolume; }
			set { textOffsetVolume = Math.Max(1, value); }
		}

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "Text Offset Price", Description = "Represents the text offset in pixel for the swing price for the swings.", Order = 11, GroupName = "2. Visualize Swings")]
		public int TextOffsetPrice
		{
			get { return textOffsetPrice; }
			set { textOffsetPrice = Math.Max(1, value); }
		}

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "Text Offset Swing Labels", Description = "Represents the text offset in pixel for the swing labels.", Order = 12, GroupName = "2. Visualize Swings")]
		public int TextOffsetLabel
		{
			get { return textOffsetLabel; }
			set { textOffsetLabel = Math.Max(1, value); }
		}

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "Text Offset Time", Description = "Represents the text offset in pixel for the time value.", Order = 13, GroupName = "2. Visualize Swings")]
		public int TextOffsetTime
		{
			get { return textOffsetTime; }
			set { textOffsetTime = Math.Max(1, value); }
		}

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "Text Offset Percent", Description = "Represents the text offset in pixel for the retracement value.", Order = 14, GroupName = "2. Visualize Swings")]
		public int TextOffsetPercent
		{
			get { return textOffsetPercent; }
			set { textOffsetPercent = Math.Max(1, value); }
		}

		[XmlIgnore]
		[Display(Name = "Zig-Zag Color Up", Description = "Represents the color of the zig-zag up lines.", Order = 15, GroupName = "2. Visualize Swings")]
		public Brush ZigZagColorUp
		{
			get { return zigZagColorUp; }
			set { zigZagColorUp = value; }
		}
		[Browsable(false)]
		public string ZigZagColorUpSerializable
		{
			get { return Serialize.BrushToString(zigZagColorUp); }
			set { zigZagColorUp = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "Zig-Zag Color Down", Description = "Represents the color of the zig-zag down lines.", Order = 16, GroupName = "2. Visualize Swings")]
		public Brush ZigZagColorDn
		{
			get { return zigZagColorDn; }
			set { zigZagColorDn = value; }
		}
		[Browsable(false)]
		public string ZigZagColorDnSerializable
		{
			get { return Serialize.BrushToString(zigZagColorDn); }
			set { zigZagColorDn = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Display(Name = "Zig-Zag Style", Description = "Represents the line style of the zig-zag lines.", Order = 17, GroupName = "2. Visualize Swings")]
		public DashStyleHelper ZigZagStyle
		{
			get { return zigZagStyle; }
			set { zigZagStyle = value; }
		}

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "Zig-Zag Width", Description = "Represents the line width of the zig-zag lines.", Order = 18, GroupName = "2. Visualize Swings")]
		public int ZigZagWidth
		{
			get { return zigZagWidth; }
			set { zigZagWidth = Math.Max(1, value); }
		}
		//=========================================================================================
		#endregion
		
		#region Gann Swings
		//=========================================================================================
		[NinjaScriptProperty]
		[Display(Name = "Ignore Inside Bars", Description = "Indicates if inside bars are ignored. If set to true it is possible that between consecutive up/down bars are inside bars. Only used if calculationSize > 1.", Order = 1, GroupName = "3. Gann Swings")]
		public bool IgnoreInsideBars
		{
			get { return ignoreInsideBars; }
			set { ignoreInsideBars = value; }
		}

		[NinjaScriptProperty]
		[Display(Name = "Use Breakouts", Description = "Indicates if the swings are updated if the last swing high/low is broken. Only used if calculationSize > 1.", Order = 2, GroupName = "3. Gann Swings")]
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

namespace PriceActionSwing.Base
{
	    #region public class SwingValues
	    //=============================================================================================
	    public class Swings
	    {
	        #region Current values
	        //-----------------------------------------------------------------------------------------
	        /// <summary>
	        /// Represents the price of the current swing.
	        /// </summary>
	        public double CurPrice { get; set; }
	        /// <summary>
	        /// Represents the bar number of the highest/lowest bar of the current swing.
	        /// </summary>
	        public int CurBar { get; set; }
	        /// <summary>
	        /// Represents the duration as time values of the current swing.
	        /// </summary>
	        public DateTime CurDateTime { get; set; }
	        /// <summary>
	        /// Represents the duration in bars of the current swing.
	        /// </summary>
	        public int CurDuration { get; set; }
	        /// <summary>
	        /// Represents the swing length in ticks of the current swing.
	        /// </summary>
	        public int CurLength { get; set; }
	        /// <summary>
	        /// Represents the percentage in relation between the last swing and the current swing. 
	        /// E. g. 61.8% fib retracement.
	        /// </summary>
	        public double CurPercent { get; set; }
	        /// <summary>
	        /// Represents the duration as integer in HHMMSS of the current swing.
	        /// </summary>
	        public int CurTime { get; set; }
	        /// <summary>
	        /// Represents the entire volume of the current swing.
	        /// </summary>
	        public long CurVolume { get; set; }
	        /// <summary>
	        /// Represents the relation to the previous swing.
	        /// -1 = Lower High | 0 = Double Top | 1 = Higher High
	        /// </summary>
	        public int CurRelation { get; set; }
	        //-----------------------------------------------------------------------------------------
	        #endregion

	        #region Last values
	        //-----------------------------------------------------------------------------------------
	        /// <summary>
	        /// Represents the price of the last swing.
	        /// </summary>
	        public double LastPrice { get; set; }
	        /// <summary>
	        /// Represents the bar number of the highest/lowest bar of the last swing.
	        /// </summary>
	        public int LastBar { get; set; }
	        /// <summary>
	        /// Represents the duration as time values of the last swing.
	        /// </summary>
	        public DateTime LastDateTime { get; set; }
	        /// <summary>
	        /// Represents the duration in bars of the last swing.
	        /// </summary>
	        public int LastDuration { get; set; }
	        /// <summary>
	        /// Represents the swing length in ticks of the last swing.
	        /// </summary>
	        public int LastLength { get; set; }
	        /// <summary>
	        /// Represents the percentage in relation between the previous swing and the last swing. 
	        /// E. g. 61.8% fib retracement.
	        /// </summary>
	        public double LastPercent { get; set; }
	        /// <summary>
	        /// Represents the duration as integer in HHMMSS of the last swing.
	        /// </summary>
	        public int LastTime { get; set; }
	        /// <summary>
	        /// Represents the entire volume of the last swing.
	        /// </summary>
	        public long LastVolume { get; set; }
	        /// <summary>
	        /// Represents the relation to the previous swing.
	        /// -1 = Lower High | 0 = Double Top | 1 = Higher High
	        /// </summary>
	        public int LastRelation { get; set; }
	        //-----------------------------------------------------------------------------------------
	        #endregion

	        #region Other values
	        //-----------------------------------------------------------------------------------------
	        /// <summary>
	        /// Represents the number of swings.
	        /// </summary>
	        public int Counter { get; set; }
	        /// <summary>
	        /// Indicates if a new swing is found.
	        /// </summary>
	        public bool New { get; set; }
	        /// <summary>
	        /// Indicates if a the current swing is updated.
	        /// </summary>
	        public bool Update { get; set; }
	        /// <summary>
	        /// Represents the volume of the signal bar for the swing.
	        /// </summary>
	        public double SignalBarVolume { get; set; }
	        /// <summary>
	        /// Represents the number of the last swing in the swing list.
	        /// </summary>
	        public int ListCount { get; set; }
	        //-----------------------------------------------------------------------------------------
	        #endregion
	    }
	    //=============================================================================================
	    #endregion

	    #region public class CurrentSwing
	    //=============================================================================================
	    public class SwingCurrent
	    {
	        /// <summary>
	        /// Represents the swing slope direction. -1 = down | 0 = init | 1 = up.
	        /// </summary>
	        public int SwingSlope { get; set; }
	        /// <summary>
	        /// Represents the bar number of the swing slope change bar.
	        /// </summary>
	        public int SwingSlopeChangeBar { get; set; }
	        /// <summary>
	        /// Indicates if a new swing is found. And whether it is a swing high or a swing low.
	        /// Used to control, that either a swing high or a swing low is set for each bar.
	        /// 0 = no swing | -1 = down swing | 1 = up swing
	        /// </summary>
	        public int NewSwing { get; set; }
	        /// <summary>
	        /// Represents the number of consecutives up/down bars.
	        /// </summary>
	        public int ConsecutiveBars { get; set; }
	        /// <summary>
	        /// Represents the bar number of the last bar which was counted to the 
	        /// consecutives up/down bars.
	        /// </summary>
	        public int ConsecutiveBarNumber { get; set; }
	        /// <summary>
	        /// Represents the high/low of the last consecutive bar.
	        /// </summary>
	        public double ConsecutiveBarValue { get; set; }
	        /// <summary>
	        /// Indicates if the outside bar calculation is stopped. Used to avoid an up swing and 
	        /// a down swing in one bar.
	        /// </summary>
	        public bool StopOutsideBarCalc { get; set; }
	    }
	    //=============================================================================================
	    #endregion

	    #region public class SwingProperties
	    //=============================================================================================
	    public class SwingProperties
	    {
	        public SwingProperties(double swingSize, int dtbStrength)
	        {
	            SwingSize = swingSize;
	            DtbStrength = dtbStrength;
	        }

	        public SwingProperties(SwingStyle swingType, double swingSize, int dtbStrength,
	            SwingLengthStyle swingLengthType, SwingDurationStyle swingDurationType, 
	            bool showSwingPrice, bool showSwingLabel, bool showSwingPercent,
	            SwingTimeStyle swingTimeType, SwingVolumeStyle swingVolumeType, 
	            VisualizationStyle visualizationType, bool useBreakouts, bool ignoreInsideBars, 
	            bool useAutoScale, Brush zigZagColorUp, Brush zigZagColorDn,
	            DashStyleHelper zigZagStyle, int zigZagWidth, Brush textColorHigherHigh,
	            Brush textColorLowerHigh, Brush textColorDoubleTop, Brush textColorHigherLow,
	            Brush textColorLowerLow, Brush textColorDoubleBottom, SimpleFont textFont, 
	            int textOffsetLength, int textOffsetPercent, int textOffsetPrice, int textOffsetLabel,
	            int textOffsetTime, int textOffsetVolume, bool useCloseValues, 
				bool drawSwingsOnPricePanel)
	        {
	            SwingType = swingType;
	            SwingSize = swingSize;
	            DtbStrength = dtbStrength;
	            SwingLengthType = swingLengthType;
	            SwingDurationType = swingDurationType;
	            ShowSwingPrice = showSwingPrice;
	            ShowSwingLabel = showSwingLabel;
	            ShowSwingPercent = showSwingPercent;
	            SwingTimeType = swingTimeType;
	            SwingVolumeType = swingVolumeType;
	            VisualizationType = visualizationType;
	            UseBreakouts = useBreakouts;
	            IgnoreInsideBars = ignoreInsideBars;
	            UseAutoScale = useAutoScale;
	            ZigZagColorUp = zigZagColorUp;
	            ZigZagColorDn = zigZagColorDn;
	            ZigZagStyle = zigZagStyle;
	            ZigZagWidth = zigZagWidth;
	            TextColorHigherHigh = textColorHigherHigh;
	            TextColorLowerHigh = textColorLowerHigh;
	            TextColorDoubleTop = textColorDoubleTop;
	            TextColorHigherLow = textColorHigherLow;
	            TextColorLowerLow = textColorLowerLow;
	            TextColorDoubleBottom = textColorDoubleBottom;
	            TextFont = textFont;
	            TextOffsetLength = textOffsetLength;
	            TextOffsetPercent = textOffsetPercent;
	            TextOffsetPrice = textOffsetPrice;
	            TextOffsetLabel = textOffsetLabel;
	            TextOffsetTime = textOffsetTime;
	            TextOffsetVolume = textOffsetVolume;
	            UseCloseValues = useCloseValues;
				DrawSwingsOnPricePanel = drawSwingsOnPricePanel;
	        }

	        /// <summary>
	        /// Represents the swing type.
	        /// </summary>
	        public SwingStyle SwingType { get; set; }
	        /// <summary>
	        /// Represents the swing size. e.g. 1 = small swings and 5 = bigger swings.
	        /// </summary>
	        public double SwingSize { get; set; }
	        /// <summary>
	        /// Represents the double top and double bottom strength.
	        /// </summary>
	        public int DtbStrength { get; set; }
	        /// <summary>
	        /// Represents the swing length visualization type.
	        /// </summary>
	        public SwingLengthStyle SwingLengthType { get; set; }
	        /// <summary>
	        /// Represents the swing duration visualization type.
	        /// </summary>
	        public SwingDurationStyle SwingDurationType { get; set; }
	        /// <summary>
	        /// Indicates if the swing price is shown.
	        /// </summary>
	        public bool ShowSwingPrice { get; set; }
	        /// <summary>
	        /// Indicates if the swing label is shown.
	        /// </summary>
	        public bool ShowSwingLabel { get; set; }
	        /// <summary>
	        /// Indicates if the swing percentage in relation to the last swing is shown.
	        /// </summary>
	        public bool ShowSwingPercent { get; set; }
	        /// <summary>
	        /// Represents the swing time visualization type.
	        /// </summary>
	        public SwingTimeStyle SwingTimeType { get; set; }
	        /// <summary>
	        /// Represents the swing volume visualization type.
	        /// </summary>
	        public SwingVolumeStyle SwingVolumeType { get; set; }
	        /// <summary>
	        /// Represents the swing visualization type. 
	        /// </summary>
	        public VisualizationStyle VisualizationType { get; set; }
	        /// <summary>
	        /// Indicates if the Gann swings are updated if the last swing high/low is broken.
	        /// </summary>
	        public bool UseBreakouts { get; set; }
	        /// <summary>
	        /// Indicates if inside bars are ignored for the Gann swing calculation. If set to true 
	        /// it is possible that between consecutive up/down bars are inside bars.
	        /// </summary>
	        public bool IgnoreInsideBars { get; set; }
	        /// <summary>
	        /// Indicates if AutoScale is used. 
	        /// </summary>
	        public bool UseAutoScale { get; set; }
	        /// <summary>
	        /// Represents the colour of the zig-zag up lines.
	        /// </summary>
	        public Brush ZigZagColorUp { get; set; }
	        /// <summary>
	        /// Represents the colour of the zig-zag down lines.
	        /// </summary>
	        public Brush ZigZagColorDn { get; set; }
	        /// <summary>
	        /// Represents the line style of the zig-zag lines.
	        /// </summary>
	        public DashStyleHelper ZigZagStyle { get; set; }
	        /// <summary>
	        /// Represents the line width of the zig-zag lines.
	        /// </summary>
	        public int ZigZagWidth { get; set; }
	        /// <summary>
	        /// Represents the colour of the swing value output for higher highs.
	        /// </summary>
	        public Brush TextColorHigherHigh { get; set; }
	        /// <summary>
	        /// Represents the colour of the swing value output for lower highs.
	        /// </summary>
	        public Brush TextColorLowerHigh { get; set; }
	        /// <summary>
	        /// Represents the colour of the swing value output for double tops.
	        /// </summary>
	        public Brush TextColorDoubleTop { get; set; }
	        /// <summary>
	        /// Represents the colour of the swing value output for higher lows.
	        /// </summary>
	        public Brush TextColorHigherLow { get; set; }
	        /// <summary>
	        /// Represents the colour of the swing value output for lower lows.
	        /// </summary>
	        public Brush TextColorLowerLow { get; set; }
	        /// <summary>
	        /// Represents the colour of the swing value output for double bottems.
	        /// </summary>
	        public Brush TextColorDoubleBottom { get; set; }
	        /// <summary>
	        /// Represents the text font for the swing value output.
	        /// </summary>
	        public SimpleFont TextFont { get; set; }
	        /// <summary>
	        /// Represents the text offset in pixel for the swing length.
	        /// </summary>
	        public int TextOffsetLength { get; set; }
	        /// <summary>
	        /// Represents the text offset in pixel for the retracement value.
	        /// </summary>
	        public int TextOffsetPercent { get; set; }
	        /// <summary>
	        /// Represents the text offset in pixel for the swing price.
	        /// </summary>
	        public int TextOffsetPrice { get; set; }
	        /// <summary>
	        /// Represents the text offset in pixel for the swing labels.
	        /// </summary>
	        public int TextOffsetLabel { get; set; }
	        /// <summary>
	        /// Represents the text offset in pixel for the swing time.
	        /// </summary>
	        public int TextOffsetTime { get; set; }
	        /// <summary>
	        /// Represents the text offset in pixel for the swing volume.
	        /// </summary>
	        public int TextOffsetVolume { get; set; }
	        /// <summary>
	        /// Indicates if high and low prices are used for the swing calculations or close values.
	        /// </summary>
	        public bool UseCloseValues { get; set; }
	        /// <summary>
	        /// Indicates if the swings are drawn on the price panel.
	        /// </summary>
	        public bool DrawSwingsOnPricePanel { get; set; }
	    }
	    //=============================================================================================
	    #endregion

	    #region public struct SwingStruct
	    //=============================================================================================
	    public struct SwingStruct
	    {
	        /// <summary>
	        /// Swing price.
	        /// </summary>
	        public double price;
	        /// <summary>
	        /// Swing bar number.
	        /// </summary>
	        public int barNumber;
	        /// <summary>
	        /// Swing time.
	        /// </summary>
	        public DateTime time;
	        /// <summary>
	        /// Swing duration in bars.
	        /// </summary>
	        public int duration;
	        /// <summary>
	        /// Swing length in ticks.
	        /// </summary>
	        public int length;
	        /// <summary>
	        /// Swing relation.
	        /// -1 = Lower | 0 = Double | 1 = Higher
	        /// </summary>
	        public int relation;
	        /// <summary>
	        /// Swing volume.
	        /// </summary>
	        public long volume;

	        public SwingStruct(double swingPrice, int swingBarNumber, DateTime swingTime,
	                int swingDuration, int swingLength, int swingRelation, long swingVolume)
	        {
	            price = swingPrice;
	            barNumber = swingBarNumber;
	            time = swingTime;
	            duration = swingDuration;
	            length = swingLength;
	            relation = swingRelation;
	            volume = swingVolume;
	        }
	    }
	    //=============================================================================================
	    #endregion

	    #region Enums
	    //=============================================================================================
	    public enum VisualizationStyle
	    {
	        False,
	        Dots,
	        Dots_ZigZag,
	        ZigZag,
	        ZigZagVolume,
	        GannStyle,
	    }

	    public enum SwingStyle
	    {
	        Standard,
	        Gann,
	        Ticks,
	        Percent,
	    }

	    public enum SwingLengthStyle
	    {
	        False,
	        Ticks,
	        Ticks_Price,
	        Price_Ticks,
	        Points,
	        Points_Price,
	        Price_Points,
	        Price,
	        Percent,
	    }

	    public enum SwingDurationStyle
	    {
	        False,
	        Bars,
	        MMSS,
	        HHMM,
	        SecondsTotal,
	        MinutesTotal,
	        HoursTotal,
	        Days,
	    }

	    public enum SwingTimeStyle
	    {
	        False,
	        Integer,
	        HHMM,
	        HHMMSS,
	        DDMM,
	    }
	    
	    public enum SwingVolumeStyle
	    {
	        False,
	        Absolute,
	        Relative,
	    }

	    public enum AbcPatternMode
	    {
	        False,
	        Long_Short,
	        Long,
	        Short,
	    }

	    public enum StatisticPositionStyle
	    {
	        False,
	        Bottom,
	        Top,
	    }

	    public enum RiskManagementStyle
	    {
	        False,
	        ToolStrip,
	        Tab,
	    }

	    public enum DivergenceMode
	    {
	        Custom,
	        False,
	        GomCD,
	        MACD,
	        Stochastics,
	    }
	    public enum DivergenceDirection
	    {
	        Long,
	        Long_Short,
	        Short
	    }
	    public enum Show
	    {
	        Trend,
	        Relation,
	        Volume,
	    }
	    //=============================================================================================
	    #endregion
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private PriceActionSwing.PriceActionSwing[] cachePriceActionSwing;
		public PriceActionSwing.PriceActionSwing PriceActionSwing(SwingStyle swingType, double swingSize, int dtbStrength, bool useCloseValues, SwingLengthStyle swingLengthType, SwingDurationStyle swingDurationType, bool showSwingPrice, bool showSwingLabel, bool showSwingPercent, SwingTimeStyle swingTimeType, SwingVolumeStyle swingVolumeType, VisualizationStyle visualizationType, NinjaTrader.Gui.Tools.SimpleFont textFont, int textOffsetLength, int textOffsetVolume, int textOffsetPrice, int textOffsetLabel, int textOffsetTime, int textOffsetPercent, DashStyleHelper zigZagStyle, int zigZagWidth, bool ignoreInsideBars, bool useBreakouts)
		{
			return PriceActionSwing(Input, swingType, swingSize, dtbStrength, useCloseValues, swingLengthType, swingDurationType, showSwingPrice, showSwingLabel, showSwingPercent, swingTimeType, swingVolumeType, visualizationType, textFont, textOffsetLength, textOffsetVolume, textOffsetPrice, textOffsetLabel, textOffsetTime, textOffsetPercent, zigZagStyle, zigZagWidth, ignoreInsideBars, useBreakouts);
		}

		public PriceActionSwing.PriceActionSwing PriceActionSwing(ISeries<double> input, SwingStyle swingType, double swingSize, int dtbStrength, bool useCloseValues, SwingLengthStyle swingLengthType, SwingDurationStyle swingDurationType, bool showSwingPrice, bool showSwingLabel, bool showSwingPercent, SwingTimeStyle swingTimeType, SwingVolumeStyle swingVolumeType, VisualizationStyle visualizationType, NinjaTrader.Gui.Tools.SimpleFont textFont, int textOffsetLength, int textOffsetVolume, int textOffsetPrice, int textOffsetLabel, int textOffsetTime, int textOffsetPercent, DashStyleHelper zigZagStyle, int zigZagWidth, bool ignoreInsideBars, bool useBreakouts)
		{
			if (cachePriceActionSwing != null)
				for (int idx = 0; idx < cachePriceActionSwing.Length; idx++)
					if (cachePriceActionSwing[idx] != null && cachePriceActionSwing[idx].SwingType == swingType && cachePriceActionSwing[idx].SwingSize == swingSize && cachePriceActionSwing[idx].DtbStrength == dtbStrength && cachePriceActionSwing[idx].UseCloseValues == useCloseValues && cachePriceActionSwing[idx].SwingLengthType == swingLengthType && cachePriceActionSwing[idx].SwingDurationType == swingDurationType && cachePriceActionSwing[idx].ShowSwingPrice == showSwingPrice && cachePriceActionSwing[idx].ShowSwingLabel == showSwingLabel && cachePriceActionSwing[idx].ShowSwingPercent == showSwingPercent && cachePriceActionSwing[idx].SwingTimeType == swingTimeType && cachePriceActionSwing[idx].SwingVolumeType == swingVolumeType && cachePriceActionSwing[idx].VisualizationType == visualizationType && cachePriceActionSwing[idx].TextFont == textFont && cachePriceActionSwing[idx].TextOffsetLength == textOffsetLength && cachePriceActionSwing[idx].TextOffsetVolume == textOffsetVolume && cachePriceActionSwing[idx].TextOffsetPrice == textOffsetPrice && cachePriceActionSwing[idx].TextOffsetLabel == textOffsetLabel && cachePriceActionSwing[idx].TextOffsetTime == textOffsetTime && cachePriceActionSwing[idx].TextOffsetPercent == textOffsetPercent && cachePriceActionSwing[idx].ZigZagStyle == zigZagStyle && cachePriceActionSwing[idx].ZigZagWidth == zigZagWidth && cachePriceActionSwing[idx].IgnoreInsideBars == ignoreInsideBars && cachePriceActionSwing[idx].UseBreakouts == useBreakouts && cachePriceActionSwing[idx].EqualsInput(input))
						return cachePriceActionSwing[idx];
			return CacheIndicator<PriceActionSwing.PriceActionSwing>(new PriceActionSwing.PriceActionSwing(){ SwingType = swingType, SwingSize = swingSize, DtbStrength = dtbStrength, UseCloseValues = useCloseValues, SwingLengthType = swingLengthType, SwingDurationType = swingDurationType, ShowSwingPrice = showSwingPrice, ShowSwingLabel = showSwingLabel, ShowSwingPercent = showSwingPercent, SwingTimeType = swingTimeType, SwingVolumeType = swingVolumeType, VisualizationType = visualizationType, TextFont = textFont, TextOffsetLength = textOffsetLength, TextOffsetVolume = textOffsetVolume, TextOffsetPrice = textOffsetPrice, TextOffsetLabel = textOffsetLabel, TextOffsetTime = textOffsetTime, TextOffsetPercent = textOffsetPercent, ZigZagStyle = zigZagStyle, ZigZagWidth = zigZagWidth, IgnoreInsideBars = ignoreInsideBars, UseBreakouts = useBreakouts }, input, ref cachePriceActionSwing);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.PriceActionSwing.PriceActionSwing PriceActionSwing(SwingStyle swingType, double swingSize, int dtbStrength, bool useCloseValues, SwingLengthStyle swingLengthType, SwingDurationStyle swingDurationType, bool showSwingPrice, bool showSwingLabel, bool showSwingPercent, SwingTimeStyle swingTimeType, SwingVolumeStyle swingVolumeType, VisualizationStyle visualizationType, NinjaTrader.Gui.Tools.SimpleFont textFont, int textOffsetLength, int textOffsetVolume, int textOffsetPrice, int textOffsetLabel, int textOffsetTime, int textOffsetPercent, DashStyleHelper zigZagStyle, int zigZagWidth, bool ignoreInsideBars, bool useBreakouts)
		{
			return indicator.PriceActionSwing(Input, swingType, swingSize, dtbStrength, useCloseValues, swingLengthType, swingDurationType, showSwingPrice, showSwingLabel, showSwingPercent, swingTimeType, swingVolumeType, visualizationType, textFont, textOffsetLength, textOffsetVolume, textOffsetPrice, textOffsetLabel, textOffsetTime, textOffsetPercent, zigZagStyle, zigZagWidth, ignoreInsideBars, useBreakouts);
		}

		public Indicators.PriceActionSwing.PriceActionSwing PriceActionSwing(ISeries<double> input , SwingStyle swingType, double swingSize, int dtbStrength, bool useCloseValues, SwingLengthStyle swingLengthType, SwingDurationStyle swingDurationType, bool showSwingPrice, bool showSwingLabel, bool showSwingPercent, SwingTimeStyle swingTimeType, SwingVolumeStyle swingVolumeType, VisualizationStyle visualizationType, NinjaTrader.Gui.Tools.SimpleFont textFont, int textOffsetLength, int textOffsetVolume, int textOffsetPrice, int textOffsetLabel, int textOffsetTime, int textOffsetPercent, DashStyleHelper zigZagStyle, int zigZagWidth, bool ignoreInsideBars, bool useBreakouts)
		{
			return indicator.PriceActionSwing(input, swingType, swingSize, dtbStrength, useCloseValues, swingLengthType, swingDurationType, showSwingPrice, showSwingLabel, showSwingPercent, swingTimeType, swingVolumeType, visualizationType, textFont, textOffsetLength, textOffsetVolume, textOffsetPrice, textOffsetLabel, textOffsetTime, textOffsetPercent, zigZagStyle, zigZagWidth, ignoreInsideBars, useBreakouts);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.PriceActionSwing.PriceActionSwing PriceActionSwing(SwingStyle swingType, double swingSize, int dtbStrength, bool useCloseValues, SwingLengthStyle swingLengthType, SwingDurationStyle swingDurationType, bool showSwingPrice, bool showSwingLabel, bool showSwingPercent, SwingTimeStyle swingTimeType, SwingVolumeStyle swingVolumeType, VisualizationStyle visualizationType, NinjaTrader.Gui.Tools.SimpleFont textFont, int textOffsetLength, int textOffsetVolume, int textOffsetPrice, int textOffsetLabel, int textOffsetTime, int textOffsetPercent, DashStyleHelper zigZagStyle, int zigZagWidth, bool ignoreInsideBars, bool useBreakouts)
		{
			return indicator.PriceActionSwing(Input, swingType, swingSize, dtbStrength, useCloseValues, swingLengthType, swingDurationType, showSwingPrice, showSwingLabel, showSwingPercent, swingTimeType, swingVolumeType, visualizationType, textFont, textOffsetLength, textOffsetVolume, textOffsetPrice, textOffsetLabel, textOffsetTime, textOffsetPercent, zigZagStyle, zigZagWidth, ignoreInsideBars, useBreakouts);
		}

		public Indicators.PriceActionSwing.PriceActionSwing PriceActionSwing(ISeries<double> input , SwingStyle swingType, double swingSize, int dtbStrength, bool useCloseValues, SwingLengthStyle swingLengthType, SwingDurationStyle swingDurationType, bool showSwingPrice, bool showSwingLabel, bool showSwingPercent, SwingTimeStyle swingTimeType, SwingVolumeStyle swingVolumeType, VisualizationStyle visualizationType, NinjaTrader.Gui.Tools.SimpleFont textFont, int textOffsetLength, int textOffsetVolume, int textOffsetPrice, int textOffsetLabel, int textOffsetTime, int textOffsetPercent, DashStyleHelper zigZagStyle, int zigZagWidth, bool ignoreInsideBars, bool useBreakouts)
		{
			return indicator.PriceActionSwing(input, swingType, swingSize, dtbStrength, useCloseValues, swingLengthType, swingDurationType, showSwingPrice, showSwingLabel, showSwingPercent, swingTimeType, swingVolumeType, visualizationType, textFont, textOffsetLength, textOffsetVolume, textOffsetPrice, textOffsetLabel, textOffsetTime, textOffsetPercent, zigZagStyle, zigZagWidth, ignoreInsideBars, useBreakouts);
		}
	}
}

#endregion
