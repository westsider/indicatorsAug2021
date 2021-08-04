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
	public class AllInputs : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "AllInputs";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				ABool					= true;
				ADouble					= 1;
				AInt					= 1;
				AString					= string.Empty;
				ATime						= DateTime.Parse("11:13", System.Globalization.CultureInfo.InvariantCulture);
				ABrush					= Brushes.Orange;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="ABool", Order=1, GroupName="Parameters")]
		public bool ABool
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="ADouble", Order=2, GroupName="Parameters")]
		public double ADouble
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="AInt", Order=3, GroupName="Parameters")]
		public int AInt
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="AString", Order=4, GroupName="Parameters")]
		public string AString
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="ATime", Order=5, GroupName="Parameters")]
		public DateTime ATime
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="ABrush", Order=6, GroupName="Parameters")]
		public Brush ABrush
		{ get; set; }

		[Browsable(false)]
		public string ABrushSerializable
		{
			get { return Serialize.BrushToString(ABrush); }
			set { ABrush = Serialize.StringToBrush(value); }
		}			
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AllInputs[] cacheAllInputs;
		public AllInputs AllInputs(bool aBool, double aDouble, int aInt, string aString, DateTime aTime, Brush aBrush)
		{
			return AllInputs(Input, aBool, aDouble, aInt, aString, aTime, aBrush);
		}

		public AllInputs AllInputs(ISeries<double> input, bool aBool, double aDouble, int aInt, string aString, DateTime aTime, Brush aBrush)
		{
			if (cacheAllInputs != null)
				for (int idx = 0; idx < cacheAllInputs.Length; idx++)
					if (cacheAllInputs[idx] != null && cacheAllInputs[idx].ABool == aBool && cacheAllInputs[idx].ADouble == aDouble && cacheAllInputs[idx].AInt == aInt && cacheAllInputs[idx].AString == aString && cacheAllInputs[idx].ATime == aTime && cacheAllInputs[idx].ABrush == aBrush && cacheAllInputs[idx].EqualsInput(input))
						return cacheAllInputs[idx];
			return CacheIndicator<AllInputs>(new AllInputs(){ ABool = aBool, ADouble = aDouble, AInt = aInt, AString = aString, ATime = aTime, ABrush = aBrush }, input, ref cacheAllInputs);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AllInputs AllInputs(bool aBool, double aDouble, int aInt, string aString, DateTime aTime, Brush aBrush)
		{
			return indicator.AllInputs(Input, aBool, aDouble, aInt, aString, aTime, aBrush);
		}

		public Indicators.AllInputs AllInputs(ISeries<double> input , bool aBool, double aDouble, int aInt, string aString, DateTime aTime, Brush aBrush)
		{
			return indicator.AllInputs(input, aBool, aDouble, aInt, aString, aTime, aBrush);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AllInputs AllInputs(bool aBool, double aDouble, int aInt, string aString, DateTime aTime, Brush aBrush)
		{
			return indicator.AllInputs(Input, aBool, aDouble, aInt, aString, aTime, aBrush);
		}

		public Indicators.AllInputs AllInputs(ISeries<double> input , bool aBool, double aDouble, int aInt, string aString, DateTime aTime, Brush aBrush)
		{
			return indicator.AllInputs(input, aBool, aDouble, aInt, aString, aTime, aBrush);
		}
	}
}

#endregion
