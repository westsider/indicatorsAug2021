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

using SharpDX.Direct2D1;
using SharpDX;
using SharpDX.DirectWrite;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class PriceRejectorCurrent : Indicator
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
		
		struct RejectorSignal {
			public int  direction;
			public double low;
		    public double  high;
		   
			public RejectorSignal(int direction, double low, double high)
			{
				this.direction = direction;
		        this.low = low;
				this.high = high;
			}
		}
		
		private List<BarSlice> FullBar = new List<BarSlice>();
		private List<double> MaxDelta = new List<double>();
		private List<double> MinDelta = new List<double>();
		private List<int> PriceRejector = new List<int>();
		private List<RejectorSignal> RejectorSignals = new List<RejectorSignal>();
		private bool GoodToTrade = true;
		private Bollinger Bollinger1;	
		
		// cureent bar draw objects
		private double lastPRhigh = 0;
		private double lastABhigh = 0;
		private double lastUBhigh = 0;
		private double lastPRLow = 0;
		private double lastABLow = 0;
		private double lastUBLow = 0;
		private double currentHigh = 0.0;
		private double currentLow = 0.0;
		
		// price rejectors
		private double volumeAtExtremeHigh = 0.0;
		private double volumeAtExtremeLow = 0.0;
		private bool LowTriggered  =false;
		private bool HighTriggered  =false;
		private double LowTriggerVolume = 0.0;
		private double HighTriggerVolume = 0.0;
		private bool LowEntryTriggered  =false;
		private bool HighEntryTriggered  =false;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name												= "Price Rejector Current";
				Calculate										= Calculate.OnBarClose;
				IsOverlay										= true;
				DisplayInDataBox						= true;
				DrawOnPricePanel						= true;
				DrawHorizontalGridLines			= true;
				DrawVerticalGridLines				= true;
				PaintPriceMarkers						= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive			= true;
				ShowPriceRejector						= true;
				ShowMaxDelta								= true; // max delta
				ShowExhaustion							= true; // filter the edges
				ShowUB											= true; 
				Minutes											= 30; 
				Opacity											= 0.2;
				AString												= "rejector.wav";
				ShowRejectorEntry 						= true;
				EntryWavString								=  "Entry.wav";
				EntryString										= "****";
				TriggerString									= "^^^";
			}
			else if (State == State.Configure)
			{
				ClearOutputWindow();
				AddVolumetric("ES 06-21", BarsPeriodType.Minute, Minutes, VolumetricDeltaType.BidAsk, 1);
			}
			else if (State == State.DataLoaded)
			{				
				Bollinger1				= Bollinger(Close, 1.5, 14);
			}
		}
		
		protected override void OnBarUpdate()
		{
			if(State == State.Historical) return;
			if (BarsInProgress == 1 ) {
				checkNewHighLow();
				PopulateList(debug: false) ;
				CleanArray() ;	
				if ( IsFirstTickOfBar) {
					volumeAtExtremeHigh = 0.0;
				}
			}
		}
		
		private void checkNewHighLow() {
			// remove all high - low text
				if ( High[0] > currentHigh) { 
					RemoveDrawObject("Exhastionh"+CurrentBar); 
					RemoveDrawObject("RejectorHigh"+lastPRhigh);
					HighTriggered = false;
					RemoveDrawObject("PRHighTrigger");
					RemoveDrawObject("PRHighEntry");
				    HighTriggerVolume = 0.0;
					HighEntryTriggered  =false; 
					ClearOutputWindow();
				}
				
				if ( Low[0] < currentLow ) { 
					RemoveDrawObject("Exhastion"+CurrentBar); 
					RemoveDrawObject("RejectorLow"+lastPRLow);
					LowTriggered = false;
					LowTriggerVolume = 0.0;
					RemoveDrawObject("PRLowEntry");
					 RemoveDrawObject("PRLowTrigger");
					LowEntryTriggered  =false;
					ClearOutputWindow();
				}
		}
		
		private void CleanArray() {
				int minC = MinDelta.Count;
				int maxC = MaxDelta.Count;
				if (minC > maxC) {
					MinDelta.RemoveAt(minC -1);
				}
				if (maxC > minC) {
					MaxDelta.RemoveAt(maxC -1);
				}
				currentHigh = High[0];
				currentLow = Low[0];
		}
		
		private void SetEdgeFilter() {
			if ( CurrentBar < 1 ) { 
						MinDelta.Add(0.0);
						MaxDelta.Add(0.0);
						PriceRejector.Add(0);
						RejectorSignal rejectorSignal = new RejectorSignal(0, Low[0], High[0]);
						RejectorSignals.Add(rejectorSignal);
						return; }
			    if ( ShowExhaustion )  {
				    if (High[0] > Bollinger1.Upper[0] || Low[0] <= Bollinger1.Lower[0])
					{
						GoodToTrade = true;
					} else {
						GoodToTrade = false;
					}
				}
		}
		
		private void PopulateList(bool debug) {
			NinjaTrader.NinjaScript.BarsTypes.VolumetricBarsType barsType = Bars.BarsSeries.BarsType as    
	        NinjaTrader.NinjaScript.BarsTypes.VolumetricBarsType;
			FullBar.Clear();
			double ticks = TickSize;
			double NumPrices = Range()[0] / ticks;
			double maxDeltaPrice = 0.0;
			double minDeltaPrice = 0.0;
			
			try
	        {
				// loop thru bar prices and fill up Struct
				if ( debug ) { Print("Bar: " + CurrentBar + "\t" + Time[0] + " \trange: " + NumPrices); }
				if ( debug ) { Print("fill array --------------------------------->");}
				for (int index = 0; index <= NumPrices; index++)
				{
				//	if ( debug ) { Print("bad loop? " + index );}
					double thisPrice = High[0] - (index * TickSize);
					//if ( debug ) { Print("bad loop? " + thisPrice );}
					double thisBid =  barsType.Volumes[CurrentBar].GetBidVolumeForPrice(thisPrice);
					double thisAsk =  barsType.Volumes[CurrentBar].GetAskVolumeForPrice(thisPrice);
					double thisDelta =  barsType.Volumes[CurrentBar].GetDeltaForPrice(thisPrice);
					BarSlice slice = new BarSlice(thisPrice, thisBid, thisAsk, thisDelta, 0.0, 0.0);
					FullBar.Add(slice);
					if ( debug ) { Print(  slice.priceLevel + " \tbid: " + slice.bidVolume + " \task: " + slice.askVolume + "\t delta: " + slice.delta);}
				}
				if ( debug ) { Print("exit array --------------------------------->");}
				int indexOfLast = FullBar.Count - 1;
				if ( indexOfLast == 0 ) { return;} 
				var maxDelta = FullBar.Max(d => d.delta);
				var minDelta = FullBar.Min(d => d.delta);
				if ( debug ) { Print("Min Delta: " + minDelta + " max: "  + maxDelta);}

				if ( ShowMaxDelta )
				foreach (var item in FullBar) {
					if (item.delta == maxDelta) {
						maxDeltaPrice = item.priceLevel;
						Draw.Text(this, "maxDeltaPrice", "Max", 0, maxDeltaPrice, Brushes.LightCyan);
						MaxDelta.Add(maxDeltaPrice);
					} 
					
					if (item.delta == minDelta) {
						minDeltaPrice = item.priceLevel;
						Draw.Text(this, "minDeltaPrice", "Max", 0, minDeltaPrice, Brushes.LightPink);
						MinDelta.Add(minDeltaPrice);
					} 
				}

				/// Check for unfinished business
				if ( debug ) { Print("first bid: " + FullBar[0].bidVolume  + " \tlast offer: " + FullBar[indexOfLast ].askVolume );}
				PriceRejector.Add(0);	
				RejectorSignal rejectorSignal = new RejectorSignal(0, Low[0], High[0]);
				RejectorSignals.Add(rejectorSignal);
				
				//RejectorSignals.Add(new RejectorSignal(0, 0.0, 0.0));
				if (FullBar[indexOfLast].askVolume != 0.0 ) { 
					RemoveDrawObject("RejectorLow"+lastPRLow);
					RemoveDrawObject("Exhastion"+CurrentBar);
					RemoveDrawObject("PRLowEntry");
					 RemoveDrawObject("PRLowTrigger");
					Draw.Text(this, "Exhastion"+CurrentBar, "UB", 0, Low[0] - 1 * TickSize, Brushes.Yellow);
					LowTriggered = false;
					// if vol lower then exhaustion
					if ( debug ) { Print("Comparing last ask " + FullBar[indexOfLast].askVolume + " to ask above" + FullBar[indexOfLast -1].askVolume );}
				} else {
					/// Check for Price rejector in completed auction
					// Volume gets lower in bar
					if (FullBar[indexOfLast].bidVolume  < FullBar[indexOfLast -1].bidVolume && FullBar[indexOfLast -1].bidVolume < FullBar[indexOfLast -2].bidVolume && FullBar[indexOfLast -2].bidVolume < FullBar[indexOfLast -3].bidVolume && 
						FullBar[indexOfLast].askVolume < FullBar[indexOfLast -1].askVolume &&  FullBar[indexOfLast -1].askVolume < FullBar[indexOfLast -2].askVolume && FullBar[indexOfLast -2].askVolume < FullBar[indexOfLast -3].askVolume) {
						// delta negative at 3 lowest levels
						if (FullBar[indexOfLast].delta <=0 && FullBar[indexOfLast - 1].delta <=0 && FullBar[indexOfLast - 2].delta <=0  ) { 
							// diagonal agression at lowest level
						//	/ low bid < low + 1 offer
							
	/// *******************************************************************************************************************
	/// ********************************************     REJECTOR LOW      *******************************************
	/// *******************************************************************************************************************
			
							if (FullBar[indexOfLast].bidVolume  < FullBar[indexOfLast - 1].askVolume )  { 
								if ( ShowPriceRejector && !LowTriggered ) { 
									RemoveDrawObject("Exhastion"+CurrentBar);
									RemoveDrawObject("RejectorLow"+lastPRLow);
									volumeAtExtremeLow = FullBar[indexOfLast].delta + FullBar[indexOfLast - 1].delta + FullBar[indexOfLast - 2].delta;
									string message = "Price Rejector " + volumeAtExtremeLow.ToString("N0");
									Draw.Text(this, "RejectorLow"+Volume[0], message, 0, Low[0] - 2 * TickSize, Brushes.DimGray); 
									PriceRejector[PriceRejector.Count - 1] = 1;
									lastPRLow = Volume[0];
									if ( !LowTriggered)
										sendAlert( message: "Price Rejector at Low", sound: AString);
									LowTriggered = true; 
									RemoveDrawObject("PRLowEntry");
					 				RemoveDrawObject("PRLowTrigger");
									LowTriggerVolume = 0.0;
								}
							}
						}
					}
				}
				
				if (LowTriggered && !LowEntryTriggered && ShowRejectorEntry) { 
					double TriggerPrice = Low[0] + (TickSize * 3);
					double TriggerPrice1 = Low[0] + (TickSize * 4);
					double TriggerPrice2 = Low[0] + (TickSize * 5);
					double TriggerPrice3 = Low[0] + (TickSize * 6);
					
					Print("TP: " + TriggerPrice);
					if (Close[0] == TriggerPrice) { 
						RemoveDrawObject("PRLowEntry");
					 	RemoveDrawObject("PRLowTrigger");
						Draw.Text(this, "PRLowTrigger", TriggerString, 0, TriggerPrice, Brushes.Cyan);
					}
					if (Close[0] > TriggerPrice) {
						LowTriggerVolume =  FullBar[indexOfLast - 3].bidVolume;
					}
					if (Close[0] > TriggerPrice1) {
						LowTriggerVolume +=  FullBar[indexOfLast - 4].bidVolume;
					}
					if (Close[0] > TriggerPrice2) {
						LowTriggerVolume +=  FullBar[indexOfLast - 5].bidVolume;
					}
					if (Close[0] > TriggerPrice3) {
						LowTriggerVolume +=  FullBar[indexOfLast - 6].bidVolume;
					}
					double SellingToOvercome = (volumeAtExtremeLow * -1);
					Print("Low Trig vol: " + LowTriggerVolume + " vol at low: " + SellingToOvercome);
					if (Close[0] > TriggerPrice && LowTriggerVolume >= SellingToOvercome && !LowEntryTriggered ) { 
						Draw.Text(this, "PRLowEntry", EntryString, 0, Close[0], Brushes.Cyan);
						sendAlert( message: "Price Rejector Entry at Low", sound: EntryWavString);
						LowEntryTriggered  =true;
					}
				}

				if ( debug ) { Print("Test bar low for UB or Price Rejector at High" );}
				// Test bar low for UB or Price Rejector at High
				if (FullBar[0].bidVolume != 0.0 ) { 
					
					RemoveDrawObject("RejectorHigh"+lastPRhigh);
					RemoveDrawObject("Exhastionh"+CurrentBar);
					RemoveDrawObject("PRHighTrigger");
					RemoveDrawObject("PRHighEntry");
					Draw.Text(this, "Exhastionh"+ CurrentBar, "UB", 0, High[0] + 1 * TickSize, Brushes.Yellow);
					HighTriggered = false;
					// if vol lower then exhaustion
					if ( debug ) { Print("Comparing last ask " + FullBar[indexOfLast].askVolume + " to ask above" + FullBar[indexOfLast -1].askVolume );}
				} else {
					/// Check for Price rejector in completed auction
					// Volume gets lower in bar
					if (FullBar[0].bidVolume  < FullBar[1].bidVolume && FullBar[1].bidVolume < FullBar[2].bidVolume && FullBar[2].bidVolume < FullBar[3].bidVolume && 
						FullBar[0].askVolume < FullBar[1].askVolume &&  FullBar[1].askVolume < FullBar[2].askVolume && FullBar[2].askVolume < FullBar[3].askVolume) {
					//	// delta negative at 3 lowest levels
						if (FullBar[0].delta >=0 && FullBar[1].delta >=0 && FullBar[2].delta >=0  ) { 
							// diagonal agression at lowest level
							// low bid < low + 1 offer
	
							
	/// *******************************************************************************************************************
	/// ********************************************     REJECTOR HIGH      *******************************************
	/// *******************************************************************************************************************
							
							if (FullBar[1].bidVolume  > FullBar[0].askVolume )  { 
								if ( ShowPriceRejector && !HighTriggered ) { 
									RemoveDrawObject("Exhastionh"+CurrentBar);
									RemoveDrawObject("RejectorHigh"+lastPRhigh);
									volumeAtExtremeHigh = FullBar[0].delta + FullBar[1].delta + FullBar[2].delta;
									//Print("Vol: " + volumeAtExtremeHigh);
									string message = "Price Rejector " + volumeAtExtremeHigh.ToString("N0");
									Draw.Text(this, "RejectorHigh"+Volume[0], message, 0, High[0] + 2 * TickSize, Brushes.Red);
									PriceRejector[PriceRejector.Count - 1] = -1;
									lastPRhigh = Volume[0];
									if ( !HighTriggered)
										sendAlert( message: "Price Rejector at High", sound: AString);
									HighTriggered = true;
									RemoveDrawObject("PRHighTrigger");
									RemoveDrawObject("PRHighEntry");
									HighTriggerVolume = 0.0;
								}	
							}
						}
					}
				}
				
				if( HighTriggered && !HighEntryTriggered && ShowRejectorEntry) {
					double TriggerPrice = High[0] - (TickSize * 3);
					double TriggerPrice1 = High[0] - (TickSize * 4);
					double TriggerPrice2 = High[0] - (TickSize * 5);
					double TriggerPrice3 = High[0] - (TickSize * 6);
					
					Print("HTP: " + TriggerPrice);
					if (Close[0] == TriggerPrice) {
						RemoveDrawObject("PRHighTrigger");
									RemoveDrawObject("PRHighEntry");
						Draw.Text(this, "PRHighTrigger", "****", 0, TriggerPrice, Brushes.Magenta);
					}
					if (Close[0] <= TriggerPrice && FullBar.Count > 3) {
						HighTriggerVolume =  FullBar[ 3].askVolume;
					}
					if (Close[0] <= TriggerPrice1 && FullBar.Count > 4) {
						HighTriggerVolume +=  FullBar[ 4].askVolume;
					}
					if (Close[0] <= TriggerPrice2 && FullBar.Count > 5) {
						HighTriggerVolume +=  FullBar[ 5].askVolume;
					}
					if (Close[0] <= TriggerPrice3 && FullBar.Count > 6) {
						HighTriggerVolume +=  FullBar[6].askVolume;
					}
					Print("High Trig vol: " + HighTriggerVolume + " vol at high: " + volumeAtExtremeHigh);
					if (Close[0] < TriggerPrice && HighTriggerVolume >= volumeAtExtremeHigh && !HighEntryTriggered) {
						HighEntryTriggered  =true;
						
							Draw.Text(this, "PRHighEntry", "vvv", 0, Close[0], Brushes.Magenta);
						    sendAlert( message: "Price Rejector Entry at High", sound: EntryWavString);
					}
				}		
	        }
	        catch{}
			if ( debug ) { Print("end of func" );}
		}
		
		private void sendAlert(string message, string sound ) {
			Print("Alert: " + message);
			message += " " + Bars.Instrument.MasterInstrument.Name;
			Alert("myAlert"+CurrentBar, Priority.High, message, NinjaTrader.Core.Globals.InstallDir+@"\sounds\"+ sound,10, Brushes.Black, Brushes.Yellow);  
			if (CurrentBar < Count -2) return;
		}
		
		#region Properties
		[NinjaScriptProperty]
		[Display(Name="ShowPriceRejector", Order=1, GroupName="Parameters")]
		public bool ShowPriceRejector
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Show Max Delta", Order=2, GroupName="Parameters")]
		public bool ShowMaxDelta
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="filter the edges", Order=3, GroupName="Parameters")]
		public bool ShowExhaustion
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowUB", Order=4, GroupName="Parameters")]
		public bool ShowUB
		{ get; set; }
		
		//  
		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name="Opacity", Order=5, GroupName="Parameters")]
		public double Opacity
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Sound Name", Order=6, GroupName="Parameters")]
		public string AString
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show Rejector Entry", Order=7, GroupName="Parameters")]
		public bool ShowRejectorEntry
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Time Series Minutes", Order=8, GroupName="Parameters")]
		public int Minutes
		{ get; set; }
		 
		[NinjaScriptProperty]
		[Display(Name="Trigger Name", Order=9, GroupName="Parameters")]
		public string TriggerString
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Entry Name", Order=10, GroupName="Parameters")]
		public string EntryString
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Entry Sound", Order=11, GroupName="Parameters")]
		public string EntryWavString
		{ get; set; }
		
		#endregion
		
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private PriceRejectorCurrent[] cachePriceRejectorCurrent;
		public PriceRejectorCurrent PriceRejectorCurrent(bool showPriceRejector, bool showMaxDelta, bool showExhaustion, bool showUB, double opacity, string aString, bool showRejectorEntry, int minutes, string triggerString, string entryString, string entryWavString)
		{
			return PriceRejectorCurrent(Input, showPriceRejector, showMaxDelta, showExhaustion, showUB, opacity, aString, showRejectorEntry, minutes, triggerString, entryString, entryWavString);
		}

		public PriceRejectorCurrent PriceRejectorCurrent(ISeries<double> input, bool showPriceRejector, bool showMaxDelta, bool showExhaustion, bool showUB, double opacity, string aString, bool showRejectorEntry, int minutes, string triggerString, string entryString, string entryWavString)
		{
			if (cachePriceRejectorCurrent != null)
				for (int idx = 0; idx < cachePriceRejectorCurrent.Length; idx++)
					if (cachePriceRejectorCurrent[idx] != null && cachePriceRejectorCurrent[idx].ShowPriceRejector == showPriceRejector && cachePriceRejectorCurrent[idx].ShowMaxDelta == showMaxDelta && cachePriceRejectorCurrent[idx].ShowExhaustion == showExhaustion && cachePriceRejectorCurrent[idx].ShowUB == showUB && cachePriceRejectorCurrent[idx].Opacity == opacity && cachePriceRejectorCurrent[idx].AString == aString && cachePriceRejectorCurrent[idx].ShowRejectorEntry == showRejectorEntry && cachePriceRejectorCurrent[idx].Minutes == minutes && cachePriceRejectorCurrent[idx].TriggerString == triggerString && cachePriceRejectorCurrent[idx].EntryString == entryString && cachePriceRejectorCurrent[idx].EntryWavString == entryWavString && cachePriceRejectorCurrent[idx].EqualsInput(input))
						return cachePriceRejectorCurrent[idx];
			return CacheIndicator<PriceRejectorCurrent>(new PriceRejectorCurrent(){ ShowPriceRejector = showPriceRejector, ShowMaxDelta = showMaxDelta, ShowExhaustion = showExhaustion, ShowUB = showUB, Opacity = opacity, AString = aString, ShowRejectorEntry = showRejectorEntry, Minutes = minutes, TriggerString = triggerString, EntryString = entryString, EntryWavString = entryWavString }, input, ref cachePriceRejectorCurrent);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.PriceRejectorCurrent PriceRejectorCurrent(bool showPriceRejector, bool showMaxDelta, bool showExhaustion, bool showUB, double opacity, string aString, bool showRejectorEntry, int minutes, string triggerString, string entryString, string entryWavString)
		{
			return indicator.PriceRejectorCurrent(Input, showPriceRejector, showMaxDelta, showExhaustion, showUB, opacity, aString, showRejectorEntry, minutes, triggerString, entryString, entryWavString);
		}

		public Indicators.PriceRejectorCurrent PriceRejectorCurrent(ISeries<double> input , bool showPriceRejector, bool showMaxDelta, bool showExhaustion, bool showUB, double opacity, string aString, bool showRejectorEntry, int minutes, string triggerString, string entryString, string entryWavString)
		{
			return indicator.PriceRejectorCurrent(input, showPriceRejector, showMaxDelta, showExhaustion, showUB, opacity, aString, showRejectorEntry, minutes, triggerString, entryString, entryWavString);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.PriceRejectorCurrent PriceRejectorCurrent(bool showPriceRejector, bool showMaxDelta, bool showExhaustion, bool showUB, double opacity, string aString, bool showRejectorEntry, int minutes, string triggerString, string entryString, string entryWavString)
		{
			return indicator.PriceRejectorCurrent(Input, showPriceRejector, showMaxDelta, showExhaustion, showUB, opacity, aString, showRejectorEntry, minutes, triggerString, entryString, entryWavString);
		}

		public Indicators.PriceRejectorCurrent PriceRejectorCurrent(ISeries<double> input , bool showPriceRejector, bool showMaxDelta, bool showExhaustion, bool showUB, double opacity, string aString, bool showRejectorEntry, int minutes, string triggerString, string entryString, string entryWavString)
		{
			return indicator.PriceRejectorCurrent(input, showPriceRejector, showMaxDelta, showExhaustion, showUB, opacity, aString, showRejectorEntry, minutes, triggerString, entryString, entryWavString);
		}
	}
}

#endregion
