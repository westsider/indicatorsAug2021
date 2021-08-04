//+----------------------------------------------------------------------------------------------+
//| Copyright © <2020>  <LizardIndicators.com - powered by AlderLab UG>
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
#endregion

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.LizardIndicators
{
	/// <summary>
	/// The SuperTrend M11 is a trailing stop which can also be used as a trend filter. The long stop is calculated by subtracting a multiple of the average true range from a moving median. 
	/// The short stop is calculated by adding a multiple of the average true range to the moving median. The trend information is exposed through a public data series and can be directly 
	/// accessed via an automated strategy or or a market analyzer column."; 
	/// </summary>
	[Gui.CategoryOrder("Input Parameters", 1000100)]
	[Gui.CategoryOrder("Display Options", 1000200)]
	[Gui.CategoryOrder("Paint Bars", 8000100)]
	[Gui.CategoryOrder("Sound Alerts", 8000200)]
	[Gui.CategoryOrder("Version", 8000300)]
	public class amaSuperTrendM11 : Indicator
	{
		private int 						basePeriod					= 8; 
        private int 						rangePeriod					= 15; 
		private int							displacement				= 0;
		private int							totalBarsRequiredToPlot		= 0;
		private int							shift						= 0;
		private double 						multiplier 					= 2.5; 
		private bool						reverseIntraBar				= false;
		private bool 						showTriangles 				= true;
		private bool 						showPaintBars 				= true;
		private bool						showStopDots				= true;
		private bool 						showStopLine				= true;
		private bool						soundAlerts					= false;
		private bool 						gap0						= false;
		private bool 						gap1						= false;
		private bool						stoppedOut					= false;
		private bool						drawTriangleUp				= false;
		private bool						drawTriangleDown			= false;
		private bool						calculateFromPriceData		= true;
		private bool						indicatorIsOnPricePanel		= true;
		private double						median						= 0.0;
		private double 						offset						= 0.0;
		private double						trailingAmount				= 0.0;
		private double						low							= 0.0;
		private double						high						= 0.0;
		private double						labelOffset					= 0.0;
		private SessionIterator				sessionIterator				= null;
		private int 						plot0Width 					= 2;
		private PlotStyle 					plot0Style					= PlotStyle.Dot;
		private int 						plot1Width 					= 1;
		private PlotStyle 					plot1Style					= PlotStyle.Line;
		private Brush						upBrush						= Brushes.Blue;
		private Brush						downBrush					= Brushes.Red;
		private Brush						upBrushUp					= Brushes.Blue;
		private Brush						upBrushDown					= Brushes.LightSkyBlue;
		private Brush						downBrushUp					= Brushes.LightCoral;
		private Brush						downBrushDown				= Brushes.Red;
		private Brush						upBrushOutline				= Brushes.Black;
		private Brush						downBrushOutline			= Brushes.Black;
		private Brush						alertBackBrush				= Brushes.Black;
		private Brush						errorBrush					= Brushes.Black;
		private SimpleFont					dotFont						= null;
		private SimpleFont					triangleFont				= null;
		private SimpleFont					errorFont					= null;
		private int							triangleFontSize			= 10;
		private string						dotString					= "n";
		private string						triangleStringUp			= "5";
		private string						triangleStringDown			= "6";
		private string						errorText					= "The amaSuperTrendM11 cannot be used with a negative displacement";
		private int							rearmTime					= 30;
		private string 						newUptrend					= "newuptrend.wav";
		private string 						newDowntrend				= "newdowntrend.wav";
		private string 						potentialUptrend			= "potentialuptrend.wav";
		private string 						potentialDowntrend			= "potentialdowntrend.wav";
		private string						pathNewUptrend				= "";
		private string						pathNewDowntrend			= "";
		private string						pathPotentialUptrend		= "";
		private string						pathPotentialDowntrend		= "";
		private string						versionString				= "v 2.6  -  March 9, 2020";
		private Series<double>				preliminaryTrend;
		private Series<double>				trend;
		private Series<double>				currentStopLong;
		private Series<double>				currentStopShort;
		private amaMovingMedian				baseline;
		private amaATR						offsetSeries;
		private ATR							barVolatility;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\nThe SuperTrend M11 is a trailing stop which can also be used as a trend filter. The long stop is calculated by subtracting a multiple "
											  + "of the average true range from a moving median. The short stop is calculated by adding a multiple of the average true range to the moving median."
											  + "The trend information is exposed through a public data series and can be directly accessed via an automated strategy or a market analyzer column."; 
				Name						= "amaSuperTrendM11";
				Calculate					= Calculate.OnPriceChange;
				IsSuspendedWhileInactive	= false;
				IsOverlay					= true;
				ArePlotsConfigurable		= false;
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Dot, "StopDot");	
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "StopLine");
				AddPlot(new Stroke(Brushes.Transparent, 2), PlotStyle.Dot, "ReverseDot");
			}
			else if (State == State.Configure)
			{
				displacement = Displacement;
				BarsRequiredToPlot = Math.Max(basePeriod, 6*rangePeriod);
				totalBarsRequiredToPlot = BarsRequiredToPlot + displacement;
				if(Calculate == Calculate.OnBarClose && !reverseIntraBar)
					shift = displacement + 1;
				else
					shift = displacement;
				Plots[0].PlotStyle = plot0Style;
				Plots[0].Width = plot0Width;
				Plots[1].PlotStyle = plot1Style;
				Plots[1].Width = plot1Width;
				Plots[2].PlotStyle = plot0Style;
				Plots[2].Width = plot0Width;
			}
			else if (State == State.DataLoaded)
			{
				preliminaryTrend = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				trend = new Series<double>(this, MaximumBarsLookBack.Infinite);
				currentStopLong = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				currentStopShort = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				baseline = amaMovingMedian(Input, basePeriod);
				offsetSeries = amaATR(Inputs[0], amaATRCalcMode.Wilder, rangePeriod);
				barVolatility = ATR(Closes[0], 256);
				if(Input is PriceSeries)
					calculateFromPriceData = true;
				else
					calculateFromPriceData = false;
		    	sessionIterator = new SessionIterator(Bars);
			}
			else if(State == State.Historical)
			{	
				if(ChartBars != null)
				{	
					errorBrush = ChartControl.Properties.AxisPen.Brush;
					errorBrush.Freeze();
					errorFont = new SimpleFont("Arial", 24);
					indicatorIsOnPricePanel = (ChartPanel.PanelIndex == ChartBars.Panel);
				}	
				else
					indicatorIsOnPricePanel = false;
				gap0 = (plot0Style == PlotStyle.Line)||(plot0Style == PlotStyle.Square);
				gap1 = (plot1Style == PlotStyle.Line)||(plot1Style == PlotStyle.Square);
				dotFont = new SimpleFont("Webdings", plot1Width + 2);
				triangleFont = new SimpleFont("Webdings", 3*triangleFontSize);
				pathNewUptrend = string.Format( @"{0}sounds\{1}", NinjaTrader.Core.Globals.InstallDir, newUptrend);
				pathNewDowntrend = string.Format( @"{0}sounds\{1}", NinjaTrader.Core.Globals.InstallDir, newDowntrend);
				pathPotentialUptrend = string.Format( @"{0}sounds\{1}", NinjaTrader.Core.Globals.InstallDir, potentialUptrend);
				pathPotentialDowntrend = string.Format( @"{0}sounds\{1}", NinjaTrader.Core.Globals.InstallDir, potentialDowntrend);
			}	
		}

		protected override void OnBarUpdate()
        {
			if(displacement < 0)
			{
				DrawOnPricePanel = false;
				Draw.TextFixed(this, "error text", errorText, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);  
				return;
			}
			
			if(CurrentBar < 2)
			{
				if(IsFirstTickOfBar)
				{
					preliminaryTrend[0] = 1.0;
					trend[0] = 1.0;
					if(CurrentBar == 0)
					{
						currentStopLong.Reset();
						currentStopShort.Reset();
					}
					else if(CurrentBar == 1)
					{
						median = baseline[1];
						offset = Math.Max(TickSize, offsetSeries[1]);
						trailingAmount = multiplier * offset;
						currentStopLong[0] = median - trailingAmount;
						currentStopShort[0] = median + trailingAmount;
					}
					StopDot.Reset();
					StopLine.Reset();
					Values[2].Reset();
					PlotBrushes[0][0] = Brushes.Transparent;
					PlotBrushes[1][0] = Brushes.Transparent;
					PlotBrushes[2][0] = Brushes.Transparent;
				}
				return;
			}
				
			if (IsFirstTickOfBar)
			{
				median = baseline[1];
				offset = Math.Max(TickSize, offsetSeries[1]);
				trailingAmount = multiplier * offset;
				labelOffset = 0.3 * barVolatility[1];
				if(preliminaryTrend[1] > 0.5)
				{
					currentStopShort[0] = median + trailingAmount;
					if(preliminaryTrend[2] > 0.5)
						currentStopLong[0] = Math.Max(currentStopLong[1], median - trailingAmount);
					else
						currentStopLong[0] = Math.Max(Values[2][1], median - trailingAmount); 
					StopDot[0] = currentStopLong[0];
					StopLine[0] = currentStopLong[0];
					Values[2][0] = currentStopShort[0];
					if(showStopDots)
					{	
						if(gap0 && preliminaryTrend[2] < -0.5)
							PlotBrushes[0][0]= Brushes.Transparent;
						else
							PlotBrushes[0][0] = upBrush;
					}
					else
						PlotBrushes[0][0]= Brushes.Transparent;
					if(showStopLine)
					{	
						if(gap1 && preliminaryTrend[2] < -0.5)
							PlotBrushes[1][0]= Brushes.Transparent;
						else
							PlotBrushes[1][0] = upBrush;
					}
					else
						PlotBrushes[1][0]= Brushes.Transparent;
				}
				else	
				{	
					currentStopLong[0] = median - trailingAmount;
					if(preliminaryTrend[2] < -0.5)
						currentStopShort[0] = Math.Min(currentStopShort[1], median + trailingAmount);
					else
						currentStopShort[0] = Math.Min(Values[2][1], median + trailingAmount);
					StopDot[0] = currentStopShort[0];
					StopLine[0] = currentStopShort[0];
					Values[2][0] = currentStopLong[0];
					if(showStopDots)
					{	
						if(gap0 && preliminaryTrend[2] > 0.5)
							PlotBrushes[0][0]= Brushes.Transparent;
						else
							PlotBrushes[0][0] = downBrush;
					}
					else
						PlotBrushes[0][0]= Brushes.Transparent;
					if(showStopLine)
					{	
						if(gap1 && preliminaryTrend[2] > 0.5)
							PlotBrushes[1][0]= Brushes.Transparent;
						else
							PlotBrushes[1][0] = downBrush;
					}
					else
						PlotBrushes[1][0]= Brushes.Transparent;
				}
				if(showStopLine && CurrentBar >= BarsRequiredToPlot)
				{	
					DrawOnPricePanel = false;
					if(plot1Style == PlotStyle.Line && reverseIntraBar) 
					{
						if(preliminaryTrend[1] > 0.5 && preliminaryTrend[2] < -0.5)
							Draw.Line(this, "line" + CurrentBar, false, 1-displacement, Values[2][1], -displacement, StopLine[0], upBrush, DashStyleHelper.Solid, plot1Width);
						else if(preliminaryTrend[1] < -0.5 && preliminaryTrend[2] > 0.5)
							Draw.Line(this, "line" + CurrentBar, false, 1-displacement, Values[2][1], -displacement, StopLine[0], downBrush, DashStyleHelper.Solid, plot1Width);
					}
					else if(plot1Style == PlotStyle.Square && !showStopDots) 
					{
						if(trend[1] > 0.5 && trend[2] < -0.5)
							Draw.Text(this, "dot" + CurrentBar, false, dotString, -displacement, StopLine[0], 0 , upBrush, dotFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0); 
						else if(trend[1] < -0.5 && trend[2] > 0.5)
							Draw.Text(this, "dot" + CurrentBar, false, dotString, -displacement, StopLine[0], 0 , downBrush, dotFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0); 
					}
				}
				
				if(showPaintBars && CurrentBar >= BarsRequiredToPlot)
				{
					if (preliminaryTrend[1] > 0.5)
					{	
						if(Open[0] < Close[0])
							BarBrushes[-displacement] = upBrushUp;
						else
							BarBrushes[-displacement] = upBrushDown;
						CandleOutlineBrushes[-displacement] = upBrushOutline;
					}	
					else
					{	
						if(Open[0] < Close[0])
							BarBrushes[-displacement] = downBrushUp;
						else
							BarBrushes[-displacement] = downBrushDown;
						CandleOutlineBrushes[-displacement] = downBrushOutline;
					}
				}
				stoppedOut = false;
			}
			
			if(reverseIntraBar) // only one trend change per bar is permitted
			{
				if(!stoppedOut)
				{
					if (preliminaryTrend[1] > 0.5 && Low[0] < currentStopLong[0])
					{
						preliminaryTrend[0] = -1.0;
						stoppedOut = true;
						if(showStopDots && !gap0)
							PlotBrushes[2][0] = downBrush;
					}	
					else if (preliminaryTrend[1] < -0.5 && High[0] > currentStopShort[0])
					{
						preliminaryTrend[0] = 1.0;
						stoppedOut = true;
						if(showStopDots && !gap0)
							PlotBrushes[2][0] = upBrush;
					}
					else
						preliminaryTrend[0] = preliminaryTrend[1];
				}
			}
			else 
			{
				if (preliminaryTrend[1] > 0.5 && Input[0] < currentStopLong[0])
					preliminaryTrend[0] = - 1.0;
				else if (preliminaryTrend[1] < -0.5 && Input[0] > currentStopShort[0])
					preliminaryTrend[0] = 1.0;
				else
					preliminaryTrend[0] = preliminaryTrend[1];
			}
					
			// this information can be accessed by a strategy - trend holds the confirmed trend, whereas preliminaryTrend may hold a preliminary trend only
			if(Calculate == Calculate.OnBarClose)
				trend[0] = preliminaryTrend[0];
			else if(IsFirstTickOfBar && !reverseIntraBar)
				trend[0] = preliminaryTrend[1];
			else if(reverseIntraBar)
				trend[0] = preliminaryTrend[0];
			
			if(CurrentBar > totalBarsRequiredToPlot)
			{
				if(showPaintBars)
				{
					if(trend[shift] > 0)
					{
						if(Open[0] < Close[0])
							BarBrushes[0] = upBrushUp;
						else
							BarBrushes[0] = upBrushDown;
						CandleOutlineBrushes[0] = upBrushOutline;
					}
					else
					{	
						if(Open[0] < Close[0])
							BarBrushes[0] = downBrushUp;
						else
							BarBrushes[0] = downBrushDown;
						CandleOutlineBrushes[0] = downBrushOutline;
					}
				}
				if(showTriangles)
				{
					DrawOnPricePanel = true;
					if(Calculate == Calculate.OnBarClose)
					{	
						if(trend[displacement] > 0.5 && trend[displacement+1] < -0.5)
							Draw.Text(this, "triangle" + CurrentBar, false, triangleStringUp, 0, Low[0] - labelOffset, -triangleFontSize, upBrush, triangleFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0); 
						else if(trend[displacement] < -0.5 && trend[displacement+1] > 0.5)
							Draw.Text(this, "triangle" + CurrentBar, false, triangleStringDown, 0, High[0] + labelOffset, triangleFontSize, downBrush, triangleFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
					}
					else if (reverseIntraBar)
					{
						if(IsFirstTickOfBar)
						{
							drawTriangleUp = false;
							drawTriangleDown = false;
						}	
						if(!drawTriangleUp && trend[displacement] > 0.5 && trend[displacement+1] < -0.5)
						{	
							low = Low[0];
							Draw.Text(this, "triangle" + CurrentBar, false, triangleStringUp, 0, low - labelOffset, -triangleFontSize, upBrush, triangleFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0); 
							drawTriangleUp = true;
						}	
						else if(drawTriangleUp && (IsFirstTickOfBar || Low[0] < low))
						{	
							low = Low[0];
							Draw.Text(this, "triangle" + CurrentBar, false, triangleStringUp, 0, low - labelOffset, -triangleFontSize, upBrush, triangleFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0); 
						}
						if(!drawTriangleDown && trend[displacement] < -0.5 && trend[displacement+1] > 0.5)
						{	
							drawTriangleDown = true;
							high = High[0];
							Draw.Text(this, "triangle" + CurrentBar, false, triangleStringDown, 0, high + labelOffset, triangleFontSize, downBrush, triangleFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
						}	
						else if(drawTriangleDown && (IsFirstTickOfBar || High[0] > high))
						{	
							high = High[0];
							Draw.Text(this, "triangle" + CurrentBar, false, triangleStringDown, 0, high + labelOffset, triangleFontSize, downBrush, triangleFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
						}
					}	
					else if (IsFirstTickOfBar)	
					{
						if(trend[displacement] > 0.5 && trend[displacement+1] < -0.5)
							Draw.Text(this, "triangle" + CurrentBar, false, triangleStringUp, 1, Low[1] - labelOffset, -triangleFontSize, upBrush, triangleFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0); 
						else if(trend[displacement] < -0.5 && trend[displacement+1] > 0.5)
							Draw.Text(this, "triangle" + CurrentBar, false, triangleStringDown, 1, High[1] + labelOffset, triangleFontSize, downBrush, triangleFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
					}	
				}	
			}
			
			if (soundAlerts && State == State.Realtime && IsConnected() && (Calculate == Calculate.OnBarClose || reverseIntraBar))
			{
				if(preliminaryTrend[0] > 0.5 && preliminaryTrend[1] < -0.5)		
				{
					try
					{
							Alert("New_Uptrend", Priority.Medium,"New Uptrend", pathNewUptrend, rearmTime, alertBackBrush, upBrush);
					}
					catch{}
				}
				else if(preliminaryTrend[0] < -0.5 && preliminaryTrend[1] > 0.5)	 
				{
					try
					{
							Alert("New_Downtrend", Priority.Medium,"New Downtrend", pathNewDowntrend, rearmTime, alertBackBrush, downBrush);
					}
					catch{}
				}
			}				
			if (soundAlerts && State == State.Realtime && IsConnected() && Calculate != Calculate.OnBarClose && !reverseIntraBar)
			{
				if(preliminaryTrend[0] > 0.5 && preliminaryTrend[1] < -0.5)		
				{
					try
					{
							Alert("Potential_Uptrend", Priority.Medium,"Potential Uptrend", pathPotentialUptrend, rearmTime, alertBackBrush, upBrush);
					}
					catch{}
				}
				else if(preliminaryTrend[0] < -0.5 && preliminaryTrend[1] > 0.5)	 
				{
					try
					{
							Alert("Potential_Downtrend", Priority.Medium,"Potential Downtrend", pathPotentialDowntrend, rearmTime, alertBackBrush, downBrush);
					}
					catch{}
				}
			}	
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> StopDot
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> StopLine
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Trend
		{
			get { return trend; }
		}
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Median period", Description = "Sets the lookback period for the median", GroupName = "Input Parameters", Order = 0)]
		public int BasePeriod
		{	
            get { return basePeriod; }
            set { basePeriod = value; }
		}
			
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ATR period", Description = "Sets the lookback period for the average true range", GroupName = "Input Parameters", Order = 1)]
		public int RangePeriod
		{	
            get { return rangePeriod; }
            set { rangePeriod = value; }
		}
			
		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ATR multiplier", Description = "Sets the multiplier for the average true range", GroupName = "Input Parameters", Order = 2)]
		public double Multiplier
		{	
            get { return multiplier; }
            set { multiplier = value; }
		}
			
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Reverse intra-bar", Description = "When set to 'true', reversal signals are triggered intra-bar", GroupName = "Input Parameters", Order = 3)]
        public bool ReverseIntraBar
        {
            get { return reverseIntraBar; }
            set { reverseIntraBar = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show stop dots", GroupName = "Display Options", Order = 0)]
        public bool ShowStopDots
        {
            get { return showStopDots; }
            set { showStopDots = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show stop line", GroupName = "Display Options", Order = 1)]
        public bool ShowStopLine
        {
            get { return showStopLine; }
            set { showStopLine = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show paint bars", GroupName = "Display Options", Order = 2)]
        public bool ShowPaintBars
        {
            get { return showPaintBars; }
            set { showPaintBars = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show triangles", GroupName = "Display Options", Order = 3)]
        public bool ShowTriangles
        {
            get { return showTriangles; }
            set { showTriangles = value; }
        }
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bullish color", Description = "Sets the color for the trailing stop", GroupName = "Plots", Order = 0)]
		public Brush UpBrush
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
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bearish color", Description = "Sets the color for the trailing stop", GroupName = "Plots", Order = 1)]
		public Brush DownBrush
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
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plotstyle stop dots", Description = "Sets the plot style for the stop dots", GroupName = "Plots", Order = 2)]
		public PlotStyle Plot0Style
		{	
            get { return plot0Style; }
            set { plot0Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dot size", Description = "Sets the size for the stop dots", GroupName = "Plots", Order = 3)]
		public int Plot0Width
		{	
            get { return plot0Width; }
            set { plot0Width = value; }
		}
			
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plotstyle stop line", Description = "Sets the plot style for the stop line", GroupName = "Plots", Order = 4)]
		public PlotStyle Plot1Style
		{	
            get { return plot1Style; }
            set { plot1Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Width stop line", Description = "Sets the width for the stop line", GroupName = "Plots", Order = 5)]
		public int Plot1Width
		{	
            get { return plot1Width; }
            set { plot1Width = value; }
		}
		
		[Range(1, 256)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Triangle size", Description = "Allows for adjusting the triangle size", GroupName = "Plots", Order = 6)]
		public int TriangleFontSize
		{	
            get { return triangleFontSize; }
            set { triangleFontSize = value; }
		}
				
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bullish Upclose", Description = "Sets the color for a bullish trend", GroupName = "Paint Bars", Order = 0)]
		public Brush UpBrushUp
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
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bullish Downclose", Description = "Sets the color for a bullish trend", GroupName = "Paint Bars", Order = 1)]
		public Brush UpBrushDown
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
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bullish candle outline", Description = "Sets the color for bullish candle outlines", GroupName = "Paint Bars", Order = 2)]
		public Brush UpBrushOutline
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
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bearish Upclose", Description = "Sets the color for a bearish trend", GroupName = "Paint Bars", Order = 3)]
		public Brush DownBrushUp
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
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bearish Downclose", Description = "Sets the color for a bearish trend", GroupName = "Paint Bars", Order = 4)]
		public Brush DownBrushDown
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
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bearish candle outline", Description = "Sets the color for bearish candle outlines", GroupName = "Paint Bars", Order = 5)]
		public Brush DownBrushOutline
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
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Sound alerts", GroupName = "Sound Alerts", Order = 0)]
        public bool SoundAlerts
        {
            get { return soundAlerts; }
            set { soundAlerts = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "New uptrend", Description = "Sound file for confirmed new uptrend", GroupName = "Sound Alerts", Order = 1)]
		public string NewUptrend
		{	
            get { return newUptrend; }
            set { newUptrend = value; }
		}		
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "New downtrend", Description = "Sound file for confirmed new downtrend", GroupName = "Sound Alerts", Order = 2)]
		public string NewDowntrend
		{	
            get { return newDowntrend; }
            set { newDowntrend = value; }
		}		
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Potential uptrend", Description = "Sound file for potential new uptrend", GroupName = "Sound Alerts", Order = 3)]
		public string PotentialUptrend
		{	
            get { return potentialUptrend; }
            set { potentialUptrend = value; }
		}		
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Potential downtrend", Description = "Sound file for potential new downtrend", GroupName = "Sound Alerts", Order = 4)]
		public string PotentialDowntrend
		{	
            get { return potentialDowntrend; }
            set { potentialDowntrend = value; }
		}				
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Rearm time", Description = "Rearm time for alerts in seconds", GroupName = "Sound Alerts", Order = 5)]
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
			set { ;}
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
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LizardIndicators.amaSuperTrendM11[] cacheamaSuperTrendM11;
		public LizardIndicators.amaSuperTrendM11 amaSuperTrendM11(int basePeriod, int rangePeriod, double multiplier, bool reverseIntraBar)
		{
			return amaSuperTrendM11(Input, basePeriod, rangePeriod, multiplier, reverseIntraBar);
		}

		public LizardIndicators.amaSuperTrendM11 amaSuperTrendM11(ISeries<double> input, int basePeriod, int rangePeriod, double multiplier, bool reverseIntraBar)
		{
			if (cacheamaSuperTrendM11 != null)
				for (int idx = 0; idx < cacheamaSuperTrendM11.Length; idx++)
					if (cacheamaSuperTrendM11[idx] != null && cacheamaSuperTrendM11[idx].BasePeriod == basePeriod && cacheamaSuperTrendM11[idx].RangePeriod == rangePeriod && cacheamaSuperTrendM11[idx].Multiplier == multiplier && cacheamaSuperTrendM11[idx].ReverseIntraBar == reverseIntraBar && cacheamaSuperTrendM11[idx].EqualsInput(input))
						return cacheamaSuperTrendM11[idx];
			return CacheIndicator<LizardIndicators.amaSuperTrendM11>(new LizardIndicators.amaSuperTrendM11(){ BasePeriod = basePeriod, RangePeriod = rangePeriod, Multiplier = multiplier, ReverseIntraBar = reverseIntraBar }, input, ref cacheamaSuperTrendM11);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaSuperTrendM11 amaSuperTrendM11(int basePeriod, int rangePeriod, double multiplier, bool reverseIntraBar)
		{
			return indicator.amaSuperTrendM11(Input, basePeriod, rangePeriod, multiplier, reverseIntraBar);
		}

		public Indicators.LizardIndicators.amaSuperTrendM11 amaSuperTrendM11(ISeries<double> input , int basePeriod, int rangePeriod, double multiplier, bool reverseIntraBar)
		{
			return indicator.amaSuperTrendM11(input, basePeriod, rangePeriod, multiplier, reverseIntraBar);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaSuperTrendM11 amaSuperTrendM11(int basePeriod, int rangePeriod, double multiplier, bool reverseIntraBar)
		{
			return indicator.amaSuperTrendM11(Input, basePeriod, rangePeriod, multiplier, reverseIntraBar);
		}

		public Indicators.LizardIndicators.amaSuperTrendM11 amaSuperTrendM11(ISeries<double> input , int basePeriod, int rangePeriod, double multiplier, bool reverseIntraBar)
		{
			return indicator.amaSuperTrendM11(input, basePeriod, rangePeriod, multiplier, reverseIntraBar);
		}
	}
}

#endregion
