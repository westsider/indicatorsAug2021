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
	/// The Laguerre Filter is a smooth IIR filter which is derived from a triangular moving average. This filter is based on the Easy Language version published by John F. Ehlers
	/// in his paper 'Time Warp without Space Travel'. In order to simplify the use of the indicator, the gamma coefficient has been calculated from a lookback period.
	/// </summary>
	[Gui.CategoryOrder("Input Parameters", 1000100)]
	[Gui.CategoryOrder("Version", 8000100)]
	public class amaLaguerreFilter : Indicator
	{
        private int 				period 						= 9;
        private double 				alpha 						= 0.0;
        private double 				gamma						= 0.0;
        private double 				recurrentPart0 				= 0.0;
		private double				recurrentPart1				= 0.0;
		private double				recurrentPart2				= 0.0;
		private double				recurrentPart3				= 0.0;
		private bool				indicatorIsOnPricePanel		= true;
		private string				versionString				= "v 1.1  -  March 7, 2020";
		private Series<double>		ls0;
		private Series<double>		ls1;
		private Series<double>		ls2;
		private Series<double>		ls3;
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\nThe Laguerre Filter is a smooth IIR filter which is derived from a triangular moving average. This filter is based on the Easy Language version published by John F. Ehlers"
											  + " in his paper 'Time Warp without Space Travel'. In order to simplify the use of the indicator, the gamma coefficient has been calculated from a lookback period."; 										
				
				Name						= "amaLaguerreFilter";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				AddPlot(new Stroke(Brushes.MediumBlue, 2), PlotStyle.Line, "Laguerre Filter");	
			}
			else if (State == State.Configure)
			{
				BarsRequiredToPlot = 3 * period;
			}
			else if (State == State.DataLoaded)
			{
				ls0 = new Series<double>(this);
				ls1 = new Series<double>(this);
				ls2 = new Series<double>(this);
				ls3 = new Series<double>(this);
			}	
			else if (State == State.Historical)
			{
				alpha = 2.0 / (1.9 + 0.1 * period);
				gamma = 1 - alpha;
				if(ChartBars != null)
					indicatorIsOnPricePanel = (ChartPanel.PanelIndex == ChartBars.Panel);
				else
					indicatorIsOnPricePanel = false;
			}	
		}

		protected override void OnBarUpdate()
		{
            if (CurrentBar < 1)
            {
                ls0[0] = Input[0];
                ls1[0] = Input[0];
                ls2[0] = Input[0];
                ls3[0] = Input[0];
				Laguerre[0] = Input[0];
				return;
            }
          	if (IsFirstTickOfBar)
			{
				recurrentPart0 = gamma * ls0[1];
				recurrentPart1 = ls0[1] + gamma * ls1[1];
				recurrentPart2 = ls1[1] + gamma * ls2[1];
				recurrentPart3 = ls2[1] + gamma * ls3[1];
			}
			ls0[0] = alpha * Input[0] + recurrentPart0;
			ls1[0] = -gamma * ls0[0] + recurrentPart1;
			ls2[0] = -gamma * ls1[0] + recurrentPart2;
			ls3[0] = -gamma * ls2[0] + recurrentPart3;
			Laguerre[0] = ((ls0[0] + 2*ls1[0] + 2*ls2[0] + ls3[0])/6);
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Laguerre
		{
			get { return Values[0]; }
		}
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "Input Parameters", Order = 1)]
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
		private LizardIndicators.amaLaguerreFilter[] cacheamaLaguerreFilter;
		public LizardIndicators.amaLaguerreFilter amaLaguerreFilter(int period)
		{
			return amaLaguerreFilter(Input, period);
		}

		public LizardIndicators.amaLaguerreFilter amaLaguerreFilter(ISeries<double> input, int period)
		{
			if (cacheamaLaguerreFilter != null)
				for (int idx = 0; idx < cacheamaLaguerreFilter.Length; idx++)
					if (cacheamaLaguerreFilter[idx] != null && cacheamaLaguerreFilter[idx].Period == period && cacheamaLaguerreFilter[idx].EqualsInput(input))
						return cacheamaLaguerreFilter[idx];
			return CacheIndicator<LizardIndicators.amaLaguerreFilter>(new LizardIndicators.amaLaguerreFilter(){ Period = period }, input, ref cacheamaLaguerreFilter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaLaguerreFilter amaLaguerreFilter(int period)
		{
			return indicator.amaLaguerreFilter(Input, period);
		}

		public Indicators.LizardIndicators.amaLaguerreFilter amaLaguerreFilter(ISeries<double> input , int period)
		{
			return indicator.amaLaguerreFilter(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaLaguerreFilter amaLaguerreFilter(int period)
		{
			return indicator.amaLaguerreFilter(Input, period);
		}

		public Indicators.LizardIndicators.amaLaguerreFilter amaLaguerreFilter(ISeries<double> input , int period)
		{
			return indicator.amaLaguerreFilter(input, period);
		}
	}
}

#endregion
