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
	public class Gap : Indicator
	{
		private double Open_D = 0.0;
		private double Close_D = 0.0;
		private double Gap_D = 0.0;
		private string message = "no message";
		private long startTime = 0;
		private	long endTime = 0;
		private int startBar = 0;
		private int lastBar = 0;
		private int rthBarCount = 0;
		private double Y_High = 0.0;
		private double Y_Low = 0.0;
		private bool inRange = false;
		private bool crossed = false;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "Gap";
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
				RTHopen						= DateTime.Parse("08:30", System.Globalization.CultureInfo.InvariantCulture);
				RTHclose					= DateTime.Parse("15:00", System.Globalization.CultureInfo.InvariantCulture);
				
				ShowOpen					= true;
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
			
			lastBar = CurrentBar - 1;
			
			// open
			if (BarsInProgress == 1 && ToTime(Time[0]) == startTime ) { 
				Open_D = Open[0];
				Gap_D = Open_D - Close_D;
				message =  Time[0].ToShortDateString() + " "  + Time[0].ToShortTimeString() + "   Open: " + Open_D.ToString() +  "   Gap: " + Gap_D.ToString();
				//if ( inRange ) { message += "\t In Range";} else {  message += "\t Outside Range";}
				Print(message);
				crossed = false;
			}
			
			/// close
			if (BarsInProgress == 1 && ToTime(Time[0]) == endTime ) { 
				Close_D = Close[0];
				//Print(Time[0].ToShortDateString() + " \t" + Time[0].ToShortTimeString() + "\t close: " + Close_D.ToString());
				//Draw.Dot(this, "close"+CurrentBar, false, 0, Close_D, Brushes.White);
				crossed = false;
			}
			
			/// pre market gap
			if (BarsInProgress == 1 && ToTime(Time[0]) < startTime ) { 
				Gap_D = Close[0] - Close_D;
				message =  Time[0].ToShortDateString() + " \t"  + Time[0].ToShortTimeString() +  " \t Pre M Gap: " + Gap_D.ToString();
				//Print(message);
			}
			
			yesterdaysRange(debug: false);
			
			// after open
			if (BarsInProgress == 1 && ToTime(Time[0]) > startTime ) { 
				message =  Time[0].ToShortDateString() + " "  + Time[0].ToShortTimeString() + "   Open: " + Open_D.ToString() +  "   Gap: " + Gap_D.ToString();
				//if ( inRange ) { message += "  In Range";} else {  message += "  Outside Range";}
			}
			Draw.TextFixed(this, "MyTextFixed", message, TextPosition.TopLeft);
			
			if (BarsInProgress == 1 && ToTime(Time[0]) > startTime  && ToTime(Time[0]) < endTime && !crossed && ShowOpen) { 
				rthBarCount = CurrentBar - startBar;
				RemoveDrawObject("open" + lastBar); 
				Draw.Line(this, "open" + CurrentBar, false, rthBarCount, Open_D, 0, Open_D, Brushes.DimGray, DashStyleHelper.Dot, 2);
				var ibEnd = startTime + 10000;
				Print(startTime + " " + ibEnd );
				if (High[0] >= Open_D && Low[0] <= Open_D) {
					if (ToTime(Time[0]) > ibEnd) {
						Draw.Dot(this, "cross" + CurrentBar, false, 0, High[0], Brushes.DodgerBlue);
						crossed = true;
					}
				}
			}
			
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
		[Display(Name="Show Open", Order=3, GroupName="Parameters")]
		public bool ShowOpen
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Gap[] cacheGap;
		public Gap Gap(DateTime rTHopen, DateTime rTHclose, bool showOpen)
		{
			return Gap(Input, rTHopen, rTHclose, showOpen);
		}

		public Gap Gap(ISeries<double> input, DateTime rTHopen, DateTime rTHclose, bool showOpen)
		{
			if (cacheGap != null)
				for (int idx = 0; idx < cacheGap.Length; idx++)
					if (cacheGap[idx] != null && cacheGap[idx].RTHopen == rTHopen && cacheGap[idx].RTHclose == rTHclose && cacheGap[idx].ShowOpen == showOpen && cacheGap[idx].EqualsInput(input))
						return cacheGap[idx];
			return CacheIndicator<Gap>(new Gap(){ RTHopen = rTHopen, RTHclose = rTHclose, ShowOpen = showOpen }, input, ref cacheGap);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Gap Gap(DateTime rTHopen, DateTime rTHclose, bool showOpen)
		{
			return indicator.Gap(Input, rTHopen, rTHclose, showOpen);
		}

		public Indicators.Gap Gap(ISeries<double> input , DateTime rTHopen, DateTime rTHclose, bool showOpen)
		{
			return indicator.Gap(input, rTHopen, rTHclose, showOpen);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Gap Gap(DateTime rTHopen, DateTime rTHclose, bool showOpen)
		{
			return indicator.Gap(Input, rTHopen, rTHclose, showOpen);
		}

		public Indicators.Gap Gap(ISeries<double> input , DateTime rTHopen, DateTime rTHclose, bool showOpen)
		{
			return indicator.Gap(input, rTHopen, rTHclose, showOpen);
		}
	}
}

#endregion
