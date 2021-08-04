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
	public class StackedLevelsCount : Indicator
	{
		private System.Windows.Controls.Button myBuyButton;
		private System.Windows.Controls.Button mySellButton;
		private System.Windows.Controls.Grid   myGrid;
		
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
		private List<int> aggressiveBuys = new List<int>();
		private List<int> aggressiveSells = new List<int>();
		 
		private bool GoodToTrade = true;
		private Bollinger Bollinger1;	
		private bool LowTriggered = false;
		private string path;
		private int LastBar = 0; 
		private string SymbolFullName = "NQ 06-21";
		private int SymbolPerodicity = 10;
		
		// buttons
		private bool BuyButtonIsOn = false;
		private bool SellButtonIsOn = false;
		
		private int consecutiveBuyAggression = 0;
		private int consecutiveSellAggression = 0;
	
		#region Setup
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Print("State.SetDefaults");
				Description								= @"Enter the description for your new custom Indicator here.";
				Name									= "Stacked Levels Count";
				Calculate								= Calculate.OnEachTick;
				IsOverlay								= true;
				DisplayInDataBox						= true;
				DrawOnPricePanel						= true;
				DrawHorizontalGridLines					= true;
				DrawVerticalGridLines					= true;
				PaintPriceMarkers						= true;
				ScaleJustification						= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive				= true;
				DiagonalMultiplication					= 2;
				Minutes									= 1;
				MinVolume								= 5;
				//Opacity									= 0.2;
				RemoveTempAgressionDots					= true;
				AString									= "smsAlert1.wav";
				OrderFlowSymbol							= "ES 09-21";
				DebugBarCreation						= false;
				DebugSignalCreation						= false;
				ShowStatus								= true;
				path									= NinjaTrader.Core.Globals.UserDataDir + "ZeroPrints.csv"; 
				ShowAgressionCount 						= false;
				HistoricalOff							= true;
			}
			else if (State == State.Configure)
			{
				Print("State.Configure -- xtra data series");
				SymbolFullName = Bars.Instrument.FullName.ToString();
				SymbolPerodicity = Bars.BarsPeriod.BaseBarsPeriodValue;
				ClearOutputWindow();
				AddVolumetric(OrderFlowSymbol, BarsPeriodType.Minute, Minutes, VolumetricDeltaType.BidAsk, 1);
				//AddVolumetric(SymbolFullName, BarsPeriodType.Minute, SymbolPerodicity, VolumetricDeltaType.BidAsk, 1);
			}
			else if (State == State.DataLoaded)
			{	
				Print("State.DataLoaded");
			}
			// Once the NinjaScript object has reached State.Historical, our custom control can now be added to the chart
			  else if (State == State.Historical)
			  {
				  Print("State.Historical");
			    // Because we're dealing with UI elements, we need to use the Dispatcher which created the object
			    // otherwise we will run into threading errors...
			    // e.g, "Error on calling 'OnStateChange' method: You are accessing an object which resides on another thread."
			    // Furthermore, we will do this operation Asynchronously to avoid conflicts with internal NT operations
			    ChartControl.Dispatcher.InvokeAsync((() =>
			    {
			        // Grid already exists
			        if (UserControlCollection.Contains(myGrid))
			          return;
			 
			        // Add a control grid which will host our custom buttons
			        myGrid = new System.Windows.Controls.Grid
			        {
			          Name = "MyCustomGrid",
			          // Align the control to the top right corner of the chart
			          HorizontalAlignment = HorizontalAlignment.Right,
			          VerticalAlignment = VerticalAlignment.Top,
			        };
			 
			        // Define the two columns in the grid, one for each button
			        System.Windows.Controls.ColumnDefinition column1 = new System.Windows.Controls.ColumnDefinition();
			        System.Windows.Controls.ColumnDefinition column2 = new System.Windows.Controls.ColumnDefinition();
			 
			        // Add the columns to the Grid
			        myGrid.ColumnDefinitions.Add(column1);
			        myGrid.ColumnDefinitions.Add(column2);
			 
			        // Define the custom Buy Button control object
			        myBuyButton = new System.Windows.Controls.Button
			        {
			          Name = "MyBuyButton",
			          Content = "LONG",
			          Foreground = Brushes.Black,
			          Background = Brushes.DarkGreen
			        };
			 
			        // Define the custom Sell Button control object
			        mySellButton = new System.Windows.Controls.Button
			        {
			          Name = "MySellButton",
			          Content = "SHORT",
			          Foreground = Brushes.Black,
			          Background = Brushes.DarkRed
			        };
			 
			        // Subscribe to each buttons click event to execute the logic we defined in OnMyButtonClick()
			        myBuyButton.Click += OnMyButtonClick;
			        mySellButton.Click += OnMyButtonClick;
			 
			        // Define where the buttons should appear in the grid
			        System.Windows.Controls.Grid.SetColumn(myBuyButton, 0);
			        System.Windows.Controls.Grid.SetColumn(mySellButton, 1);
			 
			        // Add the buttons as children to the custom grid
			        myGrid.Children.Add(myBuyButton);
			        myGrid.Children.Add(mySellButton);
			 
			        // Finally, add the completed grid to the custom NinjaTrader UserControlCollection
			        UserControlCollection.Add(myGrid);
			 
			    }));
			  }
			 
			  // When NinjaScript object is removed, make sure to unsubscribe to button click events
			  else if (State == State.Terminated)
			  {
			    if (ChartControl == null)
			        return;
			 
			    // Again, we need to use a Dispatcher to interact with the UI elements
			    ChartControl.Dispatcher.InvokeAsync((() =>
			    {
			        if (myGrid != null)
			        {
			          if (myBuyButton != null)
			          {
			              myGrid.Children.Remove(myBuyButton);
			              myBuyButton.Click -= OnMyButtonClick;
			              myBuyButton = null;
			          }
			          if (mySellButton != null)
			          {
			              myGrid.Children.Remove(mySellButton);
			              mySellButton.Click -= OnMyButtonClick;
			              mySellButton = null;
			          }
			        }
			    }));
			  }

		}
		
		#endregion
		
		protected override void OnBarUpdate()
		{	
			if (State < State.Realtime && HistoricalOff ) { return; }
			
			if (BarsInProgress == 1 && CurrentBar > 1 ) {
				LastBar = CurrentBar - 1;
				PopulateList(debug: DebugBarCreation); 
				ShowAllStats();
			}
		}
		
		#region Data Structures
		
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
				if ( debug ) { 
					int barsPer = Bars.BarsPeriod.BaseBarsPeriodValue;
					string name = Bars.Instrument.FullName.ToString(); 
					string id = SymbolFullName + "\t\t" + SymbolPerodicity + " min \t";
					Print("---------->   " + id  + Time[0].ToShortTimeString() +  "   <---------");
				}
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
				if ( debug ) { Print("---------------------------------------------------------------");}
				int loopCount = 0;
				/// Loop through entire bar looking for agression
				consecutiveBuyAggression = 0;
				consecutiveSellAggression = 0;
				foreach (var item in FullBar) { 
					SellAggression(FullBar: FullBar, loopCount: loopCount);
					BuyAggression(FullBar: FullBar, loopCount: loopCount);
					loopCount += 1;
				}
				if( DebugSignalCreation ) { Print("---------------------------------"); }
				if ( ShowAgressionCount ) {
					Draw.Text(this, "consecutiveBuyAggression"+ CurrentBar, consecutiveBuyAggression.ToString(), 0, Low[0] - 2 * TickSize, Brushes.DodgerBlue);
					Draw.Text(this, "consecutiveASellggression" + CurrentBar, consecutiveSellAggression.ToString(), 0, High[0] + 2 * TickSize, Brushes.Red);
					Print(Time[0].ToShortTimeString() + "  buys: " + consecutiveBuyAggression + " sells: " + consecutiveSellAggression);
				}
				
				aggressiveBuys.Clear();
				aggressiveSells.Clear();
	        }
	        catch{}
			if ( debug ) { Print(" " );}	
		}
		
		#endregion

		#region Entry Logic
		
		void SellAggression(List<BarSlice> FullBar, int loopCount ) {
			if ( loopCount > 0 ) {
				double priceBelow = FullBar[loopCount].priceLevel; 
				double bidBelow = FullBar[loopCount].bidVolume;  
				double askAbove = FullBar[loopCount - 1].askVolume; 
				double askAboveX2 = ( askAbove * DiagonalMultiplication); 
				
				if( DebugSignalCreation ){ Print("\tComparing : " + bidBelow + " > "  + askAboveX2 );}
				if(bidBelow >= askAboveX2 && bidBelow > MinVolume ) {
					if( DebugSignalCreation ){Print("\t-->Agg Selling at : " + priceBelow + " : " + bidBelow); } 
					if (aggressiveSells.Count > 0  && aggressiveSells.Last() == 1) {
						//consecutiveSellAggression += 1;
						sendAlert(message: "Aggressive Selling", sound: AString);
					}
					aggressiveSells.Add(1);
				} else {
					aggressiveSells.Add(0);
					//consecutiveSellAggression = 0;
				} 
				//PrintList(ThisList: aggressiveSells);
				ShowAggression(ThisList: aggressiveSells, Buy: false);
			} else {
				aggressiveSells.Add(0);
			}
		}
		
		void BuyAggression(List<BarSlice> FullBar, int loopCount ) {
			if ( loopCount > 0 ) {
				double priceBelow = FullBar[loopCount].priceLevel; 
				double bidBelow = FullBar[loopCount].bidVolume;  
				double askAbove = FullBar[loopCount - 1].askVolume; 
				double bidBelowX2 = ( bidBelow * DiagonalMultiplication); 
				
				if( DebugSignalCreation ){ Print("\tComparing : " + askAbove + " < "  + bidBelowX2 );}
				if(bidBelowX2 <= askAbove && askAbove > MinVolume) {
					if( DebugSignalCreation ){Print("\t-->Agg Selling at : " + priceBelow + " : " + bidBelow); } 
					
					// count how many times this happens and display
					if (aggressiveSells.Count > 0  && aggressiveSells.Last() == 1) {
						//consecutiveBuyAggression +=1;
						sendAlert(message: "Aggressive Selling", sound: AString);
					}
					
					aggressiveBuys.Add(1);
				} else {
					aggressiveBuys.Add(0);
					//consecutiveBuyAggression = 0;
				} 
				//PrintList(ThisList: aggressiveSells);
				ShowAggression(ThisList: aggressiveBuys, Buy: true);
			} else {
				aggressiveBuys.Add(0);
			}
		}
		
		void ShowAggression(List<int> ThisList, bool Buy) {
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
				consecutiveBuyAggression = 0;
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
						
						if ( Buy ) {
							consecutiveBuyAggression +=1;
						}
						
						double UpperPrice = High[0] - ((counter + upperDot ) * TickSize);
						double ThisPrice = High[0] - ((counter + lowerDot ) * TickSize);
						
						if( Buy ) {  
							Draw.TriangleUp(this, name1 + CurrentBar + counter, false, 0, UpperPrice, MyBrush); 
							Draw.TriangleUp(this, name2 + CurrentBar + counter, false, 0, ThisPrice, MyBrush); 
						} else { 
							Draw.TriangleDown(this, name1 + CurrentBar + counter, false, 0, UpperPrice, MyBrush); 
							Draw.TriangleDown(this, name2 + CurrentBar + counter, false, 0, ThisPrice, MyBrush);  
						}
						
						//Draw.Dot(this, name1 + CurrentBar + counter, false, 0, UpperPrice, MyBrush);
						//Draw.Dot(this, name2 + CurrentBar + counter, false, 0, ThisPrice, MyBrush);
						ShowEntry(Long: Buy);
					}
					lastItem = item;
					counter += 1;
				}
		}
				
		void ShowEntry(bool Long) {
			if( Long && BuyButtonIsOn )  {
				Draw.ArrowUp(this, "LE"+ CurrentBar, false, 0, Low[0] - TickSize, Brushes.DodgerBlue);
			} 
			
			if( !Long && SellButtonIsOn )  {
				Draw.ArrowDown(this, "SE"+ CurrentBar, false, 0, High[0] + TickSize, Brushes.Red);
			}
		}
		
		#endregion
		
		#region Helpers
		
		private void sendAlert(string message, string sound ) {
			message += " " + Bars.Instrument.MasterInstrument.Name;
			Alert("myAlert"+CurrentBar, Priority.High, message, NinjaTrader.Core.Globals.InstallDir+@"\sounds\"+ sound,10, Brushes.Black, Brushes.Yellow);  
			if (CurrentBar < Count -2) return;
		}
		
		void PrintList(List<int> ThisList) {
			string listPrint = "";
			foreach (var item in ThisList)  
			{
				listPrint +=  item  + ", ";
			}
			Print(listPrint);
		}
				
		void ShowAllStats() {
			if ( !ShowStatus ) { return; }
			string info = "\nS T A T U S:\n" + OrderFlowSymbol + "\t" + Minutes + " min\n" +
			"Diagonal Multi:\t" + DiagonalMultiplication.ToString() +
			"\nRemove Dots\t" + RemoveTempAgressionDots;
			if ( BuyButtonIsOn ) { info +=  "\nLong Trading Enabled";}
			if ( SellButtonIsOn ) { info += "\nShort Trading Enabled";}
			 info += "\n"; 
			SimpleFont NoteFont	= new SimpleFont("Arial", 14);
			
			Draw.TextFixed(this, "myTextFixed", 
				info, 
				TextPosition.BottomLeft, 
				Brushes.WhiteSmoke,  // text color
				NoteFont, 
				Brushes.DimGray, // outline color
				Brushes.Black,  // bkg color
				50); 
			
		}	
					
		#endregion
		
		#region Button Logic
		// Define a custom event method to handle our custom task when the button is clicked
		private void OnMyButtonClick(object sender, RoutedEventArgs rea)
		{
		  System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
		  if (button != null) {
			  Print(button.Name + " Clicked");
			  if ( button.Name == "MyBuyButton") {
				  if ( BuyButtonIsOn ) {
					  myBuyButton.Background = Brushes.DarkGreen;
					  myBuyButton.Foreground = Brushes.Black;
				  } else {
					  myBuyButton.Background = Brushes.LimeGreen;
					  myBuyButton.Foreground = Brushes.White;
				  }
					BuyButtonIsOn = !BuyButtonIsOn;
				  }
			  else {
				  if ( SellButtonIsOn ) {
					  mySellButton.Background = Brushes.DarkRed;
					  mySellButton.Foreground = Brushes.Black;
				  } else {
					  mySellButton.Background = Brushes.Red;
					  mySellButton.Foreground = Brushes.White;
				  }
					SellButtonIsOn = !SellButtonIsOn;
				  
			  }
		  }
		}
		#endregion
		
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
		[Display(Name="Size Filter", Order=3, GroupName="Parameters")]
		public int MinVolume
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Remove Temporary Dots", Order=4, GroupName="Parameters")]
		public bool RemoveTempAgressionDots
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show Status", Order=5, GroupName="Parameters")]
		public bool ShowStatus
		{ get; set; }
		
//		[NinjaScriptProperty]
//		[Range(0.1, double.MaxValue)]
//		[Display(Name="Opacity", Order=5, GroupName="Parameters")]
//		public double Opacity
//		{ get; set; }		
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Time Series Minutes", Order=6, GroupName="Parameters")]
		public int Minutes
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Alert File Name", Order=6, GroupName="Parameters")]
		public string AString
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Bar Creation", Order=8, GroupName="Debugging")]
		public bool DebugBarCreation
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Signal Creation", Order=9, GroupName="Debugging")]
		public bool DebugSignalCreation
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="ShowAgressionCount", Order=10, GroupName="Debugging")]
		public bool ShowAgressionCount
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Historical Off", Order=11, GroupName="Debugging")]
		public bool HistoricalOff
		{ get; set; }
		
		#endregion
		
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private StackedLevelsCount[] cacheStackedLevelsCount;
		public StackedLevelsCount StackedLevelsCount(int diagonalMultiplication, string orderFlowSymbol, int minVolume, bool removeTempAgressionDots, bool showStatus, int minutes, string aString, bool debugBarCreation, bool debugSignalCreation, bool showAgressionCount, bool historicalOff)
		{
			return StackedLevelsCount(Input, diagonalMultiplication, orderFlowSymbol, minVolume, removeTempAgressionDots, showStatus, minutes, aString, debugBarCreation, debugSignalCreation, showAgressionCount, historicalOff);
		}

		public StackedLevelsCount StackedLevelsCount(ISeries<double> input, int diagonalMultiplication, string orderFlowSymbol, int minVolume, bool removeTempAgressionDots, bool showStatus, int minutes, string aString, bool debugBarCreation, bool debugSignalCreation, bool showAgressionCount, bool historicalOff)
		{
			if (cacheStackedLevelsCount != null)
				for (int idx = 0; idx < cacheStackedLevelsCount.Length; idx++)
					if (cacheStackedLevelsCount[idx] != null && cacheStackedLevelsCount[idx].DiagonalMultiplication == diagonalMultiplication && cacheStackedLevelsCount[idx].OrderFlowSymbol == orderFlowSymbol && cacheStackedLevelsCount[idx].MinVolume == minVolume && cacheStackedLevelsCount[idx].RemoveTempAgressionDots == removeTempAgressionDots && cacheStackedLevelsCount[idx].ShowStatus == showStatus && cacheStackedLevelsCount[idx].Minutes == minutes && cacheStackedLevelsCount[idx].AString == aString && cacheStackedLevelsCount[idx].DebugBarCreation == debugBarCreation && cacheStackedLevelsCount[idx].DebugSignalCreation == debugSignalCreation && cacheStackedLevelsCount[idx].ShowAgressionCount == showAgressionCount && cacheStackedLevelsCount[idx].HistoricalOff == historicalOff && cacheStackedLevelsCount[idx].EqualsInput(input))
						return cacheStackedLevelsCount[idx];
			return CacheIndicator<StackedLevelsCount>(new StackedLevelsCount(){ DiagonalMultiplication = diagonalMultiplication, OrderFlowSymbol = orderFlowSymbol, MinVolume = minVolume, RemoveTempAgressionDots = removeTempAgressionDots, ShowStatus = showStatus, Minutes = minutes, AString = aString, DebugBarCreation = debugBarCreation, DebugSignalCreation = debugSignalCreation, ShowAgressionCount = showAgressionCount, HistoricalOff = historicalOff }, input, ref cacheStackedLevelsCount);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.StackedLevelsCount StackedLevelsCount(int diagonalMultiplication, string orderFlowSymbol, int minVolume, bool removeTempAgressionDots, bool showStatus, int minutes, string aString, bool debugBarCreation, bool debugSignalCreation, bool showAgressionCount, bool historicalOff)
		{
			return indicator.StackedLevelsCount(Input, diagonalMultiplication, orderFlowSymbol, minVolume, removeTempAgressionDots, showStatus, minutes, aString, debugBarCreation, debugSignalCreation, showAgressionCount, historicalOff);
		}

		public Indicators.StackedLevelsCount StackedLevelsCount(ISeries<double> input , int diagonalMultiplication, string orderFlowSymbol, int minVolume, bool removeTempAgressionDots, bool showStatus, int minutes, string aString, bool debugBarCreation, bool debugSignalCreation, bool showAgressionCount, bool historicalOff)
		{
			return indicator.StackedLevelsCount(input, diagonalMultiplication, orderFlowSymbol, minVolume, removeTempAgressionDots, showStatus, minutes, aString, debugBarCreation, debugSignalCreation, showAgressionCount, historicalOff);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.StackedLevelsCount StackedLevelsCount(int diagonalMultiplication, string orderFlowSymbol, int minVolume, bool removeTempAgressionDots, bool showStatus, int minutes, string aString, bool debugBarCreation, bool debugSignalCreation, bool showAgressionCount, bool historicalOff)
		{
			return indicator.StackedLevelsCount(Input, diagonalMultiplication, orderFlowSymbol, minVolume, removeTempAgressionDots, showStatus, minutes, aString, debugBarCreation, debugSignalCreation, showAgressionCount, historicalOff);
		}

		public Indicators.StackedLevelsCount StackedLevelsCount(ISeries<double> input , int diagonalMultiplication, string orderFlowSymbol, int minVolume, bool removeTempAgressionDots, bool showStatus, int minutes, string aString, bool debugBarCreation, bool debugSignalCreation, bool showAgressionCount, bool historicalOff)
		{
			return indicator.StackedLevelsCount(input, diagonalMultiplication, orderFlowSymbol, minVolume, removeTempAgressionDots, showStatus, minutes, aString, debugBarCreation, debugSignalCreation, showAgressionCount, historicalOff);
		}
	}
}

#endregion
