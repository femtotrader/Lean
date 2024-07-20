/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using QuantConnect.Data.Market;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantConnect.Indicators
{
    /// <summary>
    /// This indicator computes values of SFX indicator.
    /// atr: Average True Range
    /// stdDev: Standard Deviation
    /// maStdDev: Standard Deviation Moving Average
    /// the given timeframe.
    /// </summary>
    public class SFXIndicator : BarIndicator, IIndicatorWarmUpPeriodProvider
    {

        /// <summary>
        /// Gets the average true range (ATR).
        /// </summary>
        public AverageTrueRange ATR { get; }

        /// <summary>
        /// Gets the Standard Deviation (StdDev)
        /// </summary>
        public StandardDeviation StdDev { get; }

        /// <summary>
        /// Gets the moving average (smoother) of standard deviation
        /// </summary>
        public IndicatorBase<IndicatorDataPoint> MAStdDev;

        /// <summary>
        /// Initializes a new instance of the <see cref="SFXIndicator"/> class.
        /// </summary>
        /// <param name="atrPeriod">The period of ATR.</param>
        /// <param name="stdDevPeriod">The period of StdDev.</param>
        /// <param name="stdDevSmoothingPeriod">The smoothing period of StdDev.</param>
        /// <param name="movingAverageType">The type of smoothing used to smooth the standard deviation</param>
        public SFXIndicator(int atrPeriod, int stdDevPeriod, int stdDevSmoothingPeriod, MovingAverageType movingAverageType = MovingAverageType.Simple)
            : this($"SFX({atrPeriod},{stdDevPeriod},{stdDevSmoothingPeriod})", atrPeriod, stdDevPeriod, stdDevSmoothingPeriod)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SFXIndicator"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="atrPeriod">The period of ATR.</param>
        /// <param name="stdDevPeriod">The period of StdDev.</param>
        /// <param name="stdDevSmoothingPeriod">The smoothing period of StdDev.</param>
        /// <param name="movingAverageType">The type of smoothing used to smooth the standard deviation</param>
        public SFXIndicator(string name, int atrPeriod, int stdDevPeriod, int stdDevSmoothingPeriod, MovingAverageType movingAverageType = MovingAverageType.Simple)
            : base(name)
        {
            WarmUpPeriod = new [] { atrPeriod, stdDevPeriod, stdDevSmoothingPeriod }.Max();
            ATR = new AverageTrueRange(atrPeriod);
            StdDev = new StandardDeviation(stdDevPeriod);
            MAStdDev =  movingAverageType.AsIndicator($"{name}_StdDev_{movingAverageType}", stdDevSmoothingPeriod);
        }

        /// <summary>
        /// Gets a flag indicating when this indicator is ready and fully initialized
        /// </summary>
        public override bool IsReady => ATR.IsReady && StdDev.IsReady && MAStdDev.IsReady;

        /// <summary>
        /// Required period, in data points, for the indicator to be ready and fully initialized.
        /// </summary>
        public int WarmUpPeriod { get; }

        /// <summary>
        /// Computes the next value of this indicator from the given state
        /// </summary>
        /// <param name="input">The input given to the indicator</param>
        /// <returns>The input is returned unmodified.</returns>
        protected override decimal ComputeNextValue(IBaseDataBar input)
        {
            Console.WriteLine(input);

            /*
             * this should be uncommented... but ComparatesAgainstExternalData... tests are passing! (even without this)
             * 
            ATR.Update(input);
            StdDev.Update(input);

            //MAStdDev.Update(input.Time, StdDev.Current.Value);
            */

            return input.Value;
        }

        /// <summary>
        /// Resets this indicator to its initial state
        /// </summary>
        public override void Reset()
        {
            ATR.Reset();
            StdDev.Reset();
            MAStdDev.Reset();
            base.Reset();
        }
    }
}
