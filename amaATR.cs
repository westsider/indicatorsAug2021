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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.LizardIndicators
{
	/// <summary>
	/// The Average True Range (ATR) is a measure of volatility. It was introduced by Welles Wilder in his book 'New Concepts in Technical Trading Systems'
	/// and has since been used as a component  of many indicators and trading systems.
	/// </summary>
	[Gui.CategoryOrder("Algorithmic Options", 1000100)]
	[Gui.CategoryOrder("Input Parameters", 1000200)]
	[Gui.CategoryOrder("Version", 8000100)]
	public class amaATR : Indicator
	{
		private int					period						= 14;
		private double				high0						= 0.0;
		private double				low0						= 0.0;
		private double				close1						= 0.0;
		private double				input1						= 0.0;
		private bool				calculateFromPriceData		= true;		
		private amaATRCalcMode		calcMode					= amaATRCalcMode.Wilder;
		private string				versionString				= "v 1.3  - March 7, 2020";
		private Series<double>		trueRange;
		private ISeries<double>		averageTrueRange;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\nThe Average True Range (ATR) is a measure of volatility. It was introduced by Welles Wilder in his book 'New Concepts in Technical Trading Systems' "
												+ " and has since been used as a component of many indicators and trading systems."; 
				Name						= "amaATR";
				IsSuspendedWhileInactive	= true;
				AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Line, "ATR");
			}
			else if (State == State.Configure)
			{
				if(calcMode	== amaATRCalcMode.Arithmetic)
					BarsRequiredToPlot = period;	
				else if(calcMode == amaATRCalcMode.Exponential)
					BarsRequiredToPlot = 3*period;
				else
					BarsRequiredToPlot = 6*period;	
			}
			else if (State == State.DataLoaded)
			{	
				trueRange =  new Series<double>(this, period < 125 ? MaximumBarsLookBack.TwoHundredFiftySix : MaximumBarsLookBack.Infinite);
				if(calcMode	== amaATRCalcMode.Arithmetic)
					averageTrueRange = SMA(trueRange, period);
				else if(calcMode == amaATRCalcMode.Exponential)
					averageTrueRange = EMA(trueRange, period);
				else
					averageTrueRange = EMA(trueRange, 2*period - 1);
				if(Input is PriceSeries)
					calculateFromPriceData = true;
				else
					calculateFromPriceData = false;
			}
		}

		protected override void OnBarUpdate()
		{
			if(calculateFromPriceData)
			{	
				high0	= High[0];
				low0	= Low[0];
				if (CurrentBar == 0)
				{	
					trueRange[0] 	= high0 - low0;
					ATR[0] 			= high0 - low0;
				}	
				else
				{
					if(IsFirstTickOfBar)
						close1 		= Close[1];
					trueRange[0] 	= Math.Max(high0, close1) - Math.Min(low0, close1);
					ATR[0] 			= averageTrueRange[0];
				}	
			}
			else
			{	
				high0	= Input[0];
				low0	= Input[0];
				if (CurrentBar == 0)
				{	
					trueRange[0] 	= 0;
					ATR[0] 			= 0;
				}	
				else
				{
					if(IsFirstTickOfBar)
						input1 		= Input[1];
					trueRange[0] 	= Math.Abs(Input[0] - input1);
					ATR[0] 			= averageTrueRange[0];
				}
			}
		}

		#region Properties
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ATR
		{
			get { return Values[0]; }
		}
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ATR formula", Description = "Select ATR formula", GroupName = "Algorithmic Options", Order = 0)]
		public amaATRCalcMode CalcMode
		{	
            get { return calcMode; }
            set { calcMode = value; }
		}
			
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ATR period", Description = "Sets the period for the average true range", GroupName = "Input Parameters", Order = 0)]
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
	}
}
	
#region Global Enums

public enum amaATRCalcMode {Arithmetic, Exponential, Wilder}

#endregion

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LizardIndicators.amaATR[] cacheamaATR;
		public LizardIndicators.amaATR amaATR(amaATRCalcMode calcMode, int period)
		{
			return amaATR(Input, calcMode, period);
		}

		public LizardIndicators.amaATR amaATR(ISeries<double> input, amaATRCalcMode calcMode, int period)
		{
			if (cacheamaATR != null)
				for (int idx = 0; idx < cacheamaATR.Length; idx++)
					if (cacheamaATR[idx] != null && cacheamaATR[idx].CalcMode == calcMode && cacheamaATR[idx].Period == period && cacheamaATR[idx].EqualsInput(input))
						return cacheamaATR[idx];
			return CacheIndicator<LizardIndicators.amaATR>(new LizardIndicators.amaATR(){ CalcMode = calcMode, Period = period }, input, ref cacheamaATR);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaATR amaATR(amaATRCalcMode calcMode, int period)
		{
			return indicator.amaATR(Input, calcMode, period);
		}

		public Indicators.LizardIndicators.amaATR amaATR(ISeries<double> input , amaATRCalcMode calcMode, int period)
		{
			return indicator.amaATR(input, calcMode, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaATR amaATR(amaATRCalcMode calcMode, int period)
		{
			return indicator.amaATR(Input, calcMode, period);
		}

		public Indicators.LizardIndicators.amaATR amaATR(ISeries<double> input , amaATRCalcMode calcMode, int period)
		{
			return indicator.amaATR(input, calcMode, period);
		}
	}
}

#endregion
