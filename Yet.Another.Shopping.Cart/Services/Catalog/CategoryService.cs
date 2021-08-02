using Yet.Another.Shopping.Cart.Core.Domain.Catalog;
using Yet.Another.Shopping.Cart.Infrastructure.EFRepository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Yet.Another.Shopping.Cart.Infrastructure.Services.Catalog
{
    public interface ICategoryService
    {
        /// <summary>
        /// Get all categories
        /// </summary>
        /// <returns>List of category entities</returns>
        IList<Category> GetAllCategories();

        /// <summary>
        /// Get all categories without parent
        /// </summary>
        /// <returns>List of category entities without parent</returns>
        IList<Category> GetAllCategoriesWithoutParent();

        /// <summary>
        /// Get category using id
        /// </summary>
        /// <param name="id">Category id</param>
        /// <returns>Category entity</returns>
        Category GetCategoryById(Guid id);

        /// <summary>
        /// Get category using seo
        /// </summary>
        /// <param name="seo">Category SEO</param>
        /// <returns>Category entity</returns>
        Category GetCategoryBySeo(string seo);

        /// <summary>
        /// Insert category
        /// </summary>
        /// <param name="category">Category entity</param>
        void InsertCategory(Category category);

        /// <summary>
        /// Update category 
        /// </summary>
        /// <param name="category">Category entity</param>
        void UpdateCategory(Category category);

        /// <summary>
        /// Delete categories
        /// </summary>
        /// <param name="ids">Ids of categories to delete</param>
        void DeleteCategories(IList<Guid> ids);

        /// <summary>
        /// Insert product category mappings
        /// </summary>
        /// <param name="productCategoryMappings">List of product category mapping</param>
        void InsertProductCategoryMappings(IList<ProductCategoryMapping> productCategoryMappings);

        /// <summary>
        /// Delete all product category mappings using product id
        /// </summary>
        /// <param name="productId">Product id</param>
        void DeleteAllProductCategoryMappingsByProductId(Guid productId);
    }
    public class CategoryService : ICategoryService
    {
        #region Fields

        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<ProductCategoryMapping> _productCategoryRepository;

        #endregion

        #region Constructor

        public CategoryService(
            IRepository<Category> categoryRepository,
            IRepository<ProductCategoryMapping> productCategoryRepository)
        {
            _categoryRepository = categoryRepository;
            _productCategoryRepository = productCategoryRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get all categories
        /// </summary>
        /// <returns>List of category entities</returns>
        public IList<Category> GetAllCategories()
        {
            var entities = _categoryRepository.GetAll()
                .OrderBy(x => x.Name)
                .ToList();

            return entities;
        }

        /// <summary>
        /// Get all categories without parent
        /// </summary>
        /// <returns>List of category entities without parent</returns>
        public IList<Category> GetAllCategoriesWithoutParent()
        {
            var entities = _categoryRepository.FindManyByExpression(x => x.ParentCategoryId == Guid.Empty)
                .OrderBy(x => x.Name)
                .ToList();

            return entities;
        }

        /// <summary>
        /// Get category using id
        /// </summary>
        /// <param name="id">Category id</param>
        /// <returns>Category entity</returns>
        public Category GetCategoryById(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return _categoryRepository.FindByExpression(x => x.Id == id);
        }

        /// <summary>
        /// Get category using seo
        /// </summary>
        /// <param name="seo">Category SEO</param>
        /// <returns>Category entity</returns>
        public Category GetCategoryBySeo(string seo)
        {
            if (seo == "")
                return null;

            return _categoryRepository.FindByExpression(x => x.SeoUrl == seo);
        }

        /// <summary>
        /// Insert category
        /// </summary>
        /// <param name="category">Category entity</param>
        public void InsertCategory(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            _categoryRepository.Insert(category);
            _categoryRepository.SaveChanges();
        }

        /// <summary>
        /// Update category 
        /// </summary>
        /// <param name="category">Category entity</param>
        public void UpdateCategory(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            _categoryRepository.Update(category);
            _categoryRepository.SaveChanges();
        }

        /// <summary>
        /// Delete categories
        /// </summary>
        /// <param name="ids">Ids of categories to delete</param>
        public void DeleteCategories(IList<Guid> ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            foreach (var id in ids)
                _categoryRepository.Delete(GetCategoryById(id));

            _categoryRepository.SaveChanges();
        }

        /// <summary>
        /// Insert product category mappings
        /// </summary>
        /// <param name="productCategoryMappings">List of product category mapping</param>
        public void InsertProductCategoryMappings(IList<ProductCategoryMapping> productCategoryMappings)
        {
            if (productCategoryMappings == null)
                throw new ArgumentNullException(nameof(productCategoryMappings));

            foreach (var mapping in productCategoryMappings)
                _productCategoryRepository.Insert(mapping);

            _productCategoryRepository.SaveChanges();
        }

        /// <summary>
        /// Delete all product category mappings using product id
        /// </summary>
        /// <param name="productId">Product id</param>
        public void DeleteAllProductCategoryMappingsByProductId(Guid productId)
        {
            if (productId == Guid.Empty)
                throw new ArgumentNullException(nameof(productId));

            var mappings = _productCategoryRepository.FindManyByExpression(x => x.ProductId == productId);

            foreach (var mapping in mappings)
                _productCategoryRepository.Delete(mapping);

            _productCategoryRepository.SaveChanges();
        }

        #endregion
    }
}
