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
	/// The Triple Weighted Moving Average (TWMA) is calculated in a similar way as the Triple Exponential Moving Average (TEMA), but has the exponential moving averages replaced with weighted moving averages.
	/// </summary>
	[Gui.CategoryOrder("Input Parameters", 1000100)]
	[Gui.CategoryOrder("Version", 8000100)]
	public class amaTWMA : Indicator
	{
		private int					period						= 14;
		private bool				indicatorIsOnPricePanel		= true;
		private string				versionString				= "v 1.1  -  March 7, 2020";
		private WMA					wma1;
		private WMA					wma2;
		private WMA					wma3;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\n The Triple Weighted Moving Average (TWMA) is calculated in a similar way as the Triple Exponential Moving Average (TEMA),"
												+ " but has the exponential moving averages replaced with weighted moving averages.";
				Name						= "amaTWMA";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				AddPlot(new Stroke(Brushes.DarkOrange, 2), PlotStyle.Line, "TWMA");	
			}
			else if (State == State.Configure)
			{
				BarsRequiredToPlot = 3 * period;
			}
			else if (State == State.DataLoaded)
			{
				wma1 = WMA(Inputs[0], period);
				wma2 = WMA(wma1, period);
				wma3 = WMA(wma2, period);
			}	
			else if (State == State.Historical)
			{
				if(ChartBars != null)
					indicatorIsOnPricePanel = (ChartPanel.PanelIndex == ChartBars.Panel);
				else
					indicatorIsOnPricePanel = false;
			}
		}

		protected override void OnBarUpdate()
		{
            TWMA[0] = 3*wma1[0] - 3*wma2[0] + wma3[0];
		}
		
		#region Properties
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TWMA
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
		private LizardIndicators.amaTWMA[] cacheamaTWMA;
		public LizardIndicators.amaTWMA amaTWMA(int period)
		{
			return amaTWMA(Input, period);
		}

		public LizardIndicators.amaTWMA amaTWMA(ISeries<double> input, int period)
		{
			if (cacheamaTWMA != null)
				for (int idx = 0; idx < cacheamaTWMA.Length; idx++)
					if (cacheamaTWMA[idx] != null && cacheamaTWMA[idx].Period == period && cacheamaTWMA[idx].EqualsInput(input))
						return cacheamaTWMA[idx];
			return CacheIndicator<LizardIndicators.amaTWMA>(new LizardIndicators.amaTWMA(){ Period = period }, input, ref cacheamaTWMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaTWMA amaTWMA(int period)
		{
			return indicator.amaTWMA(Input, period);
		}

		public Indicators.LizardIndicators.amaTWMA amaTWMA(ISeries<double> input , int period)
		{
			return indicator.amaTWMA(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaTWMA amaTWMA(int period)
		{
			return indicator.amaTWMA(Input, period);
		}

		public Indicators.LizardIndicators.amaTWMA amaTWMA(ISeries<double> input , int period)
		{
			return indicator.amaTWMA(input, period);
		}
	}
}

#endregion
