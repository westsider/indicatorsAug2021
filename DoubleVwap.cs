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
	public class DoubleVwap : Indicator
	{
		private NinjaTrader.NinjaScript.Indicators.LizardIndicators.amaCurrentDayVWAP amaCurrentDayVWAP1;
		private NinjaTrader.NinjaScript.Indicators.LizardIndicators.amaCurrentWeekVWAP amaCurrentWeekVWAP1;
		private	long endTime = 0;
		private long startTime = 0;
		private int LastBar = 0;
		
		private int LowCounter = 0;
		private double LowLineValue = 0.0;
		private int HighCounter = 0;
		private double HighLineValue = 0.0;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "DoubleVwap";
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
				
				RTHopen						= DateTime.Parse("06:55", System.Globalization.CultureInfo.InvariantCulture);
				RTHclose					= DateTime.Parse("13:00", System.Globalization.CultureInfo.InvariantCulture);
			}
			else if (State == State.Configure)
			{
				ClearOutputWindow();
				startTime = long.Parse(RTHopen.ToString("HHmmss"));
			 	endTime = long.Parse(RTHclose.ToString("HHmmss"));
			}
			else if (State == State.DataLoaded)
			{				
				amaCurrentDayVWAP1				= amaCurrentDayVWAP(Close, amaSessionTypeVWAPD.Custom_Hours, amaBandTypeVWAPD.Standard_Deviation, amaTimeZonesVWAPD.Chart_Time, @"06:30", @"13:10", 1, 2, 3, 1, 2, 3);
				amaCurrentWeekVWAP1				= amaCurrentWeekVWAP(Close, amaSessionTypeVWAPW.Full_Session, amaBandTypeVWAPW.Standard_Deviation, amaTimeZonesVWAPW.Chart_Time, @"00:00", @"24:00", 1, 2, 3, 1, 2, 3);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 5) 
				return;
			
			LastBar = CurrentBar - 1;
			
			if ( ToTime(Time[0]) >= endTime || ToTime(Time[0]) <= startTime  ) {
				HighCounter = 0;
				return; 
			}
			//Print(Time[0].ToShortDateString() + " " +  amaCurrentWeekVWAP1.LowerBand2[0]  );

			///  less than daily value area
			if (Low[0] <= amaCurrentDayVWAP1.LowerBand1[0] ) { 
				//Draw.Dot(this, "BelowDaily"+CurrentBar, false, 0, High[0], Brushes.Yellow);
				/// less than weekly sdt dev 2
				if ( Close[0] <= amaCurrentWeekVWAP1.LowerBand2[0] ) {
					DrawRangeLow();
				} else {
					LowCounter = 0;
					LowLineValue = 1000000.0;
				}
			} 
			
			///  greater than daily value area
			if (High[0] >= amaCurrentDayVWAP1.UpperBand1[0] ) { 
				//Draw.Dot(this, "BelowDaily"+CurrentBar, false, 0, High[0], Brushes.Yellow);
				/// less than weekly sdt dev 2
				if ( High[0] >= amaCurrentWeekVWAP1.UpperBand2[0] ) {
					//Draw.Dot(this, "BelowWeekly"+CurrentBar, false, 0, High[0], Brushes.Magenta);
					DrawRangeHigh();
				} else {
					HighCounter = 0;
					HighLineValue = 0.0;
				}
			} 
			
			
			
		}
		
		private void DrawRangeLow() { 
			if ( Low[0] < LowLineValue ) { LowLineValue = Low[0]; }
			RemoveDrawObject("LowLine"+LastBar);
			int Length =  LowCounter;
			Draw.Line(this, "LowLine"+CurrentBar, Length, LowLineValue, 0, LowLineValue, Brushes.White);
			LowCounter += 1;
			if( LowCounter == 1)
			{
				Draw.Dot(this, "BelowWeekly"+CurrentBar, false, 0, Low[0], Brushes.Cyan);
			}
		}
		
		private void DrawRangeHigh() { 
			if ( High[0] > HighLineValue ) { HighLineValue = High[0]; }
			RemoveDrawObject("HighLine"+LastBar);
			int Length =  HighCounter;
			Draw.Line(this, "HighLine"+CurrentBar, Length, HighLineValue, 0, HighLineValue, Brushes.White);
			HighCounter += 1;
			if( HighCounter == 1)
			{
				Draw.Dot(this, "AboveWeekly"+CurrentBar, false, 0, High[0], Brushes.Magenta);
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
		#endregion
	}
	

}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DoubleVwap[] cacheDoubleVwap;
		public DoubleVwap DoubleVwap(DateTime rTHopen, DateTime rTHclose)
		{
			return DoubleVwap(Input, rTHopen, rTHclose);
		}

		public DoubleVwap DoubleVwap(ISeries<double> input, DateTime rTHopen, DateTime rTHclose)
		{
			if (cacheDoubleVwap != null)
				for (int idx = 0; idx < cacheDoubleVwap.Length; idx++)
					if (cacheDoubleVwap[idx] != null && cacheDoubleVwap[idx].RTHopen == rTHopen && cacheDoubleVwap[idx].RTHclose == rTHclose && cacheDoubleVwap[idx].EqualsInput(input))
						return cacheDoubleVwap[idx];
			return CacheIndicator<DoubleVwap>(new DoubleVwap(){ RTHopen = rTHopen, RTHclose = rTHclose }, input, ref cacheDoubleVwap);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DoubleVwap DoubleVwap(DateTime rTHopen, DateTime rTHclose)
		{
			return indicator.DoubleVwap(Input, rTHopen, rTHclose);
		}

		public Indicators.DoubleVwap DoubleVwap(ISeries<double> input , DateTime rTHopen, DateTime rTHclose)
		{
			return indicator.DoubleVwap(input, rTHopen, rTHclose);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DoubleVwap DoubleVwap(DateTime rTHopen, DateTime rTHclose)
		{
			return indicator.DoubleVwap(Input, rTHopen, rTHclose);
		}

		public Indicators.DoubleVwap DoubleVwap(ISeries<double> input , DateTime rTHopen, DateTime rTHclose)
		{
			return indicator.DoubleVwap(input, rTHopen, rTHclose);
		}
	}
}

#endregion
