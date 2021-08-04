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
using System.Collections;

#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class SerialSequnt : Indicator
	{
		
		#region Variables
		
		ArrayList myAL = new ArrayList();
		
		//private Color wDefaultColor = Color.Black;
		//private FontStyle wDefaultFontStyle = FontStyle.Bold;
		//private Color mwColor = Color.Aqua;
		//private Color mwdColor = Color.Blue;
//		private Color mwNewColor = Color.Yellow;
//		private Color mwdNewColor = Color.Yellow;
//		private Color mwPushedColor = Color.LightGray;
//		private Color mwdPushedColor = Color.LightGray;
//		private Color miwColor = Color.SpringGreen;
//		private Color miwdColor = Color.DarkSlateGray;
//		private Color miwNewColor = Color.Gold;
//		private Color miwdNewColor = Color.Gold;
//		private Color miwPushedColor = Color.Silver;
//		private Color miwdPushedColor = Color.Silver;
//		private Color mwExpColor = Color.Silver;
//		private Color mwdExpColor = Color.Silver;
//		private Color miwExpColor = Color.Silver;
//		private Color miwdExpColor = Color.Silver;
		private const string LOD = "LowOfDay";
		private const string HOD = "HighOfDay";
		private const string MINORLOW = "MinorLow";
		private const string MINORHIGH = "MinorHigh";
		private const string MWU = "MajorWaveUp";
		private const string MWUN = "MajorWaveUpNew";
		private const string MWUP = "MajorWaveUpPushed";
		private const string MWD = "MajorWaveDown";
		private const string MWDN = "MajorWaveDownNew";
		private const string MWDP = "MajorWaveDownPushed";
		private const string MIWU = "MinorWaveUp";
		private const string MIWUN = "MinorWaveUpNew";
		private const string MIWUP = "MinorWaveUpPushed";
		private const string MIWD = "MinorWaveDown";
		private const string MIWDN = "MinorWaveDownNew";
		private const string MIWDP = "MinorWaveDownPushed";
		private bool processCheck;
		private int intBarDirection;
		private int intLastBarDirection;
		private int intProcessingBar;
		private int intPriorBar;
		private int intProcessingBarID;
		private int intPriorBarID;
		private int sessionID;
		private int barsSinceSession;
		private bool isIntradayChart;
		private bool isNewSessionLow;
		private bool isNewSessionHigh;
		private int MajorUpStart;
		private int MajorUpEnd;
		private int MajorUpWave;
		private bool MajorUpConfirmed;
		private bool MajorUpBarOffSet;
		private bool MajorUpNewWaveHigh;
		private int MajorUpUnconfirmedHigh;
		private int MajorUpUnconfirmedRenderedNumber;
		private int MajorUpNewWaveNumber;
		private int MajorUpPushedWaves;
		private int MajorUpPushedBarProcessed;
		private double MajorUpPushedBarProcessedPrice;
		private int MajorUpResetBar;
		private int lastMajorUpUnpushedNumber;
		private int lastMajorUpUnpushedWaveStart;
		private int lastMajorUpUnpushedWaveLevel;
		private int lastMajorUpUnpushedWaveEnd;
		private int lastMajorUpUnpushedWaveEndID;
		private int lastMajorUpNewWaveProcess;
		private int MinorUpStart;
		private int MinorUpEnd;
		private int MinorUpWave;
		private bool MinorUpConfirmed;
		private bool MinorUpBarOffSet;
		private bool MinorUpNewWaveHigh;
		private int MinorUpUnconfirmedHigh;
		private int MinorUpNewWaveNumber;
		private int MinorUpPushedWaves;
		private int MinorUpPushedBarProcessed;
		private double MinorUpPushedBarProcessedPrice;
		private int MinorUpResetBar;
		private int lastMinorUpUnpushedNumber;
		private int lastMinorUpUnpushedWaveStart;
		private int lastMinorUpUnpushedWaveLevel;
		private int lastMinorUpUnpushedWaveEnd;
		private int lastMinorUpUnpushedWaveEndID;
		private int lastMinorUpNewWaveProcess;
		private bool isMinorUpReset;
		private int MajorDownStart;
		private int MajorDownEnd;
		private int MajorDownWave;
		private bool MajorDownConfirmed;
		private bool MajorDownBarOffSet;
		private bool MajorDownNewWaveLow;
		private int MajorDownUnconfirmedLow;
		private int MajorDownNewWaveNumber;
		private int MajorDownPushedWaves;
		private int MajorDownPushedBarProcessed;
		private double MajorDownPushedBarProcessedPrice;
		private int MajorDownResetBar;
		private int lastMajorDownUnpushedNumber;
		private int lastMajorDownUnpushedWaveStart;
		private int lastMajorDownUnpushedWaveLevel;
		private int lastMajorDownUnpushedWaveEnd;
		private int lastMajorDownUnpushedWaveEndID;
		private int lastMajorDownNewWaveProcess;
		private int MinorDownStart;
		private int MinorDownEnd;
		private int MinorDownWave;
		private bool MinorDownConfirmed;
		private bool MinorDownBarOffSet;
		private bool MinorDownNewWaveLow;
		private int MinorDownUnconfirmedLow;
		private int MinorDownNewWaveNumber;
		private int MinorDownPushedWaves;
		private int MinorDownPushedBarProcessed;
		private double MinorDownPushedBarProcessedPrice;
		private int MinorDownResetBar;
		private int lastMinorDownUnpushedNumber;
		private int lastMinorDownUnpushedWaveStart;
		private int lastMinorDownUnpushedWaveLevel;
		private int lastMinorDownUnpushedWaveEnd;
		private int lastMinorDownUnpushedWaveEndID;
		private int lastMinorDownNewWaveProcess;
		private bool isMinorDownReset;
		private bool mwShow = true;
		private bool mwpShow = true;
		private bool miShow = true;
		private bool mipShow = true;
		private bool renderNumbersHorizontal;
		private string indicatorPassword = "";
		private bool indicatorCurrentSessionOnly;
		private bool doNotRun;
		private int indicatorOpacity;
		private Color indicatorOutlineColor;
		private Color indicatorBackColor;
		private string indicatorFont = "Arial";
		private int indicatorSpacing = 2;
		private int indicatorSpacingBetweenNumbers = 2;
		private double indSpacing = 2.0;
		private double indNumberSpacing = 2.0;
		private int mwSize = 10;
		private int miwSize = 8;
		private int wDefaultFontSize = 12;
		private string currentDateString = DateTime.Now.ToString("yyyyMMdd");
		private int isToday;
		
		private bool indicatorDebug;	// = true;
		private bool debugCommon = true;
		private bool debugCommonEachTick;
		private bool debugBarNumber;
		private bool debugTempArray;
		private bool debugLines;
		private bool debugCommonDown;
		private bool debugCommonDownEachTick;
		private bool debugTempArrayDown;
		
		// me
		private bool 		PrintedThree;
		private int 		lastBar;
		//private DataSeries 	yDataSeries;
		//private DataSeries 	valueDataSeries;
		private	double 		SeqMaxHi;	
		private	int 		SeqMaxBarHi;
		private	double 		SeqMaxLo;	
		private	int 		SeqMaxBarLo;
		private bool 		ShowArrows;
		private	double 		LastSeqLo;
		private	double 		LastSeqHi;
		
		// add audio
		private bool 		audio = true; 
		private string 		LowThreeSoundFile 	= "Low3.wav";
		private string 		HighThreeSoundFile 	= "High3.wav";		
		private string 		LowSevenSoundFile 	= "Low7.wav";	//7
		private string 		HiSevenSoundFile 	= "High7.wav";	//7	
		private string 		ThirteenSoundFile 	= "13.wav";//13
		private string 		SeventeenSoundFile 	= "17.wav";
		private string 		TwentyoneSoundFile 	= "21.wav";		
		private string 		SubSoundFile 		= "Sub.wav";
		
		// data
		// private ArrayList arrMajorUp;
		///private Series<double> arrMajorUp;
		ArrayList arrMajorUp = new ArrayList();
		//private ArrayList arrMinorUp;
		///private Series<double> arrMinorUp;
		ArrayList arrMinorUp = new ArrayList();
		//private ArrayList arrMajorDown;
		///private Series<double> arrMajorDown;
		ArrayList arrMajorDown = new ArrayList();
		//private ArrayList arrMinorDown;
		///private Series<double> arrMinorDown;
		ArrayList arrMinorDown = new ArrayList();
		
		#endregion
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Wave Counter System";
				Name										= "Serial Sequent";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= false;
				DrawVerticalGridLines						= false;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive			= true;
				
				MajorWaveDownColor					= Brushes.DodgerBlue;
				MajorWaveDownUnconfirmedColor		= Brushes.Yellow;
				
				MajorWaveUpPushedColor				= Brushes.LightGray;
				MajorWaveUpUnconfirmedColor			= Brushes.Yellow;
				MajorWaveUpColor					= Brushes.DodgerBlue;
				
				MinorWaveUpColor					= Brushes.LightGray;
				MinorWaveDownColor					= Brushes.LightGray;
				MinorWaveUpUnconfirmedColor			= Brushes.LightGray;
				MinorWaveDownUnconfirmedColor		= Brushes.LightGray;
				MinorWaveUpPushedColor				= Brushes.LightGray;
				MinorWaveDownPushedColor			= Brushes.LightGray;
				wDefaultColor						= Brushes.LightGray;
				
				CurrentSessionOnly					= true;
				Space						= 4;
				SpaceBetween				= 2;
				RenderHorizontal					= true;
				ShowMajor					= true;
				ShowMinor					= true;
				ShowMajorPushed					= true;
				ShowMinorPushed					= true;
				MajorFontSize					= 25;
				MinorFontSize					= 12;
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Bar, "UpWave");
				AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Bar, "DownWave");
				AddPlot(Brushes.Red, "DownWave2");
				AddPlot(Brushes.Green, "UpWave2");
				AddLine(Brushes.Silver, 1, "Zero");
				
				isNewSessionLow = false;
				isNewSessionHigh = false;
				if (indicatorOpacity > 10)
				{
					indicatorOpacity = 1;
					if (indicatorDebug)
					{
						Print("Serial Sequent Indicator: invalid Opacity specified.");
					}
				}
				if (indSpacing < 0.0 || indSpacing > 10.0)
				{
					indSpacing = 0.5;
					if (indicatorDebug)
					{
						Print("Serial Sequent Indicator: invalid Space specified.");
					}
				}
				debugCommon = false;
				debugBarNumber = false;
				debugCommonEachTick = false;
				debugTempArray = false;
				debugLines = false;
				debugTempArrayDown = false;
				debugCommonDown = false;
				debugCommonDownEachTick = false;
				
			}
			else if (State == State.Configure)
			{
				bool flag = BarsPeriod.BarsPeriodType == BarsPeriodType.Renko || BarsPeriod.BarsPeriodType == BarsPeriodType.Kagi 
					|| BarsPeriod.BarsPeriodType == BarsPeriodType.LineBreak || BarsPeriod.BarsPeriodType == BarsPeriodType.PointAndFigure;
			}
		}

		protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
		{
			
		}

		///  Mark: - TODO - Use Color inputs
		
		protected override void OnBarUpdate()
		{
			Print("Space = " + Space);
			try
			{
				if ((this.debugCommon && IsFirstTickOfBar ) || this.debugCommonEachTick)
				{
					base.Print("-------START----------");
				}
				if (this.debugBarNumber)
				{
					double num = High[0];
					double num2 = Low[0];
					double num3 = num - num2;
					double num4 = num3 / 2.0;
					double num5 = High[0] - num4;
					//DrawText("CurrentBar" + CurrentBar, true, string.Concat( CurrentBar), 0, num5, 1, Color.Black, new Font("Arial", 8f, FontStyle.Regular), StringAlignment.Center, Color.Transparent, Color.Transparent, 100);
					Draw.Text(this, "CurrentBar" + CurrentBar, string.Concat( CurrentBar), 0, num5, Brushes.Black);
					
				}
				//if ( BarsPeriod.Id != 5 &&  BarsPeriod.Id != 6 &&  BarsPeriod.Id != 7) // PeriodType.Day
				if ( BarsPeriod.BarsPeriodType != BarsPeriodType.Day &&  BarsPeriod.BarsPeriodType != BarsPeriodType.Week &&  BarsPeriod.BarsPeriodType != BarsPeriodType.Month)  
				{
					if ( BarsPeriod.BarsPeriodType !=  BarsPeriodType.Year )	// if ( BarsPeriod.Id  != 8)
					{
						this.isIntradayChart = true;
						if (this.indicatorCurrentSessionOnly)
						{
						// this.isToday = base.ToDay(base.get_Time().get_Item(0)).ToString().CompareTo(this.currentDateString);
						//this.isToday = base.ToDay( Time[0] ).ToString().CompareTo( currentDateString );
							//if (this.isToday != 0)
							if (Time[0].Date == DateTime.Today)
							{
								return;
							}
						}
						//	if (base.get_Bars().get_FirstBarOfSession() && IsFirstTickOfBar)
						if( Bars.IsFirstBarOfSession && IsFirstTickOfBar )
						{
							this.sessionID = CurrentBar;
							this.resetMajorUp( CurrentBar );
							this.resetMajorDown( CurrentBar );
							this.resetMinorUp( CurrentBar );
							this.resetMinorDown( CurrentBar );
						}
						this.barsSinceSession = Bars.BarsSinceNewTradingDay;
						if (this.barsSinceSession == 0)
						{
							if (this.getBarDirection( CurrentBar ) == 1)
							{
								this.renderNumber("SSWaveNumber", "1", 0, High[0], 1, "MajorWaveUpNew", false);
								base.RemoveDrawObject("SSWaveDownNumber");
							}
							else
							{
								this.renderNumber("SSWaveDownNumber", "1", 0, Low[0], 1, "MajorWaveDownNew", false);
								base.RemoveDrawObject("SSWaveNumber");
							}
							return;
						}
						if (this.barsSinceSession == 1)
						{
							if ( Low[0] < Low[1] )
							{
								this.renderNumber("SSWaveDownNumber", "1", 0, Low[0], 1, "MajorWaveDownNew", false);
								base.RemoveDrawObject("SSWaveNumber");
								this.MajorDownUnconfirmedLow = CurrentBar;
							}
							else
							{
								if ( High[0] > High[1] )
								{
									this.renderNumber("SSWaveNumber", "1", 0, High[0], 1, "MajorWaveUpNew", false);
									base.RemoveDrawObject("SSWaveDownNumber");
									this.MajorUpUnconfirmedHigh = CurrentBar;
								}
							}
							return;
						}
						if ( Bars.BarsSinceNewTradingDay < 2)
						{
							return;
						}
						goto IL_34C;
					}
				}
				this.isIntradayChart = false;
				if ( CurrentBar == 0 )
				{
					this.sessionID = CurrentBar;
					this.resetMajorUp( CurrentBar );
					this.resetMajorDown( CurrentBar );
				}
				if ( CurrentBar  < 2)
				{
					return;
				}
				IL_34C:
				if (IsFirstTickOfBar)
				{
					this.MajorUpUnconfirmedRenderedNumber = 0;
					this.intProcessingBar =  CurrentBar  - 1;
					this.intBarDirection = this.getBarDirection(this.intProcessingBar);
					this.intPriorBar =  CurrentBar  - 2;
					this.intLastBarDirection = this.getBarDirection(this.intPriorBar);
					this.intProcessingBarID =  CurrentBar  - this.intProcessingBar;
					this.intPriorBarID =  CurrentBar  - this.intPriorBar;
					if ((this.debugCommon && IsFirstTickOfBar) || this.debugCommonEachTick)
					{
						base.Print("-------------> OPEN START");
						Print( Time[0].ToString() ) ;
						base.Print("CurrentBar=" +  CurrentBar );
						base.Print("arrMajorUp.Count=" + this.arrMajorUp.Count);
						base.Print("intProcessingBar=" + this.intProcessingBar);
						base.Print("intBarDirection=" + this.intBarDirection);
						base.Print("intPriorBar=" + this.intPriorBar);
						base.Print("intLastBarDirection=" + this.intLastBarDirection);
						base.Print("MajorUpStart=" + this.MajorUpStart);
						base.Print("MajorUpEnd=" + this.MajorUpEnd);
						base.Print("arrMajorUp.Count=" + this.arrMajorUp.Count);
						base.Print("MajorUpNewWaveHigh: " + this.MajorUpNewWaveHigh);
						base.Print("MajorUpConfirmed: " + this.MajorUpConfirmed);
						base.Print("MajorUpBarOffSet: " + this.MajorUpBarOffSet);
						base.Print("arrMinorUp.Count=" + this.arrMinorUp.Count);
						base.Print("MinorUpStart=" + this.MinorUpStart);
						base.Print("MinorUpEnd=" + this.MinorUpEnd);
						base.Print("arrMinorUp.Count=" + this.arrMinorUp.Count);
						base.Print("MinorUpNewWaveHigh: " + this.MinorUpNewWaveHigh);
						base.Print("MinorUpConfirmed: " + this.MinorUpConfirmed);
						base.Print("MinorUpBarOffSet: " + this.MinorUpBarOffSet);
						base.Print("-------------> OPEN END");
					}
					//Print( Time[0].ToString() ) ;
					//base.Print("arrMajorUp.Count=" + this.arrMajorUp.Count);
					//base.Print("MajorUpEnd=" + this.MajorUpEnd);
					//base.Print("MajorUpNewWaveHigh: " + this.MajorUpNewWaveHigh);
					//base.Print("arrMajorDown.Count=" + this.arrMajorDown.Count);
					if ((this.debugCommonDown && IsFirstTickOfBar) || this.debugCommonDownEachTick)
					{
						base.Print("-------------> OPEN START");
						base.Print("CurrentBar=" +  CurrentBar );
						base.Print("arrMajorDown.Count=" + this.arrMajorDown.Count);
						base.Print("intProcessingBar=" + this.intProcessingBar);
						base.Print("intBarDirection=" + this.intBarDirection);
						base.Print("intPriorBar=" + this.intPriorBar);
						base.Print("intLastBarDirection=" + this.intLastBarDirection);
						base.Print("MajorDownStart=" + this.MajorDownStart);
						base.Print("MajorDownEnd=" + this.MajorDownEnd);
						base.Print("arrMajorDown.Count=" + this.arrMajorDown.Count);
						base.Print("MajorDownNewWaveLow: " + this.MajorDownNewWaveLow);
						base.Print("MajorDownConfirmed: " + this.MajorDownConfirmed);
						base.Print("MajorDownBarOffSet: " + this.MajorDownBarOffSet);
						base.Print("-------------> OPEN END");
					}
					if (this.arrMajorUp.Count > 4)
					{
						if ( Low[ this.intProcessingBarID ] < Low[ CurrentBar  - this.MajorUpStart ] && !MajorUpNewWaveHigh  )
						{
							this.MajorUpStart = this.intProcessingBar;
						}
						if ( High[ this.intProcessingBarID ] > this.getMajorUpLastWaveHigh() || this.MajorUpNewWaveHigh) // might be wrong 
						{
							if (this.isMajorUpWaveConfirmed(this.intProcessingBar, this.intPriorBar))
							{
								this.MajorUpConfirmed = true;
								this.setMajorUpWaveEnd();
							}
							this.MajorUpNewWaveHigh = true;
						}
						if ( Low[ this.intProcessingBarID ] <= Low[ CurrentBar  - Convert.ToInt32(this.arrMajorUp[0])] )	// might be wrong not sure what to include in Low bracket
						{
							this.processMajorUpPushedWaves(this.intProcessingBarID);
							this.resetMajorUp(this.intProcessingBar);
						}
					}
					else
					{
						if ( Low[ this.intProcessingBarID ] <= Low[ CurrentBar  - this.MajorUpStart] )
						{
							this.resetMajorUp(this.intProcessingBar);
						}
						else
						{
							if (this.isMajorUpWaveConfirmed(this.intProcessingBar, this.intPriorBar))
							{
								this.MajorUpNewWaveHigh = true;
								this.MajorUpConfirmed = true;
								this.setMajorUpWaveEnd();
							}
							if ((this.debugCommon && IsFirstTickOfBar) || this.debugCommonEachTick)
							{
								base.Print("Setting MajorUpConfirmed: " + this.MajorUpConfirmed);
							}
						}
					}
					if (this.arrMinorUp.Count > 4)
					{
						if (!this.MajorUpNewWaveHigh)
						{
							if ( High[ this.intProcessingBarID ] != this.getMajorUpLastWaveHigh() )
							{
								if (!this.MajorUpNewWaveHigh)
								{
									if ( Low[ this.intProcessingBarID ] < Low[ CurrentBar  - this.MinorUpStart] && !this.MinorUpNewWaveHigh )
									{
										this.MinorUpStart = this.intProcessingBar;
									}
									if ( High[ this.intProcessingBarID ] > this.getMinorUpLastWaveHigh() || this.MinorUpNewWaveHigh )
									{
										if (this.isMinorUpWaveConfirmed(this.intProcessingBar, this.intPriorBar))
										{
											this.MinorUpConfirmed = true;
											this.setMinorUpWaveEnd();
										}
										this.MinorUpNewWaveHigh = true;
									}
								}
								if ( Low[ this.intProcessingBarID ] <= Low[ CurrentBar  - Convert.ToInt32(this.arrMinorUp[0]) ] )
								{
									this.processMinorUpPushedWaves(this.intProcessingBarID);
									this.resetMinorUp(this.intProcessingBar);
									goto IL_B58;
								}
								goto IL_B58;
							}
						}
						this.isMinorUpReset = true;
						this.processMinorUpPushedWaves(this.intProcessingBarID);
						this.resetMinorUp(this.intProcessingBar);
					}
					else
					{
						if (!this.MajorUpNewWaveHigh)
						{
							if ( High[ this.intProcessingBarID ] != this.getMajorUpLastWaveHigh() )
							{
								if ( Low[ this.intProcessingBarID ] <= Low[ CurrentBar  - this.MajorUpStart ] || Low[ this.intProcessingBarID ] <= Low[ CurrentBar  - this.MinorUpStart ] )
								{
									this.processMinorUpPushedWaves(this.intProcessingBarID);
									this.resetMinorUp(this.intProcessingBar);
									goto IL_B58;
								}
								if (this.isMinorUpWaveConfirmed(this.intProcessingBar, this.intPriorBar))
								{
									this.MinorUpNewWaveHigh = true;
									this.MinorUpConfirmed = true;
									this.setMinorUpWaveEnd();
								}
								if ((this.debugCommon && IsFirstTickOfBar) || this.debugCommonEachTick)
								{
									base.Print("Setting MinorUpConfirmed: " + this.MinorUpConfirmed);
									goto IL_B58;
								}
								goto IL_B58;
							}
						}
						this.resetMinorUp(this.intProcessingBar);
					}
					IL_B58:
					if (this.MajorUpNewWaveHigh && this.MajorUpConfirmed)
					{
						this.MajorUpUnconfirmedHigh = -1;
						this.processMajorUpWaves(this.MajorUpStart, this.MajorUpEnd, this.MajorUpBarOffSet, this.sessionID, true);
						this.setMajorUpWaveStart(this.intProcessingBar);
						this.MajorUpPushedWaves = 0;
						this.MajorUpPushedBarProcessed = -1;
						this.MajorUpPushedBarProcessedPrice = -1.0;
					}
					if (this.arrMajorUp.Count > 4)
					{
						this.processMajorUpPushedWaves(this.intProcessingBarID);
					}
					if (this.MinorUpNewWaveHigh && this.MinorUpConfirmed)
					{
						this.MinorUpUnconfirmedHigh = -1;
						this.processMinorUpWaves(this.MinorUpStart, this.MinorUpEnd, this.MinorUpBarOffSet, this.sessionID, true);
						this.setMinorUpWaveStart(this.intProcessingBar);
						this.MinorUpPushedWaves = 0;
						this.MinorUpPushedBarProcessed = -1;
						this.MinorUpPushedBarProcessedPrice = -1.0;
					}
					if (this.arrMinorUp.Count > 4)
					{
						this.processMinorUpPushedWaves(this.intProcessingBarID);
					}
					if (this.arrMajorDown.Count > 4)
					{
						if ( High[ this.intProcessingBarID ] > High[  CurrentBar  - this.MajorDownStart ] && !this.MajorDownNewWaveLow )
						{
							this.MajorDownStart = this.intProcessingBar;
						}
						if ( Low[ this.intProcessingBarID ] < this.getMajorDownLastWaveLow() || this.MajorDownNewWaveLow)
						{
							if (this.isMajorDownWaveConfirmed(this.intProcessingBar, this.intPriorBar))
							{
								this.MajorDownConfirmed = true;
								this.setMajorDownWaveEnd();
							}
							this.MajorDownNewWaveLow = true;
						}
						if ( High[ this.intProcessingBarID ] >= High[ CurrentBar  - Convert.ToInt32(this.arrMajorDown[0] ) ] )
						{
							this.processMajorDownPushedWaves(this.intProcessingBarID);
							this.resetMajorDown(this.intProcessingBar);
						}
					}
					else
					{
						if ( High[ this.intProcessingBarID ] >= High[ CurrentBar  - this.MajorDownStart] )
						{
							this.resetMajorDown(this.intProcessingBar);
						}
						else
						{
							if (this.isMajorDownWaveConfirmed(this.intProcessingBar, this.intPriorBar))
							{
								this.MajorDownNewWaveLow = true;
								this.MajorDownConfirmed = true;
								this.setMajorDownWaveEnd();
							}
							if ((this.debugCommon && IsFirstTickOfBar) || this.debugCommonEachTick)
							{
								base.Print("Setting MajorDownConfirmed: " + this.MajorDownConfirmed);
							}
						}
					}
					if (this.arrMinorDown.Count > 4)
					{
						if (!this.MajorDownNewWaveLow)
						{
							if ( Low[ this.intProcessingBarID ] != this.getMajorDownLastWaveLow())
							{
								if (!this.MajorDownNewWaveLow)
								{
									if ( High[ this.intProcessingBarID ] > High[ CurrentBar  - this.MinorDownStart ] && !this.MinorDownNewWaveLow)
									{
										this.MinorDownStart = this.intProcessingBar;
									}
									if ( Low[ this.intProcessingBarID ] < this.getMinorDownLastWaveLow() || this.MinorDownNewWaveLow)
									{
										if (this.isMinorDownWaveConfirmed(this.intProcessingBar, this.intPriorBar))
										{
											this.MinorDownConfirmed = true;
											this.setMinorDownWaveEnd();
										}
										this.MinorDownNewWaveLow = true;
									}
								}
								if ( High[ this.intProcessingBarID ] >= High[ CurrentBar  - Convert.ToInt32(this.arrMinorDown[0])])
								{
									this.processMinorDownPushedWaves(this.intProcessingBarID);
									this.resetMinorDown(this.intProcessingBar);
									goto IL_1019;
								}
								goto IL_1019;
							}
						}
						this.isMinorDownReset = true;
						this.processMinorDownPushedWaves(this.intProcessingBarID);
						this.resetMinorDown(this.intProcessingBar);
					}
					else
					{
						if (!this.MajorDownNewWaveLow)
						{
							if ( Low[ this.intProcessingBarID ] != this.getMajorDownLastWaveLow())
							{
								if ( High[ this.intProcessingBarID ] >= High[ CurrentBar  - this.MajorDownStart] || High[ this.intProcessingBarID ] >= High[ CurrentBar  - this.MinorDownStart ] )
								{
									this.processMinorDownPushedWaves(this.intProcessingBarID);
									this.resetMinorDown(this.intProcessingBar);
									goto IL_1019;
								}
								if (this.isMinorDownWaveConfirmed(this.intProcessingBar, this.intPriorBar))
								{
									this.MinorDownNewWaveLow = true;
									this.MinorDownConfirmed = true;
									this.setMinorDownWaveEnd();
								}
								if ((this.debugCommon && IsFirstTickOfBar) || this.debugCommonEachTick)
								{
									base.Print("Setting MinorDownConfirmed: " + this.MinorDownConfirmed);
									goto IL_1019;
								}
								goto IL_1019;
							}
						}
						this.resetMinorDown(this.intProcessingBar);
					}
IL_1019:
					if (this.MajorDownNewWaveLow && this.MajorDownConfirmed)
					{
						this.MajorDownUnconfirmedLow = -1;
						this.processMajorDownWaves(this.MajorDownStart, this.MajorDownEnd, this.MajorDownBarOffSet, this.sessionID, true);
						this.setMajorDownWaveStart(this.intProcessingBar);
						this.MajorDownPushedWaves = 0;
						this.MajorDownPushedBarProcessed = -1;
						this.MajorDownPushedBarProcessedPrice = -1.0;
					}
					if (this.arrMajorDown.Count > 4)
					{
						this.processMajorDownPushedWaves(this.intProcessingBarID);
					}
					if (this.MinorDownNewWaveLow && this.MinorDownConfirmed)
					{
						this.MinorDownUnconfirmedLow = -1;
						this.processMinorDownWaves(this.MinorDownStart, this.MinorDownEnd, this.MinorDownBarOffSet, this.sessionID, true);
						this.setMinorDownWaveStart(this.intProcessingBar);
						this.MinorDownPushedWaves = 0;
						this.MinorDownPushedBarProcessed = -1;
						this.MinorDownPushedBarProcessedPrice = -1.0;
					}
					if (this.arrMinorDown.Count > 4)
					{
						this.processMinorDownPushedWaves(this.intProcessingBarID);
					}
				}
				this.isNewSessionLow = false;
				if (this.arrMajorUp.Count > 4)
				{
					if ( Low[0] <= Low[ CurrentBar  - Convert.ToInt32(this.arrMajorUp[0])] )
					{
						this.isNewSessionLow = true;
					}
				}
				else
				{
					if (Low[0] <= Low[ CurrentBar  - this.MajorUpStart ] )
					{
						this.isNewSessionLow = true;
					}
				}
				if ((this.debugCommon && IsFirstTickOfBar) || this.debugCommonEachTick)
				{
					base.Print("isNewSessionLow=" + this.isNewSessionLow);
				}
				if (this.isNewSessionLow)
				{
					base.RemoveDrawObject("SSWaveNumber" + this.sessionID + this.MajorUpNewWaveNumber);
				}
				bool flag = false;
				if (Low[0] <= this.getMajorUpLastWaveLow())
				{
					if ( High[ 0 ] <= this.getMajorUpLastWaveHigh())
					{
						this.processMajorUpPushedWaves(0);
					}
					flag = true;
				}
				if (this.isNewSessionLow)
				{
					this.arrMajorUp.Clear();
					this.setMajorUpWaveStart( CurrentBar );
					this.MajorUpUnconfirmedHigh =  CurrentBar ;
				}
				if ( High[ 0 ] > this.getMajorUpLastWaveHigh() || this.MajorUpUnconfirmedHigh > -1)
				{
					bool flag2 = false;
					if (this.MajorUpUnconfirmedHigh ==  CurrentBar )
					{
						flag2 = true;
					}
					else
					{
						if (this.MajorUpUnconfirmedHigh == -1)
						{
							flag2 = true;
						}
						else
						{
							if ( High[ CurrentBar  - this.MajorUpUnconfirmedHigh ] < High[ 0 ] )
							{
								flag2 = true;
							}
							if (this.MajorUpBarOffSet && this.intBarDirection == -1)
							{
								flag2 = true;
							}
						}
					}
					if (flag2)
					{
						this.MajorUpUnconfirmedHigh =  CurrentBar ;
						if (!flag)
						{
							this.processMajorUpPushedWaves(0);
						}
						if (this.MajorUpNewWaveNumber == 1)
						{
							
							if (this.getBarDirection( CurrentBar ) == -1 && this.isNewSessionLow)
							{
								base.RemoveDrawObject("SSWaveNumber");
							}
							else
							{
								this.renderNumber("SSWaveNumber", this.MajorUpNewWaveNumber.ToString(), 0, High[ 0 ], 1, "MajorWaveUpNew", false);
							}
//*******************************************************************************************************************
//****************************		Major Swing		*****************************************************************
//*******************************************************************************************************************
							
//if( MajorUpNewWaveNumber.ToString() == "1" || MajorUpNewWaveNumber.ToString() == "13" || MajorUpNewWaveNumber.ToString() == "21")
//	DrawDot("MajorUpNewWaveNumber"+CurrentBar, true, 0, High[0] + 2 * TickSize, Color.Red);
//Print( MajorUpNewWaveNumber.ToString() );
						}
						else
						{
							this.renderNumber("SSWaveNumber", this.MajorUpNewWaveNumber.ToString(), 0, High[ 0 ], 1, "MajorWaveUpNew", false);
						}
						if (this.MajorUpNewWaveNumber > 1 && this.MajorUpPushedWaves > 0 && !this.isNewSessionLow && this.arrMajorUp.Count > 5 &&  CurrentBar  != this.lastMajorUpNewWaveProcess)
						{
							this.lastMajorUpNewWaveProcess =  CurrentBar ;
							this.lastMajorUpUnpushedNumber = this.MajorUpNewWaveNumber - 2;
							this.lastMajorUpUnpushedWaveEnd = Convert.ToInt32(this.arrMajorUp[this.arrMajorUp.Count - 4]);
							this.lastMajorUpUnpushedWaveEndID =  CurrentBar  - this.lastMajorUpUnpushedWaveEnd;
							this.renderNumber("SSWaveNumber" + this.sessionID + this.lastMajorUpUnpushedNumber, this.lastMajorUpUnpushedNumber.ToString(), this.lastMajorUpUnpushedWaveEndID, High[ this.lastMajorUpUnpushedWaveEndID ], 2, "MajorWaveUp", false);
							if (this.renderNumbersHorizontal)
							{
								int num6 = Convert.ToInt32(this.arrMajorUp[this.arrMajorUp.Count - 5]);
								int arg_147E_0 =  CurrentBar ;
								Convert.ToInt32(this.arrMajorUp[this.arrMajorUp.Count - 4]);
								int arg_14A3_0 =  CurrentBar ;
								int num7 = Convert.ToInt32(this.arrMajorUp[this.arrMajorUp.Count - 3]);
								this.renderNumber(string.Concat(new object[]
								{
									"SSWaveNumberObsolete",
									this.sessionID,
									num7,
									num6,
									num6
								}), num7.ToString(), this.lastMajorUpUnpushedWaveEndID, High[ this.lastMajorUpUnpushedWaveEndID ], 1, "MajorWaveUpPushed", true);
							}
							int num8 = this.MajorUpPushedWaves * 5 + 5;
							int num9 = this.MajorUpPushedWaves * 5 + 2;
							int num10 = this.MajorUpPushedWaves * 5 + 4;
							this.lastMajorUpUnpushedWaveStart = Convert.ToInt32(this.arrMajorUp[this.arrMajorUp.Count - num8]);
							this.lastMajorUpUnpushedWaveLevel = Convert.ToInt32(this.arrMajorUp[this.arrMajorUp.Count - num9]);
							this.lastMajorUpUnpushedWaveEnd = Convert.ToInt32(this.arrMajorUp[this.arrMajorUp.Count - num10]);
							this.lastMajorUpUnpushedWaveEndID =  CurrentBar  - this.lastMajorUpUnpushedWaveEnd;
							this.renderNumber(string.Concat(new object[]
							{
								"SSWaveNumberObsolete",
								this.sessionID,
								this.lastMajorUpUnpushedNumber,
								this.lastMajorUpUnpushedWaveStart,
								this.lastMajorUpUnpushedWaveEnd
							}), this.lastMajorUpUnpushedNumber.ToString(), this.lastMajorUpUnpushedWaveEndID, High[ this.lastMajorUpUnpushedWaveEndID ], this.lastMajorUpUnpushedWaveLevel, "MajorWaveUpPushed", false);
						}
					}
				}
				if ( High[ 0 ] >= this.getMajorUpLastWaveHigh())
				{
					base.RemoveDrawObject("SSMinorWaveNumber");
				}
				if ( High[ 0 ] < this.getMajorUpLastWaveHigh())
				{
					bool flag3 = false;
					if (Low[0] <= this.getMinorUpLastWaveLow())
					{
						if ( High[ 0 ] <= this.getMinorUpLastWaveHigh())
						{
							this.processMinorUpPushedWaves(0);
						}
						flag3 = true;
					}
					if (this.isNewSessionLow || High[ 0 ] >= this.getMajorUpLastWaveHigh())
					{
						base.RemoveDrawObject("SSMinorWaveNumber");
						this.arrMinorUp.Clear();
						this.setMinorUpWaveStart( CurrentBar );
						this.MinorUpUnconfirmedHigh =  CurrentBar ;
					}
					if ( High[ 0 ] > this.getMinorUpLastWaveHigh() || this.MinorUpUnconfirmedHigh > -1)
					{
						bool flag4 = false;
						if (this.MinorUpUnconfirmedHigh ==  CurrentBar )
						{
							flag4 = true;
						}
						else
						{
							if (this.MinorUpUnconfirmedHigh == -1)
							{
								flag4 = true;
							}
							else
							{
								if ( High[ CurrentBar  - this.MinorUpUnconfirmedHigh ] < High[ 0 ] )
								{
									flag4 = true;
								}
								if (this.MinorUpBarOffSet && this.intBarDirection == -1)
								{
									flag4 = true;
								}
							}
						}
						if (flag4)
						{
							this.MinorUpUnconfirmedHigh =  CurrentBar ;
							if (!flag3)
							{
								this.processMinorUpPushedWaves(0);
							}
							if (this.MinorUpNewWaveNumber == 1)
							{
							if (this.getBarDirection( CurrentBar ) == -1 && this.isNewSessionLow)
								{
									base.RemoveDrawObject("SSMinorWaveNumber");
								}
							else
								{
									this.renderNumber("SSMinorWaveNumber", this.MinorUpNewWaveNumber.ToString(), 0, High[ 0 ], 1, "MinorWaveUpNew", false);
								}
							}
							else
							{
								this.renderNumber("SSMinorWaveNumber", this.MinorUpNewWaveNumber.ToString(), 0, High[ 0 ], 1, "MinorWaveUpNew", false);
							}
							if (this.MinorUpNewWaveNumber > 1 && this.MinorUpPushedWaves > 0 && !this.isNewSessionLow && this.arrMinorUp.Count > 5 &&  CurrentBar  != this.lastMinorUpNewWaveProcess)
							{
								this.lastMinorUpNewWaveProcess =  CurrentBar ;
								this.lastMinorUpUnpushedNumber = this.MinorUpNewWaveNumber - 2;
								this.lastMinorUpUnpushedWaveEnd = Convert.ToInt32(this.arrMinorUp[this.arrMinorUp.Count - 4]);
								this.lastMinorUpUnpushedWaveEndID =  CurrentBar  - this.lastMinorUpUnpushedWaveEnd;
								this.renderNumber("SSMinorWaveNumber" + this.sessionID + this.lastMinorUpUnpushedNumber, this.lastMinorUpUnpushedNumber.ToString(), this.lastMinorUpUnpushedWaveEndID, High[ this.lastMinorUpUnpushedWaveEndID ], 2, "MinorWaveUp", false);
								if (this.renderNumbersHorizontal)
								{
									int num11 = Convert.ToInt32(this.arrMinorUp[this.arrMinorUp.Count - 5]);
									int arg_194C_0 =  CurrentBar ;
									int num12 = Convert.ToInt32(this.arrMinorUp[this.arrMinorUp.Count - 4]);
									int arg_1972_0 =  CurrentBar ;
									int num13 = Convert.ToInt32(this.arrMinorUp[this.arrMinorUp.Count - 3]);
									this.renderNumber(string.Concat(new object[]
									{
										"SSWaveNumberObsolete",
										this.sessionID,
										num13,
										num11,
										num12
									}), num13.ToString(), this.lastMinorUpUnpushedWaveEndID, High[ this.lastMinorUpUnpushedWaveEndID ], 1, "MajorWaveUpPushed", true);
								}
								int num14 = this.MinorUpPushedWaves * 5 + 5;
								int num15 = this.MinorUpPushedWaves * 5 + 2;
								int num16 = this.MinorUpPushedWaves * 5 + 4;
								this.lastMinorUpUnpushedWaveStart = Convert.ToInt32(this.arrMinorUp[this.arrMinorUp.Count - num14]);
								this.lastMinorUpUnpushedWaveLevel = Convert.ToInt32(this.arrMinorUp[this.arrMinorUp.Count - num15]);
								this.lastMinorUpUnpushedWaveEnd = Convert.ToInt32(this.arrMinorUp[this.arrMinorUp.Count - num16]);
								this.lastMinorUpUnpushedWaveEndID =  CurrentBar  - this.lastMinorUpUnpushedWaveEnd;
								this.renderNumber(string.Concat(new object[]
								{
									"SSMinorWaveNumberObsolete",
									this.sessionID,
									this.lastMinorUpUnpushedNumber,
									this.lastMinorUpUnpushedWaveStart,
									this.lastMinorUpUnpushedWaveEnd
								}), this.lastMinorUpUnpushedNumber.ToString(), this.lastMinorUpUnpushedWaveEndID,  High[ this.lastMinorUpUnpushedWaveEndID ], this.lastMinorUpUnpushedWaveLevel, "MinorWaveUpPushed", false);
							}
						}
					}
				}
				this.isNewSessionHigh = false;
				if (this.arrMajorDown.Count > 4)
				{
					if ( High[ 0 ] >=  High[ CurrentBar  - Convert.ToInt32(this.arrMajorDown[0]) ] )
					{
						this.isNewSessionHigh = true;
					}
				}
				else
				{
					if ( High[ 0 ] >= High[ CurrentBar  - this.MajorDownStart ])
					{
						this.isNewSessionHigh = true;
					}
				}
				if ((this.debugCommonDown && IsFirstTickOfBar) || this.debugCommonDownEachTick)
				{
					base.Print("isNewSessionHigh=" + this.isNewSessionHigh);
				}
				if (this.isNewSessionHigh)
				{
					base.RemoveDrawObject("SSWaveDownNumber" + this.sessionID + this.MajorDownNewWaveNumber);
				}
				bool flag5 = false;
				if (Low[0] <= this.getMajorUpLastWaveLow())
				{
					if ( High[ 0 ] <= this.getMajorUpLastWaveHigh())
					{
						this.processMajorUpPushedWaves(0);
					}
				}
				if ( High[ 0 ] >= this.getMajorDownLastWaveHigh())
				{
					if (Low[0] >= this.getMajorDownLastWaveLow())
					{
						this.processMajorDownPushedWaves(0);
					}
					flag5 = true;
				}
				if (this.isNewSessionHigh)
				{
					this.arrMajorDown.Clear();
					this.setMajorDownWaveStart( CurrentBar );
					this.MajorDownUnconfirmedLow =  CurrentBar ;
				}
				if (Low[0] < this.getMajorDownLastWaveLow() || this.MajorDownUnconfirmedLow > -1)
				{
					base.RemoveDrawObject("SSMinorWaveDownNumber");
					bool flag6 = false;
					if (this.MajorDownUnconfirmedLow ==  CurrentBar )
					{
						flag6 = true;
					}
					else
					{
						if (this.MajorDownUnconfirmedLow == -1)
						{
							flag6 = true;
						}
						else
						{
							if ( Low[ CurrentBar  - this.MajorDownUnconfirmedLow ] > Low[0])
							{
								flag6 = true;
							}
							if (!this.MajorDownBarOffSet)
							{
							}
						}
					}
					if (flag6)
					{
						this.MajorDownUnconfirmedLow =  CurrentBar ;
						if (!flag5)
						{
							this.processMajorDownPushedWaves(0);
						}
						if (this.MajorDownNewWaveNumber == 1)
						{
							if (this.getBarDirection( CurrentBar ) == 1 && this.isNewSessionHigh)
							{
								base.RemoveDrawObject("SSWaveDownNumber");
							}
							else
							{
								this.renderNumber("SSWaveDownNumber", this.MajorDownNewWaveNumber.ToString(), 0, Low[0], 1, "MajorWaveDownNew", false);
							}
						}
						else
						{
							this.renderNumber("SSWaveDownNumber", this.MajorDownNewWaveNumber.ToString(), 0, Low[0], 1, "MajorWaveDownNew", false);
						}
						if (this.MajorDownNewWaveNumber > 1 && this.MajorDownPushedWaves > 0 && !this.isNewSessionHigh && this.arrMajorDown.Count > 5 &&  CurrentBar  != this.lastMajorDownNewWaveProcess)
						{
							this.lastMajorDownNewWaveProcess =  CurrentBar ;
							this.lastMajorDownUnpushedNumber = this.MajorDownNewWaveNumber - 2;
							this.lastMajorDownUnpushedWaveEnd = Convert.ToInt32(this.arrMajorDown[this.arrMajorDown.Count - 4]);
							this.lastMajorDownUnpushedWaveEndID =  CurrentBar  - this.lastMajorDownUnpushedWaveEnd;
							this.renderNumber("SSWaveDownNumber" + this.sessionID + this.lastMajorDownUnpushedNumber, this.lastMajorDownUnpushedNumber.ToString(), this.lastMajorDownUnpushedWaveEndID, Low[ this.lastMajorDownUnpushedWaveEndID ], 2, "MajorWaveDown", false);
							if (this.renderNumbersHorizontal)
							{
								int num17 = Convert.ToInt32(this.arrMajorDown[this.arrMajorDown.Count - 5]);
								int arg_1ED4_0 =  CurrentBar ;
								int num18 = Convert.ToInt32(this.arrMajorDown[this.arrMajorDown.Count - 4]);
								int arg_1EFA_0 =  CurrentBar ;
								int num19 = Convert.ToInt32(this.arrMajorDown[this.arrMajorDown.Count - 3]);
								this.renderNumber(string.Concat(new object[]
								{
									"SSWaveDownNumberObsolete",
									this.sessionID,
									num19,
									num17,
									num18
								}), num19.ToString(), this.lastMajorDownUnpushedWaveEndID, Low[ this.lastMajorDownUnpushedWaveEndID ], 1, "MajorWaveDownPushed", true);
							}
							int num20 = this.MajorDownPushedWaves * 5 + 5;
							int num21 = this.MajorDownPushedWaves * 5 + 2;
							int num22 = this.MajorDownPushedWaves * 5 + 4;
							this.lastMajorDownUnpushedWaveStart = Convert.ToInt32(this.arrMajorDown[this.arrMajorDown.Count - num20]);
							this.lastMajorDownUnpushedWaveLevel = Convert.ToInt32(this.arrMajorDown[this.arrMajorDown.Count - num21]);
							this.lastMajorDownUnpushedWaveEnd = Convert.ToInt32(this.arrMajorDown[this.arrMajorDown.Count - num22]);
							this.lastMajorDownUnpushedWaveEndID =  CurrentBar  - this.lastMajorDownUnpushedWaveEnd;
							this.renderNumber(string.Concat(new object[]
							{
								"SSWaveDownNumberObsolete",
								this.sessionID,
								this.lastMajorDownUnpushedNumber,
								this.lastMajorDownUnpushedWaveStart,
								this.lastMajorDownUnpushedWaveEnd
							}), this.lastMajorDownUnpushedNumber.ToString(), this.lastMajorDownUnpushedWaveEndID, Low[ this.lastMajorDownUnpushedWaveEndID ], this.lastMajorDownUnpushedWaveLevel, "MajorWaveDownPushed", false);
						}
					}
				}
				if (Low[0] <= this.getMajorDownLastWaveLow())
				{
					base.RemoveDrawObject("SSMinorWaveDownNumber");
				}
				if (Low[0] > this.getMajorDownLastWaveLow())
				{
					bool flag7 = false;
					if ( High[ 0 ] >= this.getMinorDownLastWaveHigh())
					{
						if (Low[0] >= this.getMinorDownLastWaveLow())
						{
							this.processMinorDownPushedWaves(0);
						}
						flag7 = true;
					}
					if (this.isNewSessionHigh)
					{
						this.arrMinorDown.Clear();
						this.setMinorDownWaveStart( CurrentBar );
						this.MinorDownUnconfirmedLow =  CurrentBar ;
					}
					if (Low[0] < this.getMinorDownLastWaveLow() || this.MinorDownUnconfirmedLow > -1)
					{
						bool flag8 = false;
						if (this.MinorDownUnconfirmedLow ==  CurrentBar )
						{
							flag8 = true;
						}
						else
						{
							if (this.MinorDownUnconfirmedLow == -1)
							{
								flag8 = true;
							}
							else
							{
								if ( Low[ CurrentBar  - this.MinorDownUnconfirmedLow ] > Low[0] )
								{
									flag8 = true;
								}
								if (!this.MinorDownBarOffSet)
								{
								}
							}
						}
						if (flag8)
						{
							this.MinorDownUnconfirmedLow =  CurrentBar ;
							if (!flag7)
							{
								this.processMinorDownPushedWaves(0);
							}
							if (this.MinorDownNewWaveNumber == 1)
							{
								if (this.getBarDirection( CurrentBar ) == 1 && this.isNewSessionHigh)
								{
									base.RemoveDrawObject("SSMinorWaveDownNumber");
								}
								else
								{
									this.renderNumber("SSMinorWaveDownNumber", this.MinorDownNewWaveNumber.ToString(), 0, Low[0], 1, "MinorWaveDownNew", false);
								}
							}
							else
							{
								this.renderNumber("SSMinorWaveDownNumber", this.MinorDownNewWaveNumber.ToString(), 0, Low[0], 1, "MinorWaveDownNew", false);
							}
							if (this.MinorDownNewWaveNumber > 1 && this.MinorDownPushedWaves > 0 && !this.isNewSessionHigh && this.arrMinorDown.Count > 5 &&  CurrentBar  != this.lastMinorDownNewWaveProcess)
							{
								this.lastMinorDownNewWaveProcess =  CurrentBar ;
								this.lastMinorDownUnpushedNumber = this.MinorDownNewWaveNumber - 2;
								this.lastMinorDownUnpushedWaveEnd = Convert.ToInt32(this.arrMinorDown[this.arrMinorDown.Count - 4]);
								this.lastMinorDownUnpushedWaveEndID =  CurrentBar  - this.lastMinorDownUnpushedWaveEnd;
								this.renderNumber("SSMinorWaveDownNumber" + this.sessionID + this.lastMinorDownUnpushedNumber, this.lastMinorDownUnpushedNumber.ToString(), this.lastMinorDownUnpushedWaveEndID, Low[ this.lastMinorDownUnpushedWaveEndID ], 2, "MinorWaveDown", false);
								if (this.renderNumbersHorizontal)
								{
									int num23 = Convert.ToInt32(this.arrMinorDown[this.arrMinorDown.Count - 5]);
									int arg_2378_0 =  CurrentBar ;
									int num24 = Convert.ToInt32(this.arrMinorDown[this.arrMinorDown.Count - 4]);
									int arg_239E_0 =  CurrentBar ;
									int num25 = Convert.ToInt32(this.arrMinorDown[this.arrMinorDown.Count - 3]);
									this.renderNumber(string.Concat(new object[]
									{
										"SSMinorWaveDownNumberObsolete",
										this.sessionID,
										num25,
										num23,
										num24
									}), num25.ToString(), this.lastMinorDownUnpushedWaveEndID, Low[ this.lastMinorDownUnpushedWaveEndID ], 1, "MinorWaveDownPushed", true);
								}
								int num26 = this.MinorDownPushedWaves * 5 + 5;
								int num27 = this.MinorDownPushedWaves * 5 + 2;
								int num28 = this.MinorDownPushedWaves * 5 + 4;
								this.lastMinorDownUnpushedWaveStart = Convert.ToInt32(this.arrMinorDown[this.arrMinorDown.Count - num26]);
								this.lastMinorDownUnpushedWaveLevel = Convert.ToInt32(this.arrMinorDown[this.arrMinorDown.Count - num27]);
								this.lastMinorDownUnpushedWaveEnd = Convert.ToInt32(this.arrMinorDown[this.arrMinorDown.Count - num28]);
								this.lastMinorDownUnpushedWaveEndID =  CurrentBar  - this.lastMinorDownUnpushedWaveEnd;
								this.renderNumber(string.Concat(new object[]
								{
									"SSMinorWaveDownNumberObsolete",
									this.sessionID,
									this.lastMinorDownUnpushedNumber,
									this.lastMinorDownUnpushedWaveStart,
									this.lastMinorDownUnpushedWaveEnd
								}), this.lastMinorDownUnpushedNumber.ToString(), this.lastMinorDownUnpushedWaveEndID, Low[ this.lastMinorDownUnpushedWaveEndID ], this.lastMinorDownUnpushedWaveLevel, "MinorWaveDownPushed", false);
							}
						}
					}
				}
				if ((this.debugCommon && IsFirstTickOfBar) || this.debugCommonEachTick)
				{
					base.Print("-------------> CLOSE START");
					base.Print("arrMajorUp.Count=" + this.arrMajorUp.Count);
					base.Print("intProcessingBar=" + this.intProcessingBar);
					base.Print("intBarDirection=" + this.intBarDirection);
					base.Print("intPriorBar=" + this.intPriorBar);
					base.Print("intLastBarDirection=" + this.intLastBarDirection);
					base.Print("MajorUpStart=" + this.MajorUpStart);
					base.Print("MajorUpEnd=" + this.MajorUpEnd);
					base.Print("arrMajorUp.Count=" + this.arrMajorUp.Count);
					base.Print("MajorUpNewWaveHigh: " + this.MajorUpNewWaveHigh);
					base.Print("MajorUpConfirmed: " + this.MajorUpConfirmed);
					base.Print("MajorUpBarOffSet: " + this.MajorUpBarOffSet);
					base.Print("-------------> CLOSE END");
					base.Print("-------END----------");
				}
				if ((this.debugCommonDown && IsFirstTickOfBar) || this.debugCommonDownEachTick)
				{
					base.Print("-------------> CLOSE START");
					base.Print("arrMajorDown.Count=" + this.arrMajorDown.Count);
					base.Print("intProcessingBar=" + this.intProcessingBar);
					base.Print("intBarDirection=" + this.intBarDirection);
					base.Print("intPriorBar=" + this.intPriorBar);
					base.Print("intLastBarDirection=" + this.intLastBarDirection);
					base.Print("MajorDownStart=" + this.MajorDownStart);
					base.Print("MajorDownEnd=" + this.MajorDownEnd);
					base.Print("arrMajorDown.Count=" + this.arrMajorDown.Count);
					base.Print("MajorDownNewWaveLow: " + this.MajorDownNewWaveLow);
					base.Print("MajorDownConfirmed: " + this.MajorDownConfirmed);
					base.Print("MajorDownBarOffSet: " + this.MajorDownBarOffSet);
					base.Print("-------------> CLOSE END");
					base.Print("-------END----------");
				}
			}
			catch (Exception e)
			{
				this.printExceptionInformation(e);
			}
		}

		#region Misc_Functions
		
		private double getMajorUpLastWaveHigh()
		{
			if (this.arrMajorUp.Count == 0)
			{
				return 0.0;
			}
			int index = this.arrMajorUp.Count - 4;
			int num = Convert.ToInt32(this.arrMajorUp[index]);
			return High[ CurrentBar - num ];
		}
		
		private int getMajorUpNewWaveNumber()
		{
			return Convert.ToInt32(this.arrMajorUp.Count / 5 * 2 + 1);
		}
		
		private double getMajorUpLastWaveLow()
		{
			if (this.arrMajorUp.Count == 0)
			{
				return 0.0;
			}
			int index = this.arrMajorUp.Count - 5;
			int num = Convert.ToInt32(this.arrMajorUp[index]);
			return Low[ CurrentBar - num ];
		}
		
		private void setMajorUpWaveEnd()
		{
			if ( High[ this.intProcessingBarID ] > High[ this.intPriorBarID ])
			{
				this.MajorUpEnd = this.intProcessingBar;
				return;
			}
			this.MajorUpEnd = this.intPriorBar;
			int i = this.intPriorBar - 1;
			while (i > -1)
			{
				if ( High[ CurrentBar - i ] == High[ CurrentBar - this.intPriorBar ] && i >= this.MajorUpStart )
				{
					this.MajorUpEnd = i;
					if (i > this.MajorUpStart)
					{
						if (this.getBarDirection(i) != 1 || i != this.MajorUpStart)
						{
							i--;
							continue;
						}
					}
					else
					{
						this.MajorUpEnd = this.MajorUpStart;
					}
				}
IL_B7:	
				if ( MajorUpEnd == MajorUpStart && MajorUpBarOffSet)
				{
					MajorUpEnd++;
				}
				return;
				goto IL_B7; // I moved this up 2 lines
			}
//goto IL_B7;
		}
		

		private void setMajorUpWaveStart(int ProcessingBar)
		{
			int num = CurrentBar - ProcessingBar;
			int num2 = num + 1;
			if (this.MajorUpStart == this.MajorUpEnd)
			{
				this.MajorUpStart = ProcessingBar;
			}
			else
			{
				if ( High[ num ] >= High[ num2 ] )
				{
					this.MajorUpStart = ProcessingBar;
				}
				else
				{
					if ( Low[ num ] <=  Low[ num2 ] )
					{
						this.MajorUpStart = ProcessingBar;
					}
					else
					{
						if (this.getBarDirection( CurrentBar - num2) > -1)
						{
							this.MajorUpStart = ProcessingBar;
						}
						else
						{
							this.MajorUpStart = ProcessingBar - 1;
						}
					}
				}
			}
			if (this.getBarDirection(ProcessingBar) < 1)
			{
				if (this.arrMajorUp.Count < 4)
				{
					this.MajorUpBarOffSet = true;
				}
				this.MajorUpStart = ProcessingBar;
			}
			this.MajorUpEnd = -1;
			this.MajorUpWave = -1;
			this.MajorUpConfirmed = false;
			this.MajorUpNewWaveHigh = false;
			this.MajorUpUnconfirmedHigh = -1;
		}
		private void resetMajorUp(int ProcessingBar)
		{
			this.arrMajorUp.Clear();
			if (ProcessingBar > 1)
			{
				this.setMajorUpWaveStart(ProcessingBar);
			}
			this.MajorUpResetBar = CurrentBar;
			this.MajorUpUnconfirmedHigh = ProcessingBar;
			this.MajorUpPushedWaves = 0;
			this.MajorUpNewWaveNumber = 1;
			this.MajorUpPushedBarProcessed = -1;
			this.MajorUpPushedBarProcessedPrice = -1.0;
			this.lastMajorUpNewWaveProcess = -1;
			if (this.getBarDirection(ProcessingBar) == 1)
			{
				this.MajorUpBarOffSet = false;
			}
		}
		private void processMajorUpPushedWaves(int intProcessingBarID)
		{
			int num = 0;
			bool flag = false;
			if (this.MajorUpPushedBarProcessed == CurrentBar)
			{
				if ( Low[ intProcessingBarID ] < this.MajorUpPushedBarProcessedPrice )
				{
					this.MajorUpPushedBarProcessed = CurrentBar;
					this.MajorUpPushedBarProcessedPrice =  Low[ intProcessingBarID ];
					flag = true;
				}
			}
			else
			{
				this.MajorUpPushedBarProcessed = CurrentBar;
				this.MajorUpPushedBarProcessedPrice =  Low[ intProcessingBarID ];
				flag = true;
			}
			if (flag && this.arrMajorUp.Count > 4)
			{
				if ( Low[ intProcessingBarID ] <= this.getMajorUpLastWaveLow() )
				{
					for (int i = this.arrMajorUp.Count - 1; i > -1; i -= 5)
					{
						int num2 = Convert.ToInt32(this.arrMajorUp[i - 4]);
						int num3 = Convert.ToInt32(this.arrMajorUp[i - 3]);
						int num4 = Convert.ToInt32(this.arrMajorUp[i - 2]);
						int waveLevel = Convert.ToInt32(this.arrMajorUp[i - 1]);
						Convert.ToInt32(this.arrMajorUp[i]);
						int arg_119_0 = CurrentBar;
						int num5 = CurrentBar - num3;
						if ( Low[ intProcessingBarID ] >  Low[ CurrentBar - num2 ] )
						{
							break;
						}
						base.RemoveDrawObject("SSWaveNumber" + this.sessionID + num4);
						base.RemoveDrawObject("SSWaveDownLineA-" + this.sessionID + num4);
						base.RemoveDrawObject("SSWaveDownLineB-" + this.sessionID + num4);
						if (this.arrMajorUp.Count > 9)
						{
							if (i == this.arrMajorUp.Count - 1 && High[ 0 ] > High[ num5 ] )
							{
								this.renderNumber(string.Concat(new object[]
								{
									"SSWaveNumberObsolete",
									this.sessionID,
									num4,
									num2,
									num3
								}), num4.ToString(), num5, High[ num5 ], waveLevel, "MajorWaveUpPushed", true);
							}
							else
							{
								this.renderNumber(string.Concat(new object[]
								{
									"SSWaveNumberObsolete",
									this.sessionID,
									num4,
									num2,
									num3
								}), num4.ToString(), num5, High[ num5 ], waveLevel, "MajorWaveUpPushed", false);
							}
						}
						num++;
					}
				}
				if (num > this.MajorUpPushedWaves)
				{
					this.MajorUpPushedWaves = num;
				}
				this.MajorUpNewWaveNumber = this.getMajorUpNewWaveNumber();
				if (this.MajorUpPushedWaves > 0)
				{
					this.MajorUpNewWaveNumber -= this.MajorUpPushedWaves * 2;
				}
			}
		}
		private bool isMajorUpWaveConfirmed(int intProcessingBar, int intPriorBar)
		{
			this.intBarDirection = this.getBarDirection(intProcessingBar);
			this.intLastBarDirection = this.getBarDirection(intPriorBar);
			bool result = false;
			if ( High[ CurrentBar - intProcessingBar ] < High[ CurrentBar - intPriorBar ] )
			{
				if (!this.MajorUpBarOffSet || this.MajorUpStart != intPriorBar || this.getBarDirection(intProcessingBar) == -1)
				{
					result = true;
				}
			}
			else
			{
				if (this.intBarDirection == -1 && this.intLastBarDirection == -1)
				{
					result = true;
				}
				if (this.intBarDirection == -1 && this.intLastBarDirection == 0 && this.getBarDirection(this.findLastUpOrDownBar(intPriorBar)) == 1)
				{
					result = true;
				}
				if (this.intBarDirection == -1 && this.intLastBarDirection == 0 && High[ CurrentBar - intProcessingBar ] >=  High[ CurrentBar - intPriorBar ] )
				{
					result = true;
				}
				if (this.intBarDirection == -1 && this.intLastBarDirection == 1)
				{
					result = true;
				}
			}
			return result;
		}
		private void processMajorUpWaves(int intStart, int intEnd, bool intBarOffSet, int sessionID, bool isConfirmed)
		{
			base.RemoveDrawObject("SSWaveNumber");
			ArrayList arrayList = new ArrayList();
			if (this.arrMajorUp.Count > 4)
			{
				int num = 0;
				if (this.debugTempArray)
				{
					base.Print("-------------> TEMP ARRAY START");
				}
				if (intStart == intEnd)
				{
					for (int i = 0; i < this.arrMajorUp.Count - 1; i += 5)
					{
						int num2 = Convert.ToInt32(this.arrMajorUp[i]);
						int num3 = Convert.ToInt32(this.arrMajorUp[i + 1]);
						int num4 = Convert.ToInt32(this.arrMajorUp[i + 2]);
						int num5 = Convert.ToInt32(this.arrMajorUp[i + 3]);
						int num6 = Convert.ToInt32(this.arrMajorUp[i + 4]);
						int arg_B1_0 = CurrentBar;
						int num7 = CurrentBar - num3;
						if ( Low[ CurrentBar - intStart ] <=  Low[ CurrentBar - num2 ] )
						{
							int majorUpNewWaveNumber = this.getMajorUpNewWaveNumber();
							this.arrMajorUp.Add(intStart);
							this.arrMajorUp.Add(intEnd);
							this.arrMajorUp.Add(majorUpNewWaveNumber);
							this.arrMajorUp.Add(1);
							this.arrMajorUp.Add(intBarOffSet);
							this.renderNumber("SSWaveNumber" + sessionID + majorUpNewWaveNumber, majorUpNewWaveNumber.ToString(), CurrentBar - intEnd, High[ CurrentBar - intEnd ], 1, "MajorWaveUpPushed", false);
							this.MajorUpBarOffSet = false;
							return;
						}
					}
				}
				for (int j = 0; j < this.arrMajorUp.Count - 1; j += 5)
				{
					int num2 = Convert.ToInt32(this.arrMajorUp[j]);
					int num3 = Convert.ToInt32(this.arrMajorUp[j + 1]);
					int num4 = Convert.ToInt32(this.arrMajorUp[j + 2]);
					int num5 = Convert.ToInt32(this.arrMajorUp[j + 3]);
					int num6 = Convert.ToInt32(this.arrMajorUp[j + 4]);
					int arg_22C_0 = CurrentBar;
					int num7 = CurrentBar - num3;
					if (this.debugTempArray)
					{
						base.Print("________________________________");
						base.Print("i=" + j);
						base.Print("waveStart:" + num2);
						base.Print("waveEnd:" + num3);
						base.Print("waveNumber:" + num4);
						base.Print("waveLevel:" + num5);
						base.Print("waveBarOffSet:" + num6);
						base.Print("________________________________");
					}
					if ( Low[ CurrentBar - intStart ] <=  Low[ CurrentBar - num2 ] )
					{
						num++;
						int num8 = Convert.ToInt32(arrayList[arrayList.Count - 5]);
						int num9 = Convert.ToInt32(arrayList[arrayList.Count - 4]);
						int num10 = Convert.ToInt32(arrayList[arrayList.Count - 3]);
						int num11 = Convert.ToInt32(arrayList[arrayList.Count - 2]);
						int num12 = Convert.ToInt32(arrayList[arrayList.Count - 1]);
						int arg_380_0 = CurrentBar;
						int num13 = CurrentBar - num9;
						arrayList.RemoveAt(arrayList.Count - 1);
						arrayList.RemoveAt(arrayList.Count - 1);
						arrayList.RemoveAt(arrayList.Count - 1);
						arrayList.RemoveAt(arrayList.Count - 1);
						arrayList.RemoveAt(arrayList.Count - 1);
						arrayList.Add(num8);
						arrayList.Add(num3);
						arrayList.Add(num10);
						if (num11 == 2)
						{
							arrayList.Add(2);
						}
						else
						{
							arrayList.Add(num11 + 1);
						}
						arrayList.Add(num12);
						if (num < 2)
						{
							this.renderNumber(string.Concat(new object[]
							{
								"SSWaveNumberObsolete",
								sessionID,
								num10,
								num8,
								num9
							}), num10.ToString(), num13, High[ num13 ], num11, "MajorWaveUpPushed", false);
							if (this.renderNumbersHorizontal)
							{
							}
						}
					}
					else
					{
						arrayList.Add(num2);
						arrayList.Add(num3);
						arrayList.Add(num4);
						arrayList.Add(num5);
						arrayList.Add(num6);
					}
				}
				if (this.debugTempArray)
				{
					base.Print("-------------> TEMP ARRAY END");
				}
				this.arrMajorUp.Clear();
				for (int k = 0; k < arrayList.Count - 1; k += 5)
				{
					int num2 = Convert.ToInt32(arrayList[k]);
					int num3 = Convert.ToInt32(arrayList[k + 1]);
					int num4 = Convert.ToInt32(arrayList[k + 2]);
					int num5 = Convert.ToInt32(arrayList[k + 3]);
					int num6 = Convert.ToInt32(arrayList[k + 4]);
					int arg_584_0 = CurrentBar;
					int num7 = CurrentBar - num3;
					this.arrMajorUp.Add(num2);
					this.arrMajorUp.Add(num3);
					this.arrMajorUp.Add(num4);
					this.arrMajorUp.Add(num5);
					this.arrMajorUp.Add(num6);
					if (num4 > 1)
					{
						int index = k - 4;
						Convert.ToInt32(arrayList[index]);
						int arg_60D_0 = CurrentBar;
					}
					this.renderNumber("SSWaveNumber" + sessionID + num4, num4.ToString(), num7, High[ num7 ], num5, "MajorWaveUp", false);
				}
			}
			if (isConfirmed)
			{
				int majorUpNewWaveNumber = this.getMajorUpNewWaveNumber();
				this.arrMajorUp.Add(intStart);
				this.arrMajorUp.Add(intEnd);
				this.arrMajorUp.Add(majorUpNewWaveNumber);
				this.arrMajorUp.Add(1);
				this.arrMajorUp.Add(intBarOffSet);
				this.renderNumber("SSWaveNumber" + sessionID + majorUpNewWaveNumber, majorUpNewWaveNumber.ToString(), CurrentBar - intEnd, High[ CurrentBar - intEnd ], 1, "MajorWaveUp", false);
				this.MajorUpBarOffSet = false;
			}
		}
		private double getMinorUpLastWaveHigh()
		{
			if (this.arrMinorUp.Count == 0)
			{
				return 0.0;
			}
			int index = this.arrMinorUp.Count - 4;
			int num = Convert.ToInt32(this.arrMinorUp[index]);
			return High[ CurrentBar - num ];
		}
		private double getMinorUpLastWaveLow()
		{
			if (this.arrMinorUp.Count == 0)
			{
				return 0.0;
			}
			int index = this.arrMinorUp.Count - 5;
			int num = Convert.ToInt32(this.arrMinorUp[index]);
			return  Low[ CurrentBar - num ];
		}
		private void setMinorUpWaveEnd()
		{
			if ( High[ this.intProcessingBarID ] >  High[ this.intPriorBarID ])
			{
				this.MinorUpEnd = this.intProcessingBar;
				return;
			}
			this.MinorUpEnd = this.intPriorBar;
			int i = this.intPriorBar - 1;
			while (i > -1)
			{
				if ( High[ CurrentBar - i ] ==  High[ CurrentBar - this.intPriorBar ] && i >= this.MinorUpStart)
				{
					this.MinorUpEnd = i;
					if (i > this.MinorUpStart)
					{
						if (this.getBarDirection(i) != 1 || i != this.MinorUpStart)
						{
							i--;
							continue;
						}
					}
					else
					{
						this.MinorUpEnd = this.MinorUpStart;
					}
				}
IL_B7:
				if (this.MinorUpEnd == this.MinorUpStart && this.MinorUpBarOffSet)
				{
					this.MinorUpEnd++;
				}
				return;
				goto IL_B7;
			}
//goto IL_B7;
		}
		private void setMinorUpWaveStart(int ProcessingBar)
		{
			int num = CurrentBar - ProcessingBar;
			int num2 = num + 1;
			if (this.MinorUpStart == this.MinorUpEnd)
			{
				this.MinorUpStart = ProcessingBar;
			}
			else
			{
				if ( High[ num ] >=  High[ num2 ] )
				{
					this.MinorUpStart = ProcessingBar;
				}
				else
				{
					if ( Low[ num ] <=  Low[ num2 ] )
					{
						this.MinorUpStart = ProcessingBar;
					}
					else
					{
						if (this.getBarDirection(CurrentBar - num2) > -1)
						{
							this.MinorUpStart = ProcessingBar;
						}
						else
						{
							this.MinorUpStart = ProcessingBar - 1;
						}
					}
				}
			}
			if (this.getBarDirection(ProcessingBar) < 1)
			{
				if (this.arrMinorUp.Count < 4)
				{
					this.MinorUpBarOffSet = true;
				}
				this.MinorUpStart = ProcessingBar;
			}
			this.MinorUpEnd = -1;
			this.MinorUpWave = -1;
			this.MinorUpConfirmed = false;
			this.MinorUpNewWaveHigh = false;
			this.MinorUpUnconfirmedHigh = -1;
		}
		private void resetMinorUp(int ProcessingBar)
		{
			this.arrMinorUp.Clear();
			if (ProcessingBar > 1)
			{
				this.setMinorUpWaveStart(ProcessingBar);
			}
			this.MinorUpResetBar = CurrentBar;
			this.MinorUpUnconfirmedHigh = ProcessingBar;
			this.MinorUpPushedWaves = 0;
			this.MinorUpNewWaveNumber = 1;
			this.MinorUpPushedBarProcessed = -1;
			this.MinorUpPushedBarProcessedPrice = -1.0;
			this.lastMinorUpNewWaveProcess = -1;
			if (this.getBarDirection(ProcessingBar) == 1)
			{
				this.MinorUpBarOffSet = false;
			}
		}
		private void processMinorUpPushedWaves(int intProcessingBarID)
		{
			int num = 0;
			bool flag = false;
			if (this.MinorUpPushedBarProcessed == CurrentBar)
			{
				if ( Low[ intProcessingBarID ] < this.MinorUpPushedBarProcessedPrice)
				{
					this.MinorUpPushedBarProcessed = CurrentBar;
					this.MinorUpPushedBarProcessedPrice =  Low[ intProcessingBarID ];
					flag = true;
				}
			}
			else
			{
				this.MinorUpPushedBarProcessed = CurrentBar;
				this.MinorUpPushedBarProcessedPrice =  Low[ intProcessingBarID ];
				flag = true;
			}
			if (flag && this.arrMinorUp.Count > 4)
			{
				if ( Low[ intProcessingBarID ] <= this.getMinorUpLastWaveLow() || this.MajorUpNewWaveHigh || this.isMinorUpReset)
				{
					for (int i = this.arrMinorUp.Count - 1; i > -1; i -= 5)
					{
						int num2 = Convert.ToInt32(this.arrMinorUp[i - 4]);
						int num3 = Convert.ToInt32(this.arrMinorUp[i - 3]);
						int num4 = Convert.ToInt32(this.arrMinorUp[i - 2]);
						int waveLevel = Convert.ToInt32(this.arrMinorUp[i - 1]);
						Convert.ToInt32(this.arrMinorUp[i]);
						int arg_129_0 = CurrentBar;
						int num5 = CurrentBar - num3;
						if ( Low[ intProcessingBarID ] >  Low[ CurrentBar - num2 ] && !this.MajorUpNewWaveHigh && !this.isMinorUpReset)
						{
							break;
						}
						base.RemoveDrawObject("SSMinorWaveNumber" + this.sessionID + num4);
						base.RemoveDrawObject("SSWaveDownLineA-" + this.sessionID + num4);
						base.RemoveDrawObject("SSWaveDownLineB-" + this.sessionID + num4);
						if (this.arrMinorUp.Count > 9)
						{
							if ( i == this.arrMinorUp.Count - 1 && High[ 0 ] >  High[ num5 ] )
							{
								this.renderNumber(string.Concat(new object[]
								{
									"SSMinorWaveNumberObsolete",
									this.sessionID,
									num4,
									num2,
									num3
								}), num4.ToString(), num5,  High[ num5 ], waveLevel, "MinorWaveUpPushed", true);
							}
							else
							{
								this.renderNumber(string.Concat(new object[]
								{
									"SSMinorWaveNumberObsolete",
									this.sessionID,
									num4,
									num2,
									num3
								}), num4.ToString(), num5,  High[ num5 ], waveLevel, "MinorWaveUpPushed", false);
							}
						}
						num++;
					}
				}
				if (num > this.MinorUpPushedWaves)
				{
					this.MinorUpPushedWaves = num;
				}
				this.MinorUpNewWaveNumber = this.getNewWaveNumber(this.arrMinorUp);
				if (this.MinorUpPushedWaves > 0)
				{
					this.MinorUpNewWaveNumber -= this.MinorUpPushedWaves * 2;
				}
				this.isMinorUpReset = false;
			}
		}
		private bool isMinorUpWaveConfirmed(int intProcessingBar, int intPriorBar)
		{
			this.intBarDirection = this.getBarDirection(intProcessingBar);
			this.intLastBarDirection = this.getBarDirection(intPriorBar);
			bool result = false;
			if ( High[ CurrentBar - intProcessingBar ] <  High[ CurrentBar - intPriorBar ])
			{
				if (!this.MinorUpBarOffSet || this.MinorUpStart != intPriorBar || this.getBarDirection(intProcessingBar) == -1)
				{
					result = true;
				}
			}
			else
			{
				if (this.intBarDirection == -1 && this.intLastBarDirection == -1)
				{
					result = true;
				}
				if (this.intBarDirection == -1 && this.intLastBarDirection == 0 && this.getBarDirection(this.findLastUpOrDownBar(intPriorBar)) == 1)
				{
					result = true;
				}
				if (this.intBarDirection == -1 && this.intLastBarDirection == 0 &&  High[ CurrentBar - intProcessingBar ] >=  High[ CurrentBar - intPriorBar ] )
				{
					result = true;
				}
				if (this.intBarDirection == -1 && this.intLastBarDirection == 1)
				{
					result = true;
				}
			}
			return result;
		}
		private void processMinorUpWaves(int intStart, int intEnd, bool intBarOffSet, int sessionID, bool isConfirmed)
		{
			base.RemoveDrawObject("SSMinorWaveNumber");
			ArrayList arrayList = new ArrayList();
			if (this.arrMinorUp.Count > 4)
			{
				int num = 0;
				if (intStart == intEnd)
				{
					for (int i = 0; i < this.arrMinorUp.Count - 1; i += 5)
					{
						int num2 = Convert.ToInt32(this.arrMinorUp[i]);
						int num3 = Convert.ToInt32(this.arrMinorUp[i + 1]);
						int num4 = Convert.ToInt32(this.arrMinorUp[i + 2]);
						int num5 = Convert.ToInt32(this.arrMinorUp[i + 3]);
						int num6 = Convert.ToInt32(this.arrMinorUp[i + 4]);
						int arg_9E_0 = CurrentBar;
						int num7 = CurrentBar - num3;
						if ( Low[ CurrentBar - intStart ] <=  Low[ CurrentBar - num2 ] )
						{
							int newWaveNumber = this.getNewWaveNumber(this.arrMinorUp);
							this.arrMinorUp.Add(intStart);
							this.arrMinorUp.Add(intEnd);
							this.arrMinorUp.Add(newWaveNumber);
							this.arrMinorUp.Add(1);
							this.arrMinorUp.Add(intBarOffSet);
							this.renderNumber("SSMinorWaveNumber" + sessionID + newWaveNumber, newWaveNumber.ToString(), CurrentBar - intEnd,  High[ CurrentBar - intEnd ], 1, "MinorWaveUpPushed", false);
							this.MinorUpBarOffSet = false;
							return;
						}
					}
				}
				if (this.debugTempArray)
				{
					base.Print("-------------> TEMP ARRAY START");
				}
				for (int j = 0; j < this.arrMinorUp.Count - 1; j += 5)
				{
					int num2 = Convert.ToInt32(this.arrMinorUp[j]);
					int num3 = Convert.ToInt32(this.arrMinorUp[j + 1]);
					int num4 = Convert.ToInt32(this.arrMinorUp[j + 2]);
					int num5 = Convert.ToInt32(this.arrMinorUp[j + 3]);
					int num6 = Convert.ToInt32(this.arrMinorUp[j + 4]);
					int arg_232_0 = CurrentBar;
					int num7 = CurrentBar - num3;
					if (this.debugTempArray)
					{
						base.Print("________________________________");
						base.Print("i=" + j);
						base.Print("waveStart:" + num2);
						base.Print("waveEnd:" + num3);
						base.Print("waveNumber:" + num4);
						base.Print("waveLevel:" + num5);
						base.Print("waveBarOffSet:" + num6);
						base.Print("________________________________");
					}
					if ( Low[ CurrentBar - intStart ] <=  Low[ CurrentBar - num2 ])
					{
						num++;
						int num8 = Convert.ToInt32(arrayList[arrayList.Count - 5]);
						int num9 = Convert.ToInt32(arrayList[arrayList.Count - 4]);
						int num10 = Convert.ToInt32(arrayList[arrayList.Count - 3]);
						int num11 = Convert.ToInt32(arrayList[arrayList.Count - 2]);
						int num12 = Convert.ToInt32(arrayList[arrayList.Count - 1]);
						int arg_386_0 = CurrentBar;
						int num13 = CurrentBar - num9;
						arrayList.RemoveAt(arrayList.Count - 1);
						arrayList.RemoveAt(arrayList.Count - 1);
						arrayList.RemoveAt(arrayList.Count - 1);
						arrayList.RemoveAt(arrayList.Count - 1);
						arrayList.RemoveAt(arrayList.Count - 1);
						arrayList.Add(num8);
						arrayList.Add(num3);
						arrayList.Add(num10);
						if (num11 == 2)
						{
							arrayList.Add(2);
						}
						else
						{
							arrayList.Add(num11 + 1);
						}
						arrayList.Add(num12);
						if (num < 2)
						{
							this.renderNumber(string.Concat(new object[]
							{
								"SSMinorWaveNumberObsolete",
								sessionID,
								CurrentBar,
								num8,
								num9
							}), num10.ToString(), num13,  High[ num13 ], num11, "MinorWaveUpPushed", false);
							if (this.renderNumbersHorizontal)
							{
							}
						}
					}
					else
					{
						arrayList.Add(num2);
						arrayList.Add(num3);
						arrayList.Add(num4);
						arrayList.Add(num5);
						arrayList.Add(num6);
					}
				}
				if (this.debugTempArray)
				{
					base.Print("-------------> TEMP ARRAY END");
				}
				this.arrMinorUp.Clear();
				for (int k = 0; k < arrayList.Count - 1; k += 5)
				{
					int num2 = Convert.ToInt32(arrayList[k]);
					int num3 = Convert.ToInt32(arrayList[k + 1]);
					int num4 = Convert.ToInt32(arrayList[k + 2]);
					int num5 = Convert.ToInt32(arrayList[k + 3]);
					int num6 = Convert.ToInt32(arrayList[k + 4]);
					int arg_58E_0 = CurrentBar;
					int num7 = CurrentBar - num3;
					this.arrMinorUp.Add(num2);
					this.arrMinorUp.Add(num3);
					this.arrMinorUp.Add(num4);
					this.arrMinorUp.Add(num5);
					this.arrMinorUp.Add(num6);
					if (num4 > 1)
					{
						int index = k - 4;
						Convert.ToInt32(arrayList[index]);
						int arg_617_0 = CurrentBar;
					}
					this.renderNumber("SSMinorWaveNumber" + sessionID + num4, num4.ToString(), num7,  High[ num7 ], num5, "MinorWaveUp", false);
				}
			}
			if (isConfirmed)
			{
				int newWaveNumber2 = this.getNewWaveNumber(this.arrMinorUp);
				this.arrMinorUp.Add(intStart);
				this.arrMinorUp.Add(intEnd);
				this.arrMinorUp.Add(newWaveNumber2);
				this.arrMinorUp.Add(1);
				this.arrMinorUp.Add(intBarOffSet);
				this.renderNumber("SSMinorWaveNumber" + sessionID + newWaveNumber2, newWaveNumber2.ToString(), CurrentBar - intEnd,  High[ CurrentBar - intEnd ], 1, "MinorWaveUp", false);
				this.MinorUpBarOffSet = false;
			}
		}
		private double getMajorDownLastWaveHigh()
		{
			if (this.arrMajorDown.Count == 0)
			{
				return 0.0;
			}
			int index = this.arrMajorDown.Count - 5;
			int num = Convert.ToInt32(this.arrMajorDown[index]);
			return  High[ CurrentBar - num ];
		}
		private double getMajorDownLastWaveLow()
		{
			if (this.arrMajorDown.Count == 0)
			{
				return 0.0;
			}
			int index = this.arrMajorDown.Count - 4;
			int num = Convert.ToInt32(this.arrMajorDown[index]);
			return  Low[ CurrentBar - num ];
		}
		private void setMajorDownWaveEnd()
		{
			if ( Low[ this.intProcessingBarID ] <  Low[ this.intPriorBarID ] )
			{
				this.MajorDownEnd = this.intProcessingBar;
				return;
			}
			this.MajorDownEnd = this.intPriorBar;
			int i = this.intPriorBar - 1;
			while (i > -1)
			{
				if ( Low[ CurrentBar - i ] ==  Low[ CurrentBar - this.intPriorBar ] && i <= this.MajorDownStart)
				{
					this.MajorDownEnd = i;
					if (i > this.MajorDownStart)
					{
						if (this.getBarDirection(i) != -1 || i != this.MajorDownStart)
						{
							i--;
							continue;
						}
					}
					else
					{
						this.MajorDownEnd = this.MajorDownStart;
					}
				}
IL_B7:
				if (this.MajorDownEnd == this.MajorDownStart && this.MajorDownBarOffSet)
				{
					this.MajorDownEnd++;
				}
				return;
				goto IL_B7;
			}
//goto IL_B7;
		}
		private void setMajorDownWaveStart(int ProcessingBar)
		{
			int num = CurrentBar - ProcessingBar;
			int num2 = num + 1;
			if (this.MajorDownStart == this.MajorDownEnd)
			{
				this.MajorDownStart = ProcessingBar;
			}
			else
			{
				if ( Low[ num ] <=  Low[ num2 ] )
				{
					this.MajorDownStart = ProcessingBar;
				}
				else
				{
					if ( High[ num ] >=  High[ num2 ] )
					{
						this.MajorDownStart = ProcessingBar;
					}
					else
					{
						if (this.getBarDirection(CurrentBar - num2) < 1)
						{
							this.MajorDownStart = ProcessingBar;
						}
						else
						{
							this.MajorDownStart = ProcessingBar - 1;
						}
					}
				}
			}
			if (this.getBarDirection(ProcessingBar) > -1)
			{
				if (this.arrMajorDown.Count < 4)
				{
					this.MajorDownBarOffSet = true;
				}
				this.MajorDownStart = ProcessingBar;
			}
			this.MajorDownEnd = -1;
			this.MajorDownWave = -1;
			this.MajorDownConfirmed = false;
			this.MajorDownNewWaveLow = false;
			this.MajorDownUnconfirmedLow = -1;
		}
		private void resetMajorDown(int ProcessingBar)
		{
			this.arrMajorDown.Clear();
			if (ProcessingBar > 1)
			{
				this.setMajorDownWaveStart(ProcessingBar);
			}
			this.MajorDownResetBar = CurrentBar;
			this.MajorDownUnconfirmedLow = ProcessingBar;
			this.MajorDownPushedWaves = 0;
			this.MajorDownNewWaveNumber = 1;
			this.MajorDownPushedBarProcessed = -1;
			this.MajorDownPushedBarProcessedPrice = -1.0;
			this.lastMajorDownNewWaveProcess = -1;
			if (this.getBarDirection(ProcessingBar) == -1)
			{
				this.MajorDownBarOffSet = false;
			}
		}
		private void processMajorDownPushedWaves(int intProcessingBarID)
		{
			int num = 0;
			bool flag = false;
			if (this.MajorDownPushedBarProcessed == CurrentBar)
			{
				if ( High[ intProcessingBarID ] > this.MajorDownPushedBarProcessedPrice)
				{
					this.MajorDownPushedBarProcessed = CurrentBar;
					this.MajorDownPushedBarProcessedPrice =  High[ intProcessingBarID ];
					flag = true;
				}
			}
			else
			{
				this.MajorDownPushedBarProcessed = CurrentBar;
				this.MajorDownPushedBarProcessedPrice =  High[ intProcessingBarID ];
				flag = true;
			}
			if (flag && this.arrMajorDown.Count > 4)
			{
				if ( High[ intProcessingBarID ] >= this.getMajorDownLastWaveHigh())
				{
					for (int i = this.arrMajorDown.Count - 1; i > -1; i -= 5)
					{
						int num2 = Convert.ToInt32(this.arrMajorDown[i - 4]);
						int num3 = Convert.ToInt32(this.arrMajorDown[i - 3]);
						int num4 = Convert.ToInt32(this.arrMajorDown[i - 2]);
						int waveLevel = Convert.ToInt32(this.arrMajorDown[i - 1]);
						Convert.ToInt32(this.arrMajorDown[i]);
						int arg_119_0 = CurrentBar;
						int num5 = CurrentBar - num3;
						if ( High[ intProcessingBarID ] <  High[ CurrentBar - num2 ])
						{
							break;
						}
						base.RemoveDrawObject("SSWaveDownNumber" + this.sessionID + num4);
						base.RemoveDrawObject("SSWaveDownLineA-" + this.sessionID + num4);
						base.RemoveDrawObject("SSWaveDownLineB-" + this.sessionID + num4);
						if (this.arrMajorDown.Count > 9)
						{
							if (i == this.arrMajorDown.Count - 1 &&  Low[ 0 ] <  Low[ num5 ] )
							{
								this.renderNumber(string.Concat(new object[]
								{
									"SSWaveDownNumberObsolete",
									this.sessionID,
									num4,
									num2,
									num3
								}), num4.ToString(), num5,  Low[ num5 ], waveLevel, "MajorWaveDownPushed", true);
							}
							else
							{
								this.renderNumber(string.Concat(new object[]
								{
									"SSWaveDownNumberObsolete",
									this.sessionID,
									num4,
									num2,
									num3
								}), num4.ToString(), num5,  Low[ num5 ], waveLevel, "MajorWaveDownPushed", false);
							}
						}
						num++;
					}
				}
				if (num > this.MajorDownPushedWaves)
				{
					this.MajorDownPushedWaves = num;
				}
				this.MajorDownNewWaveNumber = this.getNewWaveNumber(this.arrMajorDown);
				if (this.MajorDownPushedWaves > 0)
				{
					this.MajorDownNewWaveNumber -= this.MajorDownPushedWaves * 2;
				}
			}
		}
		private bool isMajorDownWaveConfirmed(int intProcessingBar, int intPriorBar)
		{
			this.intBarDirection = this.getBarDirection(intProcessingBar);
			this.intLastBarDirection = this.getBarDirection(intPriorBar);
			bool result = false;
			if ( Low[ CurrentBar - intProcessingBar ] >  Low[ CurrentBar - intPriorBar ] )
			{
				if (!this.MajorDownBarOffSet || this.MajorDownStart != intPriorBar || this.getBarDirection(intProcessingBar) == 1)
				{
					result = true;
				}
			}
			else
			{
				if (this.intBarDirection == 1 && this.intLastBarDirection == 1)
				{
					result = true;
				}
				if (this.intBarDirection == 1 && this.intLastBarDirection == 0 && this.getBarDirection(this.findLastUpOrDownBar(intPriorBar)) == -1)
				{
					result = true;
				}
				if (this.intBarDirection == 1 && this.intLastBarDirection == 0 &&  Low[ CurrentBar - intProcessingBar ] <=  Low[ CurrentBar - intPriorBar ] )
				{
					result = true;
				}
				if (this.intBarDirection == 1 && this.intLastBarDirection == -1)
				{
					result = true;
				}
			}
			return result;
		}
		private void processMajorDownWaves(int intStart, int intEnd, bool intBarOffSet, int sessionID, bool isConfirmed)
		{
			base.RemoveDrawObject("SSWaveDownNumber");
			ArrayList arrayList = new ArrayList();
			if (this.arrMajorDown.Count > 4)
			{
				int num = 0;
				if (this.debugTempArrayDown)
				{
					base.Print("-------------> TEMP ARRAY START");
				}
				if (intStart == intEnd)
				{
					for (int i = 0; i < this.arrMajorDown.Count - 1; i += 5)
					{
						int num2 = Convert.ToInt32(this.arrMajorDown[i]);
						int num3 = Convert.ToInt32(this.arrMajorDown[i + 1]);
						int num4 = Convert.ToInt32(this.arrMajorDown[i + 2]);
						int num5 = Convert.ToInt32(this.arrMajorDown[i + 3]);
						int num6 = Convert.ToInt32(this.arrMajorDown[i + 4]);
						int arg_B1_0 = CurrentBar;
						int num7 = CurrentBar - num3;
						if ( High[ CurrentBar - intStart ] >=  High[ CurrentBar - num2 ] )
						{
							int newWaveNumber = this.getNewWaveNumber(this.arrMajorDown);
							this.arrMajorDown.Add(intStart);
							this.arrMajorDown.Add(intEnd);
							this.arrMajorDown.Add(newWaveNumber);
							this.arrMajorDown.Add(1);
							this.arrMajorDown.Add(intBarOffSet);
							this.renderNumber("SSWaveDownNumber" + sessionID + newWaveNumber, newWaveNumber.ToString(), CurrentBar - intEnd,  Low[ CurrentBar - intEnd ], 1, "MajorWaveDownPushed", false);
							this.MajorDownBarOffSet = false;
							return;
						}
					}
				}
				for (int j = 0; j < this.arrMajorDown.Count - 1; j += 5)
				{
					int num2 = Convert.ToInt32(this.arrMajorDown[j]);
					int num3 = Convert.ToInt32(this.arrMajorDown[j + 1]);
					int num4 = Convert.ToInt32(this.arrMajorDown[j + 2]);
					int num5 = Convert.ToInt32(this.arrMajorDown[j + 3]);
					int num6 = Convert.ToInt32(this.arrMajorDown[j + 4]);
					int arg_232_0 = CurrentBar;
					int num7 = CurrentBar - num3;
					if (this.debugTempArrayDown)
					{
						base.Print("________________________________");
						base.Print("i=" + j);
						base.Print("waveStart:" + num2);
						base.Print("waveEnd:" + num3);
						base.Print("waveNumber:" + num4);
						base.Print("waveLevel:" + num5);
						base.Print("waveBarOffSet:" + num6);
						base.Print("________________________________");
					}
					if ( High[ CurrentBar - intStart ] >=  High[ CurrentBar - num2 ] )
					{
						num++;
						int num8 = Convert.ToInt32(arrayList[arrayList.Count - 5]);
						int num9 = Convert.ToInt32(arrayList[arrayList.Count - 4]);
						int num10 = Convert.ToInt32(arrayList[arrayList.Count - 3]);
						int num11 = Convert.ToInt32(arrayList[arrayList.Count - 2]);
						int num12 = Convert.ToInt32(arrayList[arrayList.Count - 1]);
						int arg_386_0 = CurrentBar;
						int num13 = CurrentBar - num9;
						arrayList.RemoveAt(arrayList.Count - 1);
						arrayList.RemoveAt(arrayList.Count - 1);
						arrayList.RemoveAt(arrayList.Count - 1);
						arrayList.RemoveAt(arrayList.Count - 1);
						arrayList.RemoveAt(arrayList.Count - 1);
						arrayList.Add(num8);
						arrayList.Add(num3);
						arrayList.Add(num10);
						if (num11 == 2)
						{
							arrayList.Add(2);
						}
						else
						{
							arrayList.Add(num11 + 1);
						}
						arrayList.Add(num12);
						if (num < 2)
						{
							this.renderNumber(string.Concat(new object[]
							{
								"SSWaveDownNumberObsolete",
								sessionID,
								num10,
								num8,
								num9
							}), num10.ToString(), num13,  Low[ num13 ], num11, "MajorWaveDownPushed", false);
							if (this.renderNumbersHorizontal)
							{
							}
						}
					}
					else
					{
						arrayList.Add(num2);
						arrayList.Add(num3);
						arrayList.Add(num4);
						arrayList.Add(num5);
						arrayList.Add(num6);
					}
				}
				if (this.debugTempArrayDown)
				{
					base.Print("-------------> TEMP ARRAY END");
				}
				this.arrMajorDown.Clear();
				for (int k = 0; k < arrayList.Count - 1; k += 5)
				{
					int num2 = Convert.ToInt32(arrayList[k]);
					int num3 = Convert.ToInt32(arrayList[k + 1]);
					int num4 = Convert.ToInt32(arrayList[k + 2]);
					int num5 = Convert.ToInt32(arrayList[k + 3]);
					int num6 = Convert.ToInt32(arrayList[k + 4]);
					int arg_58A_0 = CurrentBar;
					int num7 = CurrentBar - num3;
					this.arrMajorDown.Add(num2);
					this.arrMajorDown.Add(num3);
					this.arrMajorDown.Add(num4);
					this.arrMajorDown.Add(num5);
					this.arrMajorDown.Add(num6);
					if (num4 > 1)
					{
						int index = k - 4;
						Convert.ToInt32(arrayList[index]);
						int arg_613_0 = CurrentBar;
					}
					this.renderNumber("SSWaveDownNumber" + sessionID + num4, num4.ToString(), num7, Low[ num7 ], num5, "MajorWaveDown", false);
				}
			}
			if (isConfirmed)
			{
				int newWaveNumber = this.getNewWaveNumber(this.arrMajorDown);
				this.arrMajorDown.Add(intStart);
				this.arrMajorDown.Add(intEnd);
				this.arrMajorDown.Add(newWaveNumber);
				this.arrMajorDown.Add(1);
				this.arrMajorDown.Add(intBarOffSet);
				this.renderNumber("SSWaveDownNumber" + sessionID + newWaveNumber, newWaveNumber.ToString(), CurrentBar - intEnd, Low[ CurrentBar - intEnd ], 1, "MajorWaveDown", false);
				this.MajorDownBarOffSet = false;
			}
		}
		private double getMinorDownLastWaveHigh()
		{
			if (this.arrMinorDown.Count == 0)
			{
				return 0.0;
			}
			int index = this.arrMinorDown.Count - 5;
			int num = Convert.ToInt32(this.arrMinorDown[index]);
			return  High[ CurrentBar - num ];
		}
		private double getMinorDownLastWaveLow()
		{
			if (this.arrMinorDown.Count == 0)
			{
				return 0.0;
			}
			int index = this.arrMinorDown.Count - 4;
			int num = Convert.ToInt32(this.arrMinorDown[index]);
			return Low[ CurrentBar - num ];
		}
		private void setMinorDownWaveEnd()
		{
			if ( Low[ this.intProcessingBarID ] < Low[ this.intPriorBarID ] )
			{
				this.MinorDownEnd = this.intProcessingBar;
				return;
			}
			this.MinorDownEnd = this.intPriorBar;
			int i = this.intPriorBar - 1;
			while (i > -1)
			{
				if ( Low[ CurrentBar - i ] == Low[ CurrentBar - this.intPriorBar ] && i <= this.MinorDownStart)
				{
					this.MinorDownEnd = i;
					if (i > this.MinorDownStart)
					{
						if (this.getBarDirection(i) != -1 || i != this.MinorDownStart)
						{
							i--;
							continue;
						}
					}
					else
					{
						this.MinorDownEnd = this.MinorDownStart;
					}
				}
				IL_B7:
				if ( MinorDownEnd ==  MinorDownStart && MinorDownBarOffSet)
				{
					MinorDownEnd++;
				}
				return;
				goto IL_B7;
			}
//goto IL_B7;
		}
		private void setMinorDownWaveStart(int ProcessingBar)
		{
			int num = CurrentBar - ProcessingBar;
			int num2 = num + 1;
			if (this.MinorDownStart == this.MinorDownEnd)
			{
				this.MinorDownStart = ProcessingBar;
			}
			else
			{
				if ( Low[ num ] <= Low[ num2 ] )
				{
					this.MinorDownStart = ProcessingBar;
				}
				else
				{
					if ( High[ num ] >=  High[ num2 ] )
					{
						this.MinorDownStart = ProcessingBar;
					}
					else
					{
						if (this.getBarDirection(CurrentBar - num2) < 1)
						{
							this.MinorDownStart = ProcessingBar;
						}
						else
						{
							this.MinorDownStart = ProcessingBar - 1;
						}
					}
				}
			}
			if (this.getBarDirection(ProcessingBar) > -1)
			{
				if (this.arrMinorDown.Count < 4)
				{
					this.MinorDownBarOffSet = true;
				}
				this.MinorDownStart = ProcessingBar;
			}
			this.MinorDownEnd = -1;
			this.MinorDownWave = -1;
			this.MinorDownConfirmed = false;
			this.MinorDownNewWaveLow = false;
			this.MinorDownUnconfirmedLow = -1;
		}
		private void resetMinorDown(int ProcessingBar)
		{
			this.arrMinorDown.Clear();
			if (ProcessingBar > 1)
			{
				this.setMinorDownWaveStart(ProcessingBar);
			}
			this.MinorDownResetBar = CurrentBar;
			this.MinorDownUnconfirmedLow = ProcessingBar;
			this.MinorDownPushedWaves = 0;
			this.MinorDownNewWaveNumber = 1;
			this.MinorDownPushedBarProcessed = -1;
			this.MinorDownPushedBarProcessedPrice = -1.0;
			this.lastMinorDownNewWaveProcess = -1;
			if (this.getBarDirection(ProcessingBar) == -1)
			{
				this.MinorDownBarOffSet = false;
			}
		}
		private void processMinorDownPushedWaves(int intProcessingBarID)
		{
			int num = 0;
			bool flag = false;
			if (this.MinorDownPushedBarProcessed == CurrentBar)
			{
				if ( High[ intProcessingBarID ] > this.MinorDownPushedBarProcessedPrice)
				{
					this.MinorDownPushedBarProcessed = CurrentBar;
					this.MinorDownPushedBarProcessedPrice =  High[ intProcessingBarID ];
					flag = true;
				}
			}
			else
			{
				this.MinorDownPushedBarProcessed = CurrentBar;
				this.MinorDownPushedBarProcessedPrice =  High[ intProcessingBarID ];
				flag = true;
			}
			if (flag && this.arrMinorDown.Count > 4)
			{
				if  ( High[ intProcessingBarID ] >= this.getMinorDownLastWaveHigh() || this.MajorDownNewWaveLow || this.isMinorDownReset)
				{
					for (int i = this.arrMinorDown.Count - 1; i > -1; i -= 5)
					{
						int num2 = Convert.ToInt32(this.arrMinorDown[i - 4]);
						int num3 = Convert.ToInt32(this.arrMinorDown[i - 3]);
						int num4 = Convert.ToInt32(this.arrMinorDown[i - 2]);
						int waveLevel = Convert.ToInt32(this.arrMinorDown[i - 1]);
						Convert.ToInt32(this.arrMinorDown[i]);
						int arg_129_0 = CurrentBar;
						int num5 = CurrentBar - num3;
						if ( High[ intProcessingBarID ] <  High[ CurrentBar - num2 ] && !this.MajorDownNewWaveLow && !this.isMinorDownReset)
						{
							break;
						}
						base.RemoveDrawObject("SSMinorWaveDownNumber" + this.sessionID + num4);
						base.RemoveDrawObject("SSMinorWaveDownLineA-" + this.sessionID + num4);
						base.RemoveDrawObject("SSMinorWaveDownLineB-" + this.sessionID + num4);
						if (this.arrMinorDown.Count > 9)
						{
							if (i == this.arrMinorDown.Count - 1 && Low[ 0 ] < Low[ num5 ] )
							{
								this.renderNumber(string.Concat(new object[]
								{
									"SSMinorWaveDownNumberObsolete",
									this.sessionID,
									num4,
									num2,
									num3
								}), num4.ToString(), num5, Low[ num5 ], waveLevel, "MinorWaveDownPushed", true);
							}
							else
							{
								this.renderNumber(string.Concat(new object[]
								{
									"SSMinorWaveDownNumberObsolete",
									this.sessionID,
									num4,
									num2,
									num3
								}), num4.ToString(), num5, Low[ num5 ], waveLevel, "MinorWaveDownPushed", false);
							}
						}
						num++;
					}
				}
				if (num > this.MinorDownPushedWaves)
				{
					this.MinorDownPushedWaves = num;
				}
				this.MinorDownNewWaveNumber = this.getNewWaveNumber(this.arrMinorDown);
				if (this.MinorDownPushedWaves > 0)
				{
					this.MinorDownNewWaveNumber -= this.MinorDownPushedWaves * 2;
				}
				this.isMinorDownReset = false;
			}
		}
		private bool isMinorDownWaveConfirmed(int intProcessingBar, int intPriorBar)
		{
			this.intBarDirection = this.getBarDirection(intProcessingBar);
			this.intLastBarDirection = this.getBarDirection(intPriorBar);
			bool result = false;
			if ( Low[ CurrentBar - intProcessingBar ] >  Low[ CurrentBar - intPriorBar ] )
			{
				if (!this.MinorDownBarOffSet || this.MinorDownStart != intPriorBar || this.getBarDirection(intProcessingBar) == 1)
				{
					result = true;
				}
			}
			else
			{
				if (this.intBarDirection == 1 && this.intLastBarDirection == 1)
				{
					result = true;
				}
				if (this.intBarDirection == 1 && this.intLastBarDirection == 0 && this.getBarDirection(this.findLastUpOrDownBar(intPriorBar)) == -1)
				{
					result = true;
				}
				if (this.intBarDirection == 1 && this.intLastBarDirection == 0 && Low[ CurrentBar - intProcessingBar ] <=  Low[ CurrentBar - intPriorBar ] )
				{
					result = true;
				}
				if (this.intBarDirection == 1 && this.intLastBarDirection == -1)
				{
					result = true;
				}
			}
			return result;
		}
		private void processMinorDownWaves(int intStart, int intEnd, bool intBarOffSet, int sessionID, bool isConfirmed)
		{
			base.RemoveDrawObject("SSMinorWaveDownNumber");
			ArrayList arrayList = new ArrayList();
			if (this.arrMinorDown.Count > 4)
			{
				int num = 0;
				if (intStart == intEnd)
				{
					for (int i = 0; i < this.arrMinorDown.Count - 1; i += 5)
					{
						int num2 = Convert.ToInt32(this.arrMinorDown[i]);
						int num3 = Convert.ToInt32(this.arrMinorDown[i + 1]);
						int num4 = Convert.ToInt32(this.arrMinorDown[i + 2]);
						int num5 = Convert.ToInt32(this.arrMinorDown[i + 3]);
						int num6 = Convert.ToInt32(this.arrMinorDown[i + 4]);
						int arg_9E_0 = CurrentBar;
						int num7 = CurrentBar - num3;
						if ( High[ CurrentBar - intStart ] >=  High[ CurrentBar - num2 ] )
						{
							int newWaveNumber = this.getNewWaveNumber(this.arrMinorDown);
							this.arrMinorDown.Add(intStart);
							this.arrMinorDown.Add(intEnd);
							this.arrMinorDown.Add(newWaveNumber);
							this.arrMinorDown.Add(1);
							this.arrMinorDown.Add(intBarOffSet);
							this.renderNumber("SSMinorWaveDownNumber" + sessionID + newWaveNumber, newWaveNumber.ToString(), CurrentBar - intEnd,  Low[ CurrentBar - intEnd ], 1, "MinorWaveDownPushed", false);
							this.MinorDownBarOffSet = false;
							return;
						}
					}
				}
				if (this.debugTempArrayDown)
				{
					base.Print("-------------> TEMP ARRAY START");
				}
				for (int j = 0; j < this.arrMinorDown.Count - 1; j += 5)
				{
					int num2 = Convert.ToInt32(this.arrMinorDown[j]);
					int num3 = Convert.ToInt32(this.arrMinorDown[j + 1]);
					int num4 = Convert.ToInt32(this.arrMinorDown[j + 2]);
					int num5 = Convert.ToInt32(this.arrMinorDown[j + 3]);
					int num6 = Convert.ToInt32(this.arrMinorDown[j + 4]);
					int arg_232_0 = CurrentBar;
					int num7 = CurrentBar - num3;
					if (this.debugTempArrayDown)
					{
						base.Print("________________________________");
						base.Print("i=" + j);
						base.Print("waveStart:" + num2);
						base.Print("waveEnd:" + num3);
						base.Print("waveNumber:" + num4);
						base.Print("waveLevel:" + num5);
						base.Print("waveBarOffSet:" + num6);
						base.Print("________________________________");
					}
					if ( High[ CurrentBar - intStart ] >=  High[ CurrentBar - num2 ] )
					{
						num++;
						int num8 = Convert.ToInt32(arrayList[arrayList.Count - 5]);
						int num9 = Convert.ToInt32(arrayList[arrayList.Count - 4]);
						int num10 = Convert.ToInt32(arrayList[arrayList.Count - 3]);
						int num11 = Convert.ToInt32(arrayList[arrayList.Count - 2]);
						int num12 = Convert.ToInt32(arrayList[arrayList.Count - 1]);
						int arg_386_0 = CurrentBar;
						int num13 = CurrentBar - num9;
						arrayList.RemoveAt(arrayList.Count - 1);
						arrayList.RemoveAt(arrayList.Count - 1);
						arrayList.RemoveAt(arrayList.Count - 1);
						arrayList.RemoveAt(arrayList.Count - 1);
						arrayList.RemoveAt(arrayList.Count - 1);
						arrayList.Add(num8);
						arrayList.Add(num3);
						arrayList.Add(num10);
						if (num11 == 2)
						{
							arrayList.Add(2);
						}
						else
						{
							arrayList.Add(num11 + 1);
						}
						arrayList.Add(num12);
						if (num < 2)
						{
							this.renderNumber(string.Concat(new object[]
							{
								"SSMinorWaveDownNumberObsolete",
								sessionID,
								CurrentBar,
								num8,
								num9
							}), num10.ToString(), num13,  Low[ num13 ], num11, "MinorWaveDownPushed", false);
							if (this.renderNumbersHorizontal)
							{
							}
						}
					}
					else
					{
						arrayList.Add(num2);
						arrayList.Add(num3);
						arrayList.Add(num4);
						arrayList.Add(num5);
						arrayList.Add(num6);
					}
				}
				if (this.debugTempArrayDown)
				{
					base.Print("-------------> TEMP ARRAY END");
				}
				this.arrMinorDown.Clear();
				for (int k = 0; k < arrayList.Count - 1; k += 5)
				{
					int num2 = Convert.ToInt32(arrayList[k]);
					int num3 = Convert.ToInt32(arrayList[k + 1]);
					int num4 = Convert.ToInt32(arrayList[k + 2]);
					int num5 = Convert.ToInt32(arrayList[k + 3]);
					int num6 = Convert.ToInt32(arrayList[k + 4]);
					int arg_58E_0 = CurrentBar;
					int num7 = CurrentBar - num3;
					this.arrMinorDown.Add(num2);
					this.arrMinorDown.Add(num3);
					this.arrMinorDown.Add(num4);
					this.arrMinorDown.Add(num5);
					this.arrMinorDown.Add(num6);
					if (num4 > 1)
					{
						int index = k - 4;
						Convert.ToInt32(arrayList[index]);
						int arg_617_0 = CurrentBar;
					}
					this.renderNumber("SSMinorWaveDownNumber" + sessionID + num4, num4.ToString(), num7,  Low[ num7 ], num5, "MinorWaveDown", false);
				}
			}
			if (isConfirmed)
			{
				int newWaveNumber2 = this.getNewWaveNumber(this.arrMinorDown);
				this.arrMinorDown.Add(intStart);
				this.arrMinorDown.Add(intEnd);
				this.arrMinorDown.Add(newWaveNumber2);
				this.arrMinorDown.Add(1);
				this.arrMinorDown.Add(intBarOffSet);
				this.renderNumber("SSMinorWaveDownNumber" + sessionID + newWaveNumber2, newWaveNumber2.ToString(), CurrentBar - intEnd,  Low[ CurrentBar - intEnd ], 1, "MinorWaveDown", false);
				this.MinorDownBarOffSet = false;
			}
		}
		private int getNewWaveNumber(ArrayList arr)
		{
			return Convert.ToInt32(arr.Count / 5 * 2 + 1);
		}
		private int findLastUpOrDownBar(int bar)
		{
			int result = -1;
			for (int i = bar; i > -1; i--)
			{
				if (this.getBarDirection(i) != 0)
				{
					result = i;
					return result;
				}
			}
			return result;
		}


		private int getBarDirection(int bar)
		{
			bar = CurrentBar - bar;
			if ( Open[ bar ] == Close[ bar ] )
			{
				return 0;
			}
			if ( Open[ bar ] < Close[ bar ] ) 
			{
				return 1;
			}
			return -1;
		}
		
		private double getSpacingFromBar(int l)
		{
//			double num = 0.5 * (double)this.indicatorSpacing;
//			double num2 = 0.5 * (double)this.indicatorSpacingBetweenNumbers;
//			Print("IndicatorSpacingInput" + indicatorSpacing);
//			if (l < 2)
//			{
//				l = 1;
//			}
			if (this.isIntradayChart)
			{
				if (l > 1)
				{
					return  TickSize * Space;
				}
				return  TickSize * (Space * SpaceBetween );
			}
			else
			{
				if (l > 1)
				{
					return TickSize * Space;
				}
				return  TickSize * (Space * SpaceBetween );
			}
		}
		
		private void renderNumber(string name, string value, int barsAgo, double y, int waveLevel, string waveType, bool moveNumber)
		{
			int num = this.mwSize;
			bool bold = false;
			//var  majorWaveUpUnconfirmedColor = Brushes.Black; 
			Brush majorWaveUpUnconfirmedColor = wDefaultColor;
			// FontStyle style = this.wDefaultFontStyle;
			double spacingFromBar;
			if (this.renderNumbersHorizontal)
			{
				spacingFromBar = this.getSpacingFromBar(1);
			}
			else
			{
				spacingFromBar = this.getSpacingFromBar(waveLevel);
			}
			switch (waveType)
			{
			case "MajorWaveUp":
				if (!this.mwShow)
				{
					value = "";
				}
				if (!this.renderNumbersHorizontal && !this.mwpShow)
				{
					spacingFromBar = this.getSpacingFromBar(1);
				}
				y += spacingFromBar;
				majorWaveUpUnconfirmedColor = MajorWaveUpColor;
				num = this.mwSize;
				//style = FontStyle.Bold;
				bold = true;
				break;
			case "MajorWaveDown":
				if (!this.mwShow)
				{
					value = "";
				}
				if (!this.renderNumbersHorizontal && !this.mwpShow)
				{
					spacingFromBar = this.getSpacingFromBar(1);
				}
				y -= spacingFromBar;
				majorWaveUpUnconfirmedColor = MajorWaveDownColor;
				num = this.mwSize;
				//style = FontStyle.Bold;
				bold = true;
				break;
			case "MajorWaveUpPushed":
				if (!this.mwShow)
				{
					value = "";
				}
				if (!this.mwpShow)
				{
					value = "";
				}
				if (this.renderNumbersHorizontal && moveNumber)
				{
					barsAgo += waveLevel;
					if (moveNumber)
					{
						barsAgo = barsAgo + this.indicatorSpacingBetweenNumbers - 1;
					}
				}
				y += spacingFromBar;
				majorWaveUpUnconfirmedColor = MajorWaveUpPushedColor; //  this.mwPushedColor;
				num = this.mwSize;
				//style = FontStyle.Bold;
				bold = true;
				break;
			case "MajorWaveDownPushed":
				if (!this.mwShow)
				{
					value = "";
				}
				if (!this.mwpShow)
				{
					value = "";
				}
				if (this.renderNumbersHorizontal && moveNumber)
				{
					barsAgo += waveLevel;
					if (moveNumber)
					{
						barsAgo = barsAgo + this.indicatorSpacingBetweenNumbers - 1;
					}
				}
				y -= spacingFromBar;
				majorWaveUpUnconfirmedColor = MinorWaveDownPushedColor; //this.mwdPushedColor;
				num = this.mwSize;
				//style = FontStyle.Bold;
				bold = true;
				break;
			case "MajorWaveUpNew":
				if (!this.mwShow)
				{
					value = "";
				}
				y += spacingFromBar;
// convert blue to the variable color MajorWaveUpUnconfirmedColor
				majorWaveUpUnconfirmedColor = MinorWaveUpColor;  //MajorWaveUpUnconfirmedColor;
				num = this.mwSize;
				//style = FontStyle.Bold;
				bold = true;
				break;
			case "MajorWaveDownNew":
				if (!this.mwShow)
				{
					value = "";
				}
				y -= spacingFromBar;
				majorWaveUpUnconfirmedColor = MinorWaveDownColor; //this.mwdNewColor;
				num = this.mwSize;
				//style = FontStyle.Bold;
				bold = true;
				break;
			case "MinorWaveUp":
				if (!this.miShow)
				{
					value = "";
				}
				if (!this.renderNumbersHorizontal && !this.mipShow)
				{
					spacingFromBar = this.getSpacingFromBar(1);
				}
				y += spacingFromBar;
				majorWaveUpUnconfirmedColor = MinorWaveUpColor; //  this.miwColor;
				num = this.miwSize;
				// style = FontStyle.Regular;
				bold = false;
				break;
			case "MinorWaveDown":
				if (!this.miShow)
				{
					value = "";
				}
				if (!this.renderNumbersHorizontal && !this.mipShow)
				{
					spacingFromBar = this.getSpacingFromBar(1);
				}
				y -= spacingFromBar;
				majorWaveUpUnconfirmedColor = MinorWaveDownColor; //  this.miwdColor;
				num = this.miwSize;
				//style = FontStyle.Regular;
				bold = false;
				break;
			case "MinorWaveUpPushed":
				if (!this.miShow)
				{
					value = "";
				}
				if (!this.mipShow)
				{
					value = "";
				}
				if (this.renderNumbersHorizontal && moveNumber)
				{
					barsAgo += waveLevel;
					if (moveNumber)
					{
						barsAgo = barsAgo + this.indicatorSpacingBetweenNumbers - 1;
					}
				}
				y += spacingFromBar;
				majorWaveUpUnconfirmedColor = MinorWaveUpPushedColor; //  this.miwPushedColor;
				num = this.miwSize;
				//style = FontStyle.Regular;
				bold = false;
				break;
			case "MinorWaveDownPushed":
				if (!this.miShow)
				{
					value = "";
				}
				if (!this.mipShow)
				{
					value = "";
				}
				if (this.renderNumbersHorizontal)
				{
					barsAgo += waveLevel;
					if (moveNumber)
					{
						barsAgo = barsAgo + this.indicatorSpacingBetweenNumbers - 1;
					}
				}
				y -= spacingFromBar;
				majorWaveUpUnconfirmedColor = MinorWaveDownPushedColor; // this.miwdPushedColor;
				num = this.miwSize;
				//style = FontStyle.Regular;
				bold = false;
				break;
			case "MinorWaveUpNew":
				if (!this.miShow)
				{
					value = "";
				}
				y += spacingFromBar;
				majorWaveUpUnconfirmedColor = MinorWaveUpColor; // = this.miwNewColor;
				num = this.miwSize;
				//style = FontStyle.Regular;
				bold = false;
				break;
			case "MinorWaveDownNew":
				if (!this.miShow)
				{
					value = "";
				}
				y -= spacingFromBar;
				majorWaveUpUnconfirmedColor = MinorWaveDownColor;  // this.miwdNewColor;
				num = this.miwSize;
				//style = FontStyle.Regular;
				bold = false;
				break;
			}
			if (barsAgo > CurrentBar)
			{
				barsAgo = CurrentBar;
			}
			// MARK: - TODO move to vars section
			var size = MinorFontSize;
			if (!bold) {
				size = MajorFontSize;
			}
			Print("spacing from bar: " + spacingFromBar.ToString());
			NinjaTrader.Gui.Tools.SimpleFont myFont = new NinjaTrader.Gui.Tools.SimpleFont("Courier New", num) { Size = size, Bold = bold };
			//DrawText(name, true, value, barsAgo, y, 1, majorWaveUpUnconfirmedColor, new Font(this.indicatorFont, (float)num, style), StringAlignment.Center, this.indicatorOutlineColor, this.indicatorBackColor, this.indicatorOpacity);	
			Draw.Text(this, name, true, value, barsAgo,  y, 1, majorWaveUpUnconfirmedColor, myFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, indicatorOpacity);
		}
		
		private void printExceptionInformation(Exception e)
		{
			//Log( name + " error. See output window for detailed error message.", 3);
			Log( Name + " error. See output window for detailed error message.", NinjaTrader.Cbi.LogLevel.Alert);
			base.Print("---------------------------");
			base.Print(string.Concat(new object[]
			{
				Name,
				" ERROR @ ",
				DateTime.Now,
				":"
			}));
			base.Print("Instrument - " + Instrument.FullName);
			base.Print("Message - " + e.Message);
			base.Print("StackTrace - " + e.StackTrace);
			base.Print("-----------------------------");
		}	
		#endregion
		
		#region Properties
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="MajorWaveUpColor", Description="Major wave up color.", Order=1, GroupName="Parameters")]
		public Brush MajorWaveUpColor
		{ get; set; }

		[Browsable(false)]
		public string MajorWaveUpColorSerializable
		{
			get { return Serialize.BrushToString(MajorWaveUpColor); }
			set { MajorWaveUpColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="MajorWaveDownColor", Description="Major wave down color.", Order=2, GroupName="Parameters")]
		public Brush MajorWaveDownColor
		{ get; set; }

		[Browsable(false)]
		public string MajorWaveDownColorSerializable
		{
			get { return Serialize.BrushToString(MajorWaveDownColor); }
			set { MajorWaveDownColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="MajorWaveUpUnconfirmedColor", Description="Major wave up unconfirmed color.", Order=3, GroupName="Parameters")]
		public Brush MajorWaveUpUnconfirmedColor
		{ get; set; }

		[Browsable(false)]
		public string MajorWaveUpUnconfirmedColorSerializable
		{
			get { return Serialize.BrushToString(MajorWaveUpUnconfirmedColor); }
			set { MajorWaveUpUnconfirmedColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="MajorWaveDownUnconfirmedColor", Description="Major wave down unconfirmed color.", Order=4, GroupName="Parameters")]
		public Brush MajorWaveDownUnconfirmedColor
		{ get; set; }

		[Browsable(false)]
		public string MajorWaveDownUnconfirmedColorSerializable
		{
			get { return Serialize.BrushToString(MajorWaveDownUnconfirmedColor); }
			set { MajorWaveDownUnconfirmedColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="MajorWaveUpPushedColor", Description="Major wave up pushed color.", Order=5, GroupName="Parameters")]
		public Brush MajorWaveUpPushedColor
		{ get; set; }

		[Browsable(false)]
		public string MajorWaveUpPushedColorSerializable
		{
			get { return Serialize.BrushToString(MajorWaveUpPushedColor); }
			set { MajorWaveUpPushedColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="MinorWaveUpColor", Description="Minor wave up color.", Order=6, GroupName="Parameters")]
		public Brush MinorWaveUpColor
		{ get; set; }

		[Browsable(false)]
		public string MinorWaveUpColorSerializable
		{
			get { return Serialize.BrushToString(MinorWaveUpColor); }
			set { MinorWaveUpColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="MinorWaveDownColor", Description="Minor wave down color.", Order=7, GroupName="Parameters")]
		public Brush MinorWaveDownColor
		{ get; set; }

		[Browsable(false)]
		public string MinorWaveDownColorSerializable
		{
			get { return Serialize.BrushToString(MinorWaveDownColor); }
			set { MinorWaveDownColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="MinorWaveUpUnconfirmedColor", Order=8, GroupName="Parameters")]
		public Brush MinorWaveUpUnconfirmedColor
		{ get; set; }

		[Browsable(false)]
		public string MinorWaveUpUnconfirmedColorSerializable
		{
			get { return Serialize.BrushToString(MinorWaveUpUnconfirmedColor); }
			set { MinorWaveUpUnconfirmedColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="MinorWaveDownUnconfirmedColor", Order=9, GroupName="Parameters")]
		public Brush MinorWaveDownUnconfirmedColor
		{ get; set; }

		[Browsable(false)]
		public string MinorWaveDownUnconfirmedColorSerializable
		{
			get { return Serialize.BrushToString(MinorWaveDownUnconfirmedColor); }
			set { MinorWaveDownUnconfirmedColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="MinorWaveUpPushedColor", Order=10, GroupName="Parameters")]
		public Brush MinorWaveUpPushedColor
		{ get; set; }

		[Browsable(false)]
		public string MinorWaveUpPushedColorSerializable
		{
			get { return Serialize.BrushToString(MinorWaveUpPushedColor); }
			set { MinorWaveUpPushedColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="MinorWaveDownPushedColor", Order=11, GroupName="Parameters")]
		public Brush MinorWaveDownPushedColor
		{ get; set; }

		[Browsable(false)]
		public string MinorWaveDownPushedColorSerializable
		{
			get { return Serialize.BrushToString(MinorWaveDownPushedColor); }
			set { MinorWaveDownPushedColor = Serialize.StringToBrush(value); }
		}			
		
		//  
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Default Color", Order=12, GroupName="Parameters")]
		public Brush wDefaultColor
		{ get; set; }

		[Browsable(false)]
		public string wDefaultColorSerializable
		{
			get { return Serialize.BrushToString(wDefaultColor); }
			set { wDefaultColor = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Display(Name="CurrentSessionOnly", Description="Render Serial Sequent during todays session only", Order=12, GroupName="Parameters")]
		public bool CurrentSessionOnly
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Space", Description="Space in ticks between bar and numbers.", Order=13, GroupName="Parameters")]
		public int Space
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="SpaceBetween", Description="Space between pushed numbers. Based on tick size of chart.", Order=14, GroupName="Parameters")]
		public int SpaceBetween
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="RenderHorizontal", Description="Display pushed numbers horizontal.", Order=15, GroupName="Parameters")]
		public bool RenderHorizontal
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowMajor", Description="Display major wave numbers.", Order=16, GroupName="Parameters")]
		public bool ShowMajor
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowMinor", Description="Display minor wave numbers", Order=17, GroupName="Parameters")]
		public bool ShowMinor
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowMajorPushed", Description="Display major wave pushed numbers.", Order=18, GroupName="Parameters")]
		public bool ShowMajorPushed
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowMinorPushed", Description="Display minor wave pushed numbers.", Order=19, GroupName="Parameters")]
		public bool ShowMinorPushed
		{ get; set; }

		[NinjaScriptProperty]
		[Range(5, int.MaxValue)]
		[Display(Name="Minor Font Size", Description="Font size for major wave numbers. Valid values between 5 - 50", Order=20, GroupName="Parameters")]
		public int MajorFontSize
		{ get; set; }

		[NinjaScriptProperty]
		[Range(5, int.MaxValue)]
		[Display(Name="Major Font Size", Description="Font size for minor wave numbers. Valid values between 5 - 50.", Order=21, GroupName="Parameters")]
		public int MinorFontSize
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> UpWave
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DownWave
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DownWave2
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> UpWave2
		{
			get { return Values[3]; }
		}

		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SerialSequnt[] cacheSerialSequnt;
		public SerialSequnt SerialSequnt(Brush majorWaveUpColor, Brush majorWaveDownColor, Brush majorWaveUpUnconfirmedColor, Brush majorWaveDownUnconfirmedColor, Brush majorWaveUpPushedColor, Brush minorWaveUpColor, Brush minorWaveDownColor, Brush minorWaveUpUnconfirmedColor, Brush minorWaveDownUnconfirmedColor, Brush minorWaveUpPushedColor, Brush minorWaveDownPushedColor, Brush wDefaultColor, bool currentSessionOnly, int space, int spaceBetween, bool renderHorizontal, bool showMajor, bool showMinor, bool showMajorPushed, bool showMinorPushed, int majorFontSize, int minorFontSize)
		{
			return SerialSequnt(Input, majorWaveUpColor, majorWaveDownColor, majorWaveUpUnconfirmedColor, majorWaveDownUnconfirmedColor, majorWaveUpPushedColor, minorWaveUpColor, minorWaveDownColor, minorWaveUpUnconfirmedColor, minorWaveDownUnconfirmedColor, minorWaveUpPushedColor, minorWaveDownPushedColor, wDefaultColor, currentSessionOnly, space, spaceBetween, renderHorizontal, showMajor, showMinor, showMajorPushed, showMinorPushed, majorFontSize, minorFontSize);
		}

		public SerialSequnt SerialSequnt(ISeries<double> input, Brush majorWaveUpColor, Brush majorWaveDownColor, Brush majorWaveUpUnconfirmedColor, Brush majorWaveDownUnconfirmedColor, Brush majorWaveUpPushedColor, Brush minorWaveUpColor, Brush minorWaveDownColor, Brush minorWaveUpUnconfirmedColor, Brush minorWaveDownUnconfirmedColor, Brush minorWaveUpPushedColor, Brush minorWaveDownPushedColor, Brush wDefaultColor, bool currentSessionOnly, int space, int spaceBetween, bool renderHorizontal, bool showMajor, bool showMinor, bool showMajorPushed, bool showMinorPushed, int majorFontSize, int minorFontSize)
		{
			if (cacheSerialSequnt != null)
				for (int idx = 0; idx < cacheSerialSequnt.Length; idx++)
					if (cacheSerialSequnt[idx] != null && cacheSerialSequnt[idx].MajorWaveUpColor == majorWaveUpColor && cacheSerialSequnt[idx].MajorWaveDownColor == majorWaveDownColor && cacheSerialSequnt[idx].MajorWaveUpUnconfirmedColor == majorWaveUpUnconfirmedColor && cacheSerialSequnt[idx].MajorWaveDownUnconfirmedColor == majorWaveDownUnconfirmedColor && cacheSerialSequnt[idx].MajorWaveUpPushedColor == majorWaveUpPushedColor && cacheSerialSequnt[idx].MinorWaveUpColor == minorWaveUpColor && cacheSerialSequnt[idx].MinorWaveDownColor == minorWaveDownColor && cacheSerialSequnt[idx].MinorWaveUpUnconfirmedColor == minorWaveUpUnconfirmedColor && cacheSerialSequnt[idx].MinorWaveDownUnconfirmedColor == minorWaveDownUnconfirmedColor && cacheSerialSequnt[idx].MinorWaveUpPushedColor == minorWaveUpPushedColor && cacheSerialSequnt[idx].MinorWaveDownPushedColor == minorWaveDownPushedColor && cacheSerialSequnt[idx].wDefaultColor == wDefaultColor && cacheSerialSequnt[idx].CurrentSessionOnly == currentSessionOnly && cacheSerialSequnt[idx].Space == space && cacheSerialSequnt[idx].SpaceBetween == spaceBetween && cacheSerialSequnt[idx].RenderHorizontal == renderHorizontal && cacheSerialSequnt[idx].ShowMajor == showMajor && cacheSerialSequnt[idx].ShowMinor == showMinor && cacheSerialSequnt[idx].ShowMajorPushed == showMajorPushed && cacheSerialSequnt[idx].ShowMinorPushed == showMinorPushed && cacheSerialSequnt[idx].MajorFontSize == majorFontSize && cacheSerialSequnt[idx].MinorFontSize == minorFontSize && cacheSerialSequnt[idx].EqualsInput(input))
						return cacheSerialSequnt[idx];
			return CacheIndicator<SerialSequnt>(new SerialSequnt(){ MajorWaveUpColor = majorWaveUpColor, MajorWaveDownColor = majorWaveDownColor, MajorWaveUpUnconfirmedColor = majorWaveUpUnconfirmedColor, MajorWaveDownUnconfirmedColor = majorWaveDownUnconfirmedColor, MajorWaveUpPushedColor = majorWaveUpPushedColor, MinorWaveUpColor = minorWaveUpColor, MinorWaveDownColor = minorWaveDownColor, MinorWaveUpUnconfirmedColor = minorWaveUpUnconfirmedColor, MinorWaveDownUnconfirmedColor = minorWaveDownUnconfirmedColor, MinorWaveUpPushedColor = minorWaveUpPushedColor, MinorWaveDownPushedColor = minorWaveDownPushedColor, wDefaultColor = wDefaultColor, CurrentSessionOnly = currentSessionOnly, Space = space, SpaceBetween = spaceBetween, RenderHorizontal = renderHorizontal, ShowMajor = showMajor, ShowMinor = showMinor, ShowMajorPushed = showMajorPushed, ShowMinorPushed = showMinorPushed, MajorFontSize = majorFontSize, MinorFontSize = minorFontSize }, input, ref cacheSerialSequnt);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SerialSequnt SerialSequnt(Brush majorWaveUpColor, Brush majorWaveDownColor, Brush majorWaveUpUnconfirmedColor, Brush majorWaveDownUnconfirmedColor, Brush majorWaveUpPushedColor, Brush minorWaveUpColor, Brush minorWaveDownColor, Brush minorWaveUpUnconfirmedColor, Brush minorWaveDownUnconfirmedColor, Brush minorWaveUpPushedColor, Brush minorWaveDownPushedColor, Brush wDefaultColor, bool currentSessionOnly, int space, int spaceBetween, bool renderHorizontal, bool showMajor, bool showMinor, bool showMajorPushed, bool showMinorPushed, int majorFontSize, int minorFontSize)
		{
			return indicator.SerialSequnt(Input, majorWaveUpColor, majorWaveDownColor, majorWaveUpUnconfirmedColor, majorWaveDownUnconfirmedColor, majorWaveUpPushedColor, minorWaveUpColor, minorWaveDownColor, minorWaveUpUnconfirmedColor, minorWaveDownUnconfirmedColor, minorWaveUpPushedColor, minorWaveDownPushedColor, wDefaultColor, currentSessionOnly, space, spaceBetween, renderHorizontal, showMajor, showMinor, showMajorPushed, showMinorPushed, majorFontSize, minorFontSize);
		}

		public Indicators.SerialSequnt SerialSequnt(ISeries<double> input , Brush majorWaveUpColor, Brush majorWaveDownColor, Brush majorWaveUpUnconfirmedColor, Brush majorWaveDownUnconfirmedColor, Brush majorWaveUpPushedColor, Brush minorWaveUpColor, Brush minorWaveDownColor, Brush minorWaveUpUnconfirmedColor, Brush minorWaveDownUnconfirmedColor, Brush minorWaveUpPushedColor, Brush minorWaveDownPushedColor, Brush wDefaultColor, bool currentSessionOnly, int space, int spaceBetween, bool renderHorizontal, bool showMajor, bool showMinor, bool showMajorPushed, bool showMinorPushed, int majorFontSize, int minorFontSize)
		{
			return indicator.SerialSequnt(input, majorWaveUpColor, majorWaveDownColor, majorWaveUpUnconfirmedColor, majorWaveDownUnconfirmedColor, majorWaveUpPushedColor, minorWaveUpColor, minorWaveDownColor, minorWaveUpUnconfirmedColor, minorWaveDownUnconfirmedColor, minorWaveUpPushedColor, minorWaveDownPushedColor, wDefaultColor, currentSessionOnly, space, spaceBetween, renderHorizontal, showMajor, showMinor, showMajorPushed, showMinorPushed, majorFontSize, minorFontSize);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SerialSequnt SerialSequnt(Brush majorWaveUpColor, Brush majorWaveDownColor, Brush majorWaveUpUnconfirmedColor, Brush majorWaveDownUnconfirmedColor, Brush majorWaveUpPushedColor, Brush minorWaveUpColor, Brush minorWaveDownColor, Brush minorWaveUpUnconfirmedColor, Brush minorWaveDownUnconfirmedColor, Brush minorWaveUpPushedColor, Brush minorWaveDownPushedColor, Brush wDefaultColor, bool currentSessionOnly, int space, int spaceBetween, bool renderHorizontal, bool showMajor, bool showMinor, bool showMajorPushed, bool showMinorPushed, int majorFontSize, int minorFontSize)
		{
			return indicator.SerialSequnt(Input, majorWaveUpColor, majorWaveDownColor, majorWaveUpUnconfirmedColor, majorWaveDownUnconfirmedColor, majorWaveUpPushedColor, minorWaveUpColor, minorWaveDownColor, minorWaveUpUnconfirmedColor, minorWaveDownUnconfirmedColor, minorWaveUpPushedColor, minorWaveDownPushedColor, wDefaultColor, currentSessionOnly, space, spaceBetween, renderHorizontal, showMajor, showMinor, showMajorPushed, showMinorPushed, majorFontSize, minorFontSize);
		}

		public Indicators.SerialSequnt SerialSequnt(ISeries<double> input , Brush majorWaveUpColor, Brush majorWaveDownColor, Brush majorWaveUpUnconfirmedColor, Brush majorWaveDownUnconfirmedColor, Brush majorWaveUpPushedColor, Brush minorWaveUpColor, Brush minorWaveDownColor, Brush minorWaveUpUnconfirmedColor, Brush minorWaveDownUnconfirmedColor, Brush minorWaveUpPushedColor, Brush minorWaveDownPushedColor, Brush wDefaultColor, bool currentSessionOnly, int space, int spaceBetween, bool renderHorizontal, bool showMajor, bool showMinor, bool showMajorPushed, bool showMinorPushed, int majorFontSize, int minorFontSize)
		{
			return indicator.SerialSequnt(input, majorWaveUpColor, majorWaveDownColor, majorWaveUpUnconfirmedColor, majorWaveDownUnconfirmedColor, majorWaveUpPushedColor, minorWaveUpColor, minorWaveDownColor, minorWaveUpUnconfirmedColor, minorWaveDownUnconfirmedColor, minorWaveUpPushedColor, minorWaveDownPushedColor, wDefaultColor, currentSessionOnly, space, spaceBetween, renderHorizontal, showMajor, showMinor, showMajorPushed, showMinorPushed, majorFontSize, minorFontSize);
		}
	}
}

#endregion
