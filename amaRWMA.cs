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
	/// The Range Weighted Moving Average (RWMA) is a weighted average based on the SMA. The coefficient for each bar is calculated from the square of the tick range.";
	/// </summary>
	[Gui.CategoryOrder("Input Parameters", 1000100)]
	[Gui.CategoryOrder("Version", 8000100)]
	public class amaRWMA : Indicator
	{
		private int					period						= 14;
		private int					numBars						= 0;
		private double				preRangePriceSum			= 0.0;
		private double				preRangeSum					= 0.0;
		private double				rangePriceSum				= 0.0;
		private double				rangeSum					= 0.0;
		private bool				calculateFromPriceData		= true;
		private bool				indicatorIsOnPricePanel		= true;
		private Brush				errorBrush					= new SolidColorBrush(System.Windows.Media.Colors.Black);
		private SimpleFont			errorFont					= null;
		private string				errorText					= "The amaRWMA only works on price data.";
		private string				versionString				= "v 1.1  -  March 7, 2020";
		private Series<double>	 	tickRange;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\n The Range Weighted Average (RWMA) is a weighted average based on the SMA. The coefficient for each bar is calculated from the square of the tick range.";
				Name						= "amaRWMA";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				AddPlot(new Stroke(Brushes.DarkOrange, 2), PlotStyle.Line, "RWMA");	
			}
			else if (State == State.Configure)
			{
				BarsRequiredToPlot = period;
			}	
			else if (State == State.DataLoaded)
			{
				tickRange = new Series<double>(this);
				if(Input is PriceSeries)
					calculateFromPriceData = true;
				else
					calculateFromPriceData = false;
			}	
			else if (State == State.Historical)
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
			}	
		}

		protected override void OnBarUpdate()
		{
			if(!calculateFromPriceData)
			{
				DrawOnPricePanel = false;
				Draw.TextFixed(this, "error text", errorText, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);  
				return;
			}	
			tickRange[0] = Math.Pow(1.0 + (High[0] - Low[0])/TickSize, 2);
			numBars = Math.Min(CurrentBar, period);

			if(IsFirstTickOfBar)
			{
				preRangePriceSum = 0.0;
				preRangeSum = 0.0;
				for (int i = 1; i < numBars; i++)
				{
					preRangePriceSum	+= Input[i] * tickRange[i];
					preRangeSum			+= tickRange[i];
				}
			}
			rangePriceSum = preRangePriceSum + Input[0] * tickRange[0];
			rangeSum = preRangeSum + tickRange[0];
			
			RWMA[0] = rangePriceSum / rangeSum;		
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> RWMA
		{
			get { return Values[0]; }
		}
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "Input Parameters", Order = 0)]
		public int Period
		{	
            get { return period; }
            set { period = value; }
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
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LizardIndicators.amaRWMA[] cacheamaRWMA;
		public LizardIndicators.amaRWMA amaRWMA(int period)
		{
			return amaRWMA(Input, period);
		}

		public LizardIndicators.amaRWMA amaRWMA(ISeries<double> input, int period)
		{
			if (cacheamaRWMA != null)
				for (int idx = 0; idx < cacheamaRWMA.Length; idx++)
					if (cacheamaRWMA[idx] != null && cacheamaRWMA[idx].Period == period && cacheamaRWMA[idx].EqualsInput(input))
						return cacheamaRWMA[idx];
			return CacheIndicator<LizardIndicators.amaRWMA>(new LizardIndicators.amaRWMA(){ Period = period }, input, ref cacheamaRWMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaRWMA amaRWMA(int period)
		{
			return indicator.amaRWMA(Input, period);
		}

		public Indicators.LizardIndicators.amaRWMA amaRWMA(ISeries<double> input , int period)
		{
			return indicator.amaRWMA(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaRWMA amaRWMA(int period)
		{
			return indicator.amaRWMA(Input, period);
		}

		public Indicators.LizardIndicators.amaRWMA amaRWMA(ISeries<double> input , int period)
		{
			return indicator.amaRWMA(input, period);
		}
	}
}

#endregion
