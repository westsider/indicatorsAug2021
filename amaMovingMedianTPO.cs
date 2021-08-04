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
	/// The TPO median is the statistical median calculated from the time price opportunities taken from the last N price bars,
	/// where N is a user selectable lookback period.
	/// </summary>
	[Gui.CategoryOrder("Input Parameters", 1000100)]
	[Gui.CategoryOrder("Version", 8000100)]
	public class amaMovingMedianTPO : Indicator
	{
		private int					period						= 20;
		private int 				lookback					= 0;
		private int 				preSampleSize				= 0;
		private int 				fieldSize					= 0;
		private int 				lowIndex					= 0;
		private int 				highIndex					= 0;
		private int 				sampleSize					= 0;
		private int					count						= 0;
		private int					priorCount					= 0;
		private double 				high						= 0.0;
		private double 				low							= 0.0;
		private double 				currentHigh					= 0.0;
		private double 				currentLow					= 0.0;
		private double				price						= 0.0;
		private bool				interpolate					= false;
		private bool 				recalculate					= false;
		private bool				calculateFromPriceData		= true;
		private bool				indicatorIsOnPricePanel		= true;
		private Brush				errorBrush					= Brushes.Black;
		private SimpleFont			errorFont					= null;
		private string				errorText					= "The amaMovingMedianTPO only works on price data.";
		private List<int> 			fList	 					= new List<int>();
		private string				versionString				= "v 1.4  -  March 7, 2020";
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\nThe TPO median is the statistical median calculated from the time price opportunities taken from the last N price bars, "
												+ "where N is a user selectable lookback period";
				Name						= "amaMovingMedianTPO";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				AddPlot(new Stroke(Brushes.Maroon, 2), PlotStyle.Line, "TPO Median");	
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
				TPO_Median[0] = Median[0];
				return;
			}

			if (IsFirstTickOfBar)
			{
				high = MAX(High,lookback-1)[1];
				low = MIN(Low, lookback-1)[1];
				preSampleSize = 0;
				fieldSize = 1 + Convert.ToInt32((high-low)/TickSize);
				fList = new List<int>();
				for (int i=0; i < fieldSize; i++)
					fList.Add(0);
				for (int j=1; j < lookback; j++)
				{
					lowIndex = Convert.ToInt32 (((Low[j] - low)/TickSize));
					highIndex = Convert.ToInt32((High[j] - low)/TickSize);
					preSampleSize += (highIndex - lowIndex + 1);
					for (int k = lowIndex; k <= highIndex; k++)
						fList[k] = fList[k] + 1;
				}
				recalculate = true;
			}	
			else if (High[0] > currentHigh || Low[0] < currentLow)
				recalculate = true;
			else
				recalculate = false;
			if (Low[0] < low)
			{
				int newLowFields = Convert.ToInt32((low-Low[0])/TickSize);
				for (int i = 0; i < newLowFields; i++) 
					fList.Insert(0, 0);
				low = Low[0];
				fieldSize += newLowFields;
			}
			if(High[0] > high)
			{
				int newHighFields = Convert.ToInt32((High[0]-high)/TickSize);
				for (int i = 0; i < newHighFields; i++) 
					fList.Add(0);
				high = High[0];
				fieldSize += newHighFields;
			}			
			if(recalculate)
			{
				lowIndex = Convert.ToInt32 (((Low[0] - low)/TickSize));
				highIndex = Convert.ToInt32((High[0] - low)/TickSize);
				sampleSize = preSampleSize + 1 + highIndex - lowIndex;
				count = 0;
				priorCount = 0;
				for (int i=0; i < fieldSize; i++)
				{	
					price = low + i*TickSize;
					if(i < lowIndex || i > highIndex)
						count += fList[i]; 
					else
						count += fList[i] + 1;
					if(2*count < sampleSize)
					{	
						priorCount = count;
						continue;
					}	
					else if (interpolate)
					{
						TPO_Median[0] = price + ((sampleSize - 2.0*priorCount)/(2.0*count - 2.0*priorCount) - 0.5)*TickSize;
						break;
					}
					else if (2*count == sampleSize)
					{	
						TPO_Median[0] = price + 0.5*TickSize;
						break;
					}	
					else
					{	
						TPO_Median[0] = price;
						break;
					}	
				}
				currentLow = Low[0];
				currentHigh = High[0];
			}			
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TPO_Median
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
		private LizardIndicators.amaMovingMedianTPO[] cacheamaMovingMedianTPO;
		public LizardIndicators.amaMovingMedianTPO amaMovingMedianTPO(int period, bool interpolate)
		{
			return amaMovingMedianTPO(Input, period, interpolate);
		}

		public LizardIndicators.amaMovingMedianTPO amaMovingMedianTPO(ISeries<double> input, int period, bool interpolate)
		{
			if (cacheamaMovingMedianTPO != null)
				for (int idx = 0; idx < cacheamaMovingMedianTPO.Length; idx++)
					if (cacheamaMovingMedianTPO[idx] != null && cacheamaMovingMedianTPO[idx].Period == period && cacheamaMovingMedianTPO[idx].Interpolate == interpolate && cacheamaMovingMedianTPO[idx].EqualsInput(input))
						return cacheamaMovingMedianTPO[idx];
			return CacheIndicator<LizardIndicators.amaMovingMedianTPO>(new LizardIndicators.amaMovingMedianTPO(){ Period = period, Interpolate = interpolate }, input, ref cacheamaMovingMedianTPO);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaMovingMedianTPO amaMovingMedianTPO(int period, bool interpolate)
		{
			return indicator.amaMovingMedianTPO(Input, period, interpolate);
		}

		public Indicators.LizardIndicators.amaMovingMedianTPO amaMovingMedianTPO(ISeries<double> input , int period, bool interpolate)
		{
			return indicator.amaMovingMedianTPO(input, period, interpolate);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaMovingMedianTPO amaMovingMedianTPO(int period, bool interpolate)
		{
			return indicator.amaMovingMedianTPO(Input, period, interpolate);
		}

		public Indicators.LizardIndicators.amaMovingMedianTPO amaMovingMedianTPO(ISeries<double> input , int period, bool interpolate)
		{
			return indicator.amaMovingMedianTPO(input, period, interpolate);
		}
	}
}

#endregion
