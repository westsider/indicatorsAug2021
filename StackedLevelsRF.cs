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
using System.IO;
using SharpDX.Direct2D1;
using SharpDX;
using SharpDX.DirectWrite;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class StackedLevelsRF : Indicator
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
		private List<int> aggressiveBuys = new List<int>();
		private List<int> aggressiveSells = new List<int>();
		
		private List<RejectorSignal> RejectorSignals = new List<RejectorSignal>();
		private bool GoodToTrade = true;
		private Bollinger Bollinger1;	
		private bool LowTriggered = false;
		private string path;
		private int LastBar = 0;
		private bool debugAggr = false;
		
		
		private void  diagoanalAgression(BarSlice slice) {
			
			if (slice.bidVolume >  slice.askVolume) {
				
			}
		}
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description								= @"Enter the description for your new custom Indicator here.";
				Name									= "Stacked Levels RF";
				Calculate								= Calculate.OnBarClose;
				IsOverlay								= true;
				DisplayInDataBox						= true;
				DrawOnPricePanel						= true;
				DrawHorizontalGridLines					= true;
				DrawVerticalGridLines					= true;
				PaintPriceMarkers						= true;
				ScaleJustification						= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive				= true;
				DiagonalMultiplication					= 2;
				Minutes									= 3;
				MinVolume								= 1;
				Opacity									= 0.2;
				RemoveTempAgressionDots					= true;
				AString									= "smsAlert1.wav";
				OrderFlowSymbol							= "ES 06-21";
				path									= NinjaTrader.Core.Globals.UserDataDir + "ZeroPrints.csv";
			}
			else if (State == State.Configure)
			{
				ClearOutputWindow();
				AddVolumetric(OrderFlowSymbol, BarsPeriodType.Minute, Minutes, VolumetricDeltaType.BidAsk, 1);
			}
			else if (State == State.DataLoaded)
			{				
				Bollinger1								= Bollinger(Close, 1.5, 14);
			}
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			  BrushProperties bp2=new BrushProperties();
			  bp2.Opacity= 1.0f;
			  // MARK - TODO - Make this number auto scale
			  int priceAdjuster = 18; // larger number is lower
			  int priceWidth = (priceAdjuster * -2 );
			  int currentBarRef = 0;
			
			  SharpDX.Direct2D1.SolidColorBrush frame2=new SharpDX.Direct2D1.SolidColorBrush(RenderTarget,SharpDX.Color.Red,bp2);
			  SharpDX.Direct2D1.SolidColorBrush frame=new SharpDX.Direct2D1.SolidColorBrush(RenderTarget,SharpDX.Color.DodgerBlue,bp2);
										
			  if (ChartBars != null)
			  {
				    for (int barIndex = ChartBars.FromIndex; barIndex <= ChartBars.ToIndex; barIndex++)
				    {
					    	currentBarRef +=1;
					}
			}
		}
		
		protected override void OnBarUpdate()
		{
			if (BarsInProgress == 1 && CurrentBar > 1 ) {
				LastBar = CurrentBar - 1;
				PopulateList(debug: false); 
			}
		}
		
		private void PopulateList(bool debug) {
			NinjaTrader.NinjaScript.BarsTypes.VolumetricBarsType barsType = Bars.BarsSeries.BarsType as    
	        NinjaTrader.NinjaScript.BarsTypes.VolumetricBarsType;
			FullBar.Clear();
			double ticks = TickSize;
			double NumPrices = Range()[0] / ticks; 
			
			try
	        {
				/// loop thru bar prices and fill Struct
				//if ( debug ) { Print("Bar: " + CurrentBar + "\t" + Time[0] + " \trange: " + NumPrices); }
				if ( debug ) { Print("-----------> enter array \t" + Time[0].ToShortTimeString() +  " <----------");}
				for (int index = 0; index <= NumPrices; index++)
				{ 
					double thisPrice = High[0] - (index * TickSize); 
					double thisBid =  barsType.Volumes[CurrentBar].GetBidVolumeForPrice(thisPrice);
					double thisAsk =  barsType.Volumes[CurrentBar].GetAskVolumeForPrice(thisPrice);
					double thisDelta =  barsType.Volumes[CurrentBar].GetDeltaForPrice(thisPrice);
					BarSlice slice = new BarSlice(thisPrice, thisBid, thisAsk, thisDelta, 0.0, 0.0);
					FullBar.Add(slice);
					if ( debug ) { Print(  slice.priceLevel.ToString("N2") + " \t\tbid: " + slice.bidVolume + " \t\task: " + slice.askVolume + "\t\t\t delta: " + slice.delta);}
				}
				if ( debug ) { Print("-------------------> exit array <------------------");}
				int loopCount = 0;
				/// Loop through entire bar looking for agression
				foreach (var item in FullBar) { 
					SellAggression(FullBar: FullBar, loopCount: loopCount);
					BuyAggression(FullBar: FullBar, loopCount: loopCount);
					loopCount += 1;
				}
				if( debugAggr ) { Print("---------------------------------"); }
				aggressiveBuys.Clear();
				aggressiveSells.Clear();
	        }
	        catch{}
			if ( debug ) { Print(" " );}	
		}

		void SellAggression(List<BarSlice> FullBar, int loopCount ) {
			if ( loopCount > 0 ) {
				double priceBelow = FullBar[loopCount].priceLevel; 
				double bidBelow = FullBar[loopCount].bidVolume;  
				double askAbove = FullBar[loopCount - 1].askVolume; 
				double askAboveX2 = ( askAbove * DiagonalMultiplication); 
				
				if( debugAggr ){ Print("\tComparing : " + bidBelow + " > "  + askAboveX2 );}
				if(bidBelow >= askAboveX2 && bidBelow > MinVolume ) {
					if( debugAggr ){Print("\t-->Agg Selling at : " + priceBelow + " : " + bidBelow); } 
					if (aggressiveSells.Count > 0  && aggressiveSells.Last() == 1) {
						sendAlert(message: "Aggressive Selling", sound: AString);
					}
					aggressiveSells.Add(1);
				} else {
					aggressiveSells.Add(0);
				} 
				//PrintList(ThisList: aggressiveSells);
				ShowAggression(ThisList: aggressiveSells, Buy: false);
			} else {
				aggressiveSells.Add(0);
			}
		}
		
		void PrintList(List<int> ThisList) {
			string listPrint = "";
			foreach (var item in ThisList)  
			{
				listPrint +=  item  + ", ";
			}
			Print(listPrint);
		}
		
		void ShowAggression(List<int> ThisList, Bool Buy) {
			var MyBrush = Brushes.Red;
			string name1 = "SellAggUpper";
			string name2 = "SellAgg";
			int upperDot = 0;
			int lowerDot = -1;
			
			if ( Buy ) { 
				MyBrush = Brushes.DodgerBlue; 
				name1 = "BuyAggUpper";
				name2 = "BuyAgg";
				upperDot = -1;
				lowerDot = -2;
			}
			int lastItem = 0;
			int counter = 0;
			if ( ThisList.Count > 0 )
				foreach (var item in ThisList)  
				{
					if( RemoveTempAgressionDots) {
						RemoveDrawObject(name1 + CurrentBar + counter);
						RemoveDrawObject(name2 + CurrentBar + counter);
					}
					if ( item == 1 && lastItem == 1) {
						
						double UpperPrice = High[0] - ((counter + upperDot ) * TickSize);
						double ThisPrice = High[0] - ((counter + lowerDot ) * TickSize);
						Draw.Dot(this, name1 + CurrentBar + counter, false, 0, UpperPrice, MyBrush);
						Draw.Dot(this, name2 + CurrentBar + counter, false, 0, ThisPrice, MyBrush);
					}
					lastItem = item;
					counter += 1;
				}
		}
		
		void BuyAggression(List<BarSlice> FullBar, int loopCount ) {
			if ( loopCount > 0 ) {
				double priceBelow = FullBar[loopCount].priceLevel; 
				double bidBelow = FullBar[loopCount].bidVolume;  
				double askAbove = FullBar[loopCount - 1].askVolume; 
				double bidBelowX2 = ( bidBelow * DiagonalMultiplication); 
				
				if( debugAggr ){ Print("\tComparing : " + askAbove + " < "  + bidBelowX2 );}
				if(bidBelowX2 <= askAbove && askAbove > MinVolume) {
					if( debugAggr ){Print("\t-->Agg Selling at : " + priceBelow + " : " + bidBelow); } 
					if (aggressiveSells.Count > 0  && aggressiveSells.Last() == 1) {
						sendAlert(message: "Aggressive Selling", sound: AString);
					}
					aggressiveBuys.Add(1);
				} else {
					aggressiveBuys.Add(0);
				} 
				//PrintList(ThisList: aggressiveSells);
				ShowAggression(ThisList: aggressiveBuys, Buy: true);
			} else {
				aggressiveBuys.Add(0);
			}
		}
		
		private void sendAlert(string message, string sound ) {
			message += " " + Bars.Instrument.MasterInstrument.Name;
			Alert("myAlert"+CurrentBar, Priority.High, message, NinjaTrader.Core.Globals.InstallDir+@"\sounds\"+ sound,10, Brushes.Black, Brushes.Yellow);  
			if (CurrentBar < Count -2) return;
		}
		
		#region Write File
		private void ClearFile(string path)
        {
            try    
			{    
				// Check if file exists with its full path    
				if (File.Exists(path))    
				{    
					// If file found, delete it    
					File.Delete(path);    
					//Print("File deleted.");    
				} 
				else  Print("File not found");    
			}    
			catch (IOException ioExp)    
			{    
				Print(ioExp.Message);    
			}  
        }
		
		private void WriteFile(string path, string newLine, bool header)
        {
			if ( header ) {
				ClearFile(path: path);
				using (var tw = new StreamWriter(path, true))
	            {
	                tw.WriteLine(newLine); 
	                tw.Close();
					tw.Dispose();
	            }
				return;
			}
			
            using (var tw = new StreamWriter(path, true))
            {
                tw.WriteLine(newLine);
                tw.Close();
				tw.Dispose();
            }
        }
		#endregion
		
		
		#region Properties
//		[NinjaScriptProperty]
//		[Display(Name="ShowPriceRejector", Order=1, GroupName="Parameters")]
//		public bool ShowPriceRejector
//		{ get; set; }

//		[NinjaScriptProperty]
//		[Display(Name="Show Max Delta", Order=2, GroupName="Parameters")]
//		public bool ShowMaxDelta
//		{ get; set; }

//		[NinjaScriptProperty]
//		[Display(Name="filter the edges", Order=3, GroupName="Parameters")]
//		public bool ShowExhaustion
//		{ get; set; }

//		[NinjaScriptProperty]
//		[Display(Name="ShowUB", Order=4, GroupName="Parameters")]
//		public bool ShowUB
//		{ get; set; }
		
				
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Diagonal Multiplication", Order=1, GroupName="Parameters")]
		public int DiagonalMultiplication
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Order Flow Symbol", Order=2, GroupName="Parameters")]
		public string OrderFlowSymbol
		{ get; set; }
		
		// MinVolume
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Min Volume", Order=3, GroupName="Parameters")]
		public int MinVolume
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Remove Temp Agression Dots", Order=4, GroupName="Parameters")]
		public bool RemoveTempAgressionDots
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name="Opacity", Order=5, GroupName="Parameters")]
		public double Opacity
		{ get; set; }		
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Time Series Minutes", Order=3, GroupName="Parameters")]
		public int Minutes
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Alert File Name", Order=6, GroupName="Parameters")]
		public string AString
		{ get; set; }
		
		#endregion
		
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private StackedLevelsRF[] cacheStackedLevelsRF;
		public StackedLevelsRF StackedLevelsRF(int diagonalMultiplication, string orderFlowSymbol, int minVolume, bool removeTempAgressionDots, double opacity, int minutes, string aString)
		{
			return StackedLevelsRF(Input, diagonalMultiplication, orderFlowSymbol, minVolume, removeTempAgressionDots, opacity, minutes, aString);
		}

		public StackedLevelsRF StackedLevelsRF(ISeries<double> input, int diagonalMultiplication, string orderFlowSymbol, int minVolume, bool removeTempAgressionDots, double opacity, int minutes, string aString)
		{
			if (cacheStackedLevelsRF != null)
				for (int idx = 0; idx < cacheStackedLevelsRF.Length; idx++)
					if (cacheStackedLevelsRF[idx] != null && cacheStackedLevelsRF[idx].DiagonalMultiplication == diagonalMultiplication && cacheStackedLevelsRF[idx].OrderFlowSymbol == orderFlowSymbol && cacheStackedLevelsRF[idx].MinVolume == minVolume && cacheStackedLevelsRF[idx].RemoveTempAgressionDots == removeTempAgressionDots && cacheStackedLevelsRF[idx].Opacity == opacity && cacheStackedLevelsRF[idx].Minutes == minutes && cacheStackedLevelsRF[idx].AString == aString && cacheStackedLevelsRF[idx].EqualsInput(input))
						return cacheStackedLevelsRF[idx];
			return CacheIndicator<StackedLevelsRF>(new StackedLevelsRF(){ DiagonalMultiplication = diagonalMultiplication, OrderFlowSymbol = orderFlowSymbol, MinVolume = minVolume, RemoveTempAgressionDots = removeTempAgressionDots, Opacity = opacity, Minutes = minutes, AString = aString }, input, ref cacheStackedLevelsRF);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.StackedLevelsRF StackedLevelsRF(int diagonalMultiplication, string orderFlowSymbol, int minVolume, bool removeTempAgressionDots, double opacity, int minutes, string aString)
		{
			return indicator.StackedLevelsRF(Input, diagonalMultiplication, orderFlowSymbol, minVolume, removeTempAgressionDots, opacity, minutes, aString);
		}

		public Indicators.StackedLevelsRF StackedLevelsRF(ISeries<double> input , int diagonalMultiplication, string orderFlowSymbol, int minVolume, bool removeTempAgressionDots, double opacity, int minutes, string aString)
		{
			return indicator.StackedLevelsRF(input, diagonalMultiplication, orderFlowSymbol, minVolume, removeTempAgressionDots, opacity, minutes, aString);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.StackedLevelsRF StackedLevelsRF(int diagonalMultiplication, string orderFlowSymbol, int minVolume, bool removeTempAgressionDots, double opacity, int minutes, string aString)
		{
			return indicator.StackedLevelsRF(Input, diagonalMultiplication, orderFlowSymbol, minVolume, removeTempAgressionDots, opacity, minutes, aString);
		}

		public Indicators.StackedLevelsRF StackedLevelsRF(ISeries<double> input , int diagonalMultiplication, string orderFlowSymbol, int minVolume, bool removeTempAgressionDots, double opacity, int minutes, string aString)
		{
			return indicator.StackedLevelsRF(input, diagonalMultiplication, orderFlowSymbol, minVolume, removeTempAgressionDots, opacity, minutes, aString);
		}
	}
}

#endregion
