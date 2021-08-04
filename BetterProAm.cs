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
	public class BetterProAm : Indicator
	{
		private Series<double> BarVolume;
		
		//public double	Var1 = 0;
		public int 		Var2 = 20;
		public double 	Var3 = 65280; 
		public bool 	MaxVolume = false;
		public bool 	MaxVolumeCurrentBar = false; 
		public bool 	MinVolume = false; 
		public bool 	HistoricalMin = false;
		public int 		downtick = 0;
		public bool 	proColor = false;
		public bool 	amColor = false;
		
		private Brush	upBrush			= Brushes.DodgerBlue;
		private Brush	downBrush		= Brushes.Yellow;
		private Brush	regularBrush	= Brushes.DarkGreen;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "BetterProAm";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				ProAlert					= true;
				AMAlert					= true;
			}
			else if (State == State.Configure)
			{
			}
			else if(State == State.DataLoaded)
			  {
			    //clear the output window as soon as the bars data is loaded
			    ClearOutputWindow();  	
				BarVolume = new Series<double>(this, MaximumBarsLookBack.Infinite);

			  }
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 20) {
				return;
			}

			if(BarsPeriod.BarsPeriodType == BarsPeriodType.Minute || BarsPeriod.BarsPeriodType == BarsPeriodType.Tick ) {
				if (Close[0] > Close[1])
					downtick++;
				BarVolume[0] = Volume[0] + downtick;
			} else {
				BarVolume[0] = Volume[0];
			}
			// Draw.Text(this, "vol"+CurrentBar, BarVolume[0].ToString(), 0, Low[0] - (TickSize * 2), Brushes.AntiqueWhite);
//			Draw.Text(this, "Volume[0]"+CurrentBar, Volume[0].ToString(), 0, High[0] + (TickSize * 2), Brushes.AntiqueWhite);
			//if Highest (Var1, Var2) <> 0 then
			//Var4 = Var1 = NthMaxList (1, Var1, Var1[1], Var1[2], Var1[3], Var1[4], Var1[5], Var1[6], Var1[7], Var1[8], Var1[9], Var1[10], Var1[11], Var1[12], Var1[13], Var1[14], Var1[15], Var1[16], Var1[17], Var1[18], Var1[19]) ;
			if( MAX(BarVolume, Var2)[0] != 0 ) {
				
				if ( BarVolume[0] == MAX(BarVolume, 19)[0] ) {
					MaxVolume = true;
				} else {
					MaxVolume = false;
				}
			
				//Print(Var4);
				//Var5 = Var1 = NthMaxList (2, Var1, Var1[1], Var1[2], Var1[3], Var1[4], Var1[5], Var1[6], Var1[7], Var1[8], Var1[9], Var1[10], Var1[11], Var1[12], Var1[13], Var1[14], Var1[15], Var1[16], Var1[17], Var1[18], Var1[19]) ;
				List<double> ListOfBarVolume = new List<double> {BarVolume[0], BarVolume[1], BarVolume[2], BarVolume[3], BarVolume[4],
                    BarVolume[5], BarVolume[6], BarVolume[7], BarVolume[8], BarVolume[9], BarVolume[10], BarVolume[11],
                    BarVolume[12], BarVolume[13], BarVolume[14], BarVolume[15], BarVolume[16], BarVolume[17], BarVolume[18],
                    BarVolume[19]};
				var VolumeOredrList = ListOfBarVolume.OrderByDescending(r => r).Skip(1).FirstOrDefault();
		
				if ( BarVolume[0] == VolumeOredrList ) {
						MaxVolumeCurrentBar = true;
					} else {
						MaxVolumeCurrentBar = false;
					}
				//Print(secondMax); Print(Var5);	
				//Var6 = Var1 = NthMinList (1, Var1, Var1[1], Var1[2], Var1[3], Var1[4], Var1[5], Var1[6], Var1[7], Var1[8], Var1[9], Var1[10], Var1[11], Var1[12], Var1[13], Var1[14], Var1[15], Var1[16], Var1[17], Var1[18], Var1[19]) ;
				if ( BarVolume[0] == MIN(BarVolume, 19)[0] ) {
					MinVolume = true;
				} else {
					MinVolume = false;
				}
	  			// Var7 = Var1 = NthMinList (2, Var1, Var1[1], Var1[2], Var1[3], Var1[4], Var1[5], Var1[6], Var1[7], Var1[8], Var1[9], Var1[10], Var1[11], Var1[12], Var1[13], Var1[14], Var1[15], Var1[16], Var1[17], Var1[18], Var1[19]) ;
				var VolumeMinList = ListOfBarVolume.OrderByDescending(r => r).Skip(1).LastOrDefault();
				if ( BarVolume[0] == VolumeMinList ) {
						HistoricalMin = true;
					} else {
						HistoricalMin = false;
					}
				//if Var4 OR Var5 then Var3 = ProColor ;
					if (  MaxVolume || MaxVolumeCurrentBar ) {
						proColor = true;
					} else {
						proColor = false;
					}
   				//if Var6 OR Var7 then Var3 = AmColor ;
					if (  MinVolume || HistoricalMin ) {
						amColor = true;
					} else {
						amColor = false;
					}
					
				BarBrush = regularBrush;
				CandleOutlineBrush = regularBrush;
					
				if ( proColor ) {
					BarBrush = upBrush; //  Brushes.Blue;
					CandleOutlineBrush =upBrush; // Brushes.Blue;	
					// BarBrushes[-displacement] = upBrushUp;
				}
				if ( amColor ) {
					BarBrush = downBrush; // Brushes.Gold;
					CandleOutlineBrush = downBrush; // Brushes.Gold;	
				}
			}
		}
		


		#region Properties
		[NinjaScriptProperty]
		[Display(Name="ProAlert", Order=1, GroupName="Parameters")]
		public bool ProAlert
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="AMAlert", Order=2, GroupName="Parameters")]
		public bool AMAlert
		{ get; set; }
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Pro color", Description = "Sets the pro color for the bars", GroupName = "Plots", Order = 1)]
		public Brush UpBrush
		{ 
			get {return upBrush;}
			set {upBrush = value;}
		}

		[Browsable(false)]
		public string UpBrushSerializable
		{
			get { return Serialize.BrushToString(upBrush); }
			set { upBrush = Serialize.StringToBrush(value); }
		}					
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Amerature color", Description = "Sets the amerature color for the bars", GroupName = "Plots", Order = 2)]
		public Brush DownBrush
		{ 
			get {return downBrush;}
			set {downBrush = value;}
		}
		
		[Browsable(false)]
		public string DownBrushSerializable
		{
			get { return Serialize.BrushToString(downBrush); }
			set { downBrush = Serialize.StringToBrush(value); }
		}	
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Normal color", Description = "Sets the normal color for the bars", GroupName = "Plots", Order = 3)]
		public Brush RegularBrush
		{ 
			get {return regularBrush;}
			set {regularBrush = value;}
		}

		[Browsable(false)]
		public string RegularBrushSerializable
		{
			get { return Serialize.BrushToString(regularBrush); }
			set { regularBrush = Serialize.StringToBrush(value); }
		}
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BetterProAm[] cacheBetterProAm;
		public BetterProAm BetterProAm(bool proAlert, bool aMAlert)
		{
			return BetterProAm(Input, proAlert, aMAlert);
		}

		public BetterProAm BetterProAm(ISeries<double> input, bool proAlert, bool aMAlert)
		{
			if (cacheBetterProAm != null)
				for (int idx = 0; idx < cacheBetterProAm.Length; idx++)
					if (cacheBetterProAm[idx] != null && cacheBetterProAm[idx].ProAlert == proAlert && cacheBetterProAm[idx].AMAlert == aMAlert && cacheBetterProAm[idx].EqualsInput(input))
						return cacheBetterProAm[idx];
			return CacheIndicator<BetterProAm>(new BetterProAm(){ ProAlert = proAlert, AMAlert = aMAlert }, input, ref cacheBetterProAm);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BetterProAm BetterProAm(bool proAlert, bool aMAlert)
		{
			return indicator.BetterProAm(Input, proAlert, aMAlert);
		}

		public Indicators.BetterProAm BetterProAm(ISeries<double> input , bool proAlert, bool aMAlert)
		{
			return indicator.BetterProAm(input, proAlert, aMAlert);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BetterProAm BetterProAm(bool proAlert, bool aMAlert)
		{
			return indicator.BetterProAm(Input, proAlert, aMAlert);
		}

		public Indicators.BetterProAm BetterProAm(ISeries<double> input , bool proAlert, bool aMAlert)
		{
			return indicator.BetterProAm(input, proAlert, aMAlert);
		}
	}
}

#endregion
