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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;

using SharpDX;
using SharpDX.DirectWrite;
using SharpDX.Direct2D1;

using System.IO;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Markup;
using NinjaTrader.Gui.Tools;
using System.Windows.Documents;

#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.Sim22
{
	/// <summary>
	/// OnRender value helper. 
	/// This was made in an effort to understand the actual values used to plot indicators.
	/// It has certainly saved my sanity once or twice!
	/// 
	/// Sim22 March 2016 NT8b10. simterann22@gmx.com
	/// </summary>
	public class Sim22_OnRenderHelper : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description							= @"OnRender value helper. Sim22 Jan 2016 NT8b10.";
				Name								= "Sim22_OnRenderHelperV1";
				Calculate							= Calculate.OnBarClose;
				IsOverlay							= true;
				DisplayInDataBox					= true;
				DrawOnPricePanel					= true;
				DrawHorizontalGridLines				= true;
				DrawVerticalGridLines				= true;
				PaintPriceMarkers					= true;
				ScaleJustification					= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive			= true;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			if( Bars == null || ChartControl == null || Bars.Instrument == null || !IsVisible ) 
			{
				return;
			}
			
			/*
				Note: There may be an easier way of accomplishing this by using a more direct call to 'properties' and listing the entire contents possibly via an array?
				I'd appreciate an email if you have a better way, thanks. simterann22@gmx.com
			*/
			try
			{
				string 	bw		= chartControl.BarWidth.ToString();
				string	sp		= chartControl.SlotsPainted.ToString();
				string	lsp		= chartControl.LastSlotPainted.ToString();
				string	fi		= ChartBars.FromIndex.ToString();
				string	ti		= ChartBars.ToIndex.ToString();
				string	co		= ChartBars.Count.ToString();
				string	gbpw	= chartControl.GetBarPaintWidth(ChartBars).ToString();
				string	cl		= chartControl.CanvasLeft.ToString();
				string 	cr		= chartControl.CanvasRight.ToString();
				string	bd		= chartControl.Properties.BarDistance.ToString();
				string	bmr		= chartControl.Properties.BarMarginRight.ToString();
				string	cch		= chartControl.ActualHeight.ToString();
				string	ccw		= chartControl.ActualWidth.ToString();
				string 	cpx		= ChartPanel.X.ToString();
				string 	cpy		= ChartPanel.Y.ToString();
				string 	cpw		= ChartPanel.W.ToString();
				string 	cph		= ChartPanel.H.ToString();
				string 	cpmax	= ChartPanel.MaxValue.ToString();
				string 	cpmin	= ChartPanel.MinValue.ToString();
				string	csgp	= chartScale.GetPixelsForDistance(TickSize).ToString();
				string	csgy	= chartScale.GetYByValue(Close[0]).ToString();
				string	csh		= chartScale.Height.ToString();
				string csmm		= chartScale.MaxMinusMin.ToString();
				string	csmx	= chartScale.MaxValue.ToString();
				string	csmn	= chartScale.MinValue.ToString();
				string	csw		= chartScale.Width.ToString();
				
				string	barNumbers		= String.Format(" Bar Numbers: \n\n ChartControl.SlotsPainted \t\t= {0} \n ChartControl.LastSlotPainted \t\t= {1} \n ChartBars.FromIndex \t\t\t= {2} \n ChartBars.ToIndex \t\t\t= {3} \n ChartBars.Count \t\t\t= {4}\n\n", sp, lsp, fi, ti, co);
				string	barFormatting	=  String.Format(" Bar Formatting: \n\n ChartControl.BarWidth \t\t\t= {0} \n ChartControl.GetBarPaintWidth() \t\t= {1} \n ChartControl.Properties.BarDistance \t= {2} \n ChartControl.Properties.BarMarginRight \t= {3}\n\n", bw, gbpw, bd, bmr);
				string	canvas			= String.Format(" Chart Properties: \n\n ChartControl.CanvasLeft \t\t\t= {0} \n ChartControl.CanvasRight \t\t= {1} \n ChartControl.ActualHeight \t\t= {2} \n ChartControl.ActualWidth \t\t\t= {3}\n\n", cl, cr, cch, ccw);
				string	chartpanel		= String.Format(" Panel Properties: \n\n ChartPanel.X \t\t\t\t= {0} \n ChartPanel.Y \t\t\t\t= {1} \n ChartPanel.W \t\t\t\t= {2} \n ChartPanel.H \t\t\t\t= {3} \n ChartPanel.MaxValue \t\t\t= {4} \n ChartPanel.MinValue \t\t\t= {5}\n\n", cpx, cpy, cpw, cph, cpmax, cpmin);
				string 	chartscale		= String.Format(" Chart Scale: \n\n ChartScale.GetPixelsForDistance(TickSize) \t= {0} \n ChartScale.GetYByValue(Close[0])\t\t= {1} \n ChartScale.Height\t\t\t= {2} \n ChartScale.Max\t\t\t\t= {3} \n ChartScale.Min\t\t\t\t= {4} \n ChartScale.MaxMinusMin\t\t\t= {5} \n ChartScale.Width\t\t\t= {6}\n\n", csgp, csgy, csh, csmx, csmn, csmm, csw);
				
	            SharpDX.DirectWrite.Factory factory = new SharpDX.DirectWrite.Factory();
	            TextFormat format = new TextFormat(factory, "arial", 18);
				TextFormat formatH = new TextFormat(factory, "arial", 18);
				formatH.TextAlignment	= SharpDX.DirectWrite.TextAlignment.Center;
	            SharpDX.Rectangle textRectangle = new SharpDX.Rectangle(100, ChartPanel.Y, 600, ChartPanel.H);
				SharpDX.Rectangle textRectangleH = new SharpDX.Rectangle(100, ChartPanel.Y, 600, ChartPanel.Y + 50);
	            SharpDX.Direct2D1.Brush textBrush = Brushes.Black.ToDxBrush(RenderTarget);
				SharpDX.Direct2D1.Brush backBrush = Brushes.White.ToDxBrush(RenderTarget);
				backBrush.Opacity	= 0.90f;
				RenderTarget.FillRectangle(textRectangle, backBrush);
				
				RenderTarget.DrawText("\n\n'Drag and drop' this helper to any panel you choose.....", formatH, textRectangleH, textBrush);
	            RenderTarget.DrawText("\n\n\n\n" + barNumbers + barFormatting + canvas + chartpanel + chartscale + "\n Please see the NT8 help for definitions.....", format, textRectangle, textBrush);
				
				format.Dispose();
				formatH.Dispose();
				factory.Dispose();
				textBrush.Dispose();
				backBrush.Dispose();
			}
			catch
			{
				Draw.TextFixed(this, "Error", "There is something wrong with the OnRender helper.", TextPosition.Center);
			}
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Sim22.Sim22_OnRenderHelper[] cacheSim22_OnRenderHelper;
		public Sim22.Sim22_OnRenderHelper Sim22_OnRenderHelper()
		{
			return Sim22_OnRenderHelper(Input);
		}

		public Sim22.Sim22_OnRenderHelper Sim22_OnRenderHelper(ISeries<double> input)
		{
			if (cacheSim22_OnRenderHelper != null)
				for (int idx = 0; idx < cacheSim22_OnRenderHelper.Length; idx++)
					if (cacheSim22_OnRenderHelper[idx] != null &&  cacheSim22_OnRenderHelper[idx].EqualsInput(input))
						return cacheSim22_OnRenderHelper[idx];
			return CacheIndicator<Sim22.Sim22_OnRenderHelper>(new Sim22.Sim22_OnRenderHelper(), input, ref cacheSim22_OnRenderHelper);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Sim22.Sim22_OnRenderHelper Sim22_OnRenderHelper()
		{
			return indicator.Sim22_OnRenderHelper(Input);
		}

		public Indicators.Sim22.Sim22_OnRenderHelper Sim22_OnRenderHelper(ISeries<double> input )
		{
			return indicator.Sim22_OnRenderHelper(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Sim22.Sim22_OnRenderHelper Sim22_OnRenderHelper()
		{
			return indicator.Sim22_OnRenderHelper(Input);
		}

		public Indicators.Sim22.Sim22_OnRenderHelper Sim22_OnRenderHelper(ISeries<double> input )
		{
			return indicator.Sim22_OnRenderHelper(input);
		}
	}
}

#endregion
