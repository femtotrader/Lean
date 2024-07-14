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

namespace QuantConnect.Indicators
{
    /// <summary>
    /// This indicator computes the Klinger Volume Oscillator (KVO)
    /// </summary>
    public class KlingerVolumeOscillator : BarIndicator, IIndicatorWarmUpPeriodProvider
    {

        private IndicatorBase<IndicatorDataPoint> _fastMA { get; }
        private IndicatorBase<IndicatorDataPoint> _slowMA { get; }

        /// <summary>
        /// Required period, in data points, for the indicator to be ready and fully initialized.
        /// </summary>
        public int WarmUpPeriod { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KlingerVolumeOscillator"/> class using the specified name.
        /// </summary>
        /// <param name="name">The name of this indicator</param>
        /// <param name="fastPeriod">The fast smoothing period used to smooth the volume force values</param>
        /// <param name="slowPeriod">The slow smoothing period used to smooth the volume force values</param>
        /// <param name="movingAverageType">The type of moving average to be used</param>
        public KlingerVolumeOscillator(string name, int fastPeriod, int slowPeriod, MovingAverageType movingAverageType = MovingAverageType.Simple)
            : base(name)
        {
            //WarmUpPeriod = 1;
            _fastMA = movingAverageType.AsIndicator($"{name}_fastMA", fastPeriod);
            _slowMA = movingAverageType.AsIndicator($"{name}_slowMA", fastPeriod);

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KlingerVolumeOscillator"/> class using the specified name.
        /// </summary>
        /// <param name="fastPeriod">The fast smoothing period used to smooth the volume force values</param>
        /// <param name="slowPeriod">The slow smoothing period used to smooth the volume force values</param>
        /// <param name="movingAverageType">The type of moving average to be used</param>
        public KlingerVolumeOscillator(int fastPeriod, int slowPeriod, MovingAverageType movingAverageType = MovingAverageType.Simple) 
            : this($"VWMA({fastPeriod},{slowPeriod},{movingAverageType})", fastPeriod, slowPeriod, movingAverageType)
        {
        }

        /// <summary>
        /// Gets a flag indicating when this indicator is ready and fully initialized
        /// </summary>
        public override bool IsReady => _fastMA.IsReady && _slowMA.IsReady;

        /// <summary>
        /// Computes the next value of this indicator from the given state
        /// </summary>
        /// <param name="input">The input given to the indicator</param>
        /// <returns>A new value for this indicator</returns>
        protected override decimal ComputeNextValue(IBaseDataBar input)
        {

            return 42;
        }

        /// <summary>
        /// Resets this indicator to its initial state
        /// </summary>
        public override void Reset()
        {
            _fastMA.Reset();
            _slowMA.Reset();
            base.Reset();
        }

    }
}
