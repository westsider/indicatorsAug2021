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
using NinjaTrader.Core;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Indicators.LizardIndicators;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using SharpDX;
using SharpDX.Direct2D1;
#endregion

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.LizardIndicators
{
	/// <summary>
	/// The Current Week VWAP is the volume weighted average price of the current week. 
	/// The indicator further displays three upper and lower volatility bands.
	/// </summary>
	/// 
	[Gui.CategoryOrder("Algorithmic Options", 1000100)]
	[Gui.CategoryOrder("Custom Hours", 1000200)]
	[Gui.CategoryOrder("Standard Deviation Bands", 1000300)]
	[Gui.CategoryOrder("Quarter Range Bands", 1000400)]
	[Gui.CategoryOrder("Plot Colors", 8000100)]
	[Gui.CategoryOrder("Plot Parameters", 8000200)]
	[Gui.CategoryOrder("Area Opacity", 8000300)]
	[Gui.CategoryOrder("Version", 8000400)]
	[TypeConverter("NinjaTrader.NinjaScript.Indicators.amaCurrentWeekVWAPTypeConverter")]
	public class amaCurrentWeekVWAP : Indicator
	{
		private DateTime						sessionDateTmp				= Globals.MinDate;
		private DateTime						cacheWeeklyEndDate			= Globals.MinDate;
		private TimeSpan						customSessionStart			= new TimeSpan(8,30,0);
		private TimeSpan						customSessionEnd			= new TimeSpan(15,15,0);
		private double							multiplierSD1				= 1.0;
		private double							multiplierSD2				= 2.0;
		private double							multiplierSD3				= 3.0;
		private double							multiplierQR1				= 1.0;
		private double							multiplierQR2				= 2.0;
		private double							multiplierQR3				= 3.0;
		private double							multiplier1					= 1.0;
		private double							multiplier2					= 2.0;
		private double							multiplier3					= 3.0;
		private double							open						= 0.0;
		private double							high						= 0.0;
		private double							low							= 0.0;
		private double							close						= 0.0;
		private double							mean						= 0.0;
		private double							mean1						= 0.0;
		private double							mean2						= 0.0;
		private	double							volSum						= 0.0;
		private	double							squareSum					= 0.0;
		private	double							priorSessionHigh			= 0.0;
		private	double							priorSessionLow				= 0.0;
		private double							priorVWAP					= 0.0;
		private double							sessionVWAP					= 0.0;
		private int								displacement				= 0;
		private int								count						= 0;
		private bool							showBands					= true;
		private bool							plotVWAP					= false;
		private bool							gap0						= true;
		private bool							gap1						= true;
		private bool							timeBased					= true;
		private bool							breakAtEOD					= true;
		private bool							calculateFromPriceData		= true;
		private bool							applyTradingHours			= false;
		private bool							anchorBar					= false;
		private bool							basicError					= false;
		private bool							errorMessage				= false;
		private bool							sundaySessionError			= false;
		private bool							isCryptoCurrency			= false;
		private bool							startEndTimeError			= false;
		private amaSessionTypeVWAPW				sessionType					= amaSessionTypeVWAPW.Full_Session;
		private amaTimeZonesVWAPW				customTZSelector			= amaTimeZonesVWAPW.Exchange_Time;
		private amaBandTypeVWAPW				bandType					= amaBandTypeVWAPW.Standard_Deviation;
		private readonly List<int>				newSessionBarIdxArr			= new List<int>();
		private SessionIterator					sessionIterator				= null;
		private System.Windows.Media.Brush		upBrush						= Brushes.Blue;
		private System.Windows.Media.Brush  	downBrush					= Brushes.Red;
		private System.Windows.Media.Brush		innerBandBrush				= Brushes.LimeGreen;
		private System.Windows.Media.Brush  	middleBandBrush				= Brushes.ForestGreen;
		private System.Windows.Media.Brush		outerBandBrush				= Brushes.DarkGreen;
		private System.Windows.Media.Brush		innerAreaBrush 				= null;
		private System.Windows.Media.Brush		middleAreaBrush 			= null;
		private System.Windows.Media.Brush		outerAreaBrush 				= null;
		private System.Windows.Media.Brush		errorBrush					= null;
		private SharpDX.Direct2D1.Brush 		innerAreaBrushDX 			= null;
		private SharpDX.Direct2D1.Brush 		middleAreaBrushDX 			= null;
		private SharpDX.Direct2D1.Brush 		outerAreaBrushDX 			= null;
		private SimpleFont						errorFont					= null;
		private string							errorText1					= "The amaCurrentWeekVWAP only works on price data.";
		private string							errorText2					= "The amaCurrentWeekVWAP can only be displayed on intraday charts.";
		private string							errorText3					= "The amaCurrentWeekVWAP cannot be used with a negative displacement.";
		private string							errorText4					= "The amaCurrentWeekVWAP cannot be used with a displacement on non-equidistant chart bars.";
		private string							errorText5					= "The amaCurrentWeekVWAP cannot be used when the 'Break at EOD' data series property is unselected.";
		private string							errorText6					= "amaCurrentWeekVWAP: The VWAP may not be calculated from fractional Sunday sessions. Please change your trading hours template.";
		private string							errorText7					= "amaCurrentWeekVWAP: Mismatch between trading hours selected for the VWAP and the session template selected for the chart bars!";
		private int								innerAreaOpacity			= 60;
		private int								middleAreaOpacity			= 0;
		private int								outerAreaOpacity			= 60;
		private int								plot0Width					= 3;
		private int								plot1Width					= 1;
		private PlotStyle						plot0Style					= PlotStyle.Line;
		private DashStyleHelper					dash0Style					= DashStyleHelper.DashDot;
		private PlotStyle						plot1Style					= PlotStyle.Line;
		private DashStyleHelper					dash1Style					= DashStyleHelper.Solid;
		private TimeZoneInfo					globalTimeZone				= Core.Globals.GeneralOptions.TimeZoneInfo;
		private TimeZoneInfo					customTimeZone;
		private string							versionString				= "v 2.8  -  February 26, 2021";
		private Series<DateTime>				tradingDate;
		private Series<DateTime>				tradingWeek;
		private Series<DateTime>				sessionBegin;
		private Series<DateTime>				anchorTime;
		private Series<DateTime>				cutoffTime;
		private Series<bool>					isFirstDayOfPeriod;
		private Series<bool>					calcOpen;
		private Series<bool>					initWeeklyPlot;
		private Series<int>						sessionBar;
		private Series<double>					firstBarOpen;
		private Series<double>					currentVolSum;
		private Series<double>					currentVWAP;
		private Series<double>					currentSquareSum;
		private Series<double>					sessionHigh;
		private Series<double>					sessionLow;
		private Series<double>					offset;
			
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\nThe Current Week VWAP is the volume weighted average price of the current week. The indicator further displays three upper and lower volatility bands.";
				Name						= "amaCurrentWeekVWAP";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				Calculate					= Calculate.OnEachTick;
				IsAutoScale					= false;
				ArePlotsConfigurable		= false;
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "Session VWAP");	
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "Upper Band SD 3");
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "Upper Band SD 2");
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "Upper Band SD 1");	
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "Lower Band SD 1");
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "Lower Band SD 2");
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "Lower Band SD 3");	
				SetZOrder(-2);
			}
			else if (State == State.Configure)
			{
				displacement = Displacement;
				Plots[1].Brush = outerBandBrush.Clone();
				Plots[2].Brush = middleBandBrush.Clone();
				Plots[3].Brush = innerBandBrush.Clone();
				Plots[4].Brush = innerBandBrush.Clone();
				Plots[5].Brush = middleBandBrush.Clone();
				Plots[6].Brush = outerBandBrush.Clone();
				Plots[0].Width = plot0Width;
				Plots[0].PlotStyle = plot0Style;
				Plots[0].DashStyleHelper = dash0Style;			
				Plots[1].Width = plot1Width;
				Plots[1].PlotStyle = plot1Style;
				Plots[1].DashStyleHelper = dash1Style;
				Plots[2].Width = plot1Width;
				Plots[2].PlotStyle = plot1Style;
				Plots[2].DashStyleHelper = dash1Style;
				Plots[3].Width = plot1Width;
				Plots[3].PlotStyle = plot1Style;
				Plots[3].DashStyleHelper = dash1Style;
				Plots[4].Width = plot1Width;
				Plots[4].PlotStyle = plot1Style;
				Plots[4].DashStyleHelper = dash1Style;
				Plots[5].Width = plot1Width;
				Plots[5].PlotStyle = plot1Style;
				Plots[5].DashStyleHelper = dash1Style;
				Plots[6].Width = plot1Width;
				Plots[6].PlotStyle = plot1Style;
				Plots[6].DashStyleHelper = dash1Style;
				innerAreaBrush	= innerBandBrush.Clone();
				innerAreaBrush.Opacity = (float) innerAreaOpacity/100.0;
				innerAreaBrush.Freeze();
				middleAreaBrush	= middleBandBrush.Clone();
				middleAreaBrush.Opacity = (float) middleAreaOpacity/100.0;
				middleAreaBrush.Freeze();
				outerAreaBrush	= outerBandBrush.Clone();
				outerAreaBrush.Opacity = (float) outerAreaOpacity/100.0;
				outerAreaBrush.Freeze();
			}
		  	else if (State == State.DataLoaded)
		 	{
				tradingDate = new Series<DateTime>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				tradingWeek = new Series<DateTime>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				sessionBegin = new Series<DateTime>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				anchorTime = new Series<DateTime>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				cutoffTime = new Series<DateTime>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				isFirstDayOfPeriod = new Series<bool>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				calcOpen = new Series<bool>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				initWeeklyPlot = new Series<bool>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				sessionBar = new Series<int>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				firstBarOpen = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				currentVolSum = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				currentVWAP = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				currentSquareSum = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				sessionHigh = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				sessionLow = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				offset = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				if (Bars.BarsType.IsTimeBased) 
					timeBased = true;
				else
					timeBased = false;
				if(Input is PriceSeries)
					calculateFromPriceData = true;
				else
					calculateFromPriceData = false;
		    	sessionIterator = new SessionIterator(Bars);
		  	}
			else if (State == State.Historical)
			{
				if (sessionType == amaSessionTypeVWAPW.Full_Session) 
					applyTradingHours = false;
				else if (sessionType == amaSessionTypeVWAPW.Custom_Hours) 
					applyTradingHours = true;
				if(bandType == amaBandTypeVWAPW.Standard_Deviation)
				{
					multiplier1 = multiplierSD1;
					multiplier2 = multiplierSD2;
					multiplier3 = multiplierSD3;
					showBands = true;
				}
				else if(bandType == amaBandTypeVWAPW.Quarter_Range)
				{
					multiplier1 = multiplierQR1;
					multiplier2 = multiplierQR2;
					multiplier3 = multiplierQR3;
					showBands = true;
				}
				else if(bandType == amaBandTypeVWAPW.None)
					showBands = false;
				if (Instrument.MasterInstrument.InstrumentType == InstrumentType.CryptoCurrency)
					isCryptoCurrency = true;
				else
					isCryptoCurrency = false;
				switch (customTZSelector)
				{
					case amaTimeZonesVWAPW.Exchange_Time:	
						customTimeZone = Instrument.MasterInstrument.TradingHours.TimeZoneInfo;
						break;
					case amaTimeZonesVWAPW.Chart_Time:	
						customTimeZone = Core.Globals.GeneralOptions.TimeZoneInfo;
						break;
					case amaTimeZonesVWAPW.US_Eastern_Standard_Time:	
						customTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");	
						break;
					case amaTimeZonesVWAPW.US_Central_Standard_Time:	
						customTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");	
						break;
					case amaTimeZonesVWAPW.US_Mountain_Standard_Time:	
						customTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");	
						break;
					case amaTimeZonesVWAPW.US_Pacific_Standard_Time:	
						customTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");	
						break;
					case amaTimeZonesVWAPW.AUS_Eastern_Standard_Time:	
						customTimeZone = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");	
						break;
					case amaTimeZonesVWAPW.Japan_Standard_Time:	
						customTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");	
						break;
					case amaTimeZonesVWAPW.China_Standard_Time:	
						customTimeZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");	
						break;
					case amaTimeZonesVWAPW.India_Standard_Time:	
						customTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");	
						break;
					case amaTimeZonesVWAPW.Central_European_Time:	
						customTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");	
						break;
					case amaTimeZonesVWAPW.GMT_Standard_Time:	
						customTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");	
						break;
				}					
				gap0 = (plot0Style == PlotStyle.Line || plot0Style == PlotStyle.Square);
				gap1 = (plot1Style == PlotStyle.Line || plot1Style == PlotStyle.Square);
				innerAreaBrush	= innerBandBrush.Clone();
				innerAreaBrush.Opacity = (float) innerAreaOpacity/100.0;
				innerAreaBrush.Freeze();
				middleAreaBrush	= middleBandBrush.Clone();
				middleAreaBrush.Opacity = (float) middleAreaOpacity/100.0;
				middleAreaBrush.Freeze();
				outerAreaBrush	= outerBandBrush.Clone();
				outerAreaBrush.Opacity = (float) outerAreaOpacity/100.0;
				outerAreaBrush.Freeze();
				if(ChartBars != null)
				{	
					breakAtEOD = ChartBars.Bars.IsResetOnNewTradingDay;
					errorBrush = ChartControl.Properties.AxisPen.Brush;
					errorBrush.Freeze();
					errorFont = new SimpleFont("Arial", 24);
				}
				basicError = false;
				errorMessage = false;
				if(!calculateFromPriceData)
				{
					Draw.TextFixed(this, "error text 1", errorText1, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);  
					errorMessage = true;
					basicError = true;
				}	
				else if (!Bars.BarsType.IsIntraday)
				{
					Draw.TextFixed(this, "error text 2", errorText2, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);  
					errorMessage = true;
					basicError = true;
				}
				else if(displacement < 0)
				{
					Draw.TextFixed(this, "error text 3", errorText3, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);  
					errorMessage = true;
					basicError = true;
				}
				else if (ChartBars != null && (ChartControl.BarSpacingType == BarSpacingType.TimeBased || ChartControl.BarSpacingType == BarSpacingType.EquidistantMulti) && displacement != 0)
				{
					Draw.TextFixed(this, "error text 4", errorText4, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);  
					errorMessage = true;
					basicError = true;
				}	
				else if(!breakAtEOD)
				{
					Draw.TextFixed(this, "error text 5", errorText5, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);  
					errorMessage = true;
					basicError = true;
				}
				sundaySessionError = false;
				startEndTimeError = false;
			}
		}

		protected override void OnBarUpdate()
		{
			if(IsFirstTickOfBar)
			{	
				if(errorMessage)
				{	
					if(basicError)
						return;
					else if(sundaySessionError)
					{	
						Draw.TextFixed(this, "error text 6", errorText6, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);
						RemoveDrawObject("error text 7");
						return;
					}	
					else if(startEndTimeError)
					{	
						Draw.TextFixed(this, "error text 7", errorText7, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);
						return;
					}
				}	
			}
			
			if (CurrentBar == 0)
			{	
				if(IsFirstTickOfBar)
				{	
					tradingDate[0] = GetLastBarSessionDateD(Time[0]);
					tradingWeek[0] = GetLastBarSessionDateW(Time[0]);
					sessionBegin[0] = sessionIterator.ActualSessionBegin;
					if(applyTradingHours)
					{	
						anchorTime[0] = Globals.MinDate;
						cutoffTime[0] = Globals.MinDate;
					}	
					isFirstDayOfPeriod[0] = false;
					calcOpen[0] = false;
					initWeeklyPlot[0] = false;
					firstBarOpen[0] = Open[0];
					anchorBar = false;
					sessionBar.Reset();
					currentVolSum.Reset();
					currentVWAP.Reset();
					currentSquareSum.Reset();
					sessionHigh.Reset();
					sessionLow.Reset();
					offset.Reset();
					SessionVWAP.Reset();
					UpperBand3.Reset();
					UpperBand2.Reset();
					UpperBand1.Reset();
					LowerBand1.Reset();
					LowerBand2.Reset();
					LowerBand3.Reset();
				}	
				return;
			}
			if(IsFirstTickOfBar)
			{	
				if(Bars.IsFirstBarOfSession)
				{	
					tradingDate[0] = GetLastBarSessionDateD(Time[0]); 
					if(tradingDate[0].DayOfWeek == DayOfWeek.Sunday)
					{
						if(!isCryptoCurrency)
						{	
							sundaySessionError = true; 
							errorMessage = true;
							return;
						}	
					}
					sessionBegin[0] = sessionIterator.ActualSessionBegin;
					tradingWeek[0] = GetLastBarSessionDateW(Time[0]);
					if(tradingWeek[0] != tradingWeek[1])				
					{	
						isFirstDayOfPeriod[0] = true;
						calcOpen[0] = false;
						initWeeklyPlot[0] = false;
						firstBarOpen[0] = Open[0];
					}
					else if(tradingDate[0] != tradingDate[1])
					{
						isFirstDayOfPeriod[0] = false;
						if(applyTradingHours)
							calcOpen[0] = false;
						else
							calcOpen[0] = calcOpen[1];
						initWeeklyPlot[0] = initWeeklyPlot[1];
						firstBarOpen[0] = firstBarOpen[1];	
					}
					else
					{	
						isFirstDayOfPeriod[0] = isFirstDayOfPeriod[1];
						calcOpen[0] = calcOpen[1];
						initWeeklyPlot[0] = initWeeklyPlot[1];
						firstBarOpen[0] = firstBarOpen[1];	
					}	
					if(tradingDate[0] != tradingDate[1])
					{
						if(applyTradingHours)
						{	
							anchorTime[0] = TimeZoneInfo.ConvertTime(tradingDate[0].Add(customSessionStart), customTimeZone, globalTimeZone);
							if(anchorTime[0] >= sessionBegin[0].AddHours(24))
								anchorTime[0] = anchorTime[0].AddHours(-24);
							else if(anchorTime[0] < sessionBegin[0])
								anchorTime[0] = anchorTime[0].AddHours(24);
							cutoffTime[0] = TimeZoneInfo.ConvertTime(tradingDate[0].Add(customSessionEnd), customTimeZone, globalTimeZone);
							if(cutoffTime[0] > sessionBegin[0].AddHours(24))
								cutoffTime[0] = cutoffTime[0].AddHours(-24);
							else if(cutoffTime[0] <= sessionBegin[0])
								cutoffTime[0] = cutoffTime[0].AddHours(24);
							if(cutoffTime[0] <= anchorTime[0])
							{
								startEndTimeError = true;
								errorMessage = true;
								return;
							}
						}	
					}	
					else
					{	
						if(applyTradingHours)
						{	
							anchorTime[0] = anchorTime[1];
							cutoffTime[0] = cutoffTime[1];
						}	
					}	
				}					
				else
				{	
					tradingDate[0] = tradingDate[1];
					tradingWeek[0] = tradingWeek[1];
					sessionBegin[0] = sessionBegin[1];
					isFirstDayOfPeriod[0] = isFirstDayOfPeriod[1];
					calcOpen[0] = calcOpen[1];
					initWeeklyPlot[0] = initWeeklyPlot[1];
					firstBarOpen[0] = firstBarOpen[1];	
					if(applyTradingHours)
					{	
						anchorTime[0] = anchorTime[1];
						cutoffTime[0] = cutoffTime[1];
					}	
				}	
			}	
			if(applyTradingHours) 
			{
				if(timeBased && Time[0] > anchorTime[0] && Time[1] <= anchorTime[0])
					anchorBar = true;
				else if(!timeBased && Time[0] >= anchorTime[0] && Time[1] < anchorTime[0])
					anchorBar = true;
				else
					anchorBar = false;
				if(timeBased && Time[0] > cutoffTime[0] && Time[1] <= cutoffTime[0])
					calcOpen[0] = false;
				else if(!timeBased && Time[0] >= cutoffTime[0] && Time[1] < cutoffTime[0])
					calcOpen[0] = false;
			}

			if ((!applyTradingHours && tradingWeek[0] != tradingWeek[1]) || (applyTradingHours && isFirstDayOfPeriod[0] && anchorBar))
			{
				if(IsFirstTickOfBar || !calcOpen[0])
				{	
					initWeeklyPlot[0] 	= true;
					sessionBar[0]		= 1;
				}	
				open				= Open[0] - firstBarOpen[0];
				high				= High[0] - firstBarOpen[0];
				low 				= Low[0] - firstBarOpen[0];
				close				= Close[0] - firstBarOpen[0];
				mean1				= 0.5*(high + low);
				mean2				= 0.5*(open + close);
				mean				= 0.5*(mean1 + mean2);
				currentVolSum[0] 	= Volume[0];
				currentVWAP[0]		= mean;
				if(bandType == amaBandTypeVWAPW.Standard_Deviation)
				{	
					currentSquareSum[0]	= Volume[0]*(open*open + high*high + low*low + close*close + 2*mean2*mean2 + 2*mean1*mean1)/8.0;
					offset[0]			= (currentVolSum[0] > 0.5) ? Math.Sqrt(currentSquareSum[0]/currentVolSum[0] - currentVWAP[0]*currentVWAP[0]) : 0;
				}
				else if(bandType == amaBandTypeVWAPW.Quarter_Range)
				{	
					sessionHigh[0]	= High[0];
					sessionLow[0]	= Low[0];
					offset[0]		= 0.25*(sessionHigh[0] - sessionLow[0]);
				}	
				else
				{
					currentSquareSum.Reset();
					sessionHigh.Reset();
					sessionLow.Reset();
					offset.Reset();
				}	
				calcOpen[0]	= true;
				plotVWAP = true;
			}
			else if (applyTradingHours && anchorBar) 
			{
				if(!calcOpen[0])
				{	
					sessionBar[0] 	= sessionBar[1] + 1;
					volSum			= currentVolSum[1];
					priorVWAP		= currentVWAP[1];
				}	
				open				= Open[0] - firstBarOpen[0];
				high				= High[0] - firstBarOpen[0];
				low 				= Low[0] - firstBarOpen[0];
				close				= Close[0] - firstBarOpen[0];
				mean1				= 0.5*(high + low);
				mean2				= 0.5*(open + close);
				mean				= 0.5*(mean1 + mean2);
				currentVolSum[0]	= volSum + Volume[0];
				currentVWAP[0]		= (currentVolSum[0] > 0.5 ) ? (volSum*priorVWAP + Volume[0]*mean)/currentVolSum[0] : mean;
				if(bandType == amaBandTypeVWAPW.Standard_Deviation)
				{	
					if(!calcOpen[0])
						squareSum 		= currentSquareSum[1];
					currentSquareSum[0] = squareSum + Volume[0]*(open*open + high*high + low*low + close*close + 2*mean2*mean2 + 2*mean1*mean1)/8.0;
					offset[0]			= (currentVolSum[0] > 0.5) ? Math.Sqrt(currentSquareSum[0]/currentVolSum[0] - currentVWAP[0]*currentVWAP[0]) : 0;
				}	
				else if(bandType == amaBandTypeVWAPW.Quarter_Range)
				{
					if(!calcOpen[0])
					{
						priorSessionHigh = sessionHigh[1];
						priorSessionLow	= sessionLow[1];
					}
					sessionHigh[0]		= Math.Max(priorSessionHigh, High[0]);
					sessionLow[0]		= Math.Min(priorSessionLow, Low[0]);
					offset[0]			= 0.25*(sessionHigh[0] - sessionLow[0]);
				}
				else
				{
					currentSquareSum.Reset();
					sessionHigh.Reset();
					sessionLow.Reset();
					offset.Reset();
				}	
				calcOpen[0] = true;
			}
			else if (calcOpen[0])
			{
				if (IsFirstTickOfBar)
				{
					sessionBar[0] 	= sessionBar[1] + 1;
					volSum			= currentVolSum[1];
					priorVWAP		= currentVWAP[1];
				}
				open				= Open[0] - firstBarOpen[0];
				high				= High[0] - firstBarOpen[0];
				low 				= Low[0] - firstBarOpen[0];
				close				= Close[0] - firstBarOpen[0];
				mean1				= 0.5*(high + low);
				mean2				= 0.5*(open + close);
				mean				= 0.5*(mean1 + mean2);
				currentVolSum[0]	= volSum + Volume[0];
				currentVWAP[0]		= (currentVolSum[0] > 0.5 ) ? (volSum*priorVWAP + Volume[0]*mean)/currentVolSum[0] : mean;
				if(bandType == amaBandTypeVWAPW.Standard_Deviation)
				{	
					if(IsFirstTickOfBar)
						squareSum 		= currentSquareSum[1];
					currentSquareSum[0]	= squareSum + Volume[0]*(open*open + high*high + low*low + close*close + 2*mean2*mean2 + 2*mean1*mean1)/8.0;
					offset[0]			= (currentVolSum[0] > 0.5) ? Math.Sqrt(currentSquareSum[0]/currentVolSum[0] - currentVWAP[0]*currentVWAP[0]) : 0;
				}	
				else if(bandType == amaBandTypeVWAPW.Quarter_Range)
				{
					if(IsFirstTickOfBar)
					{
						priorSessionHigh = sessionHigh[1];
						priorSessionLow	= sessionLow[1];
					}
					sessionHigh[0]		= Math.Max(priorSessionHigh, High[0]);
					sessionLow[0]		= Math.Min(priorSessionLow, Low[0]);
					offset[0]			= 0.25*(sessionHigh[0] - sessionLow[0]);
				}
				else
				{
					currentSquareSum.Reset();
					sessionHigh.Reset();
					sessionLow.Reset();
					offset.Reset();
				}	
			}
			else 
			{	
				if(initWeeklyPlot[0])
				{	
					if(IsFirstTickOfBar)
						sessionBar[0] = sessionBar[1] + 1;
					currentVolSum[0] = currentVolSum[1];
					currentVWAP[0] = currentVWAP[1];
					if(bandType == amaBandTypeVWAPW.Standard_Deviation)
					{	
						currentSquareSum[0] = currentSquareSum[1];
						offset[0] = offset[1];	
					}	
					else if(bandType == amaBandTypeVWAPW.Quarter_Range)	
					{	
						sessionHigh[0]	= sessionHigh[1];
						sessionLow[0]	= sessionLow[1];
						offset[0] = offset[1];
					}
					else
					{
						currentSquareSum.Reset();
						sessionHigh.Reset();
						sessionLow.Reset();
						offset.Reset();
					}	
				}	
				else if (IsFirstTickOfBar)
				{		
					sessionBar.Reset();
					currentVolSum.Reset();
					currentVWAP.Reset();
					currentSquareSum.Reset();
					sessionHigh.Reset();
					sessionLow.Reset();
					offset.Reset();
				}	
			}	

			if (plotVWAP && initWeeklyPlot[0])
			{
				sessionVWAP = currentVWAP[0] + firstBarOpen[0];
				SessionVWAP[0] = sessionVWAP;
				if (bandType == amaBandTypeVWAPW.None)
				{
					UpperBand3.Reset();
					UpperBand2.Reset();
					UpperBand1.Reset();
					LowerBand1.Reset();
					LowerBand2.Reset();
					LowerBand3.Reset();
				}	
				else
				{
					UpperBand3[0] = sessionVWAP + multiplier3 * offset[0];
					UpperBand2[0] = sessionVWAP + multiplier2 * offset[0];
					UpperBand1[0] = sessionVWAP + multiplier1 * offset[0];
					LowerBand1[0] = sessionVWAP - multiplier1 * offset[0];
					LowerBand2[0] = sessionVWAP - multiplier2 * offset[0];
					LowerBand3[0] = sessionVWAP - multiplier3 * offset[0];
				}
				
				if (sessionBar[0] == 1 && gap0)
					PlotBrushes[0][0] = Brushes.Transparent;
				else if (SessionVWAP[0] > SessionVWAP[1])
					PlotBrushes[0][0] = upBrush;
				else if (SessionVWAP[0] < SessionVWAP[1])
					PlotBrushes[0][0] = downBrush;
				else if(sessionBar[0] == 2 && gap0)
					PlotBrushes[0][0] = upBrush;
				else
					PlotBrushes[0][0] = PlotBrushes[0][1];
				if(sessionBar[0] == 1 && gap1)
				{
					for (int i = 1; i <= 6; i++)
						PlotBrushes[i][0] = Brushes.Transparent;
				}
			}
			else
			{
				SessionVWAP.Reset();
				UpperBand3.Reset();
				UpperBand2.Reset();
				UpperBand1.Reset();
				LowerBand1.Reset();
				LowerBand2.Reset();
				LowerBand3.Reset();
			}	
		}
		
		#region Properties
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SessionVWAP
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> UpperBand3
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> UpperBand2
		{
			get { return Values[2]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> UpperBand1
		{
			get { return Values[3]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> LowerBand1
		{
			get { return Values[4]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> LowerBand2
		{
			get { return Values[5]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> LowerBand3
		{
			get { return Values[6]; }
		}
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Select session", Description = "Select session - full session or custom session - for calculating the VWAP", GroupName = "Algorithmic Options", Order = 0)]
		[RefreshProperties(RefreshProperties.All)] 
		public amaSessionTypeVWAPW SessionType
		{	
            get { return sessionType; }
            set { sessionType = value; }
		}
			
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Band type", Description = "Select formula for calculating volatility bands", GroupName = "Algorithmic Options", Order = 1)]
 		[RefreshProperties(RefreshProperties.All)] 
		public amaBandTypeVWAPW BandType
		{	
            get { return bandType; }
            set { bandType = value; }
		}
			
		[NinjaScriptProperty] 
		[Display(ResourceType = typeof(Custom.Resource), Name="Select time zone", Description="Enter time zone for custom session", GroupName="Custom Hours", Order = 0)]
		public amaTimeZonesVWAPW CustomTZSelector
		{
			get
			{
				return customTZSelector;
			}
			set
			{
				customTZSelector = value;
			}
		}
			
		[Browsable(false)]
		[XmlIgnore]
		public TimeSpan CustomSessionStart
		{
			get { return customSessionStart;}
			set { customSessionStart = value;}
		}	
	
		[NinjaScriptProperty] 
		[Display(ResourceType = typeof(Custom.Resource), Name="Custom start time (+ h:min)", Description="Enter start time for VWAP calculation in time zone of exchange", GroupName="Custom Hours", Order = 1)]
		public string S_CustomSessionStart	
		{
			get 
			{ 
				return string.Format("{0:D2}:{1:D2}", customSessionStart.Hours, customSessionStart.Minutes);
			}
			set 
			{ 
				char[] delimiters = new char[] {':'};
				string[]values =((string)value).Split(delimiters, StringSplitOptions.None);
				customSessionStart = new TimeSpan(Convert.ToInt16(values[0]),Convert.ToInt16(values[1]),0);
			}
		}
	
		[Browsable(false)]
		[XmlIgnore]
		public TimeSpan CustomSessionEnd
		{
			get { return customSessionEnd;}
			set { customSessionEnd = value;}
		}	
	
		[NinjaScriptProperty] 
		[Display(ResourceType = typeof(Custom.Resource), Name="Custom end time (+ h:min)", Description="Enter end time for VWAP calculation in time zone of exchange", GroupName="Custom Hours", Order = 2)]
		public string S_CustomSessionEnd	
		{
			get 
			{ 
				return string.Format("{0:D2}:{1:D2}", customSessionEnd.Hours, customSessionEnd.Minutes);
			}
			set 
			{ 
				char[] delimiters = new char[] {':'};
				string[]values =((string)value).Split(delimiters, StringSplitOptions.None);
				customSessionEnd = new TimeSpan(Convert.ToInt16(values[0]),Convert.ToInt16(values[1]),0);
			}
		}
	
		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SD multiplier 1", Description = "Select multiplier for inner standard deviation bands", GroupName = "Standard Deviation Bands", Order = 0)]
		public double MultiplierSD1 
		{
			get { return multiplierSD1; }
			set { multiplierSD1 = value; }
		}
			
		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SD multiplier 2", Description = "Select multiplier for central standard deviation bands", GroupName = "Standard Deviation Bands", Order = 1)]
		public double MultiplierSD2
		{
			get { return multiplierSD2; }
			set { multiplierSD2 = value; }
		}
		
		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SD multiplier 3", Description = "Select multiplier for outer standard deviation bands", GroupName = "Standard Deviation Bands", Order = 2)]
		public double MultiplierSD3
		{
			get { return multiplierSD3; }
			set { multiplierSD3 = value; }
		}
			
		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "QR multiplier 1", Description = "Select multiplier for inner quarter range bands", GroupName = "Quarter Range Bands", Order = 0)]
		public double MultiplierQR1
		{
			get { return multiplierQR1; }
			set { multiplierQR1 = value; }
		}
			
		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "QR multiplier 2", Description = "Select multiplier for central quarter range bands", GroupName = "Quarter Range Bands", Order = 1)]
		public double MultiplierQR2
		{
			get { return multiplierQR2; }
			set { multiplierQR2 = value; }
		}
		
		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "QR multiplier 3", Description = "Select multiplier for outer quarter range bands", GroupName = "Quarter Range Bands", Order = 2)]
		public double MultiplierQR3
		{
			get { return multiplierQR3; }
			set { multiplierQR3 = value; }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Rising VWAP", Description = "Sets the color for a bullish VWAP", GroupName = "Plot Colors", Order = 0)]
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
		[Display(ResourceType = typeof(Custom.Resource), Name = "Falling VWAP", Description = "Sets the color for a bearish VWAP", GroupName = "Plot Colors", Order = 1)]
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
		[Display(ResourceType = typeof(Custom.Resource), Name = "Inner bands", Description = "Sets the color for the inner bands", GroupName = "Plot Colors", Order = 2)]
		public System.Windows.Media.Brush InnerBandBrush
		{ 
			get {return innerBandBrush;}
			set {innerBandBrush = value;}
		}

		[Browsable(false)]
		public string InnerBandBrushSerializable
		{
			get { return Serialize.BrushToString(innerBandBrush); }
			set { innerBandBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Middle bands", Description = "Sets the color for the middle bands", GroupName = "Plot Colors", Order = 3)]
		public System.Windows.Media.Brush MiddleBandBrush
		{ 
			get {return middleBandBrush;}
			set {middleBandBrush = value;}
		}

		[Browsable(false)]
		public string MiddleBandBrushSerializable
		{
			get { return Serialize.BrushToString(middleBandBrush); }
			set { middleBandBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Outer bands", Description = "Sets the color for the outer bands", GroupName = "Plot Colors", Order = 4)]
		public System.Windows.Media.Brush OuterBandBrush
		{ 
			get {return outerBandBrush;}
			set {outerBandBrush = value;}
		}

		[Browsable(false)]
		public string OuterBandBrushSerializable
		{
			get { return Serialize.BrushToString(outerBandBrush); }
			set { outerBandBrush = Serialize.StringToBrush(value); }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot style VWAP", Description = "Sets the plot style for the VWAP plot", GroupName = "Plot Parameters", Order = 0)]
		public PlotStyle Plot0Style
		{	
            get { return plot0Style; }
            set { plot0Style = value; }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style VWAP", Description = "Sets the dash style for the VWAP plot", GroupName = "Plot Parameters", Order = 1)]
		public DashStyleHelper Dash0Style
		{
			get { return dash0Style; }
			set { dash0Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width VWAP", Description = "Sets the plot width for the VWAP plot", GroupName = "Plot Parameters", Order = 2)]
		public int Plot0Width
		{	
            get { return plot0Width; }
            set { plot0Width = value; }
		}
			
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot style SD bands", Description = "Sets the plot style for the volatility bands", GroupName = "Plot Parameters", Order = 3)]
		public PlotStyle Plot1Style
		{	
            get { return plot1Style; }
            set { plot1Style = value; }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style SD bands", Description = "Sets the dash style for the volatility bands", GroupName = "Plot Parameters", Order = 4)]
		public DashStyleHelper Dash1Style
		{
			get { return dash1Style; }
			set { dash1Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width SD bands", Description = "Sets the plot width for the volatility bands", GroupName = "Plot Parameters", Order = 5)]
		public int Plot1Width
		{	
            get { return plot1Width; }
            set { plot1Width = value; }
		}
		
		[Range(0, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Inner bands opacity", Description = "Select channel opacity between 0 (transparent) and 100 (no opacity)", GroupName = "Area Opacity", Order = 0)]
        public int InnerAreaOpacity
        {
            get { return innerAreaOpacity; }
            set { innerAreaOpacity = value; }
        }
		
		[Range(0, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Middle bands opacity", Description = "Select channel opacity between 0 (transparent) and 100 (no opacity)", GroupName = "Area Opacity", Order = 1)]
        public int MiddleAreaOpacity
        {
            get { return middleAreaOpacity; }
            set { middleAreaOpacity = value; }
        }
		
		[Range(0, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Outer bands opacity", Description = "Select channel opacity between 0 (transparent) and 100 (no opacity)", GroupName = "Area Opacity", Order = 2)]
        public int OuterAreaOpacity
        {
            get { return outerAreaOpacity; }
            set { outerAreaOpacity = value; }
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
			return Instrument.MasterInstrument.FormatPrice(Instrument.MasterInstrument.RoundToTickSize(price));
		}
		
		private DateTime RoundUpTimeToPeriodTime(DateTime time)
		{
			return time.Date.AddDays(6 - (int)time.DayOfWeek);
		}	
		
		private DateTime GetLastBarSessionDateD(DateTime time)
		{
			sessionIterator.CalculateTradingDay(time, timeBased);
			sessionDateTmp = sessionIterator.ActualTradingDayExchange;
			return sessionDateTmp;			
		}
		
		private DateTime GetLastBarSessionDateW(DateTime time)
		{
			sessionIterator.CalculateTradingDay(time, timeBased);
			sessionDateTmp = sessionIterator.ActualTradingDayExchange;
			DateTime weeklyEndDateTmpW = RoundUpTimeToPeriodTime(sessionDateTmp);				
			if(weeklyEndDateTmpW != cacheWeeklyEndDate) 
			{
				cacheWeeklyEndDate = weeklyEndDateTmpW;
				if (newSessionBarIdxArr.Count == 0 || (newSessionBarIdxArr.Count > 0 && CurrentBar > (int) newSessionBarIdxArr[newSessionBarIdxArr.Count - 1]))
					newSessionBarIdxArr.Add(CurrentBar);
			}
			return weeklyEndDateTmpW;		
		}
		
		public override void OnRenderTargetChanged()
		{
			if (innerAreaBrushDX != null)
				innerAreaBrushDX.Dispose();
			if (middleAreaBrushDX != null)
				middleAreaBrushDX.Dispose();
			if (outerAreaBrushDX != null)
				outerAreaBrushDX.Dispose();

			if (RenderTarget != null)
			{
				try
				{
					innerAreaBrushDX 	= innerAreaBrush.ToDxBrush(RenderTarget);
					middleAreaBrushDX 	= middleAreaBrush.ToDxBrush(RenderTarget);
					outerAreaBrushDX 	= outerAreaBrush.ToDxBrush(RenderTarget);
				}
				catch (Exception e) { }
			}
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			if (ChartBars == null || errorMessage || !IsVisible) return;
			int	lastBarPainted = ChartBars.ToIndex;
			if(lastBarPainted  < 0 || BarsArray[0].Count < lastBarPainted) return;

			SharpDX.Direct2D1.AntialiasMode oldAntialiasMode = RenderTarget.AntialiasMode;
			RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.Aliased;

			bool nonEquidistant 			= (chartControl.BarSpacingType == BarSpacingType.TimeBased || chartControl.BarSpacingType == BarSpacingType.EquidistantMulti);
			int lastBarCounted				= Inputs[0].Count - 1;
			int	lastBarOnUpdate				= lastBarCounted - (Calculate == Calculate.OnBarClose ? 1 : 0);
			int	lastBarIndex				= Math.Min(lastBarPainted, lastBarOnUpdate);
			int firstBarPainted	 			= ChartBars.FromIndex;
			int firstBarIndex  	 			= Math.Max(BarsRequiredToPlot, firstBarPainted);
			int firstBarIdxToPaint  		= 0;
			int lastPlotIndex				= 0;
			int firstPlotIndex				= 0;
			int	returnBar					= 0;
			double barWidth					= chartControl.GetBarPaintWidth(chartControl.BarsArray[0]);
			int x							= 0;
			int y							= 0;
			Vector2[] cloudArray 			= new Vector2[2 * (Math.Max(0, lastBarIndex - firstBarIndex + displacement) + 1)]; 
			
			if(displacement > 0 && nonEquidistant)
				return;
			if(lastBarIndex + displacement > firstBarIndex)
			{	
				if (displacement > 0 && lastBarIndex < lastBarOnUpdate)
					lastPlotIndex = lastBarIndex + 1;
				else if (displacement > 0 && lastBarIndex == lastBarOnUpdate)
				{	
					lastPlotIndex = lastBarIndex + displacement;
					for(int i = 0; i < displacement; i++)
					{
						x = ChartControl.GetXByBarIndex(ChartBars, lastPlotIndex);
						if(x > ChartPanel.X + ChartPanel.W + 1.5*barWidth - ChartControl.Properties.BarMarginRight)
							lastPlotIndex = lastPlotIndex - 1;
						else
							break;
					}	
				}	
				else
					lastPlotIndex = lastBarIndex;
			
				if(showBands)
				{	
					do
					{
						for (int i = newSessionBarIdxArr.Count - 1; i >= 0; i--)
						{
							int prevSessionBreakIdx = newSessionBarIdxArr[i];
							if (prevSessionBreakIdx + displacement <= lastPlotIndex)
							{
								firstBarIdxToPaint = prevSessionBreakIdx + displacement;
								break;
							}
						}
						firstPlotIndex = Math.Max(firstBarIndex, firstBarIdxToPaint);
						
						if(innerAreaOpacity > 0) 
						{
							SharpDX.Direct2D1.PathGeometry 	pathI;
							SharpDX.Direct2D1.GeometrySink 	sinkI;
							pathI = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
							using (pathI)
							{
								count = -1;
								for (int idx = lastPlotIndex; idx >= firstPlotIndex; idx --)	
								{
									x = ChartControl.GetXByBarIndex(ChartBars, idx);
									if(Values[3].IsValidDataPointAt(idx-displacement))
									{	
										y = chartScale.GetYByValue(UpperBand1.GetValueAt(idx - displacement));
										returnBar = idx;	
									}	
									else
									{	
										returnBar = idx + 1;
										break;
									}	
									count = count + 1;
									cloudArray[count] = new Vector2(x,y);
								}
								if (count > 0)
								{	
									for (int idx = returnBar ; idx <= lastPlotIndex; idx ++)	
									{
										x = ChartControl.GetXByBarIndex(ChartBars, idx);
										y = chartScale.GetYByValue(LowerBand1.GetValueAt(idx - displacement));   
										count = count + 1;
										cloudArray[count] = new Vector2(x,y);
									}
								}	
								sinkI = pathI.Open();
								sinkI.BeginFigure(cloudArray[0], FigureBegin.Filled);
								for (int i = 1; i <= count; i++)
									sinkI.AddLine(cloudArray[i]);
								sinkI.EndFigure(FigureEnd.Closed);
				        		sinkI.Close();
								RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
			 					RenderTarget.FillGeometry(pathI, innerAreaBrushDX);
								RenderTarget.AntialiasMode = oldAntialiasMode;
							}
							pathI.Dispose();
							sinkI.Dispose();					
						}
						
						if(middleAreaOpacity > 0) 
						{
							SharpDX.Direct2D1.PathGeometry 	pathMU;
							SharpDX.Direct2D1.GeometrySink 	sinkMU;
							pathMU = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
							using (pathMU)
							{
								count = -1;
								for (int idx = lastPlotIndex; idx >= firstPlotIndex; idx --)	
								{
									x = ChartControl.GetXByBarIndex(ChartBars, idx);
									if(Values[2].IsValidDataPointAt(idx-displacement))
									{	
										y = chartScale.GetYByValue(UpperBand2.GetValueAt(idx - displacement));
										returnBar = idx;	
									}	
									else
									{	
										returnBar = idx + 1;
										break;
									}	
									count = count + 1;
									cloudArray[count] = new Vector2(x,y);
								}
								if (count > 0)
								{	
									for (int idx = returnBar ; idx <= lastPlotIndex; idx ++)	
									{
										x = ChartControl.GetXByBarIndex(ChartBars, idx);
										y = chartScale.GetYByValue(UpperBand1.GetValueAt(idx - displacement));   
										count = count + 1;
										cloudArray[count] = new Vector2(x,y);
									}
								}	
								sinkMU = pathMU.Open();
								sinkMU.BeginFigure(cloudArray[0], FigureBegin.Filled);
								for (int i = 1; i <= count; i++)
									sinkMU.AddLine(cloudArray[i]);
								sinkMU.EndFigure(FigureEnd.Closed);
				        		sinkMU.Close();
								RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
			 					RenderTarget.FillGeometry(pathMU, middleAreaBrushDX);
								RenderTarget.AntialiasMode = oldAntialiasMode;
							}
							pathMU.Dispose();
							sinkMU.Dispose();					
							SharpDX.Direct2D1.PathGeometry 	pathML;
							SharpDX.Direct2D1.GeometrySink 	sinkML;
							pathML = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
							using (pathML)
							{
								count = -1;
								for (int idx = lastPlotIndex; idx >= firstPlotIndex; idx --)	
								{
									x = ChartControl.GetXByBarIndex(ChartBars, idx);
									if(Values[4].IsValidDataPointAt(idx-displacement))
									{	
										y = chartScale.GetYByValue(LowerBand1.GetValueAt(idx - displacement));
										returnBar = idx;	
									}	
									else
									{	
										returnBar = idx + 1;
										break;
									}	
									count = count + 1;
									cloudArray[count] = new Vector2(x,y);
								}
								if (count > 0)
								{	
									for (int idx = returnBar ; idx <= lastPlotIndex; idx ++)	
									{
										x = ChartControl.GetXByBarIndex(ChartBars, idx);
										y = chartScale.GetYByValue(LowerBand2.GetValueAt(idx - displacement));   
										count = count + 1;
										cloudArray[count] = new Vector2(x,y);
									}
								}	
								sinkML = pathML.Open();
								sinkML.BeginFigure(cloudArray[0], FigureBegin.Filled);
								for (int i = 1; i <= count; i++)
									sinkML.AddLine(cloudArray[i]);
								sinkML.EndFigure(FigureEnd.Closed);
				        		sinkML.Close();
								RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
			 					RenderTarget.FillGeometry(pathML, middleAreaBrushDX);
								RenderTarget.AntialiasMode = oldAntialiasMode;
							}
							pathML.Dispose();
							sinkML.Dispose();						
						}					
						
						if(outerAreaOpacity > 0) 
						{
							SharpDX.Direct2D1.PathGeometry 	pathOU;
							SharpDX.Direct2D1.GeometrySink 	sinkOU;
							pathOU = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
							using (pathOU)
							{
								count = -1;
								for (int idx = lastPlotIndex; idx >= firstPlotIndex; idx --)	
								{
									x = ChartControl.GetXByBarIndex(ChartBars, idx);
									if(Values[1].IsValidDataPointAt(idx-displacement))
									{	
										y = chartScale.GetYByValue(UpperBand3.GetValueAt(idx - displacement));
										returnBar = idx;	
									}	
									else
									{	
										returnBar = idx + 1;
										break;
									}	
									count = count + 1;
									cloudArray[count] = new Vector2(x,y);
								}
								if (count > 0)
								{	
									for (int idx = returnBar ; idx <= lastPlotIndex; idx ++)	
									{
										x = ChartControl.GetXByBarIndex(ChartBars, idx);
										y = chartScale.GetYByValue(UpperBand2.GetValueAt(idx - displacement));   
										count = count + 1;
										cloudArray[count] = new Vector2(x,y);
									}
								}	
								sinkOU = pathOU.Open();
								sinkOU.BeginFigure(cloudArray[0], FigureBegin.Filled);
								for (int i = 1; i <= count; i++)
									sinkOU.AddLine(cloudArray[i]);
								sinkOU.EndFigure(FigureEnd.Closed);
				        		sinkOU.Close();
								RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
			 					RenderTarget.FillGeometry(pathOU, outerAreaBrushDX);
								RenderTarget.AntialiasMode = oldAntialiasMode;
							}
							pathOU.Dispose();
							sinkOU.Dispose();					
							SharpDX.Direct2D1.PathGeometry 	pathOL;
							SharpDX.Direct2D1.GeometrySink 	sinkOL;
							pathOL = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
							using (pathOL)
							{
								count = -1;
								for (int idx = lastPlotIndex; idx >= firstPlotIndex; idx --)	
								{
									x = ChartControl.GetXByBarIndex(ChartBars, idx);
									if(Values[5].IsValidDataPointAt(idx-displacement))
									{	
										y = chartScale.GetYByValue(LowerBand2.GetValueAt(idx - displacement));
										returnBar = idx;	
									}	
									else
									{	
										returnBar = idx + 1;
										break;
									}	
									count = count + 1;
									cloudArray[count] = new Vector2(x,y);
								}
								if (count > 0)
								{	
									for (int idx = returnBar ; idx <= lastPlotIndex; idx ++)	
									{
										x = ChartControl.GetXByBarIndex(ChartBars, idx);
										y = chartScale.GetYByValue(LowerBand3.GetValueAt(idx - displacement));   
										count = count + 1;
										cloudArray[count] = new Vector2(x,y);
									}
								}	
								sinkOL = pathOL.Open();
								sinkOL.BeginFigure(cloudArray[0], FigureBegin.Filled);
								for (int i = 1; i <= count; i++)
									sinkOL.AddLine(cloudArray[i]);
								sinkOL.EndFigure(FigureEnd.Closed);
				        		sinkOL.Close();
								RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
			 					RenderTarget.FillGeometry(pathOL, outerAreaBrushDX);
								RenderTarget.AntialiasMode = oldAntialiasMode;
							}
							pathOL.Dispose();
							sinkOL.Dispose();						
						}					
						if(lastPlotIndex < firstPlotIndex)
							lastPlotIndex = 0;
						else
							lastPlotIndex = firstPlotIndex - 1;
					}	
					while (lastPlotIndex > firstBarIndex);
				}
			}	
			RenderTarget.AntialiasMode = oldAntialiasMode;
			base.OnRender(chartControl, chartScale);
		}
		#endregion
	}
}

namespace NinjaTrader.NinjaScript.Indicators
{		
	public class amaCurrentWeekVWAPTypeConverter : NinjaTrader.NinjaScript.IndicatorBaseConverter
	{
		public override bool GetPropertiesSupported(ITypeDescriptorContext context) { return true; }

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			PropertyDescriptorCollection propertyDescriptorCollection = base.GetPropertiesSupported(context) ? base.GetProperties(context, value, attributes) : TypeDescriptor.GetProperties(value, attributes);

			amaCurrentWeekVWAP			thisVWAPInstance			= (amaCurrentWeekVWAP) value;
			amaSessionTypeVWAPW			sessionTypeFromInstance		= thisVWAPInstance.SessionType;
			amaBandTypeVWAPW			bandTypeFromInstance		= thisVWAPInstance.BandType;
			
			PropertyDescriptorCollection adjusted = new PropertyDescriptorCollection(null);
			
			foreach (PropertyDescriptor thisDescriptor in propertyDescriptorCollection)
			{
				if ((sessionTypeFromInstance == amaSessionTypeVWAPW.Full_Session) && (thisDescriptor.Name == "CustomTZSelector" 
					|| thisDescriptor.Name == "S_CustomSessionStart" || thisDescriptor.Name == "S_CustomSessionEnd"))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (bandTypeFromInstance == amaBandTypeVWAPW.None && (thisDescriptor.Name == "MultiplierSD1" || thisDescriptor.Name == "MultiplierSD2" || thisDescriptor.Name == "MultiplierSD3"	
					|| thisDescriptor.Name == "MultiplierQR1" || thisDescriptor.Name == "MultiplierQR2" || thisDescriptor.Name == "MultiplierQR3"
					|| thisDescriptor.Name == "InnerBandBrush" || thisDescriptor.Name == "MiddleBandBrush" || thisDescriptor.Name == "OuterBandBrush" 
					|| thisDescriptor.Name == "Plot1Style" || thisDescriptor.Name == "Dash1Style" || thisDescriptor.Name == "Plot1Width" 
					|| thisDescriptor.Name == "InnerAreaOpacity" || thisDescriptor.Name == "MiddleAreaOpacity" || thisDescriptor.Name == "OuterAreaOpacity"))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (bandTypeFromInstance != amaBandTypeVWAPW.Standard_Deviation && (thisDescriptor.Name == "MultiplierSD1" || thisDescriptor.Name == "MultiplierSD2" || thisDescriptor.Name == "MultiplierSD3"))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (bandTypeFromInstance != amaBandTypeVWAPW.Quarter_Range && (thisDescriptor.Name == "MultiplierQR1" || thisDescriptor.Name == "MultiplierQR2" || thisDescriptor.Name == "MultiplierQR3"))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else	
					adjusted.Add(thisDescriptor);
			}
			return adjusted;
		}
	}
}

#region Global Enums

public enum amaSessionTypeVWAPW 
{
	Full_Session, 
	Custom_Hours
}

public enum amaBandTypeVWAPW 
{
	Standard_Deviation,
	Quarter_Range,
	None
} 

public enum amaTimeZonesVWAPW
{
	Exchange_Time, 
	Chart_Time, 
	US_Eastern_Standard_Time, 
	US_Central_Standard_Time, 
	US_Mountain_Standard_Time, 
	US_Pacific_Standard_Time, 
	AUS_Eastern_Standard_Time, 
	Japan_Standard_Time, 
	China_Standard_Time, 
	India_Standard_Time, 
	Central_European_Time, 
	GMT_Standard_Time
}

#endregion

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LizardIndicators.amaCurrentWeekVWAP[] cacheamaCurrentWeekVWAP;
		public LizardIndicators.amaCurrentWeekVWAP amaCurrentWeekVWAP(amaSessionTypeVWAPW sessionType, amaBandTypeVWAPW bandType, amaTimeZonesVWAPW customTZSelector, string s_CustomSessionStart, string s_CustomSessionEnd, double multiplierSD1, double multiplierSD2, double multiplierSD3, double multiplierQR1, double multiplierQR2, double multiplierQR3)
		{
			return amaCurrentWeekVWAP(Input, sessionType, bandType, customTZSelector, s_CustomSessionStart, s_CustomSessionEnd, multiplierSD1, multiplierSD2, multiplierSD3, multiplierQR1, multiplierQR2, multiplierQR3);
		}

		public LizardIndicators.amaCurrentWeekVWAP amaCurrentWeekVWAP(ISeries<double> input, amaSessionTypeVWAPW sessionType, amaBandTypeVWAPW bandType, amaTimeZonesVWAPW customTZSelector, string s_CustomSessionStart, string s_CustomSessionEnd, double multiplierSD1, double multiplierSD2, double multiplierSD3, double multiplierQR1, double multiplierQR2, double multiplierQR3)
		{
			if (cacheamaCurrentWeekVWAP != null)
				for (int idx = 0; idx < cacheamaCurrentWeekVWAP.Length; idx++)
					if (cacheamaCurrentWeekVWAP[idx] != null && cacheamaCurrentWeekVWAP[idx].SessionType == sessionType && cacheamaCurrentWeekVWAP[idx].BandType == bandType && cacheamaCurrentWeekVWAP[idx].CustomTZSelector == customTZSelector && cacheamaCurrentWeekVWAP[idx].S_CustomSessionStart == s_CustomSessionStart && cacheamaCurrentWeekVWAP[idx].S_CustomSessionEnd == s_CustomSessionEnd && cacheamaCurrentWeekVWAP[idx].MultiplierSD1 == multiplierSD1 && cacheamaCurrentWeekVWAP[idx].MultiplierSD2 == multiplierSD2 && cacheamaCurrentWeekVWAP[idx].MultiplierSD3 == multiplierSD3 && cacheamaCurrentWeekVWAP[idx].MultiplierQR1 == multiplierQR1 && cacheamaCurrentWeekVWAP[idx].MultiplierQR2 == multiplierQR2 && cacheamaCurrentWeekVWAP[idx].MultiplierQR3 == multiplierQR3 && cacheamaCurrentWeekVWAP[idx].EqualsInput(input))
						return cacheamaCurrentWeekVWAP[idx];
			return CacheIndicator<LizardIndicators.amaCurrentWeekVWAP>(new LizardIndicators.amaCurrentWeekVWAP(){ SessionType = sessionType, BandType = bandType, CustomTZSelector = customTZSelector, S_CustomSessionStart = s_CustomSessionStart, S_CustomSessionEnd = s_CustomSessionEnd, MultiplierSD1 = multiplierSD1, MultiplierSD2 = multiplierSD2, MultiplierSD3 = multiplierSD3, MultiplierQR1 = multiplierQR1, MultiplierQR2 = multiplierQR2, MultiplierQR3 = multiplierQR3 }, input, ref cacheamaCurrentWeekVWAP);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaCurrentWeekVWAP amaCurrentWeekVWAP(amaSessionTypeVWAPW sessionType, amaBandTypeVWAPW bandType, amaTimeZonesVWAPW customTZSelector, string s_CustomSessionStart, string s_CustomSessionEnd, double multiplierSD1, double multiplierSD2, double multiplierSD3, double multiplierQR1, double multiplierQR2, double multiplierQR3)
		{
			return indicator.amaCurrentWeekVWAP(Input, sessionType, bandType, customTZSelector, s_CustomSessionStart, s_CustomSessionEnd, multiplierSD1, multiplierSD2, multiplierSD3, multiplierQR1, multiplierQR2, multiplierQR3);
		}

		public Indicators.LizardIndicators.amaCurrentWeekVWAP amaCurrentWeekVWAP(ISeries<double> input , amaSessionTypeVWAPW sessionType, amaBandTypeVWAPW bandType, amaTimeZonesVWAPW customTZSelector, string s_CustomSessionStart, string s_CustomSessionEnd, double multiplierSD1, double multiplierSD2, double multiplierSD3, double multiplierQR1, double multiplierQR2, double multiplierQR3)
		{
			return indicator.amaCurrentWeekVWAP(input, sessionType, bandType, customTZSelector, s_CustomSessionStart, s_CustomSessionEnd, multiplierSD1, multiplierSD2, multiplierSD3, multiplierQR1, multiplierQR2, multiplierQR3);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaCurrentWeekVWAP amaCurrentWeekVWAP(amaSessionTypeVWAPW sessionType, amaBandTypeVWAPW bandType, amaTimeZonesVWAPW customTZSelector, string s_CustomSessionStart, string s_CustomSessionEnd, double multiplierSD1, double multiplierSD2, double multiplierSD3, double multiplierQR1, double multiplierQR2, double multiplierQR3)
		{
			return indicator.amaCurrentWeekVWAP(Input, sessionType, bandType, customTZSelector, s_CustomSessionStart, s_CustomSessionEnd, multiplierSD1, multiplierSD2, multiplierSD3, multiplierQR1, multiplierQR2, multiplierQR3);
		}

		public Indicators.LizardIndicators.amaCurrentWeekVWAP amaCurrentWeekVWAP(ISeries<double> input , amaSessionTypeVWAPW sessionType, amaBandTypeVWAPW bandType, amaTimeZonesVWAPW customTZSelector, string s_CustomSessionStart, string s_CustomSessionEnd, double multiplierSD1, double multiplierSD2, double multiplierSD3, double multiplierQR1, double multiplierQR2, double multiplierQR3)
		{
			return indicator.amaCurrentWeekVWAP(input, sessionType, bandType, customTZSelector, s_CustomSessionStart, s_CustomSessionEnd, multiplierSD1, multiplierSD2, multiplierSD3, multiplierQR1, multiplierQR2, multiplierQR3);
		}
	}
}

#endregion
