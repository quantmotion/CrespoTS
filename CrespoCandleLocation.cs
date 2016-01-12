#region Using declarations
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    /// <summary>
    /// Enter the description of your new custom indicator here
    /// </summary>
    [Description("Enter the description of your new custom indicator here")]
    public class CrespoCandleLocation : Indicator, ICustomTypeDescriptor
    {
        #region Variables
		// Tradeable Events
        private NinjaTrader.Indicator.CTS_TradableEvent tradableEvent = CTS_TradableEvent.AnyBar; // Default setting for TradedableEvent
		private int prevBarMinSize = 5;
		private int engulfingBarMinSize = 5;
		private double pipBodySize = 1.6;
		private int numPreviousBars = 3;
		private double wickPercentage = 0.2;
		private int maLength = 20;
		private NinjaTrader.Indicator.CTS_MAType maType= CTS_MAType.Simple;
		private int limitDistanceFromMA = 30;
		private int pipMaxBodySize = 30;
		private double bodyPctOfTotalBarSize = 0.6;
		private int minSize = 2;
		private int maxSize = 4;
		private double maxWickPct = 0.2;
		private bool ignoreIndecisionBars = false;
		private CTS_BodyOrTotalSize compareBodyOrTotalSize = CTS_BodyOrTotalSize.Body;
		private double atLeastXPctTheSizeOfPreviousBar = 1.1;
		private double longWickMinPct = 0.7;
		private double shortWickMaxPct = 0.05;
		private double longWickPctAbovePreviousHigh = 0.5;
		private double openAndClosePctLocation = 0.3;
		private bool notPinBarIfInsideBar = true;
		private int barsToLookBack = 2;
        
		// Location
        private NinjaTrader.Indicator.CTS_Location location = CTS_Location.Anywhere; // Default setting for Location
		private int loc_MALength = 30;
		private NinjaTrader.Indicator.CTS_MAType loc_MAType = CTS_MAType.Exponential;
		private int loc_PipDistance = 15;
		private double loc_BBDeviation = 2;
		private double loc_BBBodyPct = 0.05;
		
		private int tradedir = 0;
        #endregion

        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
        protected override void Initialize()
        {
            Add(new Plot(Color.FromKnownColor(KnownColor.Fuchsia), PlotStyle.Dot, "Pattern"));
			Plots[0].Pen.Width = 4;
            Overlay				= true;
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
			if(CurrentBar < 100) return;
			
			bool pattern = IsCandlestickPattern();
			bool location = CheckLocation();
			
			Print(pattern + ", " + location);
			if(IsCandlestickPattern() && CheckLocation())
			{
				Pattern.Set((High[0] + Low[0])/2);
			}
			
        }
		
		bool IsCandlestickPattern()
		{
			if(TradableEvent == NinjaTrader.Indicator.CTS_TradableEvent.AnyBar)
			{
				return true;
			}
			
			bool pattern = false;
			
			pattern = IsBullIgnitingElephantBar(0) && TradableEvent == CTS_TradableEvent.BullIgnitingElephantBar;
			
			if(pattern)
			{
				tradedir = 1;
				return true;
			}
			
			pattern = IsBearIgnitingElephantBar(0) && TradableEvent == CTS_TradableEvent.BearIgnitingElephantBar;
			
			if(pattern)
			{
				tradedir = -1;
				return true;
			}
			
			pattern = IsBullExhaustionElephantBar(0) && TradableEvent == CTS_TradableEvent.BullExhaustionElephantBar;
			
			if(pattern)
			{
				tradedir = 1;
				return true;
			}
			
			pattern = IsBearExhaustionElephantBar(0) && TradableEvent == CTS_TradableEvent.BearExhaustionElephantBar;
			
			if(pattern)
			{
				tradedir = -1;
				return true;
			}
			
			pattern = IsBullBodyEngulfing(0) && TradableEvent == CTS_TradableEvent.BullBodyEngulfing;
			
			if(pattern)
			{
				tradedir = 1;
				return true;
			}
			
			pattern = IsBearBodyEngulfing(0) && TradableEvent == CTS_TradableEvent.BearBodyEngulfing;
			
			if(pattern)
			{
				tradedir = -1;
				return true;
			}
			
			pattern = IsBullBodyEngulfingOppositeColor(0) && TradableEvent == CTS_TradableEvent.BullBodyEngulfingOppositeColor;
			
			if(pattern)
			{
				tradedir = 1;
				return true;
			}
			
			pattern = IsBearBodyEngulfingOppositeColor(0) && TradableEvent == CTS_TradableEvent.BearBodyEngulfingOppositeColor;
			
			if(pattern)
			{
				tradedir = -1;
				return true;
			}
			
			pattern = IsBullBodyEngulfingIgniting(0) && TradableEvent == CTS_TradableEvent.BullBodyEngulfingIgniting;
			
			if(pattern)
			{
				tradedir = 1;
				return true;
			}
			
			pattern = IsBearBodyEngulfingIgniting(0) && TradableEvent == CTS_TradableEvent.BearBodyEngulfingIgniting;
			
			if(pattern)
			{
				tradedir = -1;
				return true;
			}
			
			pattern = IsBullBodyEngulfingOppositeColorIgniting(0) && TradableEvent == CTS_TradableEvent.BullBodyEngulfingOppositeColorIgniting;
			
			if(pattern)
			{
				tradedir = 1;
				return true;
			}
			
			pattern = IsBearBodyEngulfingOppositeColorIgniting(0) && TradableEvent == CTS_TradableEvent.BearBodyEngulfingOppositeColorIgniting;
			
			if(pattern)
			{
				tradedir = -1;
				return true;
			}
			
			pattern = IsBodyEngulfing(prevBarMinSize, engulfingBarMinSize, atLeastXPctTheSizeOfPreviousBar, 0) && TradableEvent == CTS_TradableEvent.BodyEngulfing;
			
			if(pattern)
			{
				return true;
			}
			
			pattern = IsExhausting(maLength, maType, limitDistanceFromMA, 0) && TradableEvent == CTS_TradableEvent.Exhausting;
			
			if(pattern)
			{
				return true;
			}
			
			pattern = IsIgniting(maLength, maType, limitDistanceFromMA, 0) && TradableEvent == CTS_TradableEvent.Igniting;
			
			if(pattern)
			{
				return true;
			}
			
			pattern = IsIndecisionBar(bodyPctOfTotalBarSize, 0) && TradableEvent == CTS_TradableEvent.IndecisionBar;
			
			if(pattern)
			{
				return true;
			}
			
			pattern = IsBullElephantBar(0) && TradableEvent == CTS_TradableEvent.BullElephantBar;
			
			if(pattern)
			{
				tradedir = 1;
				return true;
			}
			
			pattern = IsBearElephantBar(0) && TradableEvent == CTS_TradableEvent.BearElephantBar;
			
			if(pattern)
			{
				tradedir = -1;
				return true;
			}
			
			pattern = IsNarrowRangeBar(0) && TradableEvent == CTS_TradableEvent.NarrowRangeBar;
			
			if(pattern)
			{
				return true;
			}
			
			pattern = IsPinBarTop(0) && TradableEvent == CTS_TradableEvent.PinBarTop;
			
			if(pattern)
			{
				return true;
			}
			
			pattern = IsPinBarBottom(0) && TradableEvent == CTS_TradableEvent.PinBarBottom;
			
			if(pattern)
			{
				return true;
			}
			
			pattern = IsPinBarTopInsideBarCombo(0) && TradableEvent == CTS_TradableEvent.PinBarTopInsideBarCombo;
			
			if(pattern)
			{
				return true;
			}
			
			pattern = IsPinBarBottomInsideBarCombo(0) && TradableEvent == CTS_TradableEvent.PinBarBottomInsideBarCombo;
			
			if(pattern)
			{
				return true;
			}
			
			pattern = IsVelezBuySetup(maLength, maType, numPreviousBars, wickPercentage, ignoreIndecisionBars, 0) && TradableEvent == CTS_TradableEvent.VelezBuySetup;
			
			if(pattern)
			{
				tradedir = 1;
				return true;
			}
			
			pattern = IsVelezSellSetup(maLength, maType, numPreviousBars, wickPercentage, ignoreIndecisionBars, 0) && TradableEvent == CTS_TradableEvent.VelezSellSetup;
			
			if(pattern)
			{
				tradedir = -1;
				return true;
			}
			
			pattern = IsBottomingTailSetup(maLength, maType, numPreviousBars, ignoreIndecisionBars, 0) && TradableEvent == CTS_TradableEvent.BottomingTailSetup;
			
			if(pattern)
			{
				tradedir = 1;
				return true;
			}
			
			pattern = IsToppingTailSetup(maLength, maType, numPreviousBars, ignoreIndecisionBars, 0) && TradableEvent == CTS_TradableEvent.ToppingTailSetup;
			
			if(pattern)
			{
				tradedir = -1;
				return true;
			}
			
			return false;
		}
		
		#region General Functions
		
		private double Body(int shift)
		{
			return Math.Abs(Close[shift] - Open[shift]);
		}
		
		private double Range(int shift)
		{
			return High[shift] - Low[shift];
		}
		
		private bool IsGreen(int shift)
		{
			if(Close[shift] > Open[shift])
				return true;
			return false;
		}
		
		private bool IsRed(int shift)
		{
			if(Close[shift] < Open[shift])
				return true;
			return false;
		}
		
		private double BodyTop(int shift)
		{
			return Math.Max(Close[shift], Open[shift]);
		}
		
		private double BodyBottom(int shift)
		{
			return Math.Min(Close[shift], Open[shift]);
		}
		
		private double TopWick(int shift)
		{
			return High[shift] - BodyTop(shift);
		}
		
		private double BottomWick(int shift)
		{
			return BodyBottom(shift) - Low[shift];
		}
		
		private double MA(int period, CTS_MAType type, int shift)
		{
			double result  = 0;
			
			switch(type)
			{
				case CTS_MAType.Simple:
					result = SMA(period)[shift];
					break;
					
				case CTS_MAType.Exponential:
					result = EMA(period)[shift];
					break;
					
				case CTS_MAType.Hull:
					result = HMA(period)[shift];
					break;
					
				case CTS_MAType.DoubleExponential:
					result = DEMA(period)[shift];
					break;
					
				default:
					result = SMA(period)[shift];
					break;
			}
			
			return result;
		}
		
		private bool PreviousBarIsOppositeColor(int shift)
		{
			if((Close[shift] > Open[shift]) != (Close[shift+1] > Open[shift + 1]))
				return true;
			
			return false;
		}
		#endregion
		
		#region Candlestick Patterns
	/*	returns true when the entry bar is bigger than previous bar’s body AND
		when previous bar body size is not less than prevBarMinSize pips AND 
		when entry bar body size is not less than engulfingBarMinSize pips.*/
		private bool IsBodyEngulfing(int prevBarMinSize, int engulfingBarMinSize, double atLeastXPctTheSizeOfPreviousBar , int shift)
		{
			double barBody = Body(shift);
			double prevBarBody = Body(shift+1);
			
			if(prevBarBody > prevBarMinSize*TickSize && 
				barBody > engulfingBarMinSize*TickSize &&
				barBody > prevBarBody &&
				barBody >= atLeastXPctTheSizeOfPreviousBar*prevBarBody)
			{
				return true;
			}
			return false;	
		}
	
		private bool IsInsideBar(int prevBarMinSize, int engulfingBarMinSize, double atLeastXPctTheSizeOfPreviousBar , int shift)
		{
			double barBody = Body(shift);
			double prevBarBody = Body(shift+1);
			
			if(barBody > prevBarMinSize*TickSize && 
				prevBarBody > engulfingBarMinSize*TickSize &&
				prevBarBody > barBody &&
				prevBarBody >= atLeastXPctTheSizeOfPreviousBar*barBody)
			{
				return true;
			}
			return false;	
		}
	
	/*	returns true if the body of the bar is pipBodySize the size of any of the previous numPreviousBars bars. AND
		(if it's a green bar, it has an upper wick that is not more than wickPercentage of the bar OR
		if it's a red bar,  it has an lower wick that is not more than wickPercentage of the bar)*/
		private bool IsElephantBar(double xTimesTheSize, int numPreviousBars, double wickPercentage, CTS_BodyOrTotalSize compareBodyOrTotalSize, int shift)
		{
			bool bigger = true;
			
			for(int i = shift + 1; i <= shift + 1 + numPreviousBars; i++)
			{
				if(compareBodyOrTotalSize == CTS_BodyOrTotalSize.Body && Body(shift) <= Body(i)*xTimesTheSize)
				{
					bigger = false;
					break;
				}
				else if(compareBodyOrTotalSize == CTS_BodyOrTotalSize.TotalSize && Range(shift) <= Range(i)*xTimesTheSize)
				{
					bigger = false;
					break;
				}
			}
			
			if(bigger && ((IsGreen(shift) && TopWick(shift) <= Range(shift)*wickPercentage) || (IsRed(shift) && BottomWick(shift) <= Range(shift)*wickPercentage)))
			{
				return true;
			}
			
			return false;
		}
		
	/*	returns true if is ElephantBar  AND
		closes above the previous bar AND
		IsGreen is true*/
		private bool IsBullElephantBar(int shift)
		{
			if(IsElephantBar(pipBodySize, numPreviousBars, wickPercentage, compareBodyOrTotalSize, shift) && Close[shift] > Close[shift + 1] && IsGreen(shift))
			{
				return true;
			}
			return false;
		}
		
	/*	returns true if is ElephantBar  AND 
		closes below the previous bar AND
		IsRed is true*/
		private bool IsBearElephantBar(int shift)
		{
			if(IsElephantBar(pipBodySize, numPreviousBars, wickPercentage, compareBodyOrTotalSize, shift) && Close[shift] < Close[shift + 1] && IsRed(shift))
			{
				return true;
			}
			return false;
		}
	
	/*	returns true if the bar is igniting. 
		Instantiate the moving average according to period and type (20, simple)
		it's igniting when (starts away from the ma AND move towards it) or ( starts up to pipDistance from the ma)*/
		private bool IsIgniting(int period, CTS_MAType type, int limitDistanceFromMA, int shift)
		{
			double ma = MA(period, type, shift);
			
			if(Low[shift] <= ma + limitDistanceFromMA*TickSize && Close[shift] > Open[shift])
			{
				return true;
			}
			if(High[shift] >= ma - limitDistanceFromMA*TickSize && Close[shift] < Open[shift])
			{
				return true;
			}
			return false;
		}
		
	/*	returns true if the bar is exhausting. 
		it 's exhausting when (starts far away from the ma (longer than limitDistanceFromMA)  and moves even further away)*/	
		private bool IsExhausting(int period, CTS_MAType type, int limitDistanceFromMA, int shift)
		{
			return !IsIgniting(period, type, limitDistanceFromMA, shift);
		}
		
	/*	(returns true if the bar is an indecision bar. )
		An indecision bar is a bar with a body that is is pipMaxBodySize pips or less and the body is bodyPctOfTotalBarSize or less of the bar's size. 	
		So if IndecisionBar(3, 0.3), it means that the maximum bodi size of this bar is 3 (or less) and the body is maximum of 30% (or less) of the total size of the bar, then this will return true*/
		private bool IsIndecisionBar(double bodyPctOfTotalBarSize, int shift)
		{
			double barBody = Body(shift);
			double barRange = Range(shift);
			
			if(barBody <= barRange*bodyPctOfTotalBarSize)
			{
				return true;
			}
			return false;
		}
		
	/*	returns true when BullBodyEngulfing and Igniting is true */
		private bool IsBullBodyEngulfingIgniting(int shift)
		{
			if(IsBullBodyEngulfing(shift) && IsIgniting(maLength, maType, limitDistanceFromMA, shift))
			{
				return true;
			}
			return false;
		}
		
	/*	returns true when BearBodyEngulfing and Igniting is true */
		private bool IsBearBodyEngulfingIgniting(int shift)
		{
			if(IsBearBodyEngulfing(shift) && IsIgniting(maLength, maType, limitDistanceFromMA, shift))
			{
				return true;
			}
			return false;
		}
		
	/*	returns true when BullBodyEngulfing and Igniting is true */
		private bool IsBullBodyEngulfingOppositeColorIgniting(int shift)
		{
			if(IsBullBodyEngulfingOppositeColor(shift) && IsIgniting(maLength, maType, limitDistanceFromMA, shift))
			{
				return true;
			}
			return false;
		}
		
	/*	returns true when BearBodyEngulfing and Igniting is true */
		private bool IsBearBodyEngulfingOppositeColorIgniting(int shift)
		{
			if(IsBearBodyEngulfingOppositeColor(shift) && IsIgniting(maLength, maType, limitDistanceFromMA, shift))
			{
				return true;
			}
			return false;
		}
		
	/*	returns true when BullElephantBar and Igniting is true */
		private bool IsBullIgnitingElephantBar(int shift)
		{
			if(IsBullElephantBar(shift) && IsIgniting(maLength, maType, limitDistanceFromMA, shift))
			{
				return true;
			}
			return false;
		}
		
	/*	returns true when BearElephantBar and Igniting is true */
		private bool IsBearIgnitingElephantBar(int shift)
		{
			if(IsBearElephantBar(shift) && IsIgniting(maLength, maType, limitDistanceFromMA, shift))
			{
				return true;
			}
			return false;
		}
	
	/*	returns true when BullElephantBar and Exhausting is */
		private bool IsBullExhaustionElephantBar(int shift)
		{
			if(IsBullElephantBar(shift) && IsExhausting(maLength, maType, limitDistanceFromMA, shift))
			{
				return true;
			}
			return false;
		}

	/*	returns true when BearElephantBar and Exhausting is */
		private bool IsBearExhaustionElephantBar(int shift)
		{
			if(IsBearElephantBar(shift) && IsExhausting(maLength, maType, limitDistanceFromMA, shift))
			{
				return true;
			}
			return false;
		}
		
	/*	returns true when BodyEngulfing is true AND 
		entry bar close >  previous bar close AND
		isgreen is true */
		private bool IsBullBodyEngulfing(int shift)
		{
			double barSize = Range(shift);
			double topWickPct = TopWick(shift)/barSize;
			
			if(IsBodyEngulfing(prevBarMinSize, engulfingBarMinSize, atLeastXPctTheSizeOfPreviousBar, shift) && Close[shift] > Close[shift+1] && IsGreen(shift) && topWickPct <= MaxWickPct)
			{
				return true;
			}
			return false;
		}

	/* 	returns true when BodyEngulfing is true AND 
		entry bar close <  previous bar close AND
		isRred is true */

		private bool IsBearBodyEngulfing(int shift)
		{
			double barSize = Range(shift);
			double bottomWickPct = BottomWick(shift)/barSize;
			
			if(IsBodyEngulfing(prevBarMinSize, engulfingBarMinSize, atLeastXPctTheSizeOfPreviousBar, shift) && Close[shift] < Close[shift+1] && IsRed(shift) && bottomWickPct <= MaxWickPct)
			{
				return true;
			}
			return false;
		}
		
	/*	returns true is BullishBodyEngulfing AND 
		PreviousBarIsOfOppositeColor is true */
		private bool IsBullBodyEngulfingOppositeColor(int shift)
		{
			if(IsBullBodyEngulfing(shift) && PreviousBarIsOppositeColor(shift))
			{
				return true;
			}
			return false;
		}

	/* 	returns true when BearishBodyEngulfing AND 
		PreviousBarIsOfOppositeColor is true */
		private bool IsBearBodyEngulfingOppositeColor(int shift)
		{
			if(IsBearBodyEngulfing(shift) && PreviousBarIsOppositeColor(shift))
			{
				return true;
			}
			return false;
		}

	/* 	Bar with a maximum size of maxSizePips, 
		minimum size of minSizePips, with none of the wicks 
		greater than maxWickPct in relation to the body
		It doesn’t matter the colour  */
		private bool IsNarrowRangeBar(int shift)
		{
			double barSize = Range(shift);
			double topWickPct = TopWick(shift)/barSize;
			double bottomWickPct = BottomWick(shift)/barSize;
			
			if(barSize >= MinSize*TickSize && barSize <= MaxSize*TickSize && topWickPct <= MaxWickPct && bottomWickPct <= MaxWickPct)
			{
				return true;
			}
			return false;
		}
		
		private bool IsPinBarTop(int shift)
		{
			if(notPinBarIfInsideBar && IsInsideBar(prevBarMinSize, engulfingBarMinSize, atLeastXPctTheSizeOfPreviousBar, shift))
				return false;
			
			double barSize = Range(shift);
			double topWickPct = TopWick(shift)/barSize;
			double bottomWickPct = BottomWick(shift)/barSize;
			
			if(topWickPct >= longWickMinPct && bottomWickPct <= shortWickMaxPct && High[shift] > High[shift+1] && High[shift] - High[shift+1] > TopWick(shift)*longWickPctAbovePreviousHigh && Math.Max(Open[shift], Close[shift]) <= Low[shift] + barSize*openAndClosePctLocation)
			{
				return true;
			}
			
			return false;
		}
		
		private bool IsPinBarBottom(int shift)
		{
			if(notPinBarIfInsideBar && IsInsideBar(prevBarMinSize, engulfingBarMinSize, atLeastXPctTheSizeOfPreviousBar, shift))
				return false;
			
			double barSize = Range(shift);
			double topWickPct = TopWick(shift)/barSize;
			double bottomWickPct = BottomWick(shift)/barSize;
			
			if(bottomWickPct >= longWickMinPct && topWickPct <= shortWickMaxPct && Low[shift] < Low[shift+1] && Low[shift+1] - Low[shift] > BottomWick(shift)*longWickPctAbovePreviousHigh && Math.Min(Open[shift], Close[shift]) >= High[shift] - barSize*openAndClosePctLocation)
			{
				return true;
			}
			
			return false;
		}
		
		private bool IsPinBarTopInsideBarCombo(int shift)
		{
			int pinbar = -1;
			
			for(int i = 1; i < barsToLookBack; i++)
			{
				if(IsPinBarTop(i))
				{
					pinbar = i;
					break;
				}
			}
			
			if(pinbar > 0)
			{
				bool inside = true;
				for(int i = 0; i < pinbar; i++)
				{
					if(!IsInsideBar(prevBarMinSize, engulfingBarMinSize, atLeastXPctTheSizeOfPreviousBar, i))
					{
						inside = false;
						break;
					}
				}
				
				if(inside)
				{
					return true;
				}
			}
			
			return false;
		}
		
		private bool IsPinBarBottomInsideBarCombo(int shift)
		{
			int pinbar = -1;
			
			for(int i = 1; i < barsToLookBack; i++)
			{
				if(IsPinBarBottom(i))
				{
					pinbar = i;
					break;
				}
			}
			
			if(pinbar > 0)
			{
				bool inside = true;
				for(int i = 0; i < pinbar; i++)
				{
					if(!IsInsideBar(prevBarMinSize, engulfingBarMinSize, atLeastXPctTheSizeOfPreviousBar, i))
					{
						inside = false;
						break;
					}
				}
				
				if(inside)
				{
					return true;
				}
			}
			
			return false;
		}
		
		private bool IsBottomingTailBar(int shift)
		{
			return IsPinBarBottom(shift);
		}
			
		private bool IsToppingTailBar(int shift)
		{
			return IsPinBarTop(shift);
		}
		#endregion
		
		#region Setups
	/*	Returns true if the previous minBars had lower highs AND 
		(the previous minBars had lower lows OR all previous minBars return true for IsRe) AND 
		instantiated MA is rising AND 
		it has an upper wick that is not more than wickPercentage of the bar AND 
		AtOrNear MA is true.
		If ignoreIndecisionBars is true, previous bars that return true when called against IndecisionBar do not count towards minBars.  */
		private bool IsVelezBuySetup(int period, CTS_MAType type, int minBars, double wickPercentage, bool ignoreIndecisionBars, int shift)
		{
			bool condition = false;
			int bars = 0;
			
			for(int i = 0; i < CurrentBar - 1; i++)
			{
				if(IgnoreIndecisionBars && IsIndecisionBar(BodyPctOfTotalBarSize, shift))
				{
					continue;
				}
				
				if(High[i] < High[i+1] && (Low[i] < Low[i+1] || IsRed(i)))
				{
					bars++;
					if(bars >= minBars)
					{
						condition = true;
						break;
					}
				}
				else
				{
					break;
				}
			}
			
			double maThisBar = MA(period, type, shift);
			double maPrevBar = MA(period, type, shift+1);
			double upperWick = TopWick(shift);
			
			if(condition && maThisBar > maPrevBar && upperWick <= Range(shift)*wickPercentage)
			{
				return true;
			}
			
			return false;
		}
		
	/*	Returns true if the previous minBars had higher highs AND 
		(the previous minBars had higher lows OR all previous minBars return true for IsGreen) AND 
		instantiated MA is falling AND 
		entry candle has a lower wick that is not more than wickPercentage of the bar AND 
		AtOrNear MA is true. 
		If ignoreIndecisionBars is true, previous bars that return true when called against IndecisionBar do not count towards minBars. */
		private bool IsVelezSellSetup(int period, CTS_MAType type, int minBars, double wickPercentage, bool ignoreIndecisionBars, int shift)
		{
			bool condition = false;
			int bars = 0;
			
			for(int i = 0; i < CurrentBar - 1; i++)
			{
				if(IgnoreIndecisionBars && IsIndecisionBar(BodyPctOfTotalBarSize, shift))
				{
					continue;
				}
				
				if(Low[i] > Low[i+1] && (High[i] > High[i+1] || IsGreen(i)))
				{
					bars++;
					if(bars >= minBars)
					{
						condition = true;
						break;
					}
				}
				else
				{
					break;
				}
			}
			
			double maThisBar = MA(period, type, shift);
			double maPrevBar = MA(period, type, shift+1);
			double lowerWick = BottomWick(shift);
			
			if(condition && maThisBar < maPrevBar && lowerWick <= Range(shift)*wickPercentage)
			{
				return true;
			}
			
			return false;
		}
		
	/*	Returns true if the entry bar is a BottomingTail AND 
		if the previous minBars had lower highs AND 
		(the previous minBars had lower lows OR all previous minBars return true for IsRe) AND 
		instantiated MA is rising AND 
		AtOrNear MA is true.
		If ignoreIndecisionBars is true, previous bars that return true when called against IndecisionBar do not count towards minBars.  */
		private bool IsBottomingTailSetup(int period, CTS_MAType type, int minBars, bool ignoreIndecisionBars, int shift)
		{
			bool condition = false;
			int bars = 0;
			
			for(int i = 0; i < CurrentBar - 1; i++)
			{
				if(IgnoreIndecisionBars && IsIndecisionBar(BodyPctOfTotalBarSize, shift))
				{
					continue;
				}
				
				if(High[i] < High[i+1] && (Low[i] < Low[i+1] || IsRed(i)))
				{
					bars++;
					if(bars >= minBars)
					{
						condition = true;
						break;
					}
				}
				else
				{
					break;
				}
			}
			
			double maThisBar = MA(period, type, shift);
			double maPrevBar = MA(period, type, shift+1);
			
			if(IsBottomingTailBar(shift) && condition && maThisBar > maPrevBar)
			{
				return true;
			}
			
			return false;
		}
		
	/*	Returns true if the entry bar is a ToppingTail AND 
		the previous minBars had higher highs AND 
		(the previous minBars had higher lows OR all previous minBars return true for IsGreen) AND 
		instantiated MA is falling AND 
		AtOrNear MA is true. 
		If ignoreIndecisionBars is true, previous bars that return true when called against IndecisionBar do not count towards minBars */
		private bool IsToppingTailSetup(int period, CTS_MAType type, int minBars, bool ignoreIndecisionBars, int shift)
		{
			bool condition = false;
			int bars = 0;
			
			for(int i = 0; i < CurrentBar - 1; i++)
			{
				if(IgnoreIndecisionBars && IsIndecisionBar(BodyPctOfTotalBarSize, shift))
				{
					continue;
				}
				
				if(Low[i] > Low[i+1] && (High[i] > High[i+1] || IsGreen(i)))
				{
					bars++;
					if(bars >= minBars)
					{
						condition = true;
						break;
					}
				}
				else
				{
					break;
				}
			}
			
			double maThisBar = MA(period, type, shift);
			double maPrevBar = MA(period, type, shift+1);
			
			if(IsToppingTailBar(shift) && condition && maThisBar < maPrevBar)
			{
				return true;
			}
			
			return false;
		}
		#endregion
		
		#region Location
		private bool CheckLocation()
		{
			if(Location == CTS_Location.AtOrNearMA)
			{
				return LocationAtOrNearMA(LocMALength, LocMAType, LocPipDistance);
			}
			else if(Location == CTS_Location.AboveMA)
			{
				return LocationAboveMA(LocMALength, LocMAType, LocPipDistance);
			}
			else if(Location == CTS_Location.BelowMA)
			{
				return LocationBelowMA(LocMALength, LocMAType, LocPipDistance);
			}
			else if(Location == CTS_Location.BreakingUpperBollingerBand)
			{
				return LocationBreakingUpperBollingerBand(LocBBBodyPct, LocBBDeviation, LocMALength);
			}
			else if(Location == CTS_Location.BreakingLowerBollingerBand)
			{
				return LocationBreakingLowerBollingerBand(LocBBBodyPct, LocBBDeviation, LocMALength);
			}
			
			return true;
		}
		
		private bool LocationAtOrNearMA(int period, CTS_MAType type, int distanceForNear)
		{
			double dMA = MA(period, type, 0);
			if(High[0] >= dMA - distanceForNear*TickSize && Low[0] <= dMA + distanceForNear*TickSize)
			{
				return true;
			}
			else
			{
				return false;
			}
			
		}
		
		private bool LocationAboveMA(int period, CTS_MAType type, int minDistance)
		{
			double dMA = MA(period, type, 0);
			if(Low[0] >= dMA + minDistance*TickSize)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		
		private bool LocationBelowMA(int period, CTS_MAType type, int minDistance)
		{
			double dMA = MA(period, type, 0);
			if(High[0] <= dMA - minDistance*TickSize)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		
		private bool LocationBreakingUpperBollingerBand(double pct, double stdDeviation, int period)
		{
			double dBB = Bollinger(stdDeviation, period).Upper[0];
			if(High[0] >= dBB + Range(0)*pct)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		
		private bool LocationBreakingLowerBollingerBand(double pct, double stdDeviation, int period)
		{
			double dBB = Bollinger(stdDeviation, period).Lower[0];
			if(Low[0] <= dBB - Range(0)*pct)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		#endregion


        #region Properties
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public DataSeries Pattern
        {
            get { return Values[0]; }
        }

		 #region Tradable Event
		
		[Description("")]
        [GridCategory("01. Tradable Event")]
		[RefreshProperties(RefreshProperties.All)]
		[Gui.Design.DisplayName(" TradableEvent")]
        public NinjaTrader.Indicator.CTS_TradableEvent TradableEvent
        {
            get { return tradableEvent; }
            set { tradableEvent = value; }
        }

        [Description("")]
        [GridCategory("01. Tradable Event")]
        public int PrevBarMinSize
        {
            get { return prevBarMinSize; }
            set { prevBarMinSize = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("01. Tradable Event")]
        public int EngulfingBarMinSize
        {
            get { return engulfingBarMinSize; }
            set { engulfingBarMinSize = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("01. Tradable Event")]
		[Gui.Design.DisplayName("XTimesTheSize")]
        public double PipBodySize
        {
            get { return pipBodySize; }
            set { pipBodySize = Math.Max(1.0, value); }
        }

        [Description("")]
        [GridCategory("01. Tradable Event")]
        public double WickPercentage
        {
            get { return wickPercentage; }
            set { wickPercentage = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("01. Tradable Event")]
        public int NumPreviousBars
        {
            get { return numPreviousBars; }
            set { numPreviousBars = Math.Max(1, value); }
        }

        [Description("")]
        [GridCategory("01. Tradable Event")]
        public int MALength
        {
            get { return maLength; }
            set { maLength = Math.Max(1, value); }
        }

        [Description("")]
        [GridCategory("01. Tradable Event")]
        public NinjaTrader.Indicator.CTS_MAType MAType
        {
            get { return maType; }
            set { maType = value; }
        }

        [Description("")]
        [GridCategory("01. Tradable Event")]
        public int LimitDistanceFromMA
        {
            get { return limitDistanceFromMA; }
            set { limitDistanceFromMA = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("01. Tradable Event")]
        public int PipMaxBodySize
        {
            get { return pipMaxBodySize; }
            set { pipMaxBodySize = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("01. Tradable Event")]
        public double BodyPctOfTotalBarSize
        {
            get { return bodyPctOfTotalBarSize; }
            set { bodyPctOfTotalBarSize = Math.Max(0, Math.Min(1, value)); }
        }

        [Description("")]
        [GridCategory("01. Tradable Event")]
        public int MinSize
        {
            get { return minSize; }
            set { minSize = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("01. Tradable Event")]
        public int MaxSize
        {
            get { return maxSize; }
            set { maxSize = Math.Max(1, value); }
        }

        [Description("")]
        [GridCategory("01. Tradable Event")]
        public bool IgnoreIndecisionBars
        {
            get { return ignoreIndecisionBars; }
            set { ignoreIndecisionBars = value; }
        }

        [Description("")]
        [GridCategory("01. Tradable Event")]
        public double MaxWickPct
        {
            get { return maxWickPct; }
            set { maxWickPct = Math.Max(0, Math.Min(1, value)); }
        }
		
        [Description("")]
        [GridCategory("01. Tradable Event")]
		public NinjaTrader.Indicator.CTS_BodyOrTotalSize CompareBodyOrTotalSize
        {
            get { return compareBodyOrTotalSize; }
            set { compareBodyOrTotalSize = value; }
        }

        [Description("")]
        [GridCategory("01. Tradable Event")]
        public double AtLeastXPctTheSizeOfPreviousBar
        {
            get { return atLeastXPctTheSizeOfPreviousBar; }
            set { atLeastXPctTheSizeOfPreviousBar = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("01. Tradable Event")]
        public double LongWickMinPct
        {
            get { return longWickMinPct; }
            set { longWickMinPct = Math.Max(0, Math.Min(1, value)); }
        }

        [Description("")]
        [GridCategory("01. Tradable Event")]
        public double ShortWickMaxPct
        {
            get { return shortWickMaxPct; }
            set { shortWickMaxPct = Math.Max(0, Math.Min(1, value)); }
        }

        [Description("")]
        [GridCategory("01. Tradable Event")]
        public double LongWickPctAbovePreviousHigh
        {
            get { return longWickPctAbovePreviousHigh; }
            set { longWickPctAbovePreviousHigh = Math.Max(0, Math.Min(1, value)); }
        }

        [Description("")]
        [GridCategory("01. Tradable Event")]
        public double OpenAndClosePctLocation
        {
            get { return openAndClosePctLocation; }
            set { openAndClosePctLocation = Math.Max(0, Math.Min(1, value)); }
        }

        [Description("")]
        [GridCategory("01. Tradable Event")]
        public bool NotPinBarIfInsideBar
        {
            get { return notPinBarIfInsideBar; }
            set { notPinBarIfInsideBar = value; }
        }

        [Description("")]
        [GridCategory("01. Tradable Event")]
        public int BarsToLookBack
        {
            get { return barsToLookBack; }
            set { barsToLookBack = Math.Max(1, value); }
        }
		 #endregion
		
		 #region Location

        [Description("")]
        [GridCategory("02. Location")]
		[RefreshProperties(RefreshProperties.All)]
		[Gui.Design.DisplayName(" Location")]
        public NinjaTrader.Indicator.CTS_Location Location
        {
            get { return location; }
            set { location = value; }
        }

        [Description("")]
        [GridCategory("02. Location")]
		[Gui.Design.DisplayName("MA Length")]
        public int LocMALength
        {
            get { return loc_MALength; }
            set { loc_MALength = Math.Max(1, value); }
        }

        [Description("")]
        [GridCategory("02. Location")]
		[Gui.Design.DisplayName("MA Type")]
        public NinjaTrader.Indicator.CTS_MAType LocMAType
        {
            get { return loc_MAType; }
            set { loc_MAType = value; }
        }

        [Description("")]
        [GridCategory("02. Location")]
		[Gui.Design.DisplayName("Pip Distance")]
        public int LocPipDistance
        {
            get { return loc_PipDistance; }
            set { loc_PipDistance = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("02. Location")]
		[Gui.Design.DisplayName("BB Deviation")]
        public double LocBBDeviation
        {
            get { return loc_BBDeviation; }
            set { loc_BBDeviation = Math.Max(0.1, value); }
        }

        [Description("")]
        [GridCategory("02. Location")]
		[Gui.Design.DisplayName("BB Body Percent")]
        public double LocBBBodyPct
        {
            get { return loc_BBBodyPct; }
            set { loc_BBBodyPct = Math.Max(0, value); }
        }
		 #endregion
        #endregion
		
		#region Custom Property Manipulation
		
		private void ModifyProperties(PropertyDescriptorCollection col)
		{
			switch(TradableEvent)
			{
				case CTS_TradableEvent.AnyBar:
					col.Remove(col.Find("PrevBarMinSize", true));
					col.Remove(col.Find("EngulfingBarMinSize", true));
					col.Remove(col.Find("PipBodySize", true));
					col.Remove(col.Find("WickPercentage", true));
					col.Remove(col.Find("NumPreviousBars", true));
					col.Remove(col.Find("MALength", true));
					col.Remove(col.Find("MAType", true));
					col.Remove(col.Find("LimitDistanceFromMA", true));
					col.Remove(col.Find("PipMaxBodySize", true));
					col.Remove(col.Find("BodyPctOfTotalBarSize", true));
					col.Remove(col.Find("MinSize", true));
					col.Remove(col.Find("MaxSize", true));
					col.Remove(col.Find("MaxWickPct", true));
					col.Remove(col.Find("IgnoreIndecisionBars", true));
					col.Remove(col.Find("CompareBodyOrTotalSize", true));
					col.Remove(col.Find("AtLeastXPctTheSizeOfPreviousBar", true));
					col.Remove(col.Find("LongWickMinPct", true));
					col.Remove(col.Find("ShortWickMaxPct", true));
					col.Remove(col.Find("LongWickPctAbovePreviousHigh", true));
					col.Remove(col.Find("OpenAndClosePctLocation", true));
					col.Remove(col.Find("NotPinBarIfInsideBar", true));
					col.Remove(col.Find("BarsTolookBack", true));
					break;
					
				case CTS_TradableEvent.BullIgnitingElephantBar:
				case CTS_TradableEvent.BearIgnitingElephantBar:
					col.Remove(col.Find("PrevBarMinSize", true));
					col.Remove(col.Find("EngulfingBarMinSize", true));
					col.Remove(col.Find("PipMaxBodySize", true));
					col.Remove(col.Find("BodyPctOfTotalBarSize", true));
					col.Remove(col.Find("MinSize", true));
					col.Remove(col.Find("MaxSize", true));
					col.Remove(col.Find("MaxWickPct", true));
					col.Remove(col.Find("IgnoreIndecisionBars", true));
					col.Remove(col.Find("AtLeastXPctTheSizeOfPreviousBar", true));
					col.Remove(col.Find("LongWickMinPct", true));
					col.Remove(col.Find("ShortWickMaxPct", true));
					col.Remove(col.Find("LongWickPctAbovePreviousHigh", true));
					col.Remove(col.Find("OpenAndClosePctLocation", true));
					col.Remove(col.Find("NotPinBarIfInsideBar", true));
					col.Remove(col.Find("BarsTolookBack", true));
					break;
					
				case CTS_TradableEvent.BullExhaustionElephantBar:
				case CTS_TradableEvent.BearExhaustionElephantBar:
					col.Remove(col.Find("PrevBarMinSize", true));
					col.Remove(col.Find("EngulfingBarMinSize", true));
					col.Remove(col.Find("PipMaxBodySize", true));
					col.Remove(col.Find("BodyPctOfTotalBarSize", true));
					col.Remove(col.Find("MinSize", true));
					col.Remove(col.Find("MaxSize", true));
					col.Remove(col.Find("MaxWickPct", true));
					col.Remove(col.Find("IgnoreIndecisionBars", true));
					col.Remove(col.Find("AtLeastXPctTheSizeOfPreviousBar", true));
					col.Remove(col.Find("LongWickMinPct", true));
					col.Remove(col.Find("ShortWickMaxPct", true));
					col.Remove(col.Find("LongWickPctAbovePreviousHigh", true));
					col.Remove(col.Find("OpenAndClosePctLocation", true));
					col.Remove(col.Find("NotPinBarIfInsideBar", true));
					col.Remove(col.Find("BarsTolookBack", true));
					break;
					
				case CTS_TradableEvent.BullBodyEngulfing:
				case CTS_TradableEvent.BearBodyEngulfing:
				case CTS_TradableEvent.BullBodyEngulfingOppositeColor:
				case CTS_TradableEvent.BearBodyEngulfingOppositeColor:
				case CTS_TradableEvent.BodyEngulfing:
				case CTS_TradableEvent.InsideBar:
					col.Remove(col.Find("PipBodySize", true));
					col.Remove(col.Find("WickPercentage", true));
					col.Remove(col.Find("NumPreviousBars", true));
					col.Remove(col.Find("MALength", true));
					col.Remove(col.Find("MAType", true));
					col.Remove(col.Find("LimitDistanceFromMA", true));
					col.Remove(col.Find("PipMaxBodySize", true));
					col.Remove(col.Find("BodyPctOfTotalBarSize", true));
					col.Remove(col.Find("MinSize", true));
					col.Remove(col.Find("MaxSize", true));
					col.Remove(col.Find("IgnoreIndecisionBars", true));
					col.Remove(col.Find("CompareBodyOrTotalSize", true));
					col.Remove(col.Find("LongWickMinPct", true));
					col.Remove(col.Find("ShortWickMaxPct", true));
					col.Remove(col.Find("LongWickPctAbovePreviousHigh", true));
					col.Remove(col.Find("OpenAndClosePctLocation", true));
					col.Remove(col.Find("NotPinBarIfInsideBar", true));
					col.Remove(col.Find("BarsTolookBack", true));
					break;
					
				case CTS_TradableEvent.BullBodyEngulfingIgniting:
				case CTS_TradableEvent.BearBodyEngulfingIgniting:
				case CTS_TradableEvent.BullBodyEngulfingOppositeColorIgniting:
				case CTS_TradableEvent.BearBodyEngulfingOppositeColorIgniting:
					col.Remove(col.Find("PipBodySize", true));
					col.Remove(col.Find("WickPercentage", true));
					col.Remove(col.Find("NumPreviousBars", true));
					col.Remove(col.Find("PipMaxBodySize", true));
					col.Remove(col.Find("BodyPctOfTotalBarSize", true));
					col.Remove(col.Find("MinSize", true));
					col.Remove(col.Find("MaxSize", true));
					col.Remove(col.Find("IgnoreIndecisionBars", true));
					col.Remove(col.Find("CompareBodyOrTotalSize", true));
					col.Remove(col.Find("LongWickMinPct", true));
					col.Remove(col.Find("ShortWickMaxPct", true));
					col.Remove(col.Find("LongWickPctAbovePreviousHigh", true));
					col.Remove(col.Find("OpenAndClosePctLocation", true));
					col.Remove(col.Find("NotPinBarIfInsideBar", true));
					col.Remove(col.Find("BarsTolookBack", true));
					break;
					
				case CTS_TradableEvent.BullElephantBar:
				case CTS_TradableEvent.BearElephantBar:
					col.Remove(col.Find("PrevBarMinSize", true));
					col.Remove(col.Find("EngulfingBarMinSize", true));
					col.Remove(col.Find("MALength", true));
					col.Remove(col.Find("MAType", true));
					col.Remove(col.Find("LimitDistanceFromMA", true));
					col.Remove(col.Find("PipMaxBodySize", true));
					col.Remove(col.Find("BodyPctOfTotalBarSize", true));
					col.Remove(col.Find("MinSize", true));
					col.Remove(col.Find("MaxSize", true));
					col.Remove(col.Find("MaxWickPct", true));
					col.Remove(col.Find("IgnoreIndecisionBars", true));
					col.Remove(col.Find("AtLeastXPctTheSizeOfPreviousBar", true));
					col.Remove(col.Find("LongWickMinPct", true));
					col.Remove(col.Find("ShortWickMaxPct", true));
					col.Remove(col.Find("LongWickPctAbovePreviousHigh", true));
					col.Remove(col.Find("OpenAndClosePctLocation", true));
					col.Remove(col.Find("NotPinBarIfInsideBar", true));
					col.Remove(col.Find("BarsTolookBack", true));
					break;
					
				case CTS_TradableEvent.Igniting:
					col.Remove(col.Find("PrevBarMinSize", true));
					col.Remove(col.Find("EngulfingBarMinSize", true));
					col.Remove(col.Find("PipBodySize", true));
					col.Remove(col.Find("WickPercentage", true));
					col.Remove(col.Find("NumPreviousBars", true));
					col.Remove(col.Find("PipMaxBodySize", true));
					col.Remove(col.Find("BodyPctOfTotalBarSize", true));
					col.Remove(col.Find("MinSize", true));
					col.Remove(col.Find("MaxSize", true));
					col.Remove(col.Find("MaxWickPct", true));
					col.Remove(col.Find("IgnoreIndecisionBars", true));
					col.Remove(col.Find("CompareBodyOrTotalSize", true));
					col.Remove(col.Find("AtLeastXPctTheSizeOfPreviousBar", true));
					col.Remove(col.Find("LongWickMinPct", true));
					col.Remove(col.Find("ShortWickMaxPct", true));
					col.Remove(col.Find("LongWickPctAbovePreviousHigh", true));
					col.Remove(col.Find("OpenAndClosePctLocation", true));
					col.Remove(col.Find("NotPinBarIfInsideBar", true));
					col.Remove(col.Find("BarsTolookBack", true));
					break;
					
				case CTS_TradableEvent.Exhausting:
					col.Remove(col.Find("PrevBarMinSize", true));
					col.Remove(col.Find("EngulfingBarMinSize", true));
					col.Remove(col.Find("PipBodySize", true));
					col.Remove(col.Find("WickPercentage", true));
					col.Remove(col.Find("NumPreviousBars", true));
					col.Remove(col.Find("PipMaxBodySize", true));
					col.Remove(col.Find("BodyPctOfTotalBarSize", true));
					col.Remove(col.Find("MinSize", true));
					col.Remove(col.Find("MaxSize", true));
					col.Remove(col.Find("MaxWickPct", true));
					col.Remove(col.Find("IgnoreIndecisionBars", true));
					col.Remove(col.Find("CompareBodyOrTotalSize", true));
					col.Remove(col.Find("AtLeastXPctTheSizeOfPreviousBar", true));
					col.Remove(col.Find("LongWickMinPct", true));
					col.Remove(col.Find("ShortWickMaxPct", true));
					col.Remove(col.Find("LongWickPctAbovePreviousHigh", true));
					col.Remove(col.Find("OpenAndClosePctLocation", true));
					col.Remove(col.Find("NotPinBarIfInsideBar", true));
					col.Remove(col.Find("BarsTolookBack", true));
					break;
					
				case CTS_TradableEvent.IndecisionBar:
					col.Remove(col.Find("PrevBarMinSize", true));
					col.Remove(col.Find("EngulfingBarMinSize", true));
					col.Remove(col.Find("PipBodySize", true));
					col.Remove(col.Find("WickPercentage", true));
					col.Remove(col.Find("NumPreviousBars", true));
					col.Remove(col.Find("MALength", true));
					col.Remove(col.Find("MAType", true));
					col.Remove(col.Find("LimitDistanceFromMA", true));
					col.Remove(col.Find("MinSize", true));
					col.Remove(col.Find("MaxSize", true));
					col.Remove(col.Find("MaxWickPct", true));
					col.Remove(col.Find("PipMaxBodySize", true));
					col.Remove(col.Find("IgnoreIndecisionBars", true));
					col.Remove(col.Find("CompareBodyOrTotalSize", true));
					col.Remove(col.Find("AtLeastXPctTheSizeOfPreviousBar", true));
					col.Remove(col.Find("LongWickMinPct", true));
					col.Remove(col.Find("ShortWickMaxPct", true));
					col.Remove(col.Find("LongWickPctAbovePreviousHigh", true));
					col.Remove(col.Find("OpenAndClosePctLocation", true));
					col.Remove(col.Find("NotPinBarIfInsideBar", true));
					col.Remove(col.Find("BarsTolookBack", true));
					break;
					
				case CTS_TradableEvent.NarrowRangeBar:
					col.Remove(col.Find("PrevBarMinSize", true));
					col.Remove(col.Find("EngulfingBarMinSize", true));
					col.Remove(col.Find("PipBodySize", true));
					col.Remove(col.Find("WickPercentage", true));
					col.Remove(col.Find("NumPreviousBars", true));
					col.Remove(col.Find("MALength", true));
					col.Remove(col.Find("MAType", true));
					col.Remove(col.Find("LimitDistanceFromMA", true));
					col.Remove(col.Find("PipMaxBodySize", true));
					col.Remove(col.Find("BodyPctOfTotalBarSize", true));
					col.Remove(col.Find("IgnoreIndecisionBars", true));
					col.Remove(col.Find("CompareBodyOrTotalSize", true));
					col.Remove(col.Find("AtLeastXPctTheSizeOfPreviousBar", true));
					col.Remove(col.Find("LongWickMinPct", true));
					col.Remove(col.Find("ShortWickMaxPct", true));
					col.Remove(col.Find("LongWickPctAbovePreviousHigh", true));
					col.Remove(col.Find("OpenAndClosePctLocation", true));
					col.Remove(col.Find("NotPinBarIfInsideBar", true));
					col.Remove(col.Find("BarsTolookBack", true));
					break;
					
				case CTS_TradableEvent.VelezBuySetup:
				case CTS_TradableEvent.VelezSellSetup:
					col.Remove(col.Find("PrevBarMinSize", true));
					col.Remove(col.Find("EngulfingBarMinSize", true));
					col.Remove(col.Find("PipBodySize", true));
					col.Remove(col.Find("LimitDistanceFromMA", true));
					col.Remove(col.Find("PipMaxBodySize", true));
					col.Remove(col.Find("BodyPctOfTotalBarSize", true));
					col.Remove(col.Find("MinSize", true));
					col.Remove(col.Find("MaxSize", true));
					col.Remove(col.Find("MaxWickPct", true));
					col.Remove(col.Find("CompareBodyOrTotalSize", true));
					col.Remove(col.Find("AtLeastXPctTheSizeOfPreviousBar", true));
					col.Remove(col.Find("LongWickMinPct", true));
					col.Remove(col.Find("ShortWickMaxPct", true));
					col.Remove(col.Find("LongWickPctAbovePreviousHigh", true));
					col.Remove(col.Find("OpenAndClosePctLocation", true));
					col.Remove(col.Find("NotPinBarIfInsideBar", true));
					col.Remove(col.Find("BarsTolookBack", true));
					break;
					
				case CTS_TradableEvent.PinBarTop:
				case CTS_TradableEvent.PinBarBottom:
					col.Remove(col.Find("PrevBarMinSize", true));
					col.Remove(col.Find("EngulfingBarMinSize", true));
					col.Remove(col.Find("PipBodySize", true));
					col.Remove(col.Find("WickPercentage", true));
					col.Remove(col.Find("NumPreviousBars", true));
					col.Remove(col.Find("MALength", true));
					col.Remove(col.Find("MAType", true));
					col.Remove(col.Find("LimitDistanceFromMA", true));
					col.Remove(col.Find("PipMaxBodySize", true));
					col.Remove(col.Find("BodyPctOfTotalBarSize", true));
					col.Remove(col.Find("MinSize", true));
					col.Remove(col.Find("MaxSize", true));
					col.Remove(col.Find("MaxWickPct", true));
					col.Remove(col.Find("IgnoreIndecisionBars", true));
					col.Remove(col.Find("CompareBodyOrTotalSize", true));
					col.Remove(col.Find("AtLeastXPctTheSizeOfPreviousBar", true));
					col.Remove(col.Find("BarsTolookBack", true));
					break;
					
				case CTS_TradableEvent.PinBarTopInsideBarCombo:
				case CTS_TradableEvent.PinBarBottomInsideBarCombo:
					col.Remove(col.Find("PrevBarMinSize", true));
					col.Remove(col.Find("EngulfingBarMinSize", true));
					col.Remove(col.Find("PipBodySize", true));
					col.Remove(col.Find("WickPercentage", true));
					col.Remove(col.Find("NumPreviousBars", true));
					col.Remove(col.Find("MALength", true));
					col.Remove(col.Find("MAType", true));
					col.Remove(col.Find("LimitDistanceFromMA", true));
					col.Remove(col.Find("PipMaxBodySize", true));
					col.Remove(col.Find("BodyPctOfTotalBarSize", true));
					col.Remove(col.Find("MinSize", true));
					col.Remove(col.Find("MaxSize", true));
					col.Remove(col.Find("MaxWickPct", true));
					col.Remove(col.Find("IgnoreIndecisionBars", true));
					col.Remove(col.Find("CompareBodyOrTotalSize", true));
					col.Remove(col.Find("AtLeastXPctTheSizeOfPreviousBar", true));
					break;
					
				default:
					col.Remove(col.Find("PrevBarMinSize", true));
					col.Remove(col.Find("EngulfingBarMinSize", true));
					col.Remove(col.Find("PipBodySize", true));
					col.Remove(col.Find("WickPercentage", true));
					col.Remove(col.Find("NumPreviousBars", true));
					col.Remove(col.Find("MALength", true));
					col.Remove(col.Find("MAType", true));
					col.Remove(col.Find("LimitDistanceFromMA", true));
					col.Remove(col.Find("PipMaxBodySize", true));
					col.Remove(col.Find("BodyPctOfTotalBarSize", true));
					col.Remove(col.Find("MinSize", true));
					col.Remove(col.Find("MaxSize", true));
					col.Remove(col.Find("MaxWickPct", true));
					col.Remove(col.Find("IgnoreIndecisionBars", true));
					col.Remove(col.Find("CompareBodyOrTotalSize", true));
					col.Remove(col.Find("AtLeastXPctTheSizeOfPreviousBar", true));
					col.Remove(col.Find("LongWickMinPct", true));
					col.Remove(col.Find("ShortWickMaxPct", true));
					col.Remove(col.Find("LongWickPctAbovePreviousHigh", true));
					col.Remove(col.Find("OpenAndClosePctLocation", true));
					col.Remove(col.Find("NotPinBarIfInsideBar", true));
					col.Remove(col.Find("BarsTolookBack", true));
					break;
			}
			
			switch(Location)
			{
				case CTS_Location.Anywhere:
					col.Remove(col.Find("LocMALength", true));
					col.Remove(col.Find("LocMAType", true));
					col.Remove(col.Find("LocPipDistance", true));
					col.Remove(col.Find("LocBBDeviation", true));
					col.Remove(col.Find("LocBBBodyPct", true));
					break;

				case CTS_Location.AtOrNearMA:
				case CTS_Location.AboveMA:
				case CTS_Location.BelowMA:
					col.Remove(col.Find("LocBBDeviation", true));
					col.Remove(col.Find("LocBBBodyPct", true));
					break;

				case CTS_Location.BreakingLowerBollingerBand:
				case CTS_Location.BreakingUpperBollingerBand:
					col.Remove(col.Find("LocMAType", true));
					col.Remove(col.Find("LocPipDistance", true));
					break;
					
				default:
					col.Remove(col.Find("LocMALength", true));
					col.Remove(col.Find("LocMAType", true));
					col.Remove(col.Find("LocPipDistance", true));
					col.Remove(col.Find("LocBBDeviation", true));
					col.Remove(col.Find("LocBBBodyPct", true));
					break;
			}
		}
		
		#endregion
		
        #region ICustomTypeDescriptor Members

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(GetType());
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(GetType());
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(GetType());
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(GetType());
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(GetType());
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(GetType());
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(GetType(), editorBaseType);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(GetType(), attributes);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(GetType());
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection orig = TypeDescriptor.GetProperties(GetType(), attributes);
            PropertyDescriptor[] arr = new PropertyDescriptor[orig.Count];
            orig.CopyTo(arr, 0);
            PropertyDescriptorCollection col = new PropertyDescriptorCollection(arr);

            ModifyProperties(col);
            return col;
        
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return TypeDescriptor.GetProperties(GetType());
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        #endregion
    }
	
	public enum CTS_TradableEvent
	{
		AlertBar,
		AnyBar,
		BearBodyEngulfing,
		BearBodyEngulfingIgniting,
		BearBodyEngulfingOppositeColor,
		BearBodyEngulfingOppositeColorIgniting,
		BearElephantBar,
		BearExhaustionElephantBar,
		BearIgnitingElephantBar,
		BodyEngulfing,
		BottomingTailSetup,
		BullBodyEngulfing,
		BullBodyEngulfingIgniting,
		BullBodyEngulfingOppositeColor,
		BullBodyEngulfingOppositeColorIgniting,
		BullElephantBar,
		BullExhaustionElephantBar,
		BullIgnitingElephantBar,
		DarkCloudCover,
		Doji,
		Exhausting,
		Hammer,
		Igniting,
		IndecisionBar,
		InsideBar,
		NarrowRangeBar,
		PiercingLine,
		PinBarTop,
		PinBarBottom,
		PinBarTopInsideBarCombo,
		PinBarBottomInsideBarCombo,
		ToppingTailSetup,
		VelezBuySetup,
		VelezSellSetup
	}
	
	public enum CTS_Location
	{
		Anywhere,
		AtOrNearMA,
		BelowMA,
		AboveMA,
		BreakingUpperBollingerBand,
		BreakingLowerBollingerBand
	}
	
	public enum CTS_MAType
	{
		Simple,
		Exponential,
		DoubleExponential,
		Hull
	}
	
	public enum CTS_BodyOrTotalSize
	{
		Body,
		TotalSize
	}
}

#region NinjaScript generated code. Neither change nor remove.
// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    public partial class Indicator : IndicatorBase
    {
        private CrespoCandleLocation[] cacheCrespoCandleLocation = null;

        private static CrespoCandleLocation checkCrespoCandleLocation = new CrespoCandleLocation();

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public CrespoCandleLocation CrespoCandleLocation(double atLeastXPctTheSizeOfPreviousBar, int barsToLookBack, double bodyPctOfTotalBarSize, NinjaTrader.Indicator.CTS_BodyOrTotalSize compareBodyOrTotalSize, int engulfingBarMinSize, bool ignoreIndecisionBars, int limitDistanceFromMA, NinjaTrader.Indicator.CTS_Location location, double locBBBodyPct, double locBBDeviation, int locMALength, NinjaTrader.Indicator.CTS_MAType locMAType, int locPipDistance, double longWickMinPct, double longWickPctAbovePreviousHigh, int mALength, NinjaTrader.Indicator.CTS_MAType mAType, int maxSize, double maxWickPct, int minSize, bool notPinBarIfInsideBar, int numPreviousBars, double openAndClosePctLocation, double pipBodySize, int pipMaxBodySize, int prevBarMinSize, double shortWickMaxPct, NinjaTrader.Indicator.CTS_TradableEvent tradableEvent, double wickPercentage)
        {
            return CrespoCandleLocation(Input, atLeastXPctTheSizeOfPreviousBar, barsToLookBack, bodyPctOfTotalBarSize, compareBodyOrTotalSize, engulfingBarMinSize, ignoreIndecisionBars, limitDistanceFromMA, location, locBBBodyPct, locBBDeviation, locMALength, locMAType, locPipDistance, longWickMinPct, longWickPctAbovePreviousHigh, mALength, mAType, maxSize, maxWickPct, minSize, notPinBarIfInsideBar, numPreviousBars, openAndClosePctLocation, pipBodySize, pipMaxBodySize, prevBarMinSize, shortWickMaxPct, tradableEvent, wickPercentage);
        }

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public CrespoCandleLocation CrespoCandleLocation(Data.IDataSeries input, double atLeastXPctTheSizeOfPreviousBar, int barsToLookBack, double bodyPctOfTotalBarSize, NinjaTrader.Indicator.CTS_BodyOrTotalSize compareBodyOrTotalSize, int engulfingBarMinSize, bool ignoreIndecisionBars, int limitDistanceFromMA, NinjaTrader.Indicator.CTS_Location location, double locBBBodyPct, double locBBDeviation, int locMALength, NinjaTrader.Indicator.CTS_MAType locMAType, int locPipDistance, double longWickMinPct, double longWickPctAbovePreviousHigh, int mALength, NinjaTrader.Indicator.CTS_MAType mAType, int maxSize, double maxWickPct, int minSize, bool notPinBarIfInsideBar, int numPreviousBars, double openAndClosePctLocation, double pipBodySize, int pipMaxBodySize, int prevBarMinSize, double shortWickMaxPct, NinjaTrader.Indicator.CTS_TradableEvent tradableEvent, double wickPercentage)
        {
            if (cacheCrespoCandleLocation != null)
                for (int idx = 0; idx < cacheCrespoCandleLocation.Length; idx++)
                    if (Math.Abs(cacheCrespoCandleLocation[idx].AtLeastXPctTheSizeOfPreviousBar - atLeastXPctTheSizeOfPreviousBar) <= double.Epsilon && cacheCrespoCandleLocation[idx].BarsToLookBack == barsToLookBack && Math.Abs(cacheCrespoCandleLocation[idx].BodyPctOfTotalBarSize - bodyPctOfTotalBarSize) <= double.Epsilon && cacheCrespoCandleLocation[idx].CompareBodyOrTotalSize == compareBodyOrTotalSize && cacheCrespoCandleLocation[idx].EngulfingBarMinSize == engulfingBarMinSize && cacheCrespoCandleLocation[idx].IgnoreIndecisionBars == ignoreIndecisionBars && cacheCrespoCandleLocation[idx].LimitDistanceFromMA == limitDistanceFromMA && cacheCrespoCandleLocation[idx].Location == location && Math.Abs(cacheCrespoCandleLocation[idx].LocBBBodyPct - locBBBodyPct) <= double.Epsilon && Math.Abs(cacheCrespoCandleLocation[idx].LocBBDeviation - locBBDeviation) <= double.Epsilon && cacheCrespoCandleLocation[idx].LocMALength == locMALength && cacheCrespoCandleLocation[idx].LocMAType == locMAType && cacheCrespoCandleLocation[idx].LocPipDistance == locPipDistance && Math.Abs(cacheCrespoCandleLocation[idx].LongWickMinPct - longWickMinPct) <= double.Epsilon && Math.Abs(cacheCrespoCandleLocation[idx].LongWickPctAbovePreviousHigh - longWickPctAbovePreviousHigh) <= double.Epsilon && cacheCrespoCandleLocation[idx].MALength == mALength && cacheCrespoCandleLocation[idx].MAType == mAType && cacheCrespoCandleLocation[idx].MaxSize == maxSize && Math.Abs(cacheCrespoCandleLocation[idx].MaxWickPct - maxWickPct) <= double.Epsilon && cacheCrespoCandleLocation[idx].MinSize == minSize && cacheCrespoCandleLocation[idx].NotPinBarIfInsideBar == notPinBarIfInsideBar && cacheCrespoCandleLocation[idx].NumPreviousBars == numPreviousBars && Math.Abs(cacheCrespoCandleLocation[idx].OpenAndClosePctLocation - openAndClosePctLocation) <= double.Epsilon && Math.Abs(cacheCrespoCandleLocation[idx].PipBodySize - pipBodySize) <= double.Epsilon && cacheCrespoCandleLocation[idx].PipMaxBodySize == pipMaxBodySize && cacheCrespoCandleLocation[idx].PrevBarMinSize == prevBarMinSize && Math.Abs(cacheCrespoCandleLocation[idx].ShortWickMaxPct - shortWickMaxPct) <= double.Epsilon && cacheCrespoCandleLocation[idx].TradableEvent == tradableEvent && Math.Abs(cacheCrespoCandleLocation[idx].WickPercentage - wickPercentage) <= double.Epsilon && cacheCrespoCandleLocation[idx].EqualsInput(input))
                        return cacheCrespoCandleLocation[idx];

            lock (checkCrespoCandleLocation)
            {
                checkCrespoCandleLocation.AtLeastXPctTheSizeOfPreviousBar = atLeastXPctTheSizeOfPreviousBar;
                atLeastXPctTheSizeOfPreviousBar = checkCrespoCandleLocation.AtLeastXPctTheSizeOfPreviousBar;
                checkCrespoCandleLocation.BarsToLookBack = barsToLookBack;
                barsToLookBack = checkCrespoCandleLocation.BarsToLookBack;
                checkCrespoCandleLocation.BodyPctOfTotalBarSize = bodyPctOfTotalBarSize;
                bodyPctOfTotalBarSize = checkCrespoCandleLocation.BodyPctOfTotalBarSize;
                checkCrespoCandleLocation.CompareBodyOrTotalSize = compareBodyOrTotalSize;
                compareBodyOrTotalSize = checkCrespoCandleLocation.CompareBodyOrTotalSize;
                checkCrespoCandleLocation.EngulfingBarMinSize = engulfingBarMinSize;
                engulfingBarMinSize = checkCrespoCandleLocation.EngulfingBarMinSize;
                checkCrespoCandleLocation.IgnoreIndecisionBars = ignoreIndecisionBars;
                ignoreIndecisionBars = checkCrespoCandleLocation.IgnoreIndecisionBars;
                checkCrespoCandleLocation.LimitDistanceFromMA = limitDistanceFromMA;
                limitDistanceFromMA = checkCrespoCandleLocation.LimitDistanceFromMA;
                checkCrespoCandleLocation.Location = location;
                location = checkCrespoCandleLocation.Location;
                checkCrespoCandleLocation.LocBBBodyPct = locBBBodyPct;
                locBBBodyPct = checkCrespoCandleLocation.LocBBBodyPct;
                checkCrespoCandleLocation.LocBBDeviation = locBBDeviation;
                locBBDeviation = checkCrespoCandleLocation.LocBBDeviation;
                checkCrespoCandleLocation.LocMALength = locMALength;
                locMALength = checkCrespoCandleLocation.LocMALength;
                checkCrespoCandleLocation.LocMAType = locMAType;
                locMAType = checkCrespoCandleLocation.LocMAType;
                checkCrespoCandleLocation.LocPipDistance = locPipDistance;
                locPipDistance = checkCrespoCandleLocation.LocPipDistance;
                checkCrespoCandleLocation.LongWickMinPct = longWickMinPct;
                longWickMinPct = checkCrespoCandleLocation.LongWickMinPct;
                checkCrespoCandleLocation.LongWickPctAbovePreviousHigh = longWickPctAbovePreviousHigh;
                longWickPctAbovePreviousHigh = checkCrespoCandleLocation.LongWickPctAbovePreviousHigh;
                checkCrespoCandleLocation.MALength = mALength;
                mALength = checkCrespoCandleLocation.MALength;
                checkCrespoCandleLocation.MAType = mAType;
                mAType = checkCrespoCandleLocation.MAType;
                checkCrespoCandleLocation.MaxSize = maxSize;
                maxSize = checkCrespoCandleLocation.MaxSize;
                checkCrespoCandleLocation.MaxWickPct = maxWickPct;
                maxWickPct = checkCrespoCandleLocation.MaxWickPct;
                checkCrespoCandleLocation.MinSize = minSize;
                minSize = checkCrespoCandleLocation.MinSize;
                checkCrespoCandleLocation.NotPinBarIfInsideBar = notPinBarIfInsideBar;
                notPinBarIfInsideBar = checkCrespoCandleLocation.NotPinBarIfInsideBar;
                checkCrespoCandleLocation.NumPreviousBars = numPreviousBars;
                numPreviousBars = checkCrespoCandleLocation.NumPreviousBars;
                checkCrespoCandleLocation.OpenAndClosePctLocation = openAndClosePctLocation;
                openAndClosePctLocation = checkCrespoCandleLocation.OpenAndClosePctLocation;
                checkCrespoCandleLocation.PipBodySize = pipBodySize;
                pipBodySize = checkCrespoCandleLocation.PipBodySize;
                checkCrespoCandleLocation.PipMaxBodySize = pipMaxBodySize;
                pipMaxBodySize = checkCrespoCandleLocation.PipMaxBodySize;
                checkCrespoCandleLocation.PrevBarMinSize = prevBarMinSize;
                prevBarMinSize = checkCrespoCandleLocation.PrevBarMinSize;
                checkCrespoCandleLocation.ShortWickMaxPct = shortWickMaxPct;
                shortWickMaxPct = checkCrespoCandleLocation.ShortWickMaxPct;
                checkCrespoCandleLocation.TradableEvent = tradableEvent;
                tradableEvent = checkCrespoCandleLocation.TradableEvent;
                checkCrespoCandleLocation.WickPercentage = wickPercentage;
                wickPercentage = checkCrespoCandleLocation.WickPercentage;

                if (cacheCrespoCandleLocation != null)
                    for (int idx = 0; idx < cacheCrespoCandleLocation.Length; idx++)
                        if (Math.Abs(cacheCrespoCandleLocation[idx].AtLeastXPctTheSizeOfPreviousBar - atLeastXPctTheSizeOfPreviousBar) <= double.Epsilon && cacheCrespoCandleLocation[idx].BarsToLookBack == barsToLookBack && Math.Abs(cacheCrespoCandleLocation[idx].BodyPctOfTotalBarSize - bodyPctOfTotalBarSize) <= double.Epsilon && cacheCrespoCandleLocation[idx].CompareBodyOrTotalSize == compareBodyOrTotalSize && cacheCrespoCandleLocation[idx].EngulfingBarMinSize == engulfingBarMinSize && cacheCrespoCandleLocation[idx].IgnoreIndecisionBars == ignoreIndecisionBars && cacheCrespoCandleLocation[idx].LimitDistanceFromMA == limitDistanceFromMA && cacheCrespoCandleLocation[idx].Location == location && Math.Abs(cacheCrespoCandleLocation[idx].LocBBBodyPct - locBBBodyPct) <= double.Epsilon && Math.Abs(cacheCrespoCandleLocation[idx].LocBBDeviation - locBBDeviation) <= double.Epsilon && cacheCrespoCandleLocation[idx].LocMALength == locMALength && cacheCrespoCandleLocation[idx].LocMAType == locMAType && cacheCrespoCandleLocation[idx].LocPipDistance == locPipDistance && Math.Abs(cacheCrespoCandleLocation[idx].LongWickMinPct - longWickMinPct) <= double.Epsilon && Math.Abs(cacheCrespoCandleLocation[idx].LongWickPctAbovePreviousHigh - longWickPctAbovePreviousHigh) <= double.Epsilon && cacheCrespoCandleLocation[idx].MALength == mALength && cacheCrespoCandleLocation[idx].MAType == mAType && cacheCrespoCandleLocation[idx].MaxSize == maxSize && Math.Abs(cacheCrespoCandleLocation[idx].MaxWickPct - maxWickPct) <= double.Epsilon && cacheCrespoCandleLocation[idx].MinSize == minSize && cacheCrespoCandleLocation[idx].NotPinBarIfInsideBar == notPinBarIfInsideBar && cacheCrespoCandleLocation[idx].NumPreviousBars == numPreviousBars && Math.Abs(cacheCrespoCandleLocation[idx].OpenAndClosePctLocation - openAndClosePctLocation) <= double.Epsilon && Math.Abs(cacheCrespoCandleLocation[idx].PipBodySize - pipBodySize) <= double.Epsilon && cacheCrespoCandleLocation[idx].PipMaxBodySize == pipMaxBodySize && cacheCrespoCandleLocation[idx].PrevBarMinSize == prevBarMinSize && Math.Abs(cacheCrespoCandleLocation[idx].ShortWickMaxPct - shortWickMaxPct) <= double.Epsilon && cacheCrespoCandleLocation[idx].TradableEvent == tradableEvent && Math.Abs(cacheCrespoCandleLocation[idx].WickPercentage - wickPercentage) <= double.Epsilon && cacheCrespoCandleLocation[idx].EqualsInput(input))
                            return cacheCrespoCandleLocation[idx];

                CrespoCandleLocation indicator = new CrespoCandleLocation();
                indicator.BarsRequired = BarsRequired;
                indicator.CalculateOnBarClose = CalculateOnBarClose;
#if NT7
                indicator.ForceMaximumBarsLookBack256 = ForceMaximumBarsLookBack256;
                indicator.MaximumBarsLookBack = MaximumBarsLookBack;
#endif
                indicator.Input = input;
                indicator.AtLeastXPctTheSizeOfPreviousBar = atLeastXPctTheSizeOfPreviousBar;
                indicator.BarsToLookBack = barsToLookBack;
                indicator.BodyPctOfTotalBarSize = bodyPctOfTotalBarSize;
                indicator.CompareBodyOrTotalSize = compareBodyOrTotalSize;
                indicator.EngulfingBarMinSize = engulfingBarMinSize;
                indicator.IgnoreIndecisionBars = ignoreIndecisionBars;
                indicator.LimitDistanceFromMA = limitDistanceFromMA;
                indicator.Location = location;
                indicator.LocBBBodyPct = locBBBodyPct;
                indicator.LocBBDeviation = locBBDeviation;
                indicator.LocMALength = locMALength;
                indicator.LocMAType = locMAType;
                indicator.LocPipDistance = locPipDistance;
                indicator.LongWickMinPct = longWickMinPct;
                indicator.LongWickPctAbovePreviousHigh = longWickPctAbovePreviousHigh;
                indicator.MALength = mALength;
                indicator.MAType = mAType;
                indicator.MaxSize = maxSize;
                indicator.MaxWickPct = maxWickPct;
                indicator.MinSize = minSize;
                indicator.NotPinBarIfInsideBar = notPinBarIfInsideBar;
                indicator.NumPreviousBars = numPreviousBars;
                indicator.OpenAndClosePctLocation = openAndClosePctLocation;
                indicator.PipBodySize = pipBodySize;
                indicator.PipMaxBodySize = pipMaxBodySize;
                indicator.PrevBarMinSize = prevBarMinSize;
                indicator.ShortWickMaxPct = shortWickMaxPct;
                indicator.TradableEvent = tradableEvent;
                indicator.WickPercentage = wickPercentage;
                Indicators.Add(indicator);
                indicator.SetUp();

                CrespoCandleLocation[] tmp = new CrespoCandleLocation[cacheCrespoCandleLocation == null ? 1 : cacheCrespoCandleLocation.Length + 1];
                if (cacheCrespoCandleLocation != null)
                    cacheCrespoCandleLocation.CopyTo(tmp, 0);
                tmp[tmp.Length - 1] = indicator;
                cacheCrespoCandleLocation = tmp;
                return indicator;
            }
        }
    }
}

// This namespace holds all market analyzer column definitions and is required. Do not change it.
namespace NinjaTrader.MarketAnalyzer
{
    public partial class Column : ColumnBase
    {
        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.CrespoCandleLocation CrespoCandleLocation(double atLeastXPctTheSizeOfPreviousBar, int barsToLookBack, double bodyPctOfTotalBarSize, NinjaTrader.Indicator.CTS_BodyOrTotalSize compareBodyOrTotalSize, int engulfingBarMinSize, bool ignoreIndecisionBars, int limitDistanceFromMA, NinjaTrader.Indicator.CTS_Location location, double locBBBodyPct, double locBBDeviation, int locMALength, NinjaTrader.Indicator.CTS_MAType locMAType, int locPipDistance, double longWickMinPct, double longWickPctAbovePreviousHigh, int mALength, NinjaTrader.Indicator.CTS_MAType mAType, int maxSize, double maxWickPct, int minSize, bool notPinBarIfInsideBar, int numPreviousBars, double openAndClosePctLocation, double pipBodySize, int pipMaxBodySize, int prevBarMinSize, double shortWickMaxPct, NinjaTrader.Indicator.CTS_TradableEvent tradableEvent, double wickPercentage)
        {
            return _indicator.CrespoCandleLocation(Input, atLeastXPctTheSizeOfPreviousBar, barsToLookBack, bodyPctOfTotalBarSize, compareBodyOrTotalSize, engulfingBarMinSize, ignoreIndecisionBars, limitDistanceFromMA, location, locBBBodyPct, locBBDeviation, locMALength, locMAType, locPipDistance, longWickMinPct, longWickPctAbovePreviousHigh, mALength, mAType, maxSize, maxWickPct, minSize, notPinBarIfInsideBar, numPreviousBars, openAndClosePctLocation, pipBodySize, pipMaxBodySize, prevBarMinSize, shortWickMaxPct, tradableEvent, wickPercentage);
        }

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public Indicator.CrespoCandleLocation CrespoCandleLocation(Data.IDataSeries input, double atLeastXPctTheSizeOfPreviousBar, int barsToLookBack, double bodyPctOfTotalBarSize, NinjaTrader.Indicator.CTS_BodyOrTotalSize compareBodyOrTotalSize, int engulfingBarMinSize, bool ignoreIndecisionBars, int limitDistanceFromMA, NinjaTrader.Indicator.CTS_Location location, double locBBBodyPct, double locBBDeviation, int locMALength, NinjaTrader.Indicator.CTS_MAType locMAType, int locPipDistance, double longWickMinPct, double longWickPctAbovePreviousHigh, int mALength, NinjaTrader.Indicator.CTS_MAType mAType, int maxSize, double maxWickPct, int minSize, bool notPinBarIfInsideBar, int numPreviousBars, double openAndClosePctLocation, double pipBodySize, int pipMaxBodySize, int prevBarMinSize, double shortWickMaxPct, NinjaTrader.Indicator.CTS_TradableEvent tradableEvent, double wickPercentage)
        {
            return _indicator.CrespoCandleLocation(input, atLeastXPctTheSizeOfPreviousBar, barsToLookBack, bodyPctOfTotalBarSize, compareBodyOrTotalSize, engulfingBarMinSize, ignoreIndecisionBars, limitDistanceFromMA, location, locBBBodyPct, locBBDeviation, locMALength, locMAType, locPipDistance, longWickMinPct, longWickPctAbovePreviousHigh, mALength, mAType, maxSize, maxWickPct, minSize, notPinBarIfInsideBar, numPreviousBars, openAndClosePctLocation, pipBodySize, pipMaxBodySize, prevBarMinSize, shortWickMaxPct, tradableEvent, wickPercentage);
        }
    }
}

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.Strategy
{
    public partial class Strategy : StrategyBase
    {
        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.CrespoCandleLocation CrespoCandleLocation(double atLeastXPctTheSizeOfPreviousBar, int barsToLookBack, double bodyPctOfTotalBarSize, NinjaTrader.Indicator.CTS_BodyOrTotalSize compareBodyOrTotalSize, int engulfingBarMinSize, bool ignoreIndecisionBars, int limitDistanceFromMA, NinjaTrader.Indicator.CTS_Location location, double locBBBodyPct, double locBBDeviation, int locMALength, NinjaTrader.Indicator.CTS_MAType locMAType, int locPipDistance, double longWickMinPct, double longWickPctAbovePreviousHigh, int mALength, NinjaTrader.Indicator.CTS_MAType mAType, int maxSize, double maxWickPct, int minSize, bool notPinBarIfInsideBar, int numPreviousBars, double openAndClosePctLocation, double pipBodySize, int pipMaxBodySize, int prevBarMinSize, double shortWickMaxPct, NinjaTrader.Indicator.CTS_TradableEvent tradableEvent, double wickPercentage)
        {
            return _indicator.CrespoCandleLocation(Input, atLeastXPctTheSizeOfPreviousBar, barsToLookBack, bodyPctOfTotalBarSize, compareBodyOrTotalSize, engulfingBarMinSize, ignoreIndecisionBars, limitDistanceFromMA, location, locBBBodyPct, locBBDeviation, locMALength, locMAType, locPipDistance, longWickMinPct, longWickPctAbovePreviousHigh, mALength, mAType, maxSize, maxWickPct, minSize, notPinBarIfInsideBar, numPreviousBars, openAndClosePctLocation, pipBodySize, pipMaxBodySize, prevBarMinSize, shortWickMaxPct, tradableEvent, wickPercentage);
        }

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public Indicator.CrespoCandleLocation CrespoCandleLocation(Data.IDataSeries input, double atLeastXPctTheSizeOfPreviousBar, int barsToLookBack, double bodyPctOfTotalBarSize, NinjaTrader.Indicator.CTS_BodyOrTotalSize compareBodyOrTotalSize, int engulfingBarMinSize, bool ignoreIndecisionBars, int limitDistanceFromMA, NinjaTrader.Indicator.CTS_Location location, double locBBBodyPct, double locBBDeviation, int locMALength, NinjaTrader.Indicator.CTS_MAType locMAType, int locPipDistance, double longWickMinPct, double longWickPctAbovePreviousHigh, int mALength, NinjaTrader.Indicator.CTS_MAType mAType, int maxSize, double maxWickPct, int minSize, bool notPinBarIfInsideBar, int numPreviousBars, double openAndClosePctLocation, double pipBodySize, int pipMaxBodySize, int prevBarMinSize, double shortWickMaxPct, NinjaTrader.Indicator.CTS_TradableEvent tradableEvent, double wickPercentage)
        {
            if (InInitialize && input == null)
                throw new ArgumentException("You only can access an indicator with the default input/bar series from within the 'Initialize()' method");

            return _indicator.CrespoCandleLocation(input, atLeastXPctTheSizeOfPreviousBar, barsToLookBack, bodyPctOfTotalBarSize, compareBodyOrTotalSize, engulfingBarMinSize, ignoreIndecisionBars, limitDistanceFromMA, location, locBBBodyPct, locBBDeviation, locMALength, locMAType, locPipDistance, longWickMinPct, longWickPctAbovePreviousHigh, mALength, mAType, maxSize, maxWickPct, minSize, notPinBarIfInsideBar, numPreviousBars, openAndClosePctLocation, pipBodySize, pipMaxBodySize, prevBarMinSize, shortWickMaxPct, tradableEvent, wickPercentage);
        }
    }
}
#endregion
