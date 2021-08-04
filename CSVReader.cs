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
	public class CSVReader : Indicator
	{
		private string path;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "CSV Reader";
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
				Filename					= @"ZeroPrints.csv";
				LineColor					= Brushes.Lime;
				LineThickness					= 2;
				LineOpacity					= 0.8;
				LineLength					= 30;
				path					= NinjaTrader.Core.Globals.UserDataDir + Filename;
			}
			else if (State == State.Configure)
			{
				ClearOutputWindow();
			}
		}

		protected override void OnBarUpdate()
		{
//			using (var tw = new StreamReader(path, true))
//            {
//               string content = tw.ReadToEnd();
//				Print(content);
//                tw.Close();
//            }
			
			var sr = new StreamReader(path);
			ClearOutputWindow();
			 string line;
			while ((line = sr.ReadLine()) != null)
	            {
	                //Print(line);
					string[] words = line.Split(',');
					string date = words.First();
					string price = words[2];
					
					//Print(date);
					//DateTime myDate = DateTime.Parse(date);
					DateTime myDate;
					if (!DateTime.TryParse(date, out myDate))
					{
					    Print("Parse fail");
					} else {
						double priceDouble = Convert.ToDouble(price);
						Print(myDate + " \t" + priceDouble.ToString());
						if (Time[0] == myDate) {
								//Draw.Text(this, "zeroPrint"+CurrentBar, "- - - - - - - - - -", 0, priceDouble, Brushes.White); 
							   Draw.Line(this, "zeroPrinlt"+CurrentBar, true, 3,priceDouble, -LineLength, priceDouble, LineColor, DashStyleHelper.Dot, LineThickness);
						}
					}
						
	            }
				sr.Close();
				sr.Dispose();
		}
		


		#region Properties
		[NinjaScriptProperty]
		[Display(Name="Filename", Order=1, GroupName="Parameters")]
		public string Filename
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="LineColor", Order=2, GroupName="Parameters")]
		public Brush LineColor
		{ get; set; }

		[Browsable(false)]
		public string LineColorSerializable
		{
			get { return Serialize.BrushToString(LineColor); }
			set { LineColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Line Thickness", Order=3, GroupName="Parameters")]
		public int LineThickness
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name="Line Opacity", Order=4, GroupName="Parameters")]
		public double LineOpacity
		{ get; set; }
		
		// LineLength
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Line Length", Order=5, GroupName="Parameters")]
		public int LineLength
		{ get; set; }
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CSVReader[] cacheCSVReader;
		public CSVReader CSVReader(string filename, Brush lineColor, int lineThickness, double lineOpacity, int lineLength)
		{
			return CSVReader(Input, filename, lineColor, lineThickness, lineOpacity, lineLength);
		}

		public CSVReader CSVReader(ISeries<double> input, string filename, Brush lineColor, int lineThickness, double lineOpacity, int lineLength)
		{
			if (cacheCSVReader != null)
				for (int idx = 0; idx < cacheCSVReader.Length; idx++)
					if (cacheCSVReader[idx] != null && cacheCSVReader[idx].Filename == filename && cacheCSVReader[idx].LineColor == lineColor && cacheCSVReader[idx].LineThickness == lineThickness && cacheCSVReader[idx].LineOpacity == lineOpacity && cacheCSVReader[idx].LineLength == lineLength && cacheCSVReader[idx].EqualsInput(input))
						return cacheCSVReader[idx];
			return CacheIndicator<CSVReader>(new CSVReader(){ Filename = filename, LineColor = lineColor, LineThickness = lineThickness, LineOpacity = lineOpacity, LineLength = lineLength }, input, ref cacheCSVReader);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CSVReader CSVReader(string filename, Brush lineColor, int lineThickness, double lineOpacity, int lineLength)
		{
			return indicator.CSVReader(Input, filename, lineColor, lineThickness, lineOpacity, lineLength);
		}

		public Indicators.CSVReader CSVReader(ISeries<double> input , string filename, Brush lineColor, int lineThickness, double lineOpacity, int lineLength)
		{
			return indicator.CSVReader(input, filename, lineColor, lineThickness, lineOpacity, lineLength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CSVReader CSVReader(string filename, Brush lineColor, int lineThickness, double lineOpacity, int lineLength)
		{
			return indicator.CSVReader(Input, filename, lineColor, lineThickness, lineOpacity, lineLength);
		}

		public Indicators.CSVReader CSVReader(ISeries<double> input , string filename, Brush lineColor, int lineThickness, double lineOpacity, int lineLength)
		{
			return indicator.CSVReader(input, filename, lineColor, lineThickness, lineOpacity, lineLength);
		}
	}
}

#endregion
