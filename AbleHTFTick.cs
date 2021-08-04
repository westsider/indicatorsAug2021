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
	public class AbleHTFTick : Indicator
	{
		double LowerHTF = 0.0;
		double UpperHTF = 0.0; 
		
		/// Background Brushes
		private double iBrushOpacity = 0.1;
		private Brush iBrushBackUp = new SolidColorBrush(Colors.DarkBlue);
		private Brush iBrushBackDown = new SolidColorBrush(Colors.DarkRed);
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "Able HTF Tick";
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
				
				ColorBackground								= true;
				Opacity										= 10;
				UpColor										= Brushes.DodgerBlue;
				DnColor										= Brushes.Red;
				
				AddPlot(new Stroke(UpColor, 2), PlotStyle.Dot, "Upper");
				AddPlot(new Stroke(DnColor, 2), PlotStyle.Dot, "Lower");
			}
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Tick, 4500); 
				iBrushBackUp.Opacity = iBrushOpacity;
				iBrushBackUp.Freeze();
				iBrushBackDown.Opacity = iBrushOpacity;
				iBrushBackDown.Freeze();
			}
		}

		
		// option to color bkg - bool
		// up color, dn color
		// opacity
		
		protected override void OnBarUpdate()
		{ 
			if ( CurrentBar < 20 ) { return; }
			
			if (CurrentBars[0] < 20)
			return;

			// set up higher time frame
			foreach(int CurrentBarI in CurrentBars)
			{
				if (CurrentBarI < BarsRequiredToPlot)
				{
					return;
				}
			}
			
			// HTF bars
			if (BarsInProgress == 1)
			{
				LowerHTF = AblesysT2(2, 12, 14).Lower[0];
				UpperHTF = AblesysT2(2, 12, 14).Upper[0]; 
			}	
			
			// lower time frame bars
			if (BarsInProgress == 0)
			{
				if ( LowerHTF != 0 )		// short signal
				{
					Upper[0] = LowerHTF; 
					if ( ColorBackground )
						BackBrush = iBrushBackDown;
				}	
				
				if ( UpperHTF != 0 )		// Long signal
				{
					Lower[0] = UpperHTF; 
					if ( ColorBackground )
						BackBrush = iBrushBackUp;
				}
			}
			
		}

		#region Properties

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Upper
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Lower
		{
			get { return Values[1]; }
		}
		 
		[NinjaScriptProperty]
		[Display(Name="ColorBackground", Order=1, GroupName="Parameters")]
		public bool ColorBackground
		{ get; set; }
 

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Background Opacity", Order=2, GroupName="Parameters")]
		public int Opacity
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="UpColor", Order=3, GroupName="Parameters")]
		public Brush UpColor
		{ get; set; }

		[Browsable(false)]
		public string UpColorSerializable
		{
			get { return Serialize.BrushToString(UpColor); }
			set { UpColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="DnColor", Order=4, GroupName="Parameters")]
		public Brush DnColor
		{ get; set; }

		[Browsable(false)]
		public string DnColorSerializable
		{
			get { return Serialize.BrushToString(DnColor); }
			set { DnColor = Serialize.StringToBrush(value); }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AbleHTFTick[] cacheAbleHTFTick;
		public AbleHTFTick AbleHTFTick(bool colorBackground, int opacity, Brush upColor, Brush dnColor)
		{
			return AbleHTFTick(Input, colorBackground, opacity, upColor, dnColor);
		}

		public AbleHTFTick AbleHTFTick(ISeries<double> input, bool colorBackground, int opacity, Brush upColor, Brush dnColor)
		{
			if (cacheAbleHTFTick != null)
				for (int idx = 0; idx < cacheAbleHTFTick.Length; idx++)
					if (cacheAbleHTFTick[idx] != null && cacheAbleHTFTick[idx].ColorBackground == colorBackground && cacheAbleHTFTick[idx].Opacity == opacity && cacheAbleHTFTick[idx].UpColor == upColor && cacheAbleHTFTick[idx].DnColor == dnColor && cacheAbleHTFTick[idx].EqualsInput(input))
						return cacheAbleHTFTick[idx];
			return CacheIndicator<AbleHTFTick>(new AbleHTFTick(){ ColorBackground = colorBackground, Opacity = opacity, UpColor = upColor, DnColor = dnColor }, input, ref cacheAbleHTFTick);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AbleHTFTick AbleHTFTick(bool colorBackground, int opacity, Brush upColor, Brush dnColor)
		{
			return indicator.AbleHTFTick(Input, colorBackground, opacity, upColor, dnColor);
		}

		public Indicators.AbleHTFTick AbleHTFTick(ISeries<double> input , bool colorBackground, int opacity, Brush upColor, Brush dnColor)
		{
			return indicator.AbleHTFTick(input, colorBackground, opacity, upColor, dnColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AbleHTFTick AbleHTFTick(bool colorBackground, int opacity, Brush upColor, Brush dnColor)
		{
			return indicator.AbleHTFTick(Input, colorBackground, opacity, upColor, dnColor);
		}

		public Indicators.AbleHTFTick AbleHTFTick(ISeries<double> input , bool colorBackground, int opacity, Brush upColor, Brush dnColor)
		{
			return indicator.AbleHTFTick(input, colorBackground, opacity, upColor, dnColor);
		}
	}
}

#endregion
