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
	public class MarginPlot : Indicator
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
				Name										= "MarginPlot";
				Calculate									= Calculate.OnEachTick;
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
				
				OrderQuant					= 1;
		
				AddPlot(Brushes.Blue, "xAccountSize"); 
				AddPlot(Brushes.Red, "xInitialRequiredMarginTimesOrderQuant");  
				AddPlot(Brushes.Yellow, "xInitialRequiredMarginTimesOrderQuantDelta");
			}
			else if (State == State.Configure)
			{
			}
		}
        #endregion 
		
		#region barupdate
		protected override void OnBarUpdate()
		{
			if(CurrentBar<10) return;	
			//if(State == State.Historical) return;
			
		
			//
			lock (Account.All)           
			    account = Account.All.FirstOrDefault(a => a.Name == AccountNames);

			double xInitialRequiredMargin = Risk.Get("NinjaTrader Brokerage Default").ByMasterInstrument[Instrument.MasterInstrument].BuyIntradayMargin;
		    double xInitialRequiredMarginDelta = account.Get(AccountItem.ExcessIntradayMargin, Currency.UsDollar);
			
			
			double xInitialRequiredMarginTimesOrderQuant = xInitialRequiredMargin * OrderQuant; //How much margin you need per OrderQuant
	
			
			xAccountSize= account.Get(AccountItem.CashValue, Currency.UsDollar); //Assigns account size to xAcccountSize

			
			XAccountSize[0]=xAccountSize;
			XInitialRequiredMarginTimesOrderQuant[0]=xInitialRequiredMarginTimesOrderQuant;
			XInitialRequiredMarginTimesOrderQuantDelta[0]=xInitialRequiredMarginDelta;
			
			double gain = account.Get(AccountItem.GrossRealizedProfitLoss, Currency.UsDollar);
			Print("GP: " + gain.ToString());
			
		}
		#endregion
	
	    #region Properties
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="OrderQuant", Order=1, GroupName="NinjaScriptStrategyParameters")]
		public int OrderQuant
		{ get; set; }
		
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> XInitialRequiredMarginTimesOrderQuant
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> XAccountSize
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> XInitialRequiredMarginTimesOrderQuantDelta
		{
			get { return Values[2]; }
		}
		
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
		private MarginPlot[] cacheMarginPlot;
		public MarginPlot MarginPlot(int orderQuant, string accountNames)
		{
			return MarginPlot(Input, orderQuant, accountNames);
		}

		public MarginPlot MarginPlot(ISeries<double> input, int orderQuant, string accountNames)
		{
			if (cacheMarginPlot != null)
				for (int idx = 0; idx < cacheMarginPlot.Length; idx++)
					if (cacheMarginPlot[idx] != null && cacheMarginPlot[idx].OrderQuant == orderQuant && cacheMarginPlot[idx].AccountNames == accountNames && cacheMarginPlot[idx].EqualsInput(input))
						return cacheMarginPlot[idx];
			return CacheIndicator<MarginPlot>(new MarginPlot(){ OrderQuant = orderQuant, AccountNames = accountNames }, input, ref cacheMarginPlot);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MarginPlot MarginPlot(int orderQuant, string accountNames)
		{
			return indicator.MarginPlot(Input, orderQuant, accountNames);
		}

		public Indicators.MarginPlot MarginPlot(ISeries<double> input , int orderQuant, string accountNames)
		{
			return indicator.MarginPlot(input, orderQuant, accountNames);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MarginPlot MarginPlot(int orderQuant, string accountNames)
		{
			return indicator.MarginPlot(Input, orderQuant, accountNames);
		}

		public Indicators.MarginPlot MarginPlot(ISeries<double> input , int orderQuant, string accountNames)
		{
			return indicator.MarginPlot(input, orderQuant, accountNames);
		}
	}
}

#endregion
