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
	/// Zerolagging TEMA as published by Sylvain Vervoort in 'The Quest For Reliable Crossovers', Technical Analysis of Stocks & Commodities, May 2008.
	/// </summary>
	[Gui.CategoryOrder("Input Parameters", 1000100)]
	[Gui.CategoryOrder("Version", 8000100)]
	public class amaZerolagTEMA : Indicator
	{
        private int 				period 						= 14;
		private bool				indicatorIsOnPricePanel		= true;
		private string				versionString				= "v 1.1  -  March 7, 2020";
		private	TEMA				tema1;
		private	TEMA				tema2;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\nZerolagging TEMA as published by Sylvain Vervoort in 'The Quest For Reliable Crossovers', Technical Analysis of Stocks & Commodities, May 2008.";
				Name						= "amaZerolagTEMA";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				AddPlot(new Stroke(Brushes.DarkOrange, 2), PlotStyle.Line, "Zerolag TEMA");	
			}
			else if (State == State.Configure)
			{
				BarsRequiredToPlot	= 6 * period;
			}
			else if (State == State.DataLoaded)
			{
				tema1 = TEMA(Input, period);
				tema2 = TEMA(tema1, period);
			}	
			else if(State == State.Historical)
			{	
				if(ChartBars != null)
					indicatorIsOnPricePanel = (ChartPanel.PanelIndex == ChartBars.Panel);
				else
					indicatorIsOnPricePanel = false;
			}	
		}

		protected override void OnBarUpdate()
		{
            ZerolagTEMA[0] = 2*tema1[0] - tema2[0];
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ZerolagTEMA
		{
			get { return Values[0]; }
		}
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", Description = "Sets the lookback period for the triple exponential moving averages", GroupName = "Input Parameters", Order = 0)]
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
		private LizardIndicators.amaZerolagTEMA[] cacheamaZerolagTEMA;
		public LizardIndicators.amaZerolagTEMA amaZerolagTEMA(int period)
		{
			return amaZerolagTEMA(Input, period);
		}

		public LizardIndicators.amaZerolagTEMA amaZerolagTEMA(ISeries<double> input, int period)
		{
			if (cacheamaZerolagTEMA != null)
				for (int idx = 0; idx < cacheamaZerolagTEMA.Length; idx++)
					if (cacheamaZerolagTEMA[idx] != null && cacheamaZerolagTEMA[idx].Period == period && cacheamaZerolagTEMA[idx].EqualsInput(input))
						return cacheamaZerolagTEMA[idx];
			return CacheIndicator<LizardIndicators.amaZerolagTEMA>(new LizardIndicators.amaZerolagTEMA(){ Period = period }, input, ref cacheamaZerolagTEMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaZerolagTEMA amaZerolagTEMA(int period)
		{
			return indicator.amaZerolagTEMA(Input, period);
		}

		public Indicators.LizardIndicators.amaZerolagTEMA amaZerolagTEMA(ISeries<double> input , int period)
		{
			return indicator.amaZerolagTEMA(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaZerolagTEMA amaZerolagTEMA(int period)
		{
			return indicator.amaZerolagTEMA(Input, period);
		}

		public Indicators.LizardIndicators.amaZerolagTEMA amaZerolagTEMA(ISeries<double> input , int period)
		{
			return indicator.amaZerolagTEMA(input, period);
		}
	}
}

#endregion
