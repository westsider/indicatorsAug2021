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
	/// Heikin Ashi zerolagging TEMA as published by Sylvain Vervoort in 'The Quest For Reliable Crossovers', Technical Analysis of Stocks & Commodities, May 2008.
	/// </summary>
	[Gui.CategoryOrder("Input Parameters", 1000100)]
	[Gui.CategoryOrder("Version", 8000100)]
	public class amaZerolagHATEMA : Indicator
	{
        private int 				period 						= 14;
		private bool				calculateFromPriceData		= true;
		private bool				indicatorIsOnPricePanel		= true;
		private Brush				errorBrush					= new SolidColorBrush(System.Windows.Media.Colors.Black);
		private SimpleFont			errorFont					= null;
		private string				errorText					= "The amaZerolagHATEMA only works on price data.";
		private string				versionString				= "v 1.2  -  March 7, 2020";
		private	Series<double>		haClose;
		private	Series<double>		haOpen;
		private	TEMA				tema1;
		private	TEMA				tema2;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\nHeikin Ashi zerolagging TEMA as published by Sylvain Vervoort in 'The Quest For Reliable Crossovers', Technical Analysis of Stocks & Commodities, May 2008.";
				Name						= "amaZerolagHATEMA";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				AddPlot(new Stroke(Brushes.MediumBlue, 2), PlotStyle.Line, "Zerolag HATEMA");	
			}
			else if (State == State.Configure)
			{
				BarsRequiredToPlot = 3 * period;
			}	
			else if (State == State.DataLoaded)
			{
				haClose = new Series<double>(this);
				haOpen = new Series<double>(this);
				tema1 = TEMA(haClose, period);
				tema2 = TEMA(tema1, period);
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
			
			if(IsFirstTickOfBar)
			{	
				if (CurrentBar == 0)
					haOpen[0] = Open[0];
				else
					haOpen[0] = ((Open[1] + High[1] + Low[1] + Close[1]) / 4 + haOpen[1]) / 2;
			}	
			haClose[0] = ((Open[0] + High[0] + Low[0] + Close[0]) / 4 + haOpen[0] + Math.Max(High[0], haOpen[0]) + Math.Min(Low[0], haOpen[0])) / 4;
            ZerolagHATEMA[0] = 2*tema1[0] - tema2[0];
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ZerolagHATEMA
		{
			get { return Values[0]; }
		}
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", Description = "Sets the lookback period for the triple exponential moving averages", GroupName = "Input Parameters", Order = 0)]
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
		private LizardIndicators.amaZerolagHATEMA[] cacheamaZerolagHATEMA;
		public LizardIndicators.amaZerolagHATEMA amaZerolagHATEMA(int period)
		{
			return amaZerolagHATEMA(Input, period);
		}

		public LizardIndicators.amaZerolagHATEMA amaZerolagHATEMA(ISeries<double> input, int period)
		{
			if (cacheamaZerolagHATEMA != null)
				for (int idx = 0; idx < cacheamaZerolagHATEMA.Length; idx++)
					if (cacheamaZerolagHATEMA[idx] != null && cacheamaZerolagHATEMA[idx].Period == period && cacheamaZerolagHATEMA[idx].EqualsInput(input))
						return cacheamaZerolagHATEMA[idx];
			return CacheIndicator<LizardIndicators.amaZerolagHATEMA>(new LizardIndicators.amaZerolagHATEMA(){ Period = period }, input, ref cacheamaZerolagHATEMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaZerolagHATEMA amaZerolagHATEMA(int period)
		{
			return indicator.amaZerolagHATEMA(Input, period);
		}

		public Indicators.LizardIndicators.amaZerolagHATEMA amaZerolagHATEMA(ISeries<double> input , int period)
		{
			return indicator.amaZerolagHATEMA(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaZerolagHATEMA amaZerolagHATEMA(int period)
		{
			return indicator.amaZerolagHATEMA(Input, period);
		}

		public Indicators.LizardIndicators.amaZerolagHATEMA amaZerolagHATEMA(ISeries<double> input , int period)
		{
			return indicator.amaZerolagHATEMA(input, period);
		}
	}
}

#endregion
