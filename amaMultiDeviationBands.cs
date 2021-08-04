//+----------------------------------------------------------------------------------------------+
//| Copyright © <2021>  <LizardIndicators.com - powered by AlderLab UG>
//
//| This program is free software: you can redistribute it and/or modify
//| it under the terms of the GNU General Public License as published by
//| the Free Software Foundation, either version 3 of the License, or
//| any later version.
//|
//| This program is distributed in the hope that it will be useful,
//| but WITHOUT ANY WARRANTY; without even the implied warranty of
//| MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//| GNU General Public License for more details.
//|
//| By installing this software you confirm acceptance of the GNU
//| General Public License terms. You may find a copy of the license
//| here; http://www.gnu.org/licenses/
//+----------------------------------------------------------------------------------------------+

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
using SharpDX;
using SharpDX.Direct2D1;
#endregion

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.LizardIndicators
{
	/// <summary>
	/// The Multi Deviation Bands indicator plots an envelope around a selected moving average. The width of the envelope is calculated as a multiple of the standard deviation, 
	/// the mean absolute deviation or the median absolute deviation. All deviations are calculated around the current value of the selected moving average. 
	/// 
	[Gui.CategoryOrder("Algorithmic Options", 1000100)]
	[Gui.CategoryOrder("Input Parameters", 1000200)]
	[Gui.CategoryOrder("Display Options", 1000300)]
	[Gui.CategoryOrder("Channel Fill", 8000100)]
	[Gui.CategoryOrder("Paint Bars", 8000200)]
	[Gui.CategoryOrder("Sound Alerts", 8000300)]
	[Gui.CategoryOrder("Version", 8000400)]
	public class amaMultiDeviationBands : Indicator
	{
		private int 								basePeriod					= 20;
		private int 								devPeriod					= 20;
		private int 								threshold					= 50;
		private int									displacement				= 0;
		private int									totalBarsRequiredToPlot		= 0;
		private int									lookback					= 0;
		private int 								mIndex						= 0;
		private	double 								numDev						= 2.5;
		private double								avg							= 0.0;
		private double								sumSquares					= 0.0;
		private double								dist						= 0.0;
		private double								stdDev						= 0.0;
		private double								sumAbsDev					= 0.0;
		private double								meanAbsDev					= 0.0;
		private double								medianAbsDev				= 0.0;
		private double								smoothedAvg					= 0.0;
		private double								offset						= 0.0;
		private double 								middle						= 0.0;
		private double 								upper						= 0.0;
		private double 								lower						= 0.0;
		private double 								neutralSlope				= 0.0;
		private List<double> 						dList 						= new List<double>();
		private bool 								smoothed					= false;
		private bool 								showMidband					= true;
		private bool 								showChannel					= true;
		private bool								channelFlooding				= true;
		private bool 								showPaintBars				= true;
		private bool 								soundAlerts					= false;
		private bool								channelIsFlooded			= false;
		private bool								calculateFromPriceData		= true;
		private bool								indicatorIsOnPricePanel		= true;
		private bool								midbandTypeNotAllowed		= false;
		private bool								even						= true;
		private amaMultiDevBandsMAType				midbandMAType				= amaMultiDevBandsMAType.SMA; 
		private amaMultiDevBandsOffsetType		 	offsetType					= amaMultiDevBandsOffsetType.Mean_Abs_Dev;	
		private SessionIterator						sessionIterator				= null;
		private int 								channelOpacity				= 50;
		private int 								plot0Width 					= 1;
		private PlotStyle 							plot0Style					= PlotStyle.Line;
		private DashStyleHelper						dash0Style					= DashStyleHelper.Solid;
		private int 								plot1Width 					= 1;
		private PlotStyle 							plot1Style					= PlotStyle.Line;
		private DashStyleHelper						dash1Style					= DashStyleHelper.Solid;
		private System.Windows.Media.Brush			upBrush						= Brushes.DarkGreen;
		private System.Windows.Media.Brush			downBrush					= Brushes.DarkRed;
		private System.Windows.Media.Brush			neutralBrush				= Brushes.Black;
		private System.Windows.Media.Brush			cloudBrushUpS				= Brushes.LightGreen;
		private System.Windows.Media.Brush			cloudBrushDownS				= Brushes.Tomato;
		private System.Windows.Media.Brush			cloudBrushNeutralS			= Brushes.Gainsboro;
		private System.Windows.Media.Brush			cloudBrushUp				= Brushes.Transparent;
		private System.Windows.Media.Brush			cloudBrushDown				= Brushes.Transparent;
		private System.Windows.Media.Brush			cloudBrushNeutral			= Brushes.Transparent;
		private System.Windows.Media.Brush			upBrushUp					= Brushes.Lime;
		private System.Windows.Media.Brush			upBrushDown					= Brushes.ForestGreen;
		private System.Windows.Media.Brush			downBrushUp					= Brushes.DarkRed;
		private System.Windows.Media.Brush			downBrushDown				= Brushes.Red;
		private System.Windows.Media.Brush			neutralBrushUp				= Brushes.DimGray;
		private System.Windows.Media.Brush			neutralBrushDown			= Brushes.WhiteSmoke;
		private System.Windows.Media.Brush			upBrushOutline				= Brushes.Black;
		private System.Windows.Media.Brush			downBrushOutline			= Brushes.Black;
		private System.Windows.Media.Brush			neutralBrushOutline			= Brushes.Black;
		private System.Windows.Media.Brush			priorTrendBrush				= null;
		private System.Windows.Media.Brush			transparentBrush			= Brushes.Transparent;
		private System.Windows.Media.Brush			alertBackBrush				= Brushes.Black;
		private System.Windows.Media.Brush			errorBrush					= Brushes.Black;
		private SimpleFont							errorFont;
		private int									rearmTime					= 30;
		private string								errorText1					= "Please select a different moving average, when an indicator is used as input series for the amaMultiDeviationBands.";
		private string 								newUptrend					= "upslope.wav";
		private string 								newDowntrend				= "downslope.wav";
		private string								newNeutraltrend				= "neutralslope.wav";
		private string								pathNewUptrend				= "";
		private string								pathNewDowntrend			= "";
		private string								pathNewNeutraltrend			= "";
		private string								versionString				= "v 2.2  -  February 26, 2021";
		private Series<double>						volatility;
		private Series<double>						trend;
		private Series<System.Windows.Media.Brush>	trendBrush;
		private ISeries<double>						average;
		private ISeries<double>						smoothedAverage;
		private ISeries<double>						smoothedVolatility;
		private amaATR								averageTrueRange;
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\nThe Multi Deviation Bands indicator plots an envelope around a selected moving average. The width of the envelope is calculated"
												+ " as a multiple of the standard deviation, the mean absolute deviation or the median absolute deviation. "
												+ " All deviations are calculated around the current value of the selected moving average.";
				Name						= "amaMultiDeviationBands";
				IsSuspendedWhileInactive	= false;
				IsOverlay					= true;
				ArePlotsConfigurable		= false;
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "MidBand");	
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "Upper Band");
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "Lower Band");
				SetZOrder(-2);
			}
			else if (State == State.Configure)
			{
				if(Calculate == Calculate.OnEachTick)
					Calculate = Calculate.OnPriceChange;
				displacement = Displacement;
				BarsRequiredToPlot = Math.Max(3*basePeriod, -displacement);
				totalBarsRequiredToPlot = Math.Max(BarsRequiredToPlot, BarsRequiredToPlot + displacement);
				Plots[0].PlotStyle = plot0Style;
				Plots[0].DashStyleHelper = dash0Style;
				Plots[0].Width = plot0Width;
				Plots[1].PlotStyle = plot1Style;
				Plots[1].DashStyleHelper = dash1Style;
				Plots[1].Width = plot1Width;
				Plots[2].PlotStyle = plot1Style;
				Plots[2].DashStyleHelper = dash1Style;
				Plots[2].Width = plot1Width;
				cloudBrushUp = cloudBrushUpS.Clone();
				cloudBrushUp.Opacity = (float) channelOpacity/100.0;
				cloudBrushUp.Freeze();
				cloudBrushDown = cloudBrushDownS.Clone();
				cloudBrushDown.Opacity = (float) channelOpacity/100.0;
				cloudBrushDown.Freeze();
				cloudBrushNeutral = cloudBrushNeutralS.Clone();
				cloudBrushNeutral.Opacity = (float) channelOpacity/100.0;
				cloudBrushNeutral.Freeze();
			}
			else if (State == State.DataLoaded)
			{	
				volatility = new Series<double>(this, MaximumBarsLookBack.Infinite);
				trend = new Series<double>(this, MaximumBarsLookBack.Infinite);
				trendBrush = new Series<System.Windows.Media.Brush>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				averageTrueRange = amaATR(Inputs[0], amaATRCalcMode.Wilder, 256);				
				midbandTypeNotAllowed = false;
				if(Input is PriceSeries)
					calculateFromPriceData = true;
				else
					calculateFromPriceData = false;
				switch (midbandMAType)
				{
					case amaMultiDevBandsMAType.Adaptive_Laguerre: 
						average = amaAdaptiveLaguerreFilter(Inputs[0], basePeriod);
						break;
					case amaMultiDevBandsMAType.ADXVMA: 
						average = amaADXVMA(Inputs[0], basePeriod);
						break;
					case amaMultiDevBandsMAType.Butterworth_2: 
						average = amaButterworthFilter(Inputs[0], 2, basePeriod);
						break;
					case amaMultiDevBandsMAType.Butterworth_3: 
						average = amaButterworthFilter(Inputs[0], 3, basePeriod);
						break;
					case amaMultiDevBandsMAType.DEMA: 
						average = DEMA(Inputs[0], basePeriod);
						break;
					case amaMultiDevBandsMAType.Distant_Coefficient_Filter: 
						average = amaDistantCoefficientFilter(Inputs[0], basePeriod);
						break;
					case amaMultiDevBandsMAType.DWMA: 
						average = amaDWMA(Inputs[0], basePeriod);
						break;
					case amaMultiDevBandsMAType.EHMA: 
						average = amaEHMA(Inputs[0], basePeriod);
						break;
					case amaMultiDevBandsMAType.EMA: 
						average = EMA(Inputs[0], basePeriod);
						break;
					case amaMultiDevBandsMAType.Gauss_2: 
						average = amaGaussianFilter(Inputs[0], 2, basePeriod);
						break;
					case amaMultiDevBandsMAType.Gauss_3: 
						average = amaGaussianFilter(Inputs[0], 3, basePeriod);
						break;
					case amaMultiDevBandsMAType.Gauss_4: 
						average = amaGaussianFilter(Inputs[0], 4, basePeriod);
						break;
					case amaMultiDevBandsMAType.HMA: 
						if(basePeriod > 1)
							average = HMA(Inputs[0], basePeriod);
						else
							average = Inputs[0];
						break;
					case amaMultiDevBandsMAType.HoltEMA: 
						average = amaHoltEMA(Inputs[0], basePeriod, basePeriod);
						break;
					case amaMultiDevBandsMAType.Laguerre: 
						average = amaLaguerreFilter(Inputs[0], basePeriod);
						break;
					case amaMultiDevBandsMAType.LinReg: 
						if(basePeriod > 1)
							average = LinReg(Inputs[0], basePeriod);
						else
							average = Inputs[0];
						break;
					case amaMultiDevBandsMAType.Mean_TPO: 
						if(calculateFromPriceData)
							average = amaMovingMeanTPO(Inputs[0], basePeriod);
						else
							midbandTypeNotAllowed = true;
						break;
					case amaMultiDevBandsMAType.Median: 
						average = amaMovingMedian(Inputs[0], basePeriod);
						break;
					case amaMultiDevBandsMAType.Median_TPO:
						if(calculateFromPriceData)
							average = amaMovingMedianTPO(Inputs[0], basePeriod, true);
						else
							midbandTypeNotAllowed = true;
						break;
					case amaMultiDevBandsMAType.RWMA: 
						if(calculateFromPriceData)
							average = amaRWMA(Inputs[0], basePeriod);
						else
							midbandTypeNotAllowed = true;
						break;
					case amaMultiDevBandsMAType.SMA: 
						average = SMA(Inputs[0], basePeriod);
						break;
					case amaMultiDevBandsMAType.SuperSmoother_2: 
						average = amaSuperSmoother(Inputs[0], 2, basePeriod);
						break;
					case amaMultiDevBandsMAType.SuperSmoother_3: 
						average = amaSuperSmoother(Inputs[0], 3, basePeriod);
						break;
					case amaMultiDevBandsMAType.SWMA: 
						average = amaSWMA(Inputs[0], basePeriod);
						break;
					case amaMultiDevBandsMAType.TEMA: 
						average = TEMA(Inputs[0], basePeriod);
						break;
					case amaMultiDevBandsMAType.Tillson_T3: 
						average = amaTillsonT3(Inputs[0], amaT3CalcMode.Tillson, basePeriod, 0.7);
						break;
					case amaMultiDevBandsMAType.TMA: 
						average = TMA(Inputs[0], basePeriod);
						break;
					case amaMultiDevBandsMAType.TWMA: 
						average = amaTWMA(Inputs[0], basePeriod);
						break;
					case amaMultiDevBandsMAType.Wilder: 
						average = EMA(Inputs[0], 2*basePeriod - 1);
						break;
					case amaMultiDevBandsMAType.WMA: 
						average = WMA(Inputs[0], basePeriod);
						break;
					case amaMultiDevBandsMAType.ZerolagHATEMA: 
						if(calculateFromPriceData)
							average = amaZerolagHATEMA(Inputs[0], basePeriod);
						else
							midbandTypeNotAllowed = true;
						break;
					case amaMultiDevBandsMAType.ZerolagTEMA: 
						average = amaZerolagTEMA(Inputs[0], basePeriod);
						break;
					case amaMultiDevBandsMAType.ZLEMA: 
						if(basePeriod > 1)
							average = ZLEMA(Inputs[0], basePeriod);
						else
							average = Inputs[0];
						break;
				}
				if(smoothed)
				{	
					smoothedAverage = SMA(average,3);
					smoothedVolatility = SMA(volatility,3);
				}	
				else
				{	
					smoothedAverage = average;
					smoothedVolatility = volatility;	
				}					
		    	sessionIterator = new SessionIterator(Bars);
			}
			else if (State == State.Historical)
			{
				if (devPeriod % 2 == 0)
				{
					even = true;
					mIndex = devPeriod/2;
				}
				else
				{
					even = false;
					mIndex = (devPeriod - 1)/2;
				}			
				channelIsFlooded = (channelFlooding && channelOpacity > 0);
				cloudBrushUp = cloudBrushUpS.Clone();
				cloudBrushUp.Opacity = (float) channelOpacity/100.0;
				cloudBrushUp.Freeze();
				cloudBrushDown = cloudBrushDownS.Clone();
				cloudBrushDown.Opacity = (float) channelOpacity/100.0;
				cloudBrushDown.Freeze();
				cloudBrushNeutral = cloudBrushNeutralS.Clone();
				cloudBrushNeutral.Opacity = (float) channelOpacity/100.0;
				cloudBrushNeutral.Freeze();
				DrawOnPricePanel = false;
				if(ChartBars != null)
				{	
					errorBrush = ChartControl.Properties.AxisPen.Brush;
					errorBrush.Freeze();
					errorFont = new SimpleFont("Arial", 24);
					indicatorIsOnPricePanel = (ChartPanel.PanelIndex == ChartBars.Panel);
				}	
				else
					indicatorIsOnPricePanel = false;
				pathNewUptrend = string.Format( @"{0}sounds\{1}", NinjaTrader.Core.Globals.InstallDir, newUptrend);
				pathNewDowntrend = string.Format( @"{0}sounds\{1}", NinjaTrader.Core.Globals.InstallDir, newDowntrend);
				pathNewNeutraltrend = string.Format( @"{0}sounds\{1}", NinjaTrader.Core.Globals.InstallDir, newNeutraltrend);
			}	
		}

		protected override void OnBarUpdate()
		{
			if(midbandTypeNotAllowed)
			{
				DrawOnPricePanel = false;
				Draw.TextFixed(this, "error text1", errorText1, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);  
				return;
			}
			
			if(offsetType == amaMultiDevBandsOffsetType.RMS_Dev)
			{	
				if(CurrentBar < devPeriod)
				{
					avg = average[0];
					sumSquares = 0;
					for (int i = 0; i <= CurrentBar; i++)
					{	
						dist = Input[i] - avg;
						sumSquares = sumSquares + dist * dist;
					}
					if(sumSquares > 0)
						stdDev = Math.Sqrt(sumSquares/(CurrentBar + 1));
					else
						stdDev = 0.0;
				}
				else
				{
					avg = average[0];
					sumSquares = 0;
					for (int i = 0; i < devPeriod; i++)
					{	
						dist = Input[i] - avg;
						sumSquares = sumSquares + dist * dist;
					}	
					if(sumSquares > 0)
						stdDev = Math.Sqrt(sumSquares/devPeriod);
					else
						stdDev = 0.0;
				}
				volatility[0] = stdDev;					
			}
			else if(offsetType == amaMultiDevBandsOffsetType.Mean_Abs_Dev)
			{	
				if(CurrentBar < devPeriod)
				{
					lookback = CurrentBar + 1;
					avg = average[0];
					sumAbsDev = 0;
					for (int i = 0; i < lookback; i++)
						sumAbsDev = sumAbsDev + Math.Abs(Input[i] - avg);
					meanAbsDev = sumAbsDev/lookback;
				}
				else
				{
					avg = average[0];
					sumAbsDev = 0;
					for (int i = 0; i < devPeriod; i++)
						sumAbsDev = sumAbsDev + Math.Abs(Input[i] - avg);
					meanAbsDev = sumAbsDev/devPeriod;
				}
				volatility[0] = meanAbsDev;			
			}
			else if(offsetType == amaMultiDevBandsOffsetType.Median_Abs_Dev)
			{	
				if(CurrentBar < devPeriod)
				{
					if(IsFirstTickOfBar)
					{
						dList.Add(0.0);	
						lookback = CurrentBar + 1;
					}
					avg = average[0];
					for (int i = 0; i < lookback; i++)
						dList[i] = Math.Abs(Input[i] - avg);
					dList.Sort();
					if (lookback % 2 == 0)
						medianAbsDev = 0.5 * (dList[lookback/2] + dList[lookback/2 - 1]);
					else
						medianAbsDev = dList[(lookback - 1)/2];
				}	
				else
				{
					avg = average[0];
					for (int i = 0; i < devPeriod; i++)
						dList[i] = Math.Abs(Input[i] - avg);
					dList.Sort();
					if(even)
						medianAbsDev = 0.5*(dList[mIndex] + dList[mIndex-1]);
					else
						medianAbsDev = dList[mIndex];
				}
				volatility[0] = medianAbsDev;			
			}	
			
			smoothedAvg = smoothedAverage[0];
			offset = numDev * smoothedVolatility[0];
			Midband[0] = smoothedAvg;
			Upper[0] = smoothedAvg + offset;
			Lower[0] = smoothedAvg - offset;

			if (CurrentBar < BarsRequiredToPlot)
			{	
				trend[0] = 0.0;
				return;
			}
			
			if(IsFirstTickOfBar)
			{
           		neutralSlope =  threshold * averageTrueRange[1] / 1000;
				middle = smoothedAverage[1] + smoothedAverage[2];
				upper = Upper[1] + Upper[2];
				lower = Lower[1] + Lower[2];
				priorTrendBrush = trendBrush[1];
			}	
			if(2*smoothedAvg > middle + 3*neutralSlope)
			{
				trend[0] = 1.0;
				trendBrush[0] = upBrush;
			}
			else if(2*smoothedAvg < middle - 3*neutralSlope)
			{
				trend[0] = -1.0;
				trendBrush[0] = downBrush;
			}
			else if(threshold > 0)
			{
				trend[0] = 0.0;
				trendBrush[0] = neutralBrush;
			}
			else
			{
				trend[0] = trend[1];
				trendBrush[0] = priorTrendBrush;
			}
			
			if(showMidband)
				PlotBrushes[0][0] = trendBrush[0];
			else
				PlotBrushes[0][0] = transparentBrush;
			
			if(showChannel)
			{
				PlotBrushes[1][0] = trendBrush[0];
				PlotBrushes[2][0] = trendBrush[0];
			}
			else 
			{
				PlotBrushes[1][0] = transparentBrush;
				PlotBrushes[2][0] = transparentBrush;
			}
			
			if(showPaintBars && CurrentBar >= totalBarsRequiredToPlot)
			{
				if(displacement >= 0)
				{
					if(trend[displacement] > 0.5)
					{
						if(Open[0] < Close[0])
							BarBrushes[0] = upBrushUp;
						else
							BarBrushes[0] = upBrushDown;
						CandleOutlineBrushes[0] = upBrushOutline;
					}	
					else if(trend[displacement] < -0.5)
					{
						if(Open[0] < Close[0])
							BarBrushes[0] = downBrushUp;
						else
							BarBrushes[0] = downBrushDown;
						CandleOutlineBrushes[0] = downBrushOutline;
					}	
					else
					{
						if(Open[0] < Close[0])
							BarBrushes[0] = neutralBrushUp;
						else
							BarBrushes[0] = neutralBrushDown;
						CandleOutlineBrushes[0] = neutralBrushOutline;
					}	
				}
				else 
				{
					if(trend[0] > 0.5)
					{
						if(Open[-displacement] < Close[-displacement])
							BarBrushes[-displacement] = upBrushUp;
						else
							BarBrushes[-displacement] = upBrushDown;
						CandleOutlineBrushes[-displacement] = upBrushOutline;
					}	
					else if(trend[0] < -0.5)
					{
						if(Open[-displacement] < Close[-displacement])
							BarBrushes[-displacement] = downBrushUp;
						else
							BarBrushes[-displacement] = downBrushDown;
						CandleOutlineBrushes[-displacement] = downBrushOutline;
					}	
					else
					{
						if(Open[-displacement] < Close[-displacement])
							BarBrushes[-displacement] = neutralBrushUp;
						else
							BarBrushes[-displacement] = neutralBrushDown;
						CandleOutlineBrushes[-displacement] = neutralBrushOutline;
					}
				}
			}	
				
			if (soundAlerts && State == State.Realtime && IsConnected())
			{
				if(trend[0] > 0.5 && trend[1] < 0.5)		
				{
					try
					{
							Alert("New_Uptrend", Priority.Medium,"New uptrend", pathNewUptrend, rearmTime, alertBackBrush, upBrush);
					}
					catch{}
				}
				else if(trend[0] < -0.5 && trend[1] > -0.5)	 
				{
					try
					{
							Alert("New_Downtrend", Priority.Medium,"New downtrend", pathNewDowntrend, rearmTime, alertBackBrush, downBrush);
					}
					catch{}
				}
				else if(trend[0] < 0.5 && trend[0] > -0.5 && (trend[1] > 0.5 || trend[1] < -0.5))	 
				{
					try
					{
							Alert("New_Neutraltrend", Priority.Medium,"New neutral trend", pathNewNeutraltrend, rearmTime, alertBackBrush, neutralBrush);
					}
					catch{}
				}
			}				
		}
		
		#region Properties
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Midband
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Upper
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Lower
		{
			get { return Values[2]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Trend
		{
			get { return trend; }
		}
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Midband smoothing", Description = "Select moving average type or moving median type for the midband", GroupName = "Algorithmic Options", Order = 0)]
		public amaMultiDevBandsMAType MidbandMAType
		{	
            get { return midbandMAType; }
            set { midbandMAType = value; }
		}
			
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Deviation type", Description = "Select deviation type used for calculating the channel width", GroupName = "Algorithmic Options", Order = 1)]
		public amaMultiDevBandsOffsetType OffsetType
		{	
            get { return offsetType; }
            set { offsetType = value; }
		}
			
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Additional Smoothing", Description = "When set to 'true' midband and channel will be smoothed with a SMA(3)", GroupName = "Algorithmic Options", Order = 2)]
        public bool Smoothed
        {
            get { return smoothed; }
            set { smoothed = value; }
        }
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Midband period", Description = "Select lookback period for moving average selected as midband", GroupName = "Input Parameters", Order = 0)]
		public int BasePeriod
		{	
            get { return basePeriod; }
            set { basePeriod = value; }
		}
			
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Deviation period", Description = "Select number of data points used for selected deviation formula", GroupName = "Input Parameters", Order = 1)]
		public int DevPeriod
		{	
            get { return devPeriod; }
            set { devPeriod = value; }
		}
			
		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Deviation multiplier", Description = "Select deviation multiplier", GroupName = "Input Parameters", Order = 2)]
		public double NumDev
		{	
            get { return numDev; }
            set { numDev = value; }
		}
			
		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Neutral threshold", Description = "Select neutral threshold for determining slope range considered as 'flat' ", GroupName = "Input Parameters", Order = 3)]
		public int Threshold
		{	
            get { return threshold; }
            set { threshold = value; }
		}
			
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show midband", Description = "Displays the channel midband on the chart", GroupName = "Display Options", Order = 0)]
        public bool ShowMidband
        {
            get { return showMidband; }
            set { showMidband = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show channel lines", Description = "Displays channel lines on the chart", GroupName = "Display Options", Order = 1)]
        public bool ShowChannel
        {
            get { return showChannel; }
            set { showChannel = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Channel flooding", Description = "Activates channel flooding", GroupName = "Display Options", Order = 2)]
        public bool ChannelFlooding
        {
            get { return channelFlooding; }
            set { channelFlooding = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show paint bars", Description = "Activates paint bars", GroupName = "Display Options", Order = 3)]
        public bool ShowPaintBars
        {
            get { return showPaintBars; }
            set { showPaintBars = value; }
        }
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bullish trend", Description = "Sets the color for a bullish trend", GroupName = "Plots", Order = 0)]
		public System.Windows.Media.Brush UpBrush
		{ 
			get {return upBrush;}
			set {upBrush = value;}
		}

		[Browsable(false)]
		public string UpBrushSerializable
		{
			get { return Serialize.BrushToString(upBrush); }
			set { upBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bearish trend", Description = "Sets the color for a bearish trend", GroupName = "Plots", Order = 1)]
		public System.Windows.Media.Brush DownBrush
		{ 
			get {return downBrush;}
			set {downBrush = value;}
		}

		[Browsable(false)]
		public string DownBrushSerializable
		{
			get { return Serialize.BrushToString(downBrush); }
			set { downBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Neutral trend", Description = "Sets the color for a neutral trend", GroupName = "Plots", Order = 2)]
		public System.Windows.Media.Brush NeutralBrush
		{ 
			get {return neutralBrush;}
			set {neutralBrush = value;}
		}

		[Browsable(false)]
		public string NeutralBrushSerializable
		{
			get { return Serialize.BrushToString(neutralBrush); }
			set { neutralBrush = Serialize.StringToBrush(value); }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot style midband", Description = "Sets the plot style for the midband plot", GroupName = "Plots", Order = 3)]
		public PlotStyle Plot0Style
		{	
            get { return plot0Style; }
            set { plot0Style = value; }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style midband", Description = "Sets the dash style for the midband plot", GroupName = "Plots", Order = 4)]
		public DashStyleHelper Dash0Style
		{
			get { return dash0Style; }
			set { dash0Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width midband", Description = "Sets the plot width for the midband plot", GroupName = "Plots", Order = 5)]
		public int Plot0Width
		{	
            get { return plot0Width; }
            set { plot0Width = value; }
		}
			
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot style channel", Description = "Sets the plot style for the channel plots", GroupName = "Plots", Order = 6)]
		public PlotStyle Plot1Style
		{	
            get { return plot1Style; }
            set { plot1Style = value; }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style channel", Description = "Sets the dash style for the channel plots", GroupName = "Plots", Order = 7)]
		public DashStyleHelper Dash1Style
		{
			get { return dash1Style; }
			set { dash1Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width channel", Description = "Sets the plot width for the channel plots", GroupName = "Plots", Order = 8)]
		public int Plot1Width
		{	
            get { return plot1Width; }
            set { plot1Width = value; }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bullish trend", Description = "Sets the color for a bullish trend", GroupName = "Channel Fill", Order = 0)]
		public System.Windows.Media.Brush CloudBrushUpS
		{ 
			get {return cloudBrushUpS;}
			set {cloudBrushUpS = value;}
		}

		[Browsable(false)]
		public string CloudBrushUpSSerializable
		{
			get { return Serialize.BrushToString(cloudBrushUpS); }
			set { cloudBrushUpS = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bearish trend", Description = "Sets the color for a bearish trend", GroupName = "Channel Fill", Order = 1)]
		public System.Windows.Media.Brush CloudBrushDownS
		{ 
			get {return cloudBrushDownS;}
			set {cloudBrushDownS = value;}
		}

		[Browsable(false)]
		public string CloudBrushDownSSerializable
		{
			get { return Serialize.BrushToString(cloudBrushDownS); }
			set { cloudBrushDownS = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Neutral trend", Description = "Sets the color for a neutral trend", GroupName = "Channel Fill", Order = 2)]
		public System.Windows.Media.Brush CloudBrushNeutralS
		{ 
			get {return cloudBrushNeutralS;}
			set {cloudBrushNeutralS = value;}
		}

		[Browsable(false)]
		public string CloudBrushNeutralSSerializable
		{
			get { return Serialize.BrushToString(cloudBrushNeutralS); }
			set { cloudBrushNeutralS = Serialize.StringToBrush(value); }
		}
		
		[Range(0, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Channel opacity", Description = "Select channel opacity between 0 (transparent) and 100 (no opacity)", GroupName = "Channel Fill", Order = 4)]
        public int ChannelOpacity
        {
            get { return channelOpacity; }
            set { channelOpacity = value; }
        }
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bullish upclose", Description = "Sets the color for a bullish trend", GroupName = "Paint Bars", Order = 0)]
		public System.Windows.Media.Brush UpBrushUp
		{ 
			get {return upBrushUp;}
			set {upBrushUp = value;}
		}

		[Browsable(false)]
		public string UpBrushUpSerializable
		{
			get { return Serialize.BrushToString(upBrushUp); }
			set { upBrushUp = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bullish downclose", Description = "Sets the color for a bullish trend", GroupName = "Paint Bars", Order = 1)]
		public System.Windows.Media.Brush UpBrushDown
		{ 
			get {return upBrushDown;}
			set {upBrushDown = value;}
		}

		[Browsable(false)]
		public string UpBrushDownSerializable
		{
			get { return Serialize.BrushToString(upBrushDown); }
			set { upBrushDown = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bullish candle outline", Description = "Sets the color for candle outlines", GroupName = "Paint Bars", Order = 2)]
		public System.Windows.Media.Brush UpBrushOutline
		{ 
			get {return upBrushOutline;}
			set {upBrushOutline = value;}
		}

		[Browsable(false)]
		public string UpBrushOutlineSerializable
		{
			get { return Serialize.BrushToString(upBrushOutline); }
			set { upBrushOutline = Serialize.StringToBrush(value); }
		}					
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bearish upclose", Description = "Sets the color for a bearish trend", GroupName = "Paint Bars", Order = 3)]
		public System.Windows.Media.Brush DownBrushUp
		{ 
			get {return downBrushUp;}
			set {downBrushUp = value;}
		}

		[Browsable(false)]
		public string DownBrushUpSerializable
		{
			get { return Serialize.BrushToString(downBrushUp); }
			set { downBrushUp = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bearish downclose", Description = "Sets the color for a bearish trend", GroupName = "Paint Bars", Order = 4)]
		public System.Windows.Media.Brush DownBrushDown
		{ 
			get {return downBrushDown;}
			set {downBrushDown = value;}
		}

		[Browsable(false)]
		public string DownBrushDownSerializable
		{
			get { return Serialize.BrushToString(downBrushDown); }
			set { downBrushDown = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bearish candle outline", Description = "Sets the color for candle outlines", GroupName = "Paint Bars", Order = 5)]
		public System.Windows.Media.Brush DownBrushOutline
		{ 
			get {return downBrushOutline;}
			set {downBrushOutline = value;}
		}

		[Browsable(false)]
		public string DownBrushOutlineSerializable
		{
			get { return Serialize.BrushToString(downBrushOutline); }
			set { downBrushOutline = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Neutral upclose", Description = "Sets the color for a neutral trend", GroupName = "Paint Bars", Order = 6)]
		public System.Windows.Media.Brush NeutralBrushUp
		{ 
			get {return neutralBrushUp;}
			set {neutralBrushUp = value;}
		}

		[Browsable(false)]
		public string NeutralBrushUpSerializable
		{
			get { return Serialize.BrushToString(neutralBrushUp); }
			set { neutralBrushUp = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Neutral downclose", Description = "Sets the color for a neutral trend", GroupName = "Paint Bars", Order = 7)]
		public System.Windows.Media.Brush NeutralBrushDown
		{ 
			get {return neutralBrushDown;}
			set {neutralBrushDown = value;}
		}

		[Browsable(false)]
		public string NeutralBrushDownSerializable
		{
			get { return Serialize.BrushToString(neutralBrushDown); }
			set { neutralBrushDown = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Neutral candle outline", Description = "Sets the color for candle outlines", GroupName = "Paint Bars", Order = 8)]
		public System.Windows.Media.Brush NeutralBrushOutline
		{ 
			get {return neutralBrushOutline;}
			set {neutralBrushOutline = value;}
		}

		[Browsable(false)]
		public string NeutralBrushOutlineSerializable
		{
			get { return Serialize.BrushToString(neutralBrushOutline); }
			set { neutralBrushOutline = Serialize.StringToBrush(value); }
		}

		[Display(ResourceType = typeof(Custom.Resource), Name = "Sound alerts", GroupName = "Sound Alerts", Order = 0)]
        public bool SoundAlerts
        {
            get { return soundAlerts; }
            set { soundAlerts = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "New uptrend", Description = "Sound file for new uptrend", GroupName = "Sound Alerts", Order = 1)]
		public string NewUptrend
		{	
            get { return newUptrend; }
            set { newUptrend = value; }
		}		
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "New downtrend", Description = "Sound file for new downtrend", GroupName = "Sound Alerts", Order = 2)]
		public string NewDowntrend
		{	
            get { return newDowntrend; }
            set { newDowntrend = value; }
		}		
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "New neutral trend", Description = "Sound file for new nuetral trend", GroupName = "Sound Alerts", Order = 3)]
		public string NewNeutraltrend
		{	
            get { return newNeutraltrend; }
            set { newNeutraltrend = value; }
		}		
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Rearm time", Description = "Rearm time for alerts in seconds", GroupName = "Sound Alerts", Order = 4)]
		public int RearmTime
		{	
            get { return rearmTime; }
            set { rearmTime = value; }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Release and date", Description = "Release and date", GroupName = "Version", Order = 0)]
		public string VersionString
		{	
            get { return versionString; }
            set { ; }
		}
		#endregion
		
		#region Miscellaneous
		
		public override string FormatPriceMarker(double price)
		{
			if(indicatorIsOnPricePanel)
				return Instrument.MasterInstrument.FormatPrice(Instrument.MasterInstrument.RoundToTickSize(price));
			else
				return base.FormatPriceMarker(price);
		}
		
		private bool IsConnected()
        {
			if ( Bars != null && Bars.Instrument.GetMarketDataConnection().PriceStatus == NinjaTrader.Cbi.ConnectionStatus.Connected
					&& sessionIterator.IsInSession(Now, true, true))
				return true;
			else
            	return false;
        }
		
		private DateTime Now
		{
          get 
			{ 
				DateTime now = (Bars.Instrument.GetMarketDataConnection().Options.Provider == NinjaTrader.Cbi.Provider.Playback ? Bars.Instrument.GetMarketDataConnection().Now : DateTime.Now); 

				if (now.Millisecond > 0)
					now = NinjaTrader.Core.Globals.MinDate.AddSeconds((long) System.Math.Floor(now.Subtract(NinjaTrader.Core.Globals.MinDate).TotalSeconds));

				return now;
			}
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			if (ChartBars == null || BarsArray[0].Count < ChartBars.ToIndex || ChartBars.ToIndex < BarsRequiredToPlot || !IsVisible) return;

			SharpDX.Direct2D1.Brush upBrushDX 	= upBrush.ToDxBrush(RenderTarget);
			SharpDX.Direct2D1.Brush downBrushDX = downBrush.ToDxBrush(RenderTarget);
			SharpDX.Direct2D1.Brush neutralBrushDX = neutralBrush.ToDxBrush(RenderTarget);
			SharpDX.Direct2D1.Brush cloudBrushUpDX 	= cloudBrushUp.ToDxBrush(RenderTarget);
			SharpDX.Direct2D1.Brush cloudBrushDownDX = cloudBrushDown.ToDxBrush(RenderTarget);
			SharpDX.Direct2D1.Brush cloudBrushNeutralDX = cloudBrushNeutral.ToDxBrush(RenderTarget);
			SharpDX.Direct2D1.AntialiasMode oldAntialiasMode = RenderTarget.AntialiasMode;

	        ChartPanel panel = chartControl.ChartPanels[ChartPanel.PanelIndex];
			
			bool nonEquidistant 	= (chartControl.BarSpacingType == BarSpacingType.TimeBased || chartControl.BarSpacingType == BarSpacingType.EquidistantMulti);
			int	lastBarPainted 	 	= ChartBars.ToIndex;
			int lastBarCounted		= Input.Count - 1;
			int	lastBarOnUpdate		= lastBarCounted - (Calculate == Calculate.OnBarClose ? 1 : 0);
			int	lastBarIndex		= Math.Min(lastBarPainted, lastBarOnUpdate);
			int firstBarPainted	 	= ChartBars.FromIndex;
			int firstBarIndex  	 	= Math.Max(BarsRequiredToPlot, firstBarPainted);
			int lastIndex			= 0;
			int x 					= 0;
			int y1 					= 0;
			int y2 					= 0;
			int y 					= 0;
			int t					= 0;
			int lastX 				= 0;
			int lastY0 				= 0;
			int lastY1 				= 0;
			int lastY2 				= 0;
			int lastY 				= 0;
			int sign 				= 0;
			int lastSign 			= 0;
			int startBar 			= 0;
			int priorStartBar 		= 0;
			int returnBar 			= 0;
			int count	 			= 0;
			double barWidth			= 0;
			
			bool firstLoop 			= true;
			Vector2[] cloudArray 	= new Vector2[2 * (lastBarIndex - firstBarIndex + Math.Max(0, displacement) + 1)]; 
			Vector2[] plotArray 	= new Vector2[2 * (lastBarIndex - firstBarIndex + Math.Max(0, displacement) + 1)]; 
			
			if(lastBarIndex + displacement > firstBarIndex)
			{	
				if (displacement >= 0)
					lastIndex = lastBarIndex + displacement;
				else
					lastIndex = Math.Min(lastBarIndex, lastBarOnUpdate + displacement);
				if(nonEquidistant && lastIndex > lastBarOnUpdate)
					barWidth = Convert.ToDouble(ChartControl.GetXByBarIndex(ChartBars, lastBarPainted) - ChartControl.GetXByBarIndex(ChartBars, lastBarPainted - displacement))/displacement;
				lastY0	= chartScale.GetYByValue(Midband.GetValueAt(lastIndex - displacement));
				lastY1	= chartScale.GetYByValue(Upper.GetValueAt(lastIndex - displacement));
				lastY2	= chartScale.GetYByValue(Lower.GetValueAt(lastIndex - displacement));
				lastSign = (int) Trend.GetValueAt(lastIndex - displacement);
				
				//Area
				if(channelIsFlooded)
				{	
					lastY = lastY1;
					startBar = lastIndex;
					firstLoop = true;
					
					do
					{
						SharpDX.Direct2D1.PathGeometry 	path;
						SharpDX.Direct2D1.GeometrySink 	sink;
						path = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
						using (path)
						{
							priorStartBar = startBar;
							if(firstLoop)
								count = -1;
							else
							{	
								count = 0;
								cloudArray[count] = new Vector2(lastX, lastY);
							}	
							for (int idx = startBar; idx >= firstBarIndex; idx --)	
							{
								if(nonEquidistant && displacement > 0 && idx > lastBarCounted)
									x = ChartControl.GetXByBarIndex(ChartBars, lastBarCounted) + (int)((idx - lastBarCounted) * barWidth);
								else
									x = ChartControl.GetXByBarIndex(ChartBars, idx);
								y = chartScale.GetYByValue(Upper.GetValueAt(idx - displacement));   
								t = (int) Trend.GetValueAt(idx - displacement);
								count = count + 1;
								cloudArray[count] = new Vector2(x,y);
								returnBar = idx;
								if(lastSign > 0 && t > 0)
									sign = 1;
								else if (lastSign < 0 && t < 0)
									sign = -1;
								else if (lastSign == 0 && t == 0)
									sign = 0;
								else
								{	
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
							}
							startBar = returnBar;
							for (int idx = returnBar ; idx <= priorStartBar; idx ++)	
							{
								if(nonEquidistant && displacement > 0 && idx > lastBarCounted)
									x = ChartControl.GetXByBarIndex(ChartBars, lastBarCounted) + (int)((idx - lastBarCounted) * barWidth);
								else
									x = ChartControl.GetXByBarIndex(ChartBars, idx);
								y = chartScale.GetYByValue(Lower.GetValueAt(idx - displacement));   
								count = count + 1;
								cloudArray[count] = new Vector2(x,y);
							}
							sink = path.Open();
							sink.BeginFigure(cloudArray[0], FigureBegin.Filled);
							for (int i = 1; i <= count; i++)
								sink.AddLine(cloudArray[i]);
							sink.EndFigure(FigureEnd.Closed);
			        		sink.Close();
							RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
							if(sign == 1)
		 						RenderTarget.FillGeometry(path, cloudBrushUpDX);
							else if(sign == -1) 
		 						RenderTarget.FillGeometry(path, cloudBrushDownDX);
							else if(sign == 0)
								RenderTarget.FillGeometry(path, cloudBrushNeutralDX);
							RenderTarget.AntialiasMode = oldAntialiasMode;
						}
						path.Dispose();
						sink.Dispose();
						firstLoop = false;
					}	
					while (startBar > firstBarIndex);
				}
				
				//Plots
				if(showMidband)
				{
					lastY = lastY0;
					startBar = lastIndex;
					firstLoop = true;

					do
					{
						SharpDX.Direct2D1.PathGeometry 	pathM;
						SharpDX.Direct2D1.GeometrySink 	sinkM;
						pathM = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
						using (pathM)
						{
							priorStartBar = startBar;
							if(firstLoop)
								count = -1;
							else
							{	
								count = 0;
								plotArray[count] = new Vector2(lastX, lastY);
							}	
							for (int idx = startBar; idx >= firstBarIndex; idx --)	
							{
								if(nonEquidistant && displacement > 0 && idx > lastBarCounted)
									x = ChartControl.GetXByBarIndex(ChartBars, lastBarCounted) + (int)((idx - lastBarCounted) * barWidth);
								else
									x = ChartControl.GetXByBarIndex(ChartBars, idx);
								y = chartScale.GetYByValue(Midband.GetValueAt(idx - displacement));   
								t = (int) Trend.GetValueAt(idx - displacement);
								count = count + 1;
								plotArray[count] = new Vector2(x,y);
								startBar = idx;
								if(lastSign > 0 && t > 0)
									sign = 1;
								else if (lastSign < 0 && t < 0)
									sign = -1;
								else if (lastSign == 0 && t == 0)
									sign = 0;
								else
								{	
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
							}
							sinkM = pathM.Open();
							sinkM.BeginFigure(plotArray[0], FigureBegin.Filled);
							for (int i = 1; i <= count; i++)
								sinkM.AddLine(plotArray[i]);
							sinkM.EndFigure(FigureEnd.Open);
			        		sinkM.Close();
							RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
							if(sign == 1)
		 						RenderTarget.DrawGeometry(pathM, upBrushDX, Plots[0].Width, Plots[0].StrokeStyle);
							else if(sign == -1) 
		 						RenderTarget.DrawGeometry(pathM, downBrushDX, Plots[0].Width, Plots[0].StrokeStyle);
							else if(sign == 0)
								RenderTarget.DrawGeometry(pathM, neutralBrushDX, Plots[0].Width, Plots[0].StrokeStyle);
							RenderTarget.AntialiasMode = oldAntialiasMode;
						}
						pathM.Dispose();
						sinkM.Dispose();
						firstLoop = false;
					}	
					while (startBar > firstBarIndex);
				}
				
				
				if(showChannel) 
				{	
					lastY = lastY1;
					startBar = lastIndex;
					firstLoop = true;

					do
					{
						SharpDX.Direct2D1.PathGeometry 	pathU;
						SharpDX.Direct2D1.GeometrySink 	sinkU;
						pathU = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
						using (pathU)
						{
							priorStartBar = startBar;
							if(firstLoop)
								count = -1;
							else
							{	
								count = 0;
								plotArray[count] = new Vector2(lastX, lastY);
							}	
							for (int idx = startBar; idx >= firstBarIndex; idx --)	
							{
								if(nonEquidistant && displacement > 0 && idx > lastBarCounted)
									x = ChartControl.GetXByBarIndex(ChartBars, lastBarCounted) + (int)((idx - lastBarCounted) * barWidth);
								else
									x = ChartControl.GetXByBarIndex(ChartBars, idx);
								y = chartScale.GetYByValue(Upper.GetValueAt(idx - displacement));   
								t = (int) Trend.GetValueAt(idx - displacement);
								count = count + 1;
								plotArray[count] = new Vector2(x,y);
								startBar = idx;
								if(lastSign > 0 && t > 0)
									sign = 1;
								else if (lastSign < 0 && t < 0)
									sign = -1;
								else if (lastSign == 0 && t == 0)
									sign = 0;
								else
								{	
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
							}
							sinkU = pathU.Open();
							sinkU.BeginFigure(plotArray[0], FigureBegin.Filled);
							for (int i = 1; i <= count; i++)
								sinkU.AddLine(plotArray[i]);
							sinkU.EndFigure(FigureEnd.Open);
			        		sinkU.Close();
							RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
							if(sign == 1)
		 						RenderTarget.DrawGeometry(pathU, upBrushDX, Plots[1].Width, Plots[1].StrokeStyle);
							else if(sign == -1) 
		 						RenderTarget.DrawGeometry(pathU, downBrushDX, Plots[1].Width, Plots[1].StrokeStyle);
							else if(sign == 0)
								RenderTarget.DrawGeometry(pathU, neutralBrushDX, Plots[1].Width, Plots[1].StrokeStyle);
							RenderTarget.AntialiasMode = oldAntialiasMode;
						}
						pathU.Dispose();
						sinkU.Dispose();
						firstLoop = false;
					}	
					while (startBar > firstBarIndex);
					
					lastY = lastY2;
					startBar = lastIndex;
					firstLoop = true;
					do
					{
						SharpDX.Direct2D1.PathGeometry 	pathL;
						SharpDX.Direct2D1.GeometrySink 	sinkL;
						pathL = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
						using (pathL)
						{
							priorStartBar = startBar;
							if(firstLoop)
								count = -1;
							else
							{	
								count = 0;
								plotArray[count] = new Vector2(lastX, lastY);
							}	
							for (int idx = startBar; idx >= firstBarIndex; idx --)	
							{
								if(nonEquidistant && displacement > 0 && idx > lastBarCounted)
									x = ChartControl.GetXByBarIndex(ChartBars, lastBarCounted) + (int)((idx - lastBarCounted) * barWidth);
								else
									x = ChartControl.GetXByBarIndex(ChartBars, idx);
								y = chartScale.GetYByValue(Lower.GetValueAt(idx - displacement));   
								t = (int) Trend.GetValueAt(idx - displacement);
								count = count + 1;
								plotArray[count] = new Vector2(x,y);
								startBar = idx;
								if(lastSign > 0 && t > 0)
									sign = 1;
								else if (lastSign < 0 && t < 0)
									sign = -1;
								else if (lastSign == 0 && t == 0)
									sign = 0;
								else
								{	
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
							}
							sinkL = pathL.Open();
							sinkL.BeginFigure(plotArray[0], FigureBegin.Filled);
							for (int i = 1; i <= count; i++)
								sinkL.AddLine(plotArray[i]);
							sinkL.EndFigure(FigureEnd.Open);
			        		sinkL.Close();
							RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
							if(sign == 1)
		 						RenderTarget.DrawGeometry(pathL, upBrushDX, Plots[1].Width, Plots[1].StrokeStyle);
							else if(sign == -1) 
		 						RenderTarget.DrawGeometry(pathL, downBrushDX, Plots[1].Width, Plots[1].StrokeStyle);
							else if(sign == 0)
								RenderTarget.DrawGeometry(pathL, neutralBrushDX, Plots[1].Width, Plots[1].StrokeStyle);
							RenderTarget.AntialiasMode = oldAntialiasMode;
						}
						pathL.Dispose();
						sinkL.Dispose();
						firstLoop = false;
					}	
					while (startBar > firstBarIndex);
				}	
			}			
			upBrushDX.Dispose();
			downBrushDX.Dispose();
			neutralBrushDX.Dispose();
			cloudBrushUpDX.Dispose();
			cloudBrushDownDX.Dispose();
			cloudBrushNeutralDX.Dispose();
		}		
		#endregion	
	}
}

#region Global Enums

public enum amaMultiDevBandsMAType {Adaptive_Laguerre, ADXVMA, Butterworth_2, Butterworth_3, DEMA, Distant_Coefficient_Filter, DWMA, EHMA, EMA, Gauss_2, Gauss_3, Gauss_4, HMA, HoltEMA, 
			Laguerre, LinReg, Mean_TPO, Median, Median_TPO, RWMA, SMA, SuperSmoother_2, SuperSmoother_3, SWMA, TEMA, Tillson_T3, TMA, TWMA, Wilder, WMA, ZerolagHATEMA, ZerolagTEMA, ZLEMA}
public enum	amaMultiDevBandsOffsetType {Mean_Abs_Dev, Median_Abs_Dev, RMS_Dev}

#endregion

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LizardIndicators.amaMultiDeviationBands[] cacheamaMultiDeviationBands;
		public LizardIndicators.amaMultiDeviationBands amaMultiDeviationBands(amaMultiDevBandsMAType midbandMAType, amaMultiDevBandsOffsetType offsetType, bool smoothed, int basePeriod, int devPeriod, double numDev, int threshold)
		{
			return amaMultiDeviationBands(Input, midbandMAType, offsetType, smoothed, basePeriod, devPeriod, numDev, threshold);
		}

		public LizardIndicators.amaMultiDeviationBands amaMultiDeviationBands(ISeries<double> input, amaMultiDevBandsMAType midbandMAType, amaMultiDevBandsOffsetType offsetType, bool smoothed, int basePeriod, int devPeriod, double numDev, int threshold)
		{
			if (cacheamaMultiDeviationBands != null)
				for (int idx = 0; idx < cacheamaMultiDeviationBands.Length; idx++)
					if (cacheamaMultiDeviationBands[idx] != null && cacheamaMultiDeviationBands[idx].MidbandMAType == midbandMAType && cacheamaMultiDeviationBands[idx].OffsetType == offsetType && cacheamaMultiDeviationBands[idx].Smoothed == smoothed && cacheamaMultiDeviationBands[idx].BasePeriod == basePeriod && cacheamaMultiDeviationBands[idx].DevPeriod == devPeriod && cacheamaMultiDeviationBands[idx].NumDev == numDev && cacheamaMultiDeviationBands[idx].Threshold == threshold && cacheamaMultiDeviationBands[idx].EqualsInput(input))
						return cacheamaMultiDeviationBands[idx];
			return CacheIndicator<LizardIndicators.amaMultiDeviationBands>(new LizardIndicators.amaMultiDeviationBands(){ MidbandMAType = midbandMAType, OffsetType = offsetType, Smoothed = smoothed, BasePeriod = basePeriod, DevPeriod = devPeriod, NumDev = numDev, Threshold = threshold }, input, ref cacheamaMultiDeviationBands);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaMultiDeviationBands amaMultiDeviationBands(amaMultiDevBandsMAType midbandMAType, amaMultiDevBandsOffsetType offsetType, bool smoothed, int basePeriod, int devPeriod, double numDev, int threshold)
		{
			return indicator.amaMultiDeviationBands(Input, midbandMAType, offsetType, smoothed, basePeriod, devPeriod, numDev, threshold);
		}

		public Indicators.LizardIndicators.amaMultiDeviationBands amaMultiDeviationBands(ISeries<double> input , amaMultiDevBandsMAType midbandMAType, amaMultiDevBandsOffsetType offsetType, bool smoothed, int basePeriod, int devPeriod, double numDev, int threshold)
		{
			return indicator.amaMultiDeviationBands(input, midbandMAType, offsetType, smoothed, basePeriod, devPeriod, numDev, threshold);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaMultiDeviationBands amaMultiDeviationBands(amaMultiDevBandsMAType midbandMAType, amaMultiDevBandsOffsetType offsetType, bool smoothed, int basePeriod, int devPeriod, double numDev, int threshold)
		{
			return indicator.amaMultiDeviationBands(Input, midbandMAType, offsetType, smoothed, basePeriod, devPeriod, numDev, threshold);
		}

		public Indicators.LizardIndicators.amaMultiDeviationBands amaMultiDeviationBands(ISeries<double> input , amaMultiDevBandsMAType midbandMAType, amaMultiDevBandsOffsetType offsetType, bool smoothed, int basePeriod, int devPeriod, double numDev, int threshold)
		{
			return indicator.amaMultiDeviationBands(input, midbandMAType, offsetType, smoothed, basePeriod, devPeriod, numDev, threshold);
		}
	}
}

#endregion
