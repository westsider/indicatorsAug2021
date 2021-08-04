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
	/// The Exponential Hull Moving Average is calculated in a similar way as the traditional Hull Moving Average, but has the weighted moving averages replaced with exponential moving averages.
	/// </summary>
	[Gui.CategoryOrder("Input Parameters", 1000100)]
	[Gui.CategoryOrder("Version", 8000100)]
	public class amaEHMA : Indicator
	{
		private int					period						= 21;
 		private double				k1							= 0.0;
 		private double				k2							= 0.0;
 		private double				k3							= 0.0;
		private double				recurrentPart1				= 0.0;
		private double				recurrentPart2				= 0.0;
		private double				recurrentPart3				= 0.0;
		private bool				indicatorIsOnPricePanel		= true;
		private string				versionString				= "v 1.3  -  March 7, 2020";
		private Series<double>		ema1;
		private Series<double>		ema2;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\n The Exponential Hull Moving Average is calculated in a similar way as the traditional Hull Moving Average,"
												+ " but has the weighted moving averages replaced with exponential moving averages.";
				Name						= "amaEHMA";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				AddPlot(new Stroke(Brushes.DarkMagenta, 2), PlotStyle.Line, "EHMA");	
			}
			else if (State == State.Configure)
			{
				BarsRequiredToPlot = 3 * period;
			}
			else if (State == State.DataLoaded)
			{
				ema1 = new Series<double>(this);
				ema2 = new Series<double>(this);
			}
			else if (State == State.Historical)
			{
				k1 = 2.0 / (1 + period);
				k2 = Math.Min(1.0, 2.0 / (1 + 0.5* period));
				k3 = 2.0 / (1 + Math.Sqrt(period));  
				if(ChartBars != null)
					indicatorIsOnPricePanel = (ChartPanel.PanelIndex == ChartBars.Panel);
				else
					indicatorIsOnPricePanel = false;
			}	
		}
		
		protected override void OnBarUpdate()
		{
			if (CurrentBar == 0)
			{
				ema1[0] = Input[0];
				ema2[0] = Input[0];
				EHMA[0] = Input[0];
				return;
			}	
			
			if(IsFirstTickOfBar)
			{	
				recurrentPart1 = (1 - k1) * ema1[1];
				recurrentPart2 = (1 - k2) * ema2[1];
				recurrentPart3 = (1 - k3) * EHMA[1];
			}	
			ema1[0] = k1 * Input[0] + recurrentPart1;
			ema2[0] = k2 * Input[0] + recurrentPart2;
			EHMA[0] = k3 * (2*ema2[0] - ema1[0]) + recurrentPart3;
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> EHMA
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
		private LizardIndicators.amaEHMA[] cacheamaEHMA;
		public LizardIndicators.amaEHMA amaEHMA(int period)
		{
			return amaEHMA(Input, period);
		}

		public LizardIndicators.amaEHMA amaEHMA(ISeries<double> input, int period)
		{
			if (cacheamaEHMA != null)
				for (int idx = 0; idx < cacheamaEHMA.Length; idx++)
					if (cacheamaEHMA[idx] != null && cacheamaEHMA[idx].Period == period && cacheamaEHMA[idx].EqualsInput(input))
						return cacheamaEHMA[idx];
			return CacheIndicator<LizardIndicators.amaEHMA>(new LizardIndicators.amaEHMA(){ Period = period }, input, ref cacheamaEHMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaEHMA amaEHMA(int period)
		{
			return indicator.amaEHMA(Input, period);
		}

		public Indicators.LizardIndicators.amaEHMA amaEHMA(ISeries<double> input , int period)
		{
			return indicator.amaEHMA(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaEHMA amaEHMA(int period)
		{
			return indicator.amaEHMA(Input, period);
		}

		public Indicators.LizardIndicators.amaEHMA amaEHMA(ISeries<double> input , int period)
		{
			return indicator.amaEHMA(input, period);
		}
	}
}

#endregion
