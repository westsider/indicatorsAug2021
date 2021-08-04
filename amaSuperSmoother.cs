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
	/// The SuperSmoother Filter is a low pass IIR filter which is used to eliminate noise from price data. This version of the indicator
	/// is based on the Easy Language version published by John F. Ehlers in his book 'Cybernetic Analysis for Stocks & Futures'
	/// </summary>
	[Gui.CategoryOrder("Input Parameters", 1000100)]
	[Gui.CategoryOrder("Version", 8000100)]
	public class amaSuperSmoother : Indicator
	{
        private int 				period 						= 20;
        private int 				poles 						= 3;
		private double				sq2							= 0.0;
		private double				sq3							= 0.0;
        private double 				a1 							= 0.0;
        private double 				b1 							= 0.0;
        private double 				c1 							= 0.0;
		private double 				coeff1 						= 0.0;
 		private double 				coeff2 						= 0.0;
		private double 				coeff3 						= 0.0;
		private double 				coeff4 						= 0.0;
        private double 				recurrentPart 				= 0.0;
		private bool				indicatorIsOnPricePanel		= true;
		private string				versionString				= "v 1.1  -  March 7, 2020";
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\nThe SuperSmoother Filter is a low pass IIR filter which is used to eliminate noise from price data. This version of the indicator"
												+ " is based on the Easy Language version published by John F. Ehlers in his book 'Cybernetic Analysis for Stocks & Futures'.";
				Name						= "amaSuperSmoother";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				AddPlot(new Stroke(Brushes.MediumBlue, 2), PlotStyle.Line, "Butterworth Filter");	
			}
			else if (State == State.Configure)
			{
				BarsRequiredToPlot	= 2 * period;
			}	
			else if(State == State.Historical)
			{	
				const double pi = Math.PI;
				sq2 = Math.Sqrt(2.0);
				sq3 = Math.Sqrt(3.0);
				if (poles == 2)
				{
					a1 = Math.Exp(-sq2*pi/period);
					b1 = 2*a1*Math.Cos(sq2*pi/period);
					coeff2 = b1;
					coeff3 = -a1*a1;
					coeff1 = 1 - coeff2 - coeff3;
				}	
				else if (poles == 3)
				{
					a1 = Math.Exp(-pi/period);
	            	b1 = 2*a1*Math.Cos(sq3*pi/period);
					c1 = a1*a1;
					coeff2 =  b1 + c1;
					coeff3 = -(c1 + b1*c1);
					coeff4 = c1*c1;
					coeff1 = 1 - coeff2 - coeff3 - coeff4;
				}
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
                SuperSmoother[0] = Input[0];
				return;
            }
          	if (IsFirstTickOfBar)
			{
				if ( poles == 2)
					recurrentPart = coeff2*SuperSmoother[1] + coeff3*SuperSmoother[2];
				else if (poles == 3)
					recurrentPart = coeff2*SuperSmoother[1] + coeff3*SuperSmoother[2] + coeff4*SuperSmoother[3];
			}
			SuperSmoother[0] = recurrentPart + coeff1*Input[0];
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SuperSmoother
		{
			get { return Values[0]; }
		}
		
		[Range(2, 3), NinjaScriptProperty]
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
		private LizardIndicators.amaSuperSmoother[] cacheamaSuperSmoother;
		public LizardIndicators.amaSuperSmoother amaSuperSmoother(int poles, int period)
		{
			return amaSuperSmoother(Input, poles, period);
		}

		public LizardIndicators.amaSuperSmoother amaSuperSmoother(ISeries<double> input, int poles, int period)
		{
			if (cacheamaSuperSmoother != null)
				for (int idx = 0; idx < cacheamaSuperSmoother.Length; idx++)
					if (cacheamaSuperSmoother[idx] != null && cacheamaSuperSmoother[idx].Poles == poles && cacheamaSuperSmoother[idx].Period == period && cacheamaSuperSmoother[idx].EqualsInput(input))
						return cacheamaSuperSmoother[idx];
			return CacheIndicator<LizardIndicators.amaSuperSmoother>(new LizardIndicators.amaSuperSmoother(){ Poles = poles, Period = period }, input, ref cacheamaSuperSmoother);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaSuperSmoother amaSuperSmoother(int poles, int period)
		{
			return indicator.amaSuperSmoother(Input, poles, period);
		}

		public Indicators.LizardIndicators.amaSuperSmoother amaSuperSmoother(ISeries<double> input , int poles, int period)
		{
			return indicator.amaSuperSmoother(input, poles, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaSuperSmoother amaSuperSmoother(int poles, int period)
		{
			return indicator.amaSuperSmoother(Input, poles, period);
		}

		public Indicators.LizardIndicators.amaSuperSmoother amaSuperSmoother(ISeries<double> input , int poles, int period)
		{
			return indicator.amaSuperSmoother(input, poles, period);
		}
	}
}

#endregion
