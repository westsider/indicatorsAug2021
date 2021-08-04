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
	public class amaDistantCoefficientFilter : Indicator
	{
		private int 				period 						= 15; 
		private int 				length						= 0;
		private double				distance					= 0.0;
		private double				num1						= 0.0;
		private double				sumCoef1					= 0.0;
		private double 				num 						= 0.0;
		private double 				sumCoef  					= 0.0;
		private bool				indicatorIsOnPricePanel		= true;
		private List<double> 		coef						= new List<double>();        
		private string				versionString				= "v 1.1  -  March 7, 2020";
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\nThe Distant Coefficient filter is an adaptive non-linear FIR (finite impulse response) filter. It is calculated by applying 'distant coefficients' to prices over "
											  + "a user selectable lookback period. The filter is derived from the Distant Coefficient filter which was presented by John F. Ehlers in his paper 'Ehlers Filters'.";          
				Name						= "amaDistantCoefficientFilter";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				AddPlot(new Stroke(Brushes.MediumBlue, 2), PlotStyle.Line, "Distant Coefficient Filter");	
			}
			else if (State == State.Configure)
			{
				BarsRequiredToPlot = 2 * period;
			}
			else if (State == State.Historical)
			{
				if(coef.Count < period)
				{	
					for (int i=0; i < period; i++)
						coef.Add(0.0);
				}	
				if(ChartBars != null)
					indicatorIsOnPricePanel = (ChartPanel.PanelIndex == ChartBars.Panel);
				else
					indicatorIsOnPricePanel = false;
			}	
		}

		protected override void OnBarUpdate()
		{
            if (CurrentBar < 2)
            {
                DCFilter[0] = Input[0];
				return;
            }
 			if (CurrentBar < 2 * period - 1)
				length = CurrentBar/2 + 1;
			else
				length = period;
			
         	if (IsFirstTickOfBar)
			{
				num1 = 0.0;
				sumCoef1 = 0.0;
				for (int i = 1; i < length; i++)
				{
					distance = 0;
					for (int j = 1; j < length; j++)						
						distance = distance + (Input[i] - Input[i + j])*(Input[i] - Input[i + j]);		
					coef[i] = distance;
					num1 = num1 + coef[i]*Input[i];
					sumCoef1 = sumCoef1 + coef[i];
				}
			}
			distance = 0;
			for (int j = 1; j < length; j++)						
				distance = distance + (Input[0] - Input[j])*(Input[0] - Input[j]);		
			coef[0] = distance;
			num = num1 + coef[0]*Input[0];
			sumCoef = sumCoef1 + coef[0];
         	if(Math.Abs (sumCoef) > 100*double.Epsilon) 
				DCFilter[0] = num/sumCoef;
			else if (period > 1)
				DCFilter[0] = DCFilter[1];
			else
				DCFilter[0] = Input[0];
		}

		#region Properties
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DCFilter
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
		private LizardIndicators.amaDistantCoefficientFilter[] cacheamaDistantCoefficientFilter;
		public LizardIndicators.amaDistantCoefficientFilter amaDistantCoefficientFilter(int period)
		{
			return amaDistantCoefficientFilter(Input, period);
		}

		public LizardIndicators.amaDistantCoefficientFilter amaDistantCoefficientFilter(ISeries<double> input, int period)
		{
			if (cacheamaDistantCoefficientFilter != null)
				for (int idx = 0; idx < cacheamaDistantCoefficientFilter.Length; idx++)
					if (cacheamaDistantCoefficientFilter[idx] != null && cacheamaDistantCoefficientFilter[idx].Period == period && cacheamaDistantCoefficientFilter[idx].EqualsInput(input))
						return cacheamaDistantCoefficientFilter[idx];
			return CacheIndicator<LizardIndicators.amaDistantCoefficientFilter>(new LizardIndicators.amaDistantCoefficientFilter(){ Period = period }, input, ref cacheamaDistantCoefficientFilter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaDistantCoefficientFilter amaDistantCoefficientFilter(int period)
		{
			return indicator.amaDistantCoefficientFilter(Input, period);
		}

		public Indicators.LizardIndicators.amaDistantCoefficientFilter amaDistantCoefficientFilter(ISeries<double> input , int period)
		{
			return indicator.amaDistantCoefficientFilter(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaDistantCoefficientFilter amaDistantCoefficientFilter(int period)
		{
			return indicator.amaDistantCoefficientFilter(Input, period);
		}

		public Indicators.LizardIndicators.amaDistantCoefficientFilter amaDistantCoefficientFilter(ISeries<double> input , int period)
		{
			return indicator.amaDistantCoefficientFilter(input, period);
		}
	}
}

#endregion
