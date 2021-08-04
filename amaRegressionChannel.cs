//+----------------------------------------------------------------------------------------------+
//| Copyright © <2020>  <LizardTrader.com - powered by AlderLab UG>
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
using NinjaTrader.NinjaScript.Indicators.LizardIndicators;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using SharpDX;
using SharpDX.Direct2D1;

#endregion

//This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.LizardIndicators
{
	/// <summary>
	/// The Regression Channel is based on linear regression which uses the least squares method to plot a straight line through prices so as to minimize the distances between the prices and the resulting trendline.
	/// The upper channel line is obtained by adding a multiple of the standard deviation to the regression line. Conversely, the lower channel line is obtained by subtracting the same multiple of the standard deviation
	/// from the regression line. This indicator allows for calculating the regression channel N bars ago and then projecting the channel lines forward towards the last price bar shown on the chart.
	/// </summary>
	[Gui.CategoryOrder("Input Parameters", 1000100)]
	[Gui.CategoryOrder("Display Options", 1000200)]
	[Gui.CategoryOrder("Linear Regression Channel", 8000100)]
	[Gui.CategoryOrder("Projection Start Line", 8000200)]
	[Gui.CategoryOrder("Regression Bands", 8000300)]
	[Gui.CategoryOrder("Version", 8000400)]
	[TypeConverter("NinjaTrader.NinjaScript.Indicators.amaRegressionChannelTypeConverter")]
	public class amaRegressionChannel : Indicator
	{
		private int								regressionPeriod			= 50;
		private int								barsAgo						= 0;
		private int								lookback					= 0;
		private int								displacement				= 0;
		private double							sum_X						= 0;
		private double							sum_XX						= 0;
		private double							multiplier					= 3.5;
		private double							sum_Y						= 0;
		private double							preSum_Y					= 0;
		private double							sum_XY						= 0;
		private double							mean_X						= 0.0;
		private double							mean_Y						= 0.0;
		private double							variance					= 0.0;
		private double							covariance					= 0.0;
		private double							slope						= 0.0;
		private double							intercept					= 0.0;
		private double							regressionValue				= 0.0;
		private double							residual					= 0.0;
		private double							sumResiduals				= 0.0;
		private double							preSumResiduals				= 0.0;
		private double							squareSumResiduals			= 0.0;
		private double							preSquareSumResiduals		= 0.0;
		private double							avgResiduals				= 0.0;
		private double							stdDeviation				= 0.0;
		private double							avgTrueRange				= 0.0;
		private double							normFactorSlope1			= 0.0;
		private double							normFactorSlope2			= 0.0;
		private double							normFactorWidth				= 0.0;
		private bool							showRegressionChannel		= true;
		private bool							showRegressionLine			= true;
		private bool							showRegressionBands			= false;
		private bool							indicatorIsOnPricePanel		= true;
		private bool							errorMessage				= true;
		private System.Windows.Media.Brush		middleBrushB				= Brushes.DarkSlateGray;
		private System.Windows.Media.Brush		upperBrushB					= Brushes.DarkSlateGray;
		private System.Windows.Media.Brush		lowerBrushB					= Brushes.DarkSlateGray;
		private System.Windows.Media.Brush		middleBrush					= Brushes.Blue;
		private System.Windows.Media.Brush		upperBrush					= Brushes.Blue;
		private System.Windows.Media.Brush		lowerBrush					= Brushes.Blue;
		private System.Windows.Media.Brush		middleProjectionBrush		= Brushes.MediumVioletRed;
		private System.Windows.Media.Brush		upperProjectionBrush		= Brushes.MediumVioletRed;
		private System.Windows.Media.Brush		lowerProjectionBrush		= Brushes.MediumVioletRed;
		private System.Windows.Media.Brush		verticalBrush				= Brushes.DarkSlateGray;
		private SharpDX.Direct2D1.Brush 		middleProjectionBrushDX 	= null;
		private SharpDX.Direct2D1.Brush 		upperProjectionBrushDX 		= null;
		private SharpDX.Direct2D1.Brush 		lowerProjectionBrushDX 		= null;
		private SharpDX.Direct2D1.Brush 		middleBrushDX 				= null;
		private SharpDX.Direct2D1.Brush 		upperBrushDX 				= null;
		private SharpDX.Direct2D1.Brush 		lowerBrushDX 				= null;
		private SharpDX.Direct2D1.Brush 		verticalBrushDX				= null;
		private System.Windows.Media.Brush		errorBrush					= Brushes.Black;
		private SimpleFont						errorFont					= null;
		private string							errorText1					= "The amaRegressionChannel indicator cannot be used with a regression period smaller than 2.";
		private string							errorText2					= "The amaRegressionChannel indicator cannot be used with a displacement.";
		private PlotStyle 						plot0StyleB					= PlotStyle.Line;
		private DashStyleHelper					dash0StyleB					= DashStyleHelper.Dash;
		private int 							plot0WidthB					= 1;
		private PlotStyle 						plot1StyleB					= PlotStyle.Line;
		private DashStyleHelper					dash1StyleB					= DashStyleHelper.Solid;
		private int 							plot1WidthB					= 1;
		private PlotStyle 						plot2StyleB					= PlotStyle.Line;
		private DashStyleHelper					dash2StyleB					= DashStyleHelper.Solid;
		private int 							plot2WidthB					= 1;
		private PlotStyle 						plot0Style					= PlotStyle.Line;
		private DashStyleHelper					dash0Style					= DashStyleHelper.Dash;
		private int 							plot0Width 					= 2;
		private PlotStyle 						plot1Style					= PlotStyle.Line;
		private DashStyleHelper					dash1Style					= DashStyleHelper.Solid;
		private int 							plot1Width 					= 2;
		private PlotStyle 						plot2Style					= PlotStyle.Line;
		private DashStyleHelper					dash2Style					= DashStyleHelper.Solid;
		private int 							plot2Width 					= 2;
		private DashStyleHelper					dash3Style					= DashStyleHelper.Dash;
		private int 							plot3Width 					= 2;
		private Stroke							stroke0;
		private Stroke							stroke1;
		private Stroke							stroke2;
		private Stroke							stroke3;
		private string							versionString				= "v 1.4  -  March 9, 2020";
		private Series<double> 					interceptSeries;
		private Series<double> 					slopeSeries;
		private Series<double> 					stdDeviationSeries;
		private Series<double>					normalizedSlope;
		private Series<double>					normalizedChannelWidth;
		private Series<double>					regressionTrend;
		private amaATR							averageTrueRange;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "The Regression Channel is based on linear regression which uses the least squares method to plot a straight line through prices so as to minimize the distances"
												+ " between the prices and the resulting trendline. The upper channel line is obtained by adding a multiple of the standard deviation to the regression line."
												+ " Conversely, the lower channel line is obtained by subtracting the same multiple of the standard deviation from the regression line. This indicator allows for"
												+ " calculating the regression channel N bars ago and then projecting the channel lines forward towards the last price bar shown on the chart.";
				Name						= "amaRegressionChannel";
				Calculate					= Calculate.OnPriceChange;
				IsSuspendedWhileInactive	= true;
				IsAutoScale					= false;
				IsOverlay					= true;
				ArePlotsConfigurable		= false;
				AddPlot(new Stroke(Brushes.DarkSlateGray, 2), PlotStyle.Line, "Linear Regression");	
				AddPlot(new Stroke(Brushes.DarkSlateGray, 2), PlotStyle.Line, "Upper Band");	
				AddPlot(new Stroke(Brushes.DarkSlateGray, 2), PlotStyle.Line, "Lower Band");	
			}
			else if (State == State.Configure)
			{
				if(Calculate == Calculate.OnEachTick)
					Calculate = Calculate.OnPriceChange;
				BarsRequiredToPlot 	= regressionPeriod;
				displacement = Displacement;
				Plots[0].Brush = middleBrushB.Clone();
				Plots[1].Brush = upperBrushB.Clone();
				Plots[2].Brush = lowerBrushB.Clone();
				Plots[0].PlotStyle = plot0StyleB;
				Plots[1].PlotStyle = plot1StyleB;
				Plots[2].PlotStyle = plot2StyleB;
				Plots[0].DashStyleHelper = dash0StyleB;
				Plots[1].DashStyleHelper = dash1StyleB;
				Plots[2].DashStyleHelper = dash2StyleB;
				Plots[0].Width = plot0WidthB;
				Plots[1].Width = plot1WidthB;
				Plots[2].Width = plot2WidthB;
				stroke0	= new Stroke(middleBrush, dash0Style, plot0Width);
				stroke1	= new Stroke(upperBrush, dash1Style, plot1Width);
				stroke2	= new Stroke(lowerBrush, dash2Style, plot2Width);
				stroke3	= new Stroke(lowerBrush, dash3Style, plot3Width);
			}
 			else if (State == State.DataLoaded)
			{
				averageTrueRange		= amaATR(amaATRCalcMode.Arithmetic, Math.Max(256, regressionPeriod)); 
				interceptSeries			= new Series<double>(this, MaximumBarsLookBack.Infinite);
				slopeSeries				= new Series<double>(this, MaximumBarsLookBack.Infinite);
				stdDeviationSeries		= new Series<double>(this, MaximumBarsLookBack.Infinite);
				normalizedSlope			= new Series<double>(this, MaximumBarsLookBack.Infinite);
				normalizedChannelWidth	= new Series<double>(this, MaximumBarsLookBack.Infinite);
				regressionTrend 		= new Series<double>(this, MaximumBarsLookBack.Infinite);
			}
			else if (State == State.Historical)
			{
				normFactorSlope1 = Math.Sqrt(regressionPeriod);
				normFactorSlope2 = 180 / Math.PI;
				normFactorWidth = 2 * multiplier;
				if(ChartBars != null)
				{	
					indicatorIsOnPricePanel = (ChartPanel.PanelIndex == ChartBars.Panel);
					errorBrush = ChartControl.Properties.AxisPen.Brush;
					errorBrush.Freeze();
					errorFont = new SimpleFont("Arial", 24);
				}
				else
					indicatorIsOnPricePanel = false;
				errorMessage = false;
				if(regressionPeriod < 2)
				{
					Draw.TextFixed(this, "error text 1", errorText1, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);  
					errorMessage = true;
				}
				else if(displacement != 0)
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
				Middle[0] 	= Input[0];
				Upper[0]	= Input[0];
				Lower[0]	= Input[0];
				slopeSeries[0] = 0.0;
				interceptSeries[0] = Input[0];
				stdDeviationSeries[0] = 0.0;
				regressionTrend[0] = 0.0;
				normalizedSlope[0] = 0.0;
				normalizedChannelWidth[0] = 0.0;
				return;
			}
			
			// linear regression
			lookback = Math.Min(regressionPeriod, CurrentBar + 1);
			if(IsFirstTickOfBar)
			{
				if(CurrentBar > 0)
					avgTrueRange = averageTrueRange[1];
				else
					avgTrueRange = averageTrueRange[0];
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
			slopeSeries[0] = - slope; 
			interceptSeries[0] = intercept;
			if(CurrentBar == 0)
				regressionTrend[0] = 0.0;
			else if(slope < 0)
				regressionTrend[0] = 1.0;
			else if(slope > 0)
				regressionTrend[0] = -1.0;
			else
				regressionTrend[0] = regressionTrend[1];
			
			// standard deviation
			if(IsFirstTickOfBar)
			{	
				preSumResiduals = 0.0;
				preSquareSumResiduals = 0.0;
				for (int count = 1; count < lookback; count++) 
				{
					regressionValue = intercept + slope * count;
					residual = Input[count] - regressionValue;
					preSumResiduals = preSumResiduals + Math.Abs(residual);
					preSquareSumResiduals = preSquareSumResiduals + residual * residual;
				}
			}
			regressionValue = intercept;
			residual = Input[0] - regressionValue;
			sumResiduals = preSumResiduals + Math.Abs(residual);
			avgResiduals = sumResiduals / lookback;
			squareSumResiduals = preSquareSumResiduals + residual * residual;
			stdDeviation = Math.Sqrt(squareSumResiduals / lookback - avgResiduals * avgResiduals); 
			stdDeviationSeries[0] = stdDeviation;
			
			if(avgTrueRange.ApproxCompare(0) > 0)
			{	
				normalizedSlope[0] = normFactorSlope2 * Math.Atan(- normFactorSlope1 * slope / avgTrueRange);
				if(avgTrueRange.ApproxCompare(0) == 0)
					normalizedChannelWidth[0] = 0;
				else
					normalizedChannelWidth[0] = normFactorWidth * stdDeviation / avgTrueRange;
			}	
			else
			{
				normalizedSlope[0] = 0.0;
				normalizedChannelWidth[0] = 0.0;
			}	
			
			Middle[0]	= intercept;
			Upper[0]	= intercept + multiplier * stdDeviation;
			Lower[0]	= intercept - multiplier * stdDeviation;
			if(!showRegressionBands) // needed for price markers
			{
				if(barsAgo == 0)
				{	
					PlotBrushes[0][0] = middleBrush;
					PlotBrushes[1][0] = upperBrush;
					PlotBrushes[2][0] = lowerBrush;
				}
				else
				{	
					PlotBrushes[0][0] = Brushes.Transparent;
					PlotBrushes[1][0] = Brushes.Transparent;
					PlotBrushes[2][0] = Brushes.Transparent;
				}
			}
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Middle
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
		public Series<double> InterceptSeries
		{
			get { return interceptSeries; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SlopeSeries
		{
			get { return slopeSeries; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> StdDeviationSeries
		{
			get { return stdDeviationSeries; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> NormalizedSlope
		{
			get { return normalizedSlope; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> NormalizedChannelWidth
		{
			get { return normalizedChannelWidth; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> RegressionTrend
		{
			get { return regressionTrend; }
		}
		
		[Range(2, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Regression period", Description = "Select lookback period for linear regression", GroupName = "Input Parameters", Order = 0)]
		public int RegressionPeriod
		{	
            get { return regressionPeriod; }
            set { regressionPeriod = value; }
		}

		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Std. dev. multiplier", Description = "Select standard deviation multiplier for linear regression channel", GroupName = "Input Parameters", Order = 1)]
		public double Multiplier
		{	
            get { return multiplier; }
            set { multiplier = value; }
		}
		
		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Displayed bars ago", Description = "Select how many bars ago the regression channel shall be displayed", GroupName = "Display Options", Order = 0)]
      	[RefreshProperties(RefreshProperties.All)] 
		public int BarsAgo
		{	
            get { return barsAgo; }
            set { barsAgo = value; }
		}

		[Display(ResourceType = typeof(Custom.Resource), Name = "Show regression channel", GroupName = "Display Options", Order = 1)]
        [RefreshProperties(RefreshProperties.All)] 
      	public bool ShowRegressionChannel
        {
            get { return showRegressionChannel; }
            set { showRegressionChannel = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show regression line", GroupName = "Display Options", Order = 2)]
      	[RefreshProperties(RefreshProperties.All)] 
        public bool ShowRegressionLine
        {
            get { return showRegressionLine; }
            set { showRegressionLine = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show regression bands", GroupName = "Display Options", Order = 3)]
       	[RefreshProperties(RefreshProperties.All)] 
        public bool ShowRegressionBands
        {
            get { return showRegressionBands; }
            set { showRegressionBands = value; }
        }
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Upper channel line", Description = "Sets the color for the upper line of the regression channel", GroupName = "Linear Regression Channel", Order = 0)]
		public System.Windows.Media.Brush UpperBrush
		{ 
			get {return upperBrush;}
			set {upperBrush = value;}
		}

		[Browsable(false)]
		public string UpperBrushSerializable
		{
			get { return Serialize.BrushToString(upperBrush); }
			set { upperBrush = Serialize.StringToBrush(value); }
		}					
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Upper channel projection", Description = "Sets the color for the upper line projection of the regression channel", GroupName = "Linear Regression Channel", Order = 1)]
		public System.Windows.Media.Brush UpperProjectionBrush
		{ 
			get {return upperProjectionBrush;}
			set {upperProjectionBrush = value;}
		}

		[Browsable(false)]
		public string UpperProjectionBrushSerializable
		{
			get { return Serialize.BrushToString(upperProjectionBrush); }
			set { upperProjectionBrush = Serialize.StringToBrush(value); }
		}					
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style upper line", Description = "Sets the dash style for the upper channel line", GroupName = "Linear Regression Channel", Order = 2)]
		public DashStyleHelper Dash1Style
		{
			get { return dash1Style; }
			set { dash1Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width upper line", Description = "Sets the plot width for the upper channel line", GroupName = "Linear Regression Channel", Order = 3)]
		public int Plot1Width
		{	
            get { return plot1Width; }
            set { plot1Width = value; }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Linear regression line", Description = "Sets the color for the linear regression line", GroupName = "Linear Regression Channel", Order = 4)]
		public System.Windows.Media.Brush MiddleBrush
		{ 
			get {return middleBrush;}
			set {middleBrush = value;}
		}

		[Browsable(false)]
		public string MiddleBrushSerializable
		{
			get { return Serialize.BrushToString(middleBrush); }
			set { middleBrush = Serialize.StringToBrush(value); }
		}					
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Linear regression projection", Description = "Sets the color for the linear regression line projection", GroupName = "Linear Regression Channel", Order = 5)]
		public System.Windows.Media.Brush MiddleProjectionBrush
		{ 
			get {return middleProjectionBrush;}
			set {middleProjectionBrush = value;}
		}

		[Browsable(false)]
		public string MiddleProjectionBrushSerializable
		{
			get { return Serialize.BrushToString(middleProjectionBrush); }
			set { middleProjectionBrush = Serialize.StringToBrush(value); }
		}					
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style regression line", Description = "Sets the dash style for the linear regression line", GroupName = "Linear Regression Channel", Order = 6)]
		public DashStyleHelper Dash0Style
		{
			get { return dash0Style; }
			set { dash0Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width regression line", Description = "Sets the plot width for the linear regression line", GroupName = "Linear Regression Channel", Order = 7)]
		public int Plot0Width
		{	
            get { return plot0Width; }
            set { plot0Width = value; }
		}
			
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Lower channel line", Description = "Sets the color for the lower line of the regression channel", GroupName = "Linear Regression Channel", Order = 8)]
		public System.Windows.Media.Brush LowerBrush
		{ 
			get {return lowerBrush;}
			set {lowerBrush = value;}
		}

		[Browsable(false)]
		public string LowerBrushSerializable
		{
			get { return Serialize.BrushToString(lowerBrush); }
			set { lowerBrush = Serialize.StringToBrush(value); }
		}					
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Lower channel projection", Description = "Sets the color for the lower line projection of the regression channel", GroupName = "Linear Regression Channel", Order = 9)]
		public System.Windows.Media.Brush LowerProjectionBrush
		{ 
			get {return lowerProjectionBrush;}
			set {lowerProjectionBrush = value;}
		}

		[Browsable(false)]
		public string LowerProjectionBrushSerializable
		{
			get { return Serialize.BrushToString(lowerProjectionBrush); }
			set { lowerProjectionBrush = Serialize.StringToBrush(value); }
		}					
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style lower line", Description = "Sets the dash style for the lower channel line", GroupName = "Linear Regression Channel", Order = 10)]
		public DashStyleHelper Dash2Style
		{
			get { return dash2Style; }
			set { dash2Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width lower line", Description = "Sets the plot width for the lower channel line", GroupName = "Linear Regression Channel", Order = 11)]
		public int Plot2Width
		{	
            get { return plot2Width; }
            set { plot2Width = value; }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Vertical line", Description = "Sets the color for the vertical projection start line", GroupName = "Projection Start Line", Order = 0)]
		public System.Windows.Media.Brush VerticalBrush
		{ 
			get {return verticalBrush;}
			set {verticalBrush = value;}
		}

		[Browsable(false)]
		public string VerticalBrushSerializable
		{
			get { return Serialize.BrushToString(verticalBrush); }
			set { verticalBrush = Serialize.StringToBrush(value); }
		}					
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style vertical line", Description = "Sets the dash style for the vertical projection start line", GroupName = "Projection Start Line", Order = 1)]
		public DashStyleHelper Dash3Style
		{
			get { return dash3Style; }
			set { dash3Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width vertical line", Description = "Sets the plot width for the vertical projection start line", GroupName = "Projection Start Line", Order = 2)]
		public int Plot3Width
		{	
            get { return plot3Width; }
            set { plot3Width = value; }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Upper band", Description = "Sets the color for the upper regression band", GroupName = "Regression Bands", Order = 0)]
		public System.Windows.Media.Brush UpperBrushB
		{ 
			get {return upperBrushB;}
			set {upperBrushB = value;}
		}

		[Browsable(false)]
		public string UpperBrushBSerializable
		{
			get { return Serialize.BrushToString(upperBrushB); }
			set { upperBrushB = Serialize.StringToBrush(value); }
		}					
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot style upper band", Description = "Sets the plot style for the upper regression band", GroupName = "Regression Bands", Order = 1)]
		public PlotStyle Plot1StyleB
		{
			get { return plot1StyleB; }
			set { plot1StyleB = value; }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style upper band", Description = "Sets the dash style for the upper regression band", GroupName = "Regression Bands", Order = 2)]
		public DashStyleHelper Dash1StyleB
		{
			get { return dash1StyleB; }
			set { dash1StyleB = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width upper band", Description = "Sets the plot width for the upper regression band", GroupName = "Regression Bands", Order = 3)]
		public int Plot1WidthB
		{	
            get { return plot1WidthB; }
            set { plot1WidthB = value; }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Midband", Description = "Sets the color for the linear regression plot", GroupName = "Regression Bands", Order = 4)]
		public System.Windows.Media.Brush MiddleBrushB
		{ 
			get {return middleBrushB;}
			set {middleBrushB = value;}
		}

		[Browsable(false)]
		public string MiddleBrushBSerializable
		{
			get { return Serialize.BrushToString(middleBrushB); }
			set { middleBrushB = Serialize.StringToBrush(value); }
		}					
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot style midband", Description = "Sets the plot  style for the linear regression plot", GroupName = "Regression Bands", Order = 5)]
		public PlotStyle Plot0StyleB
		{
			get { return plot0StyleB; }
			set { plot0StyleB = value; }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style midband", Description = "Sets the dash style for the linear regression plot", GroupName = "Regression Bands", Order = 6)]
		public DashStyleHelper Dash0StyleB
		{
			get { return dash0StyleB; }
			set { dash0StyleB = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width midband", Description = "Sets the plot width for the linear regression plot", GroupName = "Regression Bands", Order = 7)]
		public int Plot0WidthB
		{	
            get { return plot0WidthB; }
            set { plot0WidthB = value; }
		}
			
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Lower band", Description = "Sets the color for the lower regression band", GroupName = "Regression Bands", Order = 8)]
		public System.Windows.Media.Brush LowerBrushB
		{ 
			get {return lowerBrushB;}
			set {lowerBrushB = value;}
		}

		[Browsable(false)]
		public string LowerBrushBSerializable
		{
			get { return Serialize.BrushToString(lowerBrushB); }
			set { lowerBrushB = Serialize.StringToBrush(value); }
		}					
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot style lower band", Description = "Sets the plot style for the lower regression band", GroupName = "Regression Bands", Order = 9)]
		public PlotStyle Plot2StyleB
		{
			get { return plot2StyleB; }
			set { plot2StyleB = value; }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style lower band", Description = "Sets the dash style for the lower regression band", GroupName = "Regression Bands", Order = 10)]
		public DashStyleHelper Dash2StyleB
		{
			get { return dash2StyleB; }
			set { dash2StyleB = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width lower band", Description = "Sets the plot width for lower regression band", GroupName = "Regression Bands", Order = 11)]
		public int Plot2WidthB
		{	
            get { return plot2WidthB; }
            set { plot2WidthB = value; }
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

		public override void OnRenderTargetChanged()
		{
			if (middleBrushDX != null)
				middleBrushDX.Dispose();
			if (upperBrushDX != null)
				upperBrushDX.Dispose();
			if (lowerBrushDX != null)
				lowerBrushDX.Dispose();
			if (middleProjectionBrushDX != null)
				middleProjectionBrushDX.Dispose();
			if (upperProjectionBrushDX != null)
				upperProjectionBrushDX.Dispose();
			if (lowerProjectionBrushDX != null)
				lowerProjectionBrushDX.Dispose();
			if (verticalBrushDX != null)
				verticalBrushDX.Dispose();

			if (RenderTarget != null)
			{
				try
				{
					middleBrushDX 	= middleBrush.ToDxBrush(RenderTarget);
					upperBrushDX 	= upperBrush.ToDxBrush(RenderTarget);
					lowerBrushDX 	= lowerBrush.ToDxBrush(RenderTarget);
					middleProjectionBrushDX = middleProjectionBrush.ToDxBrush(RenderTarget);
					upperProjectionBrushDX 	= upperProjectionBrush.ToDxBrush(RenderTarget);
					lowerProjectionBrushDX 	= lowerProjectionBrush.ToDxBrush(RenderTarget);
					verticalBrushDX	= verticalBrush.ToDxBrush(RenderTarget);
				}
				catch (Exception e) { }
			}
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			if (Bars == null || ChartControl == null || !IsVisible) return;

			if(showRegressionBands)
				base.OnRender(chartControl, chartScale);
			
			if(showRegressionChannel || showRegressionLine)
			{
				SharpDX.Direct2D1.AntialiasMode oldAntialiasMode = RenderTarget.AntialiasMode;
				RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
	            ChartPanel panel = chartControl.ChartPanels[ChartPanel.PanelIndex];

	            int			lastBarPainted  = ChartBars.ToIndex;
				int			lastBarCounted	= Input.Count - 1 - (Calculate == Calculate.OnBarClose ? 1 : 0);
				int 		idx				= Math.Min(lastBarPainted, lastBarCounted);
				
				if(idx > regressionPeriod - 2)
				{	
					double		intercept		= interceptSeries.GetValueAt(idx - barsAgo);
					double		slope			= slopeSeries.GetValueAt(idx - barsAgo);
					double		stdDev			= stdDeviationSeries.GetValueAt(idx - barsAgo);
					int			stdDevPixels	= (int) Math.Round(((stdDev*multiplier)/(panel.MaxValue - panel.MinValue))*panel.H, 0);
					int			xPos1			= ChartControl.GetXByBarIndex(ChartBars, idx - barsAgo);
					int			yPos1			= chartScale.GetYByValue(intercept);
					int			xPos0			= ChartControl.GetXByBarIndex(ChartBars, idx - (barsAgo + regressionPeriod - 1));
					int			yPos0			= chartScale.GetYByValue(intercept - slope*(regressionPeriod - 1));
					int			xPos2			= ChartControl.GetXByBarIndex(ChartBars, lastBarPainted);
					int			yPos2			= chartScale.GetYByValue(intercept + slope*(barsAgo + (lastBarPainted > idx ? 1 : 0)));
					
					if(barsAgo == 0)
					{	
						Vector2	startVector	= new Vector2(xPos0, yPos0);
						Vector2	endVector	= new Vector2(xPos2, yPos2);
						if(showRegressionLine)
							RenderTarget.DrawLine(startVector, endVector, middleBrushDX, plot0Width, stroke0.StrokeStyle);
						if(showRegressionChannel)
						{	
							RenderTarget.DrawLine(new Vector2(startVector.X, startVector.Y - stdDevPixels), new Vector2(endVector.X, endVector.Y - stdDevPixels), upperBrushDX, plot1Width, stroke1.StrokeStyle);
							RenderTarget.DrawLine(new Vector2(startVector.X, startVector.Y + stdDevPixels), new Vector2(endVector.X, endVector.Y + stdDevPixels), lowerBrushDX, plot2Width, stroke2.StrokeStyle);
						}
					}
					else
					{	
						Vector2	startVector	= new Vector2(xPos0, yPos0);
						Vector2	midVector	= new Vector2(xPos1, yPos1);
						Vector2	endVector	= new Vector2(xPos2, yPos2);
						if(showRegressionLine)
						{	
							RenderTarget.DrawLine(startVector, midVector, middleBrushDX, plot0Width, stroke0.StrokeStyle);
							RenderTarget.DrawLine(midVector, endVector, middleProjectionBrushDX, plot0Width, stroke0.StrokeStyle);
						}	
						if(showRegressionChannel)
						{	
							RenderTarget.DrawLine(new Vector2(startVector.X, startVector.Y - stdDevPixels), new Vector2(midVector.X, midVector.Y - stdDevPixels), upperBrushDX, plot1Width, stroke1.StrokeStyle);
							RenderTarget.DrawLine(new Vector2(midVector.X, midVector.Y - stdDevPixels), new Vector2(endVector.X, endVector.Y - stdDevPixels), upperProjectionBrushDX, plot1Width, stroke1.StrokeStyle);
							RenderTarget.DrawLine(new Vector2(startVector.X, startVector.Y + stdDevPixels), new Vector2(midVector.X, midVector.Y + stdDevPixels), lowerBrushDX, plot2Width, stroke2.StrokeStyle);
							RenderTarget.DrawLine(new Vector2(midVector.X, midVector.Y + stdDevPixels), new Vector2(endVector.X, endVector.Y + stdDevPixels), lowerProjectionBrushDX, plot2Width, stroke2.StrokeStyle);
						}
						RenderTarget.DrawLine(new Vector2(xPos1, ChartPanel.Y), new Vector2(xPos1, ChartPanel.Y + ChartPanel.H), verticalBrushDX, plot3Width, stroke3.StrokeStyle);
					}	
				}
				RenderTarget.AntialiasMode = oldAntialiasMode;
			}
		}
		#endregion
	}
}

namespace NinjaTrader.NinjaScript.Indicators
{
	public class amaRegressionChannelTypeConverter : NinjaTrader.NinjaScript.IndicatorBaseConverter
	{
		public override bool GetPropertiesSupported(ITypeDescriptorContext context) { return true; }

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			PropertyDescriptorCollection propertyDescriptorCollection = base.GetPropertiesSupported(context) ? base.GetProperties(context, value, attributes) : TypeDescriptor.GetProperties(value, attributes);

			amaRegressionChannel	thisRegressionChannelInstance		= (amaRegressionChannel) value;
			int						barsAgoFromInstance					= thisRegressionChannelInstance.BarsAgo;
			bool					showRegressionLineFromInstance		= thisRegressionChannelInstance.ShowRegressionLine;
			bool					showRegressionChannelFromInstance	= thisRegressionChannelInstance.ShowRegressionChannel;
			bool					showRegressionBandsFromInstance			= thisRegressionChannelInstance.ShowRegressionBands;
			
			PropertyDescriptorCollection adjusted = new PropertyDescriptorCollection(null);
			
			foreach (PropertyDescriptor thisDescriptor in propertyDescriptorCollection)
			{
				if(barsAgoFromInstance == 0 && (thisDescriptor.Name == "UpperProjectionBrush" || thisDescriptor.Name == "MiddleProjectionBrush" || thisDescriptor.Name == "LowerProjectionBrush"
					|| thisDescriptor.Name == "VerticalBrush" || thisDescriptor.Name == "Dash3Style" || thisDescriptor.Name == "Plot3Width"))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if(!showRegressionLineFromInstance && !showRegressionChannelFromInstance && (thisDescriptor.Name == "UpperBrush" || thisDescriptor.Name == "UpperProjectionBrush" || thisDescriptor.Name == "Dash1Style"
					|| thisDescriptor.Name == "Plot1Width" || thisDescriptor.Name == "MiddleBrush" || thisDescriptor.Name == "MiddleProjectionBrush" || thisDescriptor.Name == "Dash0Style" || thisDescriptor.Name == "Plot0Width"
					|| thisDescriptor.Name == "LowerBrush" || thisDescriptor.Name == "LowerProjectionBrush" || thisDescriptor.Name == "Dash2Style" || thisDescriptor.Name == "Plot2Width" || thisDescriptor.Name == "VerticalBrush"
					|| thisDescriptor.Name == "Dash3Style"|| thisDescriptor.Name == "Plot3Width"))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if(!showRegressionLineFromInstance && (thisDescriptor.Name == "MiddleBrush" || thisDescriptor.Name == "MiddleProjectionBrush" || thisDescriptor.Name == "Dash0Style" || thisDescriptor.Name == "Plot0Width"))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if(!showRegressionChannelFromInstance && (thisDescriptor.Name == "UpperBrush" || thisDescriptor.Name == "UpperProjectionBrush" || thisDescriptor.Name == "Dash1Style" || thisDescriptor.Name == "Plot1Width" 
					|| thisDescriptor.Name == "LowerBrush" || thisDescriptor.Name == "LowerProjectionBrush" || thisDescriptor.Name == "Dash2Style" 
					|| thisDescriptor.Name == "Plot2Width" ))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if(!showRegressionBandsFromInstance && (thisDescriptor.Name == "UpperBrushB" || thisDescriptor.Name == "Plot1StyleB" || thisDescriptor.Name == "Dash1StyleB" || thisDescriptor.Name == "Plot1WidthB" 
					|| thisDescriptor.Name == "MiddleBrushB" || thisDescriptor.Name == "Plot0StyleB" || thisDescriptor.Name == "Dash0StyleB" || thisDescriptor.Name == "Plot0WidthB" 
					|| thisDescriptor.Name == "LowerBrushB" || thisDescriptor.Name == "Plot2StyleB" || thisDescriptor.Name == "Dash2StyleB" || thisDescriptor.Name == "Plot2WidthB")) 
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else	
					adjusted.Add(thisDescriptor);
			}
			return adjusted;		
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LizardIndicators.amaRegressionChannel[] cacheamaRegressionChannel;
		public LizardIndicators.amaRegressionChannel amaRegressionChannel(int regressionPeriod, double multiplier)
		{
			return amaRegressionChannel(Input, regressionPeriod, multiplier);
		}

		public LizardIndicators.amaRegressionChannel amaRegressionChannel(ISeries<double> input, int regressionPeriod, double multiplier)
		{
			if (cacheamaRegressionChannel != null)
				for (int idx = 0; idx < cacheamaRegressionChannel.Length; idx++)
					if (cacheamaRegressionChannel[idx] != null && cacheamaRegressionChannel[idx].RegressionPeriod == regressionPeriod && cacheamaRegressionChannel[idx].Multiplier == multiplier && cacheamaRegressionChannel[idx].EqualsInput(input))
						return cacheamaRegressionChannel[idx];
			return CacheIndicator<LizardIndicators.amaRegressionChannel>(new LizardIndicators.amaRegressionChannel(){ RegressionPeriod = regressionPeriod, Multiplier = multiplier }, input, ref cacheamaRegressionChannel);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaRegressionChannel amaRegressionChannel(int regressionPeriod, double multiplier)
		{
			return indicator.amaRegressionChannel(Input, regressionPeriod, multiplier);
		}

		public Indicators.LizardIndicators.amaRegressionChannel amaRegressionChannel(ISeries<double> input , int regressionPeriod, double multiplier)
		{
			return indicator.amaRegressionChannel(input, regressionPeriod, multiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaRegressionChannel amaRegressionChannel(int regressionPeriod, double multiplier)
		{
			return indicator.amaRegressionChannel(Input, regressionPeriod, multiplier);
		}

		public Indicators.LizardIndicators.amaRegressionChannel amaRegressionChannel(ISeries<double> input , int regressionPeriod, double multiplier)
		{
			return indicator.amaRegressionChannel(input, regressionPeriod, multiplier);
		}
	}
}

#endregion
