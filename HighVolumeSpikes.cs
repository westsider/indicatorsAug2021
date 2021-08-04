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
	public class HighVolumeSpikes : Indicator
	{
		private NinjaTrader.NinjaScript.Indicators.Acme.TickAnalysis.AcmeVolumeDelta AcmeVolumeDelta1;
		private bool buyCondition = false;
		private bool sellCondition = false;
		private bool volumeSpike = false;
		private double deltaThresh  = 200.0;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "High Volume Spikes";
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
				
				AString										= "smsAlert2.wav";
				Opacity		= 0.25;
			}
			else if (State == State.Configure)
			{
				ClearOutputWindow();
			}
			else if (State == State.DataLoaded)
			{				
				AcmeVolumeDelta1				= AcmeVolumeDelta(Close, NinjaTrader.NinjaScript.Indicators.Acme.TickAnalysis.AcmeVolumeDelta.Displays.PerBarDelta, NinjaTrader.NinjaScript.Indicators.Acme.TickAnalysis.AcmeVolumeDelta.EvaluationStrategies.BidAsk, NinjaTrader.NinjaScript.Indicators.Acme.TickAnalysis.AcmeVolumeDelta.FilterOperators.NoFilter, 0, true, 10, true, NinjaTrader.NinjaScript.Indicators.Acme.TickAnalysis.AcmeVolumeDelta.PerBarStyles.Candles);
			}
		}

		protected override void OnBarUpdate()
		{
			if ( CurrentBar < 10 ) { return; }
			volumeSpike = false;
			buyCondition = false;

		    //BackBrush = Brushes.Transparent;

			// look for vol spike
			double vol = Volume[0]; 
			double  maxVol = (double)MAX(Volume, 4)[1];
			
			if (vol > maxVol ) {
				//Draw.Dot(this, "VolSpike"+CurrentBar, false, 0, Low[0] - 2 * TickSize, Brushes.White);
				volumeSpike = true;
			}
			
			// look for delta spike
			double  maxDelta = (double)MAX(AcmeVolumeDelta1.Delta, 4)[1];
			double  minDelta = (double)MIN(AcmeVolumeDelta1.Delta, 4)[1];
			if (AcmeVolumeDelta1.Delta[0] < -deltaThresh  && AcmeVolumeDelta1.Delta[0] < minDelta)
			{
				//Draw.ArrowUp(this, "spikeDown"+CurrentBar, false, 0, Low[0] - (TickSize), Brushes.Green);
				buyCondition = true;
			}
			
			if (AcmeVolumeDelta1.Delta[0] > deltaThresh  && AcmeVolumeDelta1.Delta[0] > maxDelta)
			{
				//Draw.ArrowDown(this, "spikeUp"+CurrentBar, false, 0, High[0] + (TickSize), Brushes.Red);
				sellCondition = true;
			}
			
			if (volumeSpike && buyCondition) {
				 BackBrush  = new SolidColorBrush(Colors.DimGray) {Opacity = 0.25};
				 sendAlert(message: "Buy", sound: AString );
			} else if (volumeSpike && sellCondition) {
				 BackBrush  = new SolidColorBrush(Colors.DimGray) {Opacity = 0.25};
				 sendAlert(message: "Sell Approaching", sound: AString );
			}
			//BackBrush.Freeze();
		}
		
		private void sendAlert(string message, string sound ) {
			message += " " + Bars.Instrument.MasterInstrument.Name;
			Alert("myAlert"+CurrentBar, Priority.High, message, NinjaTrader.Core.Globals.InstallDir+@"\sounds\"+ sound,10, Brushes.Black, Brushes.Yellow);  
			if (CurrentBar < Count -2) return;
		}
		
		#region Properties
		[NinjaScriptProperty]
		[Range(0.05, double.MaxValue)]
		[Display(Name="Opacity", Order=2, GroupName="Parameters")]
		public double Opacity
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Sound Name", Order=3, GroupName="Parameters")]
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
		private HighVolumeSpikes[] cacheHighVolumeSpikes;
		public HighVolumeSpikes HighVolumeSpikes(double opacity, string aString)
		{
			return HighVolumeSpikes(Input, opacity, aString);
		}

		public HighVolumeSpikes HighVolumeSpikes(ISeries<double> input, double opacity, string aString)
		{
			if (cacheHighVolumeSpikes != null)
				for (int idx = 0; idx < cacheHighVolumeSpikes.Length; idx++)
					if (cacheHighVolumeSpikes[idx] != null && cacheHighVolumeSpikes[idx].Opacity == opacity && cacheHighVolumeSpikes[idx].AString == aString && cacheHighVolumeSpikes[idx].EqualsInput(input))
						return cacheHighVolumeSpikes[idx];
			return CacheIndicator<HighVolumeSpikes>(new HighVolumeSpikes(){ Opacity = opacity, AString = aString }, input, ref cacheHighVolumeSpikes);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.HighVolumeSpikes HighVolumeSpikes(double opacity, string aString)
		{
			return indicator.HighVolumeSpikes(Input, opacity, aString);
		}

		public Indicators.HighVolumeSpikes HighVolumeSpikes(ISeries<double> input , double opacity, string aString)
		{
			return indicator.HighVolumeSpikes(input, opacity, aString);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.HighVolumeSpikes HighVolumeSpikes(double opacity, string aString)
		{
			return indicator.HighVolumeSpikes(Input, opacity, aString);
		}

		public Indicators.HighVolumeSpikes HighVolumeSpikes(ISeries<double> input , double opacity, string aString)
		{
			return indicator.HighVolumeSpikes(input, opacity, aString);
		}
	}
}

#endregion
