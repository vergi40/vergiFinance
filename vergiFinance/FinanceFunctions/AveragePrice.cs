using static System.Net.Mime.MediaTypeNames;

namespace vergiFinance.FinanceFunctions
{
    internal class AveragePrice
    {
        private LinkedList<(decimal amount, decimal price, decimal avPrice)> _fifoQueue { get; } = new();

        public void AddBuyEvent(decimal amount, decimal price)
        {
            _fifoQueue.AddLast((amount, price, price/amount));
        }

        public void AddDividendEvent(decimal dividendsAmount, decimal totalDayPrice)
        {
            _fifoQueue.AddLast((dividendsAmount, totalDayPrice, totalDayPrice/dividendsAmount));
        }

        public void AddSellEvent(decimal amount)
        {
            var soldRemaining = amount;
            while (Math.Abs(soldRemaining) >= 1e-6m)
            {
                if (!_fifoQueue.Any())
                {
                    throw new ArgumentException($"Failed to calculate: there are more sell-units than buy-units.");
                }

                var next = _fifoQueue.First;
                if (soldRemaining >= next!.Value.amount)
                {
                    // Sell more than was in single buy event
                    soldRemaining -= next.Value.amount;
                    _fifoQueue.RemoveFirst();
                }
                else
                {
                    // Sell less than was in single buy event. Reflect change to price also
                    next.ValueRef.amount -= soldRemaining;
                    next.ValueRef.price -= soldRemaining * next.Value.avPrice;
                    break;
                }
            }
        }

        public decimal GetAveragePrice()
        {
            if (!_fifoQueue.Any())
            {
                return 0m;
            }

            var totalAmount = _fifoQueue.Sum(f => f.amount);
            var totalPrice = _fifoQueue.Sum(f => f.price);

            if (totalAmount == 0m)
            {
                throw new InvalidOperationException("Cannot divide with zero!");
            }

            return totalPrice / totalAmount;
        }
    }
}
