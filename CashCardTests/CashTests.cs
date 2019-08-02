using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace CashCardTests
{
    [TestFixture]
    public class CashTests
    {
        [Test]
        public void Should_return_cash_card_Object()
        {
            var result = new CashCard();
            result.ShouldNotBeNull();
        }

        [Test]
        [TestCase(1234, 20)]
        public void Should_withdrawal_with_correct_pin(int pin, double amount)
        {
            var result = new CashCard();
            result.Deposit(pin, amount);
            result.Withdrawal(pin, amount);

            result.Balance.ShouldBe(0);
        }

        [Test]
        [TestCase(1231, 20)]
        public void Should_throw_exception_with_wrong_pin(int pin, double amount)
        {
            Should.Throw<ArgumentException>(() =>
            {
                var result = new CashCard();
                result.Withdrawal(1235, 20);
            });
        }


        [Test]
        [TestCase(1231, 20)]
        public void Should_throw_exception_when_withdrawing_more_than_allowed(int pin, double amount)
        {
            Should.Throw<ArgumentException>(() =>
            {
                var result = new CashCard();
                result.Withdrawal(pin, 20);
            });
        }

        [Test]
        [TestCase(1234, 20)]
        public void Should_deposit_with_correct_pin(int pin, double amount)
        {
            var result = new CashCard();
            result.Deposit(pin, amount);

            result.Balance.ShouldBe(amount);
        }

        [Test]
        public void Should_deposit_concurrently()
        {
            var card = new CashCard();

            var x = Task.Run(() => Deposit(card));
            var y = Task.Run(() => Deposit(card));

            Task.WaitAll(y, x);


            card.Balance.ShouldBe(100);

        }

        [Test]
        public void Should_withdrawal_concurrently()
        {
            var card = new CashCard();

            Deposit(card);
            Deposit(card);

            var x = Task.Run(() => Withdrawal(card));
            var y = Task.Run(() => Withdrawal(card));

            Task.WaitAll(y, x);


            card.Balance.ShouldBe(0);

        }

        private static void Deposit(CashCard card)
        {
            for (var i = 1; i <= 5; i++)
            {
                card.Deposit(1234, 10);
            }
        }

        private static void Withdrawal(CashCard card)
        {
            for (var i = 1; i <= 5; i++)
            {
                card.Withdrawal(1234, 10);
            }
        }
    }

    public class CashCard
    {
        private const int Pin = 1234;
        private readonly ConcurrentQueue<double> _transactions = new ConcurrentQueue<double>();

        public double Balance { get; set; }

        private void CalculateBalance()
        {
            while (!_transactions.IsEmpty)
            {
                _transactions.TryDequeue(out var amount);
                CheckBalance(amount);
                Balance += amount;
            }
        }

        private void CheckBalance(double amount)
        {
            if (amount < 0 && Balance < Math.Abs(amount))
                throw new ArgumentException("Not enough money");
        }

        public void Deposit(int pin, double amount)
        {
            CheckPin(pin);
            _transactions.Enqueue(amount);
            CalculateBalance();
        }

        public void Withdrawal(int pin, double amount)
        {
            CheckPin(pin);
            _transactions.Enqueue(-amount);
            CalculateBalance();
        }

        private static void CheckPin(int pin)
        {
            if (pin != Pin)
                throw new ArgumentException("Incorrect Pin");
        }
    }
}