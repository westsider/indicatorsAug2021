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
	/// The Tillson-T3 is a non-linear 6-pole Kaiman filter. Tim Tillson had published the formula in the article 'Smoothing Techniques for More Accurate Signals', 
	/// which was published in the January 1998 issue of Technical Analysis of Stocks and Commodities. The Fulks-Matulich version of the Tillson T3 has the lookback period
	/// adjusted to make it comparable with a standard exponential moving average.
	/// </summary>
	[Gui.CategoryOrder("Input Parameters", 1000100)]
	[Gui.CategoryOrder("Version", 8000100)]
	public class amaTillsonT3 : Indicator
	{
        private int 				period 						= 14;
		private int					smooth						= 0;
		private double				vFactor						= 0.7;
		private double				coef1						= 0.0;
		private double				coef2						= 0.0;
		private double				coef3						= 0.0;
		private double				coef4						= 0.0;
	   	private amaT3CalcMode	 	calcMode 					= amaT3CalcMode.Tillson;
		private bool				indicatorIsOnPricePanel		= true;
		private string				versionString				= "v 1.1  -  March 7, 2020";
		private	EMA					ema1;
		private	EMA					ema2;
		private	EMA					ema3;
		private	EMA					ema4;
		private	EMA					ema5;
		private	EMA					ema6;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\nThe Tillson-T3 is a non-linear 6-pole Kaiman filter. Tim Tillson had published the formula in the article 'Smoothing Techniques for More Accurate Signals', "
												+ "which was published in the January 1998 issue of Technical Analysis of Stocks and Commodities. The Fulks-Matulich version of the Tillson T3 has the lookback period "
												+ " adjusted to make it comparable with a standard exponential moving average.";
				Name						= "amaTillsonT3";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				AddPlot(new Stroke(Brushes.MediumBlue, 2), PlotStyle.Line, "Tillson T3");	
			}
			else if (State == State.Configure)
			{
				BarsRequiredToPlot	= 6 * smooth;
			}
			else if (State == State.DataLoaded)
			{
	            if (calcMode == amaT3CalcMode.Tillson)
					smooth = period;
				else
					smooth = 1 + (int) (0.5 * (period - 1)); 
				ema1 = EMA(Input, smooth);
				ema2 = EMA(ema1, smooth);
				ema3 = EMA(ema2, smooth);
				ema4 = EMA(ema3, smooth);
				ema5 = EMA(ema4, smooth);
				ema6 = EMA(ema5, smooth);
			}	
			else if (State == State.Historical)
			{
				coef1 = - Math.Pow(vFactor, 3);
				coef2 = 3 * Math.Pow(vFactor, 2) + 3 * Math.Pow(vFactor, 3);
				coef3 = - 3 * vFactor - 6 * Math.Pow(vFactor, 2) - 3 * Math.Pow(vFactor, 3);
				coef4 = 1 + 3 * vFactor + 3 * Math.Pow(vFactor, 2) + Math.Pow(vFactor, 3);			
				if(ChartBars != null)
					indicatorIsOnPricePanel = (ChartPanel.PanelIndex == ChartBars.Panel);
				else
					indicatorIsOnPricePanel = false;
			}	
		}

		protected override void OnBarUpdate()
		{
			T3[0] = coef1 * ema6[0] + coef2 * ema5[0] + coef3 * ema4[0] + coef4 * ema3[0];
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> T3
		{
			get { return Values[0]; }
		}
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Formula", Description = "Allows for switching between Tillson and Fulks_Matulich formula", GroupName = "Input Parameters", Order = 0)]
		public amaT3CalcMode CalcMode
		{	
            get { return calcMode; }
            set { calcMode = value; }
		}
			
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", Description = "Sets the lookback period for the exponential moving averages", GroupName = "Input Parameters", Order = 1)]
		public int Period
		{	
            get { return period; }
            set { period = value; }
		}
			
		[Range(0,1), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "V-Factor", Description = "Please select values between 0 and 1", GroupName = "Input Parameters", Order = 2)]
		public double VFactor
		{	
            get { return vFactor; }
            set { vFactor = value; }
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

#region Global Enums

public enum amaT3CalcMode {Tillson, Fulks_Matulich}

#endregion

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LizardIndicators.amaTillsonT3[] cacheamaTillsonT3;
		public LizardIndicators.amaTillsonT3 amaTillsonT3(amaT3CalcMode calcMode, int period, double vFactor)
		{
			return amaTillsonT3(Input, calcMode, period, vFactor);
		}

		public LizardIndicators.amaTillsonT3 amaTillsonT3(ISeries<double> input, amaT3CalcMode calcMode, int period, double vFactor)
		{
			if (cacheamaTillsonT3 != null)
				for (int idx = 0; idx < cacheamaTillsonT3.Length; idx++)
					if (cacheamaTillsonT3[idx] != null && cacheamaTillsonT3[idx].CalcMode == calcMode && cacheamaTillsonT3[idx].Period == period && cacheamaTillsonT3[idx].VFactor == vFactor && cacheamaTillsonT3[idx].EqualsInput(input))
						return cacheamaTillsonT3[idx];
			return CacheIndicator<LizardIndicators.amaTillsonT3>(new LizardIndicators.amaTillsonT3(){ CalcMode = calcMode, Period = period, VFactor = vFactor }, input, ref cacheamaTillsonT3);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaTillsonT3 amaTillsonT3(amaT3CalcMode calcMode, int period, double vFactor)
		{
			return indicator.amaTillsonT3(Input, calcMode, period, vFactor);
		}

		public Indicators.LizardIndicators.amaTillsonT3 amaTillsonT3(ISeries<double> input , amaT3CalcMode calcMode, int period, double vFactor)
		{
			return indicator.amaTillsonT3(input, calcMode, period, vFactor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaTillsonT3 amaTillsonT3(amaT3CalcMode calcMode, int period, double vFactor)
		{
			return indicator.amaTillsonT3(Input, calcMode, period, vFactor);
		}

		public Indicators.LizardIndicators.amaTillsonT3 amaTillsonT3(ISeries<double> input , amaT3CalcMode calcMode, int period, double vFactor)
		{
			return indicator.amaTillsonT3(input, calcMode, period, vFactor);
		}
	}
}

#endregion
