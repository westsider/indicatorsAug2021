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
	public class BetterProAmPlot : Indicator
	{
		
		private bool bInit;

		private int Trend;

		private bool Condition1;

		private bool Condition2;

		private bool Condition3;

		private bool Condition4;

		private bool Condition5;

		private bool Condition6;

		private bool Condition7;

		private bool Condition8;

		private bool Condition9;

		private bool Condition10;

		private Series<bool>  amateurs;

		private Series<bool>  professionals;

		private Series<int>  ramboSeries;

		private Series<int> noDSeries;

		private Series<int> profitTakeSeries;

		private Series<int> stoppingVolSeries;
		
		private Series<double> volume;

		private int op;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "Better ProAm Plot";
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
				IsSuspendedWhileInactive = true;
				BullishColor		= Brushes.Red;
				BearishColor		= Brushes.Silver;
				ProAm				= true;
				RAMBO				= true;
				NoD					= true;
				ProfitTake			= true;
				StoppingVol			= true;
				RAMBOAlert			= true;
				NoDAlert			= true;
				ProfitTakeAlert		= true;
				StoppingVolAlert	= true;
				AmAlert				= true;
				ProAlert			= true;
				TextSpaceMulti		= 0.5;
				Sound				= @"C:\Users\trade\OneDrive\Documents\NinjaTrader 8\acme\NT sounds\DownTick.wav";
				SoundEnabled		= true;
				ProColor			= Brushes.DodgerBlue;
				AmColor				= Brushes.Gold;
				
				amateurs = new Series<bool>(this, MaximumBarsLookBack.Infinite);
				professionals = new Series<bool>(this, MaximumBarsLookBack.Infinite);
				ramboSeries = new Series<int>(this, MaximumBarsLookBack.Infinite);
				noDSeries = new Series<int>(this, MaximumBarsLookBack.Infinite);
				profitTakeSeries = new Series<int>(this, MaximumBarsLookBack.Infinite);
				stoppingVolSeries = new Series<int>(this, MaximumBarsLookBack.Infinite);
				volume = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar <= 20) { return; }
			
			volume[0] = Volume[0];
			int period = 20;
			if (Low[0] <  MIN(Low, period)[1])
			{
				Trend = 1;
			}
			if (High[0] > MAX(High, period)[1])
			{
				Trend = -1;
			}
			
			if (MAX(volume, period)[0] != 0.0)
			{
				double num = 0.0;
				double num2 = 0.0;
				double num3 = SMA(Range(), period)[0] * TextSpaceMulti;
				
				if ( volume[0] == MAX(volume, 19)[0] ) {
					Condition1 = true;
				} else {
					Condition1 = false;
				}

//				this.Condition2 = (base.get_Volume().get_Item(0) == this.NthMaxList(1, new double[]
//				{
//					base.get_Volume().get_Item(0),
//					base.get_Volume().get_Item(1),
//					base.get_Volume().get_Item(2),
//					base.get_Volume().get_Item(3),
//					base.get_Volume().get_Item(4),
//					base.get_Volume().get_Item(5),
//					base.get_Volume().get_Item(6),
//					base.get_Volume().get_Item(7),
//					base.get_Volume().get_Item(8),
//					base.get_Volume().get_Item(9),
//					base.get_Volume().get_Item(10),
//					base.get_Volume().get_Item(11),
//					base.get_Volume().get_Item(12),
//					base.get_Volume().get_Item(13),
//					base.get_Volume().get_Item(14),
//					base.get_Volume().get_Item(15),
//					base.get_Volume().get_Item(16),
//					base.get_Volume().get_Item(17),
//					base.get_Volume().get_Item(18),
//					base.get_Volume().get_Item(19)
//				}));
				
				List<double> ListOfBarVolume = new List<double> {volume[0], volume[1], volume[2], volume[3], volume[4],
                    volume[5], volume[6], volume[7], volume[8], volume[9], volume[10], volume[11],
                    volume[12], volume[13], volume[14], volume[15], volume[16], volume[17], volume[18],
                    volume[19]};
				var VolumeOredrList = ListOfBarVolume.OrderByDescending(r => r).Skip(1).FirstOrDefault();
				
				if ( volume[0] == VolumeOredrList ) {
					Condition2 = true;
				} else {
					Condition2 = false;
				}
				
//				this.Condition3 = (base.get_Volume().get_Item(0) == this.NthMinList(0, new double[]
//				{
//					base.get_Volume().get_Item(0),
//					base.get_Volume().get_Item(1),
//					base.get_Volume().get_Item(2),
//					base.get_Volume().get_Item(3),
//					base.get_Volume().get_Item(4),
//					base.get_Volume().get_Item(5),
//					base.get_Volume().get_Item(6),
//					base.get_Volume().get_Item(7),
//					base.get_Volume().get_Item(8),
//					base.get_Volume().get_Item(9),
//					base.get_Volume().get_Item(10),
//					base.get_Volume().get_Item(11),
//					base.get_Volume().get_Item(12),
//					base.get_Volume().get_Item(13),
//					base.get_Volume().get_Item(14),
//					base.get_Volume().get_Item(15),
//					base.get_Volume().get_Item(16),
//					base.get_Volume().get_Item(17),
//					base.get_Volume().get_Item(18),
//					base.get_Volume().get_Item(19)
//				}));
				if ( volume[0] == MIN(volume, 19)[0] ) {
					Condition3 = true;
				} else {
					Condition3 = false;
				}
//				this.Condition4 = (base.get_Volume().get_Item(0) == this.NthMinList(1, new double[]
//				{
//					base.get_Volume().get_Item(0),
//					base.get_Volume().get_Item(1),
//					base.get_Volume().get_Item(2),
//					base.get_Volume().get_Item(3),
//					base.get_Volume().get_Item(4),
//					base.get_Volume().get_Item(5),
//					base.get_Volume().get_Item(6),
//					base.get_Volume().get_Item(7),
//					base.get_Volume().get_Item(8),
//					base.get_Volume().get_Item(9),
//					base.get_Volume().get_Item(10),
//					base.get_Volume().get_Item(11),
//					base.get_Volume().get_Item(12),
//					base.get_Volume().get_Item(13),
//					base.get_Volume().get_Item(14),
//					base.get_Volume().get_Item(15),
//					base.get_Volume().get_Item(16),
//					base.get_Volume().get_Item(17),
//					base.get_Volume().get_Item(18),
//					base.get_Volume().get_Item(19)
//				}));
				var VolumeMinList = ListOfBarVolume.OrderByDescending(r => r).Skip(1).LastOrDefault();
				if ( volume[0] == VolumeMinList ) {
						Condition4 = true;
					} else {
						Condition4 = false;
					}
				if (ProAm && (Condition1 || Condition2))
				{
					//base.set_BarColor(this.ProColor);
					BarBrush = ProColor; 
					CandleOutlineBrush = ProColor; 
					professionals[0]= (true);
					if (ProAlert)
					{
						//base.Alert("Professionals alert", 1, "Professionals alert", this.SoundEnabled ? this.Sound : "", 10, Color.White, this.ProColor);
						TC_Alert("Professionals alert", Sound);
					}
				}
				else
				{
					professionals[0] = (false);
				}
				if (ProAm && (Condition3 || Condition4))
				{
					//base.set_BarColor(this.AmColor);
					BarBrush = AmColor; 
					CandleOutlineBrush = AmColor;
					amateurs[0] = (true);
					if (AmAlert)
					{
						//base.Alert("Amateurs alert", 1, "Amateurs alert", this.SoundEnabled ? this.Sound : "", 10, Color.Black, this.AmColor);
						TC_Alert("Amateurs alert alert", Sound);
					}
				}
				else
				{
					amateurs[0] = (false);
				}
//				this.Condition1 = (volume.get_Item(0) == this.NthMinList(0, new double[]
//				{
//					volume.get_Item(0),
//					volume.get_Item(1),
//					volume.get_Item(2),
//					volume.get_Item(3),
//					volume.get_Item(4),
//					volume.get_Item(5),
//					volume.get_Item(6),
//					volume.get_Item(7),
//					volume.get_Item(8),
//					volume.get_Item(9),
//					volume.get_Item(10),
//					volume.get_Item(11),
//					volume.get_Item(12),
//					volume.get_Item(13),
//					volume.get_Item(14),
//					volume.get_Item(15),
//					volume.get_Item(16),
//					volume.get_Item(17),
//					volume.get_Item(18),
//					volume.get_Item(19)
//				}));
//				this.Condition2 = (volume.get_Item(0) == this.NthMinList(1, new double[]
//				{
//					volume.get_Item(0),
//					volume.get_Item(1),
//					volume.get_Item(2),
//					volume.get_Item(3),
//					volume.get_Item(4),
//					volume.get_Item(5),
//					volume.get_Item(6),
//					volume.get_Item(7),
//					volume.get_Item(8),
//					volume.get_Item(9),
//					volume.get_Item(10),
//					volume.get_Item(11),
//					volume.get_Item(12),
//					volume.get_Item(13),
//					volume.get_Item(14),
//					volume.get_Item(15),
//					volume.get_Item(16),
//					volume.get_Item(17),
//					volume.get_Item(18),
//					volume.get_Item(19)
//				}));
				if (RAMBO && (Condition3 || Condition4))
				{
					if (Low[0] == MIN(Low, 20)[0])
					{
						num += num3;
//						base.DrawText("RBull" + base.get_CurrentBar(), true, "R", 0, base.get_Low().get_Item(0) - num, 0, this.BullishColor, this.MyFont, StringAlignment.Center, Color.Transparent, this.BullishColor, this.op);
						Draw.Text(this, "RBull" + CurrentBar, "R", 0, Low[0] - num, BullishColor);
						ramboSeries[0] = (1);
						if (RAMBOAlert)
						{
							//base.Alert("RAMBO bull alert", 1, "RAMBO bull alert", this.SoundEnabled ? this.Sound : "", 10, Color.White, this.BullishColor);
							TC_Alert("RAMBO Bull Alert", Sound);
						}
					}
					if (High[0] == MAX(High, 20)[0])
					{
						num2 += num3;
						//base.DrawText("RBear" + base.get_CurrentBar(), true, "R", 0, base.get_High().get_Item(0) + num2, 0, this.BearishColor, this.MyFont, StringAlignment.Center, Color.Transparent, this.BearishColor, this.op);
						Draw.Text(this, "RBear" + CurrentBar, "R", 0, High[0] + num2, BearishColor);
						ramboSeries[0] = (-1);
						if (RAMBOAlert)
						{
							//base.Alert("RAMBO bear alert", 1, "RAMBO bear alert", this.SoundEnabled ? this.Sound : "", 10, Color.Black, this.BearishColor);
							TC_Alert("RAMBO Bear Alert", Sound);
						}
					}
				}
				Condition3 = (Close[0] >= Low[0] + 0.4 * Range()[0] && Low[0] < Low[1] && Low[0] < Low[2] && volume[0] < volume[1] && volume[0]  < volume[2]  && (Range()[0]  < Range()[1]  || Range()[0]  < Range()[2] ) && Trend == 1);
				Condition4 = (Close[0] <= High[0] - 0.4 * Range()[0] && High[0] > High[1] && High[0] > High[2] && volume[0] < volume[1] && volume[0] < volume[2] && (Range()[0]< Range()[1] || Range()[0] < Range()[2]) && Trend == -1);
				if (NoD)
				{
					if (Condition3)
					{
						num += num3;
						//base.DrawText("NoS" + base.get_CurrentBar(), true, "NoS", 0, base.get_Low().get_Item(0) - num, 0, this.BullishColor, this.MyFont, StringAlignment.Center, Color.Transparent, this.BullishColor, this.op);
						Draw.Text(this, "NoS" + CurrentBar, "NoS", 0, Low[0] - num, BullishColor);
						noDSeries[0] = (1);
						if (NoDAlert)
						{
							//base.Alert("NoSupply alert", 1, "NoSupply alert", this.SoundEnabled ? this.Sound : "", 10, Color.White, this.BullishColor);
							TC_Alert("No Supply Alert", Sound);
						}
					}
					if (Condition4)
					{
						num2 += num3;
						//base.DrawText("NoD" + base.get_CurrentBar(), true, "NoD", 0, base.get_High().get_Item(0) + num2, 0, this.BearishColor, this.MyFont, StringAlignment.Center, Color.Transparent, this.BearishColor, this.op);
						Draw.Text(this, "NoD" + CurrentBar, "NoD", 0, High[0] + num2, BearishColor);
						noDSeries[0] = (-1);
						if (NoDAlert)
						{
							//base.Alert("NoDemand alert", 1, "NoDemand alert", this.SoundEnabled ? this.Sound : "", 10, Color.Black, this.BearishColor);
							TC_Alert("No Demand Alert", Sound);
						}
					}
				}
				Condition5 = (Close[0] >= Low[0] + 0.4 * Range()[0] && Close[0] <= Low[0] + 0.6 * Range()[0] && Close[0] < Close[1] && Low[0] < Low[1] && volume[0] > volume[1] && Trend == 1);
				Condition6 = (Close[0] >= Low[0] + 0.4 * Range()[0] && Close[0] <= Low[0] + 0.6 * Range()[0] && Close[0] > Close[1] && High[0] > High[1] && volume[0] > volume[1] && Trend == -1);
				if (ProfitTake)
				{
					if (Condition5)
					{
						num += num3;
						//base.DrawText("PTBull" + base.get_CurrentBar(), true, "PT", 0, base.get_Low().get_Item(0) - num, 0, this.BullishColor, this.MyFont, StringAlignment.Center, Color.Transparent, this.BullishColor, this.op);
						Draw.Text(this, "PTBull" + CurrentBar, "PT", 0, Low[0] - num, BullishColor);
						profitTakeSeries[0] = (1);
						if (ProfitTakeAlert)
						{
							//base.Alert("Profit Take bull alert", 1, "Profit Take bull alert", this.SoundEnabled ? this.Sound : "", 10, Color.White, this.BullishColor);
							TC_Alert("Profit Take bull alert", Sound);
						}
					}
					if (Condition6)
					{
						num2 += num3;
						//base.DrawText("PTBear" + base.get_CurrentBar(), true, "PT", 0, base.get_High().get_Item(0) + num2, 0, this.BearishColor, this.MyFont, StringAlignment.Center, Color.Transparent, this.BearishColor, this.op);
						Draw.Text(this, "PTBear" + CurrentBar, "PT", 0, High[0] + num2, BearishColor);
						this.profitTakeSeries[0] = (-1);
						if (ProfitTakeAlert)
						{
							//base.Alert("Profit Take bear alert", 1, "Profit Take bear alert", this.SoundEnabled ? this.Sound : "", 10, Color.Black, this.BearishColor);
							TC_Alert("Profit Take bear alert", Sound);
						}
					}
				}
				Condition7 = (Range()[0] < Range()[1] && volume[0] > volume[1] && High[0] < Low[1]);
				Condition8 = (Range()[0] < Range()[1] && volume[0] > volume[1] && Low[0] > High[1]);
				if (StoppingVol)
				{
					if (Condition7)
					{
						num += num3;
						//base.DrawText("St*Bull" + base.get_CurrentBar(), true, "St*", 0, base.get_Low().get_Item(0) - num, 0, this.BullishColor, this.MyFont, StringAlignment.Center, Color.Transparent, this.BullishColor, this.op);
						Draw.Text(this, "St*Bull" + CurrentBar, "St", 0, Low[0] - num, BullishColor);
						stoppingVolSeries[0] = (10);
						if (StoppingVolAlert)
						{
							//base.Alert("Stopping Volume bull alert", 1, "Stopping Volume bull alert", this.SoundEnabled ? this.Sound : "", 10, Color.White, this.BullishColor);
							TC_Alert("Stopping Volume bull alert", Sound);
						}
					}
					if (Condition8)
					{
						num2 += num3;
						//base.DrawText("St*Bear" + base.get_CurrentBar(), true, "St*", 0, base.get_High().get_Item(0) + num2, 0, this.BearishColor, this.MyFont, StringAlignment.Center, Color.Transparent, this.BearishColor, this.op);
						Draw.Text(this, "St*Bear" + CurrentBar, "St", 0, High[0] + num2, BearishColor);
						stoppingVolSeries[0] = (-10);
						if (StoppingVolAlert)
						{
							//base.Alert("Stopping Volume bear alert", 1, "Stopping Volume bear alert", this.SoundEnabled ? this.Sound : "", 10, Color.Black, this.BearishColor);
							TC_Alert("Stopping Volume bear alert", Sound);
						}
					}
				}
				Condition9 = (Range()[0] < Range()[1] && Range()[0] < Range()[2] && volume[0] > volume[1] && volume[0] > volume[2] && Low[0] < Low[1] && Trend == 1);
				Condition10 = (Range()[0] < Range()[1] && Range()[0] < Range()[2] && volume[0] > volume[1] && volume[0] > volume[2] && High[0] > High[1] && Trend == -1);
				if (StoppingVol)
				{
					if (Condition9 && !Condition7)
					{
						num += num3;
						//base.DrawText("StBull" + base.get_CurrentBar(), true, "St", 0, base.get_Low().get_Item(0) - num, 0, this.BullishColor, this.MyFont, StringAlignment.Center, Color.Transparent, this.BullishColor, this.op);
						Draw.Text(this, "StBull" + CurrentBar, "St", 0, Low[0] - num, BullishColor);
						stoppingVolSeries[0] = (1);
						if (StoppingVolAlert)
						{
							//base.Alert("Stopping Volume bull alert", 1, "Stopping Volume bull alert", this.SoundEnabled ? this.Sound : "", 10, Color.White, this.BullishColor);
							TC_Alert("Stopping Volume bear alert", Sound);
						}
					}
					if (Condition10 && !Condition8)
					{
						num2 += num3;
						//base.DrawText("StBear" + base.get_CurrentBar(), true, "St", 0, base.get_High().get_Item(0) + num2, 0, this.BearishColor, this.MyFont, StringAlignment.Center, Color.Transparent, this.BearishColor, this.op);
						Draw.Text(this, "StBear" + CurrentBar, "St", 0, High[0] + num2, BearishColor);
						stoppingVolSeries[0] = (-1);
						if (StoppingVolAlert)
						{
							//base.Alert("Stopping Volume bear alert", 1, "Stopping Volume bear alert", this.SoundEnabled ? this.Sound : "", 10, Color.Black, this.BearishColor);
							TC_Alert("Stopping Volume bear alert", Sound);
						}
					}
				}
			}
		}
		
		private void TC_Alert(string message, string sound)
		{
			Alert("myAlert", Priority.High, Name + ": " + message, sound, 15, Brushes.WhiteSmoke, Brushes.Black); 
		}

		#region Properties
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Bullish Color", Order=1, GroupName="Parameters")]
		public Brush BullishColor
		{ get; set; }

		[Browsable(false)]
		public string BullishColorSerializable
		{
			get { return Serialize.BrushToString(BullishColor); }
			set { BullishColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Bearish Color", Order=2, GroupName="Parameters")]
		public Brush BearishColor
		{ get; set; }

		[Browsable(false)]
		public string BearishColorSerializable
		{
			get { return Serialize.BrushToString(BearishColor); }
			set { BearishColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Display(Name="ProAm", Order=3, GroupName="Parameters")]
		public bool ProAm
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="RAMBO", Order=4, GroupName="Parameters")]
		public bool RAMBO
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="NoD", Order=5, GroupName="Parameters")]
		public bool NoD
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Profit Take", Order=6, GroupName="Parameters")]
		public bool ProfitTake
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Stopping Vollume", Order=7, GroupName="Parameters")]
		public bool StoppingVol
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="RAMBO Alert", Order=8, GroupName="Parameters")]
		public bool RAMBOAlert
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="NoD Alert", Order=9, GroupName="Parameters")]
		public bool NoDAlert
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Profit Take Alert", Order=10, GroupName="Parameters")]
		public bool ProfitTakeAlert
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Stopping Vol Alert", Order=11, GroupName="Parameters")]
		public bool StoppingVolAlert
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Am Alert", Order=12, GroupName="Parameters")]
		public bool AmAlert
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Pro Alert", Order=13, GroupName="Parameters")]
		public bool ProAlert
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="TextSpaceMulti", Order=14, GroupName="Parameters")]
		public double TextSpaceMulti
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Sound", Order=15, GroupName="Parameters")]
		public string Sound
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Sound Enabled", Order=16, GroupName="Parameters")]
		public bool SoundEnabled
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Pro Color", Order=17, GroupName="Parameters")]
		public Brush ProColor
		{ get; set; }

		[Browsable(false)]
		public string ProColorSerializable
		{
			get { return Serialize.BrushToString(ProColor); }
			set { ProColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Am Color", Order=18, GroupName="Parameters")]
		public Brush AmColor
		{ get; set; }

		[Browsable(false)]
		public string AmColorSerializable
		{
			get { return Serialize.BrushToString(AmColor); }
			set { AmColor = Serialize.StringToBrush(value); }
		}			
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BetterProAmPlot[] cacheBetterProAmPlot;
		public BetterProAmPlot BetterProAmPlot(Brush bullishColor, Brush bearishColor, bool proAm, bool rAMBO, bool noD, bool profitTake, bool stoppingVol, bool rAMBOAlert, bool noDAlert, bool profitTakeAlert, bool stoppingVolAlert, bool amAlert, bool proAlert, double textSpaceMulti, string sound, bool soundEnabled, Brush proColor, Brush amColor)
		{
			return BetterProAmPlot(Input, bullishColor, bearishColor, proAm, rAMBO, noD, profitTake, stoppingVol, rAMBOAlert, noDAlert, profitTakeAlert, stoppingVolAlert, amAlert, proAlert, textSpaceMulti, sound, soundEnabled, proColor, amColor);
		}

		public BetterProAmPlot BetterProAmPlot(ISeries<double> input, Brush bullishColor, Brush bearishColor, bool proAm, bool rAMBO, bool noD, bool profitTake, bool stoppingVol, bool rAMBOAlert, bool noDAlert, bool profitTakeAlert, bool stoppingVolAlert, bool amAlert, bool proAlert, double textSpaceMulti, string sound, bool soundEnabled, Brush proColor, Brush amColor)
		{
			if (cacheBetterProAmPlot != null)
				for (int idx = 0; idx < cacheBetterProAmPlot.Length; idx++)
					if (cacheBetterProAmPlot[idx] != null && cacheBetterProAmPlot[idx].BullishColor == bullishColor && cacheBetterProAmPlot[idx].BearishColor == bearishColor && cacheBetterProAmPlot[idx].ProAm == proAm && cacheBetterProAmPlot[idx].RAMBO == rAMBO && cacheBetterProAmPlot[idx].NoD == noD && cacheBetterProAmPlot[idx].ProfitTake == profitTake && cacheBetterProAmPlot[idx].StoppingVol == stoppingVol && cacheBetterProAmPlot[idx].RAMBOAlert == rAMBOAlert && cacheBetterProAmPlot[idx].NoDAlert == noDAlert && cacheBetterProAmPlot[idx].ProfitTakeAlert == profitTakeAlert && cacheBetterProAmPlot[idx].StoppingVolAlert == stoppingVolAlert && cacheBetterProAmPlot[idx].AmAlert == amAlert && cacheBetterProAmPlot[idx].ProAlert == proAlert && cacheBetterProAmPlot[idx].TextSpaceMulti == textSpaceMulti && cacheBetterProAmPlot[idx].Sound == sound && cacheBetterProAmPlot[idx].SoundEnabled == soundEnabled && cacheBetterProAmPlot[idx].ProColor == proColor && cacheBetterProAmPlot[idx].AmColor == amColor && cacheBetterProAmPlot[idx].EqualsInput(input))
						return cacheBetterProAmPlot[idx];
			return CacheIndicator<BetterProAmPlot>(new BetterProAmPlot(){ BullishColor = bullishColor, BearishColor = bearishColor, ProAm = proAm, RAMBO = rAMBO, NoD = noD, ProfitTake = profitTake, StoppingVol = stoppingVol, RAMBOAlert = rAMBOAlert, NoDAlert = noDAlert, ProfitTakeAlert = profitTakeAlert, StoppingVolAlert = stoppingVolAlert, AmAlert = amAlert, ProAlert = proAlert, TextSpaceMulti = textSpaceMulti, Sound = sound, SoundEnabled = soundEnabled, ProColor = proColor, AmColor = amColor }, input, ref cacheBetterProAmPlot);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BetterProAmPlot BetterProAmPlot(Brush bullishColor, Brush bearishColor, bool proAm, bool rAMBO, bool noD, bool profitTake, bool stoppingVol, bool rAMBOAlert, bool noDAlert, bool profitTakeAlert, bool stoppingVolAlert, bool amAlert, bool proAlert, double textSpaceMulti, string sound, bool soundEnabled, Brush proColor, Brush amColor)
		{
			return indicator.BetterProAmPlot(Input, bullishColor, bearishColor, proAm, rAMBO, noD, profitTake, stoppingVol, rAMBOAlert, noDAlert, profitTakeAlert, stoppingVolAlert, amAlert, proAlert, textSpaceMulti, sound, soundEnabled, proColor, amColor);
		}

		public Indicators.BetterProAmPlot BetterProAmPlot(ISeries<double> input , Brush bullishColor, Brush bearishColor, bool proAm, bool rAMBO, bool noD, bool profitTake, bool stoppingVol, bool rAMBOAlert, bool noDAlert, bool profitTakeAlert, bool stoppingVolAlert, bool amAlert, bool proAlert, double textSpaceMulti, string sound, bool soundEnabled, Brush proColor, Brush amColor)
		{
			return indicator.BetterProAmPlot(input, bullishColor, bearishColor, proAm, rAMBO, noD, profitTake, stoppingVol, rAMBOAlert, noDAlert, profitTakeAlert, stoppingVolAlert, amAlert, proAlert, textSpaceMulti, sound, soundEnabled, proColor, amColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BetterProAmPlot BetterProAmPlot(Brush bullishColor, Brush bearishColor, bool proAm, bool rAMBO, bool noD, bool profitTake, bool stoppingVol, bool rAMBOAlert, bool noDAlert, bool profitTakeAlert, bool stoppingVolAlert, bool amAlert, bool proAlert, double textSpaceMulti, string sound, bool soundEnabled, Brush proColor, Brush amColor)
		{
			return indicator.BetterProAmPlot(Input, bullishColor, bearishColor, proAm, rAMBO, noD, profitTake, stoppingVol, rAMBOAlert, noDAlert, profitTakeAlert, stoppingVolAlert, amAlert, proAlert, textSpaceMulti, sound, soundEnabled, proColor, amColor);
		}

		public Indicators.BetterProAmPlot BetterProAmPlot(ISeries<double> input , Brush bullishColor, Brush bearishColor, bool proAm, bool rAMBO, bool noD, bool profitTake, bool stoppingVol, bool rAMBOAlert, bool noDAlert, bool profitTakeAlert, bool stoppingVolAlert, bool amAlert, bool proAlert, double textSpaceMulti, string sound, bool soundEnabled, Brush proColor, Brush amColor)
		{
			return indicator.BetterProAmPlot(input, bullishColor, bearishColor, proAm, rAMBO, noD, profitTake, stoppingVol, rAMBOAlert, noDAlert, profitTakeAlert, stoppingVolAlert, amAlert, proAlert, textSpaceMulti, sound, soundEnabled, proColor, amColor);
		}
	}
}

#endregion
