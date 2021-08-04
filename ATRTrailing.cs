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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class ATRTrailing : Indicator
	{
		private Series<double>	Plotset;
		private Series<double>	Sideset;
		private int				counter		= 0;
		private bool			longval;
		private double			ratchedval	= 0;
		private bool			shortval;
		private double			sigClose	= 0;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description = @"Wilder’s Volatility System, developed by and named after Welles Wilder, is a volatility index made up of the ongoing calculated average, the True Range. The consideration of the True Range means that days with a low trading range (little difference between daily high and low), but still showing a clear price difference to the previous day";
				Name						= "ATRTrailing";
				Calculate					= Calculate.OnBarClose;
				IsOverlay					= true;
				DisplayInDataBox			= true;
				DrawOnPricePanel			= true;
				DrawHorizontalGridLines		= true;
				DrawVerticalGridLines		= true;
				PaintPriceMarkers			= true;
				ScaleJustification			= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive	= true;
				ATRTimes					= 4;
				Period						= 10;
				Ratched						= 0.005;
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Dot, "Lower");
				AddPlot(new Stroke(Brushes.Blue, 2), PlotStyle.Dot, "Upper");
			}
			else if (State == State.Configure)
			{
				Plotset = new Series<double>(this);
				Sideset = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < Period)
				return;

			if (Low[0] < Low[1])
				Sideset[0] = 0;

			if (High[0] > High[1])
				Sideset[0] = 1;

			ratchedval = (1 - (counter * Ratched));

			if (Sideset[1] == 0)
			{
				sigClose = MIN(Low, (Period))[0];
				Plotset[0] = (sigClose + Instrument.MasterInstrument.RoundToTickSize((ratchedval * (ATRTimes * ATR(Period)[0]))));
			}

			if (Sideset[1] == 1)
			{
				sigClose = MAX(High, (Period))[0];
				Plotset[0] = (sigClose - Instrument.MasterInstrument.RoundToTickSize((ratchedval * (ATRTimes * ATR(Period)[0]))));
			}

			if (Sideset[1] == 1 && Low[1] <= Plotset[1])
			{
				Sideset[0] = 0;
				sigClose = High[1];
				counter = 0;
				Plotset[0] = (sigClose + Instrument.MasterInstrument.RoundToTickSize((ratchedval * (ATRTimes * ATR(Period)[0]))));
				Lower[0] = Plotset[0];
			}

			if (Sideset[1] == 1 && Low[1] > Plotset[1])
			{
				Sideset[0] = 1;
				sigClose = MAX(High, (Period))[0];
				Plotset[0] = (sigClose - Instrument.MasterInstrument.RoundToTickSize((ratchedval * (ATRTimes * ATR(Period)[0]))));
				if (Plotset[1] > Plotset[0])
					Plotset[0] = Plotset[1];
				Upper[0] = Plotset[0];
			}

			if (Sideset[1] == 0 && High[1] >= Plotset[1])
			{
				Sideset[0] = 1;
				sigClose = High[1];
				counter = 0;
				Plotset[0] = (sigClose - Instrument.MasterInstrument.RoundToTickSize((ratchedval * (ATRTimes * ATR(Period)[0]))));
				Upper[0] = Plotset[0];
			}

			if (Sideset[1] == 0 && High[1] < Plotset[1])
			{
				Sideset[0] = 0;
				sigClose = MIN(Low, (Period))[0];
				Plotset[0] = (sigClose + Instrument.MasterInstrument.RoundToTickSize((ratchedval * (ATRTimes * ATR(Period)[0]))));
				if (Plotset[1] < Plotset[0])
					Plotset[0] = Plotset[1];
				Lower[0] = Plotset[0];
			}

			counter++;
		}

		#region Properties
		[Range(1, double.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="ATRTimes", Description="Number of ATR (Ex. 3 Time ATR)", Order=1, GroupName="Parameters")]
		public double ATRTimes
		{ get; set; }

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Period", Order=2, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[Range(0, double.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "Ratchet Percent", Description = "Ratchet Percent", Order = 3, GroupName = "Parameters")]
		public double Ratched
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Lower
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Upper
		{
			get { return Values[1]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ATRTrailing[] cacheATRTrailing;
		public ATRTrailing ATRTrailing(double aTRTimes, int period, double ratched)
		{
			return ATRTrailing(Input, aTRTimes, period, ratched);
		}

		public ATRTrailing ATRTrailing(ISeries<double> input, double aTRTimes, int period, double ratched)
		{
			if (cacheATRTrailing != null)
				for (int idx = 0; idx < cacheATRTrailing.Length; idx++)
					if (cacheATRTrailing[idx] != null && cacheATRTrailing[idx].ATRTimes == aTRTimes && cacheATRTrailing[idx].Period == period && cacheATRTrailing[idx].Ratched == ratched && cacheATRTrailing[idx].EqualsInput(input))
						return cacheATRTrailing[idx];
			return CacheIndicator<ATRTrailing>(new ATRTrailing(){ ATRTimes = aTRTimes, Period = period, Ratched = ratched }, input, ref cacheATRTrailing);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ATRTrailing ATRTrailing(double aTRTimes, int period, double ratched)
		{
			return indicator.ATRTrailing(Input, aTRTimes, period, ratched);
		}

		public Indicators.ATRTrailing ATRTrailing(ISeries<double> input , double aTRTimes, int period, double ratched)
		{
			return indicator.ATRTrailing(input, aTRTimes, period, ratched);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ATRTrailing ATRTrailing(double aTRTimes, int period, double ratched)
		{
			return indicator.ATRTrailing(Input, aTRTimes, period, ratched);
		}

		public Indicators.ATRTrailing ATRTrailing(ISeries<double> input , double aTRTimes, int period, double ratched)
		{
			return indicator.ATRTrailing(input, aTRTimes, period, ratched);
		}
	}
}

#endregion
