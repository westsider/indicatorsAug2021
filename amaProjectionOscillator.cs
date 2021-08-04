//+----------------------------------------------------------------------------------------------+
//| Copyright © <2021>  <LizardTrader.com - powered by AlderLab UG>
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

//This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.LizardIndicators
{
	/// <summary>
	/// The Projection Oscillator is based on the Projection Bands which were presented by Mel Widner in the July 1995 issue of "Stocks and Commodities". The Projection Oscillator
	/// calculates the position of the close relative to the Projection Bands and returns a percentage between 0 and 100. It can be compared to the raw Stochastics, which calculates the 
	/// position of the close relative to the highest high and lowest low within the lookback period. This version of the indicator allows for selecting a indicator as input series.
	/// When a slow moving average is selected as input series the Projection Oscillator changes its characteristics and behaves similar to a digital filter.
	/// </summary>
	[Gui.CategoryOrder("Input Parameters", 1000100)]
	[Gui.CategoryOrder("Threshold Values", 1000200)]
	[Gui.CategoryOrder("Display Options", 1000300)]
	[Gui.CategoryOrder("Projection Oscillator", 8000100)]
	[Gui.CategoryOrder("Signal Line", 8000200)]
	[Gui.CategoryOrder("Version", 8000300)]
	public class amaProjectionOscillator : Indicator
	{
		private int								oscillatorPeriod				= 28;
		private int								signalPeriod					= 7;
		private int								lookback						= 0;
		private int								displacement					= 0;
		private int								totalBarsRequiredToPlot			= 0;
		private double							sum_X							= 0;
		private double							sum_XX							= 0;
		private double							sum_Y							= 0;
		private double							preSum_Y						= 0;
		private double							sum_XY							= 0;
		private double							mean_X							= 0.0;
		private double							mean_Y							= 0.0;
		private double							variance						= 0.0;
		private double							covariance						= 0.0;
		private double							slope							= 0.0;
		private double							intercept						= 0.0;
		private double							regressionValue					= 0.0;
		private double							upperResidual					= 0.0;
		private double							lowerResidual					= 0.0;
		private double							upperMaxDeviation				= 0.0;
		private double							lowerMaxDeviation				= 0.0;
		private double							midBand							= 0.0;
		private double							upperBand						= 0.0;
		private double							lowerBand						= 0.0;
		private double							num								= 0.0;
		private double							den								= 0.0;
		private double							upperlineValue					= 80;
		private double							midlineValue					= 50;
		private double							lowerlineValue					= 20;
		private bool							showSignal						= true;
		private bool							showOscillator					= true;
		private bool							showSignalShading				= true;
		private bool							showOscillatorShading			= true;
		private bool							applySignalShading				= true;
		private bool							applyOscillatorShading			= true;
		private bool							calculateFromPriceData			= true;
		private bool							errorMessage					= true;
		private System.Windows.Media.Brush		pOverboughtBrush				= Brushes.Navy;
		private System.Windows.Media.Brush		pOversoldBrush					= Brushes.Navy;
		private System.Windows.Media.Brush		pNeutralBrush					= Brushes.Navy;
		private System.Windows.Media.Brush		sOverboughtBrush				= Brushes.DarkSlateGray;
		private System.Windows.Media.Brush		sOversoldBrush					= Brushes.DarkSlateGray;
		private System.Windows.Media.Brush		sNeutralBrush					= Brushes.DarkSlateGray;
		private System.Windows.Media.Brush		sOverboughtAreaBrush			= Brushes.DarkGreen;
		private System.Windows.Media.Brush		sOversoldAreaBrush				= Brushes.DarkRed;
		private System.Windows.Media.Brush		pOverboughtAreaBrush			= Brushes.LawnGreen;
		private System.Windows.Media.Brush		pOversoldAreaBrush				= Brushes.Red;
		private System.Windows.Media.Brush		sOverboughtAreaBrushOpaque		= null;
		private System.Windows.Media.Brush		sOversoldAreaBrushOpaque		= null;
		private System.Windows.Media.Brush		pOverboughtAreaBrushOpaque		= null;
		private System.Windows.Media.Brush		pOversoldAreaBrushOpaque		= null;
		private System.Windows.Media.Brush		upperlineBrush					= Brushes.DarkGreen;
		private System.Windows.Media.Brush		midlineBrush					= Brushes.DarkSlateGray;
		private System.Windows.Media.Brush		lowerlineBrush					= Brushes.DarkRed;
		private	SharpDX.Direct2D1.Brush 		sOverboughtBrushDX;
		private	SharpDX.Direct2D1.Brush 		sOversoldBrushDX;
		private	SharpDX.Direct2D1.Brush 		sNeutralBrushDX;
		private	SharpDX.Direct2D1.Brush 		pOverboughtBrushDX;
		private	SharpDX.Direct2D1.Brush 		pOversoldBrushDX;
		private	SharpDX.Direct2D1.Brush 		pNeutralBrushDX;
		private	SharpDX.Direct2D1.Brush 		sOverboughtAreaBrushDX;
		private	SharpDX.Direct2D1.Brush 		sOversoldAreaBrushDX;
		private	SharpDX.Direct2D1.Brush 		pOverboughtAreaBrushDX;
		private	SharpDX.Direct2D1.Brush 		pOversoldAreaBrushDX;
		private	SharpDX.Direct2D1.Brush 		upperlineBrushDX;
		private	SharpDX.Direct2D1.Brush 		midlineBrushDX;
		private	SharpDX.Direct2D1.Brush 		lowerlineBrushDX;
		private System.Windows.Media.Brush		errorBrush						= Brushes.Black;
		private SimpleFont						errorFont						= null;
		private string							errorText1						= "The amaProjectionOscillator indicator cannot be used with a regression period smaller than 2.";
		private string							errorText2						= "The amaProjectionOscillator indicator cannot be used with a negative displacement.";
		private PlotStyle						plot0Style						= PlotStyle.Line;
		private DashStyleHelper					dash0Style						= DashStyleHelper.Solid;
		private int								plot0Width						= 2;
		private PlotStyle						plot1Style						= PlotStyle.Line;
		private DashStyleHelper					dash1Style						= DashStyleHelper.Dash;
		private int								plot1Width						= 2;
		private DashStyleHelper					upperlineStyle					= DashStyleHelper.Solid;
		private int								upperlineWidth					= 1;
		private DashStyleHelper					midlineStyle					= DashStyleHelper.Dash;
		private int								midlineWidth					= 1;
		private DashStyleHelper					lowerlineStyle					= DashStyleHelper.Solid;
		private int								lowerlineWidth					= 1;
		private int								oscillatorOpacity				= 70;
		private int								signalOpacity					= 100;
		private string							versionString					= "v 1.4  -  February 26, 2021";
		private Series<double> 					oscillatorTrend;
		private Series<double> 					signalTrend;
		private EMA								smoothedOscillator;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "The Projection Oscillator is based on the Projection Bands which were presented by Mel Widner in the July 1995 issue of Stocks and Commodities."
												+ " The Projection Oscillator calculates the position of the close relative to the Projection Bands and returns a percentage between 0 and 100."
												+ " It can be compared to the raw Stochastics, which calculates the position of the close relative to the highest high and lowest low within the lookback period."
												+ " This version of the indicator allows for selecting a indicator as input series. When a slow moving average is selected as input series"
												+ " the Projection Oscillator changes its characteristics and behaves similar to a digital filter.";
				Name						= "amaProjectionOscillator";
				Calculate					= Calculate.OnPriceChange;
				IsSuspendedWhileInactive	= true;
				ArePlotsConfigurable		= false;
				AreLinesConfigurable		= false;
				AddPlot(new Stroke(Brushes.DarkSlateGray, 2), PlotStyle.Line, "Oscillator");
				AddPlot(new Stroke(Brushes.DarkSlateGray, 2), PlotStyle.Line, "Signal");
				AddLine(new Stroke(Brushes.DarkSlateGray, 1), 80, "Upper");
				AddLine(new Stroke(Brushes.DarkSlateGray, 1), 50, "Middle");
				AddLine(new Stroke(Brushes.DarkSlateGray, 1), 20, "Lower");
			}
			else if (State == State.Configure)
			{
				if(Calculate == Calculate.OnEachTick)
					Calculate = Calculate.OnPriceChange;
				displacement = Displacement;
				BarsRequiredToPlot = Math.Max(oscillatorPeriod + signalPeriod, -displacement);
				totalBarsRequiredToPlot = Math.Max(0, BarsRequiredToPlot + displacement);
				if(showOscillator)
				{	
					Plots[0].PlotStyle = plot0Style;
					Plots[0].DashStyleHelper = dash0Style;
					Plots[0].Width = plot0Width;
				}	
				if(showSignal)
				{	
					Plots[1].PlotStyle = plot1Style;
					Plots[1].DashStyleHelper = dash1Style;
					Plots[1].Width = plot1Width;
				}	
				Lines[0].Value = upperlineValue;
				Lines[0].Brush = upperlineBrush;
				Lines[0].DashStyleHelper = upperlineStyle;
				Lines[0].Width = upperlineWidth;
				Lines[1].Value = midlineValue;
				Lines[1].Brush = midlineBrush;
				Lines[1].DashStyleHelper = midlineStyle;
				Lines[1].Width = midlineWidth;
				Lines[2].Value = lowerlineValue;
				Lines[2].Brush = lowerlineBrush;
				Lines[2].DashStyleHelper = lowerlineStyle;
				Lines[2].Width = lowerlineWidth;
				pOverboughtAreaBrushOpaque = pOverboughtAreaBrush.Clone(); 
				pOverboughtAreaBrushOpaque.Opacity = (float) oscillatorOpacity/100.0;
				pOverboughtAreaBrushOpaque.Freeze();
				pOversoldAreaBrushOpaque = pOversoldAreaBrush.Clone(); 
				pOversoldAreaBrushOpaque.Opacity = (float) oscillatorOpacity/100.0;
				pOversoldAreaBrushOpaque.Freeze();
				sOverboughtAreaBrushOpaque = sOverboughtAreaBrush.Clone(); 
				sOverboughtAreaBrushOpaque.Opacity = (float)signalOpacity/100.0;
				sOverboughtAreaBrushOpaque.Freeze();
				sOversoldAreaBrushOpaque = sOversoldAreaBrush.Clone(); 
				sOversoldAreaBrushOpaque.Opacity = (float) signalOpacity/100.0;
				sOversoldAreaBrushOpaque.Freeze();
			}
 			else if (State == State.DataLoaded)
			{
				oscillatorTrend			= new Series<double>(this, MaximumBarsLookBack.Infinite);
				signalTrend				= new Series<double>(this, MaximumBarsLookBack.Infinite);
				smoothedOscillator		= EMA(Oscillator, signalPeriod);
				if(Input is PriceSeries)
					calculateFromPriceData = true;
				else
					calculateFromPriceData = false;
			}
			else if (State == State.Historical)
			{
				applyOscillatorShading = showOscillator && showOscillatorShading && oscillatorOpacity > 0;
				applySignalShading = showSignal && showSignalShading && signalOpacity > 0;
				pOverboughtAreaBrushOpaque = pOverboughtAreaBrush.Clone(); 
				pOverboughtAreaBrushOpaque.Opacity = (float) oscillatorOpacity/100.0;
				pOverboughtAreaBrushOpaque.Freeze();
				pOversoldAreaBrushOpaque = pOversoldAreaBrush.Clone(); 
				pOversoldAreaBrushOpaque.Opacity = (float) oscillatorOpacity/100.0;
				pOversoldAreaBrushOpaque.Freeze();
				sOverboughtAreaBrushOpaque = sOverboughtAreaBrush.Clone(); 
				sOverboughtAreaBrushOpaque.Opacity = (float)signalOpacity/100.0;
				sOverboughtAreaBrushOpaque.Freeze();
				sOversoldAreaBrushOpaque = sOversoldAreaBrush.Clone(); 
				sOversoldAreaBrushOpaque.Opacity = (float) signalOpacity/100.0;
				sOversoldAreaBrushOpaque.Freeze();
				if(ChartBars != null)
				{	
					errorBrush = ChartControl.Properties.AxisPen.Brush;
					errorBrush.Freeze();
					errorFont = new SimpleFont("Arial", 24);
				}
				errorMessage = false;
				if(oscillatorPeriod < 2)
				{
					Draw.TextFixed(this, "error text 1", errorText1, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);  
					errorMessage = true;
				}
				else if(displacement < 0)
				{
					Draw.TextFixed(this, "error text 2", errorText2, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);  
					errorMessage = true;
				}
			}
		}
		
		protected override void OnBarUpdate()
		{
			if(errorMessage)
				return;

			if(CurrentBar == 0)
			{
				if(calculateFromPriceData)
				{	
					num	= Close[0] - Low[0];
					den	= High[0] - Low[0];
					if (den.ApproxCompare(0) == 0)
						Oscillator[0] = 50;
					else
						Oscillator[0] = Math.Min(100, Math.Max(0, 100 * num/den));
					Signal[0] = smoothedOscillator[0];
					OscillatorTrend[0] = 0.0;
					SignalTrend[0] = 0.0;
				}	
				else
				{	
					Oscillator[0] = 50.0;
					Signal[0] = 50.0;
					OscillatorTrend[0] = 0.0;
					SignalTrend[0] = 0.0;
				}	
				return;
			}	
			
			// linear regression
			lookback = Math.Min(oscillatorPeriod, CurrentBar + 1);
			if(IsFirstTickOfBar)
			{
				sum_X = (double) lookback * (double)(lookback - 1) / 2.0;
				sum_XX = 0;
				preSum_Y = 0.0;
				sum_XY = 0.0;
				for (int i = 1; i < lookback; i++)
				{
					sum_XX  += (double) i * (double) i;
					preSum_Y += Input[i];
					sum_XY  += i * Input[i];
				}
				mean_X = 0.5 * (lookback - 1); 
				variance = lookback * sum_XX - sum_X * sum_X ;
			}
			sum_Y = preSum_Y + Input[0];
			mean_Y = sum_Y / lookback;
			covariance = lookback * sum_XY - sum_X * sum_Y;
			slope = covariance / variance; 
			intercept = mean_Y - slope * mean_X;
			
			// upper and lower max deviation
			if(calculateFromPriceData)
			{	
				if(IsFirstTickOfBar)
				{
					upperMaxDeviation = 0.0;
					lowerMaxDeviation = 0.0;
					for (int count = 1; count < lookback; count++) 
					{
						regressionValue = intercept + slope * count;
						upperResidual = High[count] - regressionValue;
						lowerResidual = regressionValue - Low[count]; 
						upperMaxDeviation = Math.Max(upperMaxDeviation, upperResidual);
						lowerMaxDeviation = Math.Max(lowerMaxDeviation, lowerResidual);
					}	
				}
				upperResidual = High[0] - intercept;
				lowerResidual = intercept - Low[0];
				upperMaxDeviation = Math.Max(upperMaxDeviation, upperResidual);
				lowerMaxDeviation = Math.Max(lowerMaxDeviation, lowerResidual);
				midBand		= intercept;
				upperBand	= intercept + upperMaxDeviation;
				lowerBand	= intercept - lowerMaxDeviation;
				num	= Close[0] - lowerBand;
				den	= upperBand - lowerBand;
			}
			else
			{	
				if(IsFirstTickOfBar)
				{
					upperMaxDeviation = 0.0;
					lowerMaxDeviation = 0.0;
					for (int count = 1; count < lookback; count++) 
					{
						regressionValue = intercept + slope * count;
						upperResidual = Input[count] - regressionValue;
						lowerResidual = regressionValue - Input[count]; 
						upperMaxDeviation = Math.Max(upperMaxDeviation, upperResidual);
						lowerMaxDeviation = Math.Max(lowerMaxDeviation, lowerResidual);
					}	
				}
				upperResidual = Input[0] - intercept;
				lowerResidual = intercept - Input[0];
				upperMaxDeviation = Math.Max(upperMaxDeviation, upperResidual);
				lowerMaxDeviation = Math.Max(lowerMaxDeviation, lowerResidual);
				midBand		= intercept;
				upperBand	= intercept + upperMaxDeviation;
				lowerBand	= intercept - lowerMaxDeviation;
				num	= Input[0] - lowerBand;
				den	= upperBand - lowerBand;
			}
			if (den.ApproxCompare(0) == 0)
				Oscillator[0] = Oscillator[1];
			else
				Oscillator[0] = Math.Min(100, Math.Max(0, 100 * num/den));
			Signal[0] = smoothedOscillator[0];
			
			if(showOscillator)
			{
				if(Oscillator[0] > upperlineValue)
				{	
					oscillatorTrend[0] = 1.0;
					PlotBrushes[0][0] = pOverboughtBrush;
				}	
				else if(Oscillator[0] < lowerlineValue)
				{	
					oscillatorTrend[0] = -1.0;
					PlotBrushes[0][0] = pOversoldBrush;
				}	
				else
				{	
					oscillatorTrend[0] = 0.0;
					PlotBrushes[0][0] = pNeutralBrush;
				}	
			}
			else
			{	
				if(Oscillator[0] > upperlineValue)
					oscillatorTrend[0] = 1.0;
				else if(Oscillator[0] < lowerlineValue)
					oscillatorTrend[0] = -1.0;
				else
					oscillatorTrend[0] = 0.0;
				PlotBrushes[0][0] = Brushes.Transparent;
			}			
			if(showSignal)
			{
				if(Signal[0] > upperlineValue)
				{	
					signalTrend[0] = 1.0;
					PlotBrushes[1][0] = sOverboughtBrush;
				}	
				else if(Signal[0] < lowerlineValue)
				{	
					signalTrend[0] = -1.0;
					PlotBrushes[1][0] = sOversoldBrush;
				}	
				else
				{	
					signalTrend[0] = 0.0;
					PlotBrushes[1][0] = sNeutralBrush;
				}	
			}
			else
			{	
				if(Signal[0] > upperlineValue)
					signalTrend[0] = 1.0;
				else if(Signal[0] < lowerlineValue)
					signalTrend[0] = -1.0;
				else
					signalTrend[0] = 0.0;
				PlotBrushes[1][0] = Brushes.Transparent;
			}
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Oscillator
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Signal
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> OscillatorTrend
		{
			get { return oscillatorTrend; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SignalTrend
		{
			get { return signalTrend; }
		}
		
		[Range(2, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Oscillator period", Description = "Select lookback period for the projection oscillator", GroupName = "Input Parameters", Order = 0)]
		public int OscillatorPeriod
		{	
            get { return oscillatorPeriod; }
            set { oscillatorPeriod = value; }
		}

		[Range(2, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Signal period", Description = "Select lookback period for the signal line", GroupName = "Input Parameters", Order = 1)]
		public int SignalPeriod
		{	
            get { return signalPeriod; }
            set { signalPeriod = value; }
		}

		[Range(50, 100), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Oscillator overbought", Description = "Sets the overbought level for the projection oscillator", GroupName = "Threshold Values", Order = 0)]
		public double UpperlineValue
		{	
            get { return upperlineValue; }
            set { upperlineValue = value; }
		}
		
		[Range(0, 50), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Oscillator oversold", Description = "Sets the oversold level for the projection oscillator", GroupName = "Threshold Values", Order = 1)]
		public double LowerlineValue
		{	
            get { return lowerlineValue; }
            set { lowerlineValue = value; }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show oscillator", Description = "Displays oscillator line", GroupName = "Display Options", Order = 0)]
      	public bool ShowOscillator
        {
            get { return showOscillator; }
            set { showOscillator = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Oscillator shading", Description = "Activates oscillator line shading", GroupName = "Display Options", Order = 1)]
      	public bool ShowOscillatorShading
        {
            get { return showOscillatorShading; }
            set { showOscillatorShading = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show signal line", Description = "Displays signal line", GroupName = "Display Options", Order = 2)]
      	public bool ShowSignal
        {
            get { return showSignal; }
            set { showSignal = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Signal line shading", Description = "Activates signal line shading", GroupName = "Display Options", Order = 3)]
      	public bool ShowSignalShading
        {
            get { return showSignalShading; }
            set { showSignalShading = value; }
        }
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Oscillator overbought", Description = "Sets the color for the oscillator when overbought", GroupName = "Projection Oscillator", Order = 0)]
		public System.Windows.Media.Brush POverboughtBrush
		{ 
			get {return pOverboughtBrush;}
			set {pOverboughtBrush = value;}
		}

		[Browsable(false)]
		public string POverboughtBrushSerializable
		{
			get { return Serialize.BrushToString(pOverboughtBrush); }
			set { pOverboughtBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Oscillator neutral", Description = "Sets the color for the oscillator when neutral", GroupName = "Projection Oscillator", Order = 1)]
		public System.Windows.Media.Brush PNeutralBrush
		{ 
			get {return pNeutralBrush;}
			set {pNeutralBrush = value;}
		}

		[Browsable(false)]
		public string PNeutralBrushSerializable
		{
			get { return Serialize.BrushToString(pNeutralBrush); }
			set { pNeutralBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Oscillator oversold", Description = "Sets the color for the oscillator when oversold", GroupName = "Projection Oscillator", Order = 2)]
		public System.Windows.Media.Brush POversoldBrush
		{ 
			get {return pOversoldBrush;}
			set {pOversoldBrush = value;}
		}

		[Browsable(false)]
		public string POversoldBrushSerializable
		{
			get { return Serialize.BrushToString(pOversoldBrush); }
			set { pOversoldBrush = Serialize.StringToBrush(value); }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot style oscillator", Description = "Sets the plot style for the oscillator", GroupName = "Projection Oscillator", Order = 3)]
		public PlotStyle Plot0Style
		{	
            get { return plot0Style; }
            set { plot0Style = value; }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style oscillator", Description = "Sets the dash style for the oscillator", GroupName = "Projection Oscillator", Order = 4)]
		public DashStyleHelper Dash0Style
		{
			get { return dash0Style; }
			set { dash0Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width oscillator", Description = "Sets the plot width for the oscillator", GroupName = "Projection Oscillator", Order = 5)]
		public int Plot0Width
		{	
            get { return plot0Width; }
            set { plot0Width = value; }
		}
			
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Oscillator overbought section", Description = "Sets the color for the overbought section of the oscillator", GroupName = "Projection Oscillator", Order = 6)]
		public System.Windows.Media.Brush POverboughtAreaBrush
		{ 
			get {return pOverboughtAreaBrush;}
			set {pOverboughtAreaBrush = value;}
		}

		[Browsable(false)]
		public string POverboughtAreaBrushSerializable
		{
			get { return Serialize.BrushToString(pOverboughtAreaBrush); }
			set { pOverboughtAreaBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Oscillator oversold section", Description = "Sets the color for the oversold section of the oscillator", GroupName = "Projection Oscillator", Order = 7)]
		public System.Windows.Media.Brush POversoldAreaBrush
		{ 
			get {return pOversoldAreaBrush;}
			set {pOversoldAreaBrush = value;}
		}

		[Browsable(false)]
		public string POversoldAreaBrushSerializable
		{
			get { return Serialize.BrushToString(pOversoldAreaBrush); }
			set { pOversoldAreaBrush = Serialize.StringToBrush(value); }
		}
		
		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Area opacity", Description = "Sets the opacity for the overbought and oversold areas", GroupName = "Projection Oscillator", Order = 8)]
		public int OscillatorOpacity
		{	
            get { return oscillatorOpacity; }
            set { oscillatorOpacity = value; }
		}		
				
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Signal line overbought", Description = "Sets the color for the signal line when overbought", GroupName = "Signal Line", Order = 0)]
		public System.Windows.Media.Brush SOverboughtBrush
		{ 
			get {return sOverboughtBrush;}
			set {sOverboughtBrush = value;}
		}

		[Browsable(false)]
		public string SOverboughtBrushSerializable
		{
			get { return Serialize.BrushToString(sOverboughtBrush); }
			set { sOverboughtBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Signal line neutral", Description = "Sets the color for the signal line when neutral", GroupName = "Signal Line", Order = 1)]
		public System.Windows.Media.Brush SNeutralBrush
		{ 
			get {return sNeutralBrush;}
			set {sNeutralBrush = value;}
		}

		[Browsable(false)]
		public string SNeutralBrushSerializable
		{
			get { return Serialize.BrushToString(sNeutralBrush); }
			set { sNeutralBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Signal line oversold", Description = "Sets the color for the signal line when oversold", GroupName = "Signal Line", Order = 2)]
		public System.Windows.Media.Brush SOversoldBrush
		{ 
			get {return sOversoldBrush;}
			set {sOversoldBrush = value;}
		}

		[Browsable(false)]
		public string SOversoldBrushSerializable
		{
			get { return Serialize.BrushToString(sOversoldBrush); }
			set { sOversoldBrush = Serialize.StringToBrush(value); }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot style signal line", Description = "Sets the plot style for the signal line", GroupName = "Signal Line", Order = 3)]
		public PlotStyle Plot1Style
		{	
            get { return plot1Style; }
            set { plot1Style = value; }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style signal line", Description = "Sets the dash style for the signal line", GroupName = "Signal Line", Order = 4)]
		public DashStyleHelper Dash1Style
		{
			get { return dash1Style; }
			set { dash1Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width signal line", Description = "Sets the plot width for the signal line", GroupName = "Signal Line", Order = 5)]
		public int Plot1Width
		{	
            get { return plot1Width; }
            set { plot1Width = value; }
		}
			
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Signal line overbought section", Description = "Sets the color for the overbought section of the signal line", GroupName = "Signal Line", Order = 6)]
		public System.Windows.Media.Brush SOverboughtAreaBrush
		{ 
			get {return sOverboughtAreaBrush;}
			set {sOverboughtAreaBrush = value;}
		}

		[Browsable(false)]
		public string SOverboughtAreaBrushSerializable
		{
			get { return Serialize.BrushToString(sOverboughtAreaBrush); }
			set { sOverboughtAreaBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Signal line oversold section", Description = "Sets the color for the oversold section of the signal line" , GroupName = "Signal Line", Order = 7)]
		public System.Windows.Media.Brush SOversoldAreaBrush
		{ 
			get {return sOversoldAreaBrush;}
			set {sOversoldAreaBrush = value;}
		}

		[Browsable(false)]
		public string SOversoldAreaBrushSerializable
		{
			get { return Serialize.BrushToString(sOversoldAreaBrush); }
			set { sOversoldAreaBrush = Serialize.StringToBrush(value); }
		}
		
		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Area opacity", Description = "Sets the opacity for the overbought and oversold areas", GroupName = "Signal Line", Order = 8)]
		public int SignalOpacity
		{	
            get { return signalOpacity; }
            set { signalOpacity = value; }
		}

		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Upper line", Description = "Sets the color for the upper line", GroupName = "Lines", Order = 0)]
		public System.Windows.Media.Brush UpperlineBrush
		{ 
			get {return upperlineBrush;}
			set {upperlineBrush = value;}
		}

		[Browsable(false)]
		public string UpperlineBrushSerializable
		{
			get { return Serialize.BrushToString(upperlineBrush); }
			set { upperlineBrush = Serialize.StringToBrush(value); }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style upper line", Description = "Sets the dash style for the upper line", GroupName = "Lines", Order = 1)]
		public DashStyleHelper UpperlineStyle
		{
			get { return upperlineStyle; }
			set { upperlineStyle = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width upper line", Description = "Sets the plot width for the upper line", GroupName = "Lines", Order = 2)]
		public int UpperlineWidth
		{	
            get { return upperlineWidth; }
            set { upperlineWidth = value; }
		}
			
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Midline", Description = "Sets the color for the midline", GroupName = "Lines", Order = 3)]
		public System.Windows.Media.Brush MidlineBrush
		{ 
			get {return midlineBrush;}
			set {midlineBrush = value;}
		}

		[Browsable(false)]
		public string MidlineBrushSerializable
		{
			get { return Serialize.BrushToString(midlineBrush); }
			set { midlineBrush = Serialize.StringToBrush(value); }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style midline", Description = "Sets the dash style for the midline", GroupName = "Lines", Order = 4)]
		public DashStyleHelper MidlineStyle
		{
			get { return midlineStyle; }
			set { midlineStyle = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width midline", Description = "Sets the plot width for the midline", GroupName = "Lines", Order = 5)]
		public int MidlineWidth
		{	
            get { return midlineWidth; }
            set { midlineWidth = value; }
		}
			
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Lower line", Description = "Sets the color for the lower line", GroupName = "Lines", Order = 6)]
		public System.Windows.Media.Brush LowerlineBrush
		{ 
			get {return lowerlineBrush;}
			set {lowerlineBrush = value;}
		}

		[Browsable(false)]
		public string LowerlineBrushSerializable
		{
			get { return Serialize.BrushToString(lowerlineBrush); }
			set { lowerlineBrush = Serialize.StringToBrush(value); }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style lower line", Description = "Sets the dash style for the lower line", GroupName = "Lines", Order = 7)]
		public DashStyleHelper LowerlineStyle
		{
			get { return lowerlineStyle; }
			set { lowerlineStyle = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width lower line", Description = "Sets the plot width for the lower line", GroupName = "Lines", Order = 8)]
		public int LowerlineWidth
		{	
            get { return lowerlineWidth; }
            set { lowerlineWidth = value; }
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

		public override void OnRenderTargetChanged()
		{
			if (sOverboughtBrushDX != null)
				sOverboughtBrushDX.Dispose();
			if (sOversoldBrushDX != null)
				sOversoldBrushDX.Dispose();
			if (sNeutralBrushDX != null)
				sNeutralBrushDX.Dispose();
			if (pOverboughtBrushDX != null)
				pOverboughtBrushDX.Dispose();
			if (pOversoldBrushDX != null)
				pOversoldBrushDX.Dispose();
			if (pNeutralBrushDX != null)
				pNeutralBrushDX.Dispose();
			if (sOverboughtAreaBrushDX != null)
				sOverboughtAreaBrushDX.Dispose();
			if (sOversoldAreaBrushDX != null)
				sOversoldAreaBrushDX.Dispose();
			if (pOverboughtAreaBrushDX != null)
				pOverboughtAreaBrushDX.Dispose();
			if (pOversoldAreaBrushDX != null)
				pOversoldAreaBrushDX.Dispose();
			if (upperlineBrushDX != null)
				upperlineBrushDX.Dispose();
			if (midlineBrushDX != null)
				midlineBrushDX.Dispose();
			if (lowerlineBrushDX != null)
				lowerlineBrushDX.Dispose();
			
			if (RenderTarget != null)
			{
				try
				{
					sOverboughtBrushDX = sOverboughtBrush.ToDxBrush(RenderTarget);
					sOversoldBrushDX = sOversoldBrush.ToDxBrush(RenderTarget);
					sNeutralBrushDX = sNeutralBrush.ToDxBrush(RenderTarget);
					pOverboughtBrushDX = pOverboughtBrush.ToDxBrush(RenderTarget);
					pOversoldBrushDX = pOversoldBrush.ToDxBrush(RenderTarget);
					pNeutralBrushDX = pNeutralBrush.ToDxBrush(RenderTarget);
					sOverboughtAreaBrushDX = sOverboughtAreaBrushOpaque.ToDxBrush(RenderTarget);
					sOversoldAreaBrushDX = sOversoldAreaBrushOpaque.ToDxBrush(RenderTarget);
					pOverboughtAreaBrushDX = pOverboughtAreaBrushOpaque.ToDxBrush(RenderTarget);
					pOversoldAreaBrushDX = pOversoldAreaBrushOpaque.ToDxBrush(RenderTarget);
					upperlineBrushDX = upperlineBrush.ToDxBrush(RenderTarget);
					midlineBrushDX = midlineBrush.ToDxBrush(RenderTarget);
					lowerlineBrushDX = lowerlineBrush.ToDxBrush(RenderTarget);
				}
				catch (Exception e) { }
			}
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			if (Bars == null || ChartControl == null || ChartBars.ToIndex < BarsRequiredToPlot || !IsVisible) return;
			int	lastBarPainted = ChartBars.ToIndex;
			if(lastBarPainted  < 0 || BarsArray[0].Count < lastBarPainted) return;
			SharpDX.Direct2D1.AntialiasMode oldAntialiasMode = RenderTarget.AntialiasMode;
			RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.Aliased;
	        ChartPanel panel = chartControl.ChartPanels[ChartPanel.PanelIndex];
			
			bool nonEquidistant 	= (chartControl.BarSpacingType == BarSpacingType.TimeBased || chartControl.BarSpacingType == BarSpacingType.EquidistantMulti);
			int lastBarCounted		= Close.Count - 1;
			int	lastBarOnUpdate		= lastBarCounted - (Calculate == Calculate.OnBarClose ? 1 : 0);
			int	lastBarIndex		= Math.Min(lastBarPainted, lastBarOnUpdate);
			int firstBarPainted	 	= ChartBars.FromIndex;
			int firstBarIndex  	 	= Math.Max(totalBarsRequiredToPlot, firstBarPainted); 
			int lastIndex			= 0;
			int x 					= 0;
			int y0	 				= 0;
			int y1 					= 0;
			int yUpper 				= 0;
			int yMid 				= 0;
			int yLower 				= 0;
			int y 					= 0;
			int t					= 0;
			int lastX 				= 0;
			int lastY1 				= 0;
			int lastY2 				= 0;
			int lastY 				= 0;
			int sign 				= 0;
			int lastSign 			= 0;
			int startBar 			= 0;
			int returnBar 			= 0;
			int count	 			= 0;
			double barWidth			= 0;
			bool firstLoop 			= true;
			SharpDX.Vector2 startPointDX;
			SharpDX.Vector2 endPointDX;
			Vector2[] plotArray 	= new Vector2[2 * (lastBarIndex - firstBarIndex + Math.Max(0, displacement) + 1)]; 
			
			if(lastBarIndex + displacement > firstBarIndex)
			{	
				if (displacement >= 0)
					lastIndex = lastBarIndex + displacement;
				else
					lastIndex = Math.Min(lastBarIndex, lastBarOnUpdate + displacement);
				if(nonEquidistant && lastIndex > lastBarOnUpdate)
					barWidth = Convert.ToDouble(ChartControl.GetXByBarIndex(ChartBars, lastBarPainted) - ChartControl.GetXByBarIndex(ChartBars, lastBarPainted - displacement))/displacement;
				lastY1	= chartScale.GetYByValue(Oscillator.GetValueAt(lastIndex - displacement));
				lastY2	= chartScale.GetYByValue(Signal.GetValueAt(lastIndex - displacement));
				yUpper  = chartScale.GetYByValue(upperlineValue);
				yMid  = chartScale.GetYByValue(midlineValue);
				yLower  = chartScale.GetYByValue(lowerlineValue);
				
				//Lines
				startPointDX = new SharpDX.Vector2((float)ChartPanel.X, (float)yUpper);
				endPointDX	= new SharpDX.Vector2((float)ChartPanel.X + ChartPanel.W, (float)yUpper);
				RenderTarget.DrawLine(startPointDX, endPointDX, upperlineBrushDX, upperlineWidth, Lines[0].StrokeStyle);
				startPointDX = new SharpDX.Vector2((float)ChartPanel.X, (float)yMid);
				endPointDX	= new SharpDX.Vector2((float)ChartPanel.X + ChartPanel.W, (float)yMid);
				RenderTarget.DrawLine(startPointDX, endPointDX, midlineBrushDX, midlineWidth, Lines[1].StrokeStyle);
				startPointDX = new SharpDX.Vector2((float)ChartPanel.X, (float)yLower);
				endPointDX	= new SharpDX.Vector2((float)ChartPanel.X + ChartPanel.W, (float)yLower);
				RenderTarget.DrawLine(startPointDX, endPointDX, lowerlineBrushDX, lowerlineWidth, Lines[2].StrokeStyle);
				
				// Signal Line Shading
				if(applySignalShading)
				{	
					lastY = lastY2;
					lastSign = (int) SignalTrend.GetValueAt(lastIndex - displacement);
					startBar = lastIndex;
					firstLoop = true;
					do
					{
						SharpDX.Direct2D1.PathGeometry 	pathF;
						SharpDX.Direct2D1.GeometrySink 	sinkF;
						pathF = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
						using (pathF)
						{
							if(firstLoop)
							{	
								count = 0;						
								if(nonEquidistant && displacement > 0 && startBar > lastBarCounted)
									x = ChartControl.GetXByBarIndex(ChartBars, lastBarCounted) + (int)((startBar - lastBarCounted) * barWidth);
								else
									x = ChartControl.GetXByBarIndex(ChartBars, startBar);
								if (lastSign == 1)
									plotArray[count] = new Vector2(x, yUpper);
								else if(lastSign == -1)
									plotArray[count] = new Vector2(x, yLower);
							}	
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
								y = chartScale.GetYByValue(Signal.GetValueAt(idx - displacement));   
								t = (int) SignalTrend.GetValueAt(idx - displacement);
								count = count + 1;
								if(t == lastSign)
								{
									plotArray[count] = new Vector2(x,y);
									startBar = idx;
									sign = t;
									lastX = x;
									lastY = y;
								}	
								else if (t == 0 && lastSign == 1)
								{	
									double lastDiff = Convert.ToDouble(yUpper - lastY);
									double diff = Convert.ToDouble(yUpper - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yUpper;
									plotArray[count] = new Vector2(x,y);
									startBar = idx;
									sign = lastSign;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == 0 && lastSign == -1)
								{	
									double lastDiff = Convert.ToDouble(yLower - lastY);
									double diff = Convert.ToDouble(yLower - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yLower;
									plotArray[count] = new Vector2(x,y);
									startBar = idx;
									sign = lastSign;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == 1 && lastSign == 0)
								{	
									double lastDiff = Convert.ToDouble(yUpper - lastY);
									double diff = Convert.ToDouble(yUpper - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yUpper;
									plotArray[count] = new Vector2(x,y);
									startBar = idx;
									sign = lastSign;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == -1 && lastSign == 0)
								{	
									double lastDiff = Convert.ToDouble(yLower - lastY);
									double diff = Convert.ToDouble(yLower - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yLower;
									plotArray[count] = new Vector2(x,y);
									startBar = idx;
									sign = lastSign;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == -1 && lastSign == 1)
								{	
									double lastDiff = Convert.ToDouble(yUpper - lastY);
									double diff = Convert.ToDouble(yUpper - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yUpper;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = 0;
									break;
								}
								else if (t == 1 && lastSign == -1)
								{	
									double lastDiff = Convert.ToDouble(yLower - lastY);
									double diff = Convert.ToDouble(yLower - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yLower;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = 0;
									break;
								}
							}
							if(sign == 0)
							{	
								firstLoop = false;
								continue;
							}	
							if(startBar == firstBarIndex)
							{
								count = count + 1;
								if(sign == 1)
									plotArray[count] = new Vector2(lastX, yUpper);
								else if(sign == -1)
									plotArray[count] = new Vector2(lastX, yLower);
							}	
							sinkF = pathF.Open();
							sinkF.BeginFigure(plotArray[0], FigureBegin.Filled);
							for (int i = 1; i <= count; i++)
								sinkF.AddLine(plotArray[i]);
							sinkF.EndFigure(FigureEnd.Closed);
			        		sinkF.Close();
							RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
							if(sign == 1)
		 						RenderTarget.FillGeometry(pathF, sOverboughtAreaBrushDX);
							else if(sign == -1) 
		 						RenderTarget.FillGeometry(pathF, sOversoldAreaBrushDX);
							RenderTarget.AntialiasMode = oldAntialiasMode;
						}
						pathF.Dispose();
						sinkF.Dispose();
						firstLoop = false;
					}	
					while (startBar > firstBarIndex && Signal.IsValidDataPointAt(startBar - displacement));
				}
				
				// Oscillator Shading
				if(applyOscillatorShading)
				{	
					lastY = lastY1;
					lastSign = (int) OscillatorTrend.GetValueAt(lastIndex - displacement);
					startBar = lastIndex;
					firstLoop = true;
					do
					{
						SharpDX.Direct2D1.PathGeometry 	pathF;
						SharpDX.Direct2D1.GeometrySink 	sinkF;
						pathF = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
						using (pathF)
						{
							if(firstLoop)
							{	
								count = 0;						
								if(nonEquidistant && displacement > 0 && startBar > lastBarCounted)
									x = ChartControl.GetXByBarIndex(ChartBars, lastBarCounted) + (int)((startBar - lastBarCounted) * barWidth);
								else
									x = ChartControl.GetXByBarIndex(ChartBars, startBar);
								if (lastSign == 1)
									plotArray[count] = new Vector2(x, yUpper);
								else if(lastSign == -1)
									plotArray[count] = new Vector2(x, yLower);
							}	
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
								y = chartScale.GetYByValue(Oscillator.GetValueAt(idx - displacement));   
								t = (int) OscillatorTrend.GetValueAt(idx - displacement);
								count = count + 1;
								if(t == lastSign)
								{
									plotArray[count] = new Vector2(x,y);
									startBar = idx;
									sign = t;
									lastX = x;
									lastY = y;
								}	
								else if (t == 0 && lastSign == 1)
								{	
									double lastDiff = Convert.ToDouble(yUpper - lastY);
									double diff = Convert.ToDouble(yUpper - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yUpper;
									plotArray[count] = new Vector2(x,y);
									startBar = idx;
									sign = lastSign;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == 0 && lastSign == -1)
								{	
									double lastDiff = Convert.ToDouble(yLower - lastY);
									double diff = Convert.ToDouble(yLower - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yLower;
									plotArray[count] = new Vector2(x,y);
									startBar = idx;
									sign = lastSign;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == 1 && lastSign == 0)
								{	
									double lastDiff = Convert.ToDouble(yUpper - lastY);
									double diff = Convert.ToDouble(yUpper - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yUpper;
									plotArray[count] = new Vector2(x,y);
									startBar = idx;
									sign = lastSign;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == -1 && lastSign == 0)
								{	
									double lastDiff = Convert.ToDouble(yLower - lastY);
									double diff = Convert.ToDouble(yLower - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yLower;
									plotArray[count] = new Vector2(x,y);
									startBar = idx;
									sign = lastSign;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == -1 && lastSign == 1)
								{	
									double lastDiff = Convert.ToDouble(yUpper - lastY);
									double diff = Convert.ToDouble(yUpper - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yUpper;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = 0;
									break;
								}
								else if (t == 1 && lastSign == -1)
								{	
									double lastDiff = Convert.ToDouble(yLower - lastY);
									double diff = Convert.ToDouble(yLower - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yLower;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = 0;
									break;
								}
							}
							if(sign == 0)
							{	
								firstLoop = false;
								continue;
							}	
							if(startBar == firstBarIndex)
							{
								count = count + 1;
								if(sign == 1)
									plotArray[count] = new Vector2(lastX, yUpper);
								else if(sign == -1)
									plotArray[count] = new Vector2(lastX, yLower);
							}	
							sinkF = pathF.Open();
							sinkF.BeginFigure(plotArray[0], FigureBegin.Filled);
							for (int i = 1; i <= count; i++)
								sinkF.AddLine(plotArray[i]);
							sinkF.EndFigure(FigureEnd.Closed);
			        		sinkF.Close();
							RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
							if(sign == 1)
		 						RenderTarget.FillGeometry(pathF, pOverboughtAreaBrushDX);
							else if(sign == -1) 
		 						RenderTarget.FillGeometry(pathF, pOversoldAreaBrushDX);
							RenderTarget.AntialiasMode = oldAntialiasMode;
						}
						pathF.Dispose();
						sinkF.Dispose();
						firstLoop = false;
					}	
					while (startBar > firstBarIndex && Oscillator.IsValidDataPointAt(startBar - displacement));
				}
				
				// Signal line
				if(showSignal)
				{	
					lastY = lastY1;
					lastSign = 5;
					startBar = lastIndex;
					firstLoop = true;
					do
					{
						SharpDX.Direct2D1.PathGeometry 	pathE;
						SharpDX.Direct2D1.GeometrySink 	sinkE;
						pathE = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
						using (pathE)
						{
							count = 0;
							plotArray[count] = new Vector2(lastX, lastY);
							for (int idx = startBar; idx >= firstBarIndex; idx --)	
							{
								if(nonEquidistant && displacement > 0 && idx > lastBarCounted)
									x = ChartControl.GetXByBarIndex(ChartBars, lastBarCounted) + (int)((idx - lastBarCounted) * barWidth);
								else
									x = ChartControl.GetXByBarIndex(ChartBars, idx);
								y = chartScale.GetYByValue(Signal.GetValueAt(idx - displacement));   
								t = (int) SignalTrend.GetValueAt(idx - displacement);
								count = count + 1;
								if(t == lastSign)
								{
									plotArray[count] = new Vector2(x,y);
									sign = t;
									startBar = idx;
									lastX = x;
									lastY = y;
								}	
								else if(lastSign == 5)
								{	
									plotArray[count] = new Vector2(x,y);
									sign = 5;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}	
								else if (t == 0 && lastSign == 1)
								{	
									double lastDiff = Convert.ToDouble(yUpper - lastY);
									double diff = Convert.ToDouble(yUpper - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yUpper;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == 0 && lastSign == -1)
								{	
									double lastDiff = Convert.ToDouble(yLower - lastY);
									double diff = Convert.ToDouble(yLower - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yLower;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == 1 && lastSign == 0)
								{	
									double lastDiff = Convert.ToDouble(yUpper - lastY);
									double diff = Convert.ToDouble(yUpper - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yUpper;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == -1 && lastSign == 0)
								{	
									double lastDiff = Convert.ToDouble(yLower - lastY);
									double diff = Convert.ToDouble(yLower - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yLower;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == -1 && lastSign == 1)
								{	
									double lastDiff = Convert.ToDouble(yUpper - lastY);
									double diff = Convert.ToDouble(yUpper - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yUpper;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = 0;
									break;
								}
								else if (t == 1 && lastSign == -1)
								{	
									double lastDiff = Convert.ToDouble(yLower - lastY);
									double diff = Convert.ToDouble(yLower - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yLower;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = 0;
									break;
								}
							}
							sinkE = pathE.Open();
							if(firstLoop)
								sinkE.BeginFigure(plotArray[1], FigureBegin.Filled);
							else
								sinkE.BeginFigure(plotArray[0], FigureBegin.Filled);
							for (int i = 1; i <= count; i++)
								sinkE.AddLine(plotArray[i]);
							sinkE.EndFigure(FigureEnd.Open);
			        		sinkE.Close();
							RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
							if(sign == 1)
		 						RenderTarget.DrawGeometry(pathE, sOverboughtBrushDX, Plots[1].Width, Plots[1].StrokeStyle);
							else if(sign == 0) 
		 						RenderTarget.DrawGeometry(pathE, sNeutralBrushDX, Plots[1].Width, Plots[1].StrokeStyle);
							else if(sign == -1) 
		 						RenderTarget.DrawGeometry(pathE, sOversoldBrushDX, Plots[1].Width, Plots[1].StrokeStyle);
							RenderTarget.AntialiasMode = oldAntialiasMode;
						}
						pathE.Dispose();
						sinkE.Dispose();
						firstLoop = false;
					}	
					while (startBar > firstBarIndex && Signal.IsValidDataPointAt(startBar - displacement));
				}

				// Oscillator
				if(showOscillator)
				{	
					lastY = lastY1;
					lastSign = 5;
					startBar = lastIndex;
					firstLoop = true;
					do
					{
						SharpDX.Direct2D1.PathGeometry 	pathE;
						SharpDX.Direct2D1.GeometrySink 	sinkE;
						pathE = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
						using (pathE)
						{
							count = 0;
							plotArray[count] = new Vector2(lastX, lastY);
							for (int idx = startBar; idx >= firstBarIndex; idx --)	
							{
								if(nonEquidistant && displacement > 0 && idx > lastBarCounted)
									x = ChartControl.GetXByBarIndex(ChartBars, lastBarCounted) + (int)((idx - lastBarCounted) * barWidth);
								else
									x = ChartControl.GetXByBarIndex(ChartBars, idx);
								y = chartScale.GetYByValue(Oscillator.GetValueAt(idx - displacement));   
								t = (int) OscillatorTrend.GetValueAt(idx - displacement);
								count = count + 1;
								if(t == lastSign)
								{
									plotArray[count] = new Vector2(x,y);
									sign = t;
									startBar = idx;
									lastX = x;
									lastY = y;
								}	
								else if(lastSign == 5)
								{	
									plotArray[count] = new Vector2(x,y);
									sign = 5;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}	
								else if (t == 0 && lastSign == 1)
								{	
									double lastDiff = Convert.ToDouble(yUpper - lastY);
									double diff = Convert.ToDouble(yUpper - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yUpper;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == 0 && lastSign == -1)
								{	
									double lastDiff = Convert.ToDouble(yLower - lastY);
									double diff = Convert.ToDouble(yLower - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yLower;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == 1 && lastSign == 0)
								{	
									double lastDiff = Convert.ToDouble(yUpper - lastY);
									double diff = Convert.ToDouble(yUpper - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yUpper;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == -1 && lastSign == 0)
								{	
									double lastDiff = Convert.ToDouble(yLower - lastY);
									double diff = Convert.ToDouble(yLower - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yLower;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == -1 && lastSign == 1)
								{	
									double lastDiff = Convert.ToDouble(yUpper - lastY);
									double diff = Convert.ToDouble(yUpper - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yUpper;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = 0;
									break;
								}
								else if (t == 1 && lastSign == -1)
								{	
									double lastDiff = Convert.ToDouble(yLower - lastY);
									double diff = Convert.ToDouble(yLower - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yLower;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = 0;
									break;
								}
							}
							sinkE = pathE.Open();
							if(firstLoop)
								sinkE.BeginFigure(plotArray[1], FigureBegin.Filled);
							else
								sinkE.BeginFigure(plotArray[0], FigureBegin.Filled);
							for (int i = 1; i <= count; i++)
								sinkE.AddLine(plotArray[i]);
							sinkE.EndFigure(FigureEnd.Open);
			        		sinkE.Close();
							RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
							if(sign == 1)
		 						RenderTarget.DrawGeometry(pathE, pOverboughtBrushDX, Plots[0].Width, Plots[0].StrokeStyle);
							else if(sign == 0) 
		 						RenderTarget.DrawGeometry(pathE, pNeutralBrushDX, Plots[0].Width, Plots[0].StrokeStyle);
							else if(sign == -1) 
		 						RenderTarget.DrawGeometry(pathE, pOversoldBrushDX, Plots[0].Width, Plots[0].StrokeStyle);
							RenderTarget.AntialiasMode = oldAntialiasMode;
						}
						pathE.Dispose();
						sinkE.Dispose();
						firstLoop = false;
					}	
					while (startBar > firstBarIndex && Oscillator.IsValidDataPointAt(startBar - displacement));
				}			
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
		private LizardIndicators.amaProjectionOscillator[] cacheamaProjectionOscillator;
		public LizardIndicators.amaProjectionOscillator amaProjectionOscillator(int oscillatorPeriod, int signalPeriod, double upperlineValue, double lowerlineValue)
		{
			return amaProjectionOscillator(Input, oscillatorPeriod, signalPeriod, upperlineValue, lowerlineValue);
		}

		public LizardIndicators.amaProjectionOscillator amaProjectionOscillator(ISeries<double> input, int oscillatorPeriod, int signalPeriod, double upperlineValue, double lowerlineValue)
		{
			if (cacheamaProjectionOscillator != null)
				for (int idx = 0; idx < cacheamaProjectionOscillator.Length; idx++)
					if (cacheamaProjectionOscillator[idx] != null && cacheamaProjectionOscillator[idx].OscillatorPeriod == oscillatorPeriod && cacheamaProjectionOscillator[idx].SignalPeriod == signalPeriod && cacheamaProjectionOscillator[idx].UpperlineValue == upperlineValue && cacheamaProjectionOscillator[idx].LowerlineValue == lowerlineValue && cacheamaProjectionOscillator[idx].EqualsInput(input))
						return cacheamaProjectionOscillator[idx];
			return CacheIndicator<LizardIndicators.amaProjectionOscillator>(new LizardIndicators.amaProjectionOscillator(){ OscillatorPeriod = oscillatorPeriod, SignalPeriod = signalPeriod, UpperlineValue = upperlineValue, LowerlineValue = lowerlineValue }, input, ref cacheamaProjectionOscillator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaProjectionOscillator amaProjectionOscillator(int oscillatorPeriod, int signalPeriod, double upperlineValue, double lowerlineValue)
		{
			return indicator.amaProjectionOscillator(Input, oscillatorPeriod, signalPeriod, upperlineValue, lowerlineValue);
		}

		public Indicators.LizardIndicators.amaProjectionOscillator amaProjectionOscillator(ISeries<double> input , int oscillatorPeriod, int signalPeriod, double upperlineValue, double lowerlineValue)
		{
			return indicator.amaProjectionOscillator(input, oscillatorPeriod, signalPeriod, upperlineValue, lowerlineValue);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaProjectionOscillator amaProjectionOscillator(int oscillatorPeriod, int signalPeriod, double upperlineValue, double lowerlineValue)
		{
			return indicator.amaProjectionOscillator(Input, oscillatorPeriod, signalPeriod, upperlineValue, lowerlineValue);
		}

		public Indicators.LizardIndicators.amaProjectionOscillator amaProjectionOscillator(ISeries<double> input , int oscillatorPeriod, int signalPeriod, double upperlineValue, double lowerlineValue)
		{
			return indicator.amaProjectionOscillator(input, oscillatorPeriod, signalPeriod, upperlineValue, lowerlineValue);
		}
	}
}

#endregion
