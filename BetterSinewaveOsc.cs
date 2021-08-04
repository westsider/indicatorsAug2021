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
	public class BetterSinewaveOsc : Indicator
	{
		
		private bool showZero = true;
		private string password = "";
		private bool bInit;
		private bool paa;
		private string simpleP = "2339hvsw";
		
		private Series<double> BHPriceInSeries;
		private Series<double> BHSmoothSeries;
		private Series<double> BHDetrenderSeries;
		private Series<double> BHI1Series;
		private Series<double> BHQ1Series;
		private Series<double> BHI2Series;
		private Series<double> BHQ2Series;
		private Series<double> BHReSeries;
		private Series<double> BHImSeries;
		private Series<double> BHPeriodSeries;
		private Series<double> BHSmoothPeriodSeries;
		private Series<double> BHDCPhaseSeries;
		private Series<double> Value1Series;
		private Series<double> Value2Series;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "Better Sinewave Oscilator";
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
				AddPlot(Brushes.Cyan, "Sine");
				AddPlot(Brushes.Red, "LeadSine");
				if (showZero) { AddPlot(Brushes.White, "Zero"); }

				BHPriceInSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				BHSmoothSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				BHDetrenderSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				BHI1Series = new Series<double>(this, MaximumBarsLookBack.Infinite);
				BHQ1Series = new Series<double>(this, MaximumBarsLookBack.Infinite);
				BHI2Series = new Series<double>(this, MaximumBarsLookBack.Infinite);
				BHQ2Series = new Series<double>(this, MaximumBarsLookBack.Infinite);
				BHReSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				BHImSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				BHPeriodSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				BHSmoothPeriodSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				BHDCPhaseSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				Value1Series = new Series<double>(this, MaximumBarsLookBack.Infinite);
				Value2Series = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
			}
			else if (State == State.Configure)
			{
			}
		}

		public override string ToString()
		{
			return Name;
		}
		
		protected override void OnBarUpdate()
		{
			if (CurrentBar < 50)
			{
				return;
			}
			
			BHPriceInSeries[0] = ((High[0] + Low[0]) / 2.0 * 100.0);
			Value1Series[0] = (WMA(BHPriceInSeries, 3)[0]);
			Value2Series[0] = (WMA(Value1Series, 2)[0]);
			double num = (Value1Series[0] + Value2Series[0]) / 2.0;
			BHSmoothSeries[0] = (num);
			double num2 = (0.0962 * num + 0.5769 * BHSmoothSeries[2] - 0.5769 * BHSmoothSeries[4] - 0.0962 * BHSmoothSeries[6]) * (0.075 * BHPeriodSeries[1] + 0.54);
			BHDetrenderSeries[0] = (num2);
			double num3 = (0.0962 * num2 + 0.5769 * BHDetrenderSeries[2] - 0.5769 * BHDetrenderSeries[4] - 0.0962 * BHDetrenderSeries[6]) * (0.075 * BHPeriodSeries[1] + 0.54);
			BHQ1Series[0] = (num3);
			double num4 = BHDetrenderSeries[3];
			BHI1Series[0] = (num4);
			double num5 = (0.0962 * num4 + 0.5769 * BHI1Series[2] - 0.5769 * BHI1Series[4] - 0.0962 * BHI1Series[6]) * (0.075 * BHPeriodSeries[1] + 0.54);
			double num6 = (0.0962 * num3 + 0.5769 * BHQ1Series[2] - 0.5769 * BHQ1Series[4] - 0.0962 * BHQ1Series[6]) * (0.075 * BHPeriodSeries[1] + 0.54);
			double num7 = num4 - num6;
			double num8 = num3 + num5;
			num7 = 0.2 * num7 + 0.8 * BHI2Series[1];
			num8 = 0.2 * num8 + 0.8 * BHQ2Series[1];
			BHI2Series[0] = (num7);
			BHQ2Series[0] = (num8);
			double num9 = num7 * BHI2Series[1] + num8 * BHQ2Series[1];
			double num10 = num7 * BHQ2Series[1] - num8 * BHI2Series[1];
			num9 = 0.2 * num9 + 0.8 * BHReSeries[1];
			num10 = 0.2 * num10 + 0.8 * BHImSeries[1];
			BHReSeries[0] = (num9);
			BHImSeries[0] = (num10);
			double num11 = BHPeriodSeries[1];
			if (num10 != 0.0 && num9 != 0.0)
			{
				num11 = 360.0 / (Math.Atan(num10 / num9) / 3.1415926535897931 * 180.0);
			}
			if (num11 > 1.5 * BHPeriodSeries[1])
			{
				num11 = 1.5 * BHPeriodSeries[1];
			}
			else
			{
				if (num11 < 0.67 * BHPeriodSeries[1])
				{
					num11 = 0.67 * BHPeriodSeries[1];
				}
			}
			if (num11 < 6.0)
			{
				num11 = 6.0;
			}
			else
			{
				if (num11 > 50.0)
				{
					num11 = 50.0;
				}
			}
			num11 = 0.2 * num11 + 0.8 * BHPeriodSeries[1];
			BHPeriodSeries[0] = (num11);
			double num12 = 0.33 * num11 + 0.67 * BHSmoothPeriodSeries[1];
			BHSmoothPeriodSeries[0] = (num12);
			double num13 = (double)((int)Math.Floor(num12 + 0.5));
			double num14 = 0.0;
			double num15 = 0.0;
			int num16 = 0;
			while ((double)num16 < num13)
			{
				num14 += Math.Sin((double)(360 * num16) / num13 * 3.1415926535897931 / 180.0) * BHSmoothSeries[num16];
				num15 += Math.Cos((double)(360 * num16) / num13 * 3.1415926535897931 / 180.0) * BHSmoothSeries[num16];
				num16++;
			}
			double num17 = BHDCPhaseSeries[1];
			if (Math.Abs(num15) > 0.0)
			{
				num17 = Math.Atan(num14 / num15) / 3.1415926535897931 * 180.0;
			}
			if (Math.Abs(num15) <= 0.001)
			{
				num17 += (double)(90 * Math.Sign(num14));
			}
			num17 += 90.0;
			num17 += 360.0 / num12;
			if (num15 < 0.0)
			{
				num17 += 180.0;
			}
			if (num17 > 315.0)
			{
				num17 -= 360.0;
			}
			BHDCPhaseSeries[0] = (num17);
			double num18 = Math.Sin(num17 * 3.1415926535897931 / 180.0) * 100.0;
			double num19 = Math.Sin((num17 + 45.0) * 3.1415926535897931 / 180.0) * 100.0;
			Sine[0] = (num18);
			LeadSine[0] = (num19);
			if (showZero)
			{
				Zero[0] = (0.0);
			}
		}

		#region Properties

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Sine
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LeadSine
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Zero
		{
			get { return Values[2]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BetterSinewaveOsc[] cacheBetterSinewaveOsc;
		public BetterSinewaveOsc BetterSinewaveOsc()
		{
			return BetterSinewaveOsc(Input);
		}

		public BetterSinewaveOsc BetterSinewaveOsc(ISeries<double> input)
		{
			if (cacheBetterSinewaveOsc != null)
				for (int idx = 0; idx < cacheBetterSinewaveOsc.Length; idx++)
					if (cacheBetterSinewaveOsc[idx] != null &&  cacheBetterSinewaveOsc[idx].EqualsInput(input))
						return cacheBetterSinewaveOsc[idx];
			return CacheIndicator<BetterSinewaveOsc>(new BetterSinewaveOsc(), input, ref cacheBetterSinewaveOsc);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BetterSinewaveOsc BetterSinewaveOsc()
		{
			return indicator.BetterSinewaveOsc(Input);
		}

		public Indicators.BetterSinewaveOsc BetterSinewaveOsc(ISeries<double> input )
		{
			return indicator.BetterSinewaveOsc(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BetterSinewaveOsc BetterSinewaveOsc()
		{
			return indicator.BetterSinewaveOsc(Input);
		}

		public Indicators.BetterSinewaveOsc BetterSinewaveOsc(ISeries<double> input )
		{
			return indicator.BetterSinewaveOsc(input);
		}
	}
}

#endregion
