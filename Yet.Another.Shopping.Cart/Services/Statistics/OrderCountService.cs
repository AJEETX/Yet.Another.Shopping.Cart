using Yet.Another.Shopping.Cart.Core.Domain.Statistics;
using Yet.Another.Shopping.Cart.Infrastructure.EFRepository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Yet.Another.Shopping.Cart.Infrastructure.Services.Statistics
{
    public interface IOrderCountService
    {
        /// <summary>
        /// Get all OrderCount
        /// </summary>
        /// <returns>OrderCount entity</returns>
        IList<OrderCount> GetAllOrderCount();

        /// <summary>
        /// Get all OrderCount
        /// </summary>
        /// <param name="take">Number of date to return</param>
        /// <returns>OrderCount entities</returns>
        IList<OrderCount> GetAllOrderCount(int take);

        /// <summary>
        /// Get OrderCount by date
        /// </summary>
        /// <param name="date">Date</param>
        /// <returns>OrderCount entity</returns>
        OrderCount GetOrderCountByDate(DateTime date);

        /// <summary>
        /// Insert OrderCount
        /// </summary>
        /// <param name="orderCount">OrderCount entity</param>
        void InsertOrderCount(OrderCount orderCount);

        /// <summary>
        /// Update OrderCount
        /// </summary>
        /// <param name="orderCount">OrderCount entity</param>
        void UpdateOrderCount(OrderCount orderCount);
    }
    public class OrderCountService : IOrderCountService
    {
        #region Fields

        private readonly IRepository<OrderCount> _orderCountRepository;

        #endregion

        #region Constructor

        public OrderCountService(IRepository<OrderCount> orderCountRepository)
        {
            _orderCountRepository = orderCountRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get all OrderCount
        /// </summary>
        /// <returns>OrderCount entity</returns>
        public IList<OrderCount> GetAllOrderCount()
        {
            return _orderCountRepository.GetAll().ToList();
        }

        /// <summary>
        /// Get all OrderCount
        /// </summary>
        /// <param name="take">Number of date to return</param>
        /// <returns>OrderCount entities</returns>
        public IList<OrderCount> GetAllOrderCount(int take)
        {
            return _orderCountRepository.GetAll().Take(take).ToList();
        }

        /// <summary>
        /// Get OrderCount by date
        /// </summary>
        /// <param name="date">Date</param>
        /// <returns>OrderCount entity</returns>
        public OrderCount GetOrderCountByDate(DateTime date)
        {
            return _orderCountRepository.FindByExpression(x => x.Date == date.Date);
        }

        /// <summary>
        /// Insert OrderCount
        /// </summary>
        /// <param name="orderCount">OrderCount entity</param>
        public void InsertOrderCount(OrderCount orderCount)
        {
            if (orderCount == null)
                throw new ArgumentNullException("orderCount");

            orderCount.Date = orderCount.Date.Date;

            _orderCountRepository.Insert(orderCount);
            _orderCountRepository.SaveChanges();
        }

        /// <summary>
        /// Update OrderCount
        /// </summary>
        /// <param name="orderCount">OrderCount entity</param>
        public void UpdateOrderCount(OrderCount orderCount)
        {
            if (orderCount == null)
                throw new ArgumentNullException("orderCount");

            orderCount.Date = orderCount.Date.Date;
            orderCount.Count++;

            _orderCountRepository.Update(orderCount);
            _orderCountRepository.SaveChanges();
        }

        #endregion
    }
}
