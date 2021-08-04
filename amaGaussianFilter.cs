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
	/// The Gaussian Filter is a low lag IIR filter which is used to reduce high frequency components of price data. Compared to Butterworth filters
	/// Gaussian filters have a lower degree of smoothing but are minimizing the lag. This version of the indicator is based on the Easy Language version
	/// published by John F. Ehlers in his paper 'Gaussian and Other Low Lag Filters'.
	/// </summary>
	[Gui.CategoryOrder("Input Parameters", 1000100)]
	[Gui.CategoryOrder("Version", 8000100)]
	public class amaGaussianFilter : Indicator
	{
        private int 				period 						= 20;
        private int 				poles 						= 3;
		private double				sq2							= 0.0;
        private double 				alpha 						= 0.0;
        private double 				beta 						= 0.0;
        private double 				alpha2 						= 0.0;
        private double 				alpha3 						= 0.0;
        private double 				alpha4						= 0.0;
		private double 				coeff1 						= 0.0;
 		private double 				coeff2 						= 0.0;
		private double 				coeff3 						= 0.0;
		private double 				coeff4 						= 0.0;
        private double 				recurrentPart			 	= 0.0;
		private bool				indicatorIsOnPricePanel		= true;
		private string				versionString				= "v 1.2  -  March 7, 2020";
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\nThe Gaussian Filter is a low lag IIR filter which is used to reduce high frequency components of price data. Compared to Butterworth filters"
												+ " Gaussian filters have a lower degree of smoothing but are minimizing the lag. This version of the indicator is based on the Easy Language version"
												+ " published by John F. Ehlers in his paper 'Gaussian and Other Low Lag Filters'.";
				Name						= "amaGaussianFilter";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				AddPlot(new Stroke(Brushes.MediumBlue, 2), PlotStyle.Line, "Gaussian Filter");	
			}
			else if (State == State.Configure)
			{
				BarsRequiredToPlot	= period;
			}
			else if (State == State.Historical)
			{
				const double pi = Math.PI;
				double sq2 = Math.Sqrt(2.0);
				beta = (1 - Math.Cos(2*pi/period))/(Math.Pow (sq2, 2.0/Poles) - 1);
				if(period == 1)
					alpha = 1.0;
				else
					alpha = - beta + Math.Sqrt(beta*(beta + 2));
				alpha2 = alpha*alpha;
				alpha3 = alpha2*alpha;
				alpha4 = alpha3*alpha;
				coeff1 = 1.0 - alpha;
				coeff2 = coeff1*coeff1;
				coeff3 = coeff2*coeff1;
				coeff4 = coeff3*coeff1;
				if(ChartBars != null)
					indicatorIsOnPricePanel = (ChartPanel.PanelIndex == ChartBars.Panel);
				else
					indicatorIsOnPricePanel = false;
			}	
		}

		protected override void OnBarUpdate()
		{
            if (CurrentBar < poles)
            {
                Gaussian[0] = Input[0];
				return;
            }
          	if (IsFirstTickOfBar)
			{
				if ( Poles == 1)
					recurrentPart = coeff1*Gaussian[1];
				else if (Poles == 2)
					recurrentPart = 2*coeff1*Gaussian[1] - coeff2*Gaussian[2];
				else if (Poles == 3)
					recurrentPart = 3*coeff1*Gaussian[1] - 3*coeff2*Gaussian[2] + coeff3*Gaussian[3];
				else if (Poles == 4)	
					recurrentPart = 4*coeff1*Gaussian[1] - 6*coeff2*Gaussian[2] + 4*coeff3*Gaussian[3] - coeff4*Gaussian[4];
			}
			if (Poles == 1)
				Gaussian[0] = alpha*Input[0] + recurrentPart;
			else if (Poles == 2)
				Gaussian[0] = alpha2*Input[0] + recurrentPart;
			else if (Poles == 3)
				Gaussian[0] = alpha3*Input[0] + recurrentPart;
			else if (Poles == 4)	
				Gaussian[0] = alpha4*Input[0] + recurrentPart;
		}
		
		#region Properties
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Gaussian
		{
			get { return Values[0]; }
		}
		
		[Range(1, 4), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Poles", GroupName = "Input Parameters", Order = 0)]
		public int Poles
		{	
            get { return poles; }
            set { poles = value; }
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
		private LizardIndicators.amaGaussianFilter[] cacheamaGaussianFilter;
		public LizardIndicators.amaGaussianFilter amaGaussianFilter(int poles, int period)
		{
			return amaGaussianFilter(Input, poles, period);
		}

		public LizardIndicators.amaGaussianFilter amaGaussianFilter(ISeries<double> input, int poles, int period)
		{
			if (cacheamaGaussianFilter != null)
				for (int idx = 0; idx < cacheamaGaussianFilter.Length; idx++)
					if (cacheamaGaussianFilter[idx] != null && cacheamaGaussianFilter[idx].Poles == poles && cacheamaGaussianFilter[idx].Period == period && cacheamaGaussianFilter[idx].EqualsInput(input))
						return cacheamaGaussianFilter[idx];
			return CacheIndicator<LizardIndicators.amaGaussianFilter>(new LizardIndicators.amaGaussianFilter(){ Poles = poles, Period = period }, input, ref cacheamaGaussianFilter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaGaussianFilter amaGaussianFilter(int poles, int period)
		{
			return indicator.amaGaussianFilter(Input, poles, period);
		}

		public Indicators.LizardIndicators.amaGaussianFilter amaGaussianFilter(ISeries<double> input , int poles, int period)
		{
			return indicator.amaGaussianFilter(input, poles, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaGaussianFilter amaGaussianFilter(int poles, int period)
		{
			return indicator.amaGaussianFilter(Input, poles, period);
		}

		public Indicators.LizardIndicators.amaGaussianFilter amaGaussianFilter(ISeries<double> input , int poles, int period)
		{
			return indicator.amaGaussianFilter(input, poles, period);
		}
	}
}

#endregion
