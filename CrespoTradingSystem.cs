#region Using declarations
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Indicator;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Strategy;
#endregion

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.Strategy
{
    /// <summary>
    /// Enter the description of your strategy here
    /// </summary>
    [Description("Enter the description of your strategy here")]
    public class CrespoTradingSystem : Strategy, ICustomTypeDescriptor
    {
        #region Variables
        // Wizard generated variables
        private int quantity = 1; // Default setting for Quantity
        
		// Tradeable Events
        private NinjaTrader.Strategy.CTS_TradableEvent tradableEvent = CTS_TradableEvent.AnyBar; // Default setting for TradedableEvent
		private int prevBarMinSize = 5;
		private int engulfingBarMinSize = 5;
		private double pipBodySize = 1.6;
		private int numPreviousBars = 3;
		private double wickPercentage = 0.2;
		private int maLength = 20;
		private NinjaTrader.Strategy.CTS_MAType maType= CTS_MAType.Simple;
		private int limitDistanceFromMA = 30;
		private int pipMaxBodySize = 30;
		private double bodyPctOfTotalBarSize = 0.6;
		private int minSize = 2;
		private int maxSize = 4;
		private double maxWickPct = 0.2;
		private bool ignoreIndecisionBars = false;
		private CTS_BodyOrTotalSize compareBodyOrTotalSize = CTS_BodyOrTotalSize.Body;
		private double atLeastXPctTheSizeOfPreviousBar = 1.1;
        
		// Location
        private NinjaTrader.Strategy.CTS_Location location = CTS_Location.Anywhere; // Default setting for Location
		private int loc_MALength = 30;
		private NinjaTrader.Strategy.CTS_MAType loc_MAType = CTS_MAType.Exponential;
		private int loc_PipDistance = 15;
		private double loc_BBDeviation = 2;
		private double loc_BBBodyPct = 0.05;
		
        // TradeEntry
		private CTS_TradeEntry tradeEntry = CTS_TradeEntry.Immediately; // Default setting for TradeEntry
		private int te_PipBuffer = 3;
		
		// Initial Stop
        private CTS_InitialStop initialStop1 = CTS_InitialStop.EntryBarHighOrLow; // Default setting for InitialStop1
		private int is1_PipBuffer = 3;
		private int is1_NumberOfBars = 10;
		private int is1_MALength = 30;
		private CTS_MAType is1_MAType = CTS_MAType.Exponential;
		private int is1_PipDistance = 15;
		
		// Manage Trade
        private CTS_ManageTrade manageTrade1 = CTS_ManageTrade.MoveStopToNextBarHighLow; // Default setting for ManageTrade1
		private int mt1_PipBuffer = 5;
		private bool mt1_IgnoreIndecisionBars = false;
		private int mt1_ActivateOnlyAfterXBars = 4;
		private int mt1_PipsTrigger = 20;
		private int mt1_MinPipsProfit = 2;
		private double mt1_X = 0.5;
		private double mt1_Y = 0.9;
		private int mt1_MALength = 30;
		private CTS_MAType mt1_MAType = CTS_MAType.Exponential;
		private double mt1_PctRetracement = 0.61;
		
		// Conditions to Enter
        private CTS_ConditionsToEnter conditionsToEnter1 = CTS_ConditionsToEnter.NotUsed; // Default setting for ConditionsToEnter1
		private int ce1_NumBars = 6;
		private double ce1_X = 0.5;
		private int ce1_MALength = 30;
		private CTS_MAType ce1_MAType = CTS_MAType.Exponential;
		private double ce1_Slope = 0;
		private double ce1_Pct = 0;
		
		// User defined variables (add any user defined variables below)
		private double theStop = 0;
		private double initialStop = 0;
		private int entryBar = 0;
        #endregion

        /// <summary>
        /// This method is used to configure the strategy and is called once before any strategy method is called.
        /// </summary>
        protected override void Initialize()
        {
            CalculateOnBarClose = true;
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
        }
		
		private CTS_TradableEvent GetCandleStickPattern(int shift)
		{
			CTS_TradableEvent pattern = CTS_TradableEvent.AnyBar;
			
			return pattern;
		}
		
		bool IsCandlestickPattern()
		{
			if(TradableEvent == NinjaTrader.Strategy.CTS_TradableEvent.AnyBar)
			{
				return true;
			}
			
			bool pattern = false;
			
			pattern = IsBullIgnitingElephantBar(0) && TradableEvent == CTS_TradableEvent.BullIgnitingElephantBar;
			
			if(pattern)
			{
				return true;
			}
			
			pattern = IsBearIgnitingElephantBar(0) && TradableEvent == CTS_TradableEvent.BearIgnitingElephantBar;
			
			if(pattern)
			{
				return true;
			}
			
			pattern = IsBullExhaustionElephantBar(0) && TradableEvent == CTS_TradableEvent.BullExhaustionElephantBar;
			
			if(pattern)
			{
				return true;
			}
			
			pattern = IsBearExhaustionElephantBar(0) && TradableEvent == CTS_TradableEvent.BearExhaustionElephantBar;
			
			if(pattern)
			{
				return true;
			}
			
			pattern = IsBullBodyEngulfing(0) && TradableEvent == CTS_TradableEvent.BullBodyEngulfing;
			
			if(pattern)
			{
				return true;
			}
			
			pattern = IsBearBodyEngulfing(0) && TradableEvent == CTS_TradableEvent.BearBodyEngulfing;
			
			if(pattern)
			{
				return true;
			}
			
			pattern = IsBullBodyEngulfingOppositeColor(0) && TradableEvent == CTS_TradableEvent.BullBodyEngulfingOppositeColor;
			
			if(pattern)
			{
				return true;
			}
			
			pattern = IsBearBodyEngulfingOppositeColor(0) && TradableEvent == CTS_TradableEvent.BearBodyEngulfingOppositeColor;
			
			if(pattern)
			{
				return true;
			}
			
			pattern = IsBullBodyEngulfingIgniting(0) && TradableEvent == CTS_TradableEvent.BullBodyEngulfingIgniting;
			
			if(pattern)
			{
				return true;
			}
			
			pattern = IsBearBodyEngulfingIgniting(0) && TradableEvent == CTS_TradableEvent.BearBodyEngulfingIgniting;
			
			if(pattern)
			{
				return true;
			}
			
			pattern = IsBullBodyEngulfingOppositeColorIgniting(0) && TradableEvent == CTS_TradableEvent.BullBodyEngulfingOppositeColorIgniting;
			
			if(pattern)
			{
				return true;
			}
			
			pattern = IsBearBodyEngulfingOppositeColorIgniting(0) && TradableEvent == CTS_TradableEvent.BearBodyEngulfingOppositeColorIgniting;
			
			if(pattern)
			{
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
			
			pattern = IsIndecisionBar(pipMaxBodySize, bodyPctOfTotalBarSize, 0) && TradableEvent == CTS_TradableEvent.IndecisionBar;
			
			if(pattern)
			{
				return true;
			}
			
			pattern = IsBullElephantBar(0) && TradableEvent == CTS_TradableEvent.BullElephantBar;
			
			if(pattern)
			{
				return true;
			}
			
			pattern = IsBearElephantBar(0) && TradableEvent == CTS_TradableEvent.BearElephantBar;
			
			if(pattern)
			{
				return true;
			}
			
			pattern = IsNarrowRangeBar(0) && TradableEvent == CTS_TradableEvent.NarrowRangeBar;
			
			if(pattern)
			{
				return true;
			}
			
			pattern = IsVelezBuySetup(maLength, maType, numPreviousBars, wickPercentage, ignoreIndecisionBars, 0) && TradableEvent == CTS_TradableEvent.VelezBuySetup;
			
			if(pattern)
			{
				return true;
			}
			
			pattern = IsVelezSellSetup(maLength, maType, numPreviousBars, wickPercentage, ignoreIndecisionBars, 0) && TradableEvent == CTS_TradableEvent.VelezSellSetup;
			
			if(pattern)
			{
				return true;
			}
			
			pattern = IsBottomingTailSetup(maLength, maType, numPreviousBars, ignoreIndecisionBars, 0) && TradableEvent == CTS_TradableEvent.BottomingTailSetup;
			
			if(pattern)
			{
				return true;
			}
			
			pattern = IsToppingTailSetup(maLength, maType, numPreviousBars, ignoreIndecisionBars, 0) && TradableEvent == CTS_TradableEvent.ToppingTailSetup;
			
			if(pattern)
			{
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
		private bool IsIndecisionBar(int pipMaxBodySize, double bodyPctOfTotalBarSize, int shift)
		{
			double barBody = Body(shift);
			double barRange = Range(shift);
			
			if(barBody < pipMaxBodySize*TickSize && barBody <= barRange*bodyPctOfTotalBarSize)
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
			if(IsBodyEngulfing(prevBarMinSize, engulfingBarMinSize, atLeastXPctTheSizeOfPreviousBar, shift) && Close[shift] > Close[shift+1] && IsGreen(shift))
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
			if(IsBodyEngulfing(prevBarMinSize, engulfingBarMinSize, atLeastXPctTheSizeOfPreviousBar, shift) && Close[shift] < Close[shift+1] && IsRed(shift))
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
			return false;
		}
		
		private bool IsPinBarBottom(int shift)
		{
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
				if(IgnoreIndecisionBars && IsIndecisionBar(PipMaxBodySize, BodyPctOfTotalBarSize, shift))
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
				if(IgnoreIndecisionBars && IsIndecisionBar(PipMaxBodySize, BodyPctOfTotalBarSize, shift))
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
				if(IgnoreIndecisionBars && IsIndecisionBar(PipMaxBodySize, BodyPctOfTotalBarSize, shift))
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
				if(IgnoreIndecisionBars && IsIndecisionBar(PipMaxBodySize, BodyPctOfTotalBarSize, shift))
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
				double dMA = MA(LocMALength, LocMAType, 0);
				if(High[0] >= dMA - LocPipDistance*TickSize && Low[0] <= dMA + LocPipDistance*TickSize)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else if(Location == CTS_Location.BreakingUpperBollingerBand)
			{
				double dBB = Bollinger(LocBBDeviation, LocMALength).Upper[0];
				if(High[0] >= dBB + Range(0)*LocBBBodyPct)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else if(Location == CTS_Location.BreakingLowerBollingerBand)
			{
				double dBB = Bollinger(LocBBDeviation, LocMALength).Lower[0];
				if(Low[0] <= dBB - Range(0)*LocBBBodyPct)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			
			return true;
		}
		#endregion
		
		#region Trade Entry
		double GetBuyEntryPrice(CTS_TradeEntry tradeEntry)
		{
			switch(tradeEntry)
			{
				case CTS_TradeEntry.Immediately:
					return GetCurrentAsk();
					break;
					
				case CTS_TradeEntry.EntryBarClose:
					return (Close[0] + TEPipBuffer*TickSize);
					break;
					
				case CTS_TradeEntry.EntryBarHighOrLow:
					return (High[0] + TEPipBuffer*TickSize);
					break;
					
				case CTS_TradeEntry.PreviousBarHighOrLow:
					return (High[1] + TEPipBuffer*TickSize);
					break;
			}
			return GetCurrentAsk();
		}

		double GetSellEntryPrice(CTS_TradeEntry tradeEntry)
		{
			switch(tradeEntry)
			{
				case CTS_TradeEntry.Immediately:
					return GetCurrentBid();
					break;
					
				case CTS_TradeEntry.EntryBarClose:
					return (Close[0] - TEPipBuffer*TickSize);
					break;
					
				case CTS_TradeEntry.EntryBarHighOrLow:
					return (Low[0] - TEPipBuffer*TickSize);
					break;
					
				case CTS_TradeEntry.PreviousBarHighOrLow:
					return (Low[1] - TEPipBuffer*TickSize);
					break;
			}
			return GetCurrentBid();
		}
		#endregion
		
		#region Initial Stop
		double GetBuyInitialStopPrice(CTS_InitialStop initialStop, int buffer, int maLength, CTS_MAType maType, int numBars)
		{
			switch(initialStop)
			{
				case CTS_InitialStop.AboveOrBelowMA:
				{
					return (MA(maLength, maType, 0) - buffer*TickSize);
				}
				break;
				case CTS_InitialStop.EntryBarClose:
					return (Close[0] - buffer*TickSize);
					break;
					
				case CTS_InitialStop.EntryBarHighOrLow:
					return (Low[0] - buffer*TickSize);
					break;
					
				case CTS_InitialStop.PreviousBars:
					return (MIN(Low, numBars)[0] - buffer*TickSize);
					break;
			}
			return 0;
		}

		double GetSellInitialStopPrice(CTS_InitialStop initialStop, int buffer, int maLength, CTS_MAType maType, int numBars)
		{
			switch(initialStop)
			{
				case CTS_InitialStop.AboveOrBelowMA:
				{
					return (MA(maLength, maType, 0) + buffer*TickSize);
				}
				break;
				case CTS_InitialStop.EntryBarClose:
					return (Close[0] + buffer*TickSize);
					break;
					
				case CTS_InitialStop.EntryBarHighOrLow:
					return (High[0] + buffer*TickSize);
					break;
					
				case CTS_InitialStop.PreviousBars:
					return (MAX(High, numBars)[0] + buffer*TickSize);
					break;
			}
			return 0;
		}
		#endregion
		
		#region Manage Trade
		private void MoveStopToNextBarHighLow(int buffer, bool ignoreIndecisionBars, int activateOnlyAfterXBars)
		{
			if(ignoreIndecisionBars && IsIndecisionBar(pipMaxBodySize, bodyPctOfTotalBarSize, 0))
			{
				return;
			}
			
			if(activateOnlyAfterXBars > 0 && CurrentBar - entryBar <= activateOnlyAfterXBars)
			{
				return;
			}
			
			if(Position.MarketPosition == MarketPosition.Long)
			{
				if(theStop == 0 || theStop < Low[0] - buffer*TickSize)
				{
					theStop = Low[0] - buffer*TickSize;
				}
			}	
			else if(Position.MarketPosition == MarketPosition.Short)
			{
				if(theStop == 0 || theStop > High[0] + buffer*TickSize)
				{
					theStop = High[0] + buffer*TickSize;
				}
			}	
		}
		
		private void MoveStopToNextBarClose(int buffer, bool ignoreIndecisionBars, int activateOnlyAfterXBars)
		{
			if(ignoreIndecisionBars && IsIndecisionBar(pipMaxBodySize, bodyPctOfTotalBarSize, 0))
			{
				return;
			}
			
			if(activateOnlyAfterXBars > 0 && CurrentBar - entryBar <= activateOnlyAfterXBars)
			{
				return;
			}
			
			if(Position.MarketPosition == MarketPosition.Long)
			{
				if(theStop == 0 || theStop < Close[0] - buffer*TickSize)
				{
					theStop = Close[0] - buffer*TickSize;
				}
			}	
			else if(Position.MarketPosition == MarketPosition.Short)
			{
				if(theStop == 0 || theStop > Close[0] + buffer*TickSize)
				{
					theStop = Close[0] + buffer*TickSize;
				}
			}	
		}
		
		private void MoveStopToBreakEven(int triggerAfterXPips, int minPipsProfit, int activateOnlyAfterXBars)
		{
			if(activateOnlyAfterXBars > 0 && CurrentBar - entryBar <= activateOnlyAfterXBars)
			{
				return;
			}
			
			if(Position.MarketPosition == MarketPosition.Long && GetCurrentBid() >= Position.AvgPrice + triggerAfterXPips*TickSize)
			{
				if(theStop == 0 || theStop < Position.AvgPrice + minPipsProfit*TickSize)
				{
					theStop = Position.AvgPrice + minPipsProfit*TickSize;
				}
			}	
			else if(Position.MarketPosition == MarketPosition.Short && GetCurrentAsk() <= Position.AvgPrice - triggerAfterXPips*TickSize)
			{
				if(theStop == 0 || theStop > Position.AvgPrice - minPipsProfit*TickSize)
				{
					theStop = Position.AvgPrice - minPipsProfit*TickSize;
				}
			}	
		}
		
		private void TakeXPercentAtYto1(double x, double y, int activateOnlyAfterXBars)
		{
			if(Position.Quantity <= Quantity)
			{
				return;
			}
			
			if(activateOnlyAfterXBars > 0 && CurrentBar - entryBar <= activateOnlyAfterXBars)
			{
				return;
			}
			
			double target = Math.Abs(Position.AvgPrice - initialStop) * y;
			if(Position.MarketPosition == MarketPosition.Long)
			{
				if(GetCurrentBid() >= Position.AvgPrice + target)
				{
					ExitLong((int)(Quantity*y));
				}
			}	
			else if(Position.MarketPosition == MarketPosition.Short)
			{
				if(GetCurrentAsk() <= Position.AvgPrice - target)
				{
					ExitShort((int)(Quantity*y));
				}
			}	
		}
		
		private void CloseAtXToY(double x, double y, int activateOnlyAfterXBars)
		{
			if(activateOnlyAfterXBars > 0 && CurrentBar - entryBar <= activateOnlyAfterXBars)
			{
				return;
			}
			
			double target = Math.Abs(Position.AvgPrice - initialStop) * x / y;
			if(Position.MarketPosition == MarketPosition.Long)
			{
				if(GetCurrentBid() >= Position.AvgPrice + target)
				{
					ExitLong();
				}
			}	
			else if(Position.MarketPosition == MarketPosition.Short)
			{
				if(GetCurrentAsk() <= Position.AvgPrice - target)
				{
					ExitShort();
				}
			}	
		}
		
		private void CloseWithXPipsInProfit(int pipsProfit, int activateOnlyAfterXBars)
		{
			if(activateOnlyAfterXBars > 0 && CurrentBar - entryBar <= activateOnlyAfterXBars)
			{
				return;
			}
			
			if(Position.MarketPosition == MarketPosition.Long)
			{
				if(GetCurrentBid() >= Position.AvgPrice + pipsProfit*TickSize)
				{
					ExitLong();
				}
			}	
			else if(Position.MarketPosition == MarketPosition.Short)
			{
				if(GetCurrentAsk() <= Position.AvgPrice - pipsProfit*TickSize)
				{
					ExitShort();
				}
			}	
		}
		
		private void MATrailingStop(int maLength, CTS_MAType maType, int buffer, int activateOnlyAfterXBars)
		{
			if(activateOnlyAfterXBars > 0 && CurrentBar - entryBar <= activateOnlyAfterXBars)
			{
				return;
			}
			
			double ma = MA(maLength, maType, 0);
			
			if(Position.MarketPosition == MarketPosition.Long)
			{
				if(theStop == 0 || theStop < ma - buffer*TickSize)
				{
					theStop = ma - buffer*TickSize;
				}
			}	
			else if(Position.MarketPosition == MarketPosition.Short)
			{
				if(theStop == 0 || theStop > ma + buffer*TickSize)
				{
					theStop = ma + buffer*TickSize;
				}
			}	
		}
		
		private void TakeXPercentAtFiboRetracementOfLastSwing(double x, double pctRetracement)
		{
		}
		
		private void ManageStop()
		{
			 switch(ManageTrade1)
			{
				case CTS_ManageTrade.MoveStopToNextBarHighLow:
					MoveStopToNextBarHighLow(mt1_PipBuffer, mt1_IgnoreIndecisionBars, mt1_ActivateOnlyAfterXBars);
					break;
					
				case CTS_ManageTrade.MoveStopToNextBarClose:
					MoveStopToNextBarClose(mt1_PipBuffer, mt1_IgnoreIndecisionBars, mt1_ActivateOnlyAfterXBars);
					break;
					
				case CTS_ManageTrade.MoveStopToBreakEven:
					MoveStopToBreakEven(mt1_PipsTrigger, mt1_MinPipsProfit, mt1_ActivateOnlyAfterXBars);
					break;
					
				case CTS_ManageTrade.TakeXPercentAtYto1:
					TakeXPercentAtYto1(mt1_X, mt1_Y, mt1_ActivateOnlyAfterXBars);
					break;
					
				case CTS_ManageTrade.CloseAtXtoY:
					TakeXPercentAtYto1(mt1_X, mt1_Y, mt1_ActivateOnlyAfterXBars);
					break;
					
				case CTS_ManageTrade.CloseWithXPipsInProfit:
					CloseWithXPipsInProfit(mt1_MinPipsProfit, mt1_ActivateOnlyAfterXBars);
					break;
					
			}
		}
		#endregion

        #region Properties
        
		 #region Tradable Event
		
		[Description("")]
        [GridCategory("01. Tradable Event")]
		[RefreshProperties(RefreshProperties.All)]
		[Gui.Design.DisplayName(" TradableEvent")]
        public NinjaTrader.Strategy.CTS_TradableEvent TradableEvent
        {
            get { return tradableEvent; }
            set { tradableEvent = value; }
        }

        [Description("")]
        [GridCategory("01. Tradable Event")]
        public int PrevBarMinSize
        {
            get { return prevBarMinSize; }
            set { prevBarMinSize = Math.Max(1, value); }
        }

        [Description("")]
        [GridCategory("01. Tradable Event")]
        public int EngulfingBarMinSize
        {
            get { return engulfingBarMinSize; }
            set { engulfingBarMinSize = Math.Max(1, value); }
        }

        [Description("")]
        [GridCategory("01. Tradable Event")]
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
        public NinjaTrader.Strategy.CTS_MAType MAType
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
		public NinjaTrader.Strategy.CTS_BodyOrTotalSize CompareBodyOrTotalSize
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
		 #endregion
		
		 #region Location

        [Description("")]
        [GridCategory("Location")]
		[RefreshProperties(RefreshProperties.All)]
		[Gui.Design.DisplayName(" Location")]
        public CTS_Location Location
        {
            get { return location; }
            set { location = value; }
        }

        [Description("")]
        [GridCategory("Location")]
		[Gui.Design.DisplayName("MA Length")]
        public int LocMALength
        {
            get { return loc_MALength; }
            set { loc_MALength = Math.Max(1, value); }
        }

        [Description("")]
        [GridCategory("Location")]
		[Gui.Design.DisplayName("MA Type")]
        public CTS_MAType LocMAType
        {
            get { return loc_MAType; }
            set { loc_MAType = value; }
        }

        [Description("")]
        [GridCategory("Location")]
		[Gui.Design.DisplayName("Pip Distance")]
        public int LocPipDistance
        {
            get { return loc_PipDistance; }
            set { loc_PipDistance = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("Location")]
		[Gui.Design.DisplayName("BB Deviation")]
        public double LocBBDeviation
        {
            get { return loc_BBDeviation; }
            set { loc_BBDeviation = Math.Max(0.1, value); }
        }

        [Description("")]
        [GridCategory("Location")]
		[Gui.Design.DisplayName("BB Body Percent")]
        public double LocBBBodyPct
        {
            get { return loc_BBBodyPct; }
            set { loc_BBBodyPct = Math.Max(0, value); }
        }
		 #endregion
		
		 #region Trade Entry
        [Description("")]
        [GridCategory("Trade Entry")]
		[RefreshProperties(RefreshProperties.All)]
		[Gui.Design.DisplayName(" Trade Entry")]
        public CTS_TradeEntry TradeEntry
        {
            get { return tradeEntry; }
            set { tradeEntry = value; }
        }

        [Description("")]
        [GridCategory("Trade Entry")]
		[Gui.Design.DisplayName("Pip Buffer")]
        public int TEPipBuffer
        {
            get { return te_PipBuffer; }
            set { te_PipBuffer = Math.Max(0, value); }
        }
		 #endregion
		
		 #region InitialStop

        [Description("")]
        [GridCategory("Initial Stop")]
		[RefreshProperties(RefreshProperties.All)]
		[Gui.Design.DisplayName(" Initial Stop 1")]
        public CTS_InitialStop InitialStop1
        {
            get { return initialStop1; }
            set { initialStop1 = value; }
        }

        [Description("")]
        [GridCategory("Initial Stop")]
		[Gui.Design.DisplayName("Pip Buffer 1")]
        public int IS1PipBuffer
        {
            get { return is1_PipBuffer; }
            set { is1_PipBuffer = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("Initial Stop")]
		[Gui.Design.DisplayName("Number of Bars 1")]
        public int IS1NumberOfBars
        {
            get { return is1_NumberOfBars; }
            set { is1_NumberOfBars = Math.Max(1, value); }
        }

        [Description("")]
        [GridCategory("Initial Stop")]
		[Gui.Design.DisplayName("MA Length 1")]
        public int IS1MALength
        {
            get { return is1_MALength; }
            set { is1_MALength = Math.Max(1, value); }
        }

        [Description("")]
        [GridCategory("Initial Stop")]
		[Gui.Design.DisplayName("MA Type 1")]
        public CTS_MAType IS1MAType
        {
            get { return is1_MAType; }
            set { is1_MAType = value; }
        }

        [Description("")]
        [GridCategory("Initial Stop")]
		[Gui.Design.DisplayName("Pip Distance 1")]
        public int IS1PipDistance
        {
            get { return is1_PipDistance; }
            set { is1_PipDistance = Math.Max(0, value); }
        }
		 #endregion
		
		 #region Manage Trade
        [Description("")]
        [GridCategory("Manage Trade")]
		[RefreshProperties(RefreshProperties.All)]
		[Gui.Design.DisplayName(" Manage Trade 1")]
        public CTS_ManageTrade ManageTrade1
        {
            get { return manageTrade1; }
            set { manageTrade1 = value; }
        }

        [Description("")]
        [GridCategory("Manage Trade")]
		[Gui.Design.DisplayName("Pip Buffer 1")]
        public int MT1PipBuffer
        {
            get { return mt1_PipBuffer; }
            set { mt1_PipBuffer = Math.Max(0, value); }
        }
		
        [Description("")]
        [GridCategory("Manage Trade")]
		[Gui.Design.DisplayName("Ignore Indecision Bars 1")]
        public bool MT1IgnoreIndecisionBars
        {
            get { return mt1_IgnoreIndecisionBars; }
            set { mt1_IgnoreIndecisionBars = value; }
        }

        [Description("")]
        [GridCategory("Manage Trade")]
		[Gui.Design.DisplayName("Activate Only After X Bars 1")]
        public int MT1ActivateOnlyAfterXBars
        {
            get { return mt1_ActivateOnlyAfterXBars; }
            set { mt1_ActivateOnlyAfterXBars = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("Manage Trade")]
		[Gui.Design.DisplayName("Trigger After X Pips 1")]
        public int MT1PipTrigger
        {
            get { return mt1_PipsTrigger; }
            set { mt1_PipsTrigger = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("Manage Trade")]
		[Gui.Design.DisplayName("Min Pip Profit 1")]
        public int MT1MinPipsProfit
        {
            get { return mt1_MinPipsProfit; }
            set { mt1_MinPipsProfit = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("Manage Trade")]
		[Gui.Design.DisplayName("Percent Retracement 1")]
        public double MT1PctRetracement
        {
            get { return mt1_PctRetracement; }
            set { mt1_PctRetracement = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("Manage Trade")]
		[Gui.Design.DisplayName("X 1")]
        public double MT1X
        {
            get { return mt1_X; }
            set { mt1_X = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("Manage Trade")]
		[Gui.Design.DisplayName("Y 1")]
        public double MT1Y
        {
            get { return mt1_Y; }
            set { mt1_Y = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("Manage Trade")]
		[Gui.Design.DisplayName("MA Length 1")]
        public int MT1MALength
        {
            get { return mt1_MALength; }
            set { mt1_MALength = Math.Max(1, value); }
        }

        [Description("")]
        [GridCategory("Manage Trade")]
		[Gui.Design.DisplayName("MA Type 1")]
        public CTS_MAType MT1MAType
        {
            get { return mt1_MAType; }
            set { mt1_MAType = value; }
        }
		 #endregion

		 #region Conditions To Enter
        [Description("")]
        [GridCategory("Conditions To Enter")]
		[RefreshProperties(RefreshProperties.All)]
		[Gui.Design.DisplayName(" Conditions To Enter 1")]
        public CTS_ConditionsToEnter ConditionsToEnter1
        {
            get { return conditionsToEnter1; }
            set { conditionsToEnter1 = value; }
        }

        [Description("")]
        [GridCategory("Conditions To Enter")]
		[Gui.Design.DisplayName("Percentage 1")]
        public double CE1Pct
        {
            get { return ce1_Pct; }
            set { ce1_Pct = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("Conditions To Enter")]
		[Gui.Design.DisplayName("X 1")]
        public double CE1X
        {
            get { return ce1_X; }
            set { ce1_X = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("Conditions To Enter")]
		[Gui.Design.DisplayName("Slope 1")]
        public double CE1Slope
        {
            get { return ce1_Slope; }
            set { ce1_Slope = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("Conditions To Enter")]
		[Gui.Design.DisplayName("MA Length 1")]
        public int CE1MALength
        {
            get { return ce1_MALength; }
            set { ce1_MALength = Math.Max(1, value); }
        }

        [Description("")]
        [GridCategory("Conditions To Enter")]
		[Gui.Design.DisplayName("MA Type 1")]
        public CTS_MAType CE1MAType
        {
            get { return ce1_MAType; }
            set { ce1_MAType = value; }
        }
		 #endregion

        [Description("")]
        [GridCategory("Parameters")]
        public int Quantity
        {
            get { return quantity; }
            set { quantity = Math.Max(1, value); }
        }
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
					break;
					
				case CTS_TradableEvent.BullBodyEngulfing:
				case CTS_TradableEvent.BearBodyEngulfing:
				case CTS_TradableEvent.BullBodyEngulfingOppositeColor:
				case CTS_TradableEvent.BearBodyEngulfingOppositeColor:
				case CTS_TradableEvent.BodyEngulfing:
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
					col.Remove(col.Find("MaxWickPct", true));
					col.Remove(col.Find("IgnoreIndecisionBars", true));
					col.Remove(col.Find("CompareBodyOrTotalSize", true));
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
					col.Remove(col.Find("IgnoreIndecisionBars", true));
					col.Remove(col.Find("CompareBodyOrTotalSize", true));
					col.Remove(col.Find("AtLeastXPctTheSizeOfPreviousBar", true));
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
					col.Remove(col.Find("LocBBDeviation", true));
					col.Remove(col.Find("LocBBBodyPct", true));
					break;

				case CTS_Location.BreakingLowerBollingerBand:
				case CTS_Location.BreakingUpperBollingerBand:
					col.Remove(col.Find("LocMALength", true));
					col.Remove(col.Find("LocBBDeviation", true));
					col.Remove(col.Find("LocBBBodyPct", true));
					break;
					
				default:
					col.Remove(col.Find("LocMALength", true));
					col.Remove(col.Find("LocMAType", true));
					col.Remove(col.Find("LocPipDistance", true));
					col.Remove(col.Find("LocBBDeviation", true));
					col.Remove(col.Find("LocBBBodyPct", true));
					break;
			}
			
			switch(TradeEntry)
			{
				case CTS_TradeEntry.Immediately:
					col.Remove(col.Find("TEPipBuffer", true));
					break;
			}
			
			switch(InitialStop1)
			{
				case CTS_InitialStop.EntryBarHighOrLow:
				case CTS_InitialStop.EntryBarClose:
					col.Remove(col.Find("IS1NumberOfBars", true));
					col.Remove(col.Find("IS1PipDistance", true));
					col.Remove(col.Find("IS1MALength", true));
					col.Remove(col.Find("IS1MAType", true));
					break;

				case CTS_InitialStop.PreviousBars:
					col.Remove(col.Find("IS1PipDistance", true));
					col.Remove(col.Find("IS1MALength", true));
					col.Remove(col.Find("IS1MAType", true));
					break;

				case CTS_InitialStop.AboveOrBelowMA:
					col.Remove(col.Find("IS1NumberOfBars", true));
					break;

				default:
					col.Remove(col.Find("IS1NumberOfBars", true));
					col.Remove(col.Find("IS1PipBuffer", true));
					col.Remove(col.Find("IS1PipDistance", true));
					col.Remove(col.Find("IS1MALength", true));
					col.Remove(col.Find("IS1MAType", true));
					break;
			}
			
			switch(ManageTrade1)
			{
				case CTS_ManageTrade.MoveStopToNextBarHighLow:
				case CTS_ManageTrade.MoveStopToNextBarClose:
					col.Remove(col.Find("MT1PipTrigger", true));
					col.Remove(col.Find("MT1MinPipsProfit", true));
					col.Remove(col.Find("MT1PctRetracement", true));
					col.Remove(col.Find("MT1X", true));
					col.Remove(col.Find("MT1Y", true));
					col.Remove(col.Find("MT1MALength", true));
					col.Remove(col.Find("MT1MAType", true));
					break;
				
				case CTS_ManageTrade.MoveStopToBreakEven:
					col.Remove(col.Find("MT1PipBuffer", true));
					col.Remove(col.Find("MT1IgnoreIndecisionBars", true));
					col.Remove(col.Find("MT1PctRetracement", true));
					col.Remove(col.Find("MT1X", true));
					col.Remove(col.Find("MT1Y", true));
					col.Remove(col.Find("MT1MALength", true));
					col.Remove(col.Find("MT1MAType", true));
					break;
				
				case CTS_ManageTrade.TakeXPercentAtYto1:
				case CTS_ManageTrade.CloseAtXtoY:
					col.Remove(col.Find("MT1PipBuffer", true));
					col.Remove(col.Find("MT1IgnoreIndecisionBars", true));
					col.Remove(col.Find("MT1PipTrigger", true));
					col.Remove(col.Find("MT1MinPipsProfit", true));
					col.Remove(col.Find("MT1PctRetracement", true));
					col.Remove(col.Find("MT1MALength", true));
					col.Remove(col.Find("MT1MAType", true));
					break;

				case CTS_ManageTrade.CloseWithXPipsInProfit:
					col.Remove(col.Find("MT1PipBuffer", true));
					col.Remove(col.Find("MT1IgnoreIndecisionBars", true));
					col.Remove(col.Find("MT1PipTrigger", true));
					col.Remove(col.Find("MT1PctRetracement", true));
					col.Remove(col.Find("MT1X", true));
					col.Remove(col.Find("MT1Y", true));
					col.Remove(col.Find("MT1MALength", true));
					col.Remove(col.Find("MT1MAType", true));
					break;
					
				case CTS_ManageTrade.MATrailingStop:
					col.Remove(col.Find("MT1PipBuffer", true));
					col.Remove(col.Find("MT1IgnoreIndecisionBars", true));
					col.Remove(col.Find("MT1PipTrigger", true));
					col.Remove(col.Find("MT1MinPipsProfit", true));
					col.Remove(col.Find("MT1PctRetracement", true));
					col.Remove(col.Find("MT1Y", true));
					break;
					
				case CTS_ManageTrade.TakeXPercentAtFiboRetracementOfLastSwing:
					col.Remove(col.Find("MT1PipBuffer", true));
					col.Remove(col.Find("MT1IgnoreIndecisionBars", true));
					col.Remove(col.Find("MT1ActivateOnlyAfterXBars", true));
					col.Remove(col.Find("MT1PipTrigger", true));
					col.Remove(col.Find("MT1MinPipsProfit", true));
					col.Remove(col.Find("MT1Y", true));
					col.Remove(col.Find("MT1MALength", true));
					col.Remove(col.Find("MT1MAType", true));
					break;
					
				default:
					col.Remove(col.Find("MT1PipBuffer", true));
					col.Remove(col.Find("MT1IgnoreIndecisionBars", true));
					col.Remove(col.Find("MT1ActivateOnlyAfterXBars", true));
					col.Remove(col.Find("MT1PipTrigger", true));
					col.Remove(col.Find("MT1MinPipsProfit", true));
					col.Remove(col.Find("MT1PctRetracement", true));
					col.Remove(col.Find("MT1X", true));
					col.Remove(col.Find("MT1Y", true));
					col.Remove(col.Find("MT1MALength", true));
					col.Remove(col.Find("MT1MAType", true));
					break;
			}
			
			switch(ConditionsToEnter1)
			{
				case CTS_ConditionsToEnter.NotUsed:
					col.Remove(col.Find("CE1NumBars", true));
					col.Remove(col.Find("CE1Pct", true));
					col.Remove(col.Find("CE1X", true));
					col.Remove(col.Find("CE1Y", true));
					col.Remove(col.Find("CE1MALength", true));
					col.Remove(col.Find("CE1MAType", true));
					col.Remove(col.Find("CE1Slope", true));
					break;
					
				case CTS_ConditionsToEnter.MaxPreviousCOnsecutiveBarsOfSameColor:
					col.Remove(col.Find("CE1Pct", true));
					col.Remove(col.Find("CE1X", true));
					col.Remove(col.Find("CE1Y", true));
					col.Remove(col.Find("CE1MALength", true));
					col.Remove(col.Find("CE1MAType", true));
					col.Remove(col.Find("CE1Slope", true));
					break;
					
				case CTS_ConditionsToEnter.MAIsNotWIthinXTimesInitialStop:
					col.Remove(col.Find("CE1NumBars", true));
					col.Remove(col.Find("CE1Pct", true));
					col.Remove(col.Find("CE1Y", true));
					col.Remove(col.Find("CE1Slope", true));
					break;
					
				case CTS_ConditionsToEnter.PDCIsNotWithinXTimesInitialStop:
					col.Remove(col.Find("CE1NumBars", true));
					col.Remove(col.Find("CE1Pct", true));
					col.Remove(col.Find("CE1Y", true));
					col.Remove(col.Find("CE1MALength", true));
					col.Remove(col.Find("CE1MAType", true));
					col.Remove(col.Find("CE1Slope", true));
					break;
					
				case CTS_ConditionsToEnter.MAIsFlat:
				case CTS_ConditionsToEnter.MAIsRising:
				case CTS_ConditionsToEnter.MAIsFalling:
					col.Remove(col.Find("CE1NumBars", true));
					col.Remove(col.Find("CE1Pct", true));
					col.Remove(col.Find("CE1X", true));
					col.Remove(col.Find("CE1Y", true));
					break;
					
				case CTS_ConditionsToEnter.PriceIsAboveMA:
				case CTS_ConditionsToEnter.PriceIsBelowMA:
					col.Remove(col.Find("CE1NumBars", true));
					col.Remove(col.Find("CE1Pct", true));
					col.Remove(col.Find("CE1X", true));
					col.Remove(col.Find("CE1Y", true));
					col.Remove(col.Find("CE1Slope", true));
					break;
					
				case CTS_ConditionsToEnter.StochasticsIsOver:
				case CTS_ConditionsToEnter.StochasticsIsBelow:
					col.Remove(col.Find("CE1NumBars", true));
					col.Remove(col.Find("CE1X", true));
					col.Remove(col.Find("CE1Y", true));
					col.Remove(col.Find("CE1MALength", true));
					col.Remove(col.Find("CE1MAType", true));
					col.Remove(col.Find("CE1Slope", true));
					break;
					
				default:
					col.Remove(col.Find("CE1NumBars", true));
					col.Remove(col.Find("CE1Pct", true));
					col.Remove(col.Find("CE1X", true));
					col.Remove(col.Find("CE1Y", true));
					col.Remove(col.Find("CE1MALength", true));
					col.Remove(col.Find("CE1MAType", true));
					col.Remove(col.Find("CE1Slope", true));
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
	
	#region Enums
	public enum CTS_TradableEvent
	{
		AnyBar,
		BullIgnitingElephantBar,
		BearIgnitingElephantBar,
		BullExhaustionElephantBar,
		BearExhaustionElephantBar,
		BullBodyEngulfing,
		BearBodyEngulfing,
		BullBodyEngulfingOppositeColor,
		BearBodyEngulfingOppositeColor,
		BullBodyEngulfingIgniting,
		BearBodyEngulfingIgniting,
		BullBodyEngulfingOppositeColorIgniting,
		BearBodyEngulfingOppositeColorIgniting,
		BodyEngulfing,
		BullElephantBar,
		BearElephantBar,
		Igniting,
		Exhausting,
		IndecisionBar,
		AlertBar,
		DarkCloudCover,
		PiercingLine,
		Doji,
		Hammer,
		NarrowRangeBar,
		VelezBuySetup,
		VelezSellSetup,
		BottomingTailSetup,
		ToppingTailSetup
	}
	
	public enum CTS_Location
	{
		Anywhere,
		AtOrNearMA,
		BreakingUpperBollingerBand,
		BreakingLowerBollingerBand
	}
	
	public enum CTS_BodyOrTotalSize
	{
		Body,
		TotalSize
	}

	public enum CTS_TradeEntry
	{
		Immediately,
		EntryBarClose,
		EntryBarHighOrLow,
		PreviousBarHighOrLow
	}
	
	public enum CTS_InitialStop
	{
		NotUsed,
		EntryBarHighOrLow,
		EntryBarClose,
		PreviousBars,
		AboveOrBelowMA
	}
	
	public enum CTS_ManageTrade
	{
		NotUsed,
		MoveStopToNextBarHighLow,
		MoveStopToNextBarClose,
		MoveStopToBreakEven,
		TakeXPercentAtYto1,
		CloseAtXtoY,
		CloseWithXPipsInProfit,
		MATrailingStop,
		TakeXPercentAtFiboRetracementOfLastSwing
	}
	
	public enum CTS_ConditionsToEnter
	{
		NotUsed,
		MinRiskRewardXToYForSupportAndResistanceTrade,
		MaxPreviousCOnsecutiveBarsOfSameColor,
		MAIsNotWIthinXTimesInitialStop,
		PDCIsNotWithinXTimesInitialStop,
		MAIsFlat,
		MAIsRising,
		MAIsFalling,
		PriceIsAboveMA,
		PriceIsBelowMA,
		StochasticsIsOver,
		StochasticsIsBelow
	}
	
	public enum CTS_MAType
	{
		Simple,
		Exponential,
		DoubleExponential,
		Hull
	}
	#endregion
}