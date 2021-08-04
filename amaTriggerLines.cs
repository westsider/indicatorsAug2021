////+----------------------------------------------------------------------------------------------+
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
	/// The Trigger Lines indicator is built from a linear regression indicator and an exponential moving average calculated from the regression indicator.
	/// The area between the two lines is colored. The color can be used as a trend indication.
	/// </summary>
	[Gui.CategoryOrder("Algorithmic Options", 1000100)]
	[Gui.CategoryOrder("Input Parameters", 1000200)]
	[Gui.CategoryOrder("Display Options", 1000300)]
	[Gui.CategoryOrder("Cloud", 8000100)]
	[Gui.CategoryOrder("Paint Bars", 8000200)]
	[Gui.CategoryOrder("Sound Alerts", 8000300)]
	[Gui.CategoryOrder("Version", 8000400)]
	public class amaTriggerLines : Indicator
	{
		private int 							triggerPeriod 				= 80;
		private int								avgPeriod					= 20;
		private int								displacement				= 0;
		private int								totalBarsRequiredToPlot		= 0;
		private int								areaOpacity					= 50;
		private int								barOpacity					= 40;
		private double							maxTrigger					= 0;
		private double							minTrigger					= 0;
		private bool							showTriggerLines			= true;
		private bool							fillArea					= true;
		private bool							showPaintBars				= true;
		private bool							soundAlerts					= false;
		private bool							areaIsFilled				= true;
		private bool							calculateFromPriceData		= true;
		private bool							indicatorIsOnPricePanel		= true;
		private amaTriggerTrendDefinition		trendDefinition				= amaTriggerTrendDefinition.Cross;
		private SessionIterator					sessionIterator				= null;
		private System.Windows.Media.Brush		triggerBrush				= Brushes.Navy;
		private System.Windows.Media.Brush		averageBrush				= Brushes.Navy;
		private System.Windows.Media.Brush		cloudBrushUpS				= Brushes.YellowGreen;
		private System.Windows.Media.Brush		cloudBrushDownS				= Brushes.Salmon;
		private System.Windows.Media.Brush		cloudBrushUp				= Brushes.Transparent;
		private System.Windows.Media.Brush		cloudBrushDown				= Brushes.Transparent;
		private System.Windows.Media.Brush		upBrushUp					= Brushes.LimeGreen;
		private System.Windows.Media.Brush		upBrushDown					= Brushes.DarkGreen;
		private System.Windows.Media.Brush		downBrushUp					= Brushes.DarkRed;
		private System.Windows.Media.Brush		downBrushDown				= Brushes.Red;
		private System.Windows.Media.Brush		upBrushOutline				= Brushes.Black;
		private System.Windows.Media.Brush		downBrushOutline			= Brushes.Black;
		private System.Windows.Media.Brush		alertBackBrush				= Brushes.Black;
		private int								plot0Width					= 2;
		private int								plot1Width					= 2;
		private PlotStyle						plot0Style					= PlotStyle.Line;
		private DashStyleHelper					dash0Style					= DashStyleHelper.DashDot;
		private PlotStyle						plot1Style					= PlotStyle.Line;
		private DashStyleHelper					dash1Style					= DashStyleHelper.Solid;
		private int								rearmTime					= 30;
		private string 							bullishCross				= "bullishtriggercross.wav";
		private string 							bearishCross				= "bearishtriggercross.wav";
		private string							pathBullishCross			= "";
		private string							pathBearishCross			= "";
		private string							versionString				= "v 1.8  -  February 26, 2021";
		private Series<double>					trend;
		private LinReg							trigger;
		private EMA								triggerAverage;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\nThe Trigger Lines indicator is built from a linear regression indicator and an exponential moving average calculated from the regression indicator."
												+ " The area between the two lines is colored. The color can be used as a trend indication."; 
				Name						= "amaTriggerLines";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				ArePlotsConfigurable		= false;
				AddPlot(new Stroke(Brushes.MediumBlue, 2), PlotStyle.Line, "Trigger");
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Line, "Trigger Average");
				SetZOrder(-2);
			}
			else if (State == State.Configure)
			{
				displacement = Displacement;
				BarsRequiredToPlot	= Math.Max(triggerPeriod + 3 * avgPeriod, -displacement);
				totalBarsRequiredToPlot = Math.Max(0, BarsRequiredToPlot + displacement);
				if(showTriggerLines)
				{	
					Plots[0].Brush = triggerBrush;
					Plots[1].Brush = averageBrush;
				}
				else
				{	
					Plots[0].Brush = Brushes.Transparent;
					Plots[1].Brush = Brushes.Transparent;
				}
				Plots[0].Width = plot0Width;
				Plots[0].PlotStyle = plot0Style;
				Plots[0].DashStyleHelper = dash0Style;			
				Plots[1].Width = plot1Width;
				Plots[1].PlotStyle = plot1Style;
				Plots[1].DashStyleHelper = dash1Style;
				cloudBrushUp = cloudBrushUpS.Clone();
				cloudBrushUp.Opacity = (float) areaOpacity/100.0;
				cloudBrushUp.Freeze();
				cloudBrushDown = cloudBrushDownS.Clone();
				cloudBrushDown.Opacity = (float) areaOpacity/100.0;
				cloudBrushDown.Freeze();
			}	
			else if (State == State.DataLoaded)
			{
				trend = new Series<double>(this, MaximumBarsLookBack.Infinite);
				trigger = LinReg(Input, triggerPeriod);
				triggerAverage = EMA(trigger, avgPeriod);
				if(Input is PriceSeries)
					calculateFromPriceData = true;
				else
					calculateFromPriceData = false;
		    	sessionIterator = new SessionIterator(Bars);
			}	
			else if (State == State.Historical)
			{
				areaIsFilled = fillArea && areaOpacity > 0;
				cloudBrushUp = cloudBrushUpS.Clone();
				cloudBrushUp.Opacity = (float) areaOpacity/100.0;
				cloudBrushUp.Freeze();
				cloudBrushDown = cloudBrushDownS.Clone();
				cloudBrushDown.Opacity = (float) areaOpacity/100.0;
				cloudBrushDown.Freeze();
				if(ChartBars != null)
					indicatorIsOnPricePanel = (ChartPanel.PanelIndex == ChartBars.Panel);
				else
					indicatorIsOnPricePanel = false;
				pathBullishCross = string.Format( @"{0}sounds\{1}", NinjaTrader.Core.Globals.InstallDir, bullishCross);
				pathBearishCross = string.Format( @"{0}sounds\{1}", NinjaTrader.Core.Globals.InstallDir, bearishCross);
			}
		}
		
		protected override void OnBarUpdate()
		{
			Trigger[0] = trigger[0];
			TriggerAverage[0] = triggerAverage[0];
			
			if(CurrentBar < BarsRequiredToPlot)
				trend[0] = 0.0;
			else if (trendDefinition == amaTriggerTrendDefinition.Cross)
			{
				if(Trigger[0] > TriggerAverage[0])
					trend[0] = 1.0;
				else if (Trigger[0] < TriggerAverage[0])
					trend[0] = -1.0;
				else
					trend[0] = trend[1];
			}
			else if (calculateFromPriceData)
			{
				maxTrigger = Math.Max(Trigger[0], TriggerAverage[0]);
				minTrigger = Math.Min(Trigger[0], TriggerAverage[0]);
				if(Close[0] > maxTrigger && Median[0] > maxTrigger && Close[0] > High[1] && Range()[0] >= Range()[1])
					trend[0] = 1.0;
				else if (Close[0] < minTrigger && Median[0] < minTrigger && Close[0] < Low[1] && Range()[0] >= Range()[1])
					trend[0] = -1.0;
				else
					trend[0] = trend[1];
			}
			else
			{
				maxTrigger = Math.Max(Trigger[0], TriggerAverage[0]);
				minTrigger = Math.Min(Trigger[0], TriggerAverage[0]);
				if(Input[0] > maxTrigger && Close[0] > High[1] && Range()[0] >= Range()[1])
					trend[0] = 1.0;
				else if (Input[0] < minTrigger && Close[0] < Low[1] && Range()[0] >= Range()[1])
					trend[0] = -1.0;
				else
					trend[0] = trend[1];
			}
			
			// Paint bars
			if(showPaintBars && CurrentBar >= totalBarsRequiredToPlot)
			{
				if(displacement >= 0)
				{
					if(trend[displacement] > 0)
					{
						if(Open[0] < Close[0])
							BarBrushes[0] = upBrushUp;
						else
							BarBrushes[0] = upBrushDown;
						CandleOutlineBrushes[0] = upBrushOutline;
					}	
					else if(trend[displacement] < 0)
					{
						if(Open[0] < Close[0])
							BarBrushes[0] = downBrushUp;
						else
							BarBrushes[0] = downBrushDown;
						CandleOutlineBrushes[0] = downBrushOutline;
					}	
				}
				else 
				{
					if(trend[0] > 0)
					{
						if(Open[-displacement] < Close[-displacement])
							BarBrushes[-displacement] = upBrushUp;
						else
							BarBrushes[-displacement] = upBrushDown;
						CandleOutlineBrushes[-displacement] = upBrushOutline;
					}	
					else if(trend[0] < 0)
					{
						if(Open[-displacement] < Close[-displacement])
							BarBrushes[-displacement] = downBrushUp;
						else
							BarBrushes[-displacement] = downBrushDown;
						CandleOutlineBrushes[-displacement] = downBrushOutline;
					}	
				}			
			}
				
			if (soundAlerts && State == State.Realtime && IsConnected())
			{
				if(trend[0] > 0.5 && trend[1] < 0.5)		
				{
					try
					{
							Alert("Bullish_Trigger_Cross", Priority.Medium,"Bullish Trigger Cross", pathBullishCross, rearmTime, alertBackBrush, upBrushUp);
					}
					catch{}
				}
				else if(trend[0] < -0.5 && trend[1] > -0.5)		
				{
					try
					{
							Alert("Bearish_Trigger_Cross", Priority.Medium,"Bearish Trigger Cross", pathBearishCross, rearmTime, alertBackBrush, downBrushDown);
					}
					catch{}
				}
 			}				
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Trigger
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> TriggerAverage
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Trend
		{
			get { return trend; }
		}
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Trend definition", Description = "Select trend definition for the paint bars", GroupName = "Algorithmic Options", Order = 0)]
		public amaTriggerTrendDefinition TrendDefinition
		{	
            get { return trendDefinition; }
            set { trendDefinition = value; }
		}
			
		[Range(2, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Fast trigger period", Description = "Sets the period for the fast trigger", GroupName = "Input Parameters", Order = 0)]
		public int TriggerPeriod
		{	
            get { return triggerPeriod; }
            set { triggerPeriod = value; }
		}
			
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Trigger average period", Description = "Sets the period for the trigger average", GroupName = "Input Parameters", Order = 1)]
		public int AvgPeriod
		{	
            get { return avgPeriod; }
            set { avgPeriod = value; }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show trigger lines", Description = "Shows the trigger line plots", GroupName = "Display Options", Order = 0)]
        public bool ShowTriggerLines
        {
            get { return showTriggerLines; }
            set { showTriggerLines = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Fill area between triggers", Description = "Fills the space between the two triggers", GroupName = "Display Options", Order = 1)]
        public bool FillArea
        {
            get { return fillArea; }
            set { fillArea = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show paint bars", Description = "Activates paint bars", GroupName = "Display Options", Order = 2)]
        public bool ShowPaintBars
        {
            get { return showPaintBars; }
            set { showPaintBars = value; }
        }
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Fast trigger", Description = "Sets the color for the fast trigger line", GroupName = "Plots", Order = 0)]
		public System.Windows.Media.Brush TriggerBrush
		{ 
			get {return triggerBrush;}
			set {triggerBrush = value;}
		}

		[Browsable(false)]
		public string TriggerBrushSerializable
		{
			get { return Serialize.BrushToString(triggerBrush); }
			set { triggerBrush = Serialize.StringToBrush(value); }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style trigger", Description = "Sets the dash style for the fast trigger line", GroupName = "Plots", Order = 1)]
		public DashStyleHelper Dash0Style
		{
			get { return dash0Style; }
			set { dash0Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width trigger", Description = "Sets the plot width for the fast trigger line", GroupName = "Plots", Order = 2)]
		public int Plot0Width
		{	
            get { return plot0Width; }
            set { plot0Width = value; }
		}
			
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Trigger Average", Description = "Sets the color for the trigger average", GroupName = "Plots", Order = 3)]
		public System.Windows.Media.Brush AverageBrush
		{ 
			get {return averageBrush;}
			set {averageBrush = value;}
		}

		[Browsable(false)]
		public string AverageBrushSerializable
		{
			get { return Serialize.BrushToString(averageBrush); }
			set { averageBrush = Serialize.StringToBrush(value); }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style average", Description = "Sets the dash style for the trigger average", GroupName = "Plots", Order = 4)]
		public DashStyleHelper Dash1Style
		{
			get { return dash1Style; }
			set { dash1Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width average", Description = "Sets the plot width for the trigger average", GroupName = "Plots", Order = 5)]
		public int Plot1Width
		{	
            get { return plot1Width; }
            set { plot1Width = value; }
		}
				
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bullish cloud", Description = "Sets the color for a bullish trend", GroupName = "Cloud", Order = 0)]
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
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bearish cloud", Description = "Sets the color for a bullish trend", GroupName = "Cloud", Order = 1)]
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
		
		[Range(0, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Area opacity", Description = "Sets the opacity for the cloud", GroupName = "Cloud", Order = 2)]
		public int AreaOpacity
		{	
            get { return areaOpacity; }
            set { areaOpacity = value; }
		}
		
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bullish Upclose", Description = "Sets the color for a bullish trend", GroupName = "Paint Bars", Order = 0)]
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
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bullish Downclose", Description = "Sets the color for a bullish trend", GroupName = "Paint Bars", Order = 1)]
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
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bearish Upclose", Description = "Sets the color for a bearish trend", GroupName = "Paint Bars", Order = 3)]
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
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bearish Downclose", Description = "Sets the color for a bearish trend", GroupName = "Paint Bars", Order = 4)]
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
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Sound alerts", GroupName = "Sound Alerts", Order = 0)]
        public bool SoundAlerts
        {
            get { return soundAlerts; }
            set { soundAlerts = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bullish trigger cross", Description = "Sound file for bullish trigger cross", GroupName = "Sound Alerts", Order = 1)]
		public string BullishCross
		{	
            get { return bullishCross; }
            set { bullishCross = value; }
		}		
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bearish trigger cross", Description = "Sound file for bearish trigger cross", GroupName = "Sound Alerts", Order = 2)]
		public string BearishCross
		{	
            get { return bearishCross; }
            set { bearishCross = value; }
		}		

		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Rearm time", Description = "Rearm time for alerts in seconds", GroupName = "Sound Alerts", Order = 3)]
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

			SharpDX.Direct2D1.Brush cloudBrushUpDX 	= cloudBrushUp.ToDxBrush(RenderTarget);
			SharpDX.Direct2D1.Brush cloudBrushDownDX = cloudBrushDown.ToDxBrush(RenderTarget);
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
			int lastX 				= 0;
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
				lastY1	= chartScale.GetYByValue(Trigger.GetValueAt(lastIndex - displacement));
				lastY2	= chartScale.GetYByValue(TriggerAverage.GetValueAt(lastIndex - displacement));
				
				//Area
				if(areaIsFilled)
				{	
					lastY = Math.Max(lastY1, lastY2);
					if(lastY1 > lastY2)
						lastSign = -1;
					else if (lastY1 < lastY2)
						lastSign = 1;
					else
						lastSign = 0;
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
								y1 = chartScale.GetYByValue(Trigger.GetValueAt(idx - displacement));   
								y2 = chartScale.GetYByValue(TriggerAverage.GetValueAt(idx - displacement));   
								count = count + 1;
								if(lastSign < 1 && y1 > y2)
								{
									y = Math.Max(y1,y2);
									sign = -1;
									cloudArray[count] = new Vector2(x,y);
									lastX = x;
									lastY1 = y1;
									lastY2 = y2;
									lastY = y;
									lastSign = -1;
									returnBar = idx;
									startBar = idx - 1;
								}
								else if(lastSign > -1 && y1 < y2)
								{
									y = Math.Max(y1,y2);
									cloudArray[count] = new Vector2(x,y);
									sign = 1;
									lastX = x;
									lastY1 = y1;
									lastY2 = y2;
									lastY = y;
									lastSign = 1;
									returnBar = idx;
									startBar = idx - 1;
								}
								else if (lastSign == 0 && y1 == y2)
								{
									y = y1;
									cloudArray[count] = new Vector2(x,y);
									sign = 0;
									lastX = x;
									lastY1 = y;
									lastY2 = y;
									lastY = y;
									lastSign = 0;
									returnBar = idx;
									startBar = idx - 1;
									break;
								}
								else if (y1 == y2)
								{
									y = y1;
									cloudArray[count] = new Vector2(x,y);
									sign = lastSign;
									lastX = x;
									lastY1 = y;
									lastY2 = y;
									lastY = y;
									lastSign = 0;
									returnBar = idx + 1;
									startBar = idx - 1;
									break;
								}
								else
								{	
									double lastDiff = Convert.ToDouble(lastY2 - lastY1);
									double diff = Convert.ToDouble(y2 - y1);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = Convert.ToInt32((lastY2 * y1 - y2 * lastY1)/denominator);
									cloudArray[count] = new Vector2(x,y);
									sign = lastSign;
									lastX = x;
									lastY1 = y;
									lastY2 = y;
									lastY = y;
									lastSign = 0;
									returnBar = idx + 1;
									startBar = idx;
									break;
								}
								startBar = idx - 1;
							}
							for (int idx = returnBar ; idx <= priorStartBar; idx ++)	
							{
								if(nonEquidistant && displacement > 0 && idx > lastBarCounted)
									x = ChartControl.GetXByBarIndex(ChartBars, lastBarCounted) + (int)((idx - lastBarCounted) * barWidth);
								else
									x = ChartControl.GetXByBarIndex(ChartBars, idx);
								y1 = chartScale.GetYByValue(Trigger.GetValueAt(idx - displacement));   
								y2 = chartScale.GetYByValue(TriggerAverage.GetValueAt(idx - displacement));
								y = Math.Min(y1, y2);
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
							RenderTarget.AntialiasMode = oldAntialiasMode;
						}
						path.Dispose();
						sink.Dispose();
						firstLoop = false;
					}	
					while (startBar > firstBarIndex - 1);
				}
				
				//Plots
				if(showTriggerLines) 
				{	
					SharpDX.Direct2D1.PathGeometry 	pathT;
					SharpDX.Direct2D1.GeometrySink 	sinkT;
					pathT = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
					using (pathT)
					{
						startBar = lastIndex;
						count = -1;
						for (int idx = startBar; idx >= firstBarIndex; idx --)	
						{
							if(nonEquidistant && idx > lastBarCounted)
								x = ChartControl.GetXByBarIndex(ChartBars, lastBarCounted) + (int)((idx - lastBarCounted) * barWidth);
							else
								x = ChartControl.GetXByBarIndex(ChartBars, idx);
							y = chartScale.GetYByValue(Trigger.GetValueAt(idx - displacement));   
							count = count + 1;
							plotArray[count] = new Vector2(x, y);
						}
						sinkT = pathT.Open();
						sinkT.BeginFigure(plotArray[0], FigureBegin.Filled);
						for (int i = 1; i <= count; i++)
							sinkT.AddLine(plotArray[i]);
						sinkT.EndFigure(FigureEnd.Open);
		        		sinkT.Close();
						RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
						RenderTarget.DrawGeometry(pathT, Plots[0].BrushDX, Plots[0].Width, Plots[0].StrokeStyle);
						RenderTarget.AntialiasMode = oldAntialiasMode;
					}
					pathT.Dispose();
					sinkT.Dispose();
					
					SharpDX.Direct2D1.PathGeometry 	pathTA;
					SharpDX.Direct2D1.GeometrySink 	sinkTA;
					pathTA = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
					using (pathTA)
					{
						startBar = lastIndex;
						count = -1;
						for (int idx = startBar; idx >= firstBarIndex; idx --)	
						{
							if(nonEquidistant && idx > lastBarCounted)
								x = ChartControl.GetXByBarIndex(ChartBars, lastBarCounted) + (int)((idx - lastBarCounted) * barWidth);
							else
								x = ChartControl.GetXByBarIndex(ChartBars, idx);
							y = chartScale.GetYByValue(TriggerAverage.GetValueAt(idx - displacement));   
							count = count + 1;
							plotArray[count] = new Vector2(x, y);
						}
						sinkTA = pathTA.Open();
						sinkTA.BeginFigure(plotArray[0], FigureBegin.Filled);
						for (int i = 1; i <= count; i++)
							sinkTA.AddLine(plotArray[i]);
						sinkTA.EndFigure(FigureEnd.Open);
		        		sinkTA.Close();
						RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
						RenderTarget.DrawGeometry(pathTA, Plots[1].BrushDX, Plots[1].Width, Plots[1].StrokeStyle);
						RenderTarget.AntialiasMode = oldAntialiasMode;
					}
					pathTA.Dispose();
					sinkTA.Dispose();
				}
			}			
			cloudBrushUpDX.Dispose();
			cloudBrushDownDX.Dispose();
		}
		#endregion
	}
}

#region Global Enums

public enum amaTriggerTrendDefinition {Cross, Thrust}

#endregion

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LizardIndicators.amaTriggerLines[] cacheamaTriggerLines;
		public LizardIndicators.amaTriggerLines amaTriggerLines(amaTriggerTrendDefinition trendDefinition, int triggerPeriod, int avgPeriod)
		{
			return amaTriggerLines(Input, trendDefinition, triggerPeriod, avgPeriod);
		}

		public LizardIndicators.amaTriggerLines amaTriggerLines(ISeries<double> input, amaTriggerTrendDefinition trendDefinition, int triggerPeriod, int avgPeriod)
		{
			if (cacheamaTriggerLines != null)
				for (int idx = 0; idx < cacheamaTriggerLines.Length; idx++)
					if (cacheamaTriggerLines[idx] != null && cacheamaTriggerLines[idx].TrendDefinition == trendDefinition && cacheamaTriggerLines[idx].TriggerPeriod == triggerPeriod && cacheamaTriggerLines[idx].AvgPeriod == avgPeriod && cacheamaTriggerLines[idx].EqualsInput(input))
						return cacheamaTriggerLines[idx];
			return CacheIndicator<LizardIndicators.amaTriggerLines>(new LizardIndicators.amaTriggerLines(){ TrendDefinition = trendDefinition, TriggerPeriod = triggerPeriod, AvgPeriod = avgPeriod }, input, ref cacheamaTriggerLines);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaTriggerLines amaTriggerLines(amaTriggerTrendDefinition trendDefinition, int triggerPeriod, int avgPeriod)
		{
			return indicator.amaTriggerLines(Input, trendDefinition, triggerPeriod, avgPeriod);
		}

		public Indicators.LizardIndicators.amaTriggerLines amaTriggerLines(ISeries<double> input , amaTriggerTrendDefinition trendDefinition, int triggerPeriod, int avgPeriod)
		{
			return indicator.amaTriggerLines(input, trendDefinition, triggerPeriod, avgPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaTriggerLines amaTriggerLines(amaTriggerTrendDefinition trendDefinition, int triggerPeriod, int avgPeriod)
		{
			return indicator.amaTriggerLines(Input, trendDefinition, triggerPeriod, avgPeriod);
		}

		public Indicators.LizardIndicators.amaTriggerLines amaTriggerLines(ISeries<double> input , amaTriggerTrendDefinition trendDefinition, int triggerPeriod, int avgPeriod)
		{
			return indicator.amaTriggerLines(input, trendDefinition, triggerPeriod, avgPeriod);
		}
	}
}

#endregion
