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
	/// The Holt EMA is a trend corrected exponential average which is described by a linear trend model.
	/// </summary>
	[Gui.CategoryOrder("Input Parameters", 1000100)]
	[Gui.CategoryOrder("Version", 8000100)]
	public class amaHoltEMA : Indicator
	{
        private int 				period 						= 34;
		private int					trendPeriod					= 34;
		private double				alpha						= 0.0;
		private double				gamma						= 0.0;
		private double				recurrentPart1				= 0.0;
		private double				recurrentPart2 				= 0.0;
		private double				holt						= 0.0;
		private bool				indicatorIsOnPricePanel		= true;
		private string				versionString				= "v 1.1  -  March 7, 2020";
		private Series<double>		holtTrend;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\n The Holt EMA is a trend corrected exponential average which is described by a linear trend model.";
				Name						= "amaHoltEMA";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				AddPlot(new Stroke(Brushes.MediumBlue, 2), PlotStyle.Line, "Holt EMA");
			}
			else if (State == State.Configure)
			{
				BarsRequiredToPlot = period;
			}
			else if (State == State.DataLoaded)
			{
				holtTrend = new Series<double>(this);
			}	
			else if (State == State.Historical)
			{
            	alpha = 2.0/(1 + period);
				gamma = 2.0/(1 + trendPeriod);
				if(ChartBars != null)
					indicatorIsOnPricePanel = (ChartPanel.PanelIndex == ChartBars.Panel);
				else
					indicatorIsOnPricePanel = false;
			}	
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar == 0)
			{
				HoltEMA[0] = Input[0];
				HoltTrend[0] = 0.0;
				return;
			}	
			if(IsFirstTickOfBar)
			{	
				recurrentPart1 = (1 - alpha) * (HoltEMA[1] + HoltTrend[1]); 
				recurrentPart2 = - gamma * HoltEMA[1] + (1 - gamma)* HoltTrend[1];
			}
			holt = alpha * Input[0] + recurrentPart1;
			HoltEMA[0] = holt;
			HoltTrend[0] = gamma * holt + recurrentPart2;
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HoltEMA
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HoltTrend
		{
			get { return holtTrend; }
		}
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "Input Parameters", Order = 0)]
		public int Period
		{	
            get { return period; }
            set { period = value; }
		}
			
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Trend period", Description = "Trend period", GroupName = "Input Parameters", Order = 0)]
		public int TrendPeriod
		{	
            get { return trendPeriod; }
            set { trendPeriod = value; }
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
		private LizardIndicators.amaHoltEMA[] cacheamaHoltEMA;
		public LizardIndicators.amaHoltEMA amaHoltEMA(int period, int trendPeriod)
		{
			return amaHoltEMA(Input, period, trendPeriod);
		}

		public LizardIndicators.amaHoltEMA amaHoltEMA(ISeries<double> input, int period, int trendPeriod)
		{
			if (cacheamaHoltEMA != null)
				for (int idx = 0; idx < cacheamaHoltEMA.Length; idx++)
					if (cacheamaHoltEMA[idx] != null && cacheamaHoltEMA[idx].Period == period && cacheamaHoltEMA[idx].TrendPeriod == trendPeriod && cacheamaHoltEMA[idx].EqualsInput(input))
						return cacheamaHoltEMA[idx];
			return CacheIndicator<LizardIndicators.amaHoltEMA>(new LizardIndicators.amaHoltEMA(){ Period = period, TrendPeriod = trendPeriod }, input, ref cacheamaHoltEMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaHoltEMA amaHoltEMA(int period, int trendPeriod)
		{
			return indicator.amaHoltEMA(Input, period, trendPeriod);
		}

		public Indicators.LizardIndicators.amaHoltEMA amaHoltEMA(ISeries<double> input , int period, int trendPeriod)
		{
			return indicator.amaHoltEMA(input, period, trendPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaHoltEMA amaHoltEMA(int period, int trendPeriod)
		{
			return indicator.amaHoltEMA(Input, period, trendPeriod);
		}

		public Indicators.LizardIndicators.amaHoltEMA amaHoltEMA(ISeries<double> input , int period, int trendPeriod)
		{
			return indicator.amaHoltEMA(input, period, trendPeriod);
		}
	}
}

#endregion
