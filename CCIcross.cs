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
// screenshot
using System.Windows.Media.Imaging;
using System.Net.Mail;
using System.Net.Mime;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class CCIcross : Indicator
	{
		private CCI CCI1;
		private NinjaTrader.NinjaScript.Indicators.LizardIndicators.amaCurrentWeekVWAP amaCurrentWeekVWAP1;
		private int filterVwapSetting = 2;
		private double threshold = 100.0;
		private double threshold2 = 200.0;
		// mail server
		private string toEmailAddress = "3103824522@tmomail.net";
		private string fromEmailAddress = "drones@swiftsense.org";
		private string serverPass = "Jesse2005#";
		private string myServer = "imap.stackmail.com";
		private int myPort = 587;
		// screen shot
		NinjaTrader.Gui.Chart.Chart 	chart;
        BitmapFrame 					outputFrame;
		private bool ScreenShotSent 	= false;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "CCI Cross";
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
				AlertOn										= true;
				AlertSOund									= @"smsAlert1.wav";
				FilterVwap									= false;
				FilterStdDev1								= true;
			}
			else if (State == State.Configure)
			{
				Dispatcher.BeginInvoke(new Action(() =>
				{
					chart = Window.GetWindow(ChartControl) as Chart;
				})); 
			}
			else if (State == State.DataLoaded)
			{				
				CCI1				= CCI(Close, 14);
				amaCurrentWeekVWAP1				= amaCurrentWeekVWAP(Close, amaSessionTypeVWAPW.Full_Session, amaBandTypeVWAPW.Standard_Deviation, amaTimeZonesVWAPW.Exchange_Time, @"08:30", @"15:15", 1, 2, 3, 1, 2, 3);
			}
		}

		protected override void OnBarUpdate()
		{
			/// Long Entry
			if (CrossAbove(CCI1, -threshold, 1))
			{
				// filter 1 long only below vwap
				if (Low[0] <=  amaCurrentWeekVWAP1.SessionVWAP[0] && FilterVwap) {
					Draw.TriangleUp(this, "buythis"+CurrentBar, false, 0, Low[0] - 2 * TickSize, Brushes.Blue);
					sendAlert(message: "Long Entry @ " + Close[0].ToString("N2"), sound: AlertSOund );
				}
				// filter 2 short onli above std dev 1
				if (Low[0] <=  amaCurrentWeekVWAP1.LowerBand1[0] && FilterStdDev1) {
					Draw.TriangleUp(this, "buyin"+CurrentBar, false, 0, Low[0] - 2 * TickSize, Brushes.Blue);
					sendAlert(message: "Long Entry @ " + Close[0].ToString("N2"), sound: AlertSOund );
				}
				
			}
			
			if (CrossBelow(CCI1, threshold, 1))
			{
				if (High[0] >=  amaCurrentWeekVWAP1.SessionVWAP[0] && FilterVwap ) {
					Draw.TriangleDown(this, "sellin"+CurrentBar, false, 0, High[0] + 2 * TickSize, Brushes.Red);
					sendAlert(message: "Short Entry @ " + Close[0].ToString("N2"), sound: AlertSOund );
				}
				if (High[0] >=  amaCurrentWeekVWAP1.UpperBand1[0] && FilterStdDev1) {
					Draw.TriangleDown(this, "sellit"+CurrentBar, false, 0, High[0] + 2 * TickSize, Brushes.Red);
					sendAlert(message: "Short Entry @ " + Close[0].ToString("N2"), sound: AlertSOund );
				}
			}
			
		}
		
		private void sendAlert(string message, string sound ) {
			if (CurrentBar < Count -2 || !AlertOn) return;
			message += " " + Bars.Instrument.MasterInstrument.Name;
			Alert("myAlert"+CurrentBar, Priority.High, message, NinjaTrader.Core.Globals.InstallDir+@"\sounds\"+ AlertSOund,10, Brushes.Black, Brushes.Yellow);  
			if ( AlertOn ) {
				//SendMail("whansen1@mac.com", "Trade Alert", message + " " );
				if (!ScreenShotSent)
				{
					SendMailChart("Trade Alert "+ message ,"This is the body of the email",fromEmailAddress,toEmailAddress,myServer,587,fromEmailAddress,serverPass);
					ScreenShotSent = true;
				}
			}	
		}
		
		private void SendMailChart(string Subject, string Body, string From, string To, string Host, int Port, string Username, string Password)
		{
			
			try	
			{	

				Dispatcher.BeginInvoke(new Action(() =>
				{
				
						if (chart != null)
				        {
							
							RenderTargetBitmap	screenCapture = chart.GetScreenshot(ShareScreenshotType.Chart);
		                    outputFrame = BitmapFrame.Create(screenCapture);
							
		                    if (screenCapture != null)
		                    {
		                       
								PngBitmapEncoder png = new PngBitmapEncoder();
		                        png.Frames.Add(outputFrame);
								System.IO.MemoryStream stream = new System.IO.MemoryStream();
								png.Save(stream);
								stream.Position = 0;
							
								MailMessage theMail = new MailMessage(From, To, Subject, Body);
								System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(stream, "image.png");
								theMail.Attachments.Add(attachment);
							
								SmtpClient smtp = new SmtpClient(Host, Port);
								smtp.EnableSsl = true;
								smtp.Credentials = new System.Net.NetworkCredential(Username, Password);
								string token = Instrument.MasterInstrument.Name + ToDay(Time[0]) + " " + ToTime(Time[0]) + CurrentBar.ToString();
								
								Print("Sending Mail!");
								smtp.SendAsync(theMail, token);
		                  
				            }
						}
			
			    
				}));

				
				
			}
			catch (Exception ex) {
				
				Print("Sending Chart email failed -  " + ex);
			
			}
		}
	

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="AlertOn", Order=1, GroupName="Parameters")]
		public bool AlertOn
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="AlertSOund", Order=2, GroupName="Parameters")]
		public string AlertSOund
		{ get; set; }
		 
		[NinjaScriptProperty]
		[Display(Name="Filter Vwap", Order=3, GroupName="Parameters")]
		public bool FilterVwap
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Filter StdDev1", Order=4, GroupName="Parameters")]
		public bool FilterStdDev1
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CCIcross[] cacheCCIcross;
		public CCIcross CCIcross(bool alertOn, string alertSOund, bool filterVwap, bool filterStdDev1)
		{
			return CCIcross(Input, alertOn, alertSOund, filterVwap, filterStdDev1);
		}

		public CCIcross CCIcross(ISeries<double> input, bool alertOn, string alertSOund, bool filterVwap, bool filterStdDev1)
		{
			if (cacheCCIcross != null)
				for (int idx = 0; idx < cacheCCIcross.Length; idx++)
					if (cacheCCIcross[idx] != null && cacheCCIcross[idx].AlertOn == alertOn && cacheCCIcross[idx].AlertSOund == alertSOund && cacheCCIcross[idx].FilterVwap == filterVwap && cacheCCIcross[idx].FilterStdDev1 == filterStdDev1 && cacheCCIcross[idx].EqualsInput(input))
						return cacheCCIcross[idx];
			return CacheIndicator<CCIcross>(new CCIcross(){ AlertOn = alertOn, AlertSOund = alertSOund, FilterVwap = filterVwap, FilterStdDev1 = filterStdDev1 }, input, ref cacheCCIcross);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CCIcross CCIcross(bool alertOn, string alertSOund, bool filterVwap, bool filterStdDev1)
		{
			return indicator.CCIcross(Input, alertOn, alertSOund, filterVwap, filterStdDev1);
		}

		public Indicators.CCIcross CCIcross(ISeries<double> input , bool alertOn, string alertSOund, bool filterVwap, bool filterStdDev1)
		{
			return indicator.CCIcross(input, alertOn, alertSOund, filterVwap, filterStdDev1);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CCIcross CCIcross(bool alertOn, string alertSOund, bool filterVwap, bool filterStdDev1)
		{
			return indicator.CCIcross(Input, alertOn, alertSOund, filterVwap, filterStdDev1);
		}

		public Indicators.CCIcross CCIcross(ISeries<double> input , bool alertOn, string alertSOund, bool filterVwap, bool filterStdDev1)
		{
			return indicator.CCIcross(input, alertOn, alertSOund, filterVwap, filterStdDev1);
		}
	}
}

#endregion
