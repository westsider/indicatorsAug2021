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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class ATestList : Indicator
	{
		struct BarSlice {
		   public double  priceLevel;
		   public double  bidVolume;
		   public double askVolume;
		   public double delta;
		   public double maxDelta ;
		   public double minDelta;
			
			public BarSlice(double priceLevel, double bidVolume, double askVolume, double delta, double maxDelta, double minDelta)
		    {
		        this.priceLevel = priceLevel;
		        this.bidVolume = bidVolume;
				this.askVolume = askVolume;
		        this.delta = delta;
				this.maxDelta = maxDelta;
				this.minDelta = minDelta;
		    }
		}; 
		private List<BarSlice> FullBar = new List<BarSlice>();
		private List<Double> MaxDelta = new List<Double>();
		private List<Double> MinDelta = new List<Double>();
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "ATestList";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			MaxDelta.Add(High[0]);
			MinDelta.Add(Low[0]);
			
			Print(MaxDelta.Count() + " : " + MinDelta.Count());
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ATestList[] cacheATestList;
		public ATestList ATestList()
		{
			return ATestList(Input);
		}

		public ATestList ATestList(ISeries<double> input)
		{
			if (cacheATestList != null)
				for (int idx = 0; idx < cacheATestList.Length; idx++)
					if (cacheATestList[idx] != null &&  cacheATestList[idx].EqualsInput(input))
						return cacheATestList[idx];
			return CacheIndicator<ATestList>(new ATestList(), input, ref cacheATestList);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ATestList ATestList()
		{
			return indicator.ATestList(Input);
		}

		public Indicators.ATestList ATestList(ISeries<double> input )
		{
			return indicator.ATestList(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ATestList ATestList()
		{
			return indicator.ATestList(Input);
		}

		public Indicators.ATestList ATestList(ISeries<double> input )
		{
			return indicator.ATestList(input);
		}
	}
}

#endregion
