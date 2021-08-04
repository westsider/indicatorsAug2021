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
	/// The VWTPO mean is the statistical mean calculated from the volume-weighted time price opportunities taken from the last N price bars,
	/// where N is a user selectable lookback period.
	/// </summary>
	[Gui.CategoryOrder("Input Parameters", 1000100)]
	[Gui.CategoryOrder("Version", 8000100)]
	public class amaMovingMeanVWTPO : Indicator
	{
		private int					period						= 20;
		private int 				lookback					= 0;
		private double 				preVolume					= 0;
		private double 				preSum						= 0.0;
		private double 				volume						= 0.0;
		private double 				sum							= 0.0;
		private bool				calculateFromPriceData		= true;
		private bool				indicatorIsOnPricePanel		= true;
		private Brush				errorBrush					= Brushes.Black;
		private SimpleFont			errorFont					= null;
		private string				errorText					= "The amaMovingMeanVWTPO only works on price data.";
		private string				versionString				= "v 1.2  -  March 7, 2020";
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\nThe VWTPO mean is the statistical mean calculated from the volume-weighted time price opportunities taken from the last N price bars, "
												+ "where N is a user selectable lookback period";
				Name						= "amaMovingMeanVWTPO";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				AddPlot(new Stroke(Brushes.Navy, 2), PlotStyle.Line, "VWTPO Mean");	
			}
			else if (State == State.Configure)
			{	
				BarsRequiredToPlot = period;
			}	
			else if (State == State.DataLoaded)
			{
				if(Input is PriceSeries)
					calculateFromPriceData = true;
				else
					calculateFromPriceData = false;
			}
			else if (State == State.Historical)
			{
				if(ChartBars != null)
				{	
					errorBrush = ChartControl.Properties.AxisPen.Brush;
					errorBrush.Freeze();
					errorFont = new SimpleFont("Arial", 24);
					indicatorIsOnPricePanel = (ChartPanel.PanelIndex == ChartBars.Panel);
				}	
				else
					indicatorIsOnPricePanel = false;
			}	
		}

		protected override void OnBarUpdate()
		{
			if(!calculateFromPriceData)
			{
				DrawOnPricePanel = false;
				Draw.TextFixed(this, "error text", errorText, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);  
				return;
			}	
			if(CurrentBar < period)
				lookback = CurrentBar + 1;
			else 
				lookback = period;
			
			if (IsFirstTickOfBar)
			{
				preVolume = 0.0;
				preSum = 0.0;
				for (int j=1; j < lookback; j++)
				{
					preVolume += Volume[j]; 
					preSum += Volume[j]*Median[j];
				}
			}	
			volume = preVolume + Volume[0];
			sum = preSum + Volume[0] * Median[0];
			if(volume.ApproxCompare(0) == 1)
				VWTPO_Mean[0] = sum/volume;
			else if (CurrentBar > 0)
				VWTPO_Mean[0] = VWTPO_Mean[1];
			else
				VWTPO_Mean[0] = Median[0];
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> VWTPO_Mean
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
		private LizardIndicators.amaMovingMeanVWTPO[] cacheamaMovingMeanVWTPO;
		public LizardIndicators.amaMovingMeanVWTPO amaMovingMeanVWTPO(int period)
		{
			return amaMovingMeanVWTPO(Input, period);
		}

		public LizardIndicators.amaMovingMeanVWTPO amaMovingMeanVWTPO(ISeries<double> input, int period)
		{
			if (cacheamaMovingMeanVWTPO != null)
				for (int idx = 0; idx < cacheamaMovingMeanVWTPO.Length; idx++)
					if (cacheamaMovingMeanVWTPO[idx] != null && cacheamaMovingMeanVWTPO[idx].Period == period && cacheamaMovingMeanVWTPO[idx].EqualsInput(input))
						return cacheamaMovingMeanVWTPO[idx];
			return CacheIndicator<LizardIndicators.amaMovingMeanVWTPO>(new LizardIndicators.amaMovingMeanVWTPO(){ Period = period }, input, ref cacheamaMovingMeanVWTPO);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaMovingMeanVWTPO amaMovingMeanVWTPO(int period)
		{
			return indicator.amaMovingMeanVWTPO(Input, period);
		}

		public Indicators.LizardIndicators.amaMovingMeanVWTPO amaMovingMeanVWTPO(ISeries<double> input , int period)
		{
			return indicator.amaMovingMeanVWTPO(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaMovingMeanVWTPO amaMovingMeanVWTPO(int period)
		{
			return indicator.amaMovingMeanVWTPO(Input, period);
		}

		public Indicators.LizardIndicators.amaMovingMeanVWTPO amaMovingMeanVWTPO(ISeries<double> input , int period)
		{
			return indicator.amaMovingMeanVWTPO(input, period);
		}
	}
}

#endregion
