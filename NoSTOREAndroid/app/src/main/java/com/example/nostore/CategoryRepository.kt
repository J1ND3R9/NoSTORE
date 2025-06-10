package com.example.nostore

import ProductDto
import com.example.nostore.NetworkClient.authApi
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

class CategoryRepository {

    private val api = NetworkClient.authApi

    suspend fun getRootCategories(): Result<List<CategoryDto>> = withContext(Dispatchers.IO) {
        try {
            val response = api.getCategories()
            if (response.isSuccessful && response.body() != null) {
                Result.success(response.body()!!)
            } else {
                Result.failure(Exception("Ошибка загрузки категорий"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun getCategory(slug: String): Result<CategoryResponse> = withContext(Dispatchers.IO) {
        try {
            val response = api.getCategory(slug)
            if (response.isSuccessful && response.body() != null) {
                Result.success(response.body()!!)
            } else {
                Result.failure(Exception("Ошибка загрузки категории"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}