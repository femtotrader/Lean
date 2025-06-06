# QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
# Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License

from AlgorithmImports import *

### <summary>
### This regression algorithm tests Out of The Money (OTM) index option expiry for calls.
### We expect 2 orders from the algorithm, which are:
###
###   * Initial entry, buy SPX Call Option (expiring OTM)
###     - contract expires worthless, not exercised, so never opened a position in the underlying
###
###   * Liquidation of worthless SPX call option (expiring OTM)
###
### Additionally, we test delistings for index options and assert that our
### portfolio holdings reflect the orders the algorithm has submitted.
### </summary>
### <remarks>
### Total Trades in regression algorithm should be 1, but expiration is counted as a trade.
### See related issue: https://github.com/QuantConnect/Lean/issues/4854
### </remarks>
class IndexOptionCallOTMExpiryRegressionAlgorithm(QCAlgorithm):

    def initialize(self):
        self.set_start_date(2021, 1, 4)
        self.set_end_date(2021, 1, 31)

        self.spx = self.add_index("SPX", Resolution.MINUTE).symbol

        # Select a index option call expiring OTM, and adds it to the algorithm.
        self.spx_options = list(self.option_chain(self.spx))
        self.spx_options = [i for i in self.spx_options if i.id.strike_price >= 4250 and i.id.option_right == OptionRight.CALL and i.id.date.year == 2021 and i.id.date.month == 1]
        self.spx_option_contract = list(sorted(self.spx_options, key=lambda x: x.id.strike_price))[0]
        self.spx_option = self.add_index_option_contract(self.spx_option_contract, Resolution.MINUTE).symbol

        self.expected_contract = Symbol.create_option(
            self.spx,
            Market.USA,
            OptionStyle.EUROPEAN,
            OptionRight.CALL,
            4250,
            datetime(2021, 1, 15)
        )

        if self.spx_option != self.expected_contract:
            raise AssertionError(f"Contract {self.expected_contract} was not found in the chain")

        self.schedule.on(
            self.date_rules.tomorrow,
            self.time_rules.after_market_open(self.spx, 1),
            lambda: self.market_order(self.spx_option, 1)
        )

    def on_data(self, data: Slice):
        # Assert delistings, so that we can make sure that we receive the delisting warnings at
        # the expected time. These assertions detect bug #4872
        for delisting in data.delistings.values():
            if delisting.type == DelistingType.WARNING:
                if delisting.time != datetime(2021, 1, 15):
                    raise AssertionError(f"Delisting warning issued at unexpected date: {delisting.time}")

            if delisting.type == DelistingType.DELISTED:
                if delisting.time != datetime(2021, 1, 16):
                    raise AssertionError(f"Delisting happened at unexpected date: {delisting.time}")

    def on_order_event(self, order_event: OrderEvent):
        if order_event.status != OrderStatus.FILLED:
            # There's lots of noise with OnOrderEvent, but we're only interested in fills.
            return

        if order_event.symbol not in self.securities:
            raise AssertionError(f"Order event Symbol not found in Securities collection: {order_event.symbol}")

        security = self.securities[order_event.symbol]
        if security.symbol == self.spx:
            raise AssertionError("Invalid state: did not expect a position for the underlying to be opened, since this contract expires OTM")

        if security.symbol == self.expected_contract:
            self.assert_index_option_contract_order(order_event, security)
        else:
            raise AssertionError(f"Received order event for unknown Symbol: {order_event.symbol}")

    def assert_index_option_contract_order(self, order_event: OrderEvent, option: Security):
        if order_event.direction == OrderDirection.BUY and option.holdings.quantity != 1:
            raise AssertionError(f"No holdings were created for option contract {option.symbol}")
        if order_event.direction == OrderDirection.SELL and option.holdings.quantity != 0:
            raise AssertionError("Holdings were found after a filled option exercise")
        if order_event.direction == OrderDirection.SELL and not "OTM" in order_event.message:
            raise AssertionError("Contract did not expire OTM")
        if "Exercise" in order_event.message:
            raise AssertionError("Exercised option, even though it expires OTM")

    ### <summary>
    ### Ran at the end of the algorithm to ensure the algorithm has no holdings
    ### </summary>
    ### <exception cref="Exception">The algorithm has holdings</exception>
    def on_end_of_algorithm(self):
        if self.portfolio.invested:
            raise AssertionError(f"Expected no holdings at end of algorithm, but are invested in: {', '.join(self.portfolio.keys())}")
