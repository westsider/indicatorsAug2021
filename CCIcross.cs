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
	public class CCIcross : Indicator
	{
		private CCI CCI1;
		private NinjaTrader.NinjaScript.Indicators.LizardIndicators.amaCurrentWeekVWAP amaCurrentWeekVWAP1;
		private int filterVwapSetting = 2;
		
		private double threshold = 100.0;
		private double threshold2 = 200.0;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "CCI Cross";
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
				AlertOn										= true;
				AlertSOund									= @"smsAlert1.wav";
				FilterVwap									= false;
				FilterStdDev1								= true;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				CCI1				= CCI(Close, 14);
				amaCurrentWeekVWAP1				= amaCurrentWeekVWAP(Close, amaSessionTypeVWAPW.Full_Session, amaBandTypeVWAPW.Standard_Deviation, amaTimeZonesVWAPW.Exchange_Time, @"08:30", @"15:15", 1, 2, 3, 1, 2, 3);
			}
		}

		protected override void OnBarUpdate()
		{
			/// Long Entry
			if (CrossAbove(CCI1, -threshold, 1))
			{
				// filter 1 long only below vwap
				if (Low[0] <=  amaCurrentWeekVWAP1.SessionVWAP[0] && FilterVwap) {
					Draw.TriangleUp(this, "buythis"+CurrentBar, false, 0, Low[0] - 2 * TickSize, Brushes.Blue);
					sendAlert(message: "yo", sound: AlertSOund );
				}
				// filter 2 short onli above std dev 1
				if (Low[0] <=  amaCurrentWeekVWAP1.LowerBand1[0] && FilterStdDev1) {
					Draw.TriangleUp(this, "buyin"+CurrentBar, false, 0, Low[0] - 2 * TickSize, Brushes.Blue);
					sendAlert(message: "yo", sound: AlertSOund );
				}
				
			}
			
			if (CrossBelow(CCI1, threshold, 1))
			{
				if (High[0] >=  amaCurrentWeekVWAP1.SessionVWAP[0] && FilterVwap ) {
					Draw.TriangleDown(this, "sellin"+CurrentBar, false, 0, High[0] + 2 * TickSize, Brushes.Red);
					sendAlert(message: "yo", sound: AlertSOund );
				}
				if (High[0] >=  amaCurrentWeekVWAP1.UpperBand1[0] && FilterStdDev1) {
					Draw.TriangleDown(this, "sellit"+CurrentBar, false, 0, High[0] + 2 * TickSize, Brushes.Red);
					sendAlert(message: "yo", sound: AlertSOund );
				}
			}
			
		}
		
		private void sendAlert(string message, string sound ) {
			if (CurrentBar < Count -2 || !AlertOn) return;
			message += " " + Bars.Instrument.MasterInstrument.Name;
			Alert("myAlert"+CurrentBar, Priority.High, message, NinjaTrader.Core.Globals.InstallDir+@"\sounds\"+ AlertSOund,10, Brushes.Black, Brushes.Yellow);  
//			if ( AlertOn ) {
//				SendMail("whansen1@mac.com", "Trade Alert", message + " " );
//			}	
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="AlertOn", Order=1, GroupName="Parameters")]
		public bool AlertOn
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="AlertSOund", Order=2, GroupName="Parameters")]
		public string AlertSOund
		{ get; set; }
		 
		[NinjaScriptProperty]
		[Display(Name="Filter Vwap", Order=3, GroupName="Parameters")]
		public bool FilterVwap
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Filter StdDev1", Order=4, GroupName="Parameters")]
		public bool FilterStdDev1
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CCIcross[] cacheCCIcross;
		public CCIcross CCIcross(bool alertOn, string alertSOund, bool filterVwap, bool filterStdDev1)
		{
			return CCIcross(Input, alertOn, alertSOund, filterVwap, filterStdDev1);
		}

		public CCIcross CCIcross(ISeries<double> input, bool alertOn, string alertSOund, bool filterVwap, bool filterStdDev1)
		{
			if (cacheCCIcross != null)
				for (int idx = 0; idx < cacheCCIcross.Length; idx++)
					if (cacheCCIcross[idx] != null && cacheCCIcross[idx].AlertOn == alertOn && cacheCCIcross[idx].AlertSOund == alertSOund && cacheCCIcross[idx].FilterVwap == filterVwap && cacheCCIcross[idx].FilterStdDev1 == filterStdDev1 && cacheCCIcross[idx].EqualsInput(input))
						return cacheCCIcross[idx];
			return CacheIndicator<CCIcross>(new CCIcross(){ AlertOn = alertOn, AlertSOund = alertSOund, FilterVwap = filterVwap, FilterStdDev1 = filterStdDev1 }, input, ref cacheCCIcross);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CCIcross CCIcross(bool alertOn, string alertSOund, bool filterVwap, bool filterStdDev1)
		{
			return indicator.CCIcross(Input, alertOn, alertSOund, filterVwap, filterStdDev1);
		}

		public Indicators.CCIcross CCIcross(ISeries<double> input , bool alertOn, string alertSOund, bool filterVwap, bool filterStdDev1)
		{
			return indicator.CCIcross(input, alertOn, alertSOund, filterVwap, filterStdDev1);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CCIcross CCIcross(bool alertOn, string alertSOund, bool filterVwap, bool filterStdDev1)
		{
			return indicator.CCIcross(Input, alertOn, alertSOund, filterVwap, filterStdDev1);
		}

		public Indicators.CCIcross CCIcross(ISeries<double> input , bool alertOn, string alertSOund, bool filterVwap, bool filterStdDev1)
		{
			return indicator.CCIcross(input, alertOn, alertSOund, filterVwap, filterStdDev1);
		}
	}
}

#endregion
