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
	/// The Adaptive Laguerre Filter is a smooth IIR filter which is derived from a triangular moving average. It is adaptive, because low frequency components are delayed more
	/// than high frequency components. The filter is based on the Easy Language version published by John F. Ehlers in his paper 'Time Warp without Space Travel'.
	/// In order to simplify the use of the indicator, the gamma coefficient has been calculated from a lookback period.
	/// </summary>
	[Gui.CategoryOrder("Input Parameters", 1000100)]
	[Gui.CategoryOrder("Version", 8000800)]
	public class amaAdaptiveLaguerreFilter : Indicator
	{
        private int 				period 						= 20;
        private double 				gamma						= 0.0;
		private double				pAlpha						= 0.0;
		private double				phh							= 0.0;
		private double				pll							= 0.0;
		private double				hh							= 0.0;
		private double				ll							= 0.0;
		private double				lastInput					= 0.0;
		private bool				indicatorIsOnPricePanel		= true;
		private string				versionString				= "v 1.2  -  March 7, 2020";
		private Series<double>		diff;
		private Series<double>		ratio;
		private Series<double>		alpha;
		private Series<double>		ls0;
		private Series<double>		ls1;
		private Series<double>		ls2;
		private Series<double>		ls3;
		private MAX					maxDiff;
		private MIN					minDiff;
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\nThe Adaptive Laguerre Filter is a smooth IIR filter which is derived from a triangular moving average. It is adaptive, because low frequency components are delayed more"  
											  + " than high frequency components. The filter is based on the Easy Language version published by John F. Ehlers in his paper 'Time Warp without Space Travel'."
											  + " In order to simplify the use of the indicator, the gamma coefficient has been calculated from a lookback period."; 										
				Name						= "amaAdaptiveLaguerreFilter";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				AddPlot(new Stroke(Brushes.MediumBlue, 2), PlotStyle.Line, "Adaptive Laguerre Filter");	
			}
			else if (State == State.Configure)
			{
				BarsRequiredToPlot = 3 * period;
			}
			else if (State == State.DataLoaded)
			{
				diff = new Series<double>(this, period < 250 ? MaximumBarsLookBack.TwoHundredFiftySix : MaximumBarsLookBack.Infinite);
				ratio = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				alpha = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				ls0 = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				ls1 = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				ls2 = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				ls3 = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				maxDiff = MAX(diff, period-1);
				minDiff = MIN(diff, period-1);
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
            if (CurrentBar < 1 || period == 1)
            {
 				diff[0] = 0.0;
				ratio[0] = 0.5;
               	alpha[0] = 0.5;
				lastInput = Input[0];
				ls0[0] = Input[0];
                ls1[0] = Input[0];
                ls2[0] = Input[0];
                ls3[0] = Input[0];
				Laguerre[0] = Input[0];
				return;
            }
			if(IsFirstTickOfBar)
			{
				pAlpha = alpha[1];
				phh = maxDiff[1];
				pll = minDiff[1];
			}
			diff[0] = Math.Abs(Input[0] - Laguerre[1]);
			hh = Math.Max(phh, diff[0]);
			ll = Math.Min(pll, diff[0]);
			if(hh > ll)
			{	
				ratio[0] = (diff[0] - ll)/(hh - ll);
				alpha[0] = amaMovingMedian(ratio, 5)[0];
			}
			else
				alpha[0] = pAlpha;
			gamma = 1 - alpha[0];
			ls0[0] = alpha[0] * Input[0] + gamma * ls0[1];
			ls1[0] = -gamma * ls0[0] + ls0[1] + gamma * ls1[1];
			ls2[0] = -gamma * ls1[0] + ls1[1] + gamma * ls2[1];
			ls3[0] = -gamma * ls2[0] + ls2[1] + gamma * ls3[1];
			Laguerre[0] = (ls0[0] + 2*ls1[0] + 2*ls2[0] + ls3[0])/6;			
		}

		#region Properties
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Laguerre
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
		private LizardIndicators.amaAdaptiveLaguerreFilter[] cacheamaAdaptiveLaguerreFilter;
		public LizardIndicators.amaAdaptiveLaguerreFilter amaAdaptiveLaguerreFilter(int period)
		{
			return amaAdaptiveLaguerreFilter(Input, period);
		}

		public LizardIndicators.amaAdaptiveLaguerreFilter amaAdaptiveLaguerreFilter(ISeries<double> input, int period)
		{
			if (cacheamaAdaptiveLaguerreFilter != null)
				for (int idx = 0; idx < cacheamaAdaptiveLaguerreFilter.Length; idx++)
					if (cacheamaAdaptiveLaguerreFilter[idx] != null && cacheamaAdaptiveLaguerreFilter[idx].Period == period && cacheamaAdaptiveLaguerreFilter[idx].EqualsInput(input))
						return cacheamaAdaptiveLaguerreFilter[idx];
			return CacheIndicator<LizardIndicators.amaAdaptiveLaguerreFilter>(new LizardIndicators.amaAdaptiveLaguerreFilter(){ Period = period }, input, ref cacheamaAdaptiveLaguerreFilter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaAdaptiveLaguerreFilter amaAdaptiveLaguerreFilter(int period)
		{
			return indicator.amaAdaptiveLaguerreFilter(Input, period);
		}

		public Indicators.LizardIndicators.amaAdaptiveLaguerreFilter amaAdaptiveLaguerreFilter(ISeries<double> input , int period)
		{
			return indicator.amaAdaptiveLaguerreFilter(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaAdaptiveLaguerreFilter amaAdaptiveLaguerreFilter(int period)
		{
			return indicator.amaAdaptiveLaguerreFilter(Input, period);
		}

		public Indicators.LizardIndicators.amaAdaptiveLaguerreFilter amaAdaptiveLaguerreFilter(ISeries<double> input , int period)
		{
			return indicator.amaAdaptiveLaguerreFilter(input, period);
		}
	}
}

#endregion
