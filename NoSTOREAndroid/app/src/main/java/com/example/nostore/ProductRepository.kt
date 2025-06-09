package com.example.nostore

import ProductDto
import com.example.nostore.NetworkClient.authApi
import com.google.android.gms.analytics.ecommerce.Product
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

class ProductRepository {
    suspend fun getProductById(productId: String): Result<ProductDto> = withContext(Dispatchers.IO) {
        try {
            val response = authApi.getProductById(productId)
            if (response.isSuccessful && response.body() != null) {
                Result.success(response.body()!!)
            } else {
                Result.failure(Exception("Ошибка загрузки продукта"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun getProductStatus(productId: String): Result<ProductStatus> =
        withContext(Dispatchers.IO) {
            try {
                val response = authApi.getProductStatus(productId)
                if (response.isSuccessful && response.body() != null) {
                    Result.success(response.body()!!)
                } else {
                    Result.failure(Exception("Ошибка загрузки статуса продукта"))
                }
            } catch (e: Exception) {
                Result.failure(e)
            }
        }

    suspend fun getFilteredProducts(request: FilterRequest): Result<List<ProductDto>> =
        withContext(Dispatchers.IO) {
            try {
                val response = authApi.getFilteredProducts(request)
                if (response.isSuccessful && response.body() != null) {
                    Result.success(response.body()!!)
                } else {
                    Result.failure(Exception("Ошибка фильтрации"))
                }
            } catch (e: Exception) {
                Result.failure(e)
            }
        }

    suspend fun getFullProductById(productId: String): Result<ProductDto> = withContext(Dispatchers.IO) {
        try {
            val productResult = authApi.getProductById(productId)
            val statusResult = authApi.getProductStatus(productId)

            if (productResult.isSuccessful && productResult.body() != null &&
                statusResult.isSuccessful && statusResult.body() != null
            ) {
                val product = productResult.body()
                val status = statusResult.body()

                val updatedProduct = product!!.copy(
                    isFavorite = status?.inFavorite ?: false,
                    inCart = status?.inCart ?: false,
                    inCompare = status?.inCompare ?: false
                )

                Result.success(updatedProduct)
            } else {
                Result.failure(Exception("Ошибка загрузки данных"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}