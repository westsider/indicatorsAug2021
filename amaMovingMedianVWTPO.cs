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
	/// The VWTPO median is the statistical median calculated from the volume-weighted time price opportunities taken from the last N price bars,
	/// where N is a user selectable lookback period.
	/// </summary>
	[Gui.CategoryOrder("Input Parameters", 1000100)]
	[Gui.CategoryOrder("Version", 8000100)]
	public class amaMovingMedianVWTPO : Indicator
	{
		private int					period						= 20;
		private int 				lookback					= 0;
		private int 				fieldSize					= 0;
		private int 				lowIndex					= 0;
		private int 				highIndex					= 0;
		private double 				high						= 0.0;
		private double 				low							= 0.0;
		private double 				preVolume					= 0;
		private double 				slice						= 0.0;
		private double				volume						= 0;
		private double				sum							= 0.0;
		private double				priorSum					= 0.0;
		private double				price						= 0.0;
		private bool				interpolate					= false;
		private bool				calculateFromPriceData		= true;
		private bool				indicatorIsOnPricePanel		= true;
		private Brush				errorBrush					= Brushes.Black;
		private SimpleFont			errorFont					= null;
		private string				errorText					= "The amaMovingMedianVWTPO only works on price data.";
		private List<double> 		fList	 					= new List<double>();
		private string				versionString				= "v 1.4  -  March 7, 2020";
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\nThe VWTPO median is the statistical median calculated from the volume-weighted time price opportunities taken from the last N price bars, "
												+ "where N is a user selectable lookback period";
				Name						= "amaMovingMedianVWTPO";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				AddPlot(new Stroke(Brushes.Maroon, 2), PlotStyle.Line, "VWTPO Median");	
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
			if(CurrentBar >= period && period > 1)
				lookback = period;
			else if (CurrentBar > 0 && period > 1)
				lookback = CurrentBar + 1;
			else
			{
				VWTPO_Median[0] = Median[0];
				return;
			}
			if (IsFirstTickOfBar)
			{
				high = MAX(High,lookback-1)[1];
				low = MIN(Low,lookback-1)[1];
				preVolume = 0;
				fieldSize = 1 + Convert.ToInt32((high-low)/TickSize);
				fList = new List<double>();
				for (int i=0; i < fieldSize; i++)
					fList.Add(0.0);
				for (int j=1; j < lookback; j++)
				{
					lowIndex = Convert.ToInt32 ((Low[j] - low)/TickSize);
					highIndex = Convert.ToInt32((High[j] - low)/TickSize);
					preVolume += Volume[j];
					slice = Volume[j]/(1 + highIndex - lowIndex);
					for (int k=lowIndex; k<=highIndex; k++)
						fList[k] = fList[k] + slice;
				}
			}
			
			if (Low[0] < low)
			{
				int newLowFields = Convert.ToInt32((low-Low[0])/TickSize);
				for (int i = 0; i < newLowFields; i++) 
					fList.Insert(0,0.0);
				low = Low[0];
				fieldSize += newLowFields;
			}
			if(High[0] > high)
			{
				int newHighFields = Convert.ToInt32((High[0]-high)/TickSize);
				for (int i = 0; i < newHighFields; i++) 
					fList.Add(0.0);
				high = High[0];
				fieldSize += newHighFields;
			}			
			volume = preVolume + Volume[0];
			lowIndex = Convert.ToInt32 (((Low[0] - low)/TickSize));
			highIndex = Convert.ToInt32((High[0] - low)/TickSize);
			slice = Volume[0]/(1 + highIndex - lowIndex);
			sum = 0.0;
			priorSum = 0.0;
			for (int i=0; i < fieldSize; i++)
			{	
				price = low + i*TickSize;
				if(i < lowIndex || i > highIndex)
					sum += fList[i]; 
				else
					sum += fList[i] + slice;
				if(2*sum < volume)
				{	
					priorSum = sum;
					continue;
				}	
				else if (interpolate && sum.ApproxCompare(priorSum) == 1)
				{
					VWTPO_Median[0] = price + ((volume - 2.0*priorSum)/(2.0*sum - 2.0*priorSum) - 0.5)*TickSize;
					break;
				}
				else if(2*sum >= volume)
				{
					VWTPO_Median[0] = price;
					break;
				}
			}
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> VWTPO_Median
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
			
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Interpolate values", GroupName = "Input Parameters", Order = 1)]
		public bool Interpolate
		{	
            get { return interpolate; }
            set { interpolate = value; }
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
		private LizardIndicators.amaMovingMedianVWTPO[] cacheamaMovingMedianVWTPO;
		public LizardIndicators.amaMovingMedianVWTPO amaMovingMedianVWTPO(int period, bool interpolate)
		{
			return amaMovingMedianVWTPO(Input, period, interpolate);
		}

		public LizardIndicators.amaMovingMedianVWTPO amaMovingMedianVWTPO(ISeries<double> input, int period, bool interpolate)
		{
			if (cacheamaMovingMedianVWTPO != null)
				for (int idx = 0; idx < cacheamaMovingMedianVWTPO.Length; idx++)
					if (cacheamaMovingMedianVWTPO[idx] != null && cacheamaMovingMedianVWTPO[idx].Period == period && cacheamaMovingMedianVWTPO[idx].Interpolate == interpolate && cacheamaMovingMedianVWTPO[idx].EqualsInput(input))
						return cacheamaMovingMedianVWTPO[idx];
			return CacheIndicator<LizardIndicators.amaMovingMedianVWTPO>(new LizardIndicators.amaMovingMedianVWTPO(){ Period = period, Interpolate = interpolate }, input, ref cacheamaMovingMedianVWTPO);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaMovingMedianVWTPO amaMovingMedianVWTPO(int period, bool interpolate)
		{
			return indicator.amaMovingMedianVWTPO(Input, period, interpolate);
		}

		public Indicators.LizardIndicators.amaMovingMedianVWTPO amaMovingMedianVWTPO(ISeries<double> input , int period, bool interpolate)
		{
			return indicator.amaMovingMedianVWTPO(input, period, interpolate);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaMovingMedianVWTPO amaMovingMedianVWTPO(int period, bool interpolate)
		{
			return indicator.amaMovingMedianVWTPO(Input, period, interpolate);
		}

		public Indicators.LizardIndicators.amaMovingMedianVWTPO amaMovingMedianVWTPO(ISeries<double> input , int period, bool interpolate)
		{
			return indicator.amaMovingMedianVWTPO(input, period, interpolate);
		}
	}
}

#endregion
