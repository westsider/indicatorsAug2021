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
	public class BetterSinewavePlot : Indicator
	{
		
			private string password = "";
			private bool bInit;
			private bool paa;
			private Double BreakoutBar;
			private bool OS_Flag;
			private Double BreakoutBar_H;
			private bool OS_Flag_H;
		
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
			private Series<double> SlowSeries;
			private Series<double> FastSeries;
			private Series<double> LowSetupSeries;
			private Series<double> HighSetupSeries;
			private Series<double> SRSeries;
			private Series<double> CountSeries;
			private Series<double> PlotLowSeries;
			private Series<double> PlotHighSeries;
			private Series<double> BreakLowSeries;
			private Series<double> BreakHighSeries;
			private Series<bool> PullBackSeries;
			private Series<bool> EndOfTrendSeries;
			
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "Better Sinewave Plot";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive	= true;
				BreakOut					= true;
				PullBack					= true;
				EndOfTrend					= true;
				AlertAtNewSupportResistance		= true;
				AlertAtBreakOut					= true;
				AlertAtPullBack					= true;
				AlertAtEndOfTrend					= true;
				AlertSoundNewSupportResistance		= @"Alert3.wav";
				AlertSoundBreakOut					= @"Alert1.wav";
				AlertSoundPullBack					= @"Alert4.wav";
				AlertSoundEndOfTrend					= @"Alert2.wav";
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Dot, "Support");
				AddPlot(new Stroke(Brushes.Silver, 2), PlotStyle.Dot, "Resistance");
				AddPlot(new Stroke(Brushes.Silver, 6), PlotStyle.Dot, "BreakLo");
				AddPlot(new Stroke(Brushes.Red, 6), PlotStyle.Dot, "BreakHi");
				
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
				SlowSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				FastSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				LowSetupSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				HighSetupSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				SRSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				CountSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				PlotLowSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				PlotHighSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				BreakLowSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				BreakHighSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				PullBackSeries = new Series<bool>(this, MaximumBarsLookBack.Infinite);
				EndOfTrendSeries = new Series<bool>(this, MaximumBarsLookBack.Infinite);
				
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 50) { return; }
			
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
			SlowSeries[0] = (num18);
			double num19 = Math.Sin((num17 + 45.0) * 3.1415926535897931 / 180.0) * 100.0;
			FastSeries[0] = (num19);
			double tickSize = TickSize;
			bool flag = false;
			bool flag2 = false;
			double num20 = Close[0];
			int num21 = 1;
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			bool flag6 = false;
			if (CurrentBar > 0)
			{
				flag = (HighSetupSeries[1] != 0.0);
				flag2 = (LowSetupSeries[1] != 0.0);
				num20 = SRSeries[1];
				num21 = (int)Math.Round(CountSeries[1]);
				flag3 = (PlotLowSeries[1] != 0.0);
				flag4 = (PlotHighSeries[1] != 0.0);
				flag5 = (BreakLowSeries[1] != 0.0);
				flag6 = (BreakHighSeries[1] != 0.0);
			}
			if (CurrentBar > 0)
			{
				if (num19 > num18 && FastSeries[1] <= SlowSeries[1] && !flag)
				{
					flag2 = true;
				}
				if (num19 < num18 && FastSeries[1] >= SlowSeries[1] && !flag2)
				{
					flag = true;
				}
				if (num19 > num18 && flag2 && High[0] > High[1])
				{
					double num22 = num20;
					num20 = Math.Min(Low[0], Low[1]) - tickSize;
					flag2 = false;
					flag3 = true;
					flag4 = false;
					flag5 = true;
					if ((num21 == 2 || num21 == -4) && num20 >= num22)
					{
						num21 = 3;
					}
					if (num21 == -3 && num20 <= num22)
					{
						num21 = -5;
					}
					if (num21 != 3 && num21 != -5)
					{
						num21 = -1;
					}
					if (AlertAtNewSupportResistance)
					{
						TC_Alert("New Support", AlertSoundNewSupportResistance);
					}
				}
				if (num19 < num18 && flag && Low[0] < Low[1])
				{
					double num23 = num20;
					num20 = Math.Max(High[0], High[1]) + tickSize;
					flag = false;
					flag4 = true;
					flag3 = false;
					flag6 = true;
					if ((num21 == -2 || num21 == 4) && num20 <= num23)
					{
						num21 = -3;
					}
					if (num21 == 3 && num20 >= num23)
					{
						num21 = 5;
					}
					if (num21 != -3 && num21 != 5)
					{
						num21 = 1;
					}
					if (AlertAtNewSupportResistance)
					{
						TC_Alert("New Resistance", AlertSoundNewSupportResistance);
					}
				}
				if (flag3)
				{
					Support[0] = (num20);
				}
				if (flag4)
				{
					Resistance[0] = (num20);
				}
                /// show breakout below
				if (flag3 && Close[0] < num20 && num19 > 0.0 && flag5)
				{
					if (num21 == 3)
					{
						num21 = 4;
					}
					else
					{
						num21 = -2;
					}
					flag5 = false;
					if (BreakOut)
					{
						BreakLo[0] = (Low[0]); // this is the dot
						BreakoutBar = CurrentBar;	//this is my flag
						if (AlertAtBreakOut)
						{
							TC_Alert("BreakOut", AlertSoundBreakOut);
						}
					}
				}
                /// show false breakout in  3 bars
				if(Close[0] > num20 // i think num20 is current SR
					&& CurrentBar - BreakoutBar < 6  // break out 6 bars ago
					&& (CurrentBar -BreakoutBar ) > 1	// giving overshoot 1 bar to mature
					&& !OS_Flag )	// no recent print
						{
							Draw.Text(this, "LONG SIGNAL" + CurrentBar, "os", 0, Low[0]-4*TickSize, Brushes.Red);
							OS_Flag = true;
						}
				if( CurrentBar - BreakoutBar > 12 )
					OS_Flag = false;		// reset OS FLAG
					
                /// show breakout above
				if (flag4 && Close[0] > num20 && num19 < 0.0 && flag6)
				{
					if (num21 == -3)
					{
						num21 = -4;
					}
					else
					{
						num21 = 2;
					}
					flag6 = false;
					if (BreakOut)
					{
						BreakHi[0] = (High[0]);
						BreakoutBar_H = CurrentBar;	//this is my flag
						//DrawArrowUp("MyArrowUp"+CurrentBar, 0, Low[0], Color.Green);
						if (AlertAtBreakOut)
						{
							TC_Alert("BreakOut", AlertSoundBreakOut);
						}
					}						
				}
				
                /// show breakout overshoot  in  6 bars
				if( (CurrentBar -BreakoutBar_H) < 6  // break out 6 bars ago
					&& (CurrentBar -BreakoutBar_H) > 1	// giving overshoot 1 bar to mature
					&& !OS_Flag_H
					&& Close[0] < num20 )
						{
							Draw.Text(this, "HiOS" + CurrentBar, "os", 0, High[0]+4*TickSize, Brushes.WhiteSmoke);
							OS_Flag_H = true;
						}
				if( CurrentBar - BreakoutBar_H > 12 )
					OS_Flag_H = false;		// reset OS FLAG	
				
				EndOfTrendSeries[0] = (false);
				PullBackSeries[0] = (false);
				if (PullBack && num21 == 3 && (int)Math.Round(CountSeries[1]) != 3)
				{
					PullBackSeries[0] = (true);
					Draw.Text(this, "Text1" + CurrentBar, "PB", 3, num20, Brushes.Red);
					if (AlertAtPullBack)
					{
						TC_Alert("PullBack", AlertSoundPullBack);
					}
				}
				if (PullBack && num21 == -3 && (int)Math.Round(CountSeries[1]) != -3)
				{
					PullBackSeries[0] = (true);
					Draw.Text(this, "Text2" + CurrentBar, "PB", 3, num20, Brushes.WhiteSmoke);
					if (AlertAtPullBack)
					{
						TC_Alert("PullBack", AlertSoundPullBack);
					}
				}
				if (EndOfTrend && num21 == 5 && (int)Math.Round(CountSeries[1]) != 5)
				{
					EndOfTrendSeries[0] = (true);
					Draw.Text(this, "Text3" + CurrentBar, "END", 5, num20, Brushes.WhiteSmoke);
					if (AlertAtEndOfTrend)
					{
						TC_Alert("End Of Trend", AlertSoundEndOfTrend);
					}
				}
				if (EndOfTrend && num21 == -5 && (int)Math.Round(CountSeries[1]) != -5)
				{
					EndOfTrendSeries[0] = (true);					
					Draw.Text(this, "Text4" + CurrentBar, "END", 5, num20, Brushes.Red);
					if (AlertAtEndOfTrend)
					{
						TC_Alert("End Of Trend", AlertSoundEndOfTrend);
					}
				}
			}
			HighSetupSeries[0] = ((double)(flag ? 1 : 0));
			LowSetupSeries[0] = ((double)(flag2 ? 1 : 0));
			SRSeries[0] = (num20);
			CountSeries[0] = ((double)num21);
			PlotLowSeries[0] = ((double)(flag3 ? 1 : 0));
			PlotHighSeries[0] = ((double)(flag4 ? 1 : 0));
			BreakLowSeries[0] = ((double)(flag5 ? 1 : 0));
			BreakHighSeries[0] = ((double)(flag6 ? 1 : 0));
		}
		
		private void TC_Alert(string message, string sound)
		{
			Alert("myAlert", Priority.High, Name + ": " + message, sound, 15, Brushes.WhiteSmoke, Brushes.Black); 
		}
				
		public override string ToString()
		{
			return Name;
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="BreakOut", Order=1, GroupName="Parameters")]
		public bool BreakOut
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="PullBack", Order=2, GroupName="Parameters")]
		public bool PullBack
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="EndOfTrend", Order=3, GroupName="Parameters")]
		public bool EndOfTrend
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Alert At New Support / Resistance", Order=4, GroupName="Parameters")]
		public bool AlertAtNewSupportResistance
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Alert At BreakOut", Order=5, GroupName="Parameters")]
		public bool AlertAtBreakOut
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Alert At PullBack", Order=6, GroupName="Parameters")]
		public bool AlertAtPullBack
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Alert At End Of Trend", Order=7, GroupName="Parameters")]
		public bool AlertAtEndOfTrend
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="AlertSound New Support/Resistance", Order=8, GroupName="Parameters")]
		public string AlertSoundNewSupportResistance
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="AlertSound BreakOut", Order=9, GroupName="Parameters")]
		public string AlertSoundBreakOut
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="AlertSound PullBack", Order=10, GroupName="Parameters")]
		public string AlertSoundPullBack
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="AlertSound End Of Trend", Order=11, GroupName="Parameters")]
		public string AlertSoundEndOfTrend
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Support
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Resistance
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BreakLo
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BreakHi
		{
			get { return Values[3]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BetterSinewavePlot[] cacheBetterSinewavePlot;
		public BetterSinewavePlot BetterSinewavePlot(bool breakOut, bool pullBack, bool endOfTrend, bool alertAtNewSupportResistance, bool alertAtBreakOut, bool alertAtPullBack, bool alertAtEndOfTrend, string alertSoundNewSupportResistance, string alertSoundBreakOut, string alertSoundPullBack, string alertSoundEndOfTrend)
		{
			return BetterSinewavePlot(Input, breakOut, pullBack, endOfTrend, alertAtNewSupportResistance, alertAtBreakOut, alertAtPullBack, alertAtEndOfTrend, alertSoundNewSupportResistance, alertSoundBreakOut, alertSoundPullBack, alertSoundEndOfTrend);
		}

		public BetterSinewavePlot BetterSinewavePlot(ISeries<double> input, bool breakOut, bool pullBack, bool endOfTrend, bool alertAtNewSupportResistance, bool alertAtBreakOut, bool alertAtPullBack, bool alertAtEndOfTrend, string alertSoundNewSupportResistance, string alertSoundBreakOut, string alertSoundPullBack, string alertSoundEndOfTrend)
		{
			if (cacheBetterSinewavePlot != null)
				for (int idx = 0; idx < cacheBetterSinewavePlot.Length; idx++)
					if (cacheBetterSinewavePlot[idx] != null && cacheBetterSinewavePlot[idx].BreakOut == breakOut && cacheBetterSinewavePlot[idx].PullBack == pullBack && cacheBetterSinewavePlot[idx].EndOfTrend == endOfTrend && cacheBetterSinewavePlot[idx].AlertAtNewSupportResistance == alertAtNewSupportResistance && cacheBetterSinewavePlot[idx].AlertAtBreakOut == alertAtBreakOut && cacheBetterSinewavePlot[idx].AlertAtPullBack == alertAtPullBack && cacheBetterSinewavePlot[idx].AlertAtEndOfTrend == alertAtEndOfTrend && cacheBetterSinewavePlot[idx].AlertSoundNewSupportResistance == alertSoundNewSupportResistance && cacheBetterSinewavePlot[idx].AlertSoundBreakOut == alertSoundBreakOut && cacheBetterSinewavePlot[idx].AlertSoundPullBack == alertSoundPullBack && cacheBetterSinewavePlot[idx].AlertSoundEndOfTrend == alertSoundEndOfTrend && cacheBetterSinewavePlot[idx].EqualsInput(input))
						return cacheBetterSinewavePlot[idx];
			return CacheIndicator<BetterSinewavePlot>(new BetterSinewavePlot(){ BreakOut = breakOut, PullBack = pullBack, EndOfTrend = endOfTrend, AlertAtNewSupportResistance = alertAtNewSupportResistance, AlertAtBreakOut = alertAtBreakOut, AlertAtPullBack = alertAtPullBack, AlertAtEndOfTrend = alertAtEndOfTrend, AlertSoundNewSupportResistance = alertSoundNewSupportResistance, AlertSoundBreakOut = alertSoundBreakOut, AlertSoundPullBack = alertSoundPullBack, AlertSoundEndOfTrend = alertSoundEndOfTrend }, input, ref cacheBetterSinewavePlot);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BetterSinewavePlot BetterSinewavePlot(bool breakOut, bool pullBack, bool endOfTrend, bool alertAtNewSupportResistance, bool alertAtBreakOut, bool alertAtPullBack, bool alertAtEndOfTrend, string alertSoundNewSupportResistance, string alertSoundBreakOut, string alertSoundPullBack, string alertSoundEndOfTrend)
		{
			return indicator.BetterSinewavePlot(Input, breakOut, pullBack, endOfTrend, alertAtNewSupportResistance, alertAtBreakOut, alertAtPullBack, alertAtEndOfTrend, alertSoundNewSupportResistance, alertSoundBreakOut, alertSoundPullBack, alertSoundEndOfTrend);
		}

		public Indicators.BetterSinewavePlot BetterSinewavePlot(ISeries<double> input , bool breakOut, bool pullBack, bool endOfTrend, bool alertAtNewSupportResistance, bool alertAtBreakOut, bool alertAtPullBack, bool alertAtEndOfTrend, string alertSoundNewSupportResistance, string alertSoundBreakOut, string alertSoundPullBack, string alertSoundEndOfTrend)
		{
			return indicator.BetterSinewavePlot(input, breakOut, pullBack, endOfTrend, alertAtNewSupportResistance, alertAtBreakOut, alertAtPullBack, alertAtEndOfTrend, alertSoundNewSupportResistance, alertSoundBreakOut, alertSoundPullBack, alertSoundEndOfTrend);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BetterSinewavePlot BetterSinewavePlot(bool breakOut, bool pullBack, bool endOfTrend, bool alertAtNewSupportResistance, bool alertAtBreakOut, bool alertAtPullBack, bool alertAtEndOfTrend, string alertSoundNewSupportResistance, string alertSoundBreakOut, string alertSoundPullBack, string alertSoundEndOfTrend)
		{
			return indicator.BetterSinewavePlot(Input, breakOut, pullBack, endOfTrend, alertAtNewSupportResistance, alertAtBreakOut, alertAtPullBack, alertAtEndOfTrend, alertSoundNewSupportResistance, alertSoundBreakOut, alertSoundPullBack, alertSoundEndOfTrend);
		}

		public Indicators.BetterSinewavePlot BetterSinewavePlot(ISeries<double> input , bool breakOut, bool pullBack, bool endOfTrend, bool alertAtNewSupportResistance, bool alertAtBreakOut, bool alertAtPullBack, bool alertAtEndOfTrend, string alertSoundNewSupportResistance, string alertSoundBreakOut, string alertSoundPullBack, string alertSoundEndOfTrend)
		{
			return indicator.BetterSinewavePlot(input, breakOut, pullBack, endOfTrend, alertAtNewSupportResistance, alertAtBreakOut, alertAtPullBack, alertAtEndOfTrend, alertSoundNewSupportResistance, alertSoundBreakOut, alertSoundPullBack, alertSoundEndOfTrend);
		}
	}
}

#endregion
