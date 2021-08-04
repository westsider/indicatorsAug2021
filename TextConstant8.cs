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
	public class TextConstant8 : Indicator
	{
		private string message = "";
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "A Text On Chart";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				Line1					= @"one";
				Line2					= @"two";
				Line3					= @"three";
				Line4					= @"four";
				Line5					= @"fivee";
				Line6					= @"six";
				Line7					= @"seven";
				Line8					= @"eight";
				BackgroundOpacity = 70;
				NoteLocation								= TextPosition.TopLeft;
				BackgroundColor								= Brushes.RoyalBlue;
				FontColor									= Brushes.White;
				OutlineColor								= Brushes.DimGray;
				NoteFont									= new SimpleFont("Arial", 10);
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			if (Line1 != "") { message = Line1; }
			if (Line2 != "") { message += "\n" + Line2; }
			if (Line3 != "") { message += "\n" + Line3; }
			if (Line4 != "") { message += "\n" + Line4; }
			if (Line5 != "") { message += "\n" + Line5; }
			if (Line6 != "") { message += "\n" + Line6; }
			if (Line7 != "") { message += "\n" + Line7; }
			if (Line8 != "") { message += "\n" + Line8; }

			Draw.TextFixed(this, "myTextFixed", 
				message, 
				NoteLocation, 
				FontColor,  // text color
				NoteFont, 
				OutlineColor, // outline color
				BackgroundColor, 
				BackgroundOpacity); 

		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="Line 1", Order=1, GroupName="Parameters")]
		public string Line1
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Line 2", Order=2, GroupName="Parameters")]
		public string Line2
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Line 3", Order=3, GroupName="Parameters")]
		public string Line3
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Line 4", Order=4, GroupName="Parameters")]
		public string Line4
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Line 5", Order=5, GroupName="Parameters")]
		public string Line5
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Line 6", Order=6, GroupName="Parameters")]
		public string Line6
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Line 7", Order=7, GroupName="Parameters")]
		public string Line7
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Line 8", Order=8, GroupName="Parameters")]
		public string Line8
		{ get; set; }
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Background Color", Description="Background Color", Order=1, GroupName="Style")]
		public Brush BackgroundColor
		{ get; set; }

		[Browsable(false)]
		public string BackgroundColorSerializable
		{
			get { return Serialize.BrushToString(BackgroundColor); }
			set { BackgroundColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Font Color", Description="Font Color", Order=2, GroupName="Style")]
		public Brush FontColor
		{ get; set; }

		[Browsable(false)]
		public string FontColorSerializable
		{
			get { return Serialize.BrushToString(FontColor); }
			set { FontColor = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="OutlineColor Color", Description="OutlineColor Color", Order=3, GroupName="Style")]
		public Brush OutlineColor
		{ get; set; }

		[Browsable(false)]
		public string OutlineColorSerializable
		{
			get { return Serialize.BrushToString(OutlineColor); }
			set { OutlineColor = Serialize.StringToBrush(value); }
		}
		 
		[NinjaScriptProperty]
		[Display(Name="Note Font", Description="Note Font", Order=4, GroupName="Style")]
		public SimpleFont NoteFont
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Background Opacity", Description="Background Opacity", Order=5, GroupName="Style")]
		public int BackgroundOpacity
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Note1Location", Description="Note Location", Order=6, GroupName="Style")]
		public TextPosition NoteLocation
		{ get; set; }
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TextConstant8[] cacheTextConstant8;
		public TextConstant8 TextConstant8(string line1, string line2, string line3, string line4, string line5, string line6, string line7, string line8, Brush backgroundColor, Brush fontColor, Brush outlineColor, SimpleFont noteFont, int backgroundOpacity, TextPosition noteLocation)
		{
			return TextConstant8(Input, line1, line2, line3, line4, line5, line6, line7, line8, backgroundColor, fontColor, outlineColor, noteFont, backgroundOpacity, noteLocation);
		}

		public TextConstant8 TextConstant8(ISeries<double> input, string line1, string line2, string line3, string line4, string line5, string line6, string line7, string line8, Brush backgroundColor, Brush fontColor, Brush outlineColor, SimpleFont noteFont, int backgroundOpacity, TextPosition noteLocation)
		{
			if (cacheTextConstant8 != null)
				for (int idx = 0; idx < cacheTextConstant8.Length; idx++)
					if (cacheTextConstant8[idx] != null && cacheTextConstant8[idx].Line1 == line1 && cacheTextConstant8[idx].Line2 == line2 && cacheTextConstant8[idx].Line3 == line3 && cacheTextConstant8[idx].Line4 == line4 && cacheTextConstant8[idx].Line5 == line5 && cacheTextConstant8[idx].Line6 == line6 && cacheTextConstant8[idx].Line7 == line7 && cacheTextConstant8[idx].Line8 == line8 && cacheTextConstant8[idx].BackgroundColor == backgroundColor && cacheTextConstant8[idx].FontColor == fontColor && cacheTextConstant8[idx].OutlineColor == outlineColor && cacheTextConstant8[idx].NoteFont == noteFont && cacheTextConstant8[idx].BackgroundOpacity == backgroundOpacity && cacheTextConstant8[idx].NoteLocation == noteLocation && cacheTextConstant8[idx].EqualsInput(input))
						return cacheTextConstant8[idx];
			return CacheIndicator<TextConstant8>(new TextConstant8(){ Line1 = line1, Line2 = line2, Line3 = line3, Line4 = line4, Line5 = line5, Line6 = line6, Line7 = line7, Line8 = line8, BackgroundColor = backgroundColor, FontColor = fontColor, OutlineColor = outlineColor, NoteFont = noteFont, BackgroundOpacity = backgroundOpacity, NoteLocation = noteLocation }, input, ref cacheTextConstant8);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TextConstant8 TextConstant8(string line1, string line2, string line3, string line4, string line5, string line6, string line7, string line8, Brush backgroundColor, Brush fontColor, Brush outlineColor, SimpleFont noteFont, int backgroundOpacity, TextPosition noteLocation)
		{
			return indicator.TextConstant8(Input, line1, line2, line3, line4, line5, line6, line7, line8, backgroundColor, fontColor, outlineColor, noteFont, backgroundOpacity, noteLocation);
		}

		public Indicators.TextConstant8 TextConstant8(ISeries<double> input , string line1, string line2, string line3, string line4, string line5, string line6, string line7, string line8, Brush backgroundColor, Brush fontColor, Brush outlineColor, SimpleFont noteFont, int backgroundOpacity, TextPosition noteLocation)
		{
			return indicator.TextConstant8(input, line1, line2, line3, line4, line5, line6, line7, line8, backgroundColor, fontColor, outlineColor, noteFont, backgroundOpacity, noteLocation);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TextConstant8 TextConstant8(string line1, string line2, string line3, string line4, string line5, string line6, string line7, string line8, Brush backgroundColor, Brush fontColor, Brush outlineColor, SimpleFont noteFont, int backgroundOpacity, TextPosition noteLocation)
		{
			return indicator.TextConstant8(Input, line1, line2, line3, line4, line5, line6, line7, line8, backgroundColor, fontColor, outlineColor, noteFont, backgroundOpacity, noteLocation);
		}

		public Indicators.TextConstant8 TextConstant8(ISeries<double> input , string line1, string line2, string line3, string line4, string line5, string line6, string line7, string line8, Brush backgroundColor, Brush fontColor, Brush outlineColor, SimpleFont noteFont, int backgroundOpacity, TextPosition noteLocation)
		{
			return indicator.TextConstant8(input, line1, line2, line3, line4, line5, line6, line7, line8, backgroundColor, fontColor, outlineColor, noteFont, backgroundOpacity, noteLocation);
		}
	}
}

#endregion
