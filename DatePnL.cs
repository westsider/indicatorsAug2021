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
	public class DatePnL : Indicator
	{
		private double Open_D = 0.0;
		private double Close_D = 0.0;
		private double DatePnL_D = 0.0;
		private string message = "no message";
		private long startTime = 0;
		private	long endTime = 0;
		private int startBar = 0;
		private int rthBarCount = 0;
		private double Y_High = 0.0;
		private double Y_Low = 0.0;
		private bool inRange = false;
		
		private Account account;
		private double xAccountSize;
		private bool Realized		= true;
		private double intradayLoss = 0.0;
		private double gain = 0.0;
		private string name = "NA";
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "Date Profit";
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
				IsSuspendedWhileInactive					= true;
				RTHopen						= DateTime.Parse("06:30", System.Globalization.CultureInfo.InvariantCulture);
				RTHclose					= DateTime.Parse("13:00", System.Globalization.CultureInfo.InvariantCulture);
				UsePoints								= true;
				Contracts								= 2;
			}
			else if (State == State.Configure)
			{
				startTime = long.Parse(RTHopen.ToString("HHmmss"));
			 	endTime = long.Parse(RTHclose.ToString("HHmmss"));
				AddDataSeries(Data.BarsPeriodType.Minute, 1);
				ClearOutputWindow();
			}
		}

		protected override void OnBarUpdate()
		{
			if ( CurrentBar < 5 ) { return; }

			if ("Sunday"  == Time[0].DayOfWeek.ToString()) { return; }
			
			lock (Account.All)  {         
			    account = Account.All.FirstOrDefault(a => a.Name == AccountNames);
				gain =  account.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar);
			    name = account.Name;
				 
			}
			
			/// loss not calculating correctlytried to reset intraday loss @ open
			if (  gain < intradayLoss ) 
			{ 
				intradayLoss = gain; 
			}
			
			
			if (BarsInProgress == 1 && ToTime(Time[0]) == startTime ) { 
				intradayLoss = 0.0;
				gain = 0.0;
				Open_D = Open[0];
				DatePnL_D = Open_D - Close_D;
				message =  Time[0].ToShortDateString() + " "  + Time[0].ToShortTimeString() + "   Open: " + Open_D.ToString() +  "   Gap: " + DatePnL_D.ToString(); 
				Print(message);
				//Draw.Dot(this, "open"+CurrentBar, false, 0, Open_D, Brushes.White);
			}
			
			if (BarsInProgress == 1 && ToTime(Time[0]) == endTime ) { 
				Close_D = Close[0];
				//Print(Time[0].ToShortDateString() + " \t" + Time[0].ToShortTimeString() + "\t close: " + Close_D.ToString());
				//Draw.Dot(this, "close"+CurrentBar, false, 0, Close_D, Brushes.White);
				
			}
			
			/// pre market gap
			if (BarsInProgress == 1 && ToTime(Time[0]) < startTime ) { 
				DatePnL_D = Close[0] - Close_D;
				message =  Time[0].ToShortDateString() + " \t Pre M Gap: " + DatePnL_D.ToString();
				//Print(message);
				message += "  Gain: $" + gain.ToString("N0");
			}
			
			yesterdaysRange(debug: false);
			
			/// after open performance text
			if (BarsInProgress == 1 && ToTime(Time[0]) > startTime ) { 
				message =  Time[0].ToShortDateString() + "  " + name;
				if ( UsePoints ) {
					intradayLoss =  intradayLoss / ( 5 * Contracts );
					gain =  account.Get(AccountItem.GrossRealizedProfitLoss, Currency.UsDollar);
					gain = gain / ( 5 * Contracts );
					message += "  Gain: " + gain.ToString("N2") + " pts  DD: " + intradayLoss.ToString("N0");
				} else {
					message += "  Gain: $" + gain.ToString("N0") + "  DD: $" + intradayLoss.ToString("N0");
				}
				 
			}
			Draw.TextFixed(this, "MyTextFixed", message, TextPosition.TopLeft);
		}
		
		private void yesterdaysRange(bool debug) {
			if (BarsInProgress == 0 && ToTime(Time[0]) == startTime ) { 
				if ( debug ) { Draw.Dot(this, "Y_High"+CurrentBar, false, 0, Y_High, Brushes.Blue);}
				if ( debug ) { Draw.Dot(this, "Y_Low" + CurrentBar, false, 0, Y_Low, Brushes.Blue);}
				if(Open[0] < Y_High && Open[0] > Y_Low) {
					if ( debug ) { Draw.Text(this, "open"+ CurrentBar, "In Range", 0, High[0] + 2 * TickSize, Brushes.Blue); }
					inRange = true;
					//message += "\tIn Range";
				} else {
					if ( debug ) { Draw.Text(this, "outrange"+ CurrentBar, "Outside Range", 0, High[0] + 2 * TickSize, Brushes.Blue);}
					inRange = false;
					//message += "\tOutside Range";
				}
				startBar = CurrentBar;
			}
			if (BarsInProgress == 0 && ToTime(Time[0]) == endTime ) { 
				rthBarCount = CurrentBar - startBar;
				Y_High = MAX(High, rthBarCount)[0];
				Y_Low = MIN(Low, rthBarCount)[0];
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="RTHopen", Order=1, GroupName="Parameters")]
		public DateTime RTHopen
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="RTHclose", Order=2, GroupName="Parameters")]
		public DateTime RTHclose
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Use Points", Order=3, GroupName="Parameters")]
		public bool UsePoints
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Contracts", Order=3, GroupName="Parameters")]
		public int Contracts
		{ get; set; }
		
		[NinjaScriptProperty]
		[TypeConverter(typeof(AccountConverter))]
		public string AccountNames
		{get;set;}
		
	
		public class AccountConverter : TypeConverter
	    {
		
		   public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		   {
			   if (context == null)
			   {
				   return null;
			   }
	
			   List <string> list = new List <string> ();
			
		
			   foreach (Account sampleAccount in Account.All)
			   {
				   list.Add(sampleAccount.Name);
			   }
				
			
			   return new TypeConverter.StandardValuesCollection(list);
		   }
		
	

		   public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		   {
			   return true;
		   }
	    }
	}
	#endregion

}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DatePnL[] cacheDatePnL;
		public DatePnL DatePnL(DateTime rTHopen, DateTime rTHclose, bool usePoints, int contracts, string accountNames)
		{
			return DatePnL(Input, rTHopen, rTHclose, usePoints, contracts, accountNames);
		}

		public DatePnL DatePnL(ISeries<double> input, DateTime rTHopen, DateTime rTHclose, bool usePoints, int contracts, string accountNames)
		{
			if (cacheDatePnL != null)
				for (int idx = 0; idx < cacheDatePnL.Length; idx++)
					if (cacheDatePnL[idx] != null && cacheDatePnL[idx].RTHopen == rTHopen && cacheDatePnL[idx].RTHclose == rTHclose && cacheDatePnL[idx].UsePoints == usePoints && cacheDatePnL[idx].Contracts == contracts && cacheDatePnL[idx].AccountNames == accountNames && cacheDatePnL[idx].EqualsInput(input))
						return cacheDatePnL[idx];
			return CacheIndicator<DatePnL>(new DatePnL(){ RTHopen = rTHopen, RTHclose = rTHclose, UsePoints = usePoints, Contracts = contracts, AccountNames = accountNames }, input, ref cacheDatePnL);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DatePnL DatePnL(DateTime rTHopen, DateTime rTHclose, bool usePoints, int contracts, string accountNames)
		{
			return indicator.DatePnL(Input, rTHopen, rTHclose, usePoints, contracts, accountNames);
		}

		public Indicators.DatePnL DatePnL(ISeries<double> input , DateTime rTHopen, DateTime rTHclose, bool usePoints, int contracts, string accountNames)
		{
			return indicator.DatePnL(input, rTHopen, rTHclose, usePoints, contracts, accountNames);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DatePnL DatePnL(DateTime rTHopen, DateTime rTHclose, bool usePoints, int contracts, string accountNames)
		{
			return indicator.DatePnL(Input, rTHopen, rTHclose, usePoints, contracts, accountNames);
		}

		public Indicators.DatePnL DatePnL(ISeries<double> input , DateTime rTHopen, DateTime rTHclose, bool usePoints, int contracts, string accountNames)
		{
			return indicator.DatePnL(input, rTHopen, rTHclose, usePoints, contracts, accountNames);
		}
	}
}

#endregion
