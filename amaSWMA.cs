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
	/// The Sine Weighted Moving Average (SWMA) is a weighted average based on the SMA. The coefficient for each bar is calculated using the sine function.";
	/// </summary>
	[Gui.CategoryOrder("Input Parameters", 1000100)]
	[Gui.CategoryOrder("Version", 8000100)]
	public class amaSWMA : Indicator
	{
		private int					period						= 15;
		private int					lookback					= 0;
		private double				coeffNum					= 0.0;
		private double				coeffDen					= 0.0;
		private double				coeffNum0					= 0.0;
		private double				preNum						= 0.0;
		private double				preDen						= 0.0;
		private double				num							= 0.0;
		private double				den							= 0.0;
		private bool				indicatorIsOnPricePanel		= true;
		private List<double>		coeff						= new List<double>();
		private string				versionString				= "v 1.1  -  March 7, 2020";
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\n The Sine Weighted Moving Average (SWMA) is a weighted average based on the SMA. The coefficient for each bar is calculated using the sine function.";
				Name						= "amaSWMA";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Line, "SWMA");	
			}
			else if (State == State.Configure)
			{
				BarsRequiredToPlot = 0; //period;
			}	
			else if (State == State.Historical)
			{
				coeffNum = Math.PI / (period + 1);
				for(int i = 0; i < period; i++)
					coeff.Add(Math.Sin((i + 1) * coeffNum)); 
				coeffDen = 0.0;
				for(int i= 0; i < period; i++)
					coeffDen = coeffDen + coeff[i]; 
					
				if(ChartBars != null)
					indicatorIsOnPricePanel = (ChartPanel.PanelIndex == ChartBars.Panel);
				else
					indicatorIsOnPricePanel = false;
			}	
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar < period - 1)
			{
				if(IsFirstTickOfBar)
				{
					coeffNum0 = Math.PI/(CurrentBar + 2);
					preNum = 0.0;
					for (int i = 1; i < CurrentBar + 1; i++)
						preNum = preNum + Input[i] * Math.Sin((i + 1) * coeffNum0);
					preDen = 0.0;
					for(int i = 0; i <= CurrentBar; i++)
						preDen = preDen + Math.Sin((i + 1) * coeffNum0); 
				}
				num = preNum + Input[0] * Math.Sin(coeffNum0);   
				den = preDen;
				SWMA[0] = num/den;
				return;
			}
		
			if(IsFirstTickOfBar)
			{	
				preNum = 0.0;
				for (int i = 1; i < period; i++)
					preNum = preNum + Input[i] *coeff[i];
								
			}
			num = preNum + Input[0] * coeff[0];
			den = coeffDen;
			SWMA[0] = num/den;
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SWMA
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
		private LizardIndicators.amaSWMA[] cacheamaSWMA;
		public LizardIndicators.amaSWMA amaSWMA(int period)
		{
			return amaSWMA(Input, period);
		}

		public LizardIndicators.amaSWMA amaSWMA(ISeries<double> input, int period)
		{
			if (cacheamaSWMA != null)
				for (int idx = 0; idx < cacheamaSWMA.Length; idx++)
					if (cacheamaSWMA[idx] != null && cacheamaSWMA[idx].Period == period && cacheamaSWMA[idx].EqualsInput(input))
						return cacheamaSWMA[idx];
			return CacheIndicator<LizardIndicators.amaSWMA>(new LizardIndicators.amaSWMA(){ Period = period }, input, ref cacheamaSWMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaSWMA amaSWMA(int period)
		{
			return indicator.amaSWMA(Input, period);
		}

		public Indicators.LizardIndicators.amaSWMA amaSWMA(ISeries<double> input , int period)
		{
			return indicator.amaSWMA(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaSWMA amaSWMA(int period)
		{
			return indicator.amaSWMA(Input, period);
		}

		public Indicators.LizardIndicators.amaSWMA amaSWMA(ISeries<double> input , int period)
		{
			return indicator.amaSWMA(input, period);
		}
	}
}

#endregion
