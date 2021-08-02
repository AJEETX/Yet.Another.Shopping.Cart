using Yet.Another.Shopping.Cart.Core.Domain.Messages;
using Yet.Another.Shopping.Cart.Infrastructure.EFRepository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Yet.Another.Shopping.Cart.Infrastructure.Services.Messages
{
    public interface IContactUsService
    {
        /// <summary>
        /// Get all ContactUsMessage
        /// </summary>
        /// <returns>List of ContactUsMessage entities</returns>
        IList<ContactUsMessage> GetAllMessages();
        IList<ContactUsMessage> GetAllMessages(string email);

        /// <summary>
        /// Get ContactUsMessage using id
        /// </summary>
        /// <param name="id">ContactUsMessage id</param>
        /// <returns>ContactUsMessage entity</returns>
        ContactUsMessage GetMessageById(Guid id);

        /// <summary>
        /// Insert ContactUsMessage
        /// </summary>
        /// <param name="message">ContactUsMessage entity</param>
        void InsertMessage(ContactUsMessage message);

        /// <summary>
        /// Update ContactUsMessage
        /// </summary>
        /// <param name="message">ContactUsMessage entity</param>
        void UpdateMessage(ContactUsMessage message);

        /// <summary>
        /// Delete ContactUsMessage
        /// </summary>
        /// <param name="ids">List of ContactUsMessage ids</param>
        void DeleteMessages(IList<Guid> ids);

        /// <summary>
        /// Mark the ContactUsMessage as read
        /// </summary>
        /// <param name="id">ContactUsMessage id</param>
        void MarkAsRead(Guid id);
    }
    public class ContactUsService : IContactUsService
    {
        #region Fields

        private readonly IRepository<ContactUsMessage> _contactUsRepository;

        #endregion

        #region Constructor

        public ContactUsService(IRepository<ContactUsMessage> contactUsRepository)
        {
            _contactUsRepository = contactUsRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get all ContactUsMessage
        /// </summary>
        /// <returns>List of ContactUsMessage entities</returns>
        public IList<ContactUsMessage> GetAllMessages()
        {
            var entities = _contactUsRepository.GetAll()
                .OrderByDescending(x => x.SendDate)
                .ToList();

            return entities;
        }

        /// <summary>
        /// Get ContactUsMessage using id
        /// </summary>
        /// <param name="id">ContactUsMessage id</param>
        /// <returns>ContactUsMessage entity</returns>
        public ContactUsMessage GetMessageById(Guid id)
        {
            return _contactUsRepository.FindByExpression(x => x.Id == id);
        }

        /// <summary>
        /// Insert ContactUsMessage
        /// </summary>
        /// <param name="message">ContactUsMessage entity</param>
        public void InsertMessage(ContactUsMessage message)
        {
            if (message == null)
                throw new ArgumentException(nameof(message));

            _contactUsRepository.Insert(message);
            _contactUsRepository.SaveChanges();
        }

        /// <summary>
        /// Update ContactUsMessage
        /// </summary>
        /// <param name="message">ContactUsMessage entity</param>
        public void UpdateMessage(ContactUsMessage message)
        {
            if (message == null)
                throw new ArgumentException(nameof(message));

            _contactUsRepository.Update(message);
            _contactUsRepository.SaveChanges();
        }

        /// <summary>
        /// Delete ContactUsMessage
        /// </summary>
        /// <param name="ids">List of ContactUsMessage ids</param>
        public void DeleteMessages(IList<Guid> ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            foreach (var id in ids)
                _contactUsRepository.Delete(GetMessageById(id));

            _contactUsRepository.SaveChanges();
        }

        /// <summary>
        /// Mark the ContactUsMessage as read
        /// </summary>
        /// <param name="id">ContactUsMessage id</param>
        public void MarkAsRead(Guid id)
        {
            if (id == default || id == Guid.Empty)
                throw new ArgumentNullException(nameof(id));

            var message = GetMessageById(id);
            message.Read = true;

            _contactUsRepository.Update(message);
            _contactUsRepository.SaveChanges();
        }

        public IList<ContactUsMessage> GetAllMessages(string email)
        {
            return _contactUsRepository.FindManyByExpression(users => users.Email == email).OrderByDescending(x => x.SendDate).ToList();
        }

        #endregion
    }
}
