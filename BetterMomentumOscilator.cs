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
	public class BetterMomentumOscilator : Indicator
	{
		private bool exhaustionShown = true;

		private bool firstDiv = true;

		private bool diverg;

		private bool flush = true;

		private bool proSignalExit = true;

		private int exLookback = 60;

		private bool showTickModelBars;

		private bool useTickVolume = true;

		private double offsetMulti = 0.5;

		private string password = "";

		private bool firstDivAlert;

		private bool divergenceAlert;

		private bool flushAlert;

		private bool exhaustionAlert;

		private bool audioAlerts;

		//private string audioFile = "Alert4.wav";

		private bool proSignalAlert;

		private bool lastWasUpTick;

		private double lastLegacyPrice;

		private int lastCurrentBar = -1;

		private int barUpTicks;

		private int barDnTicks;

		private double barUpVol;

		private double barDnVol;

		private int lastUpTicks;

		private int lastDnTicks;

		private double lastUpVol;

		private double lastDnVol;

		private int LastBar = -1;

		private double pLL;

		private double pHH;

		private double pLLPrice;

		private double pHHPrice;

		private double pValue2;

		private int pCountL;

		private int pCountH;

		private double pOscLowOlder;

		private double pOscLowOld;

		private double pOscLow;

		private double pPriceLowOlder;

		private double pPriceLowOld;

		private double pPriceLow;

		private double pOscHighOlder;

		private double pOscHighOld;

		private double pOscHigh;

		private double pPriceHighOlder;

		private double pPriceHighOld;

		private double pPriceHigh;

		private double fLL;

		private double fHH;

		private double fLLPrice;

		private double fHHPrice;

		private double fValue2;

		private int fCountL;

		private int fCountH;

		private double fOscLowOlder;

		private double fOscLowOld;

		private double fOscLow;

		private double fPriceLowOlder;

		private double fPriceLowOld;

		private double fPriceLow;

		private double fOscHighOlder;

		private double fOscHighOld;

		private double fOscHigh;

		private double fPriceHighOlder;

		private double fPriceHighOld;

		private double fPriceHigh;

		private int FlushOK;

		private double MaxDiv;

		private double ExValue;

		private bool eod = true;

		private double lastVol;

		private double askPrice;

		private double bidPrice;

		private double lastPrice;

		private double thisBarBUYVol;

		private double thisBarSELLVol;

		/// <summary>
		///  Time
		/// </summary>
		private DateTime noTime = new DateTime(1, 1, 1);

		private DateTime askDataTime = new DateTime(1, 1, 1);

		private DateTime bidDataTime = new DateTime(1, 1, 1);

		private DateTime lastDataTime = new DateTime(1, 1, 1);
		
		private DateTime lastLegacyTickTime = DateTime.MinValue;

//		private DateTime Now
//		{
//			get
//			{
//				DateTime now;
//				// if (Bars() != null && Bars().get_MarketData() != null && Bars().get_MarketData().get_Connection().get_Options().get_Provider() == 3)
//				if (ChartBars != null && MarketData != null && Bars().get_MarketData().get_Connection().get_Options().get_Provider() == 3)
//				{
//					now = Bars().get_MarketData().get_Connection().get_Now();
//				}
//				else
//				{
//					now = DateTime.Now;
//				}
//				return now;
//			}
//		}
		
		private bool bInit;

		private bool useHLCModel = true;

		private bool skipNextTickTick;

		private bool hadFirstRealtimeTick;

		private int BarCount;

		private int LookForExit;

		private bool paa;

		private string simpleP = "2339hvsw";
		
		/// <summary>
		///  Arrays
		/// </summary>
		private Series<int> FirstDiverg;
		
		private Series<int> BetterProAm;
		
		private Series<int> signals;
		
		private Series<double> Value2Series; 
		
		private Series<double> Value3Series;
		
		private Series<double> HighestSeries;
		
		private Series<double> mom;
		
		private Series<double> Value1Series;

		private double[] series = new double[4];
		
		private bool debug = false;
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				
				FirstDiverg = new Series<int>(this, MaximumBarsLookBack.Infinite);
				BetterProAm = new Series<int>(this, MaximumBarsLookBack.Infinite);
				signals = new Series<int>(this, MaximumBarsLookBack.Infinite);
				
				Value2Series = new Series<double>(this, MaximumBarsLookBack.Infinite);
				Value3Series = new Series<double>(this, MaximumBarsLookBack.Infinite);
				HighestSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				mom = new Series<double>(this, MaximumBarsLookBack.Infinite);
				Value1Series = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "Better Momentum Oscilator";
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
				IsSuspendedWhileInactive = true;
				FirstDiv				= true;
				Diverg					= true;
				Flush					= true;
				ProSignalExit			= true;
				UseTickVolume			= true;
				FirstDivAlert			= true;
				DivergenceAlert			= true;
				FlushAlert				= true;
				ProSignalAlert			= true;
				AudioAlerts				= true;
				AudioFile				= @"C:\Users\trade\OneDrive\Documents\NinjaTrader 8\acme\NT sounds\ATG-Ping.wav";
				Spacer = 2500;
				
				AddPlot(Brushes.Cyan, "Momentum");
				AddPlot(new Stroke(Brushes.Red, 3), PlotStyle.Dot, "DivBull");
				AddPlot(new Stroke(Brushes.WhiteSmoke, 3), PlotStyle.Dot, "DivBear");
				AddPlot(new Stroke(Brushes.Cyan, 6), PlotStyle.Dot, "FlushBull");
				AddPlot(new Stroke(Brushes.Yellow, 6), PlotStyle.Dot, "FlushBear");
				AddPlot(new Stroke(Brushes.Cyan, 10), PlotStyle.Dot, "ProSignalExitBull");
				AddPlot(new Stroke(Brushes.Cyan, 10), PlotStyle.Dot, "ProSignalExitBear");
				AddPlot(new Stroke(Brushes.Red, 6), PlotStyle.Dot, "FirstDivBull");
				AddPlot(new Stroke(Brushes.WhiteSmoke, 6), PlotStyle.Dot, "FirstDivBear");
				
				ClearOutputWindow();
			}
			else if (State == State.Configure)
			{
				bool flag = BarsPeriod.BarsPeriodType == BarsPeriodType.Renko || BarsPeriod.BarsPeriodType == BarsPeriodType.Kagi 
					|| BarsPeriod.BarsPeriodType == BarsPeriodType.LineBreak || BarsPeriod.BarsPeriodType == BarsPeriodType.PointAndFigure;
			
				/// if tick volume and not daily and not monthly  
				if (UseTickVolume && BarsPeriod.BarsPeriodType != BarsPeriodType.Day && BarsPeriod.BarsPeriodType != BarsPeriodType.Month)
				{
					///if eod OR  (minute bars AND NOT minute bars  and ( not minute bars OR period > 60 )
					//if (!flag && (eod || (base.get_BarsPeriods()[0].get_Id() == 4 && (base.get_BarsPeriods()[0].get_Id() != 4 || base.get_BarsPeriods()[0].get_Value() >= 60))))
					if (!flag && (eod || (BarsPeriod.BarsPeriodType == BarsPeriodType.Minute 
						&& (BarsPeriod.BarsPeriodType != BarsPeriodType.Minute || BarsPeriod.Value  >= 60))))
					{
						AddDataSeries(Data.BarsPeriodType.Minute, 1);
						base.Print(Instrument.FullName + " adding Minute stream");
					}
					else
					{
						AddDataSeries(Data.BarsPeriodType.Tick, 1);
						Print(Instrument.FullName + " adding Tick stream");
					}
				}
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress == 0 && (CurrentBar <= 5 || !bInit))
			{
				if (!bInit)
				{
					eod = (BarsPeriod.BarsPeriodType == BarsPeriodType.Day || BarsPeriod.BarsPeriodType == BarsPeriodType.Week 
					|| BarsPeriod.BarsPeriodType == BarsPeriodType.Month || BarsPeriod.BarsPeriodType == BarsPeriodType.Year);
					//paa = this.Password.ToUpper().Contains(this.simpleP.ToUpper());
					if (Calculate == Calculate.OnPriceChange)
					{
						Draw.TextFixed(this, "MyTextFixed", "CalculateOnBarClose required for " + Instrument.FullName, TextPosition.TopRight);
						
						return;
					}
					bInit = true;
				}
				return;
			}
			if (!bInit)
			{
				return;
			}
			if (useTickVolume && CurrentBar > 1)
			{
				LegacyUpDownTickModel();
			}
			if (BarsInProgress > 0)
			{
				return;
			}
			if (CurrentBar < 21)
			{
				for (int i = 0; i < series.Length; i++)
				{
					series[i] = 0.0;
				}
				return;
			}
			
			if (CurrentBar > 0)
			{
				double num = High[0] - Low[0];
				num = rnd(num);
				GetData(num);
				if (BarsInProgress == 0)
				{
					if (CurrentBar > LastBar)
					{
						pLL = fLL;
						pHH = fHH;
						pLLPrice = fLLPrice;
						pHHPrice = fHHPrice;
						pValue2 = fValue2;
						pCountL = fCountL;
						pCountH = fCountH;
						pOscLowOlder = fOscLowOlder;
						pOscLowOld = fOscLowOld;
						pOscLow = fOscLow;
						pPriceLowOlder = fPriceLowOlder;
						pPriceLowOld = fPriceLowOld;
						pPriceLow = fPriceLow;
						pOscHighOlder = fOscHighOlder;
						pOscHighOld = fOscHighOld;
						pOscHigh = fOscHigh;
						pPriceHighOlder = fPriceHighOlder;
						pPriceHighOld = fPriceHighOld;
						pPriceHigh = fPriceHigh;
						LastBar = CurrentBar;
					}
					else
					{
						fLL = pLL;
						fHH = pHH;
						fLLPrice = pLLPrice;
						fHHPrice = pHHPrice;
						fValue2 = pValue2;
						fCountL = pCountL;
						fCountH = pCountH;
						fOscLowOlder = pOscLowOlder;
						fOscLowOld = pOscLowOld;
						fOscLow = pOscLow;
						fPriceLowOlder = pPriceLowOlder;
						fPriceLowOld = pPriceLowOld;
						fPriceLow = pPriceLow;
						fOscHighOlder = pOscHighOlder;
						fOscHighOld = pOscHighOld;
						fOscHigh = pOscHigh;
						fPriceHighOlder = pPriceHighOlder;
						fPriceHighOld = pPriceHighOld;
						fPriceHigh = pPriceHigh;
					}
					
					double arg_3D7_0 = TickSize / 1000.0;
					double num2 = series[1];
	
					Value1Series[0] = (num2);
					fValue2 = Summation(Value1Series, 5);
					Value2Series[0] = (fValue2);
					double num3 = Math.Abs(fValue2);
					Value3Series[0] = (num3);
					BetterProAm[0] = (0);
					double[] array = new double[20];
					for (int j = 0; j < array.Length; j++)
					{
						array[j] = Volumes[0][j];
					}
					Array.Sort<double>(array);
					bool flag = Volumes[0][0] == array[19];
					bool flag2 = Volumes[0][0] == array[18];
Momentum[0] = Value2Series[0]; // just pos Value3Series[0];  //*** Value2Series[0]; //maybe num2;					
					
					if (flag || flag2)
					{
						BetterProAm[0] = (6);
					}
					BarCount++;
					FirstDiverg[0] = FirstDiverg[1];
					
					if (debug)
					{
						Print(string.Concat(new object[]
						{
							"Month=\t",
							Time[0].Month,
							" Day=\t",
							Time[0].Day,
							" Time=\t",
							Time[0].TimeOfDay,
							" Volume= ",
							Volume[0],
							" Value1= ",
							num2,
							" Value2= ",
							fValue2,
							" Value3= ",
							num3,
							" CountL=\t",
							fCountL,
							" CountH=\t",
							fCountH,
							" LL= ",
							fLL,
							" HH= ",
							fHH,
							" BetterProAm=\t",
							BetterProAm[0],
							" FirstDiverge=\t",
							FirstDiverg[0],
							" FlushOK=\t",
							FlushOK,
							" ExValue= ",
							ExValue,
							" MaxDiv= ",
							MaxDiv,
							"\tSeries: ",
							series[1].ToString()
						
						}));
					}
					
					if ((fValue2 < 0.0 && pValue2 >= 0.0) || (fValue2 < fLL / 2.0 && pValue2 >= pLL / 2.0))
					{
						fLL = fValue2;
					}
					if (fValue2 < 0.0 && fValue2 <= fLL)
					{
						fLL = fValue2;
						fCountL = 0;
					}
					if ((fValue2 > 0.0 && pValue2 <= 0.0) || (fValue2 > fHH / 2.0 && pValue2 <= pHH / 2.0))
					{
						fHH = fValue2;
					}
					if (fValue2 > 0.0 && fValue2 >= fHH)
					{
						fHH = fValue2;
						fCountH = 0;
					}
					fCountL++;
					fCountH++;
					if (fValue2 > fLL / 2.0 && pValue2 <= pLL / 2.0)
					{
						fOscLowOlder = fOscLowOld;
						fOscLowOld = fOscLow;
						fOscLow = fLL;
						fPriceLowOlder = fPriceLowOld;
						fPriceLowOld = fPriceLow;
						fPriceLow = Lows[0][fCountL - 1];
					}
					if (fValue2 < fHH / 2.0 && pValue2 >= pHH / 2.0)
					{
						fOscHighOlder = fOscHighOld;
						fOscHighOld = fOscHigh;
						fOscHigh = fHH;
						fPriceHighOlder = fPriceHighOld;
						fPriceHighOld = fPriceHigh;
						fPriceHigh = Highs[0][fCountH - 1];
					}
					bool flag3 = fOscLow > fOscLowOld && fPriceLow <= fPriceLowOld;
					bool flag4 = fOscLow > fOscLowOlder && fPriceLow <= fPriceLowOlder;
					bool flag5 = fOscHigh < fOscHighOld && fPriceHigh >= fPriceHighOld;
					bool flag6 = fOscHigh < fOscHighOlder && fPriceHigh >= fPriceHighOlder;
					double num4 = ATR(20)[0] * offsetMulti;
					
					if (Value3Series[1] == MAX(Value3Series, exLookback)[1] 
						|| (Value3Series[1] > 0.98 * MAX(Value3Series, exLookback)[2] && Value2Series[1] > 0.0 && FlushOK == 1) 
						|| (Value3Series[1] > 0.98 * MAX(Value3Series, exLookback)[2] && Value2Series[1] < 0.0 && FlushOK == -1))
					{
						if (Value2Series[1] < 0.0 && Value2Series[0] > Value2Series[1])
						{
							if (BetterProAm[0] != 6 && BetterProAm[1] != 6 && BetterProAm[2] != 6)
							{
								if (BetterProAm[3] != 6)
								{
									LookForExit = 1;
									BarCount = 0;
									goto IL_B77;
								}
							}
							if (proSignalExit)
							{
								ProSignalExitBear[1] = Momentum[1]; // Low[0] - num4;
								Text_Alert("pro", true, 1);
							}
							if (proSignalAlert)
							{
								TC_Alert("Pro Signal Exit Shorts Alert", AudioFile);
							}
							IL_B77:
							FirstDiverg[0] =  1;
							FlushOK = 1;
							ExValue = Value2Series[1];
							MaxDiv = 0.0;
						}
						
						if (Value2Series[1] > 0.0 && Value2Series[0] < Value2Series[1])
						{
							if (BetterProAm[0] != 6 && BetterProAm[1] != 6 && BetterProAm[2] != 6)
							{
								if (BetterProAm[3] != 6)
								{
									LookForExit = -1;
									BarCount = 0;
									goto IL_C8E;
								}
							}
							if (proSignalExit)
							{
								ProSignalExitBull[1] = Momentum[1]; // High[0] + num4;
								Text_Alert("pro", false, 1);
							}
							if (proSignalAlert)
							{
								TC_Alert("Pro Signal Alert", AudioFile);
							}
							IL_C8E:
							FirstDiverg[0] = -1;
							FlushOK = -1;
							ExValue = Value2Series[1];
							MaxDiv = 0.0;
						}
					}
					
					if (LookForExit == 1 && BetterProAm[0] == 6 && BarCount < 4)
					{
						if (ProSignalExit)
						{
							ProSignalExitBear[0] = Momentum[0]; //Low[0] - num4;
						}
						if (proSignalAlert)
						{
							TC_Alert("Pro Signal Alert", AudioFile);
						}
						LookForExit = 0;
					}
					
					if (LookForExit == -1 && BetterProAm[0] == 6 && BarCount < 4)
					{
						if (ProSignalExit)
						{
							ProSignalExitBull[0] = Momentum[0]; //High[0] + num4;
						}
						if (proSignalAlert)
						{
							TC_Alert("Pro Signal Alert", AudioFile);
						}
						LookForExit = 0;
					}
				int BackSpace = 3;	
					if (fValue2 > fLL / 2.0 && pValue2 <= pLL / 2.0)
					{
						
						if (flag3 || flag4)
						{
							if (FirstDiverg[0] == 1)
							{
								if (firstDiv)
								{
									// first red dot
									///this.FirstDivBull.Set(base.get_Low().get_Item(0) - num4);
									Text_Alert("div", true, fCountL - 1  );
									FirstDivBull[fCountL - 1] = Momentum[fCountL - 1]; //Low[0] - num4;  
								}
								if (firstDivAlert)
								{
									TC_Alert("Bullish First Divergence Alert", AudioFile);	
								}
								FirstDiverg[0] = 0;
							}
							if (FirstDiverg[0] != 1)
							{
								if (diverg && fCountL < CurrentBar - 1)
								{
									/// this.DivBull.Set(this.fCountL - 1, base.get_Low().get_Item(this.fCountL - 1) - num4);
									DivBull[fCountL - 1] = Momentum[fCountL - 1]; //Low[fCountL - 1] - num4;
									Text_Alert("div", true, fCountL - 1);
								}
								if (divergenceAlert)
								{
									TC_Alert("Bullish Divergence Alert", AudioFile);
								}
							}
							if ((FlushOK == 1 || FlushOK == 2) && fLL < MaxDiv)
							{
								MaxDiv = fLL;
								FlushOK = 2;
							}
						}
						if (!flag3 && !flag4 && FlushOK == 2 && fLL < MaxDiv && fLL > ExValue && fLL < ExValue / 2.0)
						{
							if (flush)
							{
								///FlushBull.Set(base.get_Low().get_Item(0) - num4 * 1.1);
								FlushBull[1] = Momentum[1]; //(Low[0]- num4 * 1.1);
								Text_Alert("flush", true, 1);
							}
							if (FlushAlert)
							{
								TC_Alert("Bullish Flush Alert", AudioFile);
							}
						}
					}
					
					if (fValue2 < fHH / 2.0 && pValue2 >= pHH / 2.0)
					{
						if (flag5 || flag6)
						{
							if (FirstDiverg[0] == -1)
							{
								if (firstDiv)
								{
									Text_Alert("div", false, fCountH - 1);
									FirstDivBear[fCountH - 1] = Momentum[fCountH - 1]; //(High[0] + num4);
								}
								if (firstDivAlert)
								{
									TC_Alert("Bearish First Divergence Alert", AudioFile);
								}
								FirstDiverg[0] = 0;
							}
							if (FirstDiverg[1] != -1)
							{
								if (diverg && fCountH < CurrentBar - 1)
								{
									Text_Alert("div", false, fCountH - 1);
									//DivBear.Set(fCountH - 1, base.get_High().get_Item(fCountH - 1) + num4);
									DivBear[fCountH - 1] = Momentum[fCountH - 1]; // High[fCountH - 1] + num4;
								}
								if (divergenceAlert)
								{
									TC_Alert("Bearish Divergence Alert", AudioFile);
								}
							}
							if ((FlushOK == -1 || FlushOK == -2) && fHH > MaxDiv)
							{
								MaxDiv = fHH;
								FlushOK = -2;
							}
						}
						if (!flag5 && !flag6 && FlushOK == -2 && fHH > MaxDiv && fHH < ExValue && fHH > ExValue / 2.0)
						{
							if (flush)
							{
								FlushBear[1] = Momentum[1]; // High[0] + num4 * 1.1;
								Text_Alert("flush", false, 1);
							}
							if (FlushAlert)
							{
								TC_Alert("Bearish Flush Alert", AudioFile);
							}
						}
					}
				}
				
			}
			
			Print(mom.ToString());
			
		}

		#region Misc_Functions
		
		private void TC_Alert(string message, string sound)
		{
			Alert("myAlert", Priority.High, Name + ": " + message, sound, 15, Brushes.WhiteSmoke, Brushes.Black); 
		}
		
		private void Text_Alert(string message, bool bull, int bars)
		{
			DrawOnPricePanel = false;
			
			if (bull) {
				Draw.Text(this, "MyText"+CurrentBar, message, bars, Momentum[bars] - Spacer, Brushes.Red);
			} else {
				Draw.Text(this, "MyText"+CurrentBar, message, bars, Momentum[bars] + Spacer, Brushes.WhiteSmoke);
			}
		}
		
		private void LegacyUpDownTickModel()
		{
			int num = BarsArray.Length - 1;
			if (BarsInProgress  == num)
			{
				TickModelProcessor(num);
				skipNextTickTick = false;
				return;
			}
			if (BarsInProgress == 0)
			{
				if (State == State.Realtime || (BarsArray.Length > 1 && BarsArray[1].BarsPeriod.BarsPeriodType == BarsPeriodType.Minute))
				{
					TickModelProcessor(num);
					skipNextTickTick = true;
				}
				if (CurrentBar> lastCurrentBar)
				{
					lastUpTicks = barUpTicks;
					lastDnTicks = barDnTicks;
					lastUpVol = barUpVol;
					lastDnVol = barDnVol;
					thisBarBUYVol = barUpVol;
					thisBarSELLVol = barDnVol;
					barUpTicks = 0;
					barDnTicks = 0;
					barUpVol = 0.0;
					barDnVol = 0.0;
					lastCurrentBar = CurrentBar;
				}
			}
		}
		
		private void TickModelProcessor(int bip)
		{
			if (Closes[bip] == null)
			{
				return;
			}
			if (Volumes[bip] == null)
			{
				return;
			}
			lastLegacyTickTime = Times[bip][0];  /// did i create Times correctly?
			if (skipNextTickTick)
			{
				return;
			}
			if (State == State.Realtime && !hadFirstRealtimeTick)
			{
				hadFirstRealtimeTick = true;
			}
			if (Closes[bip][0] > lastLegacyPrice)
			{
				barUpTicks++;
				barUpVol += Volumes[bip][0];
				lastLegacyPrice = Closes[bip][0];
				lastWasUpTick = true;
				return;
			}
			if (Closes[bip][0]  < lastLegacyPrice)
			{
				barDnTicks++;
				barDnVol += Volumes[bip][0];
				lastLegacyPrice = Closes[bip][0] ;
				lastWasUpTick = false;
				return;
			}
			if (lastWasUpTick)
			{
				barUpTicks++;
				barUpVol += Volumes[bip][0];
				return;
			}
			barDnTicks++;
			barDnVol += Volumes[bip][0];
		}
		
		private void GetData(double range)
		{
			int num = (State == State.Historical  || Calculate == Calculate.OnBarClose) ? 0 : 1;
			if (useTickVolume)
			{
				if (!(lastLegacyTickTime <= Time[num + 1]) && !eod)
				{
					if (useHLCModel)
					{
						Print(Instrument.FullName + " Switching to TickModel at " + Time[0]);
					}
					useHLCModel = false;
				}
				else
				{
					if (!useHLCModel)
					{
						Print(Instrument.FullName + " Switching to HLC Model at " + Time[0]);
					}
					useHLCModel = true;
				}
			}
			else
			{
				useHLCModel = true;
			}
			if (!useHLCModel)
			{
				if (IsFirstTickOfBar && State == State.Realtime &&  Calculate == Calculate.OnPriceChange)
				{
					thisBarBUYVol = 0.0;
					thisBarSELLVol = 0.0;
				}
				series[1] = thisBarBUYVol - thisBarSELLVol;
				series[2] =  thisBarSELLVol;
				series[3] =  Math.Abs(thisBarBUYVol + thisBarSELLVol);
				if (IsFirstTickOfBar && (State == State.Historical || Calculate == Calculate.OnBarClose))
				{
					thisBarBUYVol = 0.0;
					thisBarSELLVol = 0.0;
				}
				if (showTickModelBars)
				{
					BackBrush = Brushes.PaleTurquoise;
				}
				return;
			}
			if (range == 0.0)
			{
				series[1] =  0.0;
				series[2] =  0.0;
				return;
			}
			if (Close[0] > Open[0])
			{
				series[1] = (Close[0] - Open[0]) / (2.0 * range + Open[0] - Close[0]) * Volume[0]; // questionble presidence
			}
			else if (Close[0] < Open[0])
			{
				series[1] = (Close[0] - Open[0]) / (2.0 * range + Close[0] - Open[0]) * Volume[0];
			}
			else
			{
				series[1] =  0.0;
			}
			series[2] = Volume[0] - series[1];
			series[3] = Volume[0];
		}
		
		private double rnd(double price)
		{
			return Instrument.MasterInstrument.RoundToTickSize(price);
		}

//		public override string ToString()
//		{
//			return Instrument.FullName;
//		}
		
		private double Summation(Series<double> Series, int Period)
		{
			double num = Series[0];
			for (int i = 1; i < Period; i++)
			{
				num += Series[i];
			}
			return num;
		}
		#endregion
		
		
		#region Properties
		[NinjaScriptProperty]
		[Display(Name="FirstDiv", Description="Draw graphics for First Divergence", Order=1, GroupName="Parameters")]
		public bool FirstDiv
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Diverg", Description="Draw graphics for Divergence", Order=2, GroupName="Parameters")]
		public bool Diverg
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Flush", Description="Draw graphics for Flush", Order=3, GroupName="Parameters")]
		public bool Flush
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ProSignalExit", Description="Draw graphics for ProSignalExit", Order=4, GroupName="Parameters")]
		public bool ProSignalExit
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="UseTickVolume", Description="True=Use Tick Volume. (when available.) Uses Minute data for 60m and higher timeframes.", Order=5, GroupName="Parameters")]
		public bool UseTickVolume
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="FirstDivAlert", Description="Alerts for First Divergence", Order=6, GroupName="Parameters")]
		public bool FirstDivAlert
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="DivergenceAlert", Description="Alerts for Divergence", Order=7, GroupName="Parameters")]
		public bool DivergenceAlert
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="FlushAlert", Description="Alerts for Flush", Order=8, GroupName="Parameters")]
		public bool FlushAlert
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ProSignalAlert", Description="Alerts for ProSignal Exit", Order=9, GroupName="Parameters")]
		public bool ProSignalAlert
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="AudioAlerts", Description="Alerts trigger audio", Order=10, GroupName="Parameters")]
		public bool AudioAlerts
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="AudioFile", Description="Audio file for alerts. Use NinjaTrader built-in sounds or full filepath", Order=11, GroupName="Parameters")]
		public string AudioFile
		{ get; set; }

		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Spacer", Description="CPALineWidth", Order=11, GroupName="Parameters")]
		public int Spacer
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Momentum
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DivBull
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DivBear
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> FlushBull
		{
			get { return Values[3]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> FlushBear
		{
			get { return Values[4]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ProSignalExitBull
		{
			get { return Values[5]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ProSignalExitBear
		{
			get { return Values[6]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> FirstDivBull
		{
			get { return Values[7]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> FirstDivBear
		{
			get { return Values[8]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BetterMomentumOscilator[] cacheBetterMomentumOscilator;
		public BetterMomentumOscilator BetterMomentumOscilator(bool firstDiv, bool diverg, bool flush, bool proSignalExit, bool useTickVolume, bool firstDivAlert, bool divergenceAlert, bool flushAlert, bool proSignalAlert, bool audioAlerts, string audioFile, int spacer)
		{
			return BetterMomentumOscilator(Input, firstDiv, diverg, flush, proSignalExit, useTickVolume, firstDivAlert, divergenceAlert, flushAlert, proSignalAlert, audioAlerts, audioFile, spacer);
		}

		public BetterMomentumOscilator BetterMomentumOscilator(ISeries<double> input, bool firstDiv, bool diverg, bool flush, bool proSignalExit, bool useTickVolume, bool firstDivAlert, bool divergenceAlert, bool flushAlert, bool proSignalAlert, bool audioAlerts, string audioFile, int spacer)
		{
			if (cacheBetterMomentumOscilator != null)
				for (int idx = 0; idx < cacheBetterMomentumOscilator.Length; idx++)
					if (cacheBetterMomentumOscilator[idx] != null && cacheBetterMomentumOscilator[idx].FirstDiv == firstDiv && cacheBetterMomentumOscilator[idx].Diverg == diverg && cacheBetterMomentumOscilator[idx].Flush == flush && cacheBetterMomentumOscilator[idx].ProSignalExit == proSignalExit && cacheBetterMomentumOscilator[idx].UseTickVolume == useTickVolume && cacheBetterMomentumOscilator[idx].FirstDivAlert == firstDivAlert && cacheBetterMomentumOscilator[idx].DivergenceAlert == divergenceAlert && cacheBetterMomentumOscilator[idx].FlushAlert == flushAlert && cacheBetterMomentumOscilator[idx].ProSignalAlert == proSignalAlert && cacheBetterMomentumOscilator[idx].AudioAlerts == audioAlerts && cacheBetterMomentumOscilator[idx].AudioFile == audioFile && cacheBetterMomentumOscilator[idx].Spacer == spacer && cacheBetterMomentumOscilator[idx].EqualsInput(input))
						return cacheBetterMomentumOscilator[idx];
			return CacheIndicator<BetterMomentumOscilator>(new BetterMomentumOscilator(){ FirstDiv = firstDiv, Diverg = diverg, Flush = flush, ProSignalExit = proSignalExit, UseTickVolume = useTickVolume, FirstDivAlert = firstDivAlert, DivergenceAlert = divergenceAlert, FlushAlert = flushAlert, ProSignalAlert = proSignalAlert, AudioAlerts = audioAlerts, AudioFile = audioFile, Spacer = spacer }, input, ref cacheBetterMomentumOscilator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BetterMomentumOscilator BetterMomentumOscilator(bool firstDiv, bool diverg, bool flush, bool proSignalExit, bool useTickVolume, bool firstDivAlert, bool divergenceAlert, bool flushAlert, bool proSignalAlert, bool audioAlerts, string audioFile, int spacer)
		{
			return indicator.BetterMomentumOscilator(Input, firstDiv, diverg, flush, proSignalExit, useTickVolume, firstDivAlert, divergenceAlert, flushAlert, proSignalAlert, audioAlerts, audioFile, spacer);
		}

		public Indicators.BetterMomentumOscilator BetterMomentumOscilator(ISeries<double> input , bool firstDiv, bool diverg, bool flush, bool proSignalExit, bool useTickVolume, bool firstDivAlert, bool divergenceAlert, bool flushAlert, bool proSignalAlert, bool audioAlerts, string audioFile, int spacer)
		{
			return indicator.BetterMomentumOscilator(input, firstDiv, diverg, flush, proSignalExit, useTickVolume, firstDivAlert, divergenceAlert, flushAlert, proSignalAlert, audioAlerts, audioFile, spacer);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BetterMomentumOscilator BetterMomentumOscilator(bool firstDiv, bool diverg, bool flush, bool proSignalExit, bool useTickVolume, bool firstDivAlert, bool divergenceAlert, bool flushAlert, bool proSignalAlert, bool audioAlerts, string audioFile, int spacer)
		{
			return indicator.BetterMomentumOscilator(Input, firstDiv, diverg, flush, proSignalExit, useTickVolume, firstDivAlert, divergenceAlert, flushAlert, proSignalAlert, audioAlerts, audioFile, spacer);
		}

		public Indicators.BetterMomentumOscilator BetterMomentumOscilator(ISeries<double> input , bool firstDiv, bool diverg, bool flush, bool proSignalExit, bool useTickVolume, bool firstDivAlert, bool divergenceAlert, bool flushAlert, bool proSignalAlert, bool audioAlerts, string audioFile, int spacer)
		{
			return indicator.BetterMomentumOscilator(input, firstDiv, diverg, flush, proSignalExit, useTickVolume, firstDivAlert, divergenceAlert, flushAlert, proSignalAlert, audioAlerts, audioFile, spacer);
		}
	}
}

#endregion
