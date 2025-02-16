﻿using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Business.Validation;
using Data.Entities;
using Data.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task AddAsync(ProductModel model)
        {
            if (model.IsValidModel())
            {
                var productEntity = _mapper.Map<Product>(model);

                productEntity.Category = null;

                await _unitOfWork.ProductRepository.AddAsync(productEntity);

                await _unitOfWork.SaveAsync();
            }
        }

        public async Task AddCategoryAsync(ProductCategoryModel categoryModel)
        {
            ValidateCategoryModel(categoryModel);

            var productCategory = _mapper.Map<ProductCategory>(categoryModel);

            await _unitOfWork.ProductCategoryRepository.AddAsync(productCategory);

            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteAsync(int modelId)
        {
            if (_unitOfWork.ProductRepository.GetByIdAsync(modelId) == null)
                throw new MarketException();

            await _unitOfWork.ProductRepository.DeleteByIdAsync(modelId);

            await _unitOfWork.SaveAsync();
        }

        public async Task<IEnumerable<ProductModel>> GetAllAsync()
        {
            var products = await _unitOfWork.ProductRepository.GetAllWithDetailsAsync();
            return _mapper.Map<IEnumerable<ProductModel>>(products);
        }

        public async Task<IEnumerable<ProductCategoryModel>> GetAllProductCategoriesAsync()
        {
            var products = await _unitOfWork.ProductCategoryRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductCategoryModel>>(products);
        }

        public async Task<IEnumerable<ProductModel>> GetByFilterAsync(FilterSearchModel filterSearch)
        {
            if (filterSearch == null)
                throw new ArgumentNullException(nameof(filterSearch));

            var query = await _unitOfWork.ProductRepository.GetAllWithDetailsAsync();

            if (filterSearch.MaxPrice != null)
                query = query.Where(p => p.Price <= filterSearch.MaxPrice);

            if (filterSearch.MinPrice != null)
                query = query.Where(p => p.Price >= filterSearch.MinPrice);

            if (filterSearch.CategoryId != null)
                query = query.Where(p => p.ProductCategoryId == filterSearch.CategoryId);

            return _mapper.Map<IEnumerable<ProductModel>>(query);
        }

        public async Task<ProductModel> GetByIdAsync(int id)
        {
            return _mapper.Map<ProductModel>(await _unitOfWork.ProductRepository.GetByIdWithDetailsAsync(id));
        }

        public async Task RemoveCategoryAsync(int categoryId)
        {
            if (_unitOfWork.ProductCategoryRepository.GetByIdAsync(categoryId) == null)
                throw new MarketException();

            await _unitOfWork.ProductCategoryRepository.DeleteByIdAsync(categoryId);

            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(ProductModel model)
        {
            if (!ModelsValidator.IsValidModel(model))
            {
                throw new ValidationException("Model is invalid");
            }

            var productEntity = await _unitOfWork.ProductRepository.GetByIdAsync(model.Id);
            if (productEntity == null)
            {
                throw new KeyNotFoundException($"Product with ID {model.Id} not found.");
            }

            _mapper.Map(model, productEntity);
            _unitOfWork.ProductRepository.Update(productEntity);
            await _unitOfWork.SaveAsync();
        }


        public async Task UpdateCategoryAsync(ProductCategoryModel categoryModel)
        {
            ValidateCategoryModel(categoryModel);

            var productCategory = _mapper.Map<ProductCategory>(categoryModel);

            _unitOfWork.ProductCategoryRepository.Update(productCategory);

            await _unitOfWork.SaveAsync();
        }

        private static void ValidateCategoryModel(ProductCategoryModel categoryModel)
        {
            if (categoryModel == null)
                throw new MarketException();

            if (string.IsNullOrEmpty(categoryModel.CategoryName))
                throw new MarketException();
        }
    }
}
