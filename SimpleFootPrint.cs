#region Using declarations
using System;
using System.Drawing;
using System.Collections;
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
using SharpDX.Direct2D1;
using SharpDX;
using SharpDX.DirectWrite;
//using System.Windows.Forms;

using System.Windows.Controls;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class SimpleFootPrint : Indicator
	{
		
		public class ABV
        {
            public double Price { get; set; }
            public double Vol_ask { get; set; }
            public double Vol_bid { get; set; }
			public double Volume { get; set; }
            public int Id { get; set; }
        }
		
		private List<ABV> main_list = new List<ABV>();
		
		public struct ABVolume
		{
			public double volume;
			public double ask_volume;
			public double bid_volume;
		}
		
		
		private Dictionary<double, ABVolume> _abv = new Dictionary<double, ABVolume>();
		private List <Dictionary<double, ABVolume>> lst=new List<Dictionary<double, ABVolume>>();
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description							= @"Enter the description for your new custom Indicator here.";
				Name								= "SimpleFootPrint";
				Calculate							= Calculate.OnEachTick;
				IsOverlay							= true;
				DisplayInDataBox					= true;
				DrawOnPricePanel					= true;
				DrawHorizontalGridLines				= true;
				DrawVerticalGridLines				= true;
				PaintPriceMarkers					= true;
				ScaleJustification					= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive			= true;
			}
			else if (State == State.Historical)
			{
				
			}
			else if (State == State.Terminated)
			{
			
			}
		}

		protected override void OnMarketData(MarketDataEventArgs e)
		{
			
			if (e.MarketDataType==MarketDataType.Last)
			{
				if (IsFirstTickOfBar)
				{
					if (_abv.Count>0)
					{
						foreach (KeyValuePair<double, ABVolume> kvp2 in _abv)
			            {
			                ABV tmp = new ABV();
			                tmp.Id = CurrentBar-1;
			                tmp.Price = kvp2.Key;
			                tmp.Vol_ask = kvp2.Value.ask_volume;
			                tmp.Vol_bid = kvp2.Value.bid_volume;
							tmp.Volume=kvp2.Value.volume;
			                main_list.Add(tmp);
			            }
						
					
					}
					
					_abv.Clear();
				}
				
				if (e.Price>=e.Ask)
				{
					if (_abv.ContainsKey(e.Price))
					{
						ABVolume tmp=new ABVolume();
						tmp.bid_volume=_abv[e.Price].bid_volume;
						tmp.ask_volume=_abv[e.Price].ask_volume+e.Volume;
						tmp.volume=_abv[e.Price].volume;
						_abv[e.Price]=tmp;
					}
					else
					{
						ABVolume tmp=new ABVolume();
						tmp.ask_volume=e.Volume;
						tmp.bid_volume=0;
						tmp.volume=e.Volume;
						_abv.Add(e.Price,tmp);
					}
				}
				else if(e.Price<=e.Bid)
				{
					if (_abv.ContainsKey(e.Price))
					{
						ABVolume tmp=new ABVolume();
						tmp.bid_volume=_abv[e.Price].bid_volume+e.Volume;
						tmp.ask_volume=_abv[e.Price].ask_volume;
						tmp.volume=_abv[e.Price].volume;
						_abv[e.Price]=tmp;
					}
					else
					{
						ABVolume tmp=new ABVolume();
						tmp.ask_volume=0;
						tmp.bid_volume=e.Volume;
						tmp.volume=e.Volume;
						_abv.Add(e.Price,tmp);
					}
				}
					
			
					
			}
			
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
	
			TextFormat tf=new TextFormat(new SharpDX.DirectWrite.Factory(),"Arial",16);
			tf.ParagraphAlignment=ParagraphAlignment.Center;
			tf.TextAlignment=SharpDX.DirectWrite.TextAlignment.Center;
			
			TextFormat tf_low=new TextFormat(new SharpDX.DirectWrite.Factory(),"Arial",12);
			tf_low.ParagraphAlignment=ParagraphAlignment.Near;
			tf_low.TextAlignment=SharpDX.DirectWrite.TextAlignment.Trailing;
			
			TextFormat tf_low2=new TextFormat(new SharpDX.DirectWrite.Factory(),"Arial",12);
			tf_low2.ParagraphAlignment=ParagraphAlignment.Near;
			tf_low2.TextAlignment=SharpDX.DirectWrite.TextAlignment.Leading;
			
			for (int idx = ChartBars.FromIndex; idx <= ChartBars.ToIndex; idx++)
		    {
				double      closeValue              = ChartBars.Bars.GetClose(idx);
		        double      openValue               = ChartBars.Bars.GetOpen(idx);
				double      highValue              = ChartBars.Bars.GetHigh(idx);
		        double      lowValue               = ChartBars.Bars.GetLow(idx);
				
				IEnumerable<ABV> _list = main_list.Where(p => p.Id == idx);
					
				GlyphTypeface gtf=new GlyphTypeface();
				System.Windows.Media.Typeface t_face=new System.Windows.Media.Typeface(new System.Windows.Media.FontFamily("Arial"),FontStyles.Normal,FontWeights.Normal,FontStretches.Normal);				
				t_face.TryGetGlyphTypeface(out gtf);
				double h_str=gtf.CapsHeight*16;
						
				int x=chartControl.GetXByBarIndex(ChartBars,idx)-(int)chartControl.BarWidth;
				int y_close=chartScale.GetYByValue(closeValue)-(int)(h_str*0.5*1.8);
				int y_open=chartScale.GetYByValue(openValue)-(int)(h_str*0.5*1.8);
				int y_high=chartScale.GetYByValue(highValue+TickSize)-(int)(h_str*0.5*1.8);
				int y_low=chartScale.GetYByValue(lowValue-TickSize)-(int)(h_str*0.5*1.8);
				BrushProperties bp2=new BrushProperties();
				BrushProperties bp_frame=new BrushProperties();
				bp_frame.Opacity=0.99f;
				bp2.Opacity=0.7f;
				int y3=y_close;
				SharpDX.Color clr=SharpDX.Color.Gray;
				if (closeValue>openValue)
					clr=SharpDX.Color.Green;
				else if (closeValue<openValue)
				{
					clr=SharpDX.Color.Red;
					y3=y_open;
				}
						
				SharpDX.Direct2D1.SolidColorBrush sb2=new SharpDX.Direct2D1.SolidColorBrush(RenderTarget,clr,bp2);
				SharpDX.Direct2D1.SolidColorBrush frame=new SharpDX.Direct2D1.SolidColorBrush(RenderTarget,SharpDX.Color.Black,bp2);
							
				RenderTarget.FillRectangle(new RectangleF(x-10,y3,8,(float)Math.Abs(y_close-y_open)+(int)(h_str*1.8)),sb2);	
				if (idx==ChartBars.Count-1)
				{
					
					foreach(KeyValuePair<double, ABVolume> kvp in _abv)
					{
						int y=chartScale.GetYByValue(kvp.Key)-(int)(h_str*0.5*1.8);
						int y2=chartScale.GetYByValue(kvp.Key+TickSize)-(int)(h_str);
						string str="";	
						str=kvp.Value.ask_volume.ToString()+" x "+kvp.Value.bid_volume.ToString();
						BrushProperties bp=new BrushProperties();
						bp.Opacity=0.7f;
						SharpDX.Color clr2=SharpDX.Color.Green;
						if (kvp.Value.ask_volume<kvp.Value.bid_volume)
							clr2=SharpDX.Color.Red;
						SharpDX.Direct2D1.SolidColorBrush sb=new SharpDX.Direct2D1.SolidColorBrush(RenderTarget,clr2,bp);
						RenderTarget.FillRectangle(new RectangleF(x,y,(int)chartControl.BarWidth*2,(float)(h_str*1.8)),sb);	
						
						if (kvp.Key==Close[0])
							RenderTarget.DrawRectangle(new RectangleF(x,y,(int)chartControl.BarWidth*2,(float)(h_str*1.8)),frame,3);	
							
						RenderTarget.DrawText(str,tf,new RectangleF(x,y,(int)chartControl.BarWidth*2,(float)(h_str*1.8)),
							new SharpDX.Direct2D1.SolidColorBrush(RenderTarget,SharpDX.Color.Black),
							DrawTextOptions.None,MeasuringMode.GdiClassic);
					}
					
				}
				else
				{
					double delta=0;
					double max_delta=0;
					double curr_delta=0;
					foreach (ABV t in _list)
	            	{
						curr_delta=t.Vol_ask+t.Vol_bid;
						if (curr_delta>max_delta)
							max_delta=curr_delta;
					}
					foreach (ABV t in _list)
	            	{
						
						int y=chartScale.GetYByValue(t.Price)-(int)(h_str*0.5*1.8);
						int y2=chartScale.GetYByValue(t.Price+TickSize)-(int)(h_str);
						string str="";	
						str=t.Vol_ask.ToString()+" x "+t.Vol_bid.ToString();
						delta+=t.Vol_ask;
						delta-=t.Vol_bid;
						curr_delta=t.Vol_ask+t.Vol_bid;
						double curr_procent=100*curr_delta/max_delta;
						double curr_opacity=Math.Round(curr_procent/100*0.8,1);
						BrushProperties bp=new BrushProperties();
						bp.Opacity=(float)curr_opacity;
						SharpDX.Color clr3=SharpDX.Color.Green;
						if (t.Vol_ask<t.Vol_bid)
							clr3=SharpDX.Color.Red;
						SharpDX.Direct2D1.SolidColorBrush sb=new SharpDX.Direct2D1.SolidColorBrush(RenderTarget,clr3,bp);
						RenderTarget.FillRectangle(new RectangleF(x,y,(int)chartControl.BarWidth*2,(float)(h_str*1.8)),sb);	
						
						RenderTarget.DrawText(str,tf,new RectangleF(x,y,(int)chartControl.BarWidth*2,(float)(h_str*1.8)),
							new SharpDX.Direct2D1.SolidColorBrush(RenderTarget,SharpDX.Color.Black),
						DrawTextOptions.None,MeasuringMode.GdiClassic);
					}	
					RenderTarget.DrawText(delta.ToString(),tf_low,new RectangleF(x,y_low,(int)chartControl.BarWidth*2,(float)(h_str*1.8)),
							new SharpDX.Direct2D1.SolidColorBrush(RenderTarget,SharpDX.Color.Black),DrawTextOptions.None,MeasuringMode.GdiClassic);
					RenderTarget.DrawText(Volume[ChartBars.Count-idx-1].ToString(),tf_low2,new RectangleF(x,y_low,(int)chartControl.BarWidth*2,(float)(h_str*1.8)),
							new SharpDX.Direct2D1.SolidColorBrush(RenderTarget,SharpDX.Color.Black),DrawTextOptions.None,MeasuringMode.GdiClassic);
					
					}
					
		        }
				
		}

		protected override void OnBarUpdate()
		{
			if ( CurrentBar < 10 ) { return; }
			BarBrushes[0] = Brushes.Transparent;
			CandleOutlineBrushes[0] = Brushes.Transparent;
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SimpleFootPrint[] cacheSimpleFootPrint;
		public SimpleFootPrint SimpleFootPrint()
		{
			return SimpleFootPrint(Input);
		}

		public SimpleFootPrint SimpleFootPrint(ISeries<double> input)
		{
			if (cacheSimpleFootPrint != null)
				for (int idx = 0; idx < cacheSimpleFootPrint.Length; idx++)
					if (cacheSimpleFootPrint[idx] != null &&  cacheSimpleFootPrint[idx].EqualsInput(input))
						return cacheSimpleFootPrint[idx];
			return CacheIndicator<SimpleFootPrint>(new SimpleFootPrint(), input, ref cacheSimpleFootPrint);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SimpleFootPrint SimpleFootPrint()
		{
			return indicator.SimpleFootPrint(Input);
		}

		public Indicators.SimpleFootPrint SimpleFootPrint(ISeries<double> input )
		{
			return indicator.SimpleFootPrint(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SimpleFootPrint SimpleFootPrint()
		{
			return indicator.SimpleFootPrint(Input);
		}

		public Indicators.SimpleFootPrint SimpleFootPrint(ISeries<double> input )
		{
			return indicator.SimpleFootPrint(input);
		}
	}
}

#endregion
