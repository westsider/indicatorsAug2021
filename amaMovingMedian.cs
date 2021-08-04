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
	/// The moving median is the statistical median calculated from the last N bars of the input series,
	/// where N is a user selectable lookback period.
	/// </summary>
	[Gui.CategoryOrder("Input Parameters", 1000100)]
	[Gui.CategoryOrder("Version", 8000100)]
	public class amaMovingMedian : Indicator
	{
		private int					period						= 14;
		private int 				lookback					= 0;
		private int 				mIndex						= 0;
		private bool 				even						= true;
		private bool				indicatorIsOnPricePanel		= true;
		private List<double> 		mList 						= new List<double>();
		private string				versionString				= "v 2.2  -  March 7, 2020";
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\nThe moving median is the statistical median calculated from the last N data points of the input series, where N is a user selectable lookback period.";
				Name						= "amaMovingMedian";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				AddPlot(new Stroke(Brushes.Maroon, 2), PlotStyle.Line, "Moving Median");	
			}
			else if (State == State.Configure)
			{
				BarsRequiredToPlot = period;
			}
			else if (State == State.Historical)
			{
				if (period % 2 == 0)
					even = true;
				else
					even = false;
				mIndex = period/2;
				if(ChartBars != null)
					indicatorIsOnPricePanel = (ChartPanel.PanelIndex == ChartBars.Panel);
				else
					indicatorIsOnPricePanel = false;
			}	
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar < period)
			{
				if(IsFirstTickOfBar)
				{	
					mList.Add(0.0);
					lookback = CurrentBar + 1;
				}
				for (int i = 0; i < lookback; i++)
					mList[i] = Input[i];
				mList.Sort();
				if (lookback % 2 == 0)
					Median[0] = 0.5 * (mList[lookback/2] + mList[lookback/2 - 1]);
				else
					Median[0] = mList[(lookback - 1)/2];
			}	
			else
			{
				for (int i = 0; i < period; i++)
					mList[i] = Input[i];
				mList.Sort();
				if (even)
					Median[0] = 0.5 * (mList[mIndex] + mList[mIndex-1]);
				else
					Median[0] = mList[mIndex];
			}
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Median
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
		private LizardIndicators.amaMovingMedian[] cacheamaMovingMedian;
		public LizardIndicators.amaMovingMedian amaMovingMedian(int period)
		{
			return amaMovingMedian(Input, period);
		}

		public LizardIndicators.amaMovingMedian amaMovingMedian(ISeries<double> input, int period)
		{
			if (cacheamaMovingMedian != null)
				for (int idx = 0; idx < cacheamaMovingMedian.Length; idx++)
					if (cacheamaMovingMedian[idx] != null && cacheamaMovingMedian[idx].Period == period && cacheamaMovingMedian[idx].EqualsInput(input))
						return cacheamaMovingMedian[idx];
			return CacheIndicator<LizardIndicators.amaMovingMedian>(new LizardIndicators.amaMovingMedian(){ Period = period }, input, ref cacheamaMovingMedian);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaMovingMedian amaMovingMedian(int period)
		{
			return indicator.amaMovingMedian(Input, period);
		}

		public Indicators.LizardIndicators.amaMovingMedian amaMovingMedian(ISeries<double> input , int period)
		{
			return indicator.amaMovingMedian(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaMovingMedian amaMovingMedian(int period)
		{
			return indicator.amaMovingMedian(Input, period);
		}

		public Indicators.LizardIndicators.amaMovingMedian amaMovingMedian(ISeries<double> input , int period)
		{
			return indicator.amaMovingMedian(input, period);
		}
	}
}

#endregion
