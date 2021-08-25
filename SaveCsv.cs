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
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class SaveCsv : Indicator
	{
		private NinjaTrader.NinjaScript.Indicators.LizardIndicators.amaCurrentDayVWAP amaCurrentDayVWAP1;
		private string path;
		private StreamWriter sw;
		private double upper = 0.0;
		private double lower = 0.0;
		private double upper2 = 0.0;
		private double lower2 = 0.0;
		private double center = 0.0;
		private double Open_D = 0.0;
		private double Close_D = 0.0;
		private long startTime = 0;
		private	long endTime = 0;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "Save Csv";
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
				Start						= DateTime.Parse("08:30", System.Globalization.CultureInfo.InvariantCulture);
				End						= DateTime.Parse("15:00", System.Globalization.CultureInfo.InvariantCulture);
				SendAlert					= true;
				ShowPopUp					= true;
				FileName					= @"Bands";
				path						= NinjaTrader.Core.Globals.UserDataDir + FileName + ".csv";
				AString										= "smsAlert2.wav";
			}
			else if (State == State.Configure)
			{
				startTime = long.Parse(Start.ToString("HHmmss"));
			 	endTime = long.Parse(End.ToString("HHmmss"));
				AddDataSeries(Data.BarsPeriodType.Minute, 1);
				ClearOutputWindow();
			}
			else if (State == State.DataLoaded)
			{				
				amaCurrentDayVWAP1				= amaCurrentDayVWAP(Close, amaSessionTypeVWAPD.Custom_Hours, amaBandTypeVWAPD.Standard_Deviation, amaTimeZonesVWAPD.Exchange_Time, @"08:30", @"15:15", 1, 2, 3, 1, 2, 3);
			}
			else if (State == State.Terminated)
			{
				if (sw != null)
				{
					sw.Close();
					sw.Dispose();
					sw = null;
				}
			}
			
		}

		protected override void OnBarUpdate()
		{
			if ( CurrentBar < 20 ) { return; }
			GetOpenClose();
			GetBands(); 
			SaveToFile(open: Open_D, close: Close_D, lower: lower, lower2: lower2, upper: upper, upper2: upper2);
			
			if ( !SendAlert ) { return; }
			if (Low[0] <= lower ) {
				sendAlert( message: "buy", sound: AString);
			}
			
			if (High[0] >= upper ) {
				sendAlert( message: "sell", sound: AString);
			}
		}
		
		private void sendAlert(string message, string sound ) {
			if (CurrentBar < Count -2) return;
			message += " " + Bars.Instrument.MasterInstrument.Name;
			Alert("myAlert"+CurrentBar, Priority.High, message, NinjaTrader.Core.Globals.InstallDir+@"\sounds\"+ sound,10, Brushes.Black, Brushes.Yellow);  
			if ( SendAlert ) {
				SendMail("whansen1@mac.com", "Trade Alert", message + " " + Open_D);
			}
			
		}
		
		void GetBands() 
		{
//			double tests = amaCurrentDayVWAP(Close, amaSessionTypeVWAPD.Custom_Hours, amaBandTypeVWAPD.Standard_Deviation, amaTimeZonesVWAPD.Exchange_Time, @"08:30", @"15:15", 1, 2, 3, 1, 2, 3).UpperBand1[0];
//			Print(tests);
			
//			if ( amaCurrentDayVWAP1.Value.IsValidDataPoint(0)) {
				upper = Adjust(amaCurrentDayVWAP(Close, amaSessionTypeVWAPD.Custom_Hours, amaBandTypeVWAPD.Standard_Deviation, amaTimeZonesVWAPD.Exchange_Time, @"08:30", @"15:15", 1, 2, 3, 1, 2, 3).UpperBand2[0]); 
				upper2 = Adjust(amaCurrentDayVWAP(Close, amaSessionTypeVWAPD.Custom_Hours, amaBandTypeVWAPD.Standard_Deviation, amaTimeZonesVWAPD.Exchange_Time, @"08:30", @"15:15", 1, 2, 3, 1, 2, 3).UpperBand3[0]); 
				lower = Adjust(amaCurrentDayVWAP(Close, amaSessionTypeVWAPD.Custom_Hours, amaBandTypeVWAPD.Standard_Deviation, amaTimeZonesVWAPD.Exchange_Time, @"08:30", @"15:15", 1, 2, 3, 1, 2, 3).LowerBand2[0]); 
				lower2 = Adjust(amaCurrentDayVWAP(Close, amaSessionTypeVWAPD.Custom_Hours, amaBandTypeVWAPD.Standard_Deviation, amaTimeZonesVWAPD.Exchange_Time, @"08:30", @"15:15", 1, 2, 3, 1, 2, 3).LowerBand3[0]);
				center = Adjust(amaCurrentDayVWAP(Close, amaSessionTypeVWAPD.Custom_Hours, amaBandTypeVWAPD.Standard_Deviation, amaTimeZonesVWAPD.Exchange_Time, @"08:30", @"15:15", 1, 2, 3, 1, 2, 3).SessionVWAP[0]);
//			}
		}
		
		void SaveToFile(double open, double close, double lower, double lower2, double upper, double upper2) 
		{ 
			sw = File.CreateText(path);
			sw.WriteLine(open + ", OPEN" );
			sw.WriteLine(close + ", YCLOSE" );
			sw.WriteLine(lower + ", ------" );
			sw.WriteLine(lower2 + ", =====" );
			sw.WriteLine(center + ", VWAP" );
			sw.WriteLine(upper + ", ------" ); 
			sw.WriteLine(upper2 + ", =====" ); 
			sw.Close();
			//Print(open + ", OPEN, " + close + ", CLOSE, " + upper + ", ----, " + lower + ", ----");
		}
		
		void GetOpenClose() {
			if (BarsInProgress == 1 && ToTime(Time[0]) == startTime ) { 
				Open_D = Open[0];
			}
			if (BarsInProgress == 1 && ToTime(Time[0]) == endTime ) { 
				Close_D = Close[0]; 				
			}
		}
		
		double Adjust(double input)
		{
		    double whole = Math.Truncate(input);
		    double remainder = input - whole;
		    if (remainder < 0.24)
		    {
		        remainder = 0;
		    }
		    else if (remainder < 0.49)
		    {
		        remainder = 0.25;
		    }
			else if (remainder < 0.74)
		    {
		        remainder = 0.5;
		    }
			else if (remainder < 0.99)
		    {
		        remainder = 0.75;
		    }
		    else
		    {
		        remainder = 1;
		    }
		    return whole + remainder;
		}

		#region Properties
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start", Order=1, GroupName="Parameters")]
		public DateTime Start
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="End", Order=2, GroupName="Parameters")]
		public DateTime End
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="SendAlert", Order=3, GroupName="Parameters")]
		public bool SendAlert
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowPopUp", Order=4, GroupName="Parameters")]
		public bool ShowPopUp
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="FileName", Order=5, GroupName="Parameters")]
		public string FileName
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="Sound Name", Order=6, GroupName="Parameters")]
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
		private SaveCsv[] cacheSaveCsv;
		public SaveCsv SaveCsv(DateTime start, DateTime end, bool sendAlert, bool showPopUp, string fileName, string aString)
		{
			return SaveCsv(Input, start, end, sendAlert, showPopUp, fileName, aString);
		}

		public SaveCsv SaveCsv(ISeries<double> input, DateTime start, DateTime end, bool sendAlert, bool showPopUp, string fileName, string aString)
		{
			if (cacheSaveCsv != null)
				for (int idx = 0; idx < cacheSaveCsv.Length; idx++)
					if (cacheSaveCsv[idx] != null && cacheSaveCsv[idx].Start == start && cacheSaveCsv[idx].End == end && cacheSaveCsv[idx].SendAlert == sendAlert && cacheSaveCsv[idx].ShowPopUp == showPopUp && cacheSaveCsv[idx].FileName == fileName && cacheSaveCsv[idx].AString == aString && cacheSaveCsv[idx].EqualsInput(input))
						return cacheSaveCsv[idx];
			return CacheIndicator<SaveCsv>(new SaveCsv(){ Start = start, End = end, SendAlert = sendAlert, ShowPopUp = showPopUp, FileName = fileName, AString = aString }, input, ref cacheSaveCsv);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SaveCsv SaveCsv(DateTime start, DateTime end, bool sendAlert, bool showPopUp, string fileName, string aString)
		{
			return indicator.SaveCsv(Input, start, end, sendAlert, showPopUp, fileName, aString);
		}

		public Indicators.SaveCsv SaveCsv(ISeries<double> input , DateTime start, DateTime end, bool sendAlert, bool showPopUp, string fileName, string aString)
		{
			return indicator.SaveCsv(input, start, end, sendAlert, showPopUp, fileName, aString);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SaveCsv SaveCsv(DateTime start, DateTime end, bool sendAlert, bool showPopUp, string fileName, string aString)
		{
			return indicator.SaveCsv(Input, start, end, sendAlert, showPopUp, fileName, aString);
		}

		public Indicators.SaveCsv SaveCsv(ISeries<double> input , DateTime start, DateTime end, bool sendAlert, bool showPopUp, string fileName, string aString)
		{
			return indicator.SaveCsv(input, start, end, sendAlert, showPopUp, fileName, aString);
		}
	}
}

#endregion
