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
	public class AccountGoal : Indicator
	{
		
		/// <summary>
		/// Indicator Plots: Account Size, Intraday Margin(if provided).
		/// 
		/// 
		/// Based on work by Alan Palmer.
		/// 
		/// Mod by Dan Hill:  Using intraday margin and drop down accounts.  Allow historical and show available intraday excess margin if available from broker.
		/// </summary>
		
		#region vars
		private Account account;
		private double xAccountSize;
		#endregion 
		
		#region statechange
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Plot Intraday Margin.";
				Name										= "Account Goal";
				Calculate									= Calculate.OnEachTick;
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
				
				OrderQuant					= 1;
		
				MaxGain					= 125;
				MaxLoss					= 125;
				
				BackgroundColor			= Brushes.DimGray;
				BackgroundOpacity 		= 90;
				FontColor				= Brushes.WhiteSmoke;
				OutlineColor			= Brushes.DimGray;
				NoteFont				= new SimpleFont("Arial", 12);
				Realized				= true;
				
//				AddPlot(Brushes.Blue, "xAccountSize"); 
//				AddPlot(Brushes.Red, "xInitialRequiredMarginTimesOrderQuant");  
//				AddPlot(Brushes.Yellow, "xInitialRequiredMarginTimesOrderQuantDelta");
			}
			else if (State == State.Configure)
			{
				ClearOutputWindow();
			}
		}
        #endregion 
		
		#region barupdate
		protected override void OnBarUpdate()
		{
			if(CurrentBar<10) return;	
			//if(State == State.Historical) return;
			lock (Account.All)           
			    account = Account.All.FirstOrDefault(a => a.Name == AccountNames);
//			double xInitialRequiredMargin = Risk.Get("NinjaTrader Brokerage Default").ByMasterInstrument[Instrument.MasterInstrument].BuyIntradayMargin;
//		    double xInitialRequiredMarginDelta = account.Get(AccountItem.ExcessIntradayMargin, Currency.UsDollar);
//			double xInitialRequiredMarginTimesOrderQuant = xInitialRequiredMargin * OrderQuant; //How much margin you need per OrderQuant
			double gain = account.Get(AccountItem.GrossRealizedProfitLoss, Currency.UsDollar);
			
			if ( Realized ) {
				gain = account.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar);
			}
			
			string message = "ok to trade ";
			
			if ( gain < -MaxLoss ) {
				message = "Max Loss\nShut it down";
				message += "\n$" + gain.ToString("N0");
			}
			
			if ( gain > MaxGain ) {
				message = "Max Gain\ngo do something fun";
				message += "\n$" + gain.ToString("N0");
			}
			//Draw.TextFixed(this, "MyTextFixed", message, TextPosition.TopLeft);
			Draw.TextFixed(this, "MyTextFixed", 
					message, 
					TextPosition.TopLeft, 
					FontColor,  // text color
					NoteFont, 
					OutlineColor, // outline color
					BackgroundColor, 
					BackgroundOpacity);
			
		}
		#endregion
	
	    #region Properties
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="OrderQuant", Order=1, GroupName="NinjaScriptStrategyParameters")]
		public int OrderQuant
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MaxGain", Order=1, GroupName="Trading Limits")]
		public int MaxGain
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MaxLoss", Order=2, GroupName="Trading Limits")]
		public int MaxLoss
		{ get; set; }
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Font Color", Description="Font Color", Order=2, GroupName="Parameters")]
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
		[Display(Name="Background Color", Description="Background Color", Order=10, GroupName="Parameters")]
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
		[Display(Name="OutlineColor Color", Description="OutlineColor Color", Order=3, GroupName="Parameters")]
		public Brush OutlineColor
		{ get; set; }

		[Browsable(false)]
		public string OutlineColorSerializable
		{
			get { return Serialize.BrushToString(OutlineColor); }
			set { OutlineColor = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Background Opacity", Description="Background Opacity", Order=5, GroupName="Parameters")]
		public int BackgroundOpacity
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Note Font", Description="Note Font", Order=4, GroupName="Parameters")]
		public SimpleFont NoteFont
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Realized commission", Order=3, GroupName="Trading Limits")]
		public bool Realized
		{ get; set; }
		
//		[Range(0, 100)]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolAreaOpacity", GroupName = "NinjaScriptGeneral")]
//		public int AreaOpacity
//		{
//			get { return areaOpacity; }
//			set
//			{
//				areaOpacity = Math.Max(0, Math.Min(100, value));
//				if (areaBrush != null)
//				{
//					System.Windows.Media.Brush newBrush		= areaBrush.Clone();
//					newBrush.Opacity	= areaOpacity / 100d;
//					newBrush.Freeze();
//					areaBrush			= newBrush;
//				}
//			}
//		}
		
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> XInitialRequiredMarginTimesOrderQuant
//		{
//			get { return Values[1]; }
//		}

//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> XAccountSize
//		{
//			get { return Values[0]; }
//		}
		
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> XInitialRequiredMarginTimesOrderQuantDelta
//		{
//			get { return Values[2]; }
//		}
		
		[NinjaScriptProperty]
		[TypeConverter(typeof(AccountConverter))]
		public string AccountNames
		{get;set;}
		
		#endregion	
	    
		#region converter
		public class AccountConverter : TypeConverter
	    {
		
		   public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		   {
			   if (context == null)
			   {
				   return null;
			   }
	
			   List <string> list = new List <string> ();
			
		
			   foreach (Account sampleAccount in Account.All)
			   {
				   list.Add(sampleAccount.Name);
			   }
				
			
			   return new TypeConverter.StandardValuesCollection(list);
		   }
		
	

		   public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		   {
			   return true;
		   }
	    }
		#endregion
	}
	
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AccountGoal[] cacheAccountGoal;
		public AccountGoal AccountGoal(int orderQuant, int maxGain, int maxLoss, Brush fontColor, Brush backgroundColor, Brush outlineColor, int backgroundOpacity, SimpleFont noteFont, bool realized, string accountNames)
		{
			return AccountGoal(Input, orderQuant, maxGain, maxLoss, fontColor, backgroundColor, outlineColor, backgroundOpacity, noteFont, realized, accountNames);
		}

		public AccountGoal AccountGoal(ISeries<double> input, int orderQuant, int maxGain, int maxLoss, Brush fontColor, Brush backgroundColor, Brush outlineColor, int backgroundOpacity, SimpleFont noteFont, bool realized, string accountNames)
		{
			if (cacheAccountGoal != null)
				for (int idx = 0; idx < cacheAccountGoal.Length; idx++)
					if (cacheAccountGoal[idx] != null && cacheAccountGoal[idx].OrderQuant == orderQuant && cacheAccountGoal[idx].MaxGain == maxGain && cacheAccountGoal[idx].MaxLoss == maxLoss && cacheAccountGoal[idx].FontColor == fontColor && cacheAccountGoal[idx].BackgroundColor == backgroundColor && cacheAccountGoal[idx].OutlineColor == outlineColor && cacheAccountGoal[idx].BackgroundOpacity == backgroundOpacity && cacheAccountGoal[idx].NoteFont == noteFont && cacheAccountGoal[idx].Realized == realized && cacheAccountGoal[idx].AccountNames == accountNames && cacheAccountGoal[idx].EqualsInput(input))
						return cacheAccountGoal[idx];
			return CacheIndicator<AccountGoal>(new AccountGoal(){ OrderQuant = orderQuant, MaxGain = maxGain, MaxLoss = maxLoss, FontColor = fontColor, BackgroundColor = backgroundColor, OutlineColor = outlineColor, BackgroundOpacity = backgroundOpacity, NoteFont = noteFont, Realized = realized, AccountNames = accountNames }, input, ref cacheAccountGoal);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AccountGoal AccountGoal(int orderQuant, int maxGain, int maxLoss, Brush fontColor, Brush backgroundColor, Brush outlineColor, int backgroundOpacity, SimpleFont noteFont, bool realized, string accountNames)
		{
			return indicator.AccountGoal(Input, orderQuant, maxGain, maxLoss, fontColor, backgroundColor, outlineColor, backgroundOpacity, noteFont, realized, accountNames);
		}

		public Indicators.AccountGoal AccountGoal(ISeries<double> input , int orderQuant, int maxGain, int maxLoss, Brush fontColor, Brush backgroundColor, Brush outlineColor, int backgroundOpacity, SimpleFont noteFont, bool realized, string accountNames)
		{
			return indicator.AccountGoal(input, orderQuant, maxGain, maxLoss, fontColor, backgroundColor, outlineColor, backgroundOpacity, noteFont, realized, accountNames);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AccountGoal AccountGoal(int orderQuant, int maxGain, int maxLoss, Brush fontColor, Brush backgroundColor, Brush outlineColor, int backgroundOpacity, SimpleFont noteFont, bool realized, string accountNames)
		{
			return indicator.AccountGoal(Input, orderQuant, maxGain, maxLoss, fontColor, backgroundColor, outlineColor, backgroundOpacity, noteFont, realized, accountNames);
		}

		public Indicators.AccountGoal AccountGoal(ISeries<double> input , int orderQuant, int maxGain, int maxLoss, Brush fontColor, Brush backgroundColor, Brush outlineColor, int backgroundOpacity, SimpleFont noteFont, bool realized, string accountNames)
		{
			return indicator.AccountGoal(input, orderQuant, maxGain, maxLoss, fontColor, backgroundColor, outlineColor, backgroundOpacity, noteFont, realized, accountNames);
		}
	}
}

#endregion
